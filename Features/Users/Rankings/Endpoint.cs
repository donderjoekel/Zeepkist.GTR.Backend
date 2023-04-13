using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Rankings;

internal class Endpoint : Endpoint<GenericGetRequestDTO, UsersRankingsResponseDTO>
{
    private readonly IDirectusClient client;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users/rankings");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        Result<List<RecordModel>> getAllWorldRecordsResult = await GetAllWorldRecords(ct);

        if (getAllWorldRecordsResult.IsFailed)
        {
            Logger.LogCritical("Unable to get all world record levels: {Result}", getAllWorldRecordsResult.ToString());
            await SendAsync(null!, 500, ct);
            return;
        }

        List<IGrouping<UserModel, RecordModel>> groupedWorldRecords =
            getAllWorldRecordsResult.Value
                .GroupBy(x => x.User.AsT1)
                .OrderByDescending(x => x.Count())
                .ToList();

        List<UsersRankingsResponseDTO.Ranking> rankings = new List<UsersRankingsResponseDTO.Ranking>();

        for (int i = 0; i < groupedWorldRecords.Count; i++)
        {
            IGrouping<UserModel, RecordModel> grouping = groupedWorldRecords[i];

            rankings.Add(new UsersRankingsResponseDTO.Ranking()
            {
                User = new UsersRankingsResponseDTO.UserModel()
                {
                    Id = grouping.Key.Id,
                    SteamId = grouping.Key.SteamId,
                    SteamName = grouping.Key.SteamName
                },
                Position = i + 1,
                AmountOfWorldRecords = grouping.Count()
            });
        }

        int limit = req.Limit.HasValue ? req.Limit.Value : 50;
        int offset = req.Offset.HasValue ? req.Offset.Value : 0;

        if (offset > rankings.Count)
        {
            await SendAsync(new UsersRankingsResponseDTO()
                {
                    TotalAmount = rankings.Count,
                    Rankings = new List<UsersRankingsResponseDTO.Ranking>()
                },
                cancellation: ct);

            return;
        }

        int max = limit + offset;
        if (max > rankings.Count)
        {
            limit -= max - rankings.Count;
        }

        List<UsersRankingsResponseDTO.Ranking> ranged = rankings.GetRange(offset, limit);

        UsersRankingsResponseDTO responseModel = new UsersRankingsResponseDTO()
        {
            Rankings = ranged,
            TotalAmount = rankings.Count
        };

        await SendAsync(responseModel, cancellation: ct);
    }

    private async Task<Result<List<RecordModel>>> GetAllWorldRecords(CancellationToken ct)
    {
        List<RecordModel> worldRecords = new List<RecordModel>();

        int offset = 0;
        int limit = 100;

        while (true)
        {
            Result<DirectusGetMultipleResponse<RecordModel>> getResult =
                await client.Get<DirectusGetMultipleResponse<RecordModel>>(
                    $"items/records?fields=*.*&filter[is_wr][_eq]=true&offset={offset}&limit={limit}&meta=filter_count",
                    ct);

            if (getResult.IsFailed)
                return getResult.ToResult();

            if (!getResult.Value.HasItems)
                break;

            worldRecords.AddRange(getResult.Value.Data);
            offset += limit;
        }

        return worldRecords;
    }
}
