using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
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

        Upvote? upvote = await context.Upvotes.AsNoTracking()
            .Where(u => u.Id == req.Id && u.User == userId)
            .FirstOrDefaultAsync(ct);

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
