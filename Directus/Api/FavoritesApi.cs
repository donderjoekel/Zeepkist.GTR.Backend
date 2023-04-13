using TNRD.Zeepkist.GTR.Backend.Directus.Api.Core;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Api;

internal class FavoritesApi : BaseApi<FavoritesFilterBuilder, FavoritesFactory, FavoriteModel>
{
    /// <inheritdoc />
    public FavoritesApi(IDirectusClient client)
        : base(client)
    {
    }

    /// <inheritdoc />
    protected override string Collection => "favorites";

    /// <inheritdoc />
    protected override int DefaultDepth => 3;
}
