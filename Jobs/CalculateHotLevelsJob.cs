using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Database;

namespace TNRD.Zeepkist.GTR.Backend.Jobs;

internal class CalculateHotLevelsJob : PopularityJobBase
{
    /// <inheritdoc />
    public CalculateHotLevelsJob(GTRContext db, IMemoryCache cache)
        : base(db, cache)
    {
    }

    /// <inheritdoc />
    protected override DateTime GetStamp()
    {
        return DateTime.UtcNow.AddDays(-1);
    }

    /// <inheritdoc />
    protected override int GetTakeCount()
    {
        return 10;
    }

    /// <inheritdoc />
    protected override string GetCacheKey()
    {
        return "hot";
    }
}
