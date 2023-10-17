using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.WorldRecords.GetGhost;

public class Endpoint : Endpoint<WorldRecordGetGhostRequestDTO, WorldRecordGetGhostResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("wrs/ghost");
    }

    public override async Task HandleAsync(WorldRecordGetGhostRequestDTO req, CancellationToken ct)
    {
        WorldRecord? worldRecord = await context.WorldRecords
            .AsNoTracking()
            .Include(x => x.RecordNavigation)
            .ThenInclude(y => y.Media)
            .Include(x => x.UserNavigation)
            .Where(x => x.Level == req.Level && x.PeriodStart == null && x.PeriodEnd == null)
            .FirstOrDefaultAsync(ct);

        if (worldRecord?.UserNavigation == null || worldRecord.RecordNavigation?.Media is not { Count: 1 })
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new WorldRecordGetGhostResponseDTO
            {
                Record = worldRecord.RecordNavigation.ToResponseModel(),
                User = worldRecord.UserNavigation.ToResponseModel(),
                Media = worldRecord.RecordNavigation.Media.First().ToResponseModel()
            },
            ct);
    }
}
