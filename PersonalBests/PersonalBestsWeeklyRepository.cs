using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsWeeklyRepository : IBasicRepository<PersonalBestWeekly>
{
}

public class PersonalBestsWeeklyRepository : BasicRepository<PersonalBestWeekly>, IPersonalBestsWeeklyRepository
{
    public PersonalBestsWeeklyRepository(IDatabase database, ILogger<PersonalBestsWeeklyRepository> logger)
        : base(database, logger)
    {
    }
}
