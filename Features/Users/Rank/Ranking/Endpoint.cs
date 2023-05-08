using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
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
        int amountOfWorldRecords = await (from r in context.Records
            where r.User == req.Id && r.IsWr
            orderby r.Id
            select r).CountAsync(ct);

        User? user = await (from u in context.Users
            where u.Id == req.Id
            select u).FirstOrDefaultAsync(ct);

        if (user == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendOkAsync(new UsersRankingResponseDTO()
                {
                    Position = user.Position ?? -1,
                    AmountOfWorldRecords = amountOfWorldRecords,
                    Score = user.Score ?? 0f
                },
                ct);
        }
    }
}
