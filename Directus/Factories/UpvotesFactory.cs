using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class UpvotesFactory : BaseFactory<UpvotesFactory>
{
    public UpvotesFactory WithUser(int userId)
    {
        model.user = userId;
        return this;
    }

    public UpvotesFactory WithLevel(int levelId)
    {
        model.level = levelId;
        return this;
    }
}
