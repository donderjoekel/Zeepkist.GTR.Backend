using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.All;

internal class Endpoint : Endpoint<RecordsGetRequestDTO, RecordsGetResponseDTO>
{
    private static readonly Dictionary<string, string> sortingMap =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "id", "id" },
            { "levelId", "level.id" },
            { "levelUid", "level.uid" },
            { "levelWorkshopId", "level.wid" },
            { "userId", "user.id" },
            { "userSteamId", "user.steam_id" },
            { "time", "time" },
            { "timeAuthor", "level.time_author" },
            { "timeGold", "level.time_gold" },
            { "timeSilver", "level.time_silver" },
            { "timeBronze", "level.time_bronze" },
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
                query = split switch
                {
                    "id" => query.OrderBy(x => x.Id),
                    "-id" => query.OrderByDescending(x => x.Id),
                    "levelId" => query.OrderBy(x => x.LevelNavigation!.Id),
                    "-levelId" => query.OrderByDescending(x => x.LevelNavigation!.Id),
                    "levelUid" => query.OrderBy(x => x.LevelNavigation!.Uid),
                    "-levelUid" => query.OrderByDescending(x => x.LevelNavigation!.Uid),
                    "levelWorkshopId" => query.OrderBy(x => x.LevelNavigation!.Wid),
                    "-levelWorkshopId" => query.OrderByDescending(x => x.LevelNavigation!.Wid),
                    "userId" => query.OrderBy(x => x.UserNavigation!.Id),
                    "-userId" => query.OrderByDescending(x => x.UserNavigation!.Id),
                    "userSteamId" => query.OrderBy(x => x.UserNavigation!.SteamId),
                    "-userSteamId" => query.OrderByDescending(x => x.UserNavigation!.SteamId),
                    "time" => query.OrderBy(x => x.Time),
                    "-time" => query.OrderByDescending(x => x.Time),
                    "timeAuthor" => query.OrderBy(x => x.LevelNavigation!.TimeAuthor),
                    "-timeAuthor" => query.OrderByDescending(x => x.LevelNavigation!.TimeAuthor),
                    "timeGold" => query.OrderBy(x => x.LevelNavigation!.TimeGold),
                    "-timeGold" => query.OrderByDescending(x => x.LevelNavigation!.TimeGold),
                    "timeSilver" => query.OrderBy(x => x.LevelNavigation!.TimeSilver),
                    "-timeSilver" => query.OrderByDescending(x => x.LevelNavigation!.TimeSilver),
                    "timeBronze" => query.OrderBy(x => x.LevelNavigation!.TimeBronze),
                    "-timeBronze" => query.OrderByDescending(x => x.LevelNavigation!.TimeBronze),
                    _ => query
                };
            }
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
