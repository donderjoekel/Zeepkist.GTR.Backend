using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class VotesFilterBuilder : BaseFilterBuilder<VotesFilterBuilder>
{
    public VotesFilterBuilder WithLevel(int? levelId, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter(CreateMultiKey("level", "id"), levelId, mode);
    }

    public VotesFilterBuilder WithUser(int? userId, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter(CreateMultiKey("user", "id"), userId, mode);
    }

    public VotesFilterBuilder WithSteamId(string? steamId, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter(CreateMultiKey("user", "steam_id"), steamId, mode);
    }

    public VotesFilterBuilder WithUid(string? uid, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter(CreateMultiKey("level", "uid"), uid, mode);
    }

    public VotesFilterBuilder WithWorkshopId(string? workshopId, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter(CreateMultiKey("level", "wid"), workshopId, mode);
    }

    public VotesFilterBuilder WithCategory(int? categoryId, FilterMode mode = FilterMode.Equals)
    {
        return AddFilter("category", categoryId, mode);
    }
}
