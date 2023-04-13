using System.Globalization;
using System.Web;
using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Search;

internal class Endpoint : Endpoint<LevelsSearchRequestDTO, LevelsSearchResponseDTO>
{
    private readonly IDirectusClient client;

    /// <inheritdoc />
    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/search");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsSearchRequestDTO req, CancellationToken ct)
    {
        List<string> queries = new List<string>();

        queries.Add("fields=*.*");
        queries.Add("meta=filter_count");

        if (!string.IsNullOrEmpty(req.Query))
            queries.Add($"search={HttpUtility.UrlEncode(req.Query)}");

        AddTimingQueries(req, queries);

        if (req.Offset.HasValue)
            queries.Add($"offset={req.Offset}");
        else
            queries.Add("offset=0");

        if (req.Limit.HasValue)
            queries.Add($"limit={Math.Min(req.Limit.Value, 100).ToString()}");
        else
            queries.Add("limit=100");

        string queryString = string.Join('&', queries);

        Result<DirectusGetMultipleResponse<LevelModel>> result =
            await client.Get<DirectusGetMultipleResponse<LevelModel>>($"items/levels?{queryString}", ct);

        if (result.IsFailed)
        {
            Logger.LogCritical("Unable to get levels: {Result}", result.ToString());
            await SendAsync(null!, 500, ct);
            return;
        }

        List<LevelResponseModel> levels = new List<LevelResponseModel>();

        foreach (LevelModel model in result.Value.Data)
        {
            levels.Add(model);
        }

        LevelsSearchResponseDTO responseModel = new LevelsSearchResponseDTO()
        {
            TotalAmount = result.Value.Metadata!.FilterCount!.Value,
            Levels = levels,
        };

        await SendAsync(responseModel, cancellation: ct);
    }

    private static void AddTimingQueries(LevelsSearchRequestDTO req, List<string> queries)
    {
        if (req.MinAuthor.HasValue && req.MaxAuthor.HasValue)
            queries.Add(
                $"filter[time_author][_between]={req.MinAuthor.Value.ToString(CultureInfo.InvariantCulture)},{req.MaxAuthor.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MinAuthor.HasValue)
            queries.Add($"filter[time_author][_gte]={req.MinAuthor.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MaxAuthor.HasValue)
            queries.Add($"filter[time_author][_lte]={req.MaxAuthor.Value.ToString(CultureInfo.InvariantCulture)}");

        if (req.MinGold.HasValue && req.MaxGold.HasValue)
            queries.Add(
                $"filter[time_gold][_between]={req.MinGold.Value.ToString(CultureInfo.InvariantCulture)},{req.MaxGold.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MinGold.HasValue)
            queries.Add($"filter[time_gold][_gte]={req.MinGold.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MaxGold.HasValue)
            queries.Add($"filter[time_gold][_lte]={req.MaxGold.Value.ToString(CultureInfo.InvariantCulture)}");

        if (req.MinSilver.HasValue && req.MaxSilver.HasValue)
            queries.Add(
                $"filter[time_silver][_between]={req.MinSilver.Value.ToString(CultureInfo.InvariantCulture)},{req.MaxSilver.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MinSilver.HasValue)
            queries.Add($"filter[time_silver][_gte]={req.MinSilver.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MaxSilver.HasValue)
            queries.Add($"filter[time_silver][_lte]={req.MaxSilver.Value.ToString(CultureInfo.InvariantCulture)}");

        if (req.MinBronze.HasValue && req.MaxBronze.HasValue)
            queries.Add(
                $"filter[time_bronze][_between]={req.MinBronze.Value.ToString(CultureInfo.InvariantCulture)},{req.MaxBronze.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MinBronze.HasValue)
            queries.Add($"filter[time_bronze][_gte]={req.MinBronze.Value.ToString(CultureInfo.InvariantCulture)}");
        else if (req.MaxBronze.HasValue)
            queries.Add($"filter[time_bronze][_lte]={req.MaxBronze.Value.ToString(CultureInfo.InvariantCulture)}");
    }
}
