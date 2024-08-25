using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Levels.Items;
using TNRD.Zeepkist.GTR.Backend.PersonalBests;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Points.Jobs;

public class CalculateLevelPointsJob
{
    private readonly ILevelPointsService _levelPointsService;
    private readonly IPersonalBestsService _personalBestsService;
    private readonly ILevelService _levelService;
    private readonly ILevelItemsService _levelItemsService;
    private readonly ILogger<CalculateLevelPointsJob> _logger;

    public CalculateLevelPointsJob(
        ILevelPointsService levelPointsService,
        IPersonalBestsService personalBestsService,
        ILevelService levelService,
        ILevelItemsService levelItemsService,
        ILogger<CalculateLevelPointsJob> logger)
    {
        _levelPointsService = levelPointsService;
        _personalBestsService = personalBestsService;
        _levelService = levelService;
        _levelItemsService = levelItemsService;
        _logger = logger;
    }

    [UsedImplicitly]
    public Task ExecuteAsync()
    {
        _logger.LogInformation("Calculating level points");

        List<int> levelIds = new();

        foreach (int levelId in _levelService.GetAllIds())
        {
            if (_levelItemsService.ExistsForLevel(levelId))
                levelIds.Add(levelId);
        }

        foreach (int levelId in levelIds)
        {
            IEnumerable<PersonalBestGlobal> personalBests = _personalBestsService.GetForLevel(levelId);
            int levelPoints = personalBests.Count();
            if (levelPoints < 8)
                levelPoints = 0;

            _levelPointsService.Update(levelId, levelPoints);
            _logger.LogInformation("Updated level {LevelId} with {LevelPoints} points", levelId, levelPoints);
        }

        _logger.LogInformation("Finished calculating level points");
        return Task.CompletedTask;
    }
}
