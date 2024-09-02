using System.Diagnostics.CodeAnalysis;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.RemoteStorage;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Items;

public interface ILevelItemsService
{
    IEnumerable<LevelItem> GetAll();
    IEnumerable<LevelItem> GetForPublishedFileDetails(PublishedFileDetails publishedFileDetails);
    bool ExistsForLevel(int levelId);
    bool Exists(PublishedFileDetails publishedFileDetails, ZeepLevel zeepLevel, string hash);

    void Create(
        PublishedFileDetails publishedFileDetails,
        WorkshopLevel workshopLevel,
        ZeepLevel zeepLevel,
        Level level);

    void MarkDeleted(LevelItem levelItem);
}

public class LevelItemsService : ILevelItemsService
{
    private readonly ILevelItemsRepository _repository;
    private readonly ILevelService _levelService;
    private readonly ILogger<LevelItemsService> _logger;
    private readonly IRemoteStorageService _remoteStorageService;

    public LevelItemsService(
        ILevelItemsRepository repository,
        ILevelService levelService,
        ILogger<LevelItemsService> logger,
        IRemoteStorageService remoteStorageService)
    {
        _repository = repository;
        _levelService = levelService;
        _logger = logger;
        _remoteStorageService = remoteStorageService;
    }

    public IEnumerable<LevelItem> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<LevelItem> GetForPublishedFileDetails(PublishedFileDetails publishedFileDetails)
    {
        if (ulong.TryParse(publishedFileDetails.PublishedFileId, out ulong publishedFileId))
        {
            return _repository.GetAll(item => item.Deleted == false && item.WorkshopId == publishedFileId);
        }

        return Enumerable.Empty<LevelItem>();
    }

    public bool ExistsForLevel(int levelId)
    {
        return _repository.Exists(x => x.IdLevel == levelId);
    }

    public bool Exists(PublishedFileDetails publishedFileDetails, ZeepLevel zeepLevel, string hash)
    {
        if (!_levelService.TryGetByHash(hash, out Level? level))
        {
            return false;
        }

        return _repository.Exists(
            level.Id,
            decimal.Parse(publishedFileDetails.PublishedFileId),
            decimal.Parse(publishedFileDetails.Creator),
            zeepLevel.UniqueId);
    }

    private bool Exists(
        PublishedFileDetails publishedFileDetails,
        ZeepLevel zeepLevel,
        string hash,
        [NotNullWhen(true)] out Level? level)
    {
        if (!_levelService.TryGetByHash(hash, out level))
        {
            _logger.LogWarning("Unable to create level item because there is no level with hash {Hash}", hash);
            return false;
        }

        return _repository.Exists(
            level.Id,
            decimal.Parse(publishedFileDetails.PublishedFileId),
            decimal.Parse(publishedFileDetails.Creator),
            zeepLevel.UniqueId);
    }

    public void Create(
        PublishedFileDetails publishedFileDetails,
        WorkshopLevel workshopLevel,
        ZeepLevel zeepLevel,
        Level level)
    {
        if (_repository.Exists(
                level.Id,
                decimal.Parse(publishedFileDetails.PublishedFileId),
                decimal.Parse(publishedFileDetails.Creator),
                zeepLevel.UniqueId))
        {
            return;
        }

        byte[] buffer = File.ReadAllBytes(workshopLevel.ThumbnailPath);
        Result<string> uploadResult = _remoteStorageService
            .UploadImage(buffer, "thumbnails", Guid.NewGuid().ToString(), ".jpg")
            .GetAwaiter().GetResult();

        if (uploadResult.IsFailed)
        {
            _logger.LogError("Failed to upload level thumbnail: {Result}", uploadResult.ToString());
        }

        LevelItem item = new()
        {
            IdLevel = level.Id,
            WorkshopId = decimal.Parse(publishedFileDetails.PublishedFileId),
            AuthorId = decimal.Parse(publishedFileDetails.Creator),
            Name = Path.GetFileNameWithoutExtension(workshopLevel.ZeeplevelPath),
            ImageUrl = uploadResult.IsSuccess ? uploadResult.Value : string.Empty,
            FileAuthor = zeepLevel.PlayerName,
            FileUid = zeepLevel.UniqueId,
            ValidationTimeAuthor = zeepLevel.ValidationTime,
            ValidationTimeGold = zeepLevel.GoldTime,
            ValidationTimeSilver = zeepLevel.SilverTime,
            ValidationTimeBronze = zeepLevel.BronzeTime,
            Deleted = false,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(publishedFileDetails.TimeCreated).UtcDateTime,
            UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(publishedFileDetails.TimeUpdated).UtcDateTime
        };

        _repository.Insert(item);
    }

    public void MarkDeleted(LevelItem levelItem)
    {
        levelItem.Deleted = true;
        _repository.Update(levelItem);
    }
}
