using System.Data.Common;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Popular;

internal class Endpoint : EndpointWithoutRequest<LevelsGetPopularResponseDTO>
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
        AllowAnonymous();
        Get("levels/popular");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        // TODO: Should definitely add caching here

        List<LevelsGetPopularResponseDTO.Info> infos = new();

        var query = from r in context.Records
            join l in context.Levels on r.Level equals l.Id
            where r.DateCreated?.Month == DateTime.Today.Month
            group r by l
            into g
            orderby g.Count() descending
            select new
            {
                Level = g.Key,
                RecordsCount = g.Select(r => r.User).Distinct().Count()
            };

        var result = await query.Take(10).ToListAsync(cancellationToken: ct);

        foreach (var x1 in result)
        {
            LevelsGetPopularResponseDTO.Info info = new()
            {
                Level = new LevelResponseModel()
                {
                    Id = x1.Level.Id,
                    Name = x1.Level.Name,
                    Author = x1.Level.Author,
                    IsValid = x1.Level.IsValid,
                    ThumbnailUrl = x1.Level.ThumbnailUrl,
                    TimeAuthor = x1.Level.TimeAuthor,
                    TimeGold = x1.Level.TimeGold,
                    TimeSilver = x1.Level.TimeSilver,
                    TimeBronze = x1.Level.TimeBronze,
                    UniqueId = x1.Level.Uid,
                    WorkshopId = x1.Level.Wid,
                },
                RecordsCount = x1.RecordsCount
            };

            infos.Add(info);
        }

        await SendOkAsync(new LevelsGetPopularResponseDTO()
            {
                Levels = infos
            },
            ct);
    }
}
