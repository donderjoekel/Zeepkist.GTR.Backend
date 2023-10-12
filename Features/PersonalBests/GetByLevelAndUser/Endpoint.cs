using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetByLevelAndUser;

public class Endpoint : Endpoint<PersonalBestGetByLevelAndUserRequestDTO, PersonalBestGetByLevelAndUserResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/pbs/level/{Level}/user/{User}");
    }

    public override async Task HandleAsync(PersonalBestGetByLevelAndUserRequestDTO req, CancellationToken ct)
    {
        PersonalBest? personalBest = await context.PersonalBests.AsNoTracking()
            .Where(x => x.Level == req.Level && x.User == req.User)
            .FirstOrDefaultAsync(ct);

        if (personalBest != null)
        {
            await SendOkAsync(new PersonalBestGetByLevelAndUserResponseDTO()
                {
                    PersonalBest = personalBest.ToResponseModel()
                },
                ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
