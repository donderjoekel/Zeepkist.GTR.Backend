using FastEndpoints;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rank.Ranking;

internal class Endpoint : Endpoint<UsersRankingGetRequestDTO, UsersRankingResponseDTO>
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
        Get("users/ranking");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersRankingGetRequestDTO req, CancellationToken ct)
    {
        if (!req.UserId.HasValue && string.IsNullOrEmpty(req.SteamId))
        {
            ThrowError("Request needs either a SteamId or UserId");
        }

        List<UsersRankingsResponseDTO.Ranking> rankings = RankUtilities.GetRankings(cache, context);

        UsersRankingsResponseDTO.Ranking? ranking;

        if (req.UserId.HasValue)
        {
            ranking = rankings.FirstOrDefault(x => x.User.Id == req.UserId);
        }
        else
        {
            ranking = rankings.FirstOrDefault(x =>
                string.Equals(x.User.SteamId, req.SteamId, StringComparison.OrdinalIgnoreCase));
        }

        if (ranking == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(new UsersRankingResponseDTO()
                {
                    Position = ranking.Position,
                    AmountOfWorldRecords = ranking.AmountOfWorldRecords
                },
                ct);
        }
    }
}
