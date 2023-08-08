using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Search;

internal class Endpoint : Endpoint<LevelsSearchRequestDTO, LevelsSearchResponseDTO>
{
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
        Get("levels/search");
        Summary(s =>
        {
            s.RequestParam(p => p.Sort!,
                "Comma separated value. <br /> Can be negated by adding a '-' in front of the option. <br /> Valid options are: " +
                "id, name, author, uniqueId, workshopId, rank, points, timeAuthor, timeGold, timeSilver, timeBronze, dateCreated");
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsSearchRequestDTO req, CancellationToken ct)
    {
        IQueryable<Level> queryable = context.Levels.AsNoTracking();

        if (req.AuthorOnly.HasValue && req.AuthorOnly.Value)
        {
            queryable = queryable.Where(x => EF.Functions.ILike(x.Author, $"%{req.Query}%"));
        }
        else if (req.LevelOnly.HasValue && req.LevelOnly.Value)
        {
            queryable = queryable.Where(x => EF.Functions.ILike(x.Name, $"%{req.Query}%"));
        }
        else
        {
            queryable = queryable.Where(x =>
                EF.Functions.ILike(x.Name, $"%{req.Query}%") || EF.Functions.ILike(x.Author, $"%{req.Query}%"));
        }

        queryable = HandleMinMaxQueries(req, queryable);

        IOrderedQueryable<Level> sortedQuery = SortQuery(req, queryable);

        int count = sortedQuery.Count();

        var items = await sortedQuery.GroupJoin(context.Records.AsNoTracking().Include(r => r.UserNavigation),
                l => l.Id,
                r => r.Level,
                (l, r) => new
                {
                    Level = l,
                    WorldRecord = r.FirstOrDefault(x => x.IsWr)
                })
            .Skip(req.Offset ?? 0)
            .Take(req.Limit.HasValue ? Math.Min(100, req.Limit.Value) : 100)
            .ToListAsync(ct);

        await SendOkAsync(new LevelsSearchResponseDTO()
            {
                TotalAmount = count,
                Levels = items.Select(x => x.Level.ToResponseModel(x.WorldRecord)).ToList()
            },
            ct);
    }

    private static IQueryable<Level> HandleMinMaxQueries(LevelsSearchRequestDTO req, IQueryable<Level> queryable)
    {
        if (req.MinAuthor.HasValue)
            queryable = queryable.Where(x => x.TimeAuthor >= req.MinAuthor.Value);
        if (req.MaxAuthor.HasValue)
            queryable = queryable.Where(x => x.TimeAuthor <= req.MaxAuthor.Value);
        if (req.MinGold.HasValue)
            queryable = queryable.Where(x => x.TimeGold >= req.MinGold.Value);
        if (req.MaxGold.HasValue)
            queryable = queryable.Where(x => x.TimeGold <= req.MaxGold.Value);
        if (req.MinSilver.HasValue)
            queryable = queryable.Where(x => x.TimeSilver >= req.MinSilver.Value);
        if (req.MaxSilver.HasValue)
            queryable = queryable.Where(x => x.TimeSilver <= req.MaxSilver.Value);
        if (req.MinBronze.HasValue)
            queryable = queryable.Where(x => x.TimeBronze >= req.MinBronze.Value);
        if (req.MaxBronze.HasValue)
            queryable = queryable.Where(x => x.TimeBronze <= req.MaxBronze.Value);
        if (req.MinPoints.HasValue)
            queryable = queryable.Where(x => x.Points >= req.MinPoints.Value);
        if (req.MaxPoints.HasValue)
            queryable = queryable.Where(x => x.Points <= req.MaxPoints.Value);
        return queryable;
    }

    private static IOrderedQueryable<Level> SortQuery(LevelsSearchRequestDTO req, IQueryable<Level> queryable)
    {
        if (string.IsNullOrEmpty(req.Sort))
        {
            return queryable.OrderBy(l => l.Id);
        }

        string[] splits = req.Sort.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        IOrderedQueryable<Level>? orderedQueryable = null;

        foreach (string split in splits)
        {
            bool isNegative = false;
            string s = split;

            if (s.StartsWith('-'))
            {
                isNegative = true;
                s = s[1..];
            }

            orderedQueryable = s switch
            {
                "id" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Id)
                        : orderedQueryable.ThenByDescending(l => l.Id)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Id)
                        : orderedQueryable.ThenBy(l => l.Id),
                "name" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Name)
                        : orderedQueryable.ThenByDescending(l => l.Name)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Name)
                        : orderedQueryable.ThenBy(l => l.Name),
                "author" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Author)
                        : orderedQueryable.ThenByDescending(l => l.Author)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Author)
                        : orderedQueryable.ThenBy(l => l.Author),
                "uniqueId" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Uid)
                        : orderedQueryable.ThenByDescending(l => l.Uid)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Uid)
                        : orderedQueryable.ThenBy(l => l.Uid),
                "workshopId" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Wid)
                        : orderedQueryable.ThenByDescending(l => l.Wid)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Wid)
                        : orderedQueryable.ThenBy(l => l.Wid),
                "rank" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Rank)
                        : orderedQueryable.ThenByDescending(l => l.Rank)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Rank)
                        : orderedQueryable.ThenBy(l => l.Rank),
                "points" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.Points)
                        : orderedQueryable.ThenByDescending(l => l.Points)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.Points)
                        : orderedQueryable.ThenBy(l => l.Points),
                "timeAuthor" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.TimeAuthor)
                        : orderedQueryable.ThenByDescending(l => l.TimeAuthor)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.TimeAuthor)
                        : orderedQueryable.ThenBy(l => l.TimeAuthor),
                "timeGold" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.TimeGold)
                        : orderedQueryable.ThenByDescending(l => l.TimeGold)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.TimeGold)
                        : orderedQueryable.ThenBy(l => l.TimeGold),
                "timeSilver" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.TimeSilver)
                        : orderedQueryable.ThenByDescending(l => l.TimeSilver)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.TimeSilver)
                        : orderedQueryable.ThenBy(l => l.TimeSilver),
                "timeBronze" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.TimeBronze)
                        : orderedQueryable.ThenByDescending(l => l.TimeBronze)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.TimeBronze)
                        : orderedQueryable.ThenBy(l => l.TimeBronze),
                "dateCreated" => isNegative
                    ? orderedQueryable == null
                        ? queryable.OrderByDescending(l => l.DateCreated)
                        : orderedQueryable.ThenByDescending(l => l.DateCreated)
                    : orderedQueryable == null
                        ? queryable.OrderBy(l => l.DateCreated)
                        : orderedQueryable.ThenBy(l => l.DateCreated),
                _ => orderedQueryable
            };
        }

        return orderedQueryable ?? queryable.OrderBy(x => x.Id);
    }
}
