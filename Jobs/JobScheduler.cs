using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

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
        // TODO: Uncomment when ready
        // ScheduleRecurringJob<FullWorkshopScanJob>("0 0 1 * *");
        // ScheduleRecurringJob<PartialWorkshopScanJob>("*/15 * * * *");
    }

    private void ScheduleRecurringJob<TJob>(string cronExpression)
    {
        Job job = new Job(
            typeof(TJob),
            typeof(TJob).GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance));

        _recurringJobManager.AddOrUpdate(
            Guid.NewGuid().ToString(),
            job,
            cronExpression);
    }
}
