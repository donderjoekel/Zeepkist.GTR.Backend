using System.Threading.Channels;
using TNRD.Zeepkist.GTR.Backend.Records.Requests;
using TNRD.Zeepkist.GTR.Backend.WorldRecords;

namespace TNRD.Zeepkist.GTR.Backend.Records.Processors;

public class ProcessWorldRecordHostedService : BackgroundService
{
    private readonly ILogger<ProcessWorldRecordHostedService> _logger;
    private readonly IServiceProvider _provider;
    private readonly Channel<ProcessWorldRecordRequest> _channel;

    public ProcessWorldRecordHostedService(ILogger<ProcessWorldRecordHostedService> logger, IServiceProvider provider,
        Channel<ProcessWorldRecordRequest> channel)
    {
        _logger = logger;
        _provider = provider;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            ProcessWorldRecordRequest request = await _channel.Reader.ReadAsync(stoppingToken);
            using IServiceScope scope = _provider.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;
            IWorldRecordsService worldRecordsService = provider.GetRequiredService<IWorldRecordsService>();
            worldRecordsService.UpdateWorldRecord(request.Record);
        }
    }
}
