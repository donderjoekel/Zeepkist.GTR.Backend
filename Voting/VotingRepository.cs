using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Voting;

public interface IVotingRepository : IBasicRepository<Vote>
{
}

public class VotingRepository : BasicRepository<Vote>, IVotingRepository
{
    public VotingRepository(IDatabase database, ILogger<VotingRepository> logger)
        : base(database, logger)
    {
    }
}
