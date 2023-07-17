using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Hot;

internal class Endpoint : PopularityBaseEndpoint<LevelsGetHotResponseDTO>
{
    /// <inheritdoc />
    public Endpoint(IMemoryCache cache, GTRContext context)
        : base(cache, context)
    {
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/hot");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        List<LevelPopularityResponseModel> levels = await GetLevelsFromCache(req, "hot", ct);

        LevelsGetHotResponseDTO response = new()
        {
            Levels = levels
        };

        await SendOkAsync(response, ct);
    }
}
