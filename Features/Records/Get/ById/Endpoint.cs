using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.ById;

internal class Endpoint : Endpoint<GenericIdResponseDTO, RecordResponseModel>
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
        Get("records/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdResponseDTO req, CancellationToken ct)
    {
        Record? record = await context.Records
            .AsNoTracking()
            .Include(x => x.UserNavigation)
            .Where(r => r.Id == req.Id)
            .FirstOrDefaultAsync(ct);

        if (record != null)
        {
            RecordResponseModel responseModel = record.ToResponseModel();
            await SendOkAsync(responseModel, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
