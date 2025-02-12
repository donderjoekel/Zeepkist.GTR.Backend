using System.Threading.Channels;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Media;
using TNRD.Zeepkist.GTR.Backend.Records.Requests;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records.Processors;

public class ProcessRecordMediaHostedService : BackgroundService
{
    private readonly ILogger<ProcessRecordMediaHostedService> _logger;
    private readonly IServiceProvider _provider;
    private readonly Channel<ProcessRecordMediaRequest> _channel;

    public ProcessRecordMediaHostedService(ILogger<ProcessRecordMediaHostedService> logger,
        IServiceProvider provider,
        Channel<ProcessRecordMediaRequest> channel)
    {
        _logger = logger;
        _provider = provider;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            (Record? record, string? ghostData, int retries) = await _channel.Reader.ReadAsync(stoppingToken);
            if (retries == 3)
            {
                // TODO: Log
                continue;
            }

            using IServiceScope scope = _provider.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;
            IRecordsService recordsService = provider.GetRequiredService<IRecordsService>();
            IMediaService mediaService = provider.GetRequiredService<IMediaService>();

            List<Record> topTen = recordsService.GetTop(6, record.IdUser, record.IdLevel).ToList();

            int index = topTen.FindIndex(x => x.Id == record.Id);
            if (index is -1 or >= 5)
                continue; // We only care if a run is in the top 5 per player per level

            Result<string> result = await mediaService.UploadGhost(record.Id, ghostData,
                $"{record.IdUser}-{record.IdUser}-{Guid.NewGuid()}");

            if (result.IsFailed)
            {
                await _channel.Writer.WriteAsync(new ProcessRecordMediaRequest(record, ghostData, retries + 1),
                    stoppingToken);
                continue;
            }

            if (topTen.Count <= 5)
                continue;

            Record sixthRecord = topTen[5];
            Result deleteResult = await mediaService.DeleteGhost(sixthRecord.Id);
            if (deleteResult.IsFailed)
            {
                _logger.LogError("Failed to delete ghost for record id {Id}; Result: {Result}",
                    sixthRecord.Id,
                    deleteResult.ToString());
            }
        }
    }
}
