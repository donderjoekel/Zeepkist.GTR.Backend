using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Remove;

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
        Delete("favorites/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
        }

        Favorite? favorite = await context.Favorites.AsNoTracking()
            .Where(f => f.Id == req.Id && f.User == userId)
            .FirstOrDefaultAsync(ct);

        if (favorite == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        context.Favorites.Remove(favorite);
        await context.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}
