using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsGlobalRepository : IBasicRepository<WorldRecordGlobal>
{
}

public class WorldRecordsGlobalRepository : BasicRepository<WorldRecordGlobal>, IWorldRecordsGlobalRepository
{
    public WorldRecordsGlobalRepository(IDatabase database, ILogger<WorldRecordsGlobalRepository> logger)
        : base(database, logger)
    {
    }
}
