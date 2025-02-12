using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Media;

public interface IMediaRepository : IBasicRepository<RecordMedia>
{
    bool HasGhost(int recordId);
}

public class MediaRepository : BasicRepository<RecordMedia>, IMediaRepository
{
    public MediaRepository(IDatabase database, ILogger<MediaRepository> logger)
        : base(database, logger)
    {
    }

    public bool HasGhost(int recordId)
    {
        RecordMedia? recordMedia = GetSingle(x => x.IdRecord == recordId);
        return recordMedia != null && !string.IsNullOrWhiteSpace(recordMedia.GhostUrl);
    }
}