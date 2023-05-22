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

    public Endpoint(GTRContext context)
    {
        this.context = context;
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

        List<User> users = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Position)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        List<UsersRankingsResponseDTO.Ranking> rankings = new();

        foreach (User user in users)
        {
            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                User = new UserResponseModel()
                {
                    Id = user.Id,
                    SteamId = user.SteamId,
                    SteamName = user.SteamName
                },
                Position = user.Position ?? 0,
                Score = user.Score ?? 0f,
                AmountOfWorldRecords = user.WorldRecords ?? 0
            });
        }

        await SendOkAsync(new UsersRankingsResponseDTO()
            {
                Rankings = rankings,
                TotalAmount = await context.Users.AsNoTracking().CountAsync(ct)
            },
            ct);
    }
}
