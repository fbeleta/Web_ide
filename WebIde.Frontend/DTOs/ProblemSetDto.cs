using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class ProblemSetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int OrganizationId { get; set; }
    public List<int> ProblemIds { get; set; } = [];
}

public class CreateProblemSetDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";
    public bool IsPublic { get; set; }
    public int OrganizationId { get; set; }
    public List<int> ProblemIds { get; set; } = [];
}

public class UpdateProblemSetDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public List<int>? ProblemIds { get; set; }
}
