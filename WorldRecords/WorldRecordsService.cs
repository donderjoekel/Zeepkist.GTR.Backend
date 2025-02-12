using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsService
{
    IEnumerable<WorldRecordGlobal> GetAll();
    IEnumerable<WorldRecordGlobal> GetForUser(int userId);
    WorldRecordGlobal? UpdateWorldRecord(Record record);
}

public class WorldRecordsService : IWorldRecordsService
{
    private readonly IWorldRecordsRepository _repository;

    public WorldRecordsService(IWorldRecordsRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<WorldRecordGlobal> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<WorldRecordGlobal> GetForUser(int userId)
    {
        return _repository.GetAll(
            x => x.Record.IdUser == userId,
            set => set.Include(x => x.Record));
    }

    public WorldRecordGlobal? UpdateWorldRecord(Record record)
    {
        WorldRecordGlobal? existing
            = _repository.GetSingle(x => x.IdLevel == record.IdLevel, set => set.Include(x => x.Record));

        if (existing == null)
        {
            return _repository.Insert(
                new WorldRecordGlobal()
                {
                    IdRecord = record.Id,
                    IdLevel = record.IdLevel
                });
        }

        if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            return _repository.Update(existing);
        }

        return existing;
    }
}
