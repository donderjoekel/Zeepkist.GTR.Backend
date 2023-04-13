using System.Net;
using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, FavoriteResponseModel>
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
        Get("favorites/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        FavoritesApi api = new FavoritesApi(client);
        Result<FavoriteModel?> result = await api.GetById(req.Id, ct);

        if (result.IsFailed)
        {
            if (result.TryGetReason(out StatusCodeReason reason) && reason.StatusCode == HttpStatusCode.NotFound)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            Logger.LogError("Unable to get favorite: {Result}", result);
            ThrowError("Unable to get favorite");
        }

        if (result.Value == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(result.Value!, ct);
        }
    }
}
