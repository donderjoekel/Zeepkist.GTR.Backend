using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels;
using TNRD.Zeepkist.GTR.Backend.Media;
using TNRD.Zeepkist.GTR.Backend.Media.Jobs;
using TNRD.Zeepkist.GTR.Backend.PersonalBests.Jobs;
using TNRD.Zeepkist.GTR.Backend.Records.Resources;
using TNRD.Zeepkist.GTR.Backend.Results;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Backend.WorldRecords.Jobs;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records;

public interface IRecordsService
{
    IEnumerable<Record> GetAll();
    Record? GetById(int id);
    Result Submit(ulong steamId, RecordResource resource);
}

public class RecordsService : IRecordsService
{
    private readonly ILogger<RecordsService> _logger;
    private readonly IRecordsRepository _repository;
    private readonly ILevelService _levelService;
    private readonly IJobScheduler _jobScheduler;
    private readonly IUserService _userService;
    private readonly IMediaService _mediaService;

    public RecordsService(
        ILogger<RecordsService> logger,
        IRecordsRepository repository,
        ILevelService levelService,
        IJobScheduler jobScheduler,
        IUserService userService,
        IMediaService mediaService)
    {
        _logger = logger;
        _repository = repository;
        _levelService = levelService;
        _jobScheduler = jobScheduler;
        _userService = userService;
        _mediaService = mediaService;
    }

    public IEnumerable<Record> GetAll()
    {
        return _repository.GetAll();
    }

    public Record? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public Result Submit(ulong steamId, RecordResource resource)
    {
        if (!_userService.TryGet(steamId, out User? user))
        {
            return Result.Fail(new UserNotFoundError());
        }

        if (!_levelService.TryGetByHash(resource.Level, out Level? level))
        {
            level = _levelService.Create(resource.Level);
        }

        DateTime now = DateTime.UtcNow;

        Record record = _repository.Insert(
            new Record()
            {
                IdUser = user.Id,
                IdLevel = level.Id,
                Time = resource.Time,
                Splits = resource.Splits,
                Speeds = resource.Speeds,
                IsValid = resource.IsValid,
                GameVersion = resource.GameVersion,
                ModVersion = resource.ModVersion,
                DateCreated = now,
            });

        UploadMediaJob.Schedule(_jobScheduler, record.Id, resource.GhostData, resource.ScreenshotData);
        ProcessPersonalBestJob.Schedule(_jobScheduler, record.Id, record.IdUser, record.IdLevel);
        ProcessWorldRecordJob.Schedule(_jobScheduler, record.Id, record.IdLevel);
        return Result.Ok();
    }
}
