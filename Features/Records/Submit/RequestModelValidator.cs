using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Submit;

internal class RequestModelValidator : Validator<RecordsSubmitRequestDTO>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.User).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Time).GreaterThan(0);
        RuleFor(x => x.GhostData).NotEmpty();
        RuleFor(x => x.ScreenshotData).NotEmpty();
        RuleFor(x => x.Level).NotEmpty();
        RuleFor(x => x.ModVersion).NotEmpty();
    }
}
