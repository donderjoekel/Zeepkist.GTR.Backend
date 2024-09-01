using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests.Jobs;

public class ProcessPersonalBestJob
{
    private readonly IPersonalBestsService _personalBestsService;
    private readonly IRecordsService _recordsService;

    public ProcessPersonalBestJob(IPersonalBestsService personalBestsService, IRecordsService recordsService)
    {
        _personalBestsService = personalBestsService;
        _recordsService = recordsService;
    }

    [UsedImplicitly]
    public Task ExecuteAsync(int recordId, int userId, int levelId)
    {
        Record? record = _recordsService.GetById(recordId);
        if (record == null)
        {
            throw new InvalidOperationException($"Record with ID '{recordId}' not found");
        }

        _personalBestsService.UpdateDaily(record, userId, levelId);
        _personalBestsService.UpdateWeekly(record, userId, levelId);
        _personalBestsService.UpdateMonthly(record, userId, levelId);
        _personalBestsService.UpdateQuarterly(record, userId, levelId);
        _personalBestsService.UpdateYearly(record, userId, levelId);
        _personalBestsService.UpdateGlobal(record, userId, levelId);
        return Task.CompletedTask;
    }

    public static void Schedule(IJobScheduler jobScheduler, int recordId, int userId, int levelId)
    {
        jobScheduler.Enqueue<ProcessPersonalBestJob>(recordId, userId, levelId);
    }
}
