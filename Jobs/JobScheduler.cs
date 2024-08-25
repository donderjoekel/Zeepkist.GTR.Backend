using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using TNRD.Zeepkist.GTR.Backend.Levels.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels.Points.Jobs;
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

    public JobScheduler(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
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
        ScheduleRecurringJob<FixWorldRecordsJob>(Cron.Never());
        ScheduleRecurringJob<FixPersonalBestsJob>(Cron.Never());

        ScheduleRecurringJob<FullWorkshopScanJob>(Cron.Never());
        ScheduleRecurringJob<PartialWorkshopScanJob>(Cron.Never());
        ScheduleRecurringJob<CalculateLevelPointsJob>(Cron.Never());
        ScheduleRecurringJob<CalculateUserPointsJob>(Cron.Never());
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
