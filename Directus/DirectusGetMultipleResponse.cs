using Newtonsoft.Json;

namespace TNRD.Zeepkist.GTR.Backend.Directus;

internal class DirectusGetMultipleResponse<T>
{
    [JsonProperty("meta")] public Metadata? Metadata { get; set; }
    [JsonProperty("data")] public T[] Data { get; set; } = null!;

    public bool HasItems => Data.Length > 0;

    public T? FirstItem => Data[0];
}

internal class DirectusGetSingleResponse<T>
{
    [JsonProperty("data")] public T? Data { get; set; }
}
