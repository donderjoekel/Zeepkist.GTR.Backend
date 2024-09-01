using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests.Jobs;

public class FixPersonalBestsJob
{
    private readonly IRecordsService _recordsService;
    private readonly IServiceProvider _provider;
    private readonly ILogger<FixWorldRecordsJob> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(50, 50);
    private int counter;
    private int total;

    public FixPersonalBestsJob(
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
            .GroupBy(x => x.IdUser)
            .OrderBy(x => x.Key)
            .ToList();

        _logger.LogInformation("Fixing personal bests ({Count})", groupedRecords.Count);
        List<Task> tasks = new();

        total = groupedRecords.Count;
        foreach (IGrouping<int, Record> group in groupedRecords)
        {
            tasks.Add(Task.Run(() => FixPersonalBests(group)));
        }

        await Task.WhenAll(tasks);
        _logger.LogInformation("Finished fixing personal bests");
    }

    private async Task FixPersonalBests(IGrouping<int, Record> group)
    {
        await _semaphore.WaitAsync();
        IServiceScope scope = _provider.CreateScope();

        try
        {
            IPersonalBestsService personalBestsService
                = scope.ServiceProvider.GetRequiredService<IPersonalBestsService>();

            int userId = group.Key;
            _logger.LogInformation("Fixing personal bests for user {User}", userId);

            List<IGrouping<int, Record>> levelGroups = group
                .GroupBy(x => x.IdLevel)
                .ToList();

            for (int i = 0; i < levelGroups.Count; i++)
            {
                IGrouping<int, Record> levelGroup = levelGroups[i];
                int levelId = levelGroup.Key;
                List<Record> records = levelGroup.OrderBy(x => x.DateCreated).ToList();

                foreach (Record record in records)
                {
                    personalBestsService.UpdateDaily(record, userId, levelId);
                    personalBestsService.UpdateWeekly(record, userId, levelId);
                    personalBestsService.UpdateMonthly(record, userId, levelId);
                    personalBestsService.UpdateQuarterly(record, userId, levelId);
                    personalBestsService.UpdateYearly(record, userId, levelId);
                    personalBestsService.UpdateGlobal(record, userId, levelId);
                }
            }

            Interlocked.Increment(ref counter);
            _logger.LogInformation(
                "Finished fixing personal bests for user {User} ({Counter}/{Total})",
                userId,
                counter,
                total);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fixing personal bests for user {User}", group.Key);
        }
        finally
        {
            scope.Dispose();
            _semaphore.Release();
        }
    }

    public static void Schedule(IJobScheduler jobScheduler)
    {
        jobScheduler.Enqueue<FixPersonalBestsJob>();
    }
}
