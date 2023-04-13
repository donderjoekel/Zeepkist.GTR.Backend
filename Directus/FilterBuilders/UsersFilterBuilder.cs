using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class UsersFilterBuilder : BaseFilterBuilder<UsersFilterBuilder>
{
    public UsersFilterBuilder WithSteamId(string steamId)
    {
        AddFilter("steam_id", steamId);
        return this;
    }
}
