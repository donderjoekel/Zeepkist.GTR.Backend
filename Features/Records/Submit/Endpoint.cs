using System.Collections.Concurrent;
using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Google;
using TNRD.Zeepkist.GTR.Backend.Rabbit;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.Rabbit;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class Endpoint : Endpoint<RequestModel, ResponseModel>
{
    private class UploadDataResult
    {
        public string? GhostUrl { get; set; }
        public string? ScreenshotUrl { get; set; }
    }

    private class UpdateRecordsResult
    {
        public bool IsBest { get; set; }
        public bool IsWorldRecord { get; set; }
    }

    private static readonly ConcurrentDictionary<int, AutoResetEvent> levelIdToAutoResetEvent =
        new ConcurrentDictionary<int, AutoResetEvent>();

    private readonly IDirectusClient client;
    private readonly IGoogleUploadService googleUploadService;
    private readonly GTRContext context;
    private readonly IRabbitPublisher publisher;

    public Endpoint(
        IDirectusClient client,
        IGoogleUploadService googleUploadService,
        GTRContext context,
        IRabbitPublisher publisher
    )
    {
        this.client = client;
        this.googleUploadService = googleUploadService;
        this.context = context;
        this.publisher = publisher;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("records/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(b => b.ExcludeFromDescription());
        // PostProcessors(new TrackOfTheDayPostProcessor());
    }

    private async Task<bool> DoesRecordExist(RequestModel req, CancellationToken ct)
    {
        string joinedSplits = string.Join('|', req.Splits);

        IQueryable<Record> queryable = from r in context.Records.AsNoTracking()
            where r.User.Value == req.User &&
                  r.Level.Value == req.Level &&
                  Math.Abs(r.Time.Value - req.Time) < 0.001f &&
                  r.Splits == joinedSplits
            select r;

        Record? existingRecord = await queryable.FirstOrDefaultAsync(ct);
        if (existingRecord == null)
            return false;

        TimeSpan a = DateTime.Now - existingRecord.DateCreated!.Value;
        TimeSpan b = DateTime.UtcNow - existingRecord.DateCreated!.Value;

        return a < TimeSpan.FromMinutes(1) || b < TimeSpan.FromMinutes(1);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(RequestModel req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find user id!");
            return;
        }

        if (await this.UserIsBanned(context))
        {
            Logger.LogWarning("Banned user tried to submit record");
            ThrowError("You are banned!");
            return;
        }

        if (userId != req.User)
        {
            Logger.LogCritical("UserId claim does not match request!");
            ThrowError("User id does not match!");
            return;
        }

        if (await DoesRecordExist(req, ct))
        {
            Logger.LogWarning("Double record submission detected!");
            ThrowError("Record already exists!");
            return;
        }

        RecordModel postModel = new RecordModel()
        {
            Level = req.Level,
            User = req.User,
            Time = req.Time,
            Splits = string.Join('|', req.Splits),
            GhostUrl = string.Empty,
            ScreenshotUrl = string.Empty,
            IsValid = req.IsValid,
            IsBest = false,
            IsWorldRecord = false,
            GameVersion = req.GameVersion
        };

        Result<DirectusPostResponse<RecordModel>> result =
            await client.Post<DirectusPostResponse<RecordModel>>("items/records?fields=*.*", postModel, ct);

        if (result.IsFailed)
        {
            Logger.LogCritical("Unable to post record: {Reason}", result.ToString());
            await SendAsync(null!, 500, ct);
        }

        Result<UpdateRecordsResult> updateRecordsResult = await UpdateRecords(req, result.Value.Data, ct);

        if (updateRecordsResult.IsFailed)
        {
            await SendAsync(null!, 500, ct);
            return;
        }

        RecordModel data = result.Value.Data;

        await SendAsync(new ResponseModel()
            {
                Id = data.Id,
                Level = data.Level.AsT1.Id,
                User = data.User.AsT1.Id,
                Time = data.Time,
                Splits = string.IsNullOrEmpty(data.Splits)
                    ? Array.Empty<float>()
                    : data.Splits.Split('|').Select(float.Parse).ToArray(),
                GhostUrl = data.GhostUrl,
                ScreenshotUrl = data.ScreenshotUrl,
                IsValid = data.IsValid,
                IsBest = updateRecordsResult.Value.IsBest,
                IsWorldRecord = updateRecordsResult.Value.IsWorldRecord,
                GameVersion = data.GameVersion,
                DateCreated = data.DateCreated,
                DateUpdated = data.DateUpdated
            },
            cancellation: ct);

        publisher.Publish("records",
            new PublishableRecord
            {
                Id = data.Id,
                User = data.User.AsT1.Id,
                Level = data.Level.AsT1.Id,
                Time = data.Time,
                IsValid = data.IsValid,
                IsBest = updateRecordsResult.Value.IsBest,
                IsWorldRecord = updateRecordsResult.Value.IsWorldRecord,
                Splits = string.IsNullOrEmpty(data.Splits)
                    ? Array.Empty<float>()
                    : data.Splits.Split('|').Select(float.Parse).ToArray(),
                GhostUrl = data.GhostUrl,
                ScreenshotUrl = data.ScreenshotUrl,
            });

        publisher.Publish("media",
            new UploadRecordMediaRequest
            {
                Id = data.Id,
                GhostData = req.GhostData,
                ScreenshotData = req.ScreenshotData
            });
    }

    private async Task<Result<UpdateRecordsResult>> UpdateRecords(
        RequestModel req,
        RecordModel data,
        CancellationToken ct
    )
    {
        if (!req.IsValid)
        {
            return new UpdateRecordsResult()
            {
                IsBest = false,
                IsWorldRecord = false
            };
        }

        bool isBest = false;
        bool isWorldRecord = false;

        Result<bool> isBestResult = await UpdateBestRecord(req, data, ct);
        if (isBestResult.IsFailed)
        {
            Logger.LogCritical("Unable to update best record: {Reason}", isBestResult.ToString());
            await SendAsync(null!, 500, ct);
            return isBestResult.ToResult();
        }

        isBest = isBestResult.Value;

        Result<bool> isWorldRecordResult = await UpdateWorldRecord(req, data, ct);
        if (isWorldRecordResult.IsFailed)
        {
            Logger.LogCritical("Unable to update world record: {Reason}", isWorldRecordResult.ToString());
            await SendAsync(null!, 500, ct);
            return isWorldRecordResult.ToResult();
        }

        isWorldRecord = isWorldRecordResult.Value;

        return new UpdateRecordsResult()
        {
            IsBest = isBest,
            IsWorldRecord = isWorldRecord
        };
    }

    private async Task<Result<bool>> UpdateBestRecord(
        RequestModel req,
        RecordModel getRecordModel,
        CancellationToken ct
    )
    {
        Result<DirectusGetMultipleResponse<RecordModel>> getCurrentBestResult =
            await client.Get<DirectusGetMultipleResponse<RecordModel>>(
                $"items/records?fields=*.*&filter[user][id][_eq]={req.User}&filter[level][id][_eq]={req.Level}&filter[is_best][_eq]=true",
                ct);

        if (getCurrentBestResult.IsFailed)
        {
            Logger.LogCritical("Unable to get current best: {Result}", getCurrentBestResult.ToString());
            return getCurrentBestResult.ToResult();
        }

        if (getCurrentBestResult.Value.HasItems)
        {
            RecordModel item = getCurrentBestResult.Value.FirstItem!;

            if (item.Time < req.Time)
                return false;

            Result<DirectusPostResponse<RecordModel>> patchResult =
                await client.Patch<DirectusPostResponse<RecordModel>>($"items/records/{item.Id}",
                    RecordsFactory.New().WithIsBestRun(false).Build(),
                    ct);

            if (patchResult.IsFailed)
            {
                Logger.LogCritical("Unable to remove current best record: {Result}", patchResult.ToString());
                return patchResult.ToResult();
            }
        }

        Result<DirectusPostResponse<RecordModel>> patchNewBestResult =
            await client.Patch<DirectusPostResponse<RecordModel>>($"items/records/{getRecordModel.Id}",
                RecordsFactory.New().WithIsBestRun(true).Build(),
                ct);

        if (patchNewBestResult.IsFailed)
        {
            Logger.LogCritical("Unable to set new best record: {Result}", patchNewBestResult.ToString());
            return false;
        }

        return true;
    }

    private async Task<Result<bool>> UpdateWorldRecord(
        RequestModel req,
        RecordModel getRecordModel,
        CancellationToken ct
    )
    {
        AutoResetEvent autoResetEvent = levelIdToAutoResetEvent.GetOrAdd(req.Level, new AutoResetEvent(true));
        autoResetEvent.WaitOne();

        try
        {
            Result<DirectusGetMultipleResponse<RecordModel>> getCurrentWorldRecord =
                await client.Get<DirectusGetMultipleResponse<RecordModel>>(
                    $"items/records?fields=*.*&filter[level][id][_eq]={req.Level}&filter[is_wr][_eq]=true",
                    ct);

            if (getCurrentWorldRecord.IsFailed)
            {
                Logger.LogCritical("Unable to get current world record: {Result}", getCurrentWorldRecord.ToString());
                return getCurrentWorldRecord.ToResult();
            }

            if (getCurrentWorldRecord.Value.HasItems)
            {
                RecordModel item = getCurrentWorldRecord.Value.FirstItem!;

                if (item.Time < req.Time)
                    return false;

                Result<DirectusPostResponse<RecordModel>> patchResult =
                    await client.Patch<DirectusPostResponse<RecordModel>>($"items/records/{item.Id}",
                        RecordsFactory.New().WithIsWorldRecord(false).Build(),
                        ct);

                if (patchResult.IsFailed)
                {
                    Logger.LogCritical("Unable to remove current world record: {Result}", patchResult.ToString());
                    return patchResult.ToResult();
                }
            }

            Result<DirectusPostResponse<RecordModel>> patchNewWorldRecordResult =
                await client.Patch<DirectusPostResponse<RecordModel>>($"items/records/{getRecordModel.Id}",
                    new RecordsFactory().WithIsWorldRecord(true).Build(),
                    ct);

            if (patchNewWorldRecordResult.IsFailed)
            {
                Logger.LogCritical("Unable to set new best record: {Result}", patchNewWorldRecordResult.ToString());
                return false;
            }

            return true;
        }
        finally
        {
            autoResetEvent.Set();
        }
    }
}
