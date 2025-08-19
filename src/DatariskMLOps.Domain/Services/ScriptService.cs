using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;

namespace DatariskMLOps.Domain.Services;

public class ScriptService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IScriptSecurityValidator _securityValidator;

    public ScriptService(IScriptRepository scriptRepository, IScriptSecurityValidator securityValidator)
    {
        _scriptRepository = scriptRepository;
        _securityValidator = securityValidator;
    }

    public async Task<Script> CreateAsync(string name, string content, string? description = null)
    {
        // Validate script security before creating
        var validationResult = await _securityValidator.ValidateScriptAsync(content);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Script validation failed: {validationResult.ErrorMessage}", nameof(content));
        }

        var script = new Script
        {
            Id = Guid.NewGuid(),
            Name = name,
            Content = content,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _scriptRepository.CreateAsync(script);
    }

    public async Task<Script?> GetByIdAsync(Guid id)
    {
        return await _scriptRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Script>> GetAllAsync(int page = 1, int size = 10)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 10;

        return await _scriptRepository.GetAllAsync(page, size);
    }

    public async Task<Script> UpdateAsync(Guid id, string name, string content, string? description = null)
    {
        // Validate script security before updating
        var validationResult = await _securityValidator.ValidateScriptAsync(content);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Script validation failed: {validationResult.ErrorMessage}", nameof(content));
        }

        var script = await _scriptRepository.GetByIdAsync(id);
        if (script == null)
            throw new ArgumentException("Script not found", nameof(id));

        script.Name = name;
        script.Content = content;
        script.Description = description;
        script.UpdatedAt = DateTime.UtcNow;

        return await _scriptRepository.UpdateAsync(script);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _scriptRepository.ExistsAsync(id))
            throw new ArgumentException("Script not found", nameof(id));

        await _scriptRepository.DeleteAsync(id);
    }
}
