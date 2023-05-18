using FastEndpoints;
using FluentResults;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.All;

internal class Endpoint : Endpoint<LevelsGetRequestDTO, LevelsGetResponseDTO>
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
        Get("levels");
        Summary(s =>
        {
            s.RequestParam(p => p.ValidOnly!,
                "If true this will only return levels that are considered valid <br />" +
                "If false this will also include invalid levels");

            s.RequestParam(p => p.InvalidOnly!,
                "If true this will only return levels that are considered invalid <br />" +
                "Does not work in combination with ValidOnly");

            s.RequestParam(p => p.Sort!,
                "Comma separated value. <br /> Can be negated by adding a '-' in front of the option. <br /> Valid options are: " +
                "id, name, author, uniqueId, workshopId, rank, points, timeAuthor, timeGold, timeSilver, timeBronze, dateCreated");
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsGetRequestDTO req, CancellationToken ct)
    {
        ExpressionStarter<Level> builder = PredicateBuilder.New<Level>(true);

        if (!string.IsNullOrEmpty(req.Uid))
            builder = builder.And(l => l.Uid == req.Uid);

        if (!string.IsNullOrEmpty(req.Author))
            builder = builder.And(l => l.Author == req.Author);

        if (!string.IsNullOrEmpty(req.Name))
            builder = builder.And(l => l.Name == req.Name);

        if (!string.IsNullOrEmpty(req.WorkshopId))
            builder = builder.And(l => l.Wid == req.WorkshopId);

        if (req.ValidOnly == true)
            builder = builder.And(l => l.IsValid == true);

        if (req.InvalidOnly == true)
            builder = builder.And(l => l.IsValid == false);

        int limit = req.Limit ?? 100;
        int offset = req.Offset ?? 0;

        IQueryable<Level> query = (from l in context.Levels
                orderby l.Id descending
                select l)
            .AsExpandable()
            .Where(builder);

        IOrderedQueryable<Level> sortedQuery = SortQuery(req, query);

        int count = sortedQuery.Count();
        List<Level> levels = sortedQuery.Skip(offset).Take(limit).ToList();

        List<LevelResponseModel> responseModels = levels.Select(l => l.ToResponseModel()).ToList();

        await SendAsync(new LevelsGetResponseDTO()
            {
                TotalAmount = count,
                Levels = responseModels
            },
            cancellation: ct);
    }

    private static IOrderedQueryable<Level> SortQuery(LevelsGetRequestDTO req, IQueryable<Level> queryable)
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
