using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.Backend.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Google;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Add;

internal class Endpoint : Endpoint<LevelsAddRequestDTO, GenericIdResponseDTO>
{
    private static readonly ConcurrentDictionary<string, AutoResetEvent> uidToAutoResetEvent = new();

    private readonly IGoogleUploadService googleUploadService;
    private readonly GTRContext context;

    public Endpoint(IGoogleUploadService googleUploadService, GTRContext context)
    {
        this.googleUploadService = googleUploadService;
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("levels");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(b => b.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsAddRequestDTO req, CancellationToken ct)
    {
        if (!this.TryGetUserId(out int userId))
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find userid!");
        }

        Result<Level?> getResult = await AttemptGet(req, ct);
        if (getResult.IsFailed)
        {
            Logger.LogCritical("Failed to get level. Result: {Result}", getResult);
            ThrowError("Failed to get level");
            return;
        }

        AutoResetEvent autoResetEvent = uidToAutoResetEvent.GetOrAdd(req.Uid, new AutoResetEvent(true));
        autoResetEvent.WaitOne();

        try
        {
            if (getResult.Value == null)
            {
                await CreateLevel(userId, req, ct);
            }
            else
            {
                await SendOkAsync(new GenericIdResponseDTO(getResult.Value.Id), ct);
            }
        }
        finally
        {
            autoResetEvent.Set();
        }
    }

    private async Task<Result<Level?>> AttemptGet(LevelsAddRequestDTO req, CancellationToken ct)
    {
        Level? level;

        try
        {
            level = await context.Levels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uid == req.Uid && x.Wid == req.Wid, ct);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }

        if (level == null)
            return Result.Ok();

        await SendAsync(new GenericIdResponseDTO()
            {
                Id = level.Id
            },
            cancellation: ct);

        return Result.Ok<Level?>(level);
    }

    private async Task CreateLevel(int userId, LevelsAddRequestDTO req, CancellationToken ct)
    {
        Result<Level?> getResult = await AttemptGet(req, ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Failed to get level. Result: {Result}", getResult);
            ThrowError("Failed to get level");
            return;
        }

        Result<string> uploadThumbnailResult = string.Empty;
        if (!string.IsNullOrEmpty(req.Thumbnail))
        {
            uploadThumbnailResult = await googleUploadService.UploadThumbnail(req.Uid, req.Thumbnail, ct);
            if (uploadThumbnailResult.IsFailed)
            {
                Logger.LogError("Unable to upload thumbnail: {Result}", uploadThumbnailResult.ToString());
            }
        }

        EntityEntry<Level> entry;

        try
        {
            entry = context.Levels.Add(new Level()
            {
                Uid = req.Uid,
                Wid = req.Wid,
                Name = req.Name,
                Author = RemoveHtmlTags(req.Author),
                TimeAuthor = req.TimeAuthor,
                TimeGold = req.TimeGold,
                TimeSilver = req.TimeSilver,
                TimeBronze = req.TimeBronze,
                ThumbnailUrl = uploadThumbnailResult.IsSuccess ? uploadThumbnailResult.Value : string.Empty,
                CreatedBy = userId,
                IsValid = req.IsValid
            });
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Unable to add level to database!");
            ThrowError("Unable to add level to database!");
            return;
        }

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Unable to save level to database!");
            ThrowError("Unable to save level to database!");
            return;
        }

        await SendOkAsync(new GenericIdResponseDTO()
            {
                Id = entry.Entity.Id
            },
            ct);
    }

    private async Task UpdateThumbnailForLevel(Level level, LevelsAddRequestDTO req, CancellationToken ct)
    {
        Result<string> uploadThumbnailResult = await googleUploadService.UploadThumbnail(req.Uid, req.Thumbnail, ct);
        if (uploadThumbnailResult.IsFailed)
        {
            Logger.LogError("Unable to upload thumbnail: {Result}", uploadThumbnailResult.ToString());
            return;
        }

        level.ThumbnailUrl = uploadThumbnailResult.Value;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Logger.LogCritical("Unable to save level to database! {Exception}", e);
        }

        await SendOkAsync(new GenericIdResponseDTO()
            {
                Id = level.Id
            },
            ct);
    }

    private string RemoveHtmlTags(string author)
    {
        return Regex.Replace(author, "<[^>]*>", string.Empty);
    }
}
