using System.Globalization;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.WorldRecords;

public interface IWorldRecordsService
{
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

    public void UpdateDailyWorldRecord(Record record, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int day = CultureInfo.InvariantCulture.Calendar.GetDayOfYear(record.DateCreated);

        _dailyRepository.Upsert(
            daily => daily.IdLevel == levelId
                     && daily.Year == year
                     && daily.Day == day,
            () => new WorldRecordDaily()
            {
                IdRecord = record.Id,
                IdLevel = levelId,
                Year = year,
                Day = day
            },
            daily =>
            {
                daily.IdRecord = record.Id;
                return daily;
            });
    }

    public void UpdateWeeklyWorldRecord(Record record, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            record.DateCreated,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        _weeklyRepository.Upsert(
            weekly => weekly.IdLevel == levelId
                      && weekly.Year == year
                      && weekly.Week == week,
            () => new WorldRecordWeekly()
            {
                IdRecord = record.Id,
                IdLevel = levelId,
                Year = year,
                Week = week
            },
            weekly =>
            {
                weekly.IdRecord = record.Id;
                return weekly;
            });
    }

    public void UpdateMonthlyWorldRecord(Record record, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int month = CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated);

        _monthlyRepository.Upsert(
            monthly => monthly.IdLevel == levelId
                       && monthly.Year == year
                       && monthly.Month == month,
            () => new WorldRecordMonthly()
            {
                IdRecord = record.Id,
                IdLevel = levelId,
                Year = year,
                Month = month
            },
            monthly =>
            {
                monthly.IdRecord = record.Id;
                return monthly;
            });
    }

    public void UpdateQuarterlyWorldRecord(Record record, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int quarter = (CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated) - 1) / 3 + 1;

        _quarterlyRepository.Upsert(
            monthly => monthly.IdLevel == levelId
                       && monthly.Year == year
                       && monthly.Quarter == quarter,
            () => new WorldRecordQuarterly()
            {
                IdRecord = record.Id,
                IdLevel = levelId,
                Year = year,
                Quarter = quarter
            },
            monthly =>
            {
                monthly.IdRecord = record.Id;
                return monthly;
            });
    }

    public void UpdateYearlyWorldRecord(Record record, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);

        _yearlyRepository.Upsert(
            yearly => yearly.IdLevel == levelId
                      && yearly.Year == year,
            () => new WorldRecordYearly()
            {
                IdRecord = record.Id,
                IdLevel = levelId,
                Year = year
            },
            yearly =>
            {
                yearly.IdRecord = record.Id;
                return yearly;
            });
    }

    public void UpdateGlobalWorldRecord(Record record, int levelId)
    {
        _globalRepository.Upsert(
            global => global.IdLevel == levelId,
            () => new WorldRecordGlobal()
            {
                IdRecord = record.Id,
                IdLevel = levelId
            },
            global =>
            {
                global.IdRecord = record.Id;
                return global;
            });
    }
}
