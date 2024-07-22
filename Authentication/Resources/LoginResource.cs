namespace TNRD.Zeepkist.GTR.Backend.Authentication.Resources;

public class LoginResource
{
    public string ModVersion { get; set; } = null!;
    public ulong SteamId { get; set; }
    public string AuthenticationTicket { get; set; } = null!;
}
