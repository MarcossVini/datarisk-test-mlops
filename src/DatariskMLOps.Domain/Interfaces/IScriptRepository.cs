using DatariskMLOps.Domain.Entities;

namespace DatariskMLOps.Domain.Interfaces;

public interface IScriptRepository
{
    Task<Script?> GetByIdAsync(Guid id);
    Task<IEnumerable<Script>> GetAllAsync(int page, int size);
    Task<Script> CreateAsync(Script script);
    Task<Script> UpdateAsync(Script script);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
