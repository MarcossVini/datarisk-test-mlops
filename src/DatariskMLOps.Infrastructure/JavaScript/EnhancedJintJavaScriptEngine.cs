using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Native.Function;
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

                _logger.LogInformation("Executing JavaScript with {DataSize} bytes. Data type: {DataType}",
                    JsonSerializer.Serialize(inputData).Length, inputData?.GetType().Name);
                _logger.LogInformation("Input data: {InputData}", JsonSerializer.Serialize(inputData));

                // Try to execute the script directly first
                JsValue result;
                try
                {
                    // Execute the script to define functions
                    engine.Execute(script);

                    // Try to find and execute a function that processes data
                    result = TryExecuteFunction(engine, inputData);

                    // If no function found, execute as expression
                    if (result.IsUndefined())
                    {
                        result = engine.Evaluate(script);
                    }
                }
                catch
                {
                    // If script execution fails, try as direct expression
                    result = engine.Evaluate(script);
                }

                var convertedResult = ConvertJsValueToObject(result);
                _logger.LogInformation("JavaScript result converted to: {Type}", convertedResult?.GetType().Name);
                return convertedResult ?? new object();
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

    private JsValue TryExecuteFunction(Engine engine, object inputData)
    {
        // List of common function names to try
        var functionNames = new[] { "process", "execute", "run", "main", "processData" };

        foreach (var functionName in functionNames)
        {
            try
            {
                var func = engine.GetValue(functionName);
                if (!func.IsUndefined() && !func.IsNull())
                {
                    _logger.LogInformation("Found and executing function: {FunctionName}", functionName);
                    return engine.Evaluate($"{functionName}(data)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to execute function {FunctionName}: {Error}", functionName, ex.Message);
            }
        }

        // If no function found, return undefined to trigger fallback
        _logger.LogInformation("No function found, trying to execute as expression");
        return JsValue.Undefined;
    }

    private object ConvertJsValueToObject(JsValue jsValue)
    {
        if (jsValue.IsUndefined() || jsValue.IsNull())
        {
            return null!;
        }

        if (jsValue.IsBoolean())
        {
            return jsValue.AsBoolean();
        }

        if (jsValue.IsNumber())
        {
            return jsValue.AsNumber();
        }

        if (jsValue.IsString())
        {
            return jsValue.AsString();
        }

        if (jsValue.IsArray())
        {
            var array = jsValue.AsArray();
            var result = new List<object>();
            for (uint i = 0; i < array.Length; i++)
            {
                var element = array.Get(i.ToString());
                result.Add(ConvertJsValueToObject(element));
            }
            return result.ToArray();
        }

        if (jsValue.IsObject())
        {
            var obj = jsValue.AsObject();
            var result = new Dictionary<string, object>();

            // Get properties using GetOwnProperties
            foreach (var property in obj.GetOwnProperties())
            {
                var key = property.Key.AsString();
                var value = property.Value.Value;
                if (!value.IsUndefined())
                {
                    result[key] = ConvertJsValueToObject(value);
                }
            }
            return result;
        }

        // Fallback to ToObject for other types
        try
        {
            return jsValue.ToObject() ?? new object();
        }
        catch
        {
            return jsValue.ToString() ?? string.Empty;
        }
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
