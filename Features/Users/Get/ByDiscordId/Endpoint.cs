using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
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
        User? user = await (from u in context.Users
            where u.DiscordId == req.DiscordId
            select u).FirstOrDefaultAsync(ct);

        if (user == null)
            await SendNotFoundAsync(ct);
        else
            await SendOkAsync(user!.ToResponseModel(), ct);
    }
}
