using System.Globalization;
using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.All;

internal class Endpoint : Endpoint<RecordsGetRequestDTO, RecordsGetResponseDTO>
{
    private record SortingMethod(string Key, Func<IOrderedQueryable<Record>, IOrderedQueryable<Record>> Method)
    {
        public IOrderedQueryable<Record> Process(string input, IOrderedQueryable<Record> query)
        {
            return Key.Equals(input, StringComparison.InvariantCultureIgnoreCase) ? Method(query) : query;
        }
    }

    private static readonly HashSet<SortingMethod> sortingMethods =
        new()
        {
            new SortingMethod("id", query => query.ThenBy(x => x.Id)),
            new SortingMethod("-id", query => query.ThenByDescending(x => x.Id)),
            new SortingMethod("level", query => query.ThenBy(x => x.Level)),
            new SortingMethod("-level", query => query.ThenByDescending(x => x.Level)),
            new SortingMethod("userId", query => query.ThenBy(x => x.User)),
            new SortingMethod("-userId", query => query.ThenByDescending(x => x.User)),
            new SortingMethod("userSteamId", query => query.ThenBy(x => x.UserNavigation!.SteamId)),
            new SortingMethod("-userSteamId", query => query.ThenByDescending(x => x.UserNavigation!.SteamId)),
            new SortingMethod("time", query => query.ThenBy(x => x.Time)),
            new SortingMethod("-time", query => query.ThenByDescending(x => x.Time))
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
            s.RequestParam(p => p.ValidOnly!,
                "If true this will only return records that are considered valid <br />" +
                "If false this will also include invalid records");

            s.RequestParam(p => p.InvalidOnly!,
                "If true this will only return records that are considered invalid <br />" +
                "Does not work in combination with BestOnly, ValidOnly, or WorldRecordOnly");

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
            .Include(x => x.UserNavigation);

        query = ApplyRegularFilters(req, query);
        query = ApplySpecialFilters(req, query);
        query = ApplySort(req, query);

        DateTime? beforeDateTime = null;
        if (!string.IsNullOrEmpty(req.Before) && long.TryParse(req.Before, out long before))
        {
            beforeDateTime = DateTimeOffset.FromUnixTimeSeconds(before).UtcDateTime;
            query = query.Where(x => x.DateCreated < beforeDateTime);
        }

        DateTime? afterDateTime = null;
        if (!string.IsNullOrEmpty(req.After) && long.TryParse(req.After, out long after))
        {
            afterDateTime = DateTimeOffset.FromUnixTimeSeconds(after).UtcDateTime;
            query = query.Where(x => x.DateCreated > afterDateTime);
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
                After = afterDateTime,
                Before = beforeDateTime
            },
            ct);
    }

    private static IQueryable<Record> ApplyRegularFilters(RecordsGetRequestDTO req, IQueryable<Record> query)
    {
        if (req.Level.HasValue())
            query = query.Where(x => x.Level == req.Level);
        if (!string.IsNullOrEmpty(req.UserSteamId))
            query = query.Where(x => x.UserNavigation!.SteamId == req.UserSteamId);
        if (req.UserId.HasValue)
            query = query.Where(x => x.User == req.UserId);
        if (!string.IsNullOrEmpty(req.GameVersion))
            query = query.Where(x => x.GameVersion == req.GameVersion);
        if (req.MinimumTime.HasValue)
            query = query.Where(x => x.Time >= req.MinimumTime.Value);
        if (req.MaximumTime.HasValue)
            query = query.Where(x => x.Time <= req.MaximumTime.Value);
        if (req.GameVersion.HasValue())
            query = query.Where(x => x.GameVersion == req.GameVersion);

        return query;
    }

    private static IQueryable<Record> ApplySpecialFilters(RecordsGetRequestDTO req, IQueryable<Record> query)
    {
        if (req.InvalidOnly == true)
            query = query.Where(x => x.IsValid == false);
        else if (req.ValidOnly == true)
            query = query.Where(x => x.IsValid == true);

        return query;
    }

    private static IQueryable<Record> ApplySort(RecordsGetRequestDTO req, IQueryable<Record> query)
    {
        if (!string.IsNullOrEmpty(req.Sort))
        {
            IOrderedQueryable<Record> orderedQuery = query.OrderBy(x => 0);

            string[] splits = req.Sort.Split(',');
            foreach (string split in splits)
            {
                foreach (SortingMethod sortingMethod in sortingMethods)
                {
                    orderedQuery = sortingMethod.Process(split, orderedQuery);
                }
            }

            query = orderedQuery;
        }
        else
        {
            query = query.OrderBy(x => x.Level).ThenBy(x => x.Time);
        }

        return query;
    }
}
