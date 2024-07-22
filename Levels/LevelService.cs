using System.Diagnostics.CodeAnalysis;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels;

public interface ILevelService
{
    Level Create(string hash);
    bool TryGetByHash(string hash, [NotNullWhen(true)] out Level? level);
}

public class LevelService : ILevelService
{
    private readonly ILevelRepository _repository;
    private readonly AutoResetEvent _resetEvent;

    public LevelService(ILevelRepository repository)
    {
        _repository = repository;
        _resetEvent = new AutoResetEvent(true);
    }

    public Level Create(string hash)
    {
        try
        {
            _resetEvent.WaitOne();

            if (_repository.TryGetByHash(hash, out Level? level))
            {
                return level;
            }

            return _repository.Insert(
                new Level
                {
                    Hash = hash
                });
        }
        finally
        {
            _resetEvent.Set();
        }
    }

    public bool TryGetByHash(string hash, [NotNullWhen(true)] out Level? level)
    {
        return _repository.TryGetByHash(hash, out level);
    }
}
