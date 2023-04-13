using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.ById;

internal class RequestModelValidator : Validator<GenericIdRequestDTO>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
