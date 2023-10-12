using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.PersonalBests.GetByUser;

public class Endpoint : Endpoint<GenericIdWithLimitOffsetRequestDTO, PersonalBestGetByUserResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("/pbs/user/{Id}");
    }

    public override async Task HandleAsync(GenericIdWithLimitOffsetRequestDTO req, CancellationToken ct)
    {
        int count = await context.PersonalBests.AsNoTracking()
            .Where(x => x.User == x.Id)
            .CountAsync(ct);

        List<PersonalBest> items = await context.PersonalBests.AsNoTracking()
            .Where(x => x.User == x.Id)
            .OrderBy(x => x.Id)
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new PersonalBestGetByUserResponseDTO()
            {
                TotalAmount = count,
                Items = items.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }
}
