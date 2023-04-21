using System.Data.Common;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Hot;

internal class Endpoint : EndpointWithoutRequest<LevelsGetHotResponseDTO>
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
        Get("levels/hot");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        List<LevelsGetHotResponseDTO.Info> infos = new();

        var query = from r in context.Records
            join l in context.Levels on r.Level equals l.Id
            where r.DateCreated.Value.Date == DateTime.Today && r.IsValid == true
            group r by l
            into g
            orderby g.Count() descending
            select new
            {
                Level = g.Key,
                Records = g.Select(r => r)
            };

        var results = await query.ToListAsync(ct);
        foreach(var result in results)
        {
            HashSet<int> userIds = new HashSet<int>();

            foreach(var record in result.Records)
            {
                if (userIds.Contains(record.User!.Value))
                    continue;

                userIds.Add(record.User!.Value);
            }

            infos.Add(new LevelsGetHotResponseDTO.Info()
            {
                Level = new LevelResponseModel()
                {
                    Id = result.Level.Id,
                    Name = result.Level.Name,
                    Author = result.Level.Author,
                    IsValid = result.Level.IsValid,
                    ThumbnailUrl = result.Level.ThumbnailUrl,
                    TimeAuthor = result.Level.TimeAuthor,
                    TimeGold = result.Level.TimeGold,
                    TimeSilver = result.Level.TimeSilver,
                    TimeBronze = result.Level.TimeBronze,
                    UniqueId = result.Level.Uid,
                    WorkshopId = result.Level.Wid,
                },
                RecordsCount = userIds.Count
            });
        }

        await SendOkAsync(new LevelsGetHotResponseDTO()
            {
                Levels = infos.Take(10).OrderByDescending(x => x.RecordsCount).ToList()
            },
            ct);
    }
}
