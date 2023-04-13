using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Remove;

internal class Endpoint : Endpoint<UpvotesRemoveRequestDTO>
{
    private readonly IDirectusClient client;
    private readonly UpvotesApi api;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
        api = new UpvotesApi(client);
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Delete("upvotes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpvotesRemoveRequestDTO req, CancellationToken ct)
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
        Result<UpvoteModel?> getResult = await api.GetById(id, ct);

        if (getResult.IsFailed)
        {
            Logger.LogError("Failed to get upvote: {Result}", getResult.ToString());
            ThrowError("Failed to get upvote");
        }

        if (getResult.Value == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        int upvoteUserId = getResult.Value.User.Match(i => i, m => m.Id);

        if (userId != upvoteUserId)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        Result deleteResult = await api.Delete(id, ct);

        if (deleteResult.IsFailed)
        {
            Logger.LogError("Failed to delete upvote: {Result}", deleteResult.ToString());
            ThrowError("Failed to delete upvote");
        }

        await SendOkAsync(ct);
    }

    private async Task RemoveByLevelId(int userId, int levelId, CancellationToken ct)
    {
        Result<DirectusGetMultipleResponse<UpvoteModel>> getResult = await api.Get(filter =>
            {
                filter
                    .WithUserId(userId)
                    .WithLevelId(levelId);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogError("Failed to get upvote: {Result}", getResult.ToString());
            ThrowError("Failed to get upvote");
        }

        if (!getResult.Value.HasItems)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Result deleteResult = await api.Delete(getResult.Value.FirstItem!.Id, ct);

        if (deleteResult.IsFailed)
        {
            Logger.LogCritical("Failed to delete upvote: {Result}", deleteResult.ToString());
            ThrowError("Failed to delete upvote");
        }

        await SendOkAsync(ct);
    }
}
