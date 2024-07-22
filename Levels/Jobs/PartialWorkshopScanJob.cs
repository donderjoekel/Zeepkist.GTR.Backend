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

public class PartialWorkshopScanJob : WorkshopScanJob
{
    private readonly IPublishedFileServiceApi _publishedFileServiceApi;
    private readonly SteamOptions _steamOptions;

    public PartialWorkshopScanJob(
        ILogger<PartialWorkshopScanJob> logger,
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
        await ScanByPublicationDate();
        await ScanByUpdatedDate();
    }

    private async Task ScanByPublicationDate()
    {
        string cursor = "*";

        for (int i = 0; i < 5; i++)
        {
            QueryFilesResult result
                = await _publishedFileServiceApi.QueryFilesByPublicationDate(_steamOptions.ApiKey, cursor);

            cursor = result.Response.NextCursor;

            await ProcessPage(result);
        }
    }

    private async Task ScanByUpdatedDate()
    {
        string cursor = "*";

        for (int i = 0; i < 5; i++)
        {
            QueryFilesResult result
                = await _publishedFileServiceApi.QueryFilesByUpdatedDate(_steamOptions.ApiKey, cursor);

            cursor = result.Response.NextCursor;

            await ProcessPage(result);
        }
    }
}
