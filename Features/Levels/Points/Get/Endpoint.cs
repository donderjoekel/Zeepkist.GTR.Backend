using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Points.Get;

public class Endpoint : Endpoint<GenericGetRequestDTO, LevelsGetPointsResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/points");
    }

    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        List<LevelPoints> levelPoints = await context.LevelPoints
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new LevelsGetPointsResponseDTO()
            {
                Items = levelPoints.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }
}
