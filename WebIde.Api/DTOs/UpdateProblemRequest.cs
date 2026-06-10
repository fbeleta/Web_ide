using System.ComponentModel.DataAnnotations;

namespace WebIde.Api.DTOs;

public class UpdateProblemRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Difficulty { get; set; } = string.Empty;

    [Range(1, 60_000)]
    public int TimeLimitMs { get; set; }

    [Range(1, 1_048_576)]
    public int MemoryLimitKb { get; set; }

    public List<string>? Tags { get; set; }
}
