using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class OldCalculateRankingJob : IJob
{
    private readonly GTRContext db;

    public OldCalculateRankingJob(GTRContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        List<User> users = await db.Users.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
        Dictionary<int, UserData> userToData = users.ToDictionary(x => x.Id, y => new UserData());

        List<Level> levels = await (from l in db.Levels.AsNoTracking()
            orderby l.Id descending
            select l).ToListAsync(ct);

        foreach (Level level in levels)
        {
            List<Record> records = await (from r in db.Records.AsNoTracking()
                where r.Level == level.Id && (r.IsBest || r.IsWr)
                orderby r.Time
                select r).ToListAsync(ct);

            int recordCount = records.Count;

            if (recordCount <= 1)
                continue;

            IEnumerable<float> times = records.Select(x => x.Time!.Value);
            double median = times.Median();

            for (int i = 0; i < recordCount; i++)
            {
                Record record = records[i];

                double points = Math.Max(0, 1000 * (1 - ((double)record.Time! - median) / median));
                int user = record.User!.Value;
                userToData[user].TotalPoints += points;
                userToData[user].CompletedLevels++;
                double positionScore = Math.Max(0, 100 - 50 * ((i + 1) - 1) / (recordCount - 1));
                userToData[user].PositionScore += positionScore;
            }
        }

        List<KeyValuePair<int, UserData>> sortedUsers = userToData.OrderByDescending(x => x.Value.TotalScore).ToList();

        List<User> trackedUsers = await db.Users.ToListAsync(ct);
        foreach (User user in trackedUsers)
        {
            if (!userToData.TryGetValue(user.Id, out UserData? data))
                continue;

            int index = sortedUsers.FindIndex(x => x.Key == user.Id);
            if (index == -1)
                continue;

            user.Score = double.IsNaN(data.TotalScore) ? 0 : (float)data.TotalScore;
            user.Position = index + 1;
        }

        await db.SaveChangesAsync(ct);
    }

    private class UserData
    {
        public int CompletedLevels { get; set; }
        public double TotalPoints { get; set; }
        public double PositionScore { get; set; }

        public double AveragePoints => TotalPoints / CompletedLevels;
        public double BonusScore => PositionScore / CompletedLevels;
        public double TotalScore => AveragePoints + BonusScore;
    }
}
