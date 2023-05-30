using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Get.All;

internal class Endpoint : Endpoint<FavoritesGetAllRequestDTO, FavoritesGetAllResponseDTO>
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
        AllowAnonymous();
        Get("favorites");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(FavoritesGetAllRequestDTO req, CancellationToken ct)
    {
        IQueryable<Favorite> query = context.Favorites.AsNoTracking()
            .Include(f => f.LevelNavigation)
            .Include(f => f.UserNavigation);

        if (req.LevelId.HasValue)
        {
            query = query.Where(f => f.Level == req.LevelId.Value);
        }

        if (!string.IsNullOrEmpty(req.LevelUid))
        {
            query = query.Where(f => f.LevelNavigation.Uid == req.LevelUid);
        }

        if (!string.IsNullOrEmpty(req.LevelWorkshopId))
        {
            query = query.Where(f => f.LevelNavigation.Wid == req.LevelWorkshopId);
        }

        if (req.UserId.HasValue)
        {
            query = query.Where(f => f.User == req.UserId.Value);
        }

        if (!string.IsNullOrEmpty(req.UserSteamId))
        {
            query = query.Where(f => f.UserNavigation.SteamId == req.UserSteamId);
        }

        IOrderedQueryable<Favorite> orderedQuery = query.OrderBy(f => f.Id);

        int count = orderedQuery.Count();

        List<Favorite> favorites = await orderedQuery
            .Skip(req.Offset ?? 0)
            .Take(req.Limit ?? 100)
            .ToListAsync(ct);

        await SendOkAsync(new FavoritesGetAllResponseDTO()
            {
                Favorites = favorites.Select(x => x.ToResponseModel()).ToList(),
                TotalAmount = count
            },
            ct);
    }
}
