using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Favorites;

public interface IFavoritesRepository : IBasicRepository<Favorite>
{
}

public class FavoritesRepository : BasicRepository<Favorite>, IFavoritesRepository
{
    public FavoritesRepository(IDatabase database, ILogger<FavoritesRepository> logger)
        : base(database, logger)
    {
    }
}
