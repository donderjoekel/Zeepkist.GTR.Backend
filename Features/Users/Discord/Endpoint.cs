using System.Security.Claims;
using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Discord;

internal class Endpoint : Endpoint<UsersUpdateDiscordIdRequestDTO>
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
        Post("users/discord");
        Description(x => x.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersUpdateDiscordIdRequestDTO req, CancellationToken ct)
    {
        Claim? userClaim = User.FindFirst("UserId");
        if (userClaim == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        string userId = userClaim.Value.Split('_')[0];
        if (!int.TryParse(userId, out int id))
        {
            Logger.LogCritical("Unable to parse user id: {UserId}", userId);
            await SendUnauthorizedAsync(ct);
            return;
        }

        User? user = await context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (user == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            user.DiscordId = req.DiscordId;
            await context.SaveChangesAsync(ct);
            await SendOkAsync(ct);
        }
    }
}
