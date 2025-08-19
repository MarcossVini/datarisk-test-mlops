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
            var inputData = JsonSerializer.Deserialize<object>(execution.InputData.RootElement.GetRawText());

            if (inputData == null)
            {
                throw new InvalidOperationException("Input data cannot be null");
            }

            // Executar o script
            var result = await _jsEngine.ExecuteAsync(script.Content, inputData);

            // Serializar o resultado
            execution.OutputData = JsonSerializer.SerializeToDocument(result);
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
}
