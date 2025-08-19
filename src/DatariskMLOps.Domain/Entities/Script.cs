namespace DatariskMLOps.Domain.Entities;

public class Script
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Execution> Executions { get; set; } = new List<Execution>();
    public virtual ICollection<ScriptVersion> Versions { get; set; } = new List<ScriptVersion>();
}
