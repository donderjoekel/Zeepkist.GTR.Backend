using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Google;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Add;

internal class Endpoint : Endpoint<LevelsAddRequestDTO, GenericIdResponseDTO>
{
    private static readonly ConcurrentDictionary<string, AutoResetEvent> uidToAutoResetEvent =
        new ConcurrentDictionary<string, AutoResetEvent>();

    private readonly IDirectusClient client;
    private readonly IGoogleUploadService googleUploadService;

    public Endpoint(IDirectusClient client, IGoogleUploadService googleUploadService)
    {
        this.client = client;
        this.googleUploadService = googleUploadService;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("levels");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(b => b.ExcludeFromDescription());
    }

    private async Task<bool> AttemptGet(LevelsAddRequestDTO req, CancellationToken ct)
    {
        Result<DirectusGetMultipleResponse<LevelModel>> getResult =
            await client.Get<DirectusGetMultipleResponse<LevelModel>>(
                $"items/levels?fields=*.*&filter[uid][_eq]={HttpUtility.UrlEncode(req.Uid)}",
                ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to check if level already exists: {Result}", getResult.ToString());
            ThrowError("Unable to check if level already exists");
        }

        if (getResult.Value.HasItems)
        {
            await SendAsync(new GenericIdResponseDTO() { Id = getResult.Value.FirstItem!.Id }, cancellation: ct);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override async Task HandleAsync(LevelsAddRequestDTO req, CancellationToken ct)
    {
        Claim? userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            Logger.LogCritical("No UserId claim found!");
            ThrowError("Unable to find userid!");
        }

        int id = int.Parse(userIdClaim.Value.Split('_')[0]);

        if (await AttemptGet(req, ct))
        {
            return;
        }

        AutoResetEvent autoResetEvent = uidToAutoResetEvent.GetOrAdd(req.Uid, new AutoResetEvent(true));
        autoResetEvent.WaitOne();

        if (await AttemptGet(req, ct))
            return;

        try
        {
            Result<string> uploadThumbnailResult = string.Empty;
            if (!string.IsNullOrEmpty(req.Thumbnail))
            {
                uploadThumbnailResult = await googleUploadService.UploadThumbnail(req.Uid, req.Thumbnail, ct);
                if (uploadThumbnailResult.IsFailed)
                {
                    Logger.LogError("Unable to upload thumbnail: {Result}", uploadThumbnailResult.ToString());
                }
            }

            LevelModel postData = new LevelModel()
            {
                UniqueId = req.Uid,
                WorkshopId = req.Wid,
                Name = req.Name,
                Author = RemoveHtmlTags(req.Author),
                TimeAuthor = req.TimeAuthor,
                TimeGold = req.TimeGold,
                TimeSilver = req.TimeSilver,
                TimeBronze = req.TimeBronze,
                ThumbnailUrl = uploadThumbnailResult.IsSuccess ? uploadThumbnailResult.Value : string.Empty,
                CreatedBy = id,
                IsValid = req.IsValid
            };

            Result<DirectusPostResponse<LevelModel>> postResult =
                await client.Post<DirectusPostResponse<LevelModel>>("items/levels?fields=*.*", postData, ct);

            if (postResult.IsFailed)
            {
                Logger.LogCritical("Unable to create new level: {Result}", postResult.ToString());
                ThrowError("Unable to create new level");
            }

            await SendAsync(new GenericIdResponseDTO() { Id = postResult.Value.Data.Id }, cancellation: ct);
        }
        finally
        {
            autoResetEvent.Set();
        }
    }

    private string RemoveHtmlTags(string author)
    {
        return Regex.Replace(author, "<[^>]*>", string.Empty);
    }
}
