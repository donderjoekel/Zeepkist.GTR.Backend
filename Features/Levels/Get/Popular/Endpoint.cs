using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Popular;

internal class Endpoint : EndpointWithoutRequest<LevelsGetPopularResponseDTO>
{
    private readonly GTRContext context;
    private readonly IMemoryCache cache;

    /// <inheritdoc />
    public Endpoint(GTRContext context, IMemoryCache cache)
    {
        this.context = context;
        this.cache = cache;
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
        if (cache.TryGetValue<List<LevelsGetPopularResponseDTO.Info>>("popular",
                out List<LevelsGetPopularResponseDTO.Info>? cachedInfos) && cachedInfos != null)
        {
            await SendOkAsync(new LevelsGetPopularResponseDTO()
                {
                    Levels = cachedInfos
                },
                ct);

            return;
        }

        Dictionary<LevelResponseModel, int> levelRecordsCount = new();

        const int minDay = 1;
        DateTime now = DateTime.Now;
        int maxDay = now.Day;

        for (int i = minDay; i < maxDay; i++)
        {
            DateTime date = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, i), DateTimeKind.Utc);
            List<LevelsGetPopularResponseDTO.Info> infosForDay = await GetPopularLevelsForDay(date, ct);
            foreach (LevelsGetPopularResponseDTO.Info info in infosForDay)
            {
                if (levelRecordsCount.ContainsKey(info.Level))
                    levelRecordsCount[info.Level] += info.RecordsCount;
                else
                    levelRecordsCount[info.Level] = info.RecordsCount;
            }
        }

        List<LevelsGetPopularResponseDTO.Info> infos = new();
        foreach (KeyValuePair<LevelResponseModel, int> pair in levelRecordsCount)
        {
            LevelsGetPopularResponseDTO.Info info = new()
            {
                Level = pair.Key,
                RecordsCount = pair.Value
            };
            infos.Add(info);
        }

        TimeSpan expiry = now.AddDays(1).Date - now;
        if (expiry > TimeSpan.FromHours(1))
            expiry = TimeSpan.FromHours(1);

        cache.Set("popular", infos, expiry);

        await SendOkAsync(new LevelsGetPopularResponseDTO()
            {
                Levels = infos.OrderByDescending(x => x.RecordsCount).Take(25).ToList()
            },
            ct);
    }

    private async Task<List<LevelsGetPopularResponseDTO.Info>> GetPopularLevelsForDay(
        DateTime date,
        CancellationToken ct
    )
    {
        List<LevelsGetPopularResponseDTO.Info> infos = new();

        var query = from r in context.Records
            join l in context.Levels on r.Level equals l.Id
            where r.DateCreated.Value.Date == date.Date
            group r by l
            into g
            orderby g.Count() descending
            select new
            {
                Level = g.Key,
                RecordsCount = g.Select(r => r.User).Distinct().Count()
            };

        var result = await query.Take(25).ToListAsync(cancellationToken: ct);

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

        return infos;
    }
}
