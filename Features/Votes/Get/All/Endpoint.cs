using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Get.All;

internal class Endpoint : Endpoint<VotesGetRequestDTO, VotesGetResponseDTO>
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
        Get("votes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VotesGetRequestDTO req, CancellationToken ct)
    {
        IQueryable<Vote> query = context.Votes.AsNoTracking()
            .Include(v => v.UserNavigation)
            .Include(v => v.LevelNavigation);

        if (req.UserId.HasValue)
            query = query.Where(x => x.User == req.UserId.Value);

        if (req.UserSteamId.HasValue())
            query = query.Where(x => x.UserNavigation.SteamId == req.UserSteamId);

        if (req.LevelId.HasValue)
            query = query.Where(x => x.Level == req.LevelId.Value);

        if (req.LevelUid.HasValue())
            query = query.Where(x => x.LevelNavigation.Uid == req.LevelUid);

        if (req.LevelWorkshopId.HasValue())
            query = query.Where(x => x.LevelNavigation.Wid == req.LevelWorkshopId);

        IOrderedQueryable<Vote> orderedQuery = query.OrderBy(x => x.Id);

        int totalAmount = await orderedQuery.CountAsync(ct);

        List<Vote> votes = await orderedQuery
            .Skip(req.Offset ?? 0)
            .Take(req.Limit ?? 100)
            .ToListAsync(ct);

        VotesGetResponseDTO responseModel = new VotesGetResponseDTO()
        {
            Votes = votes.Select(x => x.ToResponseModel()).ToList(),
            TotalAmount = totalAmount
        };

        await SendAsync(responseModel, cancellation: ct);
    }
}
