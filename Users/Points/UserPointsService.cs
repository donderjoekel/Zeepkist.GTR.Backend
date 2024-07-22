namespace TNRD.Zeepkist.GTR.Backend.Users.Points;

public interface IUserPointsService
{
}

public class UserPointsService : IUserPointsService
{
    private readonly IUserPointsRepository _repository;

    public UserPointsService(IUserPointsRepository repository)
    {
        _repository = repository;
    }
}
