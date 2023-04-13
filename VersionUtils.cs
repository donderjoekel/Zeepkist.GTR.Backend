using Semver;

namespace TNRD.Zeepkist.GTR.Backend;

internal class VersionUtils
{
    public static readonly SemVersion MinimumVersion = SemVersion.Parse("0.16.0", SemVersionStyles.Strict);

    public static bool MeetsMinimumVersion(string version)
    {
        if (SemVersion.TryParse(version, SemVersionStyles.Strict, out SemVersion? semVersion))
        {
            return semVersion.ComparePrecedenceTo(MinimumVersion) >= 0;
        }

        return false;
    }
}
