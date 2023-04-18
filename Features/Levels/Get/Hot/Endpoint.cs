using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Hot;

internal class Endpoint : EndpointWithoutRequest<LevelsGetHotResponseDTO>
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
        Get("levels/hot");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var list = await context.Records
            .Where(x => GTRContext.DateTrunc("day", x.DateCreated!.Value) == GTRContext.DateTrunc("day", DateTime.Now))
            .GroupBy(x => x.Level)
            .Select(x => new
            {
                LevelId = x.Key!.Value,
                LevelName = x.First().LevelNavigation!.Name!,
                RecordsCount = x.Count()
            })
            .OrderByDescending(x => x.RecordsCount)
            .ToListAsync(ct);

        List<LevelsGetHotResponseDTO.Info> infos = new();

        foreach (var item in list)
        {
            LevelsGetHotResponseDTO.Info info = new LevelsGetHotResponseDTO.Info()
            {
                RecordsCount = item.RecordsCount,
                LevelId = item.LevelId,
                LevelName = item.LevelName
            };
        }

        await SendOkAsync(new LevelsGetHotResponseDTO()
            {
                Levels = infos
            },
            ct);
    }
}
