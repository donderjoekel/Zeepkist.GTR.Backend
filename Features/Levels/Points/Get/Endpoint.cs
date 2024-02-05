using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Points.Get;

public class Endpoint : Endpoint<LevelGetPointsRequestDTO, LevelsGetPointsResponseDTO>
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

    public override async Task HandleAsync(LevelGetPointsRequestDTO req, CancellationToken ct)
    {
        IQueryable<LevelPoints> query = context.LevelPoints
            .AsNoTracking();

        if (req.SortByPoints.HasValue)
        {
            query = req.Ascending.HasValue
                ? req.Ascending.Value
                    ? query.OrderBy(x => x.Points)
                    : query.OrderByDescending(x => x.Points)
                : query.OrderByDescending(x => x.Points);
        }
        else
        {
            query = req.Ascending.HasValue
                ? req.Ascending.Value
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id)
                : query.OrderByDescending(x => x.Id);
        }

        List<LevelPoints> levelPoints = await query
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
