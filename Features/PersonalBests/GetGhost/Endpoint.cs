using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetGhost;

public class Endpoint : Endpoint<PersonalBestGetGhostRequestDTO, PersonalBestGetGhostResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("pbs/ghost");
    }

    public override async Task HandleAsync(PersonalBestGetGhostRequestDTO req, CancellationToken ct)
    {
        PersonalBest? personalBest = await context.PersonalBests
            .AsNoTracking()
            .Include(x => x.RecordNavigation)
            .ThenInclude(y => y.Media)
            .Include(x => x.UserNavigation)
            .Where(x => x.Level == req.Level && x.User == req.User && x.PeriodStart == null && x.PeriodEnd == null)
            .FirstOrDefaultAsync(ct);

        if (personalBest?.UserNavigation == null || personalBest.RecordNavigation?.Media is not { Count: 1 })
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new PersonalBestGetGhostResponseDTO
            {
                Record = personalBest.RecordNavigation.ToResponseModel(),
                User = personalBest.UserNavigation.ToResponseModel(),
                Media = personalBest.RecordNavigation.Media.First().ToResponseModel()
            },
            ct);
    }
}
