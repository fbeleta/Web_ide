namespace WebIde.Worker.Models;

public record SubmissionJob(
    int    SubmissionId,
    int    ProblemId,
    string Language,
    string SourceCode,
    int    TimeLimitMs,
    int    MemoryLimitKb);
