using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Frontend.Models;

public class ProblemCreateModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 10000 characters.")]
    public string Description { get; set; } = "";

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

    [Range(100, 10000, ErrorMessage = "Time limit must be between 100ms and 10000ms.")]
    public int TimeLimitMs { get; set; } = 1000;

    [Range(1024, 524288, ErrorMessage = "Memory limit must be between 1024KB and 524288KB.")]
    public int MemoryLimitKb { get; set; } = 65536;

    [Required(ErrorMessage = "Author username is required.")]
    [StringLength(50)]
    public string AuthorUsername { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // IDs from the autocomplete multi-select
    public List<int> TagIds { get; set; } = new();
}

public class ProblemEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 10000 characters.")]
    public string Description { get; set; } = "";

    public DifficultyLevel Difficulty { get; set; }

    [Range(100, 10000, ErrorMessage = "Time limit must be between 100ms and 10000ms.")]
    public int TimeLimitMs { get; set; }

    [Range(1024, 524288, ErrorMessage = "Memory limit must be between 1024KB and 524288KB.")]
    public int MemoryLimitKb { get; set; }

    [Required(ErrorMessage = "Author username is required.")]
    [StringLength(50)]
    public string AuthorUsername { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public List<int> TagIds { get; set; } = new();
}
