using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Points;

public interface ILevelPointsService
{
    void Update(int levelId, int points);
}

public class LevelPointsService : ILevelPointsService
{
    private readonly ILevelPointsRepository _repository;

    public LevelPointsService(ILevelPointsRepository repository)
    {
        _repository = repository;
    }

    public void Update(int levelId, int points)
    {
        _repository.Upsert(
            levelPoints => levelPoints.IdLevel == levelId,
            () => new LevelPoints()
            {
                IdLevel = levelId,
                Points = points
            },
            levelPoints =>
            {
                levelPoints.Points = points;
                return levelPoints;
            });
    }
}
