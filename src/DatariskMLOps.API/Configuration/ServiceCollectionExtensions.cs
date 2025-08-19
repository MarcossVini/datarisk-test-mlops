using DatariskMLOps.Infrastructure.Data;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Domain.Services;
using DatariskMLOps.Infrastructure.Repositories;
using DatariskMLOps.Infrastructure.JavaScript;
using DatariskMLOps.Infrastructure.BackgroundJobs;
using DatariskMLOps.Infrastructure.Services;
using DatariskMLOps.API.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace DatariskMLOps.API.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatariskMLOpsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IExecutionRepository, ExecutionRepository>();

        // Security Services
        services.AddScoped<IScriptSecurityValidator, ScriptSecurityValidator>();

        // Services
        services.AddScoped<IJavaScriptEngine, EnhancedJintJavaScriptEngine>();
        services.AddScoped<ScriptService>();
        services.AddScoped<ExecutionService>();

        // Background Jobs
        services.AddScoped<IScriptExecutionJob, ScriptExecutionJob>();

        // Backup Services
        services.Configure<BackupOptions>(configuration.GetSection("Backup"));
        services.AddScoped<IBackupService, BackupService>();
        // services.AddHostedService<AutomatedBackupService>();

        // Health Checks
        services.AddScoped<DatabaseHealthCheck>();
        services.AddScoped<JavaScriptEngineHealthCheck>();

        return services;
    }
}
