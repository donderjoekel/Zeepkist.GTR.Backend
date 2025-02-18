using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Backend.Jobs;

namespace TNRD.Zeepkist.GTR.Backend;

public class Bootstrapper : BackgroundService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly IServiceProvider _provider;

    public Bootstrapper(IJobScheduler jobScheduler, IServiceProvider provider)
    {
        _jobScheduler = jobScheduler;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _provider.CreateScope();
        IDatabase database = scope.ServiceProvider.GetRequiredService<IDatabase>();
        await database.EnsureCreated();

        _jobScheduler.ScheduleRecurringJobs();
    }
}
