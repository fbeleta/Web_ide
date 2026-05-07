using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebIde.Model.Enums;

namespace WebIde.Model;

public class Submission
{
    [Key]
    public int Id { get; set; }
    public required string SourceCode { get; set; }
    public required string Language { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int Score { get; set; }
    public int WallTimeMs { get; set; }
    public int PeakMemoryKb { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    public int ProblemId { get; set; }
    [ForeignKey("ProblemId")]
    public virtual Problem Problem { get; set; } = null!;
    public int? ExecutionResultId { get; set; }
    [ForeignKey("ExecutionResultId")]
    public virtual ExecutionResult? ExecutionResult { get; set; }
}
