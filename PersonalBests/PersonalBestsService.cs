using System.Globalization;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.PersonalBests;

public interface IPersonalBestsService
{
    void UpdateDailyPersonalBest(Record record, int userId, int levelId);
    void UpdateWeeklyPersonalBest(Record record, int userId, int levelId);
    void UpdateMonthlyPersonalBest(Record record, int userId, int levelId);
    void UpdateQuarterlyPersonalBest(Record record, int userId, int levelId);
    void UpdateYearlyPersonalBest(Record record, int userId, int levelId);
    void UpdateGlobalPersonalBest(Record record, int userId, int levelId);
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

    public void UpdateDailyPersonalBest(Record record, int userId, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int day = CultureInfo.InvariantCulture.Calendar.GetDayOfYear(record.DateCreated);

        _dailyRepository.Upsert(
            daily => daily.IdUser == userId
                     && daily.IdLevel == levelId
                     && daily.Year == year
                     && daily.Day == day,
            () => new PersonalBestDaily()
            {
                IdRecord = record.Id,
                IdUser = userId,
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

    public void UpdateWeeklyPersonalBest(Record record, int userId, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            record.DateCreated,
            CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

        _weeklyRepository.Upsert(
            weekly => weekly.IdUser == userId
                      && weekly.IdLevel == levelId
                      && weekly.Year == year
                      && weekly.Week == week,
            () => new PersonalBestWeekly()
            {
                IdRecord = record.Id,
                IdUser = userId,
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

    public void UpdateMonthlyPersonalBest(Record record, int userId, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int month = CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated);

        _monthlyRepository.Upsert(
            monthly => monthly.IdUser == userId
                       && monthly.IdLevel == levelId
                       && monthly.Year == year
                       && monthly.Month == month,
            () => new PersonalBestMonthly()
            {
                IdRecord = record.Id,
                IdUser = userId,
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

    public void UpdateQuarterlyPersonalBest(Record record, int userId, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);
        int quarter = (CultureInfo.InvariantCulture.Calendar.GetMonth(record.DateCreated) - 1) / 3 + 1;

        _quarterlyRepository.Upsert(
            quarterly => quarterly.IdUser == userId
                         && quarterly.IdLevel == levelId
                         && quarterly.Year == year
                         && quarterly.Quarter == quarter,
            () => new PersonalBestQuarterly()
            {
                IdRecord = record.Id,
                IdUser = userId,
                IdLevel = levelId,
                Year = year,
                Quarter = quarter
            },
            quarterly =>
            {
                quarterly.IdRecord = record.Id;
                return quarterly;
            });
    }

    public void UpdateYearlyPersonalBest(Record record, int userId, int levelId)
    {
        int year = CultureInfo.InvariantCulture.Calendar.GetYear(record.DateCreated);

        _yearlyRepository.Upsert(
            yearly => yearly.IdUser == userId
                      && yearly.IdLevel == levelId
                      && yearly.Year == year,
            () => new PersonalBestYearly()
            {
                IdRecord = record.Id,
                IdUser = userId,
                IdLevel = levelId,
                Year = year
            },
            yearly =>
            {
                yearly.IdRecord = record.Id;
                return yearly;
            });
    }

    public void UpdateGlobalPersonalBest(Record record, int userId, int levelId)
    {
        _globalRepository.Upsert(
            global => global.IdUser == userId
                      && global.IdLevel == levelId,
            () => new PersonalBestGlobal()
            {
                IdRecord = record.Id,
                IdUser = userId,
                IdLevel = levelId
            },
            global =>
            {
                global.IdRecord = record.Id;
                return global;
            });
    }
}
