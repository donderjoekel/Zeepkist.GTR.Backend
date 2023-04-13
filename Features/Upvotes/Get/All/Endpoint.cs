using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Get.All;

internal class Endpoint : Endpoint<UpvotesGetRequestDTO, UpvotesGetResponseDTO>
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
        Get("upvotes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpvotesGetRequestDTO req, CancellationToken ct)
    {
        UpvotesApi upvotesApi = new UpvotesApi(client);

        Result<DirectusGetMultipleResponse<UpvoteModel>> getResult = await upvotesApi.Get(
            builder => { BuildFilter(builder, req); },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get upvotes: {Result}", getResult.ToString());
            ThrowError("Unable to get upvotes");
        }

        List<UpvoteResponseModel> upvotes = new();

        foreach (UpvoteModel model in getResult.Value.Data)
        {
            upvotes.Add(model);
        }

        await SendOkAsync(new UpvotesGetResponseDTO()
            {
                Upvotes = upvotes,
                TotalAmount = getResult.Value.Metadata!.FilterCount!.Value
            },
            cancellation: ct);
    }

    private static void BuildFilter(UpvotesFilterBuilder builder, UpvotesGetRequestDTO req)
    {
        builder.WithDepth(3);

        if (!string.IsNullOrEmpty(req.UserSteamId))
            builder.WithUserSteamId(req.UserSteamId);
        if (req.UserId.HasValue)
            builder.WithUserId(req.UserId.Value);
        if (!string.IsNullOrEmpty(req.LevelUid))
            builder.WithLevelUid(req.LevelUid);
        if (req.LevelId.HasValue)
            builder.WithLevelId(req.LevelId.Value);
        if (!string.IsNullOrEmpty(req.LevelWorkshopId))
            builder.WithLevelWorkshopId(req.LevelWorkshopId);

        builder.WithOffset(req.Offset);
        builder.WithLimit(req.Limit);
    }
}
