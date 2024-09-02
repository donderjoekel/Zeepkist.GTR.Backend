using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Workshop;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class PartialWorkshopScanJob : WorkshopScanJob
{
    public PartialWorkshopScanJob(
        ILogger<PartialWorkshopScanJob> logger,
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
        if (FullWorkshopScanJob.IsRunning)
        {
            Logger.LogInformation("Skipping partial workshop scan because full workshop scan is running");
            return;
        }

        await ScanByPublicationDate();
        await ScanByUpdatedDate();
    }

    private async Task ScanByPublicationDate()
    {
        Logger.LogInformation("Starting partial workshop scan by publication date");
        await Run(WorkshopLister.QueryType.ByPublicationDate, 5);
        Logger.LogInformation("Finished partial workshop scan by publication date");
    }
    
    private async Task ScanByUpdatedDate()
    {
        Logger.LogInformation("Starting partial workshop scan by updated date");
        await Run(WorkshopLister.QueryType.ByUpdatedDate, 5);
        Logger.LogInformation("Finished partial workshop scan by updated date");
    }
}
