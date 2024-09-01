using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsQuarterlyRepository : IBasicRepository<WorldRecordQuarterly>
{
}

public class WorldRecordsQuarterlyRepository : BasicRepository<WorldRecordQuarterly>, IWorldRecordsQuarterlyRepository
{
    public WorldRecordsQuarterlyRepository(IDatabase database, ILogger<WorldRecordsQuarterlyRepository> logger)
        : base(database, logger)
    {
    }
}
