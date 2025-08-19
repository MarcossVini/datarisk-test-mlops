using DatariskMLOps.Domain.Entities;

namespace DatariskMLOps.Domain.Interfaces;

public interface IExecutionRepository
{
    Task<Execution?> GetByIdAsync(Guid id);
    Task<IEnumerable<Execution>> GetByScriptIdAsync(Guid scriptId);
    Task<Execution> CreateAsync(Execution execution);
    Task<Execution> UpdateAsync(Execution execution);
    Task DeleteAsync(Guid id);
}
