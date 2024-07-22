namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public class RemoteStorageOptions
{
    public const string Key = "RemoteStorage";

    public string Credentials { get; set; } = null!;
    public string Bucket { get; set; } = null!;
}
