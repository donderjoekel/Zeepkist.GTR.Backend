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

        List<int> levelIds = _levelService.GetAllIds().ToList();
        List<LevelItem> levelItems = _levelItemsService.GetAll().ToList();
        List<PersonalBestGlobal> allPersonalBests = _personalBestsService.GetAll().ToList();

        Dictionary<int, LevelPoints> existingLevelPoints = _levelPointsService.GetAll()
            .ToDictionary(x => x.IdLevel, x => x);
        List<LevelPoints> newLevelPoints = new();

        foreach (int levelId in levelIds)
        {
            int points = allPersonalBests.Count(x => x.IdLevel == levelId);
            if (points < 8)
                points = 0;

            if (levelItems.All(x => x.IdLevel != levelId))
                points = 0;

            if (existingLevelPoints.TryGetValue(levelId, out LevelPoints? levelPoints))
            {
                levelPoints.Points = points;
            }
            else
            {
                newLevelPoints.Add(
                    new LevelPoints
                    {
                        IdLevel = levelId,
                        Points = points
                    });
            }

            _logger.LogInformation("Updated level {LevelId} with {Points} points", levelId, points);
        }

        if (newLevelPoints.Count > 0)
            _levelPointsService.AddRange(newLevelPoints);
        if (existingLevelPoints.Count > 0)
            _levelPointsService.UpdateRange(existingLevelPoints.Values);

        _logger.LogInformation("Finished calculating level points");
        return Task.CompletedTask;
    }
}
