using System.Security.Claims;
using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Name;

internal class Endpoint : Endpoint<UsersUpdateNameRequestDTO>
{
    private readonly IDirectusClient client;

    /// <inheritdoc />
    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("users/name");
        Description(x => x.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersUpdateNameRequestDTO req, CancellationToken ct)
    {
        Claim? userClaim = User.FindFirst("UserId");
        if (userClaim == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        string userId = userClaim.Value.Split('_')[0];

        Result<DirectusPostResponse<UserModel>> result =
            await client.Patch<DirectusPostResponse<UserModel>>($"items/users/{userId}",
                UsersFactory.New().WithSteamName(req.SteamName).Build(),
                ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            Logger.LogCritical("Unable to patch steam name: {Result}", result.ToString());
            await SendAsync(null!, 500, ct);
        }
    }
}
