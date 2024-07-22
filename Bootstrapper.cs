using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;

namespace TNRD.Zeepkist.GTR.Backend;

public class Bootstrapper : BackgroundService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly IZeeplevelService _zeeplevelService;

    public Bootstrapper(IJobScheduler jobScheduler, IZeeplevelService zeeplevelService)
    {
        _jobScheduler = jobScheduler;
        _zeeplevelService = zeeplevelService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _jobScheduler.ScheduleRecurringJobs();
        return Task.CompletedTask;
    }
}
