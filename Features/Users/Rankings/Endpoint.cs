using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rankings;

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
        List<UsersRankingsResponseDTO.Ranking> rankings = GetRankings();

        int limit = req.Limit ?? 50;
        int offset = req.Offset ?? 0;

        if (offset > rankings.Count)
        {
            await SendAsync(new UsersRankingsResponseDTO()
                {
                    TotalAmount = rankings.Count,
                    Rankings = new List<UsersRankingsResponseDTO.Ranking>()
                },
                cancellation: ct);

            return;
        }

        int max = limit + offset;
        if (max > rankings.Count)
        {
            limit -= max - rankings.Count;
        }

        List<UsersRankingsResponseDTO.Ranking> ranged = rankings.GetRange(offset, limit);

        UsersRankingsResponseDTO responseModel = new UsersRankingsResponseDTO()
        {
            Rankings = ranged,
            TotalAmount = rankings.Count
        };

        await SendOkAsync(responseModel, ct);
    }

    private List<UsersRankingsResponseDTO.Ranking> GetRankings()
    {
        if (cache.TryGetValue<List<UsersRankingsResponseDTO.Ranking>>("rankings",
                out List<UsersRankingsResponseDTO.Ranking>? cachedRankings))
        {
            return cachedRankings!;
        }

        var wrCounts = from r in context.Records
            join u in context.Users on r.User equals u.Id
            where r.IsWr
            group u by new { u.Id, u.SteamId, u.SteamName }
            into g
            orderby g.Count() descending
            select new
            {
                UserId = g.Key.Id,
                g.Key.SteamId,
                g.Key.SteamName,
                WrCount = g.Count()
            };

        List<UsersRankingsResponseDTO.Ranking> rankings = new();

        int i = 1;
        foreach (var wrCount in wrCounts)
        {
            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                Position = i,
                AmountOfWorldRecords = wrCount.WrCount,
                User = new UserResponseModel()
                {
                    Id = wrCount.UserId,
                    SteamName = wrCount.SteamName,
                    SteamId = wrCount.SteamId
                }
            });

            i++;
        }

        cache.Set("rankings", rankings, TimeSpan.FromMinutes(30));

        return rankings;
    }
}
