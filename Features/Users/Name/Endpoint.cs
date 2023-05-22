using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Name;

internal class Endpoint : Endpoint<UsersUpdateNameRequestDTO>
{
    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("users/name");
        Description(x => x.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersUpdateNameRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        User? user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null)
        {
            Logger.LogError("Unable to find user");
            ThrowError("Unable to find user");
        }

        user.SteamName = req.SteamName;
        await context.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}
