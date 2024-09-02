using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public abstract class WorkshopScanJob
{
    private readonly ILogger _logger;
    private readonly WorkshopLister _workshopLister;
    private readonly WorkshopDownloader _workshopDownloader;
    private readonly WorkshopProcessor _workshopProcessor;
    private readonly IWorkshopService _workshopService;

    protected WorkshopScanJob(ILogger logger,
        WorkshopLister workshopLister,
        WorkshopDownloader workshopDownloader,
        WorkshopProcessor workshopProcessor,
        IWorkshopService workshopService)
    {
        _logger = logger;
        _workshopLister = workshopLister;
        _workshopDownloader = workshopDownloader;
        _workshopProcessor = workshopProcessor;
        _workshopService = workshopService;
    }

    public ILogger Logger => _logger;

    protected async Task Run(WorkshopLister.QueryType queryType, int pageLimit = -1)
    {
        try
        {
            Result<List<PublishedFileDetails>> publishedFileResults = await _workshopLister.List(queryType, pageLimit);

            if (publishedFileResults.IsFailed)
            {
                _logger.LogWarning("Failed to get published file ids");
                return;
            }

            DownloadResult[] downloadResults = await _workshopDownloader.Download(publishedFileResults.Value);

            await _workshopProcessor.Process(downloadResults);

            foreach (DownloadResult downloadResult in downloadResults)
            {
                if (downloadResult.Result.IsSuccess)
                {
                    _workshopService.RemoveDownloads(downloadResult.Result.Value);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to run job");
        }
    }
}
