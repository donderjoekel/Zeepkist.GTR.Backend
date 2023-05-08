using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rank.Rankings;

internal class Endpoint : Endpoint<GenericGetRequestDTO, UsersRankingsResponseDTO>
{
    private readonly GTRContext context;
    private readonly IMemoryCache cache;

    public Endpoint(GTRContext context, IMemoryCache cache)
    {
        this.context = context;
        this.cache = cache;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users/rankings");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        int limit = req.Limit ?? 50;
        int offset = req.Offset ?? 0;

        List<User> users = await (
                from u in context.Users
                orderby u.Position
                select u)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        List<UsersRankingsResponseDTO.Ranking> rankings = new();

        foreach (User user in users)
        {
            string key = $"user_{user.Id}_wr_count";

            if (!cache.TryGetValue(key, out int wrCount))
            {
                wrCount = await (from r in context.Records
                    where r.User == user.Id && r.IsWr
                    orderby r.Id
                    select r).CountAsync(ct);

                cache.Set(key, wrCount, TimeSpan.FromMinutes(15));
            }

            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                User = new UserResponseModel()
                {
                    Id = user.Id,
                    SteamId = user.SteamId,
                    SteamName = user.SteamName
                },
                Position = user.Position ?? -1,
                Score = user.Score ?? 0f,
                AmountOfWorldRecords = wrCount
            });
        }

        await SendOkAsync(new UsersRankingsResponseDTO()
            {
                Rankings = rankings,
                TotalAmount = await context.Users.CountAsync(ct)
            },
            ct);
    }
}
