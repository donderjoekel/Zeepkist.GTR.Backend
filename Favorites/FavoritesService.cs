namespace TNRD.Zeepkist.GTR.Backend.Favorites;

public interface IFavoritesService
{
}

public class FavoritesService : IFavoritesService
{
    private readonly IFavoritesRepository _repository;

    public FavoritesService(IFavoritesRepository repository)
    {
        _repository = repository;
    }
}
