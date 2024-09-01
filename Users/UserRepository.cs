using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users;

public interface IUserRepository : IBasicRepository<User>
{
}

public class UserRepository : BasicRepository<User>, IUserRepository
{
    public UserRepository(IDatabase database, ILogger<UserRepository> logger)
        : base(database, logger)
    {
    }
}
