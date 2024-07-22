using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Items;

public interface ILevelItemsRepository : IBasicRepository<LevelItem>
{
    bool Exists(int levelId, decimal workshopId, decimal authorId, string fileUid);
}

public class LevelItemsRepository : BasicRepository<LevelItem>, ILevelItemsRepository
{
    public LevelItemsRepository(IDatabase database, ILogger<LevelItemsRepository> logger)
        : base(database, logger)
    {
    }

    public bool Exists(int levelId, decimal workshopId, decimal authorId, string fileUid)
    {
        return Query(
                x => x.IdLevel == levelId
                     && x.WorkshopId == workshopId
                     && x.AuthorId == authorId
                     && x.FileUid == fileUid)
            .Any();
    }
}
