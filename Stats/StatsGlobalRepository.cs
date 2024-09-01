using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsGlobalRepository : IBasicRepository<StatsGlobal>
{
    void AddOrUpdate(int user, int level, Dictionary<string, decimal> data);
}

public class StatsGlobalRepository : StatsRepository<StatsGlobal>, IStatsGlobalRepository
{
    public StatsGlobalRepository(IDatabase database, ILogger<StatsGlobalRepository> logger)
        : base(database, logger)
    {
    }

    public void AddOrUpdate(int user, int level, Dictionary<string, decimal> data)
    {
        Upsert(
            daily => daily.IdUser == user && daily.IdLevel == level,
            () => new StatsGlobal()
            {
                IdUser = user,
                IdLevel = level,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });

        Upsert(
            daily => daily.IdUser == user,
            () => new StatsGlobal
            {
                IdUser = user,
                // Data = JsonConvert.SerializeObject(data)
            },
            daily =>
            {
                // daily.Data = UpdateData(daily.Data, data);
                return daily;
            });
    }
}
