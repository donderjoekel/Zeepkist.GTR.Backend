using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class FavoritesFactory : BaseFactory<FavoritesFactory>
{
    public FavoritesFactory WithLevel(int levelId)
    {
        model.level = levelId;
        return this;
    }
    
    public FavoritesFactory WithUser(int userId)
    {
        model.user = userId;
        return this;
    }
}
