using TNRD.Zeepkist.GTR.Backend.Directus.Api.Core;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Api;

internal class RecordsApi : BaseApi<RecordsFilterBuilder, RecordsFactory, RecordModel>
{
    /// <inheritdoc />
    public RecordsApi(IDirectusClient client)
        : base(client)
    {
    }

    /// <inheritdoc />
    protected override string Collection => "records";

    /// <inheritdoc />
    protected override int DefaultDepth => 2;
}
