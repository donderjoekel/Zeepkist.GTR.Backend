namespace TNRD.Zeepkist.GTR.Backend.Authentication.Resources;

public class RefreshResource
{
    public string ModVersion { get; set; } = null!;
    public ulong SteamId { get; set; }
    public string LoginToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
