using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Equality;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class CalculateHotLevelsJob : IJob
{
    private readonly GTRContext db;
    private readonly IMemoryCache cache;

    public CalculateHotLevelsJob(GTRContext db, IMemoryCache cache)
    {
        this.db = db;
        this.cache = cache;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        Dictionary<Level, int> levelToCount = new(LevelEqualityComparer.Instance);

        DateTime stamp = DateTime.UtcNow.AddDays(-1);

        var query = from r in db.Records.AsNoTracking()
            join l in db.Levels.AsNoTracking() on r.Level equals l.Id
            where r.DateCreated >= stamp && r.IsBest
            orderby r.Level
            select new
            {
                Record = r,
                Level = l
            };

        var enumerable = query.AsAsyncEnumerable().WithCancellation(context.CancellationToken);

        await foreach (var item in enumerable)
        {
            levelToCount.TryAdd(item.Level, 0);
            levelToCount[item.Level]++;
        }

        List<LevelPopularityResponseModel> ordered = levelToCount
            .OrderByDescending(x => x.Value)
            .Select(x => new LevelPopularityResponseModel()
            {
                Level = x.Key.ToResponseModel(),
                RecordsCount = x.Value
            })
            .Take(25)
            .ToList();

        cache.Set("hot", ordered);
    }
}
