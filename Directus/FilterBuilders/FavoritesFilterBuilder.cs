using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class FavoritesFilterBuilder : BaseFilterBuilder<FavoritesFilterBuilder>
{
    public FavoritesFilterBuilder WithLevel(int? levelId, FilterMode mode = FilterMode.Equals)
    {
        if (levelId.HasValue)
            AddFilter(CreateMultiKey("level", "id"), levelId.Value, mode);
        return this;
    }

    public FavoritesFilterBuilder WithUser(int? userId, FilterMode mode = FilterMode.Equals)
    {
        if (userId.HasValue)
            AddFilter(CreateMultiKey("user", "id"), userId.Value, mode);
        return this;
    }

    public FavoritesFilterBuilder WithSteamId(string? steamId, FilterMode mode = FilterMode.Equals)
    {
        if (!string.IsNullOrEmpty(steamId))
            AddFilter(CreateMultiKey("user", "steam_id"), steamId, mode);
        return this;
    }

    public FavoritesFilterBuilder WithUid(string? uid, FilterMode mode = FilterMode.Equals)
    {
        if (!string.IsNullOrEmpty(uid))
            AddFilter(CreateMultiKey("level", "uid"), uid, mode);
        return this;
    }

    public FavoritesFilterBuilder WithWorkshopId(string? workshopId, FilterMode mode = FilterMode.Equals)
    {
        if (!string.IsNullOrEmpty(workshopId))
            AddFilter(CreateMultiKey("level", "wid"), workshopId, mode);
        return this;
    }
}
