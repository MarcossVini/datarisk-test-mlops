using System.ComponentModel.DataAnnotations;

namespace DatariskMLOps.API.DTOs;

public class CreateScriptRequest
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
