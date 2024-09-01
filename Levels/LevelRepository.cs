using System.Diagnostics.CodeAnalysis;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels;

public interface ILevelRepository : IBasicRepository<Level>
{
    bool TryGetByHash(string hash, [NotNullWhen(true)] out Level? level);
}

public class LevelRepository : BasicRepository<Level>, ILevelRepository
{
    public LevelRepository(IDatabase database, ILogger<LevelRepository> logger) : base(database, logger)
    {
    }

    public bool TryGetByHash(string hash, [NotNullWhen(true)] out Level? level)
    {
        level = GetSingle(x => x.Hash == hash);
        return level != null;
    }
}
