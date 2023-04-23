using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

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
        List<UsersRankingsResponseDTO.Ranking> rankings = RankUtilities.GetRankings(cache, context);

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
}
