using System.Text.Json;

namespace DatariskMLOps.Domain.Entities;

public class Execution
{
    public Guid Id { get; set; }
    public Guid ScriptId { get; set; }
    public Guid? ScriptVersionId { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public JsonDocument InputData { get; set; } = null!;
    public JsonDocument? OutputData { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ExecutionTimeMs { get; set; }

    // Navigation properties
    public virtual Script Script { get; set; } = null!;
    //public virtual ScriptVersion? ScriptVersion { get; set; } // Temporariamente comentado
}

public enum ExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}
