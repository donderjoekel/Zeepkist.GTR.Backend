using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

internal static class MappingExtensions
{
    public static LevelResponseModel ToResponseModel(this Level level, Record? worldRecord = null)
    {
        return new LevelResponseModel
        {
            Id = level.Id,
            Author = level.Author,
            Name = level.Name,
            IsValid = level.IsValid,
            ThumbnailUrl = level.ThumbnailUrl,
            TimeAuthor = level.TimeAuthor,
            TimeBronze = level.TimeBronze,
            TimeGold = level.TimeGold,
            TimeSilver = level.TimeSilver,
            UniqueId = level.Uid,
            WorkshopId = level.Wid,
            Points = level.Points,
            Rank = level.Rank,
            WorldRecord = worldRecord?.ToWorldRecordResponseModel() ?? null
        };
    }

    private static RecordResponseModel ToWorldRecordResponseModel(
        this Record record,
        UserResponseModel? user = null
    )
    {
        RecordResponseModel responseModel = record.ToResponseModel(null, user);
        responseModel.Level = null;
        return responseModel;
    }

    public static UserResponseModel ToResponseModel(this User user, StatsResponseModel? stat = null)
    {
        return new UserResponseModel
        {
            Id = user.Id,
            DiscordId = user.DiscordId,
            SteamId = user.SteamId,
            SteamName = user.SteamName,
            // Bit hacky, should probably just fix the backend to use integer instead of float
            Score = user.Score.HasValue ? (int)Math.Floor(user.Score.Value) : 0,
            Position = user.Position
        };
    }

    public static FavoriteResponseModel ToResponseModel(
        this Favorite favorite,
        LevelResponseModel? level = null,
        UserResponseModel? user = null
    )
    {
        return new FavoriteResponseModel()
        {
            Id = favorite.Id,
            Level = level ?? favorite.LevelNavigation?.ToResponseModel() ??
                new LevelResponseModel { Id = favorite.Level!.Value },
            User = user ?? favorite.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = favorite.User!.Value }
        };
    }

    public static UpvoteResponseModel ToResponseModel(
        this Upvote upvote,
        LevelResponseModel? level = null,
        UserResponseModel? user = null
    )
    {
        return new UpvoteResponseModel()
        {
            Id = upvote.Id,
            Level = level ?? upvote.LevelNavigation?.ToResponseModel() ??
                new LevelResponseModel { Id = upvote.Level!.Value },
            User = user ?? upvote.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = upvote.User!.Value }
        };
    }

    public static VoteResponseModel ToResponseModel(
        this Vote vote,
        LevelResponseModel? level = null,
        UserResponseModel? user = null
    )
    {
        return new VoteResponseModel()
        {
            Id = vote.Id,
            Score = vote.Score,
            Category = vote.Category,
            Level = level ?? vote.LevelNavigation?.ToResponseModel() ??
                new LevelResponseModel { Id = vote.Level!.Value },
            User = user ?? vote.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = vote.User!.Value }
        };
    }

    public static RecordResponseModel ToResponseModel(
        this Record record,
        LevelResponseModel? level = null,
        UserResponseModel? user = null
    )
    {
        return new RecordResponseModel()
        {
            Level = level ?? record.LevelNavigation?.ToResponseModel() ??
                new LevelResponseModel { Id = record.Level!.Value },
            User = user ?? record.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = record.User!.Value },
            Id = record.Id,
            IsValid = record.IsValid,
            Splits = string.IsNullOrEmpty(record.Splits)
                ? Array.Empty<float>()
                : record.Splits.Split('|').Select(float.Parse).ToArray(),
            Time = record.Time,
            DateCreated = record.DateCreated,
            GameVersion = record.GameVersion,
            GhostUrl = record.GhostUrl,
            IsBest = record.IsBest,
            ScreenshotUrl = record.ScreenshotUrl,
            IsWorldRecord = record.IsWr
        };
    }

    public static StatsResponseModel ToResponseModel(this Stat stat, UserResponseModel? user = null)
    {
        return new StatsResponseModel
        {
            CrashEye = stat.CrashEye,
            CrashGhost = stat.CrashGhost,
            CrashRegular = stat.CrashRegular,
            CrashTotal = stat.CrashTotal,
            CrashSticky = stat.CrashSticky,
            DistanceBraking = stat.DistanceBraking,
            DistanceGrounded = stat.DistanceGrounded,
            DistanceRagdoll = stat.DistanceRagdoll,
            DistanceArmsUp = stat.DistanceArmsUp,
            DistanceInAir = stat.DistanceInAir,
            DistanceOnGrass = stat.DistanceOnGrass,
            DistanceOnIce = stat.DistanceOnIce,
            DistanceOnRegular = stat.DistanceOnRegular,
            DistanceOnFourWheels = stat.DistanceOnFourWheels,
            DistanceOnNoWheels = stat.DistanceOnNoWheels,
            DistanceOnOneWheel = stat.DistanceOnOneWheel,
            DistanceOnThreeWheels = stat.DistanceOnThreeWheels,
            DistanceOnTwoWheels = stat.DistanceOnTwoWheels,
            DistanceWithFourWheels = stat.DistanceWithFourWheels,
            DistanceWithNoWheels = stat.DistanceWithNoWheels,
            DistanceWithOneWheel = stat.DistanceWithOneWheel,
            DistanceWithThreeWheels = stat.DistanceWithThreeWheels,
            DistanceWithTwoWheels = stat.DistanceWithTwoWheels,
            TimeBraking = stat.TimeBraking,
            TimeGrounded = stat.TimeGrounded,
            TimeRagdoll = stat.TimeRagdoll,
            TimeArmsUp = stat.TimeArmsUp,
            TimeInAir = stat.TimeInAir,
            TimeOnGrass = stat.TimeOnGrass,
            TimeOnIce = stat.TimeOnIce,
            TimeOnRegular = stat.TimeOnRegular,
            TimeOnFourWheels = stat.TimeOnFourWheels,
            TimeOnNoWheels = stat.TimeOnNoWheels,
            TimeOnOneWheel = stat.TimeOnOneWheel,
            TimeOnThreeWheels = stat.TimeOnThreeWheels,
            TimeOnTwoWheels = stat.TimeOnTwoWheels,
            TimeWithFourWheels = stat.TimeWithFourWheels,
            TimeWithNoWheels = stat.TimeWithNoWheels,
            TimeWithOneWheel = stat.TimeWithOneWheel,
            TimeWithThreeWheels = stat.TimeWithThreeWheels,
            TimeWithTwoWheels = stat.TimeWithTwoWheels,
            TimesFinished = stat.TimesFinished,
            TimesStarted = stat.TimesStarted,
            WheelsBroken = stat.WheelsBroken,
            CheckpointsCrossed = stat.CheckpointsCrossed,
            Month = stat.Month,
            Year = stat.Year
        };
    }

    public static MediaResponseModel ToResponseModel(this Media media, RecordResponseModel? record = null)
    {
        return new MediaResponseModel
        {
            Id = media.Id,
            Record = record ?? media.RecordNavigation?.ToResponseModel() ??
                new RecordResponseModel { Id = media.Record },
            GhostUrl = media.GhostUrl,
            ScreenshotUrl = media.ScreenshotUrl
        };
    }

    public static PersonalBestResponseModel ToResponseModel(
        this PersonalBest personalBest,
        RecordResponseModel? record = null,
        UserResponseModel? user = null,
        LevelResponseModel? level = null
    )
    {
        return new PersonalBestResponseModel()
        {
            Id = personalBest.Id,
            PeriodEnd = personalBest.PeriodEnd,
            PeriodStart = personalBest.PeriodStart,
            Record = record ?? personalBest.RecordNavigation?.ToResponseModel() ??
                new RecordResponseModel { Id = personalBest.Record },
            User = user ?? personalBest.UserNavigation?.ToResponseModel() ??
                new UserResponseModel() { Id = personalBest.User },
            Level = level ?? personalBest.LevelNavigation?.ToResponseModel() ??
                new LevelResponseModel { Id = personalBest.Level }
        };
    }
}
