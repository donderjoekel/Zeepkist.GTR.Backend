using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.PersonalBests;
using TNRD.Zeepkist.GTR.Backend.WorldRecords;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Users.Points.Jobs;

public class CalculateUserPointsJob
{
    private static readonly Dictionary<int, double> Fibbonus = new()
    {
        { 0, 0.21 },
        { 1, 0.13 },
        { 2, 0.08 },
        { 3, 0.05 },
        { 4, 0.03 },
        { 5, 0.02 },
        { 6, 0.01 },
        { 7, 0.01 }
    };

    private readonly IUserPointsService _userPointsService;
    private readonly IUserService _userService;
    private readonly IWorldRecordsService _worldRecordsService;
    private readonly IPersonalBestsService _personalBestsService;
    private readonly ILogger<CalculateUserPointsJob> _logger;

    public CalculateUserPointsJob(
        IUserPointsService userPointsService,
        IUserService userService,
        IWorldRecordsService worldRecordsService,
        IPersonalBestsService personalBestsService,
        ILogger<CalculateUserPointsJob> logger)
    {
        _userPointsService = userPointsService;
        _userService = userService;
        _worldRecordsService = worldRecordsService;
        _personalBestsService = personalBestsService;
        _logger = logger;
    }

    [UsedImplicitly]
    public Task ExecuteAsync()
    {
        int totalUsers = _userService.Count();

        IEnumerable<User> users = _userService.GetAll();

        List<IGrouping<int, WorldRecordGlobal>> worldRecordGroups = [];

        foreach (User user in users)
        {
            List<WorldRecordGlobal> worldRecordGlobals = _worldRecordsService.GetForUser(user.Id).ToList();
            worldRecordGroups.Add(new Grouping<int, WorldRecordGlobal>(user.Id, worldRecordGlobals));
        }

        Dictionary<int, int> worldRecordCountPerUser = worldRecordGroups.ToDictionary(x => x.Key, x => x.Count());

        List<IGrouping<int, PersonalBestGlobal>> groups = _personalBestsService.GetAllIncludingRecord()
            .OrderBy(x => x.Id)
            .GroupBy(x => x.IdLevel)
            .ToList();

        Dictionary<int, int> pointsPerUser = new();

        foreach (IGrouping<int, PersonalBestGlobal> group in groups)
        {
            List<PersonalBestGlobal> personalBests = group.OrderBy(x => x.Record.Time).ToList();
            int count = personalBests.Count;
            for (int i = 0; i < count; i++)
            {
                int placementPoints = Math.Max(0, count - i);
                double a = 1d / (totalUsers / (double)count);
                int b = i + 1;
                double c = i < 8 ? Fibbonus[i] : 0;
                double points = placementPoints * (1 + a / b) + c;
                PersonalBestGlobal personalBest = personalBests[i];
                pointsPerUser.TryAdd(personalBest.IdUser, 0);
                pointsPerUser[personalBest.IdUser] += (int)Math.Floor(points); // Note: this was round before
            }
        }

        List<KeyValuePair<int, int>> orderedPoints = pointsPerUser.OrderByDescending(x => x.Value).ToList();
        for (int i = 0; i < orderedPoints.Count; i++)
        {
            KeyValuePair<int, int> kvp = orderedPoints[i];
            int newRank = i + 1;

            _logger.LogInformation(
                "Updating user {UserId} with {Points} points, rank {Rank} and {WorldRecords} world records",
                kvp.Key,
                kvp.Value,
                newRank,
                worldRecordCountPerUser.GetValueOrDefault(kvp.Key, 0));

            _userPointsService.Update(
                kvp.Key,
                kvp.Value,
                newRank,
                worldRecordCountPerUser.GetValueOrDefault(kvp.Key, 0));
        }

        return Task.CompletedTask;
    }
}
