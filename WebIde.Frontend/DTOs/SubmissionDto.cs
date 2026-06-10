using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class SubmissionDto
{
    public int Id { get; set; }
    public string Language { get; set; } = "";
    public string Status { get; set; } = "";
    public string SourceCode { get; set; } = "";
    public DateTime SubmittedAt { get; set; }
    public int Score { get; set; }
    public int WallTimeMs { get; set; }
    public int PeakMemoryKb { get; set; }
    public int UserId { get; set; }
    public int ProblemId { get; set; }
}

public class CreateSubmissionDto
{
    [Required]
    public int ProblemId { get; set; }

    [Required]
    public string Language { get; set; } = "";

    [Required]
    public string SourceCode { get; set; } = "";
}
