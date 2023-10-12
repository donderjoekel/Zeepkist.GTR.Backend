using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetById;

public class Endpoint : Endpoint<GenericIdRequestDTO, PersonalBestGetByIdResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/pbs/{Id}");
    }

    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        PersonalBest? personalBest = await context.PersonalBests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (personalBest != null)
        {
            await SendOkAsync(new PersonalBestGetByIdResponseDTO() { PersonalBest = personalBest.ToResponseModel() },
                ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
