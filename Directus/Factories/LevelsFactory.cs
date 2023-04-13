using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Factories;

internal class LevelsFactory : BaseFactory<LevelsFactory>
{
    public LevelsFactory WithAuthor(string author)
    {
        model.author = author;
        return this;
    }

    public LevelsFactory WithThumbnailUrl(string url)
    {
        model.thumbnail_url = url;
        return this;
    }
}
