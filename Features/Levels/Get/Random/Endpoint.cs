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
        List<Level> levels =
            await (from l in context.Levels.AsNoTracking()
                    orderby EF.Functions.Random()
                    select l)
                .Skip(req.Offset ?? 0)
                .Take(Max(req.Limit, 100, 1))
                .ToListAsync(ct);

        await SendOkAsync(new LevelsGetRandomResponseDTO()
            {
                Levels = levels.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }

    private static int Max(int? a, int b, int def)
    {
        return a == null ? def : Math.Max(a.Value, b);
    }
}
