using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Rabbit;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.Rabbit;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class Endpoint : Endpoint<RequestModel, ResponseModel>
{
    private readonly IDirectusClient client;
    private readonly GTRContext context;
    private readonly IRabbitPublisher publisher;

    public Endpoint(
        IDirectusClient client,
        GTRContext context,
        IRabbitPublisher publisher
    )
    {
        this.client = client;
        this.context = context;
        this.publisher = publisher;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("records/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(b => b.ExcludeFromDescription());
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
                IsBest = false,
                IsWorldRecord = false,
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
                IsBest = false,
                IsWorldRecord = false,
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

        publisher.Publish("pb",
            new ProcessPersonalBestRequest
            {
                Record = data.Id,
                User = userId,
                Level = req.Level,
                Time = data.Time
            });

        publisher.Publish("wr",
            new ProcessWorldRecordRequest()
            {
                Record = data.Id,
                User = userId,
                Level = req.Level,
                Time = data.Time
            });
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
}
