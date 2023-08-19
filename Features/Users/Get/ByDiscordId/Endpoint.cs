using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.ByDiscordId;

internal class Endpoint : Endpoint<UsersGetByDiscordIdRequestDTO, UserResponseModel>
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
        Get("users/discord/{DiscordId}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersGetByDiscordIdRequestDTO req, CancellationToken ct)
    {
        User? user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.DiscordId == req.DiscordId, ct);

        if (user == null)
            await SendNotFoundAsync(ct);
        else
            await SendOkAsync(user!.ToResponseModel(), ct);
    }
}
