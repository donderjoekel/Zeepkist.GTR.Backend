using FastEndpoints;

namespace TNRD.Zeepkist.GTR.Backend.Features.Version;

public class Endpoint : EndpointWithoutRequest<ResponseModel>
{
    public override void Configure()
    {
        AllowAnonymous();
        Description(b => b.ExcludeFromDescription());
        Get("version");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return SendOkAsync(new ResponseModel()
            {
                MinimumVersion = "0.20.5",
                LatestVersion = "0.22.2"
            },
            ct);
    }
}
