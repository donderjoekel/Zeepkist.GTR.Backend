using FluentResults;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Backend.RemoteStorage;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Media;

public interface IMediaService : IBasicService<RecordMedia>
{
    bool HasGhost(int recordId);
    Task<Result<string>> UploadGhost(int recordId, string b64, string identifier);
    Task<Result> DeleteGhost(int recordId);
}

public class MediaService : BasicService<RecordMedia>, IMediaService
{
    private const string GhostsFolder = "ghosts";
    private const string GhostsExtension = ".ghost";
    private const string GhostsContentType = "application/octet-stream";

    private readonly IMediaRepository _repository;
    private readonly IRemoteStorageService _remoteStorageService;

    public MediaService(IMediaRepository repository, IRemoteStorageService remoteStorageService)
        : base(repository)
    {
        _repository = repository;
        _remoteStorageService = remoteStorageService;
    }

    public bool HasGhost(int recordId)
    {
        return _repository.HasGhost(recordId);
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
            media => media.IdRecord == recordId,
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

    public async Task<Result> DeleteGhost(int recordId)
    {
        RecordMedia? recordMedia = _repository.GetSingle(x => x.IdRecord == recordId);
        if (recordMedia == null)
        {
            return Result.Ok();
        }

        Result result =
            await _remoteStorageService.Delete(recordMedia.GhostUrl.Replace("https://cdn.zeepkist-gtr.com/",
                string.Empty));
        return result.IsFailed ? result : Result.OkIf(_repository.Delete(recordMedia), "Failed to delete RecordMedia");
    }
}
