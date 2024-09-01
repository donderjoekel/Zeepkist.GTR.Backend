using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users.Points;

public interface IUserPointsRepository : IBasicRepository<UserPoints>
{
}

public class UserPointsRepository : BasicRepository<UserPoints>, IUserPointsRepository
{
    public UserPointsRepository(IDatabase database, ILogger<UserPointsRepository> logger)
        : base(database, logger)
    {
    }
}
