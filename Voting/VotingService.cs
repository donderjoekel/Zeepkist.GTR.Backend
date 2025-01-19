using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Levels;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Voting;

public interface IVotingService
{
    Result Upvote(ulong steamId, string levelHash);
    Result Downvote(ulong steamId, string levelHash);
    Result DoubleUpvote(ulong steamId, string levelHash);
    Result DoubleDownvote(ulong steamId, string levelHash);
}

public class VotingService : IVotingService
{
    private readonly ILogger<VotingService> _logger;
    private readonly IVotingRepository _repository;
    private readonly IUserService _userService;
    private readonly ILevelService _levelService;

    public VotingService(ILogger<VotingService> logger, IVotingRepository repository, IUserService userService,
        ILevelService levelService)
    {
        _logger = logger;
        _repository = repository;
        _userService = userService;
        _levelService = levelService;
    }

    public Result Upvote(ulong steamId, string levelHash)
    {
        return Upsert(steamId, levelHash, 1);
    }

    public Result Downvote(ulong steamId, string levelHash)
    {
        return Upsert(steamId, levelHash, -1);
    }

    public Result DoubleUpvote(ulong steamId, string levelHash)
    {
        return Upsert(steamId, levelHash, 2);
    }

    public Result DoubleDownvote(ulong steamId, string levelHash)
    {
        return Upsert(steamId, levelHash, -2);
    }

    private Result Upsert(ulong steamId, string levelHash, int value)
    {
        if (!_userService.TryGet(steamId, out User? user))
        {
            _logger.LogWarning("Unable to get user with steam id {SteamId}", steamId);
            return Result.Fail($"Unable to get user with steam id '{steamId}'");
        }

        if (!_levelService.TryGetByHash(levelHash, out Level? level))
        {
            _logger.LogWarning("Unable to get level with hash '{Hash}'", levelHash);
            return Result.Fail($"Unable to get level with hash '{levelHash}'");
        }

        Vote vote = _repository.Upsert(vote => vote.IdUser == user.Id && vote.IdLevel == level.Id,
            () => new Vote
            {
                IdUser = user.Id,
                IdLevel = level.Id,
                Value = value
            },
            vote =>
            {
                vote.Value = value;
                return vote;
            });

        return Result.OkIf(vote.IdUser == user.Id &&
                           vote.IdLevel == level.Id &&
                           vote.Value == value,
            "Failed to upsert vote");
    }
}
