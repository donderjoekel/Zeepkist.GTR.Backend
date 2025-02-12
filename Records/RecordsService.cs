using System.Threading.Channels;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels;
using TNRD.Zeepkist.GTR.Backend.Records.Requests;
using TNRD.Zeepkist.GTR.Backend.Records.Resources;
using TNRD.Zeepkist.GTR.Backend.Results;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records;

public interface IRecordsService : IBasicService<Record>
{
    IEnumerable<Record> GetByLevelId(int levelId);
    IEnumerable<Record> GetTop(int amount, int userId, int levelId);
    IEnumerable<Record> GetByUserId(int userId);
    Record? GetBest(int levelId);
    Record? GetBestForUser(int userId, int levelId);
    Task<Result> Submit(ulong steamId, RecordResource resource);
}

public class RecordsService : BasicService<Record>, IRecordsService
{
    private readonly ILogger<RecordsService> _logger;
    private readonly IRecordsRepository _repository;
    private readonly ILevelService _levelService;
    private readonly IJobScheduler _jobScheduler;
    private readonly IUserService _userService;
    private readonly Channel<ProcessWorldRecordRequest> _worldRecordChannel;
    private readonly Channel<ProcessPersonalBestRequest> _personalBestChannel;
    private readonly Channel<ProcessRecordMediaRequest> _recordMediaChannel;

    public RecordsService(
        ILogger<RecordsService> logger,
        IRecordsRepository repository,
        ILevelService levelService,
        IJobScheduler jobScheduler,
        IUserService userService,
        Channel<ProcessWorldRecordRequest> worldRecordChannel,
        Channel<ProcessPersonalBestRequest> personalBestChannel,
        Channel<ProcessRecordMediaRequest> recordMediaChannel)
        : base(repository)
    {
        _logger = logger;
        _repository = repository;
        _levelService = levelService;
        _jobScheduler = jobScheduler;
        _userService = userService;
        _worldRecordChannel = worldRecordChannel;
        _personalBestChannel = personalBestChannel;
        _recordMediaChannel = recordMediaChannel;
    }

    public IEnumerable<Record> GetByLevelId(int levelId)
    {
        return _repository.GetByLevelId(levelId);
    }

    public IEnumerable<Record> GetTop(int amount, int userId, int levelId)
    {
        return _repository
            .GetAll(x => x.IdUser == userId && x.IdLevel == levelId)
            .OrderBy(x => x.Time)
            .Take(amount);
    }

    public IEnumerable<Record> GetByUserId(int userId)
    {
        return _repository
            .GetAll(x => x.IdUser == userId, set => set.Include(x => x.RecordMedia));
    }

    public Record? GetBest(int levelId)
    {
        return _repository.GetBest(levelId);
    }

    public Record? GetBestForUser(int userId, int levelId)
    {
        return _repository.GetBestForUser(userId, levelId);
    }

    public async Task<Result> Submit(ulong steamId, RecordResource resource)
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
            new Record
            {
                IdUser = user.Id,
                IdLevel = level.Id,
                Time = resource.Time,
                Splits = resource.Splits,
                Speeds = resource.Speeds,
                GameVersion = resource.GameVersion,
                ModVersion = resource.ModVersion,
                DateCreated = now,
            });

        await _personalBestChannel.Writer.WriteAsync(new ProcessPersonalBestRequest(record));
        await _worldRecordChannel.Writer.WriteAsync(new ProcessWorldRecordRequest(record));
        await _recordMediaChannel.Writer.WriteAsync(new ProcessRecordMediaRequest(record, resource.GhostData));

        return Result.Ok();
    }
}
