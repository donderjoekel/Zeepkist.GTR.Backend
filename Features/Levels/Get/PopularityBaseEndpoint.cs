using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get;

internal class PopularityBaseEndpoint<T> : Endpoint<GenericGetRequestDTO, T>
{
    private readonly IMemoryCache cache;
    private readonly GTRContext context;

    protected PopularityBaseEndpoint(IMemoryCache cache, GTRContext context)
    {
        this.cache = cache;
        this.context = context;
    }

    protected async Task<List<LevelPopularityResponseModel>> GetLevelsFromCache(
        GenericGetRequestDTO req,
        string key,
        int defaultLimit,
        CancellationToken ct
    )
    {
        if (!cache.TryGetValue<List<LevelPopularityResponseModel>>(key, out List<LevelPopularityResponseModel>? cached))
            return new List<LevelPopularityResponseModel>();

        int limit = req.Limit ?? defaultLimit;
        int offset = req.Offset ?? 0;
        List<LevelPopularityResponseModel> levels = cached.Skip(offset).Take(limit).ToList();

        foreach (LevelPopularityResponseModel model in levels)
        {
            Record? worldRecord = await context.Records.AsNoTracking()
                .Where(x => x.IsWr && x.Level == model.Level.Id)
                .Include(x => x.UserNavigation)
                .FirstOrDefaultAsync(ct);

            model.Level.WorldRecord = worldRecord?.ToResponseModel() ?? null;
        }

        return levels;
    }
}
