namespace TNRD.Zeepkist.GTR.Backend.Levels.Points;

public interface ILevelPointsService
{
}

public class LevelPointsService : ILevelPointsService
{
    private readonly ILevelPointsRepository _repository;

    public LevelPointsService(ILevelPointsRepository repository)
    {
        _repository = repository;
    }
}
