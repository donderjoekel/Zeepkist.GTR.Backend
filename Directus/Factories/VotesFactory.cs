using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class VotesFactory : BaseFactory<VotesFactory>
{
    public VotesFactory WithLevel(int levelId)
    {
        model.level = levelId;
        return this;
    }

    public VotesFactory WithUser(int userId)
    {
        model.user = userId;
        return this;
    }

    public VotesFactory WithCategory(int category)
    {
        model.category = category;
        return this;
    }

    public VotesFactory WithScore(int score)
    {
        model.score = score;
        return this;
    }
}
