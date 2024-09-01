using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsDailyRepository : IBasicRepository<StatsDaily>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsDailyRepository : StatsRepository<StatsDaily>, IStatsDailyRepository
{
    public StatsDailyRepository(IDatabase database, ILogger<StatsDailyRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        DateTime now = DateTime.UtcNow;

        Upsert(
            daily => daily.IdUser == user
                     && daily.IdLevel == level
                     && daily.Year == now.Year
                     && daily.Day == now.DayOfYear,
            () => new StatsDaily
            {
                IdUser = user,
                IdLevel = level,
                Day = now.DayOfYear,
                Year = now.Year,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });

        Upsert(
            daily => daily.IdUser == user
                     && daily.Year == now.Year
                     && daily.Day == now.DayOfYear,
            () => new StatsDaily
            {
                IdUser = user,
                Day = now.DayOfYear,
                Year = now.Year,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });
    }
}
