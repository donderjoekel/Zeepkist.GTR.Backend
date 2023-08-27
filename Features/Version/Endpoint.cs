using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;

namespace TNRD.Zeepkist.GTR.Backend.Features.Version;

public class Endpoint : EndpointWithoutRequest<ResponseModel>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Description(b => b.ExcludeFromDescription());
        Get("version");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Database.Models.Version version = await context.Versions.AsNoTracking().FirstAsync(ct);

        await SendOkAsync(new ResponseModel()
            {
                MinimumVersion = version.Minimum!,
                LatestVersion = version.Latest!
            },
            ct);
    }
}
