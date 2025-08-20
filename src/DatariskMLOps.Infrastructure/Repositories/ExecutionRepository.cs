using Microsoft.EntityFrameworkCore;
using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Infrastructure.Data;

namespace DatariskMLOps.Infrastructure.Repositories;

public class ExecutionRepository : IExecutionRepository
{
    private readonly ApplicationDbContext _context;

    public ExecutionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Execution?> GetByIdAsync(Guid id)
    {
        return await _context.Executions
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Execution>> GetByScriptIdAsync(Guid scriptId)
    {
        return await _context.Executions
            .Where(e => e.ScriptId == scriptId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();
    }

    public async Task<Execution> CreateAsync(Execution execution)
    {
        _context.Executions.Add(execution);
        await _context.SaveChangesAsync();
        return execution;
    }

    public async Task<Execution> UpdateAsync(Execution execution)
    {
        _context.Executions.Update(execution);
        await _context.SaveChangesAsync();
        return execution;
    }

    public async Task DeleteAsync(Guid id)
    {
        var execution = await _context.Executions.FindAsync(id);
        if (execution != null)
        {
            _context.Executions.Remove(execution);
            await _context.SaveChangesAsync();
        }
    }
}
