using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.WorldRecords.GetById;

public class Endpoint : Endpoint<GenericIdRequestDTO, WorldRecordGetByIdResponseDTO>
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
        WorldRecord? worldRecord = await context.WorldRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (worldRecord != null)
        {
            await SendOkAsync(new WorldRecordGetByIdResponseDTO() { WorldRecord = worldRecord.ToResponseModel() },
                ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
