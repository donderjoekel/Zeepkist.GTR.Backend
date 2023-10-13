using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Favorites.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, FavoriteResponseModel>
{
    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("favorites/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        Favorite? favorite = await context.Favorites
            .AsNoTracking()
            .Include(f => f.UserNavigation)
            .Where(f => f.Id == req.Id)
            .FirstOrDefaultAsync(ct);

        if (favorite != null)
        {
            FavoriteResponseModel responseModel = favorite.ToResponseModel();
            await SendOkAsync(responseModel, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
