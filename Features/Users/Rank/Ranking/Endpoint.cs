using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rank.Ranking;

internal class Endpoint : Endpoint<GenericIdRequestDTO, UsersRankingResponseDTO>
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
        Get("users/ranking/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        Database.Models.PlayerPoints? playerPoints = await context.PlayerPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.User == req.Id, ct);

        if (playerPoints == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        int amountOfWorldRecords = await context.WorldRecords
            .AsNoTracking()
            .CountAsync(x => x.User == req.Id, ct);

        await SendOkAsync(new UsersRankingResponseDTO()
            {
                Position = playerPoints.Rank,
                AmountOfWorldRecords = amountOfWorldRecords,
                Score = playerPoints.Points
            },
            ct);
    }
}
