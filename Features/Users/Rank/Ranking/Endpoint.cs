using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rank.Ranking;

internal class Endpoint : Endpoint<GenericIdRequestDTO, UsersRankingResponseDTO>
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
        Get("users/ranking/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (user == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(new UsersRankingResponseDTO()
                {
                    Position = user.Position ?? 0,
                    AmountOfWorldRecords = user.WorldRecords ?? 0,
                    Score = user.Score ?? 0f
                },
                ct);
        }
    }
}
