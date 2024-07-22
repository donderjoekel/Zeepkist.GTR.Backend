using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsYearlyRepository : IBasicRepository<PersonalBestYearly>
{
}

public class PersonalBestsYearlyRepository : BasicRepository<PersonalBestYearly>, IPersonalBestsYearlyRepository
{
    public PersonalBestsYearlyRepository(IDatabase database, ILogger<PersonalBestsYearlyRepository> logger)
        : base(database, logger)
    {
    }
}
