using Newtonsoft.Json;

namespace TNRD.Zeepkist.GTR.Backend.Directus;

internal class Metadata
{
    [JsonProperty("total_count")] public int? TotalCount { get; set; }
    [JsonProperty("filter_count")] public int? FilterCount { get; set; }
}
