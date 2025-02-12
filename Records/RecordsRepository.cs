using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records;

public interface IRecordsRepository : IBasicRepository<Record>
{
    IEnumerable<Record> GetByLevelId(int levelId);
    Record? GetBest(int levelId);
    Record? GetBestForUser(int userId, int levelId);
}

public class RecordsRepository : BasicRepository<Record>, IRecordsRepository
{
    public RecordsRepository(IDatabase database, ILogger<RecordsRepository> logger)
        : base(database, logger)
    {
    }

    public IEnumerable<Record> GetByLevelId(int levelId)
    {
        return GetAll(x => x.IdLevel == levelId);
    }

    public Record? GetBest(int levelId)
    {
        return GetAll(x => x.IdLevel == levelId)
            .OrderBy(x => x.Time)
            .FirstOrDefault();
    }

    public Record? GetBestForUser(int userId, int levelId)
    {
        return GetAll(x => x.IdLevel == levelId && x.IdUser == userId)
            .OrderBy(x => x.Time)
            .FirstOrDefault();
    }
}