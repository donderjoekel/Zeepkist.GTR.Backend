using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsDailyRepository : IBasicRepository<PersonalBestDaily>
{
}

public class PersonalBestsDailyRepository : BasicRepository<PersonalBestDaily>, IPersonalBestsDailyRepository
{
    public PersonalBestsDailyRepository(IDatabase database, ILogger<PersonalBestsDailyRepository> logger)
        : base(database, logger)
    {
    }
}
