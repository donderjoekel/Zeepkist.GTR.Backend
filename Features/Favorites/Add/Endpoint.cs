using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Add;

internal class Endpoint : Endpoint<FavoritesAddRequestDTO, GenericIdResponseDTO>
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
        Post("favorites");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(FavoritesAddRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        FavoritesApi api = new FavoritesApi(client);

        Result<DirectusGetMultipleResponse<FavoriteModel>> getResult = await api.Get(filter =>
            {
                filter
                    .WithLevel(req.LevelId)
                    .WithUser(userId);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get favorite: {Result}", getResult.ToString());
            ThrowError("Unable to get favorite");
        }

        if (getResult.Value.HasItems)
        {
            await SendOkAsync(ct);
            return;
        }

        Result<int> postResult = await api.Post(builder =>
            {
                builder
                    .WithUser(userId)
                    .WithLevel(req.LevelId);
            },
            ct);

        if (postResult.IsSuccess)
        {
            await SendOkAsync(new GenericIdResponseDTO()
                {
                    Id = postResult.Value
                },
                ct);
        }
        else
        {
            Logger.LogCritical("Failed to post favorite: {Result}", postResult.ToString());
            ThrowError("Failed to post favorite");
        }
    }
}
