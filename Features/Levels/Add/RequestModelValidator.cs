using FastEndpoints;
using FluentValidation;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Add;

internal class RequestModelValidator : Validator<LevelsAddRequestDTO>
{
    public RequestModelValidator()
    {
        RuleForEach(x => x.Uid)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty();
        RuleFor(x => x.Author)
            .NotEmpty();
        RuleFor(x => x.TimeAuthor)
            .GreaterThan(0);
        RuleFor(x => x.TimeGold)
            .GreaterThan(0);
        RuleFor(x => x.TimeSilver)
            .GreaterThan(0);
        RuleFor(x => x.TimeBronze)
            .GreaterThan(0);
    }
}
