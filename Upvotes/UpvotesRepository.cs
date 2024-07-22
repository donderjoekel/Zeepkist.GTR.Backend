using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Upvotes;

public interface IUpvotesRepository : IBasicRepository<Upvote>
{
}

public class UpvotesRepository : BasicRepository<Upvote>, IUpvotesRepository
{
    public UpvotesRepository(IDatabase database, ILogger<UpvotesRepository> logger)
        : base(database, logger)
    {
    }
}
