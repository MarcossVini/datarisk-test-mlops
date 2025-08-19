using Microsoft.AspNetCore.Mvc;
using DatariskMLOps.API.DTOs;
using DatariskMLOps.Domain.Services;

namespace DatariskMLOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptsController : ControllerBase
{
    private readonly ScriptService _scriptService;
    private readonly ILogger<ScriptsController> _logger;

    public ScriptsController(ScriptService scriptService, ILogger<ScriptsController> logger)
    {
        _scriptService = scriptService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new script
    /// </summary>
    /// <param name="request">Script creation request</param>
    /// <returns>Created script</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScript([FromBody] CreateScriptRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new script: {ScriptName}", request.Name);

        try
        {
            var script = await _scriptService.CreateAsync(request.Name, request.Content, request.Description);

            var scriptDto = new ScriptDto
            {
                Id = script.Id,
                Name = script.Name,
                Content = script.Content,
                Description = script.Description,
                CreatedAt = script.CreatedAt,
                UpdatedAt = script.UpdatedAt
            };

            return CreatedAtAction(nameof(GetScript), new { id = script.Id }, scriptDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Script validation failed: {Error}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating script");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets a script by ID
    /// </summary>
    /// <param name="id">Script ID</param>
    /// <returns>Script details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScript(Guid id)
    {
        var script = await _scriptService.GetByIdAsync(id);

        if (script == null)
            return NotFound();

        var scriptDto = new ScriptDto
        {
            Id = script.Id,
            Name = script.Name,
            Content = script.Content,
            Description = script.Description,
            CreatedAt = script.CreatedAt,
            UpdatedAt = script.UpdatedAt
        };

        return Ok(scriptDto);
    }

    /// <summary>
    /// Gets all scripts with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10, max: 100)</param>
    /// <returns>List of scripts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScriptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScripts([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var scripts = await _scriptService.GetAllAsync(page, size);

        var scriptDtos = scripts.Select(s => new ScriptDto
        {
            Id = s.Id,
            Name = s.Name,
            Content = s.Content,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        });

        return Ok(scriptDtos);
    }

    /// <summary>
    /// Updates an existing script
    /// </summary>
    /// <param name="id">Script ID</param>
    /// <param name="request">Script update request</param>
    /// <returns>Updated script</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScript(Guid id, [FromBody] UpdateScriptRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var script = await _scriptService.UpdateAsync(id, request.Name, request.Content, request.Description);

            var scriptDto = new ScriptDto
            {
                Id = script.Id,
                Name = script.Name,
                Content = script.Content,
                Description = script.Description,
                CreatedAt = script.CreatedAt,
                UpdatedAt = script.UpdatedAt
            };

            return Ok(scriptDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a script
    /// </summary>
    /// <param name="id">Script ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScript(Guid id)
    {
        try
        {
            await _scriptService.DeleteAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
