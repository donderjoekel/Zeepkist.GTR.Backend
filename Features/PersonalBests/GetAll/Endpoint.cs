using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetAll;

public class Endpoint : Endpoint<GenericGetRequestDTO, PersonalBestGetAllResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/pbs");
    }

    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        int count = await context.PersonalBests.AsNoTracking().CountAsync(ct);

        List<PersonalBest> items = await context.PersonalBests.AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        PersonalBestGetAllResponseDTO dto = new()
        {
            TotalAmount = count,
            Items = items.Select(x => x.ToResponseModel()).ToList()
        };

        await SendOkAsync(dto, ct);
    }
}
