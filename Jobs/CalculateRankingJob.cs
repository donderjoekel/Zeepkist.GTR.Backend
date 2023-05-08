using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Quartz;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class CalculateRankingJob : IJob
{
    private readonly GTRContext db;

    public CalculateRankingJob(GTRContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        List<User> users = await db.Users.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
        Dictionary<int, double> userToScore = users.ToDictionary(x => x.Id, y => 0.0);
        int totalUsers = users.Count;

        List<Level> levels = await db.Levels.AsNoTracking().ToListAsync(cancellationToken: ct);
        foreach (Level level in levels)
        {
            IOrderedQueryable<Record> records = from r in db.Records.AsNoTracking()
                where r.Level == level.Id && (r.IsBest || r.IsWr)
                orderby r.Time
                select r;

            if (await records.CountAsync(ct) <= 1)
                continue;

            List<Record> rs = await records.ToListAsync(ct);
            int usersOnLevel = rs.Count;

            for (int i = 0; i < rs.Count; i++)
            {
                Record record = rs[i];

                int rank = i + 1;
                double score = (1 - (rank - 1.0) / (totalUsers - 1)) * (1 + Math.Log2(usersOnLevel));
                userToScore[record.User!.Value] += score;
            }
        }

        List<KeyValuePair<int, double>> orderedScoring = userToScore
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .ToList();

        foreach (KeyValuePair<int, double> kvp in orderedScoring)
        {
            int userId = kvp.Key;
            User user = await db.Users.SingleAsync(x => x.Id == userId, ct);
            user.Position = orderedScoring.IndexOf(kvp) + 1;
            user.Score = (float)kvp.Value;
        }
        
        await db.SaveChangesAsync(ct);
    }
}
