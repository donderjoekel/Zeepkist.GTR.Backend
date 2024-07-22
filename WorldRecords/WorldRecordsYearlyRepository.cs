using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsYearlyRepository : IBasicRepository<WorldRecordYearly>
{
}

public class WorldRecordsYearlyRepository : BasicRepository<WorldRecordYearly>, IWorldRecordsYearlyRepository
{
    public WorldRecordsYearlyRepository(IDatabase database, ILogger<WorldRecordsYearlyRepository> logger)
        : base(database, logger)
    {
    }
}
