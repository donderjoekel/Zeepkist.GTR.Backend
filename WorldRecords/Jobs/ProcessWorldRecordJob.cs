using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;

public class ProcessWorldRecordJob
{
    private readonly IWorldRecordsService _worldRecordsService;
    private readonly IRecordsService _recordsService;

    public ProcessWorldRecordJob(IWorldRecordsService worldRecordsService, IRecordsService recordsService)
    {
        _worldRecordsService = worldRecordsService;
        _recordsService = recordsService;
    }

    [UsedImplicitly]
    public Task ExecuteAsync(int recordId, int levelId)
    {
        Record? record = _recordsService.GetById(recordId);
        if (record == null)
        {
            throw new InvalidOperationException($"Record with ID '{recordId}' not found");
        }

        _worldRecordsService.UpdateDailyWorldRecord(record, levelId);
        _worldRecordsService.UpdateWeeklyWorldRecord(record, levelId);
        _worldRecordsService.UpdateMonthlyWorldRecord(record, levelId);
        _worldRecordsService.UpdateQuarterlyWorldRecord(record, levelId);
        _worldRecordsService.UpdateYearlyWorldRecord(record, levelId);
        _worldRecordsService.UpdateGlobalWorldRecord(record, levelId);
        return Task.CompletedTask;
    }

    public static void Schedule(IJobScheduler jobScheduler, int recordId, int levelId)
    {
        jobScheduler.Enqueue<ProcessWorldRecordJob>(recordId, levelId);
    }
}
