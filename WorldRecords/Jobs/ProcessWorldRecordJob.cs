using System.Collections.Concurrent;
using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;

public class ProcessWorldRecordJob
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> Semaphores = new();
    private readonly IWorldRecordsService _worldRecordsService;
    private readonly IRecordsService _recordsService;

    public ProcessWorldRecordJob(IWorldRecordsService worldRecordsService, IRecordsService recordsService)
    {
        _worldRecordsService = worldRecordsService;
        _recordsService = recordsService;
    }

    [UsedImplicitly]
    public async Task ExecuteAsync(int levelId)
    {
        Record? record = _recordsService.GetBestValid(levelId);
        if (record == null)
        {
            throw new InvalidOperationException($"No valid record found for level '{levelId}'");
        }

        SemaphoreSlim semaphore = Semaphores.GetOrAdd(levelId, x => new SemaphoreSlim(1, 1));

        try
        {
            await semaphore.WaitAsync();
            // _worldRecordsService.UpdateDailyWorldRecord(record, levelId);
            // _worldRecordsService.UpdateWeeklyWorldRecord(record, levelId);
            // _worldRecordsService.UpdateMonthlyWorldRecord(record, levelId);
            // _worldRecordsService.UpdateQuarterlyWorldRecord(record, levelId);
            // _worldRecordsService.UpdateYearlyWorldRecord(record, levelId);
            _worldRecordsService.UpdateGlobalWorldRecord(record, levelId);
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1)
            {
                Semaphores.TryRemove(levelId, out _);
            }
        }
    }

    public static void Schedule(IJobScheduler jobScheduler, int levelId)
    {
        jobScheduler.Enqueue<ProcessWorldRecordJob>(levelId);
    }
}
