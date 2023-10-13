// using Microsoft.Extensions.Caching.Memory;
// using TNRD.Zeepkist.GTR.Database;
//
// namespace TNRD.Zeepkist.GTR.Backend.Jobs;
//
// internal class CalculatePopularLevelsJob : PopularityJobBase
// {
//     /// <inheritdoc />
//     public CalculatePopularLevelsJob(GTRContext db, IMemoryCache cache)
//         : base(db, cache)
//     {
//     }
//
//     /// <inheritdoc />
//     protected override DateTime GetStamp()
//     {
//         return DateTime.UtcNow.AddDays(-(DateTime.UtcNow.Day - 1)).Date;
//     }
//
//     /// <inheritdoc />
//     protected override int GetTakeCount()
//     {
//         return 100;
//     }
//
//     /// <inheritdoc />
//     protected override string GetCacheKey()
//     {
//         return "popular";
//     }
// }
