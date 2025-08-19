using System.Text.Json;
using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;
using Hangfire;

namespace DatariskMLOps.Domain.Services;

public class ExecutionService
{
    private readonly IExecutionRepository _executionRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ExecutionService(
        IExecutionRepository executionRepository,
        IScriptRepository scriptRepository,
        IBackgroundJobClient backgroundJobClient)
    {
        _executionRepository = executionRepository;
        _scriptRepository = scriptRepository;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<Execution> StartExecutionAsync(Guid scriptId, object inputData)
    {
        // Verificar se o script existe
        var script = await _scriptRepository.GetByIdAsync(scriptId);
        if (script == null)
            throw new ArgumentException("Script not found", nameof(scriptId));

        // Criar a execução
        var execution = new Execution
        {
            Id = Guid.NewGuid(),
            ScriptId = scriptId,
            Status = ExecutionStatus.Pending,
            InputData = JsonSerializer.SerializeToDocument(inputData),
            StartedAt = DateTime.UtcNow
        };

        // Salvar no banco
        execution = await _executionRepository.CreateAsync(execution);

        // Enfileirar para processamento assíncrono
        _backgroundJobClient.Enqueue<IScriptExecutionJob>(job =>
            job.ProcessExecutionAsync(execution.Id));

        return execution;
    }

    public async Task<Execution?> GetByIdAsync(Guid id)
    {
        return await _executionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Execution>> GetByScriptIdAsync(Guid scriptId)
    {
        return await _executionRepository.GetByScriptIdAsync(scriptId);
    }
}

public interface IScriptExecutionJob
{
    Task ProcessExecutionAsync(Guid executionId);
}
