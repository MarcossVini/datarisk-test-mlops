using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using Serilog;
using DatariskMLOps.Infrastructure.Data;
using DatariskMLOps.API.Middleware;
using DatariskMLOps.API.Infrastructure;
using DatariskMLOps.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs to listen on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:80");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/datarisk-mlops-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add Datarisk MLOps Services
builder.Services.AddDatariskMLOpsServices(builder.Configuration);

// Redis for Hangfire
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseRedisStorage(redis));

builder.Services.AddHangfireServer();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Datarisk MLOps API",
        Version = "v1",
        Description = "API para pré-processamento de dados em pipelines de MLOps",
        Contact = new OpenApiContact
        {
            Name = "Datarisk Team",
            Email = "dev@datarisk.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Health Checks
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Datarisk MLOps API v1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowAll");
app.UseRouting();

// Health checks endpoint
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

// Database migration on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.Migrate();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration failed");
    }
}

Log.Information("Starting Datarisk MLOps API");

try
{
    Log.Information("About to start app.Run()");
    app.Run();
    Log.Information("app.Run() completed normally");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}

public partial class Program { } // Needed for integration tests
