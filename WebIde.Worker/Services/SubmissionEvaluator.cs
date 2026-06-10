using System.Text.Json;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Worker.Models;

namespace WebIde.Worker.Services;

public class EvaluationResult
{
    public SubmissionStatus Status { get; init; }
    public int Score { get; init; }
    public int WallTimeMs { get; init; }
    public int PeakMemoryKb { get; init; }
    public IReadOnlyList<SandboxCaseResult> CaseResults { get; init; } = Array.Empty<SandboxCaseResult>();
    // Truncated per-case JSON for DB storage (64 KB cap)
    public string ResultJson { get; init; } = "[]";
    public string Stderr { get; init; } = "";
}

public class SubmissionEvaluator
{
    private const int DbJsonCap = 65_536; // 64 KB

    public EvaluationResult Evaluate(SandboxRunResult run, IList<TestCase> testCases)
    {
        // Compile error — no per-case results
        if (run.ExitCode == 2)
            return new EvaluationResult
            {
                Status  = SubmissionStatus.CompileError,
                Stderr  = Truncate(run.Stderr, DbJsonCap),
            };

        // Wrapper crash or worker hard timeout
        if (run.ExitCode != 0 && run.ExitCode != 124)
            return new EvaluationResult { Status = SubmissionStatus.InternalError };

        if (string.IsNullOrWhiteSpace(run.Stdout))
            return new EvaluationResult { Status = SubmissionStatus.InternalError };

        List<SandboxCaseResult>? cases;
        try
        {
            cases = JsonSerializer.Deserialize<List<SandboxCaseResult>>(run.Stdout);
        }
        catch
        {
            return new EvaluationResult { Status = SubmissionStatus.InternalError };
        }

        if (cases is null || cases.Count == 0)
            return new EvaluationResult { Status = SubmissionStatus.InternalError };

        var pointsById = testCases.ToDictionary(tc => tc.Id, tc => tc.Points);
        var status     = SubmissionStatus.Accepted;
        var score      = 0;
        var wallMs     = 0;
        var peakKb     = 0;

        foreach (var cr in cases)
        {
            var caseStatus = ParseVerdict(cr.Verdict);
            status  = WorseVerdict(status, caseStatus);
            wallMs  = Math.Max(wallMs,  cr.WallMs);
            peakKb  = Math.Max(peakKb,  cr.PeakKb);
            if (caseStatus == SubmissionStatus.Accepted && pointsById.TryGetValue(cr.Id, out var pts))
                score += pts;
        }

        var resultJson = Truncate(run.Stdout, DbJsonCap);

        return new EvaluationResult
        {
            Status      = status,
            Score       = score,
            WallTimeMs  = wallMs,
            PeakMemoryKb = peakKb,
            CaseResults = cases,
            ResultJson  = resultJson,
        };
    }

    private static SubmissionStatus ParseVerdict(string v) => v switch
    {
        "Accepted"           => SubmissionStatus.Accepted,
        "WrongAnswer"        => SubmissionStatus.WrongAnswer,
        "TimeLimitExceeded"  => SubmissionStatus.TimeLimitExceeded,
        "MemoryLimitExceeded"=> SubmissionStatus.MemoryLimitExceeded,
        "RuntimeError"       => SubmissionStatus.RuntimeError,
        _                    => SubmissionStatus.InternalError,
    };

    // Precedence: MLE > TLE > RuntimeError > WrongAnswer > Accepted
    private static SubmissionStatus WorseVerdict(SubmissionStatus a, SubmissionStatus b)
    {
        static int Rank(SubmissionStatus s) => s switch
        {
            SubmissionStatus.MemoryLimitExceeded => 5,
            SubmissionStatus.TimeLimitExceeded   => 4,
            SubmissionStatus.RuntimeError        => 3,
            SubmissionStatus.WrongAnswer         => 2,
            SubmissionStatus.Accepted            => 1,
            _                                    => 0,
        };
        return Rank(a) >= Rank(b) ? a : b;
    }

    private static string Truncate(string s, int maxBytes)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(s) <= maxBytes) return s;
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        return System.Text.Encoding.UTF8.GetString(bytes, 0, maxBytes)
               + "\n[truncated]";
    }
}
