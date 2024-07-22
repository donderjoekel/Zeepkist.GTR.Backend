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
        string cursor = "*";

        while (true)
        {
            QueryFilesResult result
                = await _publishedFileServiceApi.QueryFiles(_steamOptions.ApiKey, cursor);

            string currentCursor = cursor;
            cursor = result.Response.NextCursor;

            await ProcessPage(result);

            if (cursor == currentCursor)
                break;
        }
    }
}
