using Hangfire.Dashboard;

namespace DatariskMLOps.API.Infrastructure;

/// <summary>
/// Helper class for Hangfire dashboard authorization in development
/// </summary>
public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true; // Allow all users in development
    }
}
