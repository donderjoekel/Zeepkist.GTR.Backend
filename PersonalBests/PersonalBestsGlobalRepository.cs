using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsGlobalRepository : IBasicRepository<PersonalBestGlobal>
{
}

public class PersonalBestsGlobalRepository : BasicRepository<PersonalBestGlobal>, IPersonalBestsGlobalRepository
{
    public PersonalBestsGlobalRepository(IDatabase database, ILogger<PersonalBestsGlobalRepository> logger)
        : base(database, logger)
    {
    }
}
