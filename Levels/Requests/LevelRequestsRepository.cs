using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests;

public interface ILevelRequestsRepository : IBasicRepository<LevelRequest>
{
}

public class LevelRequestsRepository : BasicRepository<LevelRequest>, ILevelRequestsRepository
{
    public LevelRequestsRepository(IDatabase database, ILogger<LevelRequestsRepository> logger)
        : base(database, logger)
    {
    }
}
