using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

internal static class MappingExtensions
{
    private static RecordResponseModel ToWorldRecordResponseModel(
        this Record record,
        UserResponseModel? user = null
    )
    {
        RecordResponseModel responseModel = record.ToResponseModel(user);
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
        UserResponseModel? user = null
    )
    {
        return new FavoriteResponseModel()
        {
            Id = favorite.Id,
            Level = favorite.Level,
            User = user ?? favorite.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = favorite.User }
        };
    }

    public static UpvoteResponseModel ToResponseModel(
        this Upvote upvote,
        UserResponseModel? user = null
    )
    {
        return new UpvoteResponseModel()
        {
            Id = upvote.Id,
            Level = upvote.Level,
            User = user ?? upvote.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = upvote.User }
        };
    }

    public static VoteResponseModel ToResponseModel(
        this Vote vote,
        UserResponseModel? user = null
    )
    {
        return new VoteResponseModel()
        {
            Id = vote.Id,
            Score = vote.Score,
            Category = vote.Category,
            Level = vote.Level,
            User = user ?? vote.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = vote.User }
        };
    }

    public static RecordResponseModel ToResponseModel(
        this Record record,
        UserResponseModel? user = null
    )
    {
        return new RecordResponseModel()
        {
            Level = record.Level,
            User = user ?? record.UserNavigation?.ToResponseModel() ??
                new UserResponseModel { Id = record.User!.Value },
            Id = record.Id,
            IsValid = record.IsValid,
            Splits = string.IsNullOrEmpty(record.Splits)
                ? Array.Empty<float>()
                : record.Splits.Split('|').Select(float.Parse).ToArray(),
            Time = record.Time,
            GameVersion = record.GameVersion,
            ModVersion = record.ModVersion
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
        UserResponseModel? user = null
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
            Level = personalBest.Level
        };
    }

    public static WorldRecordResponseModel ToResponseModel(
        this WorldRecord personalBest,
        RecordResponseModel? record = null,
        UserResponseModel? user = null
    )
    {
        return new WorldRecordResponseModel()
        {
            Id = personalBest.Id,
            PeriodEnd = personalBest.PeriodEnd,
            PeriodStart = personalBest.PeriodStart,
            Record = record ?? personalBest.RecordNavigation?.ToResponseModel() ??
                new RecordResponseModel { Id = personalBest.Record },
            User = user ?? personalBest.UserNavigation?.ToResponseModel() ??
                new UserResponseModel() { Id = personalBest.User },
            Level = personalBest.Level
        };
    }
}
