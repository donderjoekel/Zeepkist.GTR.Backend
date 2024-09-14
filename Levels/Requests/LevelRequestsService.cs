using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests;

public interface ILevelRequestsService
{
    void Add(ulong workshopId);
    IEnumerable<LevelRequest> GetRequests();
    void Delete(LevelRequest request);
    void Delete(IEnumerable<LevelRequest> requests);
}

public class LevelRequestsService : ILevelRequestsService
{
    private readonly ILevelRequestsRepository _repository;

    public LevelRequestsService(ILevelRequestsRepository repository)
    {
        _repository = repository;
    }

    public void Add(ulong workshopId)
    {
        LevelRequest? existing = _repository.GetSingle(x => x.WorkshopId == workshopId);
        if (existing == null)
        {
            _repository.Insert(new LevelRequest()
            {
                WorkshopId = workshopId
            });
        }
    }

    public IEnumerable<LevelRequest> GetRequests()
    {
        return _repository.GetAll();
    }

    public void Delete(LevelRequest request)
    {
        _repository.Delete(request);
    }

    public void Delete(IEnumerable<LevelRequest> requests)
    {
        _repository.Delete(requests);
    }
}
