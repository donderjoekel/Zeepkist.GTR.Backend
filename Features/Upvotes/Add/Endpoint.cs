using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Add;

internal class Endpoint : Endpoint<UpvotesAddRequestDTO, GenericIdResponseDTO>
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
        Post("upvotes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpvotesAddRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        UpvotesApi api = new UpvotesApi(client);

        Result<DirectusGetMultipleResponse<UpvoteModel>> getResult = await api.Get(filter =>
            {
                filter
                    .WithLevelId(req.LevelId)
                    .WithUserId(userId);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get upvote: {Result}", getResult.ToString());
            ThrowError("Unable to get upvote");
        }

        if (getResult.Value.HasItems)
        {
            await SendOkAsync(new GenericIdResponseDTO()
                {
                    Id = getResult.Value.FirstItem!.Id
                },
                ct);
            return;
        }

        Result<int> postResult = await api.Post(builder =>
            {
                builder
                    .WithUser(userId)
                    .WithLevel(req.LevelId);
            },
            ct);

        if (postResult.IsFailed)
        {
            Logger.LogError("Failed to post upvote: {Result}", postResult.ToString());
            ThrowError("Failed to post upvote");
        }

        await SendOkAsync(new GenericIdResponseDTO(postResult.Value), ct);
    }
}
