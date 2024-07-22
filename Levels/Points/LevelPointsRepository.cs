using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Points;

public interface ILevelPointsRepository : IBasicRepository<LevelPoints>
{
}

public class LevelPointsRepository : BasicRepository<LevelPoints>, ILevelPointsRepository
{
    public LevelPointsRepository(IDatabase database, ILogger<LevelPointsRepository> logger)
        : base(database, logger)
    {
    }
}
