using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TNRD.Zeepkist.GTR.Backend.Jwt;

public interface IJwtService
{
    const string SteamIdClaimName = "steamid";

    string GenerateToken(ulong steamId);
}

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;

    public JwtService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(ulong steamId)
    {
        JwtSecurityTokenHandler handler = new();
        byte[] key = Encoding.UTF8.GetBytes(_jwtOptions.Token);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, steamId.ToString()),
            new Claim(IJwtService.SteamIdClaimName, steamId.ToString()),
        ];

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = "https://api.zeepkist-gtr.com",
            Audience = "https://api.zeepkist-gtr.com",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
