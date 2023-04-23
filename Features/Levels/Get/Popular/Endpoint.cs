using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Popular;

internal class Endpoint : EndpointWithoutRequest<LevelsGetPopularResponseDTO>
{
    private readonly IMemoryCache cache;

    /// <inheritdoc />
    public Endpoint(IMemoryCache cache)
    {
        this.cache = cache;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/popular");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        LevelsGetPopularResponseDTO response = new()
        {
            Levels = new List<LevelPopularityResponseModel>()
        };

        if (cache.TryGetValue<List<LevelPopularityResponseModel>>("popular",
                out List<LevelPopularityResponseModel>? cached))
        {
            response.Levels = cached!;
        }

        await SendOkAsync(response, ct);
    }
}
