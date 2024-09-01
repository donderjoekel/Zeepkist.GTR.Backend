using Version = TNRD.Zeepkist.GTR.Database.Data.Entities.Version;

namespace TNRD.Zeepkist.GTR.Backend.Versioning;

public interface IVersionService
{
    bool NeedsUpdate(string version);
}

public class VersionService : IVersionService
{
    private readonly IVersionRepository _versionRepository;

    public VersionService(IVersionRepository versionRepository)
    {
        _versionRepository = versionRepository;
    }

    public bool NeedsUpdate(string version)
    {
#if DEBUG
        return false;
#endif
        if (!System.Version.TryParse(version, out System.Version? clientVersion))
        {
            throw new ArgumentException("Invalid version format");
        }

        Version backendVersion = _versionRepository.Get();
        if (!System.Version.TryParse(backendVersion.Minimum, out System.Version? minimumVersion))
        {
            throw new InvalidOperationException("Invalid version format");
        }

        return clientVersion < minimumVersion;
    }
}
