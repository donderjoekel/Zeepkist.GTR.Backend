using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

internal static class MappingExtensions
{
    public static UserResponseModel ToResponseModel(this User user)
    {
        return new UserResponseModel
        {
            Id = user.Id,
            DiscordId = user.DiscordId,
            SteamId = user.SteamId,
            SteamName = user.SteamName
        };
    }

    public static FavoriteResponseModel ToResponseModel(this Favorite favorite)
    {
        return new FavoriteResponseModel
        {
            Id = favorite.Id,
            Level = favorite.Level,
            User = favorite.User
        };
    }

    public static UpvoteResponseModel ToResponseModel(this Upvote upvote)
    {
        return new UpvoteResponseModel
        {
            Id = upvote.Id,
            Level = upvote.Level,
            User = upvote.User
        };
    }

    public static VoteResponseModel ToResponseModel(this Vote vote)
    {
        return new VoteResponseModel
        {
            Id = vote.Id,
            Score = vote.Score,
            Level = vote.Level,
            User = vote.User
        };
    }

    public static RecordResponseModel ToResponseModel(this Record record)
    {
        return new RecordResponseModel
        {
            Level = record.Level,
            User = record.User,
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

    public static StatsResponseModel ToResponseModel(this Stat stat)
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

    public static MediaResponseModel ToResponseModel(this Media media)
    {
        return new MediaResponseModel
        {
            Id = media.Id,
            Record = media.Record,
            GhostUrl = media.GhostUrl,
            ScreenshotUrl = media.ScreenshotUrl
        };
    }

    public static PersonalBestResponseModel ToResponseModel(this PersonalBest personalBest)
    {
        return new PersonalBestResponseModel
        {
            Id = personalBest.Id,
            PeriodEnd = personalBest.PeriodEnd,
            PeriodStart = personalBest.PeriodStart,
            Record = personalBest.Record,
            User = personalBest.User,
            Level = personalBest.Level
        };
    }

    public static WorldRecordResponseModel ToResponseModel(this WorldRecord worldRecord)
    {
        return new WorldRecordResponseModel
        {
            Id = worldRecord.Id,
            PeriodEnd = worldRecord.PeriodEnd,
            PeriodStart = worldRecord.PeriodStart,
            Record = worldRecord.Record,
            User = worldRecord.User,
            Level = worldRecord.Level
        };
    }
}
