using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsRepository : IBasicRepository<PersonalBestGlobal>
{
}

public class PersonalBestsRepository : BasicRepository<PersonalBestGlobal>, IPersonalBestsRepository
{
    public PersonalBestsRepository(IDatabase database, ILogger<PersonalBestsRepository> logger)
        : base(database, logger)
    {
    }
}
