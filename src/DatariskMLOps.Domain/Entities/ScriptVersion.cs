using System.ComponentModel.DataAnnotations;

namespace DatariskMLOps.Domain.Entities;

public class ScriptVersion
{
    public Guid Id { get; set; }
    public Guid ScriptId { get; set; }
    public int Version { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }

    // Navigation properties
    public virtual Script Script { get; set; } = null!;
    public virtual ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
