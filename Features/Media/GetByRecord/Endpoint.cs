using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Media.GetByRecord;

public class Endpoint : Endpoint<GenericIdRequestDTO, MediaGetByRecordResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("media/record/{Id}");
    }

    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        Database.Models.Media? media = await context.Media.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Record == req.Id, ct);

        if (media != null)
        {
            await SendOkAsync(new MediaGetByRecordResponseDTO()
                {
                    Media = media.ToResponseModel()
                },
                ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
