namespace WebIde.Worker.Models;

// Published to Redis channel "execution:{submissionId}" after evaluation.
// RedisSubscriptionService in the Frontend bridges this to SignalR.
public record SubmissionResultEvent(
    int                       SubmissionId,
    string                    Status,
    int                       Score,
    int                       WallTimeMs,
    int                       PeakMemoryKb,
    IReadOnlyList<CaseVerdict> CaseResults);

public record CaseVerdict(int Id, string Verdict, int WallMs, int PeakKb);
