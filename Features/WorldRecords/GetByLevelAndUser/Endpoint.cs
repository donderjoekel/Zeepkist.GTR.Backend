using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.WorldRecords.GetByLevelAndUser;

public class Endpoint : Endpoint<WorldRecordGetByLevelAndUserRequestDTO, WorldRecordGetByLevelAndUserResponseDTO>
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

    public override async Task HandleAsync(WorldRecordGetByLevelAndUserRequestDTO req, CancellationToken ct)
    {
        WorldRecord? worldRecord = await context.WorldRecords.AsNoTracking()
            .Where(x => x.Level == req.Level && x.User == req.User)
            .FirstOrDefaultAsync(ct);

        if (worldRecord != null)
        {
            await SendOkAsync(new WorldRecordGetByLevelAndUserResponseDTO()
                {
                    WorldRecord = worldRecord.ToResponseModel()
                },
                ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
