using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class WorkshopDownloader
{
    private const int ItemsPerChunk = 10;
    private const int MaxConcurrency = 5;

    private readonly ILogger<WorkshopDownloader> _logger;
    private readonly IWorkshopService _workshopService;

    public WorkshopDownloader(ILogger<WorkshopDownloader> logger, IWorkshopService workshopService)
    {
        _logger = logger;
        _workshopService = workshopService;
    }

    public async Task<DownloadResult[]> Download(IEnumerable<PublishedFileDetails> publishedFileDetails)
    {
        List<PublishedFileDetails[]> chunks = publishedFileDetails.Chunk(ItemsPerChunk).ToList();
        SemaphoreSlim semaphore = new(MaxConcurrency, MaxConcurrency);
        List<Task<DownloadResult>> tasks = new();

        for (int i = 0; i < chunks.Count; i++)
        {
            PublishedFileDetails[] chunk = chunks[i];
            int index = i;
            tasks.Add(DownloadWorkshopItems(chunk, semaphore, index));
        }

        _logger.LogInformation("Waiting for all ({Amount}) tasks to complete (this may take a while)", chunks.Count);
        DownloadResult[] results = await Task.WhenAll(tasks);
        _logger.LogInformation("All tasks completed");
        return results;
    }

    private async Task<DownloadResult> DownloadWorkshopItems(
        IEnumerable<PublishedFileDetails> publishedFileDetails,
        SemaphoreSlim semaphore,
        int index)
    {
        await semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Starting workshop download {Index}", index);
            Result<WorkshopDownloads> result =
                await _workshopService.DownloadWorkshopItems(
                    publishedFileDetails.Select(x => ulong.Parse(x.PublishedFileId)));
            _logger.LogInformation("Finished workshop download {Index}", index);
            return new DownloadResult(publishedFileDetails, result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to download workshop items");
            return new DownloadResult(publishedFileDetails, Result.Fail(new ExceptionalError(e)));
        }
        finally
        {
            semaphore.Release();
        }
    }
}