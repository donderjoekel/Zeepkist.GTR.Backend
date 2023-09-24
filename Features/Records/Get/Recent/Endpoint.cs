using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.Recent;

internal class Endpoint : Endpoint<GenericGetRequestDTO, RecordsGetRecentResponseDTO>
{
    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("records/recent");
        Description(b => { b.Produces<RecordsGetRecentResponseDTO>(); });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        int limit = req.Limit ?? 100;
        int offset = req.Offset ?? 0;

        List<Record> records = await context.Records.AsNoTracking()
            .OrderByDescending(x => x.DateCreated)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken: ct);

        await SendOkAsync(new RecordsGetRecentResponseDTO()
            {
                Records = records.Select(x => x.ToResponseModel()).ToList(),
            },
            ct);
    }
}
