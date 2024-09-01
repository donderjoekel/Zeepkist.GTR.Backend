using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Points;

public interface ILevelPointsService
{
    IEnumerable<LevelPoints> GetAll();
    void AddRange(IEnumerable<LevelPoints> levelPoints);
    void UpdateRange(IEnumerable<LevelPoints> levelPoints);
    void Update(int levelId, int points);
}

public class LevelPointsService : ILevelPointsService
{
    private readonly ILevelPointsRepository _repository;

    public LevelPointsService(ILevelPointsRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<LevelPoints> GetAll()
    {
        return _repository.GetAll();
    }

    public void AddRange(IEnumerable<LevelPoints> levelPoints)
    {
        _repository.InsertRange(levelPoints);
    }

    public void UpdateRange(IEnumerable<LevelPoints> levelPoints)
    {
        _repository.UpdateRange(levelPoints);
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
