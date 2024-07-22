using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests;

public interface ILevelRequestsService
{
    IEnumerable<LevelRequest> GetRequests();
    void Delete(LevelRequest request);
}

public class LevelRequestsService : ILevelRequestsService
{
    private readonly ILevelRequestsRepository _repository;

    public LevelRequestsService(ILevelRequestsRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<LevelRequest> GetRequests()
    {
        return _repository.GetAll();
    }

    public void Delete(LevelRequest request)
    {
        _repository.Delete(request);
    }
}
