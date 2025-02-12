using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsService
{
    IEnumerable<PersonalBestGlobal> GetAll();
    IEnumerable<PersonalBestGlobal> GetAllIncludingRecord();
    IEnumerable<PersonalBestGlobal> GetForLevel(int levelId);
    PersonalBestGlobal? UpdatePersonalBest(Record record);
}

public class PersonalBestsService : IPersonalBestsService
{
    private readonly IPersonalBestsRepository _repository;

    public PersonalBestsService(IPersonalBestsRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<PersonalBestGlobal> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<PersonalBestGlobal> GetAllIncludingRecord()
    {
        return _repository.GetAll(set => { return set.Include(x => x.Record); });
    }

    public IEnumerable<PersonalBestGlobal> GetForLevel(int levelId)
    {
        return _repository.GetAll(global => global.IdLevel == levelId);
    }

    public PersonalBestGlobal? UpdatePersonalBest(Record record)
    {
        PersonalBestGlobal? existing
            = _repository.GetSingle(
                x => x.IdUser == record.IdUser
                     && x.IdLevel == record.IdLevel,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            return _repository.Insert(
                new PersonalBestGlobal()
                {
                    IdRecord = record.Id,
                    IdUser = record.IdUser,
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
