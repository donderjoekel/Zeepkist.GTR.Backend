using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Rabbit;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.Rabbit;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class Endpoint : Endpoint<RequestModel>
{
    private readonly GTRContext context;
    private readonly IRabbitPublisher publisher;

    public Endpoint(GTRContext context, IRabbitPublisher publisher)
    {
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

        EntityEntry<Record> entry = context.Records.Add(new Record()
        {
            Level = req.Level,
            User = req.User,
            Time = req.Time,
            Splits = string.Join('|', req.Splits),
            GhostUrl = string.Empty,
            ScreenshotUrl = string.Empty,
            IsValid = req.IsValid,
            IsBest = false,
            IsWr = false,
            GameVersion = req.GameVersion,
            DateCreated = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);

        publisher.Publish("records",
            new RecordId
            {
                Id = entry.Entity.Id
            });

        publisher.Publish("media",
            new UploadRecordMediaRequest
            {
                Id = entry.Entity.Id,
                GhostData = req.GhostData,
                ScreenshotData = req.ScreenshotData
            });

        publisher.Publish("pb",
            new ProcessPersonalBestRequest
            {
                Record = entry.Entity.Id,
                User = userId,
                Level = req.Level,
                Time = entry.Entity.Time!.Value
            });

        publisher.Publish("wr",
            new ProcessWorldRecordRequest()
            {
                Record = entry.Entity.Id,
                User = userId,
                Level = req.Level,
                Time = entry.Entity.Time!.Value
            });

        await SendOkAsync(ct);
    }

    private async Task<bool> DoesRecordExist(RequestModel req, CancellationToken ct)
    {
        string joinedSplits = string.Join('|', req.Splits);

        Record? existingRecord = await context.Records.AsNoTracking()
            .Where(r => r.User == req.User && r.Level == req.Level && Math.Abs(r.Time!.Value - req.Time) < 0.001f &&
                        r.Splits == joinedSplits)
            .FirstOrDefaultAsync(ct);

        if (existingRecord == null)
            return false;

        TimeSpan a = DateTime.Now - existingRecord.DateCreated!.Value;
        TimeSpan b = DateTime.UtcNow - existingRecord.DateCreated!.Value;

        return a < TimeSpan.FromMinutes(1) || b < TimeSpan.FromMinutes(1);
    }
}
