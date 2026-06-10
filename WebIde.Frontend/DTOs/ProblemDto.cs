using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class ProblemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int TimeLimitMs { get; set; }
    public int MemoryLimitKb { get; set; }
    public double? FloatTolerance { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}

public class CreateProblemDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    public string Difficulty { get; set; } = "Easy";
    public int TimeLimitMs { get; set; } = 2000;
    public int MemoryLimitKb { get; set; } = 262144;
    public double? FloatTolerance { get; set; }
    public string AuthorUsername { get; set; } = "";
    public List<int> TagIds { get; set; } = [];
}

public class UpdateProblemDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public int? TimeLimitMs { get; set; }
    public int? MemoryLimitKb { get; set; }
    public double? FloatTolerance { get; set; }
    public List<int>? TagIds { get; set; }
}
