using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsService
{
    IEnumerable<PersonalBestGlobal> GetAll();
    IEnumerable<PersonalBestGlobal> GetAllIncludingRecord();
    IEnumerable<PersonalBestGlobal> GetForLevel(int levelId);
    void UpdateDaily(Record record, int userId, int levelId);
    void UpdateWeekly(Record record, int userId, int levelId);
    void UpdateMonthly(Record record, int userId, int levelId);
    void UpdateQuarterly(Record record, int userId, int levelId);
    void UpdateYearly(Record record, int userId, int levelId);
    void UpdateGlobal(Record record, int userId, int levelId);
}

public class PersonalBestsService : IPersonalBestsService
{
    private readonly IPersonalBestsDailyRepository _dailyRepository;
    private readonly IPersonalBestsGlobalRepository _globalRepository;
    private readonly IPersonalBestsMonthlyRepository _monthlyRepository;
    private readonly IPersonalBestsQuarterlyRepository _quarterlyRepository;
    private readonly IPersonalBestsWeeklyRepository _weeklyRepository;
    private readonly IPersonalBestsYearlyRepository _yearlyRepository;

    public PersonalBestsService(
        IPersonalBestsDailyRepository dailyRepository,
        IPersonalBestsGlobalRepository globalRepository,
        IPersonalBestsMonthlyRepository monthlyRepository,
        IPersonalBestsQuarterlyRepository quarterlyRepository,
        IPersonalBestsWeeklyRepository weeklyRepository,
        IPersonalBestsYearlyRepository yearlyRepository)
    {
        _dailyRepository = dailyRepository;
        _globalRepository = globalRepository;
        _monthlyRepository = monthlyRepository;
        _quarterlyRepository = quarterlyRepository;
        _weeklyRepository = weeklyRepository;
        _yearlyRepository = yearlyRepository;
    }

    public IEnumerable<PersonalBestGlobal> GetAll()
    {
        return _globalRepository.GetAll();
    }

    public IEnumerable<PersonalBestGlobal> GetAllIncludingRecord()
    {
        return _globalRepository.GetAll(set => { return set.Include(x => x.Record); });
    }

    public IEnumerable<PersonalBestGlobal> GetForLevel(int levelId)
    {
        return _globalRepository.GetAll(global => global.IdLevel == levelId);
    }

    public void UpdateDaily(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int day = CultureInfo.InvariantCulture.Calendar.GetDayOfYear(record.DateCreated.UtcDateTime);

        PersonalBestDaily? existing
            = _dailyRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId
                     && x.Year == year
                     && x.Day == day,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _dailyRepository.Insert(
                new PersonalBestDaily()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId,
                    Year = year,
                    Day = day
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _dailyRepository.Update(existing);
        }
    }

    public void UpdateWeekly(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            record.DateCreated.UtcDateTime,
            CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

        PersonalBestWeekly? existing
            = _weeklyRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId
                     && x.Year == year
                     && x.Week == week,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _weeklyRepository.Insert(
                new PersonalBestWeekly()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId,
                    Year = year,
                    Week = week
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _weeklyRepository.Update(existing);
        }
    }

    public void UpdateMonthly(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int month = CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated.UtcDateTime);

        PersonalBestMonthly? existing
            = _monthlyRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId
                     && x.Year == year
                     && x.Month == month,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _monthlyRepository.Insert(
                new PersonalBestMonthly()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId,
                    Year = year,
                    Month = month
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _monthlyRepository.Update(existing);
        }
    }

    public void UpdateQuarterly(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int quarter = (CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated.UtcDateTime) - 1) / 3 + 1;

        PersonalBestQuarterly? existing
            = _quarterlyRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId
                     && x.Year == year
                     && x.Quarter == quarter,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _quarterlyRepository.Insert(
                new PersonalBestQuarterly()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId,
                    Year = year,
                    Quarter = quarter
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _quarterlyRepository.Update(existing);
        }
    }

    public void UpdateYearly(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);

        PersonalBestYearly? existing
            = _yearlyRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId
                     && x.Year == year,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _yearlyRepository.Insert(
                new PersonalBestYearly()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId,
                    Year = year
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _yearlyRepository.Update(existing);
        }
    }

    public void UpdateGlobal(Record record, int userId, int levelId)
    {
        if (!record.IsValid)
            return;

        PersonalBestGlobal? existing
            = _globalRepository.GetSingle(
                x => x.IdUser == userId
                     && x.IdLevel == levelId,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _globalRepository.Insert(
                new PersonalBestGlobal()
                {
                    IdRecord = record.Id,
                    IdUser = userId,
                    IdLevel = levelId
                });
        }
        else if (existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _globalRepository.Update(existing);
        }
    }
}
