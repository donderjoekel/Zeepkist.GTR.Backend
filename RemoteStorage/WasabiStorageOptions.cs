namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public class WasabiStorageOptions
{
    public const string Key = "WasabiStorage";

    public string AccessKeyId { get; set; } = null!;
    public string SecretAccessKey { get; set; } = null!;
    public string Bucket { get; set; } = null!;
    public string ServiceUrl { get; set; } = null!;
}
