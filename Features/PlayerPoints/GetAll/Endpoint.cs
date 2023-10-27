using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PlayerPoints.GetAll;

public class Endpoint : Endpoint<GenericGetRequestDTO, PlayerPointsGetAllResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("playerpoints");
    }

    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        IOrderedQueryable<Database.Models.PlayerPoints> query = context
            .PlayerPoints
            .AsNoTracking()
            .OrderByDescending(x => x.Points);

        int count = await query.CountAsync(ct);

        List<Database.Models.PlayerPoints> items = await query
            .Take(req.Limit!.Value)
            .Skip(req.Offset!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new PlayerPointsGetAllResponseDTO()
            {
                TotalAmount = count,
                Items = items.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }
}
