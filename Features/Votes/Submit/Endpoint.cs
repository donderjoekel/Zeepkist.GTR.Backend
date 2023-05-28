using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Submit;

internal class Endpoint : Endpoint<VotesSubmitRequestDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
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

        if (await this.UserIsBanned(context))
        {
            Logger.LogWarning("Banned user tried to submit record");
            ThrowError("You are banned!");
            return;
        }

        int score = Math.Clamp(req.Score, 1, 5);

        Vote? vote = await context.Votes
            .Where(x => x.Level == req.Level && x.User == userId)
            .FirstOrDefaultAsync(ct);

        if (vote != null)
        {
            if (vote.Score != score)
            {
                vote.Score = score;
                await context.SaveChangesAsync(ct);
            }
        }
        else
        {
            vote = new Vote()
            {
                Level = req.Level,
                User = userId,
                Score = score
            };

            await context.Votes.AddAsync(vote, ct);
            await context.SaveChangesAsync(ct);
        }

        await SendOkAsync(ct);
    }
}
