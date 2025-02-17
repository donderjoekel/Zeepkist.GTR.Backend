using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<int, ConcurrentQueue<ProcessRecordMediaRequest>> _queues;
    private readonly ConcurrentDictionary<int, Task> _tasks;

    public ProcessRecordMediaHostedService(ILogger<ProcessRecordMediaHostedService> logger,
        IServiceProvider provider,
        Channel<ProcessRecordMediaRequest> channel)
    {
        _logger = logger;
        _provider = provider;
        _channel = channel;

        _queues = new ConcurrentDictionary<int, ConcurrentQueue<ProcessRecordMediaRequest>>();
        _tasks = new ConcurrentDictionary<int, Task>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                _logger.LogInformation("Getting item with {Amount} left in channel", _channel.Reader.Count);
                ProcessRecordMediaRequest request = await _channel.Reader.ReadAsync(stoppingToken);
                if (request.Retries == 3)
                {
                    _logger.LogInformation("Failed to process record ({RecordId}) for User {UserId}", request.Record.Id,
                        request.Record.IdUser);
                    continue;
                }

                if (!_queues.ContainsKey(request.Record.IdUser))
                {
                    while (!_queues.TryAdd(request.Record.IdUser, new ConcurrentQueue<ProcessRecordMediaRequest>()))
                    {
                        await Task.Delay(10, stoppingToken);
                    }
                }

                ConcurrentQueue<ProcessRecordMediaRequest>? queue = null;

                while (!_queues.TryGetValue(request.Record.IdUser, out queue))
                {
                    await Task.Delay(10, stoppingToken);
                }

                queue.Enqueue(request);

                if (!_tasks.ContainsKey(request.Record.IdUser))
                {
                    Task task = ProcessQueue(request.Record.IdUser, CancellationToken.None);
                    while (!_tasks.TryAdd(request.Record.IdUser, task))
                    {
                        await Task.Delay(10, stoppingToken);
                    }
                }
                else
                {
                    Task? existingTask;
                    while (!_tasks.TryGetValue(request.Record.IdUser, out existingTask))
                    {
                        await Task.Delay(10, stoppingToken);
                    }

                    if (existingTask.Status == TaskStatus.Running)
                        continue;

                    Task newTask = ProcessQueue(request.Record.IdUser, CancellationToken.None);
                    while (!_tasks.TryUpdate(request.Record.IdUser, newTask, existingTask))
                    {
                        await Task.Delay(10, stoppingToken);
                    }
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Main task failed");
        }

        await Task.WhenAll(_tasks.Values);
    }

    private async Task ProcessQueue(int userId, CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = _provider.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;

            IRecordsService recordsService = provider.GetRequiredService<IRecordsService>();
            IMediaService mediaService = provider.GetRequiredService<IMediaService>();

            ConcurrentQueue<ProcessRecordMediaRequest>? queue;
            while (!_queues.TryGetValue(userId, out queue))
            {
                await Task.Delay(10, stoppingToken);
            }

            while (!queue.IsEmpty)
            {
                ProcessRecordMediaRequest? request = null;
                while (!queue.TryDequeue(out request))
                {
                    await Task.Delay(10, stoppingToken);
                }

                (Record? record, string? ghostData, int retries) = request;
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
        catch (Exception e)
        {
            _logger.LogError(e, "Processing Record Media Queue for User {UserId} Failed", userId);
        }
    }
}
