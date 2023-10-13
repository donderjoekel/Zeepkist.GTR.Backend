using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.WorldRecords.GetByLevel;

public class Endpoint : Endpoint<WorldRecordGetByLevelRequestDTO, WorldRecordGetByLevelResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/wrs/level/{Id}");
    }

    public override async Task HandleAsync(WorldRecordGetByLevelRequestDTO req, CancellationToken ct)
    {
        WorldRecord? worldRecord = await context.WorldRecords.AsNoTracking()
            .Where(x => x.Level == req.Level)
            .FirstOrDefaultAsync(ct);

        if (worldRecord != null)
        {
            await SendOkAsync(new WorldRecordGetByLevelResponseDTO()
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
