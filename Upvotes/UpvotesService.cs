namespace TNRD.Zeepkist.GTR.Backend.Upvotes;

public interface IUpvotesService
{
}

public class UpvotesService : IUpvotesService
{
    private readonly IUpvotesRepository _repository;

    public UpvotesService(IUpvotesRepository repository)
    {
        _repository = repository;
    }
}
