using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.ByIds;

public class Endpoint : Endpoint<UsersGetByIdsRequestDTO, UsersGetByIdsResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("users/ids");
    }

    public override async Task HandleAsync(UsersGetByIdsRequestDTO req, CancellationToken ct)
    {
        List<User> result = await context.Users
            .AsNoTracking()
            .Where(x => req.Ids.Contains(x.Id))
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        await SendOkAsync(new UsersGetByIdsResponseDTO()
            {
                Items = result.Select(x => x.ToResponseModel()).ToList()
            },
            ct);
    }
}
