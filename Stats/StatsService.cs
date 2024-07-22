using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Levels;
using TNRD.Zeepkist.GTR.Backend.Results;
using TNRD.Zeepkist.GTR.Backend.Stats.Resources;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

public interface IStatsService
{
    Result Submit(ulong steamId, StatsResource stats);
}

public class StatsService : IStatsService
{
    private readonly IStatsDailyRepository _dailyRepository;
    private readonly IStatsWeeklyRepository _weeklyRepository;
    private readonly IStatsMonthlyRepository _monthlyRepository;
    private readonly IStatsQuarterlyRepository _quarterlyRepository;
    private readonly IStatsYearlyRepository _yearlyRepository;
    private readonly IStatsGlobalRepository _globalRepository;
    private readonly IUserService _userService;
    private readonly ILevelService _levelService;

    public StatsService(
        IStatsDailyRepository dailyRepository,
        IStatsWeeklyRepository weeklyRepository,
        IStatsMonthlyRepository monthlyRepository,
        IStatsQuarterlyRepository quarterlyRepository,
        IStatsYearlyRepository yearlyRepository,
        IStatsGlobalRepository globalRepository,
        IUserService userService,
        ILevelService levelService)
    {
        _dailyRepository = dailyRepository;
        _weeklyRepository = weeklyRepository;
        _monthlyRepository = monthlyRepository;
        _quarterlyRepository = quarterlyRepository;
        _yearlyRepository = yearlyRepository;
        _globalRepository = globalRepository;
        _userService = userService;
        _levelService = levelService;
    }

    public Result Submit(ulong steamId, StatsResource stats)
    {
        if (!_userService.TryGet(steamId, out User? user))
        {
            return Result.Fail(new UserNotFoundError());
        }

        if (!_levelService.TryGetByHash(stats.Level, out Level? level))
        {
            level = _levelService.Create(stats.Level);
        }

        _dailyRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        _weeklyRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        _monthlyRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        _quarterlyRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        _yearlyRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        _globalRepository.AddOrUpdate(user.Id, level.Id, stats.Data);
        return Result.Ok();
    }
}
