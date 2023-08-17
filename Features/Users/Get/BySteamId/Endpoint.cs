using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.BySteamId;

internal class Endpoint : Endpoint<UsersGetBySteamIdRequestDTO, UserResponseModel>
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
        Get("users/steam/{SteamId}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersGetBySteamIdRequestDTO req, CancellationToken ct)
    {
        User? user = await context.Users.AsNoTracking()
            .Include(x => x.StatsNavigation)
            .FirstOrDefaultAsync(x => x.SteamId == req.SteamId, ct);

        if (user != null)
            await SendOkAsync(user.ToResponseModel(), ct);
        else
            await SendNotFoundAsync(ct);
    }
}
