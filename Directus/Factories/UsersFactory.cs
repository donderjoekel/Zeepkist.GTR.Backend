using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class UsersFactory : BaseFactory<UsersFactory>
{
    public UsersFactory WithSteamName(string steamName)
    {
        model.steam_name = steamName;
        return this;
    }
}
