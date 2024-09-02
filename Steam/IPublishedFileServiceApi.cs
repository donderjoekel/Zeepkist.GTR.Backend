using Refit;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Steam;

public interface IPublishedFileServiceApi
{
    [Get("/QueryFiles/v1/")]
    Task<QueryFilesResult> QueryFiles(
        string key,
        string cursor,
        [AliasAs("numperpage")] int numPerPage = 10,
        [AliasAs("appid")] uint appId = 1440670,
        [AliasAs("return_metadata")] bool returnMetadata = true,
        [AliasAs("return_details")] bool returnDetails = true);

    [Get("/QueryFiles/v1/")]
    Task<QueryFilesResult> QueryFilesByPublicationDate(
        string key,
        string cursor,
        [AliasAs("numperpage")] int numPerPage = 10,
        [AliasAs("query_type")] int queryType = 1,
        [AliasAs("appid")] uint appId = 1440670,
        [AliasAs("return_metadata")] bool returnMetadata = true,
        [AliasAs("return_details")] bool returnDetails = true);

    [Get("/QueryFiles/v1/")]
    Task<QueryFilesResult> QueryFilesByUpdatedDate(
        string key,
        string cursor,
        [AliasAs("numperpage")] int numPerPage = 10,
        [AliasAs("query_type")] int queryType = 21,
        [AliasAs("appid")] uint appId = 1440670,
        [AliasAs("return_metadata")] bool returnMetadata = true,
        [AliasAs("return_details")] bool returnDetails = true);

    [Get("/GetDetails/v1/")]
    Task<QueryFilesResult> GetDetails(
        string key,
        [AliasAs("publishedfileids[0]")] decimal publishedFileId);
}
