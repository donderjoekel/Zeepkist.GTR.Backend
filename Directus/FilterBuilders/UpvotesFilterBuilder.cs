using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class UpvotesFilterBuilder : BaseFilterBuilder<UpvotesFilterBuilder>
{
    public UpvotesFilterBuilder WithUserSteamId(string steamId, FilterMode filterMode = FilterMode.Equals)
    {
        AddFilter(CreateMultiKey("user", "steam_id"), steamId, filterMode);
        return this;
    }

    public UpvotesFilterBuilder WithUserId(int id, FilterMode filterMode = FilterMode.Equals)
    {
        AddFilter(CreateMultiKey("user", "id"), id, filterMode);
        return this;
    }

    public UpvotesFilterBuilder WithLevelUid(string uid, FilterMode filterMode = FilterMode.Equals)
    {
        AddFilter(CreateMultiKey("level", "uid"), uid, filterMode);
        return this;
    }

    public UpvotesFilterBuilder WithLevelId(int id, FilterMode filterMode = FilterMode.Equals)
    {
        AddFilter(CreateMultiKey("level", "id"), id, filterMode);
        return this;
    }

    public UpvotesFilterBuilder WithLevelWorkshopId(string workshopId, FilterMode filterMode = FilterMode.Equals)
    {
        AddFilter(CreateMultiKey("level", "wid"), workshopId, filterMode);
        return this;
    }
}
