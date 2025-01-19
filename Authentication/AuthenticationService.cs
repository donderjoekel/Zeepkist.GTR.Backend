using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Steam;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthenticationData>> Login(ulong steamId, string authenticationTicket);
    Result<AuthenticationData> Refresh(ulong steamId, string accessToken, string refreshToken);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ISteamService _steamService;
    private readonly IJwtService _jwtService;
    private readonly IAuthenticationRepository _repository;
    private readonly IUserService _userService;

    public AuthenticationService(
        ISteamService steamService,
        IJwtService jwtService,
        IAuthenticationRepository repository,
        IUserService userService)
    {
        _steamService = steamService;
        _jwtService = jwtService;
        _repository = repository;
        _userService = userService;
    }

    public async Task<Result<AuthenticationData>> Login(ulong steamId, string authenticationTicket)
    {
        Result<string> authenticationResult = await _steamService.Authenticate(authenticationTicket);
        if (authenticationResult.IsFailed)
        {
            return authenticationResult.ToResult();
        }
        
        if (authenticationResult.Value != steamId.ToString())
        {
            return Result.Fail("Steam ID does not match");
        }

        if (!_userService.TryGet(steamId, out User? user))
        {
            user = _userService.Create(steamId);
        }

        string accessToken = _jwtService.GenerateToken(steamId);
        long accessTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        string refreshToken = Guid.NewGuid().ToString();
        long refreshTokenExpiry = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds();

        _repository.Upsert(
            auth => auth.IdUser == user.Id,
            () => new Auth
            {
                IdUser = user.Id,
                Type = 0,
                AccessToken = accessToken,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            },
            auth =>
            {
                auth.AccessToken = accessToken;
                auth.AccessTokenExpiry = accessTokenExpiry;
                auth.RefreshToken = refreshToken;
                auth.RefreshTokenExpiry = refreshTokenExpiry;
                return auth;
            });

        return Result.Ok(
            new AuthenticationData
            {
                AccessToken = accessToken,
                AccessTokenExpiry = accessTokenExpiry.ToString(),
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry.ToString()
            });
    }

    public Result<AuthenticationData> Refresh(ulong steamId, string accessToken, string refreshToken)
    {
        if (!_userService.TryGet(steamId, out User? user))
        {
            return Result.Fail("User not found");
        }

        Auth? auth = _repository.GetSingle(x => x.IdUser == user.Id);

        if (auth == null)
        {
            return Result.Fail("No authentication data");
        }

        if (auth.RefreshToken != refreshToken)
        {
            return Result.Fail("Invalid refresh token");
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > auth.RefreshTokenExpiry)
        {
            return Result.Fail("Refresh token expired");
        }

        string newAccessToken = _jwtService.GenerateToken(steamId);
        long newAccessTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        string newRefreshToken = Guid.NewGuid().ToString();
        long newRefreshTokenExpiry = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds();

        auth.AccessToken = newAccessToken;
        auth.AccessTokenExpiry = newAccessTokenExpiry;
        auth.RefreshToken = newRefreshToken;
        auth.RefreshTokenExpiry = newRefreshTokenExpiry;

        _repository.Update(auth);

        return Result.Ok(
            new AuthenticationData
            {
                AccessToken = newAccessToken,
                AccessTokenExpiry = newAccessTokenExpiry.ToString(),
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = newRefreshTokenExpiry.ToString()
            });
    }
}
