using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

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
        IOrderedQueryable<Database.Models.PlayerPoints> query = context.PlayerPoints
            .AsNoTracking()
            .OrderByDescending(x => x.Points);

        int count = await query.CountAsync(ct);

        List<Database.Models.PlayerPoints> playerPoints = await query
            .Include(x => x.UserNavigation)
            .Take(req.Limit!.Value)
            .Skip(req.Offset!.Value)
            .ToListAsync(ct);

        List<IGrouping<int, WorldRecord>> worldRecordsPerUser = await context.WorldRecords
            .AsNoTracking()
            .GroupBy(x => x.User)
            .ToListAsync(ct);

        List<UsersRankingsResponseDTO.Ranking> rankings = new();

        foreach (Database.Models.PlayerPoints playerPoint in playerPoints)
        {
            int amountOfWorldRecords = 0;
            IGrouping<int, WorldRecord>? grouping = worldRecordsPerUser.FirstOrDefault(x => x.Key == playerPoint.User);
            if (grouping != null)
                amountOfWorldRecords = grouping.Count();

            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                Position = playerPoint.Rank,
                Score = playerPoint.Points,
                AmountOfWorldRecords = amountOfWorldRecords,
                User = playerPoint.UserNavigation!.ToResponseModel()
            });
        }

        await SendOkAsync(new UsersRankingsResponseDTO()
            {
                Rankings = rankings,
                TotalAmount = count
            },
            ct);

        // foreach (Database.Models.PlayerPoints playerPoint in playerPoints)
        // {
        //     rankings.Add(new UsersRankingsResponseDTO.Ranking()
        //     {
        //         Position = playerPoint.Rank,
        //         Score = playerPoint.Points,
        //         AmountOfWorldRecords =
        //             User = 
        //     });
        // }
        //
        // foreach (User user in users)
        // {
        //     rankings.Add(new UsersRankingsResponseDTO.Ranking()
        //     {
        //         User = new UserResponseModel()
        //         {
        //             Id = user.Id,
        //             SteamId = user.SteamId,
        //             SteamName = user.SteamName
        //         },
        //         Position = user.Position ?? 0,
        //         Score = user.Score ?? 0f,
        //         AmountOfWorldRecords = user.WorldRecords ?? 0
        //     });
        // }
        //
        // await SendOkAsync(new UsersRankingsResponseDTO()
        //     {
        //         Rankings = rankings,
        //         TotalAmount = await context.Users.AsNoTracking().CountAsync(ct)
        //     },
        //     ct);
    }
}
