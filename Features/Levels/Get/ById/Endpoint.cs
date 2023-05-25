using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, LevelResponseModel>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        var item = await context.Levels.AsNoTracking()
            .GroupJoin(context.Records.AsNoTracking().Include(r => r.UserNavigation),
                l => l.Id,
                r => r.Level,
                (l, r) => new
                {
                    Level = l,
                    WorldRecord = r.FirstOrDefault(x => x.IsWr)
                })
            .FirstOrDefaultAsync(x => x.Level.Id == req.Id, ct);

        if (item != null)
            await SendOkAsync(item.Level.ToResponseModel(item.WorldRecord), ct);
        else
            await SendNotFoundAsync(ct);
    }
}
