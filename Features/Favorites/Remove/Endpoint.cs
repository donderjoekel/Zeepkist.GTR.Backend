using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Remove;

internal class Endpoint : Endpoint<FavoritesRemoveRequestDTO>
{
    private readonly IDirectusClient client;
    private readonly FavoritesApi api;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
        api = new FavoritesApi(client);
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Delete("favorites");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(FavoritesRemoveRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        if (req.Id.HasValue)
        {
            await RemoveById(userId, req.Id.Value, ct);
        }
        else if (req.LevelId.HasValue)
        {
            await RemoveByLevelId(userId, req.LevelId.Value, ct);
        }
    }

    private async Task RemoveById(int userId, int id, CancellationToken ct)
    {
        Result<FavoriteModel?> getResult = await api.GetById(id, ct);

        if (getResult.IsFailed)
        {
            Logger.LogError("Failed to get favorite: {Result}", getResult.ToString());
            ThrowError("Failed to get favorite");
        }

        if (getResult.Value == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        int favoriteUserId = getResult.Value.User.Match(i => i, m => m.Id);

        if (userId != favoriteUserId)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        Result deleteResult = await api.Delete(getResult.Value.Id, ct);

        if (deleteResult.IsFailed)
        {
            Logger.LogCritical("Failed to delete favorite: {Result}", deleteResult.ToString());
            ThrowError("Failed to delete favorite");
        }

        await SendOkAsync(ct);
    }

    private async Task RemoveByLevelId(int userId, int levelId, CancellationToken ct)
    {
        Result<DirectusGetMultipleResponse<FavoriteModel>> getResult = await api.Get(filter =>
            {
                filter
                    .WithUser(userId)
                    .WithLevel(levelId);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogError("Failed to get favorite: {Result}", getResult.ToString());
            ThrowError("Failed to get favorite");
        }

        if (!getResult.Value.HasItems)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        Result deleteResult = await api.Delete(getResult.Value.FirstItem!.Id, ct);

        if (deleteResult.IsFailed)
        {
            Logger.LogCritical("Failed to delete favorite: {Result}", deleteResult.ToString());
            ThrowError("Failed to delete favorite");
        }

        await SendOkAsync(ct);
    }
}
