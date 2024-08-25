using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users.Points;

public interface IUserPointsService
{
    void Update(int userId, int points, int rank, int worldRecords);
}

public class UserPointsService : IUserPointsService
{
    private readonly IUserPointsRepository _repository;

    public UserPointsService(IUserPointsRepository repository)
    {
        _repository = repository;
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
