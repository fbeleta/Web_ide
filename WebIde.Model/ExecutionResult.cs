using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Model;

public class ExecutionResult
{
    [Key]
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int TestCaseId { get; set; }
    public required string Stdout { get; set; }
    public required string Stderr { get; set; }
    public int ExitCode { get; set; }
    public int WallTimeMs { get; set; }
    public int PeakMemoryKb { get; set; }
    public Verdict Verdict { get; set; }
    public bool TimedOut { get; set; }
    public bool MemoryExceeded { get; set; }

    public virtual Submission Submission { get; set; } = null!;
    public virtual TestCase TestCase { get; set; } = null!;
}
