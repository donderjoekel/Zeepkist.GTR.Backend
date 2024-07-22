using FluentResults;
using TNRD.Zeepkist.GTR.Backend.RemoteStorage;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Media;

public interface IMediaService
{
    bool HasGhost(int recordId);
    bool HasScreenshot(int recordId);

    Task<Result<string>> UploadGhost(int recordId, string b64, string identifier);
    Task<Result<string>> UploadScreenshot(int recordId, string b64, string identifier);
}

public class MediaService : IMediaService
{
    private const string GhostsFolder = "ghosts";
    private const string GhostsExtension = "bin";
    private const string GhostsContentType = "application/octet-stream";

    private const string ScreenshotsFolder = "screenshots";
    private const string ScreenshotsExtension = "jpeg";
    private const string ScreenshotsContentType = "image/jpeg";

    private readonly IMediaRepository _repository;
    private readonly IRemoteStorageService _remoteStorageService;

    public MediaService(IMediaRepository repository, IRemoteStorageService remoteStorageService)
    {
        _repository = repository;
        _remoteStorageService = remoteStorageService;
    }

    public bool HasGhost(int recordId)
    {
        return _repository.HasGhost(recordId);
    }

    public bool HasScreenshot(int recordId)
    {
        return _repository.HasScreenshot(recordId);
    }

    public async Task<Result<string>> UploadGhost(int recordId, string b64, string identifier)
    {
        Result<string> result = await _remoteStorageService.Upload(
            b64,
            GhostsFolder,
            identifier,
            GhostsExtension,
            GhostsContentType,
            true);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        RecordMedia recordMedia = _repository.Upsert(
            recordId,
            () => new RecordMedia
            {
                IdRecord = recordId,
                GhostUrl = result.Value,
            },
            media =>
            {
                media.GhostUrl = result.Value;
                return media;
            });

        return Result.Ok(recordMedia.GhostUrl);
    }

    public async Task<Result<string>> UploadScreenshot(int recordId, string b64, string identifier)
    {
        Result<string> result = await _remoteStorageService.Upload(
            b64,
            ScreenshotsFolder,
            identifier,
            ScreenshotsExtension,
            ScreenshotsContentType,
            false);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        RecordMedia recordMedia = _repository.Upsert(
            recordId,
            () => new RecordMedia
            {
                IdRecord = recordId,
                ScreenshotUrl = result.Value,
            },
            media =>
            {
                media.ScreenshotUrl = result.Value;
                return media;
            });

        return Result.Ok(recordMedia.ScreenshotUrl);
    }
}
