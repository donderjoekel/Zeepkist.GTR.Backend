using System.Globalization;
using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsQuarterlyRepository : IBasicRepository<StatsQuarterly>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsQuarterlyRepository : StatsRepository<StatsQuarterly>, IStatsQuarterlyRepository
{
    public StatsQuarterlyRepository(IDatabase database, ILogger<StatsQuarterlyRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        DateTime now = DateTime.UtcNow;
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(now);
        int quarter = (CultureInfo.InvariantCulture.Calendar.GetMonth(now) - 1) / 3 + 1;

        Upsert(
            daily => daily.IdUser == user
                     && daily.IdLevel == level
                     && daily.Year == year
                     && daily.Quarter == quarter,
            () => new StatsQuarterly
            {
                IdUser = user,
                IdLevel = level,
                Quarter = quarter,
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
                     && daily.Quarter == quarter,
            () => new StatsQuarterly
            {
                IdUser = user,
                Quarter = quarter,
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
