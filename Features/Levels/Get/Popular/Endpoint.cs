using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Popular;

internal class Endpoint : PopularityBaseEndpoint<LevelsGetPopularResponseDTO>
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
        Get("levels/popular");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        List<LevelPopularityResponseModel> levels = await GetLevelsFromCache(req, "popular", ct);

        LevelsGetPopularResponseDTO response = new()
        {
            Levels = levels
        };

        await SendOkAsync(response, ct);
    }
}
