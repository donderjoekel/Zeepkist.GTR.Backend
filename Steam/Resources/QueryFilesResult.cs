using Newtonsoft.Json;

namespace TNRD.Zeepkist.GTR.Backend.Steam.Resources;

public class QueryFilesResult
{
    [JsonProperty("response")] public Response Response { get; set; }
}

public class Response
{
    [JsonProperty("total")] public int Total { get; set; }
    [JsonProperty("next_cursor")] public string NextCursor { get; set; }
    [JsonProperty("publishedfiledetails")] public PublishedFileDetails[] PublishedFileDetails { get; set; }
}
