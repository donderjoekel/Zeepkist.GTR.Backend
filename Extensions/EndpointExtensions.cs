using System.Security.Claims;
using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;

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
    
    public static async Task<bool> UserIsBanned(this IEndpoint endpoint, GTRContext db)
    {
        if (!endpoint.TryGetUserId(out int userId))
            return false;

        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == userId);
        if (user == null)
            return false;

        return user.Banned ?? false;
    }
}
