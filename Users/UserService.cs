using System.Diagnostics.CodeAnalysis;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users;

public interface IUserService
{
    int Count();
    User Create(ulong steamId);
    IEnumerable<User> GetAll();
    bool TryGet(int id, [NotNullWhen(true)] out User? user);
    bool TryGet(ulong steamId, [NotNullWhen(true)] out User? user);
    void UpdateName(ulong steamId, string name);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public int Count()
    {
        return _repository.Count();
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

    public IEnumerable<User> GetAll()
    {
        return _repository.GetAll();
    }

    public bool TryGet(int id, [NotNullWhen(true)] out User? user)
    {
        user = _repository.GetById(id);
        return user != null;
    }

    public bool TryGet(ulong steamId, [NotNullWhen(true)] out User? user)
    {
        user = _repository.GetSingle(x => x.SteamId == steamId);
        return user != null;
    }

    public void UpdateName(ulong steamId, string name)
    {
        User? user = _repository.GetSingle(x => x.SteamId == steamId);
        if (user == null)
        {
            return;
        }

        user.SteamName = name;
        _repository.Update(user);
    }
}
