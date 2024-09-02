using FluentResults;
using Microsoft.Extensions.Options;
using Refit;
using TNRD.Zeepkist.GTR.Backend.Steam;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public class WorkshopLister
{
    public enum QueryType
    {
        Normal,
        ByPublicationDate,
        ByUpdatedDate
    }

    private ILogger<WorkshopLister> _logger;
    private IPublishedFileServiceApi _publishedFileServiceApi;
    private SteamOptions _steamOptions;

    public WorkshopLister(ILogger<WorkshopLister> logger,
        IPublishedFileServiceApi publishedFileServiceApi,
        IOptions<SteamOptions> steamOptions)
    {
        _logger = logger;
        _publishedFileServiceApi = publishedFileServiceApi;
        _steamOptions = steamOptions.Value;
    }

    public async Task<Result<List<PublishedFileDetails>>> List(decimal publishedFileId)
    {
        QueryFilesResult result =
            await _publishedFileServiceApi.GetDetails(_steamOptions.ApiKey, publishedFileId);

        if (result.Response.PublishedFileDetails == null)
        {
            return Result.Fail("Unable to parse published file details");
        }

        return Result.Ok(result.Response.PublishedFileDetails.ToList());
    }

    public async Task<Result<List<PublishedFileDetails>>> List(QueryType queryType,
        int pageLimit = -1)
    {
        string cursor = "*";
        int attempts = 0;
        int currentPage = 0;

        List<PublishedFileDetails> publishedFileIds = new();

        while (currentPage < pageLimit || pageLimit == -1)
        {
            _logger.LogInformation("Scanning page {Cursor}", cursor);

            QueryFilesResult result = await QueryFilesResult(queryType, cursor);

            string currentCursor = cursor;
            cursor = result.Response.NextCursor;

            if (cursor == currentCursor && result.Response.PublishedFileDetails == null)
            {
                _logger.LogInformation("Reached end of listing {Cursor}", cursor);
                break; // No more pages
            }

            try
            {
                publishedFileIds.AddRange(result.Response.PublishedFileDetails);
                attempts = 0;
                currentPage++;
            }
            catch (Exception e)
            {
                if (attempts >= 3)
                {
                    _logger.LogError("Failed to process page {Cursor} after 3 attempts", cursor);
                    return Result.Fail($"Failed to process page {cursor} after 3 attempts");
                }

                _logger.LogError(e, "Failed to process page {Cursor}", cursor);
                cursor = currentCursor;
                int delay = 5 * ++attempts;
                _logger.LogInformation("Waiting {Delay} minutes", delay);
                await Task.Delay(TimeSpan.FromMinutes(delay));
                continue;
            }

            if (cursor == currentCursor)
                break;
        }

        return Result.Ok(publishedFileIds);
    }

    private async Task<QueryFilesResult> QueryFilesResult(QueryType queryType, string cursor)
    {
        QueryFilesResult result = queryType switch
        {
            QueryType.Normal => await _publishedFileServiceApi.QueryFiles(_steamOptions.ApiKey, cursor,
                numPerPage: 100),
            QueryType.ByPublicationDate => await _publishedFileServiceApi.QueryFilesByPublicationDate(
                _steamOptions.ApiKey, cursor, numPerPage: 100),
            QueryType.ByUpdatedDate => await _publishedFileServiceApi.QueryFilesByUpdatedDate(_steamOptions.ApiKey,
                cursor, numPerPage: 100),
            _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null)
        };

        return result;
    }
}