using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsMonthlyRepository : IBasicRepository<WorldRecordMonthly>
{
}

public class WorldRecordsMonthlyRepository : BasicRepository<WorldRecordMonthly>, IWorldRecordsMonthlyRepository
{
    public WorldRecordsMonthlyRepository(IDatabase database, ILogger<WorldRecordsMonthlyRepository> logger)
        : base(database, logger)
    {
    }
}
