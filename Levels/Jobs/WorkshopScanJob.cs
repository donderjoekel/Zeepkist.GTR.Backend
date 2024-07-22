using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Hashing;
using TNRD.Zeepkist.GTR.Backend.Levels.Items;
using TNRD.Zeepkist.GTR.Backend.Levels.Metadata;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public abstract class WorkshopScanJob
{
    private readonly ILogger _logger;
    private readonly IHashService _hashService;
    private readonly ILevelService _levelService;
    private readonly ILevelMetadataService _levelMetadataService;
    private readonly ILevelItemsService _levelItemsService;
    private readonly IWorkshopService _workshopService;
    private readonly IZeeplevelService _zeeplevelService;

    protected WorkshopScanJob(
        ILogger logger,
        IHashService hashService,
        ILevelService levelService,
        ILevelMetadataService levelMetadataService,
        ILevelItemsService levelItemsService,
        IWorkshopService workshopService,
        IZeeplevelService zeeplevelService)
    {
        _logger = logger;
        _hashService = hashService;
        _levelService = levelService;
        _levelMetadataService = levelMetadataService;
        _levelItemsService = levelItemsService;
        _workshopService = workshopService;
        _zeeplevelService = zeeplevelService;
    }

    protected async Task ProcessPage(QueryFilesResult result)
    {
        List<ulong> publishedFileIds = result.Response.PublishedFileDetails
            .Select(x => ulong.Parse(x.PublishedFileId))
            .ToList();

        Result<WorkshopDownloads> downloadResult = await _workshopService.DownloadWorkshopItems(publishedFileIds);

        if (downloadResult.IsFailed)
        {
            _logger.LogWarning("Failed to download workshop items");
            _workshopService.RemoveAllDownloads();
            return;
        }

        Dictionary<ulong, WorkshopItem> workshopItems = downloadResult.Value.Items.ToDictionary(x => x.PublishedFileId);
        foreach (PublishedFileDetails publishedFileDetails in result.Response.PublishedFileDetails)
        {
            ProcessPublishedFileDetails(publishedFileDetails, workshopItems);
        }

        _workshopService.RemoveDownloads(downloadResult.Value);
    }

    private void ProcessPublishedFileDetails(
        PublishedFileDetails publishedFileDetails,
        Dictionary<ulong, WorkshopItem> workshopItems)
    {
        ulong publishedFileId = ulong.Parse(publishedFileDetails.PublishedFileId);

        // TODO: get level items based on workshop id and check what needs to be marked as deleted

        if (!workshopItems.TryGetValue(publishedFileId, out WorkshopItem? workshopItem))
        {
            _logger.LogWarning(
                "PublishedFileId ({PublishedFileId}) has not been downloaded from workshop",
                publishedFileId);

            return;
        }

        foreach (WorkshopLevel level in workshopItem.Levels)
        {
            ProcessWorkshopLevel(publishedFileId, publishedFileDetails, level);
        }
    }

    private void ProcessWorkshopLevel(
        ulong publishedFileId,
        PublishedFileDetails publishedFileDetails,
        WorkshopLevel workshopLevel)
    {
        ZeepLevel? zeepLevel = _zeeplevelService.Parse(workshopLevel.ZeeplevelPath);
        if (zeepLevel == null)
        {
            _logger.LogWarning(
                "Unable to parse zeeplevel from workshop item {PublishedFileId}",
                publishedFileId);

            return;
        }

        string hash = _hashService.Hash(zeepLevel);

        if (!_levelService.TryGetByHash(hash, out _))
        {
            _levelService.Create(hash);
        }

        if (!_levelItemsService.Exists(publishedFileDetails, zeepLevel, hash))
        {
            _levelItemsService.Create(publishedFileDetails, workshopLevel, zeepLevel, hash);
        }

        if (!_levelMetadataService.Exists(hash))
        {
            _levelMetadataService.Create(zeepLevel, hash);
        }
    }
}
