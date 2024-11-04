using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Options;
using TNRD.Zeepkist.GTR.Backend.Levels.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels.Points.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels.Requests.Jobs;
using TNRD.Zeepkist.GTR.Backend.PersonalBests.Jobs;
using TNRD.Zeepkist.GTR.Backend.Users.Points.Jobs;
using TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

public interface IJobScheduler
{
    void Enqueue<TJob>(params object[] args);
    void ScheduleRecurringJobs();
}

public class JobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly JobOptions _options;

    public JobScheduler(IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IOptions<JobOptions> options)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _options = options.Value;
    }

    public void Enqueue<TJob>(params object[] args)
    {
        Job job = new Job(
            typeof(TJob),
            typeof(TJob).GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance),
            args);
        _backgroundJobClient.Create(job, new EnqueuedState());
    }

    public void ScheduleRecurringJobs()
    {
#if DEBUG
        ScheduleRecurringJob<DownloadAllLevelsJob>(Cron.Never());
        ScheduleRecurringJob<FixWorldRecordsJob>(Cron.Never());
        ScheduleRecurringJob<FixPersonalBestsJob>(Cron.Never());
        ScheduleRecurringJob<ProcessLevelRequestsJob>(Cron.Never());
        ScheduleRecurringJob<FullWorkshopScanJob>(Cron.Never());
        ScheduleRecurringJob<PartialWorkshopScanJob>(Cron.Never());
        ScheduleRecurringJob<CalculateLevelPointsJob>(Cron.Never());
        ScheduleRecurringJob<CalculateUserPointsJob>(Cron.Never());
#else
        if (_options.EnableWorkshop)
        {
            ScheduleRecurringJob<ProcessLevelRequestsJob>(Cron.MinuteInterval(5));
            ScheduleRecurringJob<FullWorkshopScanJob>(Cron.Monthly());
            ScheduleRecurringJob<PartialWorkshopScanJob>(Cron.Hourly());
        }
        else
        {
            ScheduleRecurringJob<CalculateLevelPointsJob>(Cron.Daily());
            ScheduleRecurringJob<CalculateUserPointsJob>(Cron.Hourly());
        }
#endif
    }

    private void ScheduleRecurringJob<TJob>(string cronExpression)
    {
        Job job = new Job(
            typeof(TJob),
            typeof(TJob).GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance));

        _recurringJobManager.AddOrUpdate(
            typeof(TJob).Name,
            job,
            cronExpression);
    }
}
