using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Get.All;

internal class Endpoint : Endpoint<FavoritesGetAllRequestDTO, FavoritesGetAllResponseDTO>
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
        AllowAnonymous();
        Get("favorites");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(FavoritesGetAllRequestDTO req, CancellationToken ct)
    {
        FavoritesApi api = new FavoritesApi(client);

        Result<DirectusGetMultipleResponse<FavoriteModel>> getResult = await api.Get(filter =>
            {
                filter.WithUser(req.UserId)
                    .WithSteamId(req.UserSteamId)
                    .WithLevel(req.LevelId)
                    .WithUid(req.LevelUid)
                    .WithWorkshopId(req.LevelWorkshopId)
                    .WithOffset(req.Offset)
                    .WithLimit(req.Limit);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get favorites: {Result}", getResult.ToString());
            ThrowError("Unable to get favorites");
        }

        List<FavoriteResponseModel> favorites = new();

        foreach (FavoriteModel model in getResult.Value.Data)
        {
            favorites.Add(model);
        }

        await SendOkAsync(new FavoritesGetAllResponseDTO()
            {
                Favorites = favorites,
                TotalAmount = getResult.Value.Metadata!.FilterCount!.Value
            },
            ct);
    }
}
