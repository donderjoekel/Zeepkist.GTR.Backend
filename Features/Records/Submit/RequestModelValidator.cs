using FastEndpoints;
using FluentValidation;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class RequestModelValidator : Validator<RequestModel>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.Level)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.User)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.Time)
            .GreaterThan(0);
        RuleFor(x => x.GhostData)
            .NotEmpty();
        RuleFor(x => x.ScreenshotData)
            .NotEmpty();
    }
}
