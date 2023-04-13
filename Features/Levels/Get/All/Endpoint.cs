using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.All;

internal class Endpoint : Endpoint<LevelsGetRequestDTO, LevelsGetResponseDTO>
{
    private readonly IDirectusClient client;

    private static readonly Dictionary<string, string> sortingMap =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "id", "id" },
            { "name", "name" },
            { "author", "author" },
            { "uniqueId", "uid" },
            { "workshopId", "wid" },
            { "timeAuthor", "time_author" },
            { "timeGold", "time_gold" },
            { "timeSilver", "time_silver" },
            { "timeBronze", "time_bronze" },
            { "dateCreated", "date_created" }
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
                "id, name, author, uniqueId, workshopId, timeAuthor, timeGold, timeSilver, timeBronze, dateCreated");
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsGetRequestDTO req, CancellationToken ct)
    {
        LevelsApi api = new LevelsApi(client);

        Result<DirectusGetMultipleResponse<LevelModel>> result = await api.Get(filter =>
            {
                filter
                    .WithUid(req.Uid)
                    .WithAuthor(req.Author)
                    .WithName(req.Name)
                    .WithWid(req.WorkshopId)
                    .WithInvalidOnly(req.InvalidOnly)
                    .WithValidOnly(req.ValidOnly)
                    .WithLimit(req.Limit)
                    .WithOffset(req.Offset)
                    .WithSort(MapAndBuildSort(req));
            },
            ct);

        if (result.IsFailed)
        {
            Logger.LogError("Unable to get levels: {Result}", result.ToString());
            ThrowError("Unable to get levels");
        }

        List<LevelResponseModel> levels = new();

        foreach (LevelModel model in result.Value.Data)
        {
            levels.Add(model);
        }

        LevelsGetResponseDTO responseModel = new LevelsGetResponseDTO()
        {
            TotalAmount = result.Value.Metadata!.FilterCount!.Value,
            Levels = levels,
        };

        await SendAsync(responseModel, cancellation: ct);
    }

    private static string MapAndBuildSort(LevelsGetRequestDTO req)
    {
        if (string.IsNullOrEmpty(req.Sort))
        {
            return "id";
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
