using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsRepository : IBasicRepository<WorldRecordGlobal>
{
}

public class WorldRecordsRepository : BasicRepository<WorldRecordGlobal>, IWorldRecordsRepository
{
    public WorldRecordsRepository(IDatabase database, ILogger<WorldRecordsRepository> logger)
        : base(database, logger)
    {
    }
}
