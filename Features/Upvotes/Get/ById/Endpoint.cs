using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Upvotes.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, UpvoteResponseModel>
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
        Get("upvotes/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        Upvote? upvote = await context.Upvotes
            .AsNoTracking()
            .Include(f => f.UserNavigation)
            .Include(f => f.LevelNavigation)
            .Where(f => f.Id == req.Id)
            .FirstOrDefaultAsync(ct);

        if (upvote != null)
        {
            UpvoteResponseModel responseModel = upvote.ToResponseModel();
            await SendOkAsync(responseModel, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
