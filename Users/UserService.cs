using System.Diagnostics.CodeAnalysis;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users;

public interface IUserService
{
    User Create(ulong steamId);
    bool TryGet(ulong steamId, [NotNullWhen(true)] out User? user);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public User Create(ulong steamId)
    {
        User user = new()
        {
            SteamId = steamId,
            Banned = false
        };

        return _repository.Insert(user);
    }

    public bool TryGet(ulong steamId, [NotNullWhen(true)] out User? user)
    {
        user = _repository.GetSingle(x => x.SteamId == steamId);
        return user != null;
    }
}
