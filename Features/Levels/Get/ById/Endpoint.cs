using System.Net;
using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, LevelResponseModel>
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
        Get("levels/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        LevelsApi api = new LevelsApi(client);
        Result<LevelModel?> result = await api.GetById(req.Id, ct);

        if (result.IsFailed)
        {
            if (result.TryGetReason(out StatusCodeReason reason) && reason.StatusCode == HttpStatusCode.NotFound)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            Logger.LogError("Unable to get level: {Result}", result);
            ThrowError("Unable to get level");
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
