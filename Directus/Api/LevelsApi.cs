using TNRD.Zeepkist.GTR.Backend.Directus.Api.Core;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Api;

internal class LevelsApi : BaseApi<LevelsFilterBuilder, LevelsFactory, LevelModel>
{
    /// <inheritdoc />
    public LevelsApi(IDirectusClient client)
        : base(client)
    {
    }

    /// <inheritdoc />
    protected override string Collection => "levels";
}
