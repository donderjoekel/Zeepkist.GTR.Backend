using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Remove;

internal class Endpoint : Endpoint<GenericIdRequestDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Delete("upvotes/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        Upvote? upvote = await (from u in context.Upvotes.AsNoTracking()
            where u.Id == req.Id && u.User == userId
            select u).FirstOrDefaultAsync(ct);

        if (upvote == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        context.Upvotes.Remove(upvote);
        await context.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}
