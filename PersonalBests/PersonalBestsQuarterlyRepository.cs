using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsQuarterlyRepository : IBasicRepository<PersonalBestQuarterly>
{
}

public class PersonalBestsQuarterlyRepository : BasicRepository<PersonalBestQuarterly>,
    IPersonalBestsQuarterlyRepository
{
    public PersonalBestsQuarterlyRepository(IDatabase database, ILogger<PersonalBestsQuarterlyRepository> logger)
        : base(database, logger)
    {
    }
}
