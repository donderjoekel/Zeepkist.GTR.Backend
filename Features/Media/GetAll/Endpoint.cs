using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Media.GetAll;

public class Endpoint : Endpoint<GenericGetRequestDTO, MediaGetAllResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("media");
    }

    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        int count = await context.Media.AsNoTracking().CountAsync(ct);
        List<Database.Models.Media> items = await context.Media.AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        MediaGetAllResponseDTO responseDTO = new()
        {
            TotalAmount = count,
            Items = items.Select(x => x.ToResponseModel()).ToList()
        };

        await SendAsync(responseDTO, cancellation: ct);
    }
}
