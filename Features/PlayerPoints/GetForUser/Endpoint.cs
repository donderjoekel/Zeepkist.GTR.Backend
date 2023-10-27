using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.PlayerPoints.GetForUser;

public class Endpoint : Endpoint<GenericIdRequestDTO, PlayerPointsResponseModel>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("playerpoints/{Id}");
    }

    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        Database.Models.PlayerPoints? playerPoints = await context
            .PlayerPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.User == req.Id, ct);

        if (playerPoints == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(playerPoints.ToResponseModel(), ct);
        }
    }
}
