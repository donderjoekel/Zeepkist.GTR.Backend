using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Submit;

internal class RequestModelValidator : Validator<VotesSubmitRequestDTO>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.Level)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(5);
        RuleFor(x => x.Category)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(3);
    }
}
