using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Domain.Services;

namespace DatariskMLOps.Infrastructure.BackgroundJobs;

public class ScriptExecutionJob : IScriptExecutionJob
{
    private readonly IExecutionRepository _executionRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IJavaScriptEngine _jsEngine;
    private readonly ILogger<ScriptExecutionJob> _logger;

    public ScriptExecutionJob(
        IExecutionRepository executionRepository,
        IScriptRepository scriptRepository,
        IJavaScriptEngine jsEngine,
        ILogger<ScriptExecutionJob> logger)
    {
        _executionRepository = executionRepository;
        _scriptRepository = scriptRepository;
        _jsEngine = jsEngine;
        _logger = logger;
    }

    public async Task ProcessExecutionAsync(Guid executionId)
    {
        _logger.LogInformation("Starting script execution for execution {ExecutionId}", executionId);

        var execution = await _executionRepository.GetByIdAsync(executionId);
        if (execution == null)
        {
            _logger.LogWarning("Execution {ExecutionId} not found", executionId);
            return;
        }

        var script = await _scriptRepository.GetByIdAsync(execution.ScriptId);
        if (script == null)
        {
            _logger.LogError("Script {ScriptId} not found for execution {ExecutionId}", execution.ScriptId, executionId);
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = "Script not found";
            execution.CompletedAt = DateTime.UtcNow;
            await _executionRepository.UpdateAsync(execution);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            execution.Status = ExecutionStatus.Running;
            await _executionRepository.UpdateAsync(execution);

            _logger.LogInformation("Executing script {ScriptId} for execution {ExecutionId}", script.Id, executionId);

            // Deserializar os dados de entrada
            var rawData = execution.InputData.RootElement.GetRawText();
            _logger.LogInformation("Raw input data: {RawData}", rawData);

            // Parse direto do JSON para obter o tipo correto
            object inputData;
            if (rawData.TrimStart().StartsWith("["))
            {
                // É um array - deserializar como JsonElement e converter
                var jsonArray = JsonSerializer.Deserialize<JsonElement>(rawData);
                var list = new List<object>();
                foreach (var item in jsonArray.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }
                inputData = list.ToArray();
            }
            else
            {
                // É um objeto - converter JsonElement
                var jsonObj = JsonSerializer.Deserialize<JsonElement>(rawData);
                inputData = ConvertJsonElement(jsonObj);
            }

            _logger.LogInformation("Deserialized input data type: {Type}", inputData?.GetType().Name);
            _logger.LogInformation("Deserialized input data: {Data}", JsonSerializer.Serialize(inputData));

            if (inputData == null)
            {
                throw new InvalidOperationException("Input data cannot be null");
            }

            // Executar o script
            var result = await _jsEngine.ExecuteAsync(script.Content, inputData);

            _logger.LogInformation("Script result type: {Type}", result?.GetType().Name);
            _logger.LogInformation("Script result value: {Result}", result?.ToString());

            // Serializar o resultado
            try
            {
                // Converter o resultado para JSON string primeiro e depois para Document
                var jsonString = JsonSerializer.Serialize(result);
                _logger.LogInformation("Result serialized as JSON: {Json}", jsonString);
                execution.OutputData = JsonSerializer.SerializeToDocument(JsonSerializer.Deserialize<object>(jsonString));
                _logger.LogInformation("Serialization successful");
            }
            catch (Exception serEx)
            {
                _logger.LogError(serEx, "Failed to serialize result: {Error}", serEx.Message);

                // Fallback: tentar serializar como string
                try
                {
                    var simpleResult = result?.ToString() ?? "null";
                    execution.OutputData = JsonSerializer.SerializeToDocument(simpleResult);
                }
                catch
                {
                    execution.OutputData = JsonSerializer.SerializeToDocument(new { error = "Serialization failed", type = result?.GetType().Name });
                }
            }
            execution.Status = ExecutionStatus.Completed;

            _logger.LogInformation("Script execution completed successfully for execution {ExecutionId}", executionId);
        }
        catch (Exception ex)
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Script execution failed for execution {ExecutionId}", executionId);
        }
        finally
        {
            stopwatch.Stop();
            execution.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            execution.CompletedAt = DateTime.UtcNow;
            await _executionRepository.UpdateAsync(execution);

            _logger.LogInformation("Script execution finished for execution {ExecutionId} in {ElapsedMs}ms with status {Status}",
                executionId, execution.ExecutionTimeMs, execution.Status);
        }
    }

    private static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.ToString()
        };
    }

    private static Dictionary<string, object> ConvertJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertJsonElement(property.Value);
        }
        return result;
    }

    private static object[] ConvertJsonArray(JsonElement element)
    {
        var result = new List<object>();
        foreach (var item in element.EnumerateArray())
        {
            result.Add(ConvertJsonElement(item));
        }
        return result.ToArray();
    }
}
