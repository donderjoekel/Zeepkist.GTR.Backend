using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Stats.Aggregate;

public class Endpoint : EndpointWithoutRequest<StatsResponseModel>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        Get("stats/aggregate");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Stat? summed = await context.Stats.AsNoTracking()
            .GroupBy(x => 1)
            .Select(x => new Stat()
            {
                CrashTotal = x.Sum(y => y.CrashTotal),
                CrashRegular = x.Sum(y => y.CrashRegular),
                CrashEye = x.Sum(y => y.CrashEye),
                CrashGhost = x.Sum(y => y.CrashGhost),
                CrashSticky = x.Sum(y => y.CrashSticky),
                DistanceArmsUp = x.Sum(y => y.DistanceArmsUp),
                DistanceBraking = x.Sum(y => y.DistanceBraking),
                DistanceGrounded = x.Sum(y => y.DistanceGrounded),
                DistanceInAir = x.Sum(y => y.DistanceInAir),
                DistanceOnNoWheels = x.Sum(y => y.DistanceOnNoWheels),
                DistanceOnOneWheel = x.Sum(y => y.DistanceOnOneWheel),
                DistanceOnTwoWheels = x.Sum(y => y.DistanceOnTwoWheels),
                DistanceOnThreeWheels = x.Sum(y => y.DistanceOnThreeWheels),
                DistanceOnFourWheels = x.Sum(y => y.DistanceOnFourWheels),
                DistanceRagdoll = x.Sum(y => y.DistanceRagdoll),
                DistanceWithNoWheels = x.Sum(y => y.DistanceWithNoWheels),
                DistanceWithOneWheel = x.Sum(y => y.DistanceWithOneWheel),
                DistanceWithTwoWheels = x.Sum(y => y.DistanceWithTwoWheels),
                DistanceWithThreeWheels = x.Sum(y => y.DistanceWithThreeWheels),
                DistanceWithFourWheels = x.Sum(y => y.DistanceWithFourWheels),
                DistanceOnRegular = x.Sum(y => y.DistanceOnRegular),
                DistanceOnGrass = x.Sum(y => y.DistanceOnGrass),
                DistanceOnIce = x.Sum(y => y.DistanceOnIce),
                TimeArmsUp = x.Sum(y => y.TimeArmsUp),
                TimeBraking = x.Sum(y => y.TimeBraking),
                TimeGrounded = x.Sum(y => y.TimeGrounded),
                TimeInAir = x.Sum(y => y.TimeInAir),
                TimeOnNoWheels = x.Sum(y => y.TimeOnNoWheels),
                TimeOnOneWheel = x.Sum(y => y.TimeOnOneWheel),
                TimeOnTwoWheels = x.Sum(y => y.TimeOnTwoWheels),
                TimeOnThreeWheels = x.Sum(y => y.TimeOnThreeWheels),
                TimeOnFourWheels = x.Sum(y => y.TimeOnFourWheels),
                TimeRagdoll = x.Sum(y => y.TimeRagdoll),
                TimeWithNoWheels = x.Sum(y => y.TimeWithNoWheels),
                TimeWithOneWheel = x.Sum(y => y.TimeWithOneWheel),
                TimeWithTwoWheels = x.Sum(y => y.TimeWithTwoWheels),
                TimeWithThreeWheels = x.Sum(y => y.TimeWithThreeWheels),
                TimeWithFourWheels = x.Sum(y => y.TimeWithFourWheels),
                TimeOnRegular = x.Sum(y => y.TimeOnRegular),
                TimeOnGrass = x.Sum(y => y.TimeOnGrass),
                TimeOnIce = x.Sum(y => y.TimeOnIce),
                TimesFinished = x.Sum(y => y.TimesFinished),
                TimesStarted = x.Sum(y => y.TimesStarted),
                WheelsBroken = x.Sum(y => y.WheelsBroken),
                CheckpointsCrossed = x.Sum(y => y.CheckpointsCrossed),
            })
            .FirstOrDefaultAsync(ct);

        await SendOkAsync(summed.ToResponseModel(), ct);
    }
}
