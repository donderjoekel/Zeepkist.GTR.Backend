using System.Globalization;
using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsWeeklyRepository : IBasicRepository<StatsWeekly>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsWeeklyRepository : StatsRepository<StatsWeekly>, IStatsWeeklyRepository
{
    public StatsWeeklyRepository(IDatabase database, ILogger<StatsWeeklyRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        DateTime now = DateTime.UtcNow;
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(now);
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            now,
            CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

        Upsert(
            daily => daily.IdUser == user
                     && daily.IdLevel == level
                     && daily.Year == year
                     && daily.Week == week,
            () => new StatsWeekly
            {
                IdUser = user,
                IdLevel = level,
                Week = week,
                Year = year,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });

        Upsert(
            daily => daily.IdUser == user
                     && daily.Year == year
                     && daily.Week == week,
            () => new StatsWeekly
            {
                IdUser = user,
                Week = week,
                Year = year,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });
    }
}
