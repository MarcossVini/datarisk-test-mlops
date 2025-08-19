using DatariskMLOps.Domain.Entities;

namespace DatariskMLOps.API.DTOs;

public class ExecutionDto
{
    public Guid Id { get; set; }
    public Guid ScriptId { get; set; }
    public string Status { get; set; } = string.Empty;
    public object InputData { get; set; } = null!;
    public object? OutputData { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public ScriptDto? Script { get; set; }
}
