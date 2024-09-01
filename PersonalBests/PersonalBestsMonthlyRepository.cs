using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsMonthlyRepository : IBasicRepository<PersonalBestMonthly>
{
}

public class PersonalBestsMonthlyRepository : BasicRepository<PersonalBestMonthly>, IPersonalBestsMonthlyRepository
{
    public PersonalBestsMonthlyRepository(IDatabase database, ILogger<PersonalBestsMonthlyRepository> logger)
        : base(database, logger)
    {
    }
}
