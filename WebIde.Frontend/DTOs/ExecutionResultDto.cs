namespace WebIde.Web.DTOs;

public class ExecutionResultDto
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int TestCaseId { get; set; }
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public int ExitCode { get; set; }
    public int WallTimeMs { get; set; }
    public int PeakMemoryKb { get; set; }
    public string Verdict { get; set; } = "";
    public bool TimedOut { get; set; }
    public bool MemoryExceeded { get; set; }
}
