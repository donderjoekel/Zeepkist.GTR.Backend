using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Get.All;

internal class Endpoint : Endpoint<UpvotesGetRequestDTO, UpvotesGetResponseDTO>
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
        Get("upvotes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpvotesGetRequestDTO req, CancellationToken ct)
    {
        IQueryable<Upvote> query = context.Upvotes.AsNoTracking()
            .Include(f => f.UserNavigation);

        if (req.Level.HasValue())
        {
            query = query.Where(f => f.Level == req.Level);
        }

        if (req.UserId.HasValue)
        {
            query = query.Where(f => f.User == req.UserId.Value);
        }

        if (!string.IsNullOrEmpty(req.UserSteamId))
        {
            query = query.Where(f => f.UserNavigation.SteamId == req.UserSteamId);
        }

        IOrderedQueryable<Upvote> orderedQuery = query.OrderBy(f => f.Id);

        int count = orderedQuery.Count();

        List<Upvote> upvotes = await orderedQuery
            .Skip(req.Offset ?? 0)
            .Take(req.Limit ?? 100)
            .ToListAsync(ct);

        await SendOkAsync(new UpvotesGetResponseDTO()
            {
                Upvotes = upvotes.Select(x => x.ToResponseModel()).ToList(),
                TotalAmount = count
            },
            ct);
    }
}
