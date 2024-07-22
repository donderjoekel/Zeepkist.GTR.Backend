using System.Globalization;
using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsMonthlyRepository : IBasicRepository<StatsMonthly>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsMonthlyRepository : StatsRepository<StatsMonthly>, IStatsMonthlyRepository
{
    public StatsMonthlyRepository(IDatabase database, ILogger<StatsMonthlyRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        DateTime now = DateTime.UtcNow;
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(now);
        int month = CultureInfo.InvariantCulture.Calendar.GetMonth(now);

        Upsert(
            daily => daily.IdUser == user
                     && daily.IdLevel == level
                     && daily.Year == year
                     && daily.Month == month,
            () => new StatsMonthly
            {
                IdUser = user,
                IdLevel = level,
                Month = month,
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
                     && daily.Month == month,
            () => new StatsMonthly
            {
                IdUser = user,
                Month = month,
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
