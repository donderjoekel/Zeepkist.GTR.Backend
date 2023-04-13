using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Ranking;

internal class Endpoint : Endpoint<UsersRankingGetRequestDTO, UsersRankingResponseDTO>
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
        Get("users/ranking");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UsersRankingGetRequestDTO req, CancellationToken ct)
    {
        if (!req.UserId.HasValue && string.IsNullOrEmpty(req.SteamId))
        {
            ThrowError("Request needs either a SteamId or UserId");
        }

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

        UsersRankingResponseDTO? responseModel = null;

        for (int i = 0; i < groupedWorldRecords.Count; i++)
        {
            IGrouping<UserModel, RecordModel> grouping = groupedWorldRecords[i];
            if (req.UserId.HasValue && grouping.Key.Id != req.UserId.Value)
                continue;
            if (!string.IsNullOrEmpty(req.SteamId) && grouping.Key.SteamId != req.SteamId)
                continue;

            responseModel = new UsersRankingResponseDTO()
            {
                Position = i + 1,
                AmountOfWorldRecords = grouping.Count()
            };
            break;
        }

        if (responseModel == null)
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            await SendAsync(responseModel, cancellation: ct);
        }
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
