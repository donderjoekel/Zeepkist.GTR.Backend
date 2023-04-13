using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.BySteamId;

internal class RequestModelValidator : Validator<UsersGetBySteamIdRequestDTO>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.SteamId)
            .NotEmpty();
    }
}
