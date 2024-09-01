using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsWeeklyRepository : IBasicRepository<WorldRecordWeekly>
{
}

public class WorldRecordsWeeklyRepository : BasicRepository<WorldRecordWeekly>, IWorldRecordsWeeklyRepository
{
    public WorldRecordsWeeklyRepository(IDatabase database, ILogger<WorldRecordsWeeklyRepository> logger)
        : base(database, logger)
    {
    }
}
