using System.Globalization;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class RecordsFilterBuilder : BaseFilterBuilder<RecordsFilterBuilder>
{
    public RecordsFilterBuilder WithUserSteamId(string? steamId)
    {
        return AddFilter(CreateMultiKey("user", "steam_id"), steamId);
    }

    public RecordsFilterBuilder WithUserId(int? userId)
    {
        return AddFilter(CreateMultiKey("user", "id"), userId);
    }

    public RecordsFilterBuilder WithLevelUid(string? levelUid)
    {
        return AddFilter(CreateMultiKey("level", "uid"), levelUid);
    }

    public RecordsFilterBuilder WithLevelWid(string? levelWid)
    {
        return AddFilter(CreateMultiKey("level", "wid"), levelWid);
    }

    public RecordsFilterBuilder WithLevelId(int? levelId)
    {
        return AddFilter(CreateMultiKey("level", "id"), levelId);
    }

    public RecordsFilterBuilder WithGameVersion(string? gameVersion)
    {
        return AddFilter("game_version", gameVersion);
    }

    public RecordsFilterBuilder WithInvalidOnly()
    {
        return AddFilter("is_valid", false);
    }

    public RecordsFilterBuilder WithValidOnly(bool? validOnly)
    {
        return AddFilter("is_valid", validOnly);
    }

    public RecordsFilterBuilder WithBestOnly(bool? bestOnly)
    {
        return AddFilter("is_best", bestOnly);
    }

    public RecordsFilterBuilder WithWorldRecordOnly(bool? worldRecordOnly)
    {
        return AddFilter("is_wr", worldRecordOnly);
    }

    public RecordsFilterBuilder WithTime(float? minimumTime, float? maximumTime)
    {
        if (minimumTime != null && maximumTime != null)
        {
            AddFilter("time",
                minimumTime.Value.ToString(CultureInfo.InvariantCulture) + "," +
                maximumTime.Value.ToString(CultureInfo.InvariantCulture),
                FilterMode.Between);
        }
        else if (maximumTime != null)
        {
            return AddFilter("time",
                maximumTime.Value.ToString(CultureInfo.InvariantCulture),
                FilterMode.LessThanEquals);
        }
        else if (minimumTime != null)
        {
            return AddFilter("time",
                minimumTime.Value.ToString(CultureInfo.InvariantCulture),
                FilterMode.GreaterThanEquals);
        }

        return this;
    }
}
