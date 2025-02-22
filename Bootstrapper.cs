using Microsoft.Extensions.Options;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Backend.Jobs;

namespace TNRD.Zeepkist.GTR.Backend;

public class Bootstrapper : BackgroundService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly JobOptions _jobOptions;
    private readonly IServiceProvider _provider;
    private readonly ILogger<Bootstrapper> _logger;

    public Bootstrapper(IJobScheduler jobScheduler, IOptions<JobOptions> jobOptions, IServiceProvider provider,
        ILogger<Bootstrapper> logger)
    {
        _jobScheduler = jobScheduler;
        _jobOptions = jobOptions.Value;
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_jobOptions.EnableWorkshop)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        else
        {
            _logger.LogInformation("Ensuring database created");
            using IServiceScope scope = _provider.CreateScope();
            IDatabase database = scope.ServiceProvider.GetRequiredService<IDatabase>();
            await database.EnsureCreated();
        }

        _logger.LogInformation("Scheduling recurring jobs");
        _jobScheduler.ScheduleRecurringJobs();
    }
}
