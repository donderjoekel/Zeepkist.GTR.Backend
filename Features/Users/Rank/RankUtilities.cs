using Microsoft.Extensions.Caching.Memory;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rank;

internal class RankUtilities
{
    public static List<UsersRankingsResponseDTO.Ranking> GetRankings(IMemoryCache cache, GTRContext context)
    {
        if (cache.TryGetValue<List<UsersRankingsResponseDTO.Ranking>>("rankings",
                out List<UsersRankingsResponseDTO.Ranking>? cachedRankings))
        {
            return cachedRankings!;
        }

        var wrCounts = from r in context.Records
            join u in context.Users on r.User equals u.Id
            where r.IsWr
            group u by new { u.Id, u.SteamId, u.SteamName }
            into g
            orderby g.Count() descending
            select new
            {
                UserId = g.Key.Id,
                g.Key.SteamId,
                g.Key.SteamName,
                WrCount = g.Count()
            };

        List<UsersRankingsResponseDTO.Ranking> rankings = new();

        int i = 1;
        foreach (var wrCount in wrCounts)
        {
            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                Position = i,
                AmountOfWorldRecords = wrCount.WrCount,
                User = new UserResponseModel()
                {
                    Id = wrCount.UserId,
                    SteamName = wrCount.SteamName,
                    SteamId = wrCount.SteamId
                }
            });

            i++;
        }

        cache.Set("rankings", rankings, TimeSpan.FromMinutes(30));

        return rankings;
    }
}
