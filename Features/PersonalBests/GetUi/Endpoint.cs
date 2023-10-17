using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetUi;

public class Endpoint : Endpoint<PersonalBestGetUiRequestDTO, PersonalBestGetUiResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("pbs/ui");
    }

    public override async Task HandleAsync(PersonalBestGetUiRequestDTO req, CancellationToken ct)
    {
        PersonalBest? personalBest = await context.PersonalBests
            .AsNoTracking()
            .Include(x => x.RecordNavigation)
            .Include(x => x.UserNavigation)
            .Where(x => x.Level == req.Level && x.User == req.User && x.PeriodStart == null && x.PeriodEnd == null)
            .FirstOrDefaultAsync(ct);

        if (personalBest?.UserNavigation == null || personalBest?.RecordNavigation == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new PersonalBestGetUiResponseDTO
            {
                Record = personalBest.RecordNavigation.ToResponseModel(),
                User = personalBest.UserNavigation.ToResponseModel()
            },
            ct);
    }
}
