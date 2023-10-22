using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Popular;

public class Endpoint : EndpointWithoutRequest<LevelsGetPopularResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/levels/popular");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await context.PersonalBests.AsNoTracking()
            .Where(x => x.DateUpdated > DateTime.UtcNow.AddDays(-30))
            .GroupBy(x => x.Level)
            .Select(x => new
            {
                Level = x.Key,
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(100)
            .ToListAsync(ct);

        LevelsGetPopularResponseDTO response = new()
        {
            Levels = items.Select(x => new LevelPopularityResponseModel
            {
                Level = x.Level,
                RecordsCount = x.Count
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}
