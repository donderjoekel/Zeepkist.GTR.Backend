using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Submit;

internal class Endpoint : Endpoint<VotesSubmitRequestDTO>
{
    private readonly IDirectusClient client;
    private readonly VotesApi api;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
        api = new VotesApi(client);
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("votes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VotesSubmitRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        Result<DirectusGetMultipleResponse<VoteModel>> result = await api.Get(filter =>
            {
                filter
                    .WithLevel(req.Level)
                    .WithUser(userId)
                    .WithCategory(req.Category);
            },
            ct);

        if (result.IsFailed)
        {
            Logger.LogError("Unable to get vote: {Result}", result.ToString());
            ThrowError("Unable to get vote");
        }

        if (result.Value.HasItems)
        {
            VoteModel item = result.Value.FirstItem!;
            if (item.Score == req.Score)
                await SendOkAsync(ct);
            else
                await UpdateExistingVote(req, item.Id, ct);
        }
        else
        {
            await PostNewVote(req, userId, ct);
        }
    }

    private async Task UpdateExistingVote(VotesSubmitRequestDTO req, int voteId, CancellationToken ct)
    {
        Result<VoteModel> patchResult = await api.Patch(voteId,
            builder =>
            {
                builder.WithScore(req.Score);
            },
            ct);

        if (patchResult.IsFailed)
        {
            Logger.LogError("Unable to patch vote: {Result}", patchResult.ToString());
            ThrowError("Unable to patch vote");
        }

        await SendOkAsync(ct);
    }

    private async Task PostNewVote(VotesSubmitRequestDTO req, int userId, CancellationToken ct)
    {
        Result<int> postResult = await api.Post(builder =>
            {
                builder
                    .WithLevel(req.Level)
                    .WithUser(userId)
                    .WithCategory(req.Category)
                    .WithScore(req.Score);
            },
            ct);

        if (postResult.IsFailed)
        {
            Logger.LogError("Unable to post vote: {Result}", postResult.ToString());
            ThrowError("Unable to post vote");
        }

        await SendOkAsync(ct);
    }
}
