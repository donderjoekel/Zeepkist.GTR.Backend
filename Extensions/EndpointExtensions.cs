using System.Security.Claims;
using FastEndpoints;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

public static class EndpointExtensions
{
    public static bool TryGetUserId(this IEndpoint endpoint, out int userId)
    {
        return TryGetUserId(endpoint.HttpContext, out userId);
    }

    public static bool TryGetUserId(this HttpContext context, out int userId)
    {
        Claim? claim = context.User.FindFirst("UserId");
        if (claim != null)
        {
            userId = int.Parse(claim.Value);
            return true;
        }
        else
        {
            userId = -1;
            return false;
        }
    }
}
