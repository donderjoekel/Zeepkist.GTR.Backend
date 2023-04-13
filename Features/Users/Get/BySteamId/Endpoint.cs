using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.BySteamId;

internal class Endpoint : Endpoint<UsersGetBySteamIdRequestDTO, UserResponseModel>
{
    private readonly IDirectusClient client;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users/steam/{SteamId}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersGetBySteamIdRequestDTO req, CancellationToken ct)
    {
        UsersApi api = new UsersApi(client);

        Result<DirectusGetMultipleResponse<UserModel>> getResult =
            await api.Get(filter => filter.WithSteamId(req.SteamId), ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to check if user exists: {Result}", getResult.ToString());
            ThrowError("Unable to check if user exists");
        }

        if (getResult.Value.HasItems)
        {
            await SendOkAsync(getResult.Value.FirstItem!, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
