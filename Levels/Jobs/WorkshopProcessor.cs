using TNRD.Zeepkist.GTR.Backend.Hashing;
using TNRD.Zeepkist.GTR.Backend.Levels.Items;
using TNRD.Zeepkist.GTR.Backend.Levels.Metadata;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class WorkshopProcessor
{
    private const int MaxConcurrency = 10;

    private readonly ILogger<WorkshopProcessor> _logger;
    private readonly IServiceProvider _provider;

    public WorkshopProcessor(
        ILogger<WorkshopProcessor> logger,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public async Task Process(DownloadResult[] downloadResults)
    {
        List<Task> tasks = new();
        SemaphoreSlim semaphore = new(MaxConcurrency, MaxConcurrency);

        foreach (DownloadResult downloadResult in downloadResults)
        {
            if (downloadResult.Result.IsFailed)
            {
                _logger.LogError("Failed to download workshop items: {Result}", downloadResult.Result.ToString());
                continue;
            }

            tasks.Add(CreateTask(semaphore, downloadResult));
        }

        await Task.WhenAll(tasks);
    }

    private Task CreateTask(SemaphoreSlim semaphoreSlim, DownloadResult downloadResult)
    {
        return Task.Run(() =>
        {
            semaphoreSlim.Wait();
            IServiceScope scope = _provider.CreateScope();

            try
            {
                WorkshopProcess workshopProcess = scope.ServiceProvider.GetRequiredService<WorkshopProcess>();

                Dictionary<ulong, WorkshopItem> workshopItems =
                    downloadResult.Result.Value.Items.ToDictionary(x => x.PublishedFileId);
                foreach (PublishedFileDetails publishedFileDetails in downloadResult.PublishedFileDetails)
                {
                    workshopProcess.Execute(publishedFileDetails, workshopItems);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process workshop items");
            }
            finally
            {
                scope.Dispose();
                semaphoreSlim.Release();
            }
        });
    }

    public class WorkshopProcess
    {
        private readonly ILogger<WorkshopProcess> _logger;
        private readonly ILevelService _levelService;
        private readonly ILevelMetadataService _levelMetadataService;
        private readonly ILevelItemsService _levelItemsService;
        private readonly IHashService _hashService;
        private readonly IZeeplevelService _zeeplevelService;

        public WorkshopProcess(
            ILogger<WorkshopProcess> logger,
            ILevelService levelService,
            ILevelMetadataService levelMetadataService,
            ILevelItemsService levelItemsService,
            IHashService hashService,
            IZeeplevelService zeeplevelService)
        {
            _logger = logger;
            _levelService = levelService;
            _levelMetadataService = levelMetadataService;
            _levelItemsService = levelItemsService;
            _hashService = hashService;
            _zeeplevelService = zeeplevelService;
        }

        public void Execute(
            PublishedFileDetails publishedFileDetails,
            Dictionary<ulong, WorkshopItem> workshopItems)
        {
            _logger.LogInformation("Processing published file details {PublishedFileId}",
                publishedFileDetails.PublishedFileId);

            DeleteMissingLevels(publishedFileDetails, workshopItems);

            ulong publishedFileId = ulong.Parse(publishedFileDetails.PublishedFileId);

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

        private void DeleteMissingLevels(PublishedFileDetails publishedFileDetails,
            Dictionary<ulong, WorkshopItem> workshopItems)
        {
            _logger.LogInformation("Deleting missing levels for {PublishedFileId}",
                publishedFileDetails.PublishedFileId);

            IEnumerable<LevelItem> existingLevelItems =
                _levelItemsService.GetForPublishedFileDetails(publishedFileDetails);
            List<ZeepLevel> zeepLevels = new();

            foreach (KeyValuePair<ulong, WorkshopItem> kvp in workshopItems)
            {
                foreach (WorkshopLevel workshopLevel in kvp.Value.Levels)
                {
                    ZeepLevel? zeepLevel = _zeeplevelService.Parse(workshopLevel.ZeeplevelPath);
                    if (zeepLevel != null)
                    {
                        zeepLevels.Add(zeepLevel);
                    }
                }
            }

            foreach (LevelItem levelItem in existingLevelItems)
            {
                if (zeepLevels.Any(x => x.UniqueId == levelItem.FileUid))
                    continue;

                _logger.LogInformation("Marking level item {Id} for {PublishedFileId} as deleted",
                    levelItem.Id,
                    publishedFileDetails.PublishedFileId);

                _levelItemsService.MarkDeleted(levelItem);
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

            if (!_levelService.TryGetByHash(hash, out Level? level))
            {
                _logger.LogInformation("Creating level for {PublishedFileId};{Uid}",
                    publishedFileId,
                    zeepLevel.UniqueId);
                level = _levelService.Create(hash);
            }

            if (!_levelItemsService.Exists(publishedFileDetails, zeepLevel, hash))
            {
                _logger.LogInformation("Creating level item for {PublishedFileId};{Uid}",
                    publishedFileId,
                    zeepLevel.UniqueId);
                _levelItemsService.Create(publishedFileDetails, workshopLevel, zeepLevel, level);
            }

            if (!_levelMetadataService.Exists(hash))
            {
                _logger.LogInformation("Creating level metadata for {PublishedFileId};{Uid}",
                    publishedFileId,
                    zeepLevel.UniqueId);
                _levelMetadataService.Create(zeepLevel, hash);
            }
        }
    }
}