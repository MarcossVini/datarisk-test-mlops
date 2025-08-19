using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using DatariskMLOps.API.DTOs;
using DatariskMLOps.Domain.Services;

namespace DatariskMLOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionsController : ControllerBase
{
    private readonly ExecutionService _executionService;
    private readonly ILogger<ExecutionsController> _logger;

    public ExecutionsController(ExecutionService executionService, ILogger<ExecutionsController> logger)
    {
        _executionService = executionService;
        _logger = logger;
    }

    /// <summary>
    /// Executes a script asynchronously
    /// </summary>
    /// <param name="request">Script execution request</param>
    /// <returns>Execution details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ExecutionDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteScript([FromBody] ExecuteScriptRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Starting script execution for script {ScriptId}", request.ScriptId);

        try
        {
            var execution = await _executionService.StartExecutionAsync(request.ScriptId, request.Data);

            var executionDto = new ExecutionDto
            {
                Id = execution.Id,
                ScriptId = execution.ScriptId,
                Status = execution.Status.ToString(),
                InputData = JsonSerializer.Deserialize<object>(execution.InputData.RootElement.GetRawText())!,
                OutputData = execution.OutputData != null ?
                    JsonSerializer.Deserialize<object>(execution.OutputData.RootElement.GetRawText()) : null,
                ErrorMessage = execution.ErrorMessage,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                ExecutionTimeMs = execution.ExecutionTimeMs
            };

            return Accepted($"/api/executions/{execution.Id}", executionDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets an execution by ID
    /// </summary>
    /// <param name="id">Execution ID</param>
    /// <returns>Execution details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecution(Guid id)
    {
        var execution = await _executionService.GetByIdAsync(id);

        if (execution == null)
            return NotFound();

        var executionDto = new ExecutionDto
        {
            Id = execution.Id,
            ScriptId = execution.ScriptId,
            Status = execution.Status.ToString(),
            InputData = JsonSerializer.Deserialize<object>(execution.InputData.RootElement.GetRawText())!,
            OutputData = execution.OutputData != null ?
                JsonSerializer.Deserialize<object>(execution.OutputData.RootElement.GetRawText()) : null,
            ErrorMessage = execution.ErrorMessage,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            ExecutionTimeMs = execution.ExecutionTimeMs,
            Script = execution.Script != null ? new ScriptDto
            {
                Id = execution.Script.Id,
                Name = execution.Script.Name,
                Content = execution.Script.Content,
                Description = execution.Script.Description,
                CreatedAt = execution.Script.CreatedAt,
                UpdatedAt = execution.Script.UpdatedAt
            } : null
        };

        return Ok(executionDto);
    }

    /// <summary>
    /// Gets all executions for a specific script
    /// </summary>
    /// <param name="scriptId">Script ID</param>
    /// <returns>List of executions</returns>
    [HttpGet("by-script/{scriptId}")]
    [ProducesResponseType(typeof(IEnumerable<ExecutionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecutionsByScript(Guid scriptId)
    {
        var executions = await _executionService.GetByScriptIdAsync(scriptId);

        var executionDtos = executions.Select(e => new ExecutionDto
        {
            Id = e.Id,
            ScriptId = e.ScriptId,
            Status = e.Status.ToString(),
            InputData = JsonSerializer.Deserialize<object>(e.InputData.RootElement.GetRawText())!,
            OutputData = e.OutputData != null ?
                JsonSerializer.Deserialize<object>(e.OutputData.RootElement.GetRawText()) : null,
            ErrorMessage = e.ErrorMessage,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt,
            ExecutionTimeMs = e.ExecutionTimeMs
        });

        return Ok(executionDtos);
    }
}
