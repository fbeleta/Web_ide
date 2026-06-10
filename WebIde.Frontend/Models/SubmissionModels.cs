using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Frontend.Models;

public class SubmissionCreateModel
{
    [Required(ErrorMessage = "Source code is required.")]
    [StringLength(100000, MinimumLength = 1, ErrorMessage = "Source code must not be empty.")]
    public string SourceCode { get; set; } = "";

    [Required(ErrorMessage = "Language is required.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Language must be specified.")]
    public string Language { get; set; } = "";

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
    public int Score { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Wall time must be non-negative.")]
    public int WallTimeMs { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Peak memory must be non-negative.")]
    public int PeakMemoryKb { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a user.")]
    public int UserId { get; set; }

    public string UserDisplayName { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Please select a problem.")]
    public int ProblemId { get; set; }

    public string ProblemTitle { get; set; } = "";
}

public class SubmissionEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Source code is required.")]
    [StringLength(100000, MinimumLength = 1)]
    public string SourceCode { get; set; } = "";

    [Required(ErrorMessage = "Language is required.")]
    [StringLength(30, MinimumLength = 1)]
    public string Language { get; set; } = "";

    public SubmissionStatus Status { get; set; }

    public DateTime SubmittedAt { get; set; }

    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
    public int Score { get; set; }

    [Range(0, int.MaxValue)]
    public int WallTimeMs { get; set; }

    [Range(0, int.MaxValue)]
    public int PeakMemoryKb { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a user.")]
    public int UserId { get; set; }

    public string UserDisplayName { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Please select a problem.")]
    public int ProblemId { get; set; }

    public string ProblemTitle { get; set; } = "";
}
