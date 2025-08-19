using Microsoft.EntityFrameworkCore;
using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Infrastructure.Data;

namespace DatariskMLOps.Infrastructure.Repositories;

public class ScriptRepository : IScriptRepository
{
    private readonly ApplicationDbContext _context;

    public ScriptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Script?> GetByIdAsync(Guid id)
    {
        return await _context.Scripts
            .Include(s => s.Executions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Script>> GetAllAsync(int page, int size)
    {
        return await _context.Scripts
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<Script> CreateAsync(Script script)
    {
        _context.Scripts.Add(script);
        await _context.SaveChangesAsync();
        return script;
    }

    public async Task<Script> UpdateAsync(Script script)
    {
        _context.Scripts.Update(script);
        await _context.SaveChangesAsync();
        return script;
    }

    public async Task DeleteAsync(Guid id)
    {
        var script = await _context.Scripts.FindAsync(id);
        if (script != null)
        {
            _context.Scripts.Remove(script);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Scripts.AnyAsync(s => s.Id == id);
    }
}
