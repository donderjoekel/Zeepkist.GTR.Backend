namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public class GoogleCloudStorageOptions
{
    public const string Key = "GoogleCloudStorage";

    public string Credentials { get; set; } = null!;
    public string Bucket { get; set; } = null!;
}
