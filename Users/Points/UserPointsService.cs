using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users.Points;

public interface IUserPointsService
{
    IEnumerable<UserPoints> GetAll();
    void AddRange(IEnumerable<UserPoints> userPoints);
    void UpdateRange(IEnumerable<UserPoints> userPoints);
    void Update(int userId, int points, int rank, int worldRecords);
}

public class UserPointsService : IUserPointsService
{
    private readonly IUserPointsRepository _repository;

    public UserPointsService(IUserPointsRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<UserPoints> GetAll()
    {
        return _repository.GetAll();
    }

    public void AddRange(IEnumerable<UserPoints> userPoints)
    {
        _repository.InsertRange(userPoints);
    }

    public void UpdateRange(IEnumerable<UserPoints> userPoints)
    {
        _repository.UpdateRange(userPoints);
    }

    public void Update(int userId, int points, int rank, int worldRecords)
    {
        _repository.Upsert(
            userPoints => userPoints.IdUser == userId,
            () => new UserPoints()
            {
                IdUser = userId,
                Points = points,
                Rank = rank,
                WorldRecords = worldRecords
            },
            userPoints =>
            {
                userPoints.Points = points;
                userPoints.Rank = rank;
                userPoints.WorldRecords = worldRecords;
                return userPoints;
            });
    }
}
