using FastEndpoints;
using Npgsql;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.Best;

public class Endpoint : Endpoint<RequestModel, RecordsGetResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("records/best");
    }

    public async Task<IEnumerable<Record>> GetBestRecords(
        DateTime? start,
        DateTime? end,
        int? user,
        int? level,
        bool? valid
    )
    {
        NpgsqlParameter paramStart = new("start_date", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        {
            Value = start
        };

        NpgsqlParameter paramEnd = new("end_date", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        {
            Value = end
        };

        NpgsqlParameter paramUser = new("user_filter", NpgsqlTypes.NpgsqlDbType.Integer)
        {
            Value = user
        };

        NpgsqlParameter paramLevel = new("level_filter", NpgsqlTypes.NpgsqlDbType.Integer)
        {
            Value = level
        };

        NpgsqlParameter paramValid = new("is_valid_filter", NpgsqlTypes.NpgsqlDbType.Boolean)
        {
            Value = valid
        };

        return await context.Records
            .FromSqlRaw(
                @"SELECT * FROM get_best(@start_date, @end_date, @user_filter, @level_filter, @is_valid_filter)",
                paramStart,
                paramEnd,
                paramUser,
                paramLevel,
                paramValid
            )
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task HandleAsync(RequestModel req, CancellationToken ct)
    {
        // IQueryable<Record> query = context.Records
        // .AsNoTracking();

        DateTime? beforeDateTime = null;
        if (!string.IsNullOrEmpty(req.Before) && long.TryParse(req.Before, out long before))
        {
            beforeDateTime = DateTimeOffset.FromUnixTimeSeconds(before).UtcDateTime;
            // query = query.Where(x => x.DateCreated < beforeDateTime);
        }

        DateTime? afterDateTime = null;
        if (!string.IsNullOrEmpty(req.After) && long.TryParse(req.After, out long after))
        {
            afterDateTime = DateTimeOffset.FromUnixTimeSeconds(after).UtcDateTime;
            // query = query.Where(x => x.DateCreated > afterDateTime);
        }

        bool? valid = null;
        if (req.ValidOnly.HasValue || req.InvalidOnly.HasValue)
        {
            if (req.ValidOnly.HasValue && req.ValidOnly.Value)
                valid = true;
            else if (req.InvalidOnly.HasValue && req.InvalidOnly.Value)
                valid = false;
        }

        List<BestRecord> bestRecords = await context.GetBestRecords(
            afterDateTime,
            beforeDateTime,
            req.User,
            req.Level,
            valid,
            req.Limit.HasValue ? Math.Min(100, req.Limit.Value) : 100,
            req.Offset.HasValue ? Math.Max(0, req.Offset.Value) : 0);

        List<int> bestRecordIds = bestRecords.Select(x => x.Id).ToList();

        List<Record> actualRecords = await context.Records
            .AsNoTracking()
            .Include(x => x.LevelNavigation)
            .Include(x => x.UserNavigation)
            .Where(x => bestRecordIds.Contains(x.Id))
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Time)
            .ToListAsync(ct);

        await SendOkAsync(new RecordsGetResponseDTO
            {
                TotalAmount = -1,
                After = afterDateTime,
                Before = beforeDateTime,
                Records = actualRecords.Select(x => x.ToResponseModel()).ToList()
            },
            ct);

        // if (req.User.HasValue)
        //     query = query.Where(x => x.User == req.User.Value);
        //
        // if (req.Level.HasValue)
        //     query = query.Where(x => x.Level == req.Level.Value);
        //
        // var lowestTimes = query
        //     .GroupBy(x => new { x.User, x.Level })
        //     .Select(x => new RecordKey
        //     {
        //         User = x.Key.User,
        //         Level = x.Key.Level,
        //         Time = x.Min(y => y.Time)
        //     }).AsEnumerable();
        //
        // var query2 = context.Records
        //     .AsNoTracking()
        //     .Join<Record, RecordKey, RecordKey, Record>(lowestTimes,
        //         r => new RecordKey() { User = r.User, Level = r.Level, Time = r.Time },
        //         l => new RecordKey() { User = l.User, Level = l.Level, Time = l.Time },
        //         (r, l) => r);
        //
        // if (beforeDateTime.HasValue)
        // {
        //     query2 = query2.Where(x => x.DateCreated < beforeDateTime);
        // }
        //
        // if (afterDateTime.HasValue)
        // {
        //     query2 = query2.Where(x => x.DateCreated > afterDateTime);
        // }
        //
        // List<Record> records = await query2.ToListAsync(ct);
    }

    private class RecordKey
    {
        public int? User { get; set; }
        public int? Level { get; set; }
        public float? Time { get; set; }
    }
}
