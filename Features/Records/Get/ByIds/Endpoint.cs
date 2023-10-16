using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.ByIds;

public class Endpoint : Endpoint<RecordsGetByIdsRequestDTO, RecordsGetByIdsResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("records/ids");
    }

    public override async Task HandleAsync(RecordsGetByIdsRequestDTO req, CancellationToken ct)
    {
        List<Record> records = await context.Records
            .AsNoTracking()
            .Where(x => req.Ids.Contains(x.Id))
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new RecordsGetByIdsResponseDTO()
            {
                Items = records.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }
}
