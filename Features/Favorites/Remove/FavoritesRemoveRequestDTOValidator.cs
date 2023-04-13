using FastEndpoints;
using FluentValidation;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Remove;

internal class FavoritesRemoveRequestDTOValidator : Validator<FavoritesRemoveRequestDTO>
{
    public FavoritesRemoveRequestDTOValidator()
    {
        RuleFor(x => x.Id).NotNull().When(x => x.LevelId == null);
        RuleFor(x => x.LevelId).NotNull().When(x => x.Id == null);
    }
}
