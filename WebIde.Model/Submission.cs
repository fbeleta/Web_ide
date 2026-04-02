using WebIde.Model.Enums;

namespace WebIde.Model;

public class Submission
{
    public int Id { get; set; }
    public required string SourceCode { get; set; }
    public required string Language { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int Score { get; set; }
    public int WallTimeMs { get; set; }
    public int PeakMemoryKb { get; set; }
    public required User User { get; set; }
    public required Problem Problem { get; set; }
    public ExecutionResult? ExecutionResult { get; set; }
}
