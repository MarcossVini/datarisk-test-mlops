using Jint;
using Jint.Native;
using Jint.Runtime;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DatariskMLOps.Infrastructure.JavaScript;

public class EnhancedJintJavaScriptEngine : IJavaScriptEngine
{
    private readonly ILogger<EnhancedJintJavaScriptEngine> _logger;
    private readonly IScriptSecurityValidator _securityValidator;

    public EnhancedJintJavaScriptEngine(ILogger<EnhancedJintJavaScriptEngine> logger, IScriptSecurityValidator securityValidator)
    {
        _logger = logger;
        _securityValidator = securityValidator;
    }

    public async Task<object> ExecuteAsync(string script, object inputData, CancellationToken cancellationToken = default)
    {
        // Validate script security first
        var validationResult = await _securityValidator.ValidateScriptAsync(script);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Script validation failed: {Error}", validationResult.ErrorMessage);
            throw new InvalidOperationException($"Script validation failed: {validationResult.ErrorMessage}");
        }

        if (validationResult.Warnings.Any())
        {
            _logger.LogWarning("Script has warnings: {Warnings}", string.Join(", ", validationResult.Warnings));
        }

        return await Task.Run(() =>
        {
            var engine = new Engine(options =>
            {
                options.TimeoutInterval(TimeSpan.FromMinutes(5));
                options.MaxStatements(10000);
                options.LimitRecursion(50);
                options.CancellationToken(cancellationToken);
                options.Strict(true); // Enable strict mode
            });

            try
            {
                // Set up secure environment
                ConfigureSecureEnvironment(engine);

                // Disponibiliza apenas o que é necessário
                engine.SetValue("data", inputData);

                _logger.LogInformation("Executing JavaScript with {DataSize} bytes",
                    JsonSerializer.Serialize(inputData).Length);

                // Executa o script como uma função que recebe data
                var result = engine.Evaluate($"({script})(data)");

                return result.ToObject() ?? new object();
            }
            catch (JavaScriptException ex)
            {
                _logger.LogError(ex, "JavaScript execution error: {Error}", ex.Message);
                throw new InvalidOperationException($"JavaScript execution error: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Script execution timeout");
                throw new InvalidOperationException("Script execution timeout", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during script execution");
                throw new InvalidOperationException($"Unexpected error during script execution: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    private void ConfigureSecureEnvironment(Engine engine)
    {
        // Remove dangerous global objects and functions
        var dangerousGlobals = new[]
        {
            "require", "import", "eval", "Function", "constructor",
            "setTimeout", "setInterval", "clearTimeout", "clearInterval",
            "XMLHttpRequest", "fetch", "WebSocket", "EventSource",
            "localStorage", "sessionStorage", "indexedDB",
            "navigator", "location", "history", "document", "window",
            "global", "process", "Buffer", "console"
        };

        foreach (var globalName in dangerousGlobals)
        {
            engine.Global.Delete(globalName);
        }

        // Provide safe alternatives
        engine.SetValue("console", new
        {
            log = new Action<object>(obj => _logger.LogInformation("Script log: {Message}", obj?.ToString()))
        });

        // Add safe Date operations with limited functionality
        engine.SetValue("SafeDate", new
        {
            now = new Func<long>(() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            toISOString = new Func<DateTime, string>(date => date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
        });
    }
}
