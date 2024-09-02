using FluentResults;
using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Levels.Jobs;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests.Jobs;

public class ProcessLevelRequestsJob
{
    private readonly ILevelRequestsService _service;
    private readonly WorkshopLister _workshopLister;
    private readonly WorkshopDownloader _workshopDownloader;
    private readonly WorkshopProcessor _workshopProcessor;
    private readonly ILogger<ProcessLevelRequestsJob> _logger;
    private readonly IWorkshopService _workshopService;

    public ProcessLevelRequestsJob(ILevelRequestsService service,
        WorkshopLister workshopLister,
        WorkshopDownloader workshopDownloader,
        WorkshopProcessor workshopProcessor,
        ILogger<ProcessLevelRequestsJob> logger,
        IWorkshopService workshopService)
    {
        _service = service;
        _workshopLister = workshopLister;
        _workshopDownloader = workshopDownloader;
        _workshopProcessor = workshopProcessor;
        _logger = logger;
        _workshopService = workshopService;
    }

    [UsedImplicitly]
    public async Task ExecuteAsync()
    {
        List<LevelRequest> requests = _service.GetRequests().ToList();

        foreach (LevelRequest request in requests)
        {
            try
            {
                Result<List<PublishedFileDetails>>
                    publishedFileResults = await _workshopLister.List(request.WorkshopId);
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
                _logger.LogError(e, "Failed to process level request");
                _service.Delete(request);
            }
        }
    }
}