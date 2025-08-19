using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using DatariskMLOps.Infrastructure.Data;
using DatariskMLOps.Domain.Interfaces;

namespace DatariskMLOps.API.HealthChecks;

public class JavaScriptEngineHealthCheck : IHealthCheck
{
    private readonly IJavaScriptEngine _jsEngine;

    public JavaScriptEngineHealthCheck(IJavaScriptEngine jsEngine)
    {
        _jsEngine = jsEngine;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test basic JavaScript execution
            var testScript = "function test(data) { return data + 1; }";
            var result = await _jsEngine.ExecuteAsync(testScript, 5, cancellationToken);

            if (result?.ToString() == "6")
            {
                return HealthCheckResult.Healthy("JavaScript engine is working correctly");
            }

            return HealthCheckResult.Unhealthy("JavaScript engine returned unexpected result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("JavaScript engine failed", ex);
        }
    }
}

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connectivity
            await _context.Database.CanConnectAsync(cancellationToken);

            // Count scripts to test basic query
            var scriptCount = await _context.Scripts.CountAsync(cancellationToken);

            return HealthCheckResult.Healthy($"Database is healthy. Scripts count: {scriptCount}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
