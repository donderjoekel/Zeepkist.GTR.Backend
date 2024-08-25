namespace TNRD.Zeepkist.GTR.Backend.Jwt;

public class JwtOptions
{
    public const string Key = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
