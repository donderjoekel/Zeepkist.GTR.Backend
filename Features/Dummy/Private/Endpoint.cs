using FastEndpoints;

namespace TNRD.Zeepkist.GTR.Backend.Features.Dummy.Private;

internal class Endpoint : EndpointWithoutRequest
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("dummy/private");
        Description(b => b.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(ct);
    }
}
