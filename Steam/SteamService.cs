using FluentResults;
using Steam.Models.SteamUserAuth;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace TNRD.Zeepkist.GTR.Backend.Steam;

public interface ISteamService
{
    Task<Result<string>> Authenticate(string ticket);
}

public class SteamService : ISteamService
{
    private const uint AppId = 1440670;

    private readonly ISteamWebInterfaceFactory _interfaceFactory;
    private readonly ILogger<SteamService> _logger;

    public SteamService(ISteamWebInterfaceFactory interfaceFactory, ILogger<SteamService> logger)
    {
        _interfaceFactory = interfaceFactory;
        _logger = logger;
    }

    public async Task<Result<string>> Authenticate(string ticket)
    {
        SteamUserAuth userAuth = _interfaceFactory.CreateSteamWebInterface<SteamUserAuth>(new HttpClient());
        ISteamWebResponse<SteamUserAuthResponseModel> result = await userAuth.AuthenticateUserTicket(AppId, ticket);
        SteamUserAuthResponse response = result.Data.Response;

        if (!response.Success)
        {
            _logger.LogError(
                "Unable to authenticate with steam: {Code}:{Description}",
                response.Error.ErrorCode,
                response.Error.ErrorDesc);

            return Result.Fail("Unable to authenticate with steam");
        }

        if (response.Params.PublisherBanned)
        {
            return Result.Fail("Publisher banned");
        }

        if (response.Params.VacBanned)
        {
            return Result.Fail("VAC banned");
        }

        if (!string.IsNullOrWhiteSpace(response.Params.OwnerSteamId) &&
            response.Params.OwnerSteamId != response.Params.SteamId)
        {
            return Result.Fail("Steam ID mismatch");
        }

        return Result.Ok(response.Params.SteamId);
    }

    public async Task QueryFiles(string? cursor = null)
    {
    }
}
