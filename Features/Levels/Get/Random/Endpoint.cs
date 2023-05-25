using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Random;

internal class Endpoint : Endpoint<GenericGetRequestDTO, LevelsGetRandomResponseDTO>
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
        Get("levels/random");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        var items = await context.Levels.AsNoTracking()
            .OrderBy(l => EF.Functions.Random())
            .GroupJoin(context.Records.AsNoTracking().Include(r => r.UserNavigation),
                l => l.Id,
                r => r.Level,
                (l, r) => new
                {
                    Level = l,
                    WorldRecord = r.FirstOrDefault(x => x.IsWr)
                })
            .Skip(req.Offset ?? 0)
            .Take(Max(req.Limit, 100, 1))
            .ToListAsync(ct);

        await SendOkAsync(new LevelsGetRandomResponseDTO()
            {
                Levels = items.Select(x => x.Level.ToResponseModel(x.WorldRecord)).ToList()
            },
            ct);
    }

    private static int Max(int? a, int b, int def)
    {
        return a == null ? def : Math.Min(a.Value, b);
    }
}
