using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Add;

internal class Endpoint : Endpoint<FavoritesAddRequestDTO, GenericIdResponseDTO>
{
    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("favorites");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(FavoritesAddRequestDTO req, CancellationToken ct)
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

        Favorite? favorite = await (from f in context.Favorites.AsNoTracking()
            where f.User == userId && f.Level == req.LevelId
            select f).FirstOrDefaultAsync(ct);

        if (favorite != null)
        {
            await SendOkAsync(new GenericIdResponseDTO(favorite.Id), ct);
            return;
        }

        EntityEntry<Favorite> entity = await context.Favorites.AddAsync(new Favorite()
            {
                User = userId,
                Level = req.LevelId,
                DateCreated = DateTime.UtcNow
            },
            ct);

        await context.SaveChangesAsync(ct);
        await SendOkAsync(new GenericIdResponseDTO(entity.Entity.Id), ct);
    }
}
