using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Authentication;

public interface IAuthenticationRepository : IBasicRepository<Auth>
{
}

public class AuthenticationRepository : BasicRepository<Auth>, IAuthenticationRepository
{
    public AuthenticationRepository(IDatabase database, ILogger<AuthenticationRepository> logger)
        : base(database, logger)
    {
    }
}
