using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetLeaderboard;

public class Endpoint : Endpoint<PersonalBestGetLeaderboardRequestDTO, PersonalBestGetLeaderboardResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("pbs/leaderboard");
    }

    public override async Task HandleAsync(PersonalBestGetLeaderboardRequestDTO req, CancellationToken ct)
    {
        List<PersonalBest> personalBests = await context.PersonalBests
            .AsNoTracking()
            .Include(x => x.RecordNavigation)
            .Include(x => x.UserNavigation)
            .Where(x => x.Level == req.Level && x.PeriodEnd == null && x.PeriodStart == null)
            .OrderBy(x => x.RecordNavigation!.Time)
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new PersonalBestGetLeaderboardResponseDTO()
            {
                Items = personalBests.Select(x => new PersonalBestGetLeaderboardResponseDTO.Item()
                {
                    Record = x.RecordNavigation!.ToResponseModel(),
                    User = x.UserNavigation!.ToResponseModel()
                }).ToList()
            },
            ct);
    }
}
