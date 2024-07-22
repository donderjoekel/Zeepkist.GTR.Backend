using TNRD.Zeepkist.GTR.Backend.DataStore;
using Version = TNRD.Zeepkist.GTR.Database.Data.Entities.Version;

namespace TNRD.Zeepkist.GTR.Backend.Versioning;

public interface IVersionRepository : IBasicRepository<Version>
{
    Version Get();
}

public class VersionRepository : BasicRepository<Version>, IVersionRepository
{
    public VersionRepository(IDatabase database, ILogger<VersionRepository> logger)
        : base(database, logger)
    {
    }

    public Version Get()
    {
        Version? version = GetSingle(x => true);
        if (version == null)
        {
            throw new InvalidOperationException("No version found");
        }

        return version;
    }
}
