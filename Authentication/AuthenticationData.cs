namespace TNRD.Zeepkist.GTR.Backend.Authentication;

public class AuthenticationData
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string AccessTokenExpiry { get; set; }
    public string RefreshTokenExpiry { get; set; }
}
