using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;

public class FixWorldRecordsJob
{
    private readonly IRecordsService _recordsService;
    private readonly IServiceProvider _provider;
    private readonly ILogger<FixWorldRecordsJob> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(50, 50);
    private int counter;
    private int total;

    public FixWorldRecordsJob(
        IRecordsService recordsService,
        IServiceProvider provider,
        ILogger<FixWorldRecordsJob> logger)
    {
        _recordsService = recordsService;
        _provider = provider;
        _logger = logger;
    }

    [UsedImplicitly]
    public async Task ExecuteAsync()
    {
        List<IGrouping<int, Record>> groupedRecords = _recordsService.GetAll()
            .ToList()
            .GroupBy(x => x.IdLevel)
            .OrderBy(x => x.Count())
            .ToList();

        _logger.LogInformation("Fixing world records ({Count})", groupedRecords.Count);
        List<Task> tasks = new();

        counter = 0;
        total = groupedRecords.Count;
        foreach (IGrouping<int, Record> group in groupedRecords)
        {
            tasks.Add(Task.Run(() => FixWorldRecords(group)));
        }

        await Task.WhenAll(tasks);
        _logger.LogInformation("Finished fixing world records");
    }

    private async Task FixWorldRecords(IGrouping<int, Record> group)
    {
        await _semaphore.WaitAsync();
        IServiceScope scope = _provider.CreateScope();

        try
        {
            IWorldRecordsService worldRecordsService = scope.ServiceProvider.GetRequiredService<IWorldRecordsService>();
            int levelId = group.Key;
            List<Record> records = group.OrderBy(x => x.DateCreated).ToList();

            _logger.LogInformation("Fixing world records for level {LevelId}", levelId);
            foreach (Record record in records)
            {
                worldRecordsService.UpdateDailyWorldRecord(record, levelId);
                worldRecordsService.UpdateWeeklyWorldRecord(record, levelId);
                worldRecordsService.UpdateMonthlyWorldRecord(record, levelId);
                worldRecordsService.UpdateQuarterlyWorldRecord(record, levelId);
                worldRecordsService.UpdateYearlyWorldRecord(record, levelId);
                worldRecordsService.UpdateGlobalWorldRecord(record, levelId);
            }

            Interlocked.Increment(ref counter);
            _logger.LogInformation(
                "Finished fixing world records for level {LevelId} ({Counter}/{Total})",
                levelId,
                counter,
                total);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fixing world records for level {LevelId}", group.Key);
        }
        finally
        {
            scope.Dispose();
            _semaphore.Release();
        }
    }

    public static void Schedule(IJobScheduler jobScheduler)
    {
        jobScheduler.Enqueue<FixWorldRecordsJob>();
    }
}
