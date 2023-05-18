using Microsoft.EntityFrameworkCore;
using Quartz;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class CalculateRankingJob : IJob
{
    private record LevelStats(int Level, int TimesBeaten, int UsersBeaten)
    {
        public int Points => TimesBeaten + UsersBeaten;
    }

    private record UserPoints(int User, int Points);

    private readonly GTRContext db;

    public CalculateRankingJob(GTRContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        List<LevelStats> levelStats = await CalculateLevelStats(context.CancellationToken);
        await AssignLevelStats(levelStats, context.CancellationToken);
        List<UserPoints> userPoints = await CalculateUserPoints(levelStats, context.CancellationToken);
        await AssignUserPoints(userPoints, context.CancellationToken);
    }

    private async Task<List<LevelStats>> CalculateLevelStats(CancellationToken ct)
    {
        List<LevelStats> stats = new();

        var items = await (from r in db.Records.AsNoTracking()
            orderby r.Id
            group r by r.Level
            into g
            orderby g.Key
            select new
            {
                Level = g.Key,
                Records = g.ToList()
            }).ToListAsync(ct);

        foreach (var item in items)
        {
            HashSet<int> users = new HashSet<int>();
            float fastestTime = float.MaxValue;
            int timesBeaten = 0;

            foreach (Record record in item.Records.OrderBy(x => x.DateCreated))
            {
                if (!users.Contains(record.User!.Value))
                    users.Add(record.User.Value);

                if (record.Time < fastestTime)
                {
                    fastestTime = record.Time.Value;
                    timesBeaten++;
                }
            }

            stats.Add(new(item.Level.Value, timesBeaten, users.Count));
        }

        return stats;
    }

    private async Task AssignLevelStats(List<LevelStats> levelStats, CancellationToken ct)
    {
        List<Level> levels = await (from l in db.Levels
            orderby l.Id
            select l).ToListAsync(ct);

        List<LevelStats> ordered = levelStats.OrderByDescending(x => x.Points).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            LevelStats levelStat = ordered[i];
            Level level = levels.First(x => x.Id == levelStat.Level);

            level.Rank = i + 1;
            level.Points = levelStat.Points;
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task<List<UserPoints>> CalculateUserPoints(List<LevelStats> levelStats, CancellationToken ct)
    {
        Dictionary<int, int> userToPoints = new();

        var items = await (from r in db.Records.AsNoTracking()
            where r.IsBest
            group r by r.Level
            into g
            orderby g.Key
            select new
            {
                Level = g.Key,
                Records = g.ToList()
            }).ToListAsync(ct);

        foreach (var item in items)
        {
            List<Record> records = item.Records.OrderBy(x => x.Time!.Value).ToList();
            for (int i = 0; i < records.Count; i++)
            {
                Record record = records[i];
                int rank = i + 1;
                CalculatePercentageYield(rank);
                LevelStats stats = levelStats.First(x => x.Level == record.Level);
                double percentage = CalculatePercentageYield(rank);
                int points = (int)Math.Floor(percentage * stats.Points / 100d);
                int user = record.User!.Value;
                userToPoints.TryAdd(user, 0);
                userToPoints[user] += points;
            }
        }

        return userToPoints
            .OrderByDescending(x => x.Value)
            .Select(x => new UserPoints(x.Key, x.Value))
            .ToList();
    }

    private static double CalculatePercentageYield(int position)
    {
        switch (position)
        {
            case 1:
                return 100;
            case >= 25:
                return 5;
            default:
            {
                double percentage = Math.Round(100 * Math.Exp(-0.15 * (position - 1)));
                return Math.Max(percentage, 5);
            }
        }
    }

    private async Task AssignUserPoints(List<UserPoints> userPoints, CancellationToken ct)
    {
        List<User> users = await (from u in db.Users
            orderby u.Id
            select u).ToListAsync(ct);

        for (int i = 0; i < userPoints.Count; i++)
        {
            UserPoints userPoint = userPoints[i];
            User user = users.First(x => x.Id == userPoint.User);
            user.Position = i + 1;
            user.Score = userPoint.Points;
        }

        await db.SaveChangesAsync(ct);
    }
}
