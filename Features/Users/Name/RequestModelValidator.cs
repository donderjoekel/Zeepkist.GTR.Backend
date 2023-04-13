using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Name;

internal class RequestModelValidator : Validator<UsersUpdateNameRequestDTO>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.SteamName)
            .NotEmpty();
    }
}
