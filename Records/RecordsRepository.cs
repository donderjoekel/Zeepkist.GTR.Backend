using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records;

public interface IRecordsRepository : IBasicRepository<Record>
{
}

public class RecordsRepository : BasicRepository<Record>, IRecordsRepository
{
    public RecordsRepository(IDatabase database, ILogger<RecordsRepository> logger)
        : base(database, logger)
    {
    }
}