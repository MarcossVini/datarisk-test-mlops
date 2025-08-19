using System.ComponentModel.DataAnnotations;

namespace DatariskMLOps.API.DTOs;

public class ExecuteScriptRequest
{
    [Required]
    public Guid ScriptId { get; set; }

    [Required]
    public object Data { get; set; } = null!;
}
