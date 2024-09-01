using System.Globalization;
using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsYearlyRepository : IBasicRepository<StatsYearly>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsYearlyRepository : StatsRepository<StatsYearly>, IStatsYearlyRepository
{
    public StatsYearlyRepository(IDatabase database, ILogger<StatsYearlyRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        DateTime now = DateTime.UtcNow;
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(now);

        Upsert(
            daily => daily.IdUser == user
                     && daily.IdLevel == level
                     && daily.Year == year,
            () => new StatsYearly()
            {
                IdUser = user,
                IdLevel = level,
                Year = year,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });

        Upsert(
            daily => daily.IdUser == user && daily.Year == year,
            () => new StatsYearly
            {
                IdUser = user,
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
