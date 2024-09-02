using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Workshop;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class FullWorkshopScanJob : WorkshopScanJob
{
    public static bool IsRunning { get; private set; }

    public FullWorkshopScanJob(
        ILogger<FullWorkshopScanJob> logger,
        WorkshopLister workshopLister,
        WorkshopDownloader workshopDownloader,
        WorkshopProcessor workshopProcessor,
        IWorkshopService workshopService)
        : base(logger, workshopLister, workshopDownloader, workshopProcessor, workshopService)
    {
    }

    [UsedImplicitly]
    public async Task ExecuteAsync()
    {
        if (IsRunning)
        {
            Logger.LogInformation("Full workshop scan already running");
            return;
        }

        IsRunning = true;
        Logger.LogInformation("Starting full workshop scan");
        await Run(WorkshopLister.QueryType.Normal);
        Logger.LogInformation("Finished full workshop scan");
        IsRunning = false;
    }
}
