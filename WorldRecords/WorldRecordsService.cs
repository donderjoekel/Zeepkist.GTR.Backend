using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsService
{
    IEnumerable<WorldRecordGlobal> GetAll();
    IEnumerable<WorldRecordGlobal> GetForUser(int userId);
    void UpdateDailyWorldRecord(Record record, int levelId);
    void UpdateWeeklyWorldRecord(Record record, int levelId);
    void UpdateMonthlyWorldRecord(Record record, int levelId);
    void UpdateQuarterlyWorldRecord(Record record, int levelId);
    void UpdateYearlyWorldRecord(Record record, int levelId);
    void UpdateGlobalWorldRecord(Record record, int levelId);
}

public class WorldRecordsService : IWorldRecordsService
{
    private readonly IWorldRecordsDailyRepository _dailyRepository;
    private readonly IWorldRecordsGlobalRepository _globalRepository;
    private readonly IWorldRecordsMonthlyRepository _monthlyRepository;
    private readonly IWorldRecordsQuarterlyRepository _quarterlyRepository;
    private readonly IWorldRecordsWeeklyRepository _weeklyRepository;
    private readonly IWorldRecordsYearlyRepository _yearlyRepository;

    public WorldRecordsService(
        IWorldRecordsDailyRepository dailyRepository,
        IWorldRecordsGlobalRepository globalRepository,
        IWorldRecordsMonthlyRepository monthlyRepository,
        IWorldRecordsWeeklyRepository weeklyRepository,
        IWorldRecordsQuarterlyRepository quarterlyRepository,
        IWorldRecordsYearlyRepository yearlyRepository)
    {
        _dailyRepository = dailyRepository;
        _globalRepository = globalRepository;
        _monthlyRepository = monthlyRepository;
        _weeklyRepository = weeklyRepository;
        _yearlyRepository = yearlyRepository;
        _quarterlyRepository = quarterlyRepository;
    }

    public IEnumerable<WorldRecordGlobal> GetAll()
    {
        return _globalRepository.GetAll();
    }

    public IEnumerable<WorldRecordGlobal> GetForUser(int userId)
    {
        return _globalRepository.GetAll(
            x => x.Record.IdUser == userId,
            set => set.Include(x => x.Record));
    }

    public void UpdateDailyWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int day = CultureInfo.InvariantCulture.Calendar.GetDayOfYear(record.DateCreated.UtcDateTime);

        WorldRecordDaily? existing
            = _dailyRepository.GetSingle(
                x => x.IdLevel == levelId && x.Year == year && x.Day == day,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _dailyRepository.Insert(
                new WorldRecordDaily()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId,
                    Year = year,
                    Day = day
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _dailyRepository.Update(existing);
        }
    }

    public void UpdateWeeklyWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            record.DateCreated.UtcDateTime,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        WorldRecordWeekly? existing
            = _weeklyRepository.GetSingle(
                x => x.IdLevel == levelId && x.Year == year && x.Week == week,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _weeklyRepository.Insert(
                new WorldRecordWeekly()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId,
                    Year = year,
                    Week = week
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _weeklyRepository.Update(existing);
        }
    }

    public void UpdateMonthlyWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int month = CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated.UtcDateTime);

        WorldRecordMonthly? existing
            = _monthlyRepository.GetSingle(
                x => x.IdLevel == levelId && x.Year == year && x.Month == month,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _monthlyRepository.Insert(
                new WorldRecordMonthly()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId,
                    Year = year,
                    Month = month
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _monthlyRepository.Update(existing);
        }
    }

    public void UpdateQuarterlyWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);
        int quarter = (CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated.UtcDateTime) - 1) / 3 + 1;

        WorldRecordQuarterly? existing
            = _quarterlyRepository.GetSingle(
                x =>
                    x.IdLevel == levelId &&
                    x.Year == year &&
                    x.Quarter == quarter,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _quarterlyRepository.Insert(
                new WorldRecordQuarterly()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId,
                    Year = year,
                    Quarter = quarter
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _quarterlyRepository.Update(existing);
        }
    }

    public void UpdateYearlyWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated.UtcDateTime);

        WorldRecordYearly? existing
            = _yearlyRepository.GetSingle(
                x => x.IdLevel == levelId && x.Year == year,
                set => set.Include(x => x.Record));

        if (existing == null)
        {
            _yearlyRepository.Insert(
                new WorldRecordYearly()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId,
                    Year = year
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _yearlyRepository.Update(existing);
        }
    }

    public void UpdateGlobalWorldRecord(Record record, int levelId)
    {
        if (!record.IsValid)
            return;

        WorldRecordGlobal? existing
            = _globalRepository.GetSingle(x => x.IdLevel == levelId, set => set.Include(x => x.Record));

        if (existing == null)
        {
            _globalRepository.Insert(
                new WorldRecordGlobal()
                {
                    IdRecord = record.Id,
                    IdLevel = levelId
                });
        }
        else if (!existing.Record.IsValid || existing.IdRecord != record.Id && record.Time < existing.Record.Time)
        {
            existing.IdRecord = record.Id;
            _globalRepository.Update(existing);
        }
    }
}
