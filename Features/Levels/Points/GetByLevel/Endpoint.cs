using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Points.GetByLevel;

public class Endpoint : Endpoint<LevelGetPointsByLevelRequestDTO, LevelsGetPointsByLevelResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/points/{Level}");
    }

    public override async Task HandleAsync(LevelGetPointsByLevelRequestDTO req, CancellationToken ct)
    {
        LevelPoints? levelPoints = await context.LevelPoints
            .AsNoTracking()
            .Where(x => x.Level == req.Level)
            .FirstOrDefaultAsync(ct);

        if (levelPoints == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(new LevelsGetPointsByLevelResponseDTO()
                {
                    LevelPoints = levelPoints.ToResponseModel()
                },
                ct);
        }
    }
}
