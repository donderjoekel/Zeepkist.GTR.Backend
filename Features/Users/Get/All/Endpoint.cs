using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.All;

internal class Endpoint : Endpoint<GenericGetRequestDTO, UsersGetAllResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        IQueryable<User> query = context.Users.AsNoTracking().Include(x => x.StatsNavigation);
        int count = query.Count();
        List<User> users = await query
            .Skip(req.Offset ?? 0)
            .Take(req.Limit ?? 100)
            .ToListAsync(ct);

        UsersGetAllResponseDTO usersGetAllResponseDTO = new UsersGetAllResponseDTO()
        {
            TotalAmount = count,
            Users = users.Select(x => x.ToResponseModel()).ToList()
        };

        await SendAsync(usersGetAllResponseDTO, cancellation: ct);
    }
}
