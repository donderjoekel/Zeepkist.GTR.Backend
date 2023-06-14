using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.All;

internal class Endpoint : Endpoint<RecordsGetRequestDTO, RecordsGetResponseDTO>
{
    private readonly IDirectusClient client;

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

    /// <inheritdoc />
    public Endpoint(IDirectusClient client)
    {
        this.client = client;
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
        RecordsApi api = new RecordsApi(client);

        Result<DirectusGetMultipleResponse<RecordModel>> result = await api.Get(filter =>
            {
                filter
                    .WithUserId(req.UserId)
                    .WithUserSteamId(req.UserSteamId)
                    .WithGameVersion(req.GameVersion)
                    .WithLevelId(req.LevelId)
                    .WithLevelUid(req.LevelUid)
                    .WithLevelWid(req.LevelWorkshopId);

                if (req.InvalidOnly == true)
                {
                    filter.WithInvalidOnly();
                }
                else
                {
                    if (req.BestOnly.HasValue && req.BestOnly.Value)
                    {
                        filter.WithBestOnly(true);
                    }
                    
                    if (req.ValidOnly.HasValue && req.ValidOnly.Value)
                    {
                        filter.WithValidOnly(true);
                    }
                    
                    if (req.WorldRecordOnly.HasValue && req.WorldRecordOnly.Value)
                    {
                        filter.WithWorldRecordOnly(true);
                    }
                }

                filter
                    .WithTime(req.MinimumTime, req.MaximumTime)
                    .WithSort(MapAndBuildSort(req))
                    .WithLimit(req.Limit)
                    .WithOffset(req.Offset);
            },
            ct);

        if (result.IsFailed)
        {
            Logger.LogError("Unable to get records: {Result}", result.ToString());
            ThrowError("Unable to get records");
        }

        List<RecordResponseModel> records = new();

        foreach (RecordModel model in result.Value.Data)
        {
            records.Add(model);
        }

        RecordsGetResponseDTO responseModel = new RecordsGetResponseDTO()
        {
            TotalAmount = result.Value.Metadata!.FilterCount!.Value,
            Records = records,
        };

        await SendAsync(responseModel, cancellation: ct);
    }

    private static string MapAndBuildSort(RecordsGetRequestDTO req)
    {
        if (string.IsNullOrEmpty(req.Sort))
        {
            return "level.id,time";
        }

        string[] splits = req.Sort.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        List<string> sorts = new List<string>();

        foreach (string split in splits)
        {
            bool isNegative = false;
            string? s = split;

            if (s.StartsWith('-'))
            {
                isNegative = true;
                s = s.Substring(1);
            }

            s = MapSort(s);

            if (string.IsNullOrEmpty(s))
                continue;

            if (isNegative)
                s = "-" + s;

            sorts.Add(s);
        }

        return string.Join(',', sorts);
    }

    private static string? MapSort(string input)
    {
        if (sortingMap.TryGetValue(input, out string? output))
            return output;

        return null;
    }
}
