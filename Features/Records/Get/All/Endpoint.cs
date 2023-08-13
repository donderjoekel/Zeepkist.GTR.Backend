using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.All;

internal class Endpoint : Endpoint<RecordsGetRequestDTO, RecordsGetResponseDTO>
{
    private record SortingMethod(string Key, Func<IQueryable<Record>, IQueryable<Record>> Method)
    {
        public IQueryable<Record> Process(string input, IQueryable<Record> query)
        {
            return Key.Equals(input, StringComparison.InvariantCultureIgnoreCase) ? Method(query) : query;
        }
    }

    private static readonly HashSet<SortingMethod> sortingMethods =
        new()
        {
            new SortingMethod("id", query => query.OrderBy(x => x.Id)),
            new SortingMethod("-id", query => query.OrderByDescending(x => x.Id)),
            new SortingMethod("levelId", query => query.OrderBy(x => x.Level)),
            new SortingMethod("-levelId", query => query.OrderByDescending(x => x.Level)),
            new SortingMethod("levelUid", query => query.OrderBy(x => x.LevelNavigation!.Uid)),
            new SortingMethod("-levelUid", query => query.OrderByDescending(x => x.LevelNavigation!.Uid)),
            new SortingMethod("levelWorkshopId", query => query.OrderBy(x => x.LevelNavigation!.Wid)),
            new SortingMethod("-levelWorkshopId", query => query.OrderByDescending(x => x.LevelNavigation!.Wid)),
            new SortingMethod("userId", query => query.OrderBy(x => x.User)),
            new SortingMethod("-userId", query => query.OrderByDescending(x => x.User)),
            new SortingMethod("userSteamId", query => query.OrderBy(x => x.UserNavigation!.SteamId)),
            new SortingMethod("-userSteamId", query => query.OrderByDescending(x => x.UserNavigation!.SteamId)),
            new SortingMethod("time", query => query.OrderBy(x => x.Time)),
            new SortingMethod("-time", query => query.OrderByDescending(x => x.Time)),
            new SortingMethod("timeAuthor", query => query.OrderBy(x => x.LevelNavigation!.TimeAuthor)),
            new SortingMethod("-timeAuthor", query => query.OrderByDescending(x => x.LevelNavigation!.TimeAuthor)),
            new SortingMethod("timeGold", query => query.OrderBy(x => x.LevelNavigation!.TimeGold)),
            new SortingMethod("-timeGold", query => query.OrderByDescending(x => x.LevelNavigation!.TimeGold)),
            new SortingMethod("timeSilver", query => query.OrderBy(x => x.LevelNavigation!.TimeSilver)),
            new SortingMethod("-timeSilver", query => query.OrderByDescending(x => x.LevelNavigation!.TimeSilver)),
            new SortingMethod("timeBronze", query => query.OrderBy(x => x.LevelNavigation!.TimeBronze)),
            new SortingMethod("-timeBronze", query => query.OrderByDescending(x => x.LevelNavigation!.TimeBronze)),
        };

    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("records");
        Summary(s =>
        {
            s.RequestParam(p => p.BestOnly!,
                "If true this will only return records that are the best records per player per map <br />" +
                "If false this will also include non-best records");

            s.RequestParam(p => p.ValidOnly!,
                "If true this will only return records that are considered valid <br />" +
                "If false this will also include invalid records");

            s.RequestParam(p => p.InvalidOnly!,
                "If true this will only return records that are considered invalid <br />" +
                "Does not work in combination with BestOnly, ValidOnly, or WorldRecordOnly");

            s.RequestParam(p => p.WorldRecordOnly!,
                "If true this will only return world records per map <br />" +
                "If false this will also include non-world record records");

            s.RequestParam(p => p.Sort!,
                "Comma separated value. <br /> Can be negated by adding a '-' in front of the option. <br /> Valid options are: " +
                "id, levelId, levelUid, levelWorkshopId, userId, userSteamId, " +
                "time, timeAuthor, timeGold, timeSilver, timeBronze");
        });
        Description(b => { b.ProducesProblem(500); });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(RecordsGetRequestDTO req, CancellationToken ct)
    {
        IQueryable<Record> query = context.Records.AsNoTracking()
            .Include(x => x.LevelNavigation)
            .Include(x => x.UserNavigation);

        if (req.LevelId.HasValue)
            query = query.Where(x => x.Level == req.LevelId.Value);
        if (!string.IsNullOrEmpty(req.LevelUid))
            query = query.Where(x => x.LevelNavigation!.Uid == req.LevelUid);
        if (!string.IsNullOrEmpty(req.LevelWorkshopId))
            query = query.Where(x => x.LevelNavigation!.Wid == req.LevelWorkshopId);
        if (!string.IsNullOrEmpty(req.UserSteamId))
            query = query.Where(x => x.UserNavigation!.SteamId == req.UserSteamId);
        if (!string.IsNullOrEmpty(req.GameVersion))
            query = query.Where(x => x.GameVersion == req.GameVersion);
        if (req.MinimumTime.HasValue)
            query = query.Where(x => x.Time >= req.MinimumTime.Value);
        if (req.MaximumTime.HasValue)
            query = query.Where(x => x.Time <= req.MaximumTime.Value);

        if (req.InvalidOnly == true)
            query = query.Where(x => x.IsValid == false);
        else
        {
            if (req.ValidOnly == true)
                query = query.Where(x => x.IsValid == true);
            if (req.BestOnly == true)
                query = query.Where(x => x.IsBest == true);
            if (req.WorldRecordOnly == true)
                query = query.Where(x => x.IsWr == true);
        }

        if (!string.IsNullOrEmpty(req.Sort))
        {
            string[] splits = req.Sort.Split(',');
            foreach (string split in splits)
            {
                foreach (SortingMethod sortingMethod in sortingMethods)
                {
                    sortingMethod.Process(split, query);
                }
            }
        }
        else
        {
            query = query.OrderBy(x => x.Level).ThenBy(x => x.Time);
        }

        int total = await query.CountAsync(ct);

        int limit = req.Limit ?? 100;
        int offset = req.Offset ?? 0;

        List<Record> records = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        await SendOkAsync(new RecordsGetResponseDTO()
            {
                TotalAmount = total,
                Records = records.Select(x => x.ToResponseModel()).ToList(),
            },
            ct);
    }
}
