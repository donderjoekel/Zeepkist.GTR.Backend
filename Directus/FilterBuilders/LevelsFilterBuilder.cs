using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;

internal class LevelsFilterBuilder : BaseFilterBuilder<LevelsFilterBuilder>
{
    public LevelsFilterBuilder WithUid(string? uid)
    {
        return AddFilter("uid", uid);
    }

    public LevelsFilterBuilder WithWid(string? wid)
    {
        return AddFilter("wid", wid);
    }

    public LevelsFilterBuilder WithName(string? name)
    {
        return AddFilter("name", name);
    }

    public LevelsFilterBuilder WithAuthor(string? author)
    {
        return AddFilter("author", author);
    }

    public LevelsFilterBuilder WithValidOnly(bool? validOnly)
    {
        if (validOnly == true)
            AddFilter("is_valid", true);
        return this;
    }

    public LevelsFilterBuilder WithInvalidOnly(bool? invalidOnly)
    {
        if (invalidOnly == true)
            AddFilter("is_valid", false);
        return this;
    }
}
