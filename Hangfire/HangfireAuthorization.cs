using Hangfire.Dashboard;

namespace TNRD.Zeepkist.GTR.Backend.Hangfire;

public class HangfireAuthorization : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
