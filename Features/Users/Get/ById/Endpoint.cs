using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.ById;

internal class Endpoint : Endpoint<GenericIdRequestDTO, UserResponseModel>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        User? user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (user != null)
            await SendOkAsync(user.ToResponseModel(), ct);
        else
            await SendNotFoundAsync(ct);
    }
}
