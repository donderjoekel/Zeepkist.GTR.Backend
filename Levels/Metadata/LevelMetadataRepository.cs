using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Metadata;

public interface ILevelMetadataRepository : IBasicRepository<LevelMetadata>
{
    bool ExistsForLevel(int levelId);
}

public class LevelMetadataRepository : BasicRepository<LevelMetadata>, ILevelMetadataRepository
{
    public LevelMetadataRepository(IDatabase database, ILogger<LevelMetadataRepository> logger)
        : base(database, logger)
    {
    }

    public bool ExistsForLevel(int levelId)
    {
        return Query(x => x.IdLevel == levelId).Any();
    }
}
