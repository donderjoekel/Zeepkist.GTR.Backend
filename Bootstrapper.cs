using TNRD.Zeepkist.GTR.Backend.Jobs;

namespace TNRD.Zeepkist.GTR.Backend;

public class Bootstrapper : BackgroundService
{
    private readonly IJobScheduler _jobScheduler;

    public Bootstrapper(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _jobScheduler.ScheduleRecurringJobs();
        return Task.CompletedTask;
    }
}
