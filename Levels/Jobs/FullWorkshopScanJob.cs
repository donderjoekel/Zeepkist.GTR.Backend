using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using TNRD.Zeepkist.GTR.Backend.Hashing;
using TNRD.Zeepkist.GTR.Backend.Levels.Items;
using TNRD.Zeepkist.GTR.Backend.Levels.Metadata;
using TNRD.Zeepkist.GTR.Backend.Steam;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class FullWorkshopScanJob : WorkshopScanJob
{
    private static bool IsRunning = false;

    private readonly IPublishedFileServiceApi _publishedFileServiceApi;
    private readonly SteamOptions _steamOptions;

    public FullWorkshopScanJob(
        ILogger<FullWorkshopScanJob> logger,
        IHashService hashService,
        ILevelService levelService,
        ILevelMetadataService levelMetadataService,
        ILevelItemsService levelItemsService,
        IWorkshopService workshopService,
        IZeeplevelService zeeplevelService,
        IPublishedFileServiceApi publishedFileServiceApi,
        IOptions<SteamOptions> steamOptions)
        : base(
            logger,
            hashService,
            levelService,
            levelMetadataService,
            levelItemsService,
            workshopService,
            zeeplevelService)
    {
        _publishedFileServiceApi = publishedFileServiceApi;
        _steamOptions = steamOptions.Value;
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

        string cursor = "*";
        int attempts = 0;

        while (true)
        {
            Logger.LogInformation("Scanning page {Cursor}", cursor);
            QueryFilesResult result
                = await _publishedFileServiceApi.QueryFiles(_steamOptions.ApiKey, cursor);

            string currentCursor = cursor;
            cursor = result.Response.NextCursor;

            if (cursor == currentCursor && result.Response.PublishedFileDetails == null)
            {
                Logger.LogInformation("Reached end of listing {Cursor}", cursor);
                break; // No more pages
            }

            try
            {
                await ProcessPage(result);
                attempts = 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process page {Cursor}", cursor);
                if (attempts >= 3)
                {
                    Logger.LogError("Failed to process page {Cursor} after 3 attempts", cursor);
                    break;
                }

                Logger.LogInformation("Changing cursor back from {Current} to {Previous}", cursor, currentCursor);
                cursor = currentCursor;
                int delay = 5 * ++attempts;
                Logger.LogInformation("Waiting {Delay} minutes", delay);
                await Task.Delay(TimeSpan.FromMinutes(delay));
                continue;
            }

            if (cursor == currentCursor)
                break;
        }

        IsRunning = false;
    }
}
