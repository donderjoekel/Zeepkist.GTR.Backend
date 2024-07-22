using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsDailyRepository : IBasicRepository<WorldRecordDaily>
{
}

public class WorldRecordsDailyRepository : BasicRepository<WorldRecordDaily>, IWorldRecordsDailyRepository
{
    public WorldRecordsDailyRepository(IDatabase database, ILogger<WorldRecordsDailyRepository> logger)
        : base(database, logger)
    {
    }
}
