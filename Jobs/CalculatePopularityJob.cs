using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class CalculatePopularityJob : IJob
{
    private readonly IMemoryCache cache;
    private readonly GTRContext db;

    public CalculatePopularityJob(IMemoryCache cache, GTRContext db)
    {
        this.cache = cache;
        this.db = db;
    }

    /// <inheritdoc />
    public Task Execute(IJobExecutionContext context)
    {
        const int minDay = 1;
        DateTime now = DateTime.UtcNow;
        int maxDay = now.Day;

        Dictionary<int, int> levelIdToAmountOfRecordsToday = new Dictionary<int, int>();
        Dictionary<int, int> levelIdToAmountOfRecordsMonth = new Dictionary<int, int>();

        for (int i = minDay; i <= maxDay; i++)
        {
            DateTime date = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, i), DateTimeKind.Utc);
            Dictionary<int, HashSet<int>> levelIdToUserIds = GetRecordsForDay(date);

            if (date == DateTime.Today)
            {
                levelIdToAmountOfRecordsToday = levelIdToUserIds.ToDictionary(x => x.Key, y => y.Value.Count);
            }

            foreach (KeyValuePair<int, HashSet<int>> kvp in levelIdToUserIds)
            {
                levelIdToAmountOfRecordsMonth.TryAdd(kvp.Key, 0);
                levelIdToAmountOfRecordsMonth[kvp.Key] += kvp.Value.Count;
            }
        }

        List<KeyValuePair<int, int>> popularKVPs = levelIdToAmountOfRecordsMonth
            .OrderByDescending(x => x.Value)
            .Take(50)
            .ToList();

        List<KeyValuePair<int, int>> hotKVPs = levelIdToAmountOfRecordsToday
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();

        List<int> levelIds = popularKVPs.Select(x => x.Key)
            .Concat(hotKVPs.Select(x => x.Key))
            .Distinct()
            .ToList();

        List<Level> levelsByIds = GetLevelsByIds(levelIds);

        List<LevelPopularityResponseModel> popular = popularKVPs.Select(x =>
            new LevelPopularityResponseModel()
            {
                Level = MapToResponseModel(levelsByIds.FirstOrDefault(y => y.Id == x.Key))!,
                RecordsCount = x.Value
            }).ToList();

        List<LevelPopularityResponseModel> hot = hotKVPs.Select(x =>
            new LevelPopularityResponseModel()
            {
                Level = MapToResponseModel(levelsByIds.FirstOrDefault(y => y.Id == x.Key))!,
                RecordsCount = x.Value
            }).ToList();

        cache.Set("popular", popular);
        cache.Set("hot", hot);

        return Task.CompletedTask;
    }

    private static LevelResponseModel? MapToResponseModel(Level? level)
    {
        if (level == null)
            return null;

        return new LevelResponseModel()
        {
            Id = level.Id,
            Name = level.Name,
            Author = level.Author,
            IsValid = level.IsValid,
            ThumbnailUrl = level.ThumbnailUrl,
            TimeAuthor = level.TimeAuthor,
            TimeGold = level.TimeGold,
            TimeSilver = level.TimeSilver,
            TimeBronze = level.TimeBronze,
            UniqueId = level.Uid,
            WorkshopId = level.Wid,
        };
    }

    private Dictionary<int, HashSet<int>> GetRecordsForDay(DateTime day)
    {
        var queryable = from r in db.Records
            where r.DateCreated.Value.Date == day && r.IsValid
            orderby r.Id ascending
            select new
            {
                r.Id,
                r.Level,
                r.User
            };

        const int limit = 500;
        int page = 0;

        var batch = queryable.AsNoTracking().Skip(page * limit).Take(limit).AsEnumerable();
        Dictionary<int, HashSet<int>> levelIdToUserIds = new();

        while (batch.Any())
        {
            foreach (var record in batch)
            {
                if (!levelIdToUserIds.ContainsKey(record.Level!.Value))
                    levelIdToUserIds[record.Level!.Value] = new HashSet<int>();

                if (levelIdToUserIds[record.Level!.Value].Contains(record.User!.Value))
                    continue;

                levelIdToUserIds[record.Level!.Value].Add(record.User!.Value);
            }

            page++;
            batch = queryable.AsNoTracking().Skip(page * limit).Take(limit).AsEnumerable();
        }

        return levelIdToUserIds;
    }

    private List<Level> GetLevelsByIds(List<int> levelIds)
    {
        IQueryable<Level> queryable = from l in db.Levels
            where levelIds.Contains(l.Id)
            select l;

        const int limit = 500;
        int page = 0;

        IEnumerable<Level> batch = queryable.AsNoTracking().Skip(page * limit).Take(limit).AsEnumerable();
        HashSet<Level> levels = new(new Level.EqualityComparer());

        while (batch.Any())
        {
            foreach (Level level in batch)
            {
                levels.Add(level);
            }

            page++;
            batch = queryable.AsNoTracking().Skip(page * limit).Take(limit).AsEnumerable();
        }

        return levels.ToList();
    }
}
