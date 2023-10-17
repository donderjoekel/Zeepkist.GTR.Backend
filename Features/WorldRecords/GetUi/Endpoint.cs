using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.WorldRecords.GetUi;

public class Endpoint : Endpoint<WorldRecordsGetUiRequestDTO, WorldRecordGetUiResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("wrs/ui");
    }

    public override async Task HandleAsync(WorldRecordsGetUiRequestDTO req, CancellationToken ct)
    {
        WorldRecord? worldRecord = await context.WorldRecords
            .AsNoTracking()
            .Include(x => x.RecordNavigation)
            .Include(x => x.UserNavigation)
            .Where(x => x.Level == req.Level && x.PeriodStart == null && x.PeriodEnd == null)
            .FirstOrDefaultAsync(ct);

        if (worldRecord?.UserNavigation == null || worldRecord?.RecordNavigation == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new WorldRecordGetUiResponseDTO
            {
                Record = worldRecord.RecordNavigation.ToResponseModel(),
                User = worldRecord.UserNavigation.ToResponseModel()
            },
            ct);
    }
}
