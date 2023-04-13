using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Get.Average;

internal class Endpoint : Endpoint<VotesGetAverageRequestDTO, VotesGetAverageResponseDTO>
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
        Get("votes/average");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VotesGetAverageRequestDTO req, CancellationToken ct)
    {
        string queryString = CreateQueryString(req);

        Result<DirectusGetMultipleResponse<AverageVoteModel>> result =
            await client.Get<DirectusGetMultipleResponse<AverageVoteModel>>($"items/votes?{queryString}", ct);

        if (result.IsFailed)
        {
            Logger.LogCritical("Unable to get votes: {Result}", result.ToString());
            await SendAsync(null!, 500, ct);
            return;
        }

        List<VotesGetAverageResponseDTO.AverageLevelScore> averageLevelScores = new List<VotesGetAverageResponseDTO.AverageLevelScore>();

        foreach (AverageVoteModel item in result.Value.Data)
        {
            VotesGetAverageResponseDTO.AverageLevelScore averageLevelScore = new VotesGetAverageResponseDTO.AverageLevelScore()
            {
                AverageScore = item.AverageScore.Value,
                Category = item.Category,
                Level = item.Level,
                AmountOfVotes = item.Count.Value
            };

            averageLevelScores.Add(averageLevelScore);
        }

        VotesGetAverageResponseDTO responseModel = new VotesGetAverageResponseDTO()
        {
            AverageScores = averageLevelScores
        };

        await SendAsync(responseModel, cancellation: ct);
    }

    private static string CreateQueryString(VotesGetAverageRequestDTO req)
    {
        List<string> queries = new List<string>();

        queries.Add("aggregate[avg]=score");
        queries.Add("aggregate[count]=id");
        queries.Add("groupBy[]=level");
        queries.Add("groupBy[]=category");

        if (req.LevelId.HasValue)
            queries.Add($"filter[level][id][_eq]={req.LevelId.Value}");

        queries.Add(req.Offset.HasValue
            ? $"offset={req.Offset}"
            : "offset=0");

        queries.Add(req.Limit.HasValue
            ? $"limit={Math.Min(req.Limit.Value, 100).ToString()}"
            : "limit=100");

        string queryString = string.Join('&', queries);
        return queryString;
    }
}
