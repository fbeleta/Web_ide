using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Worker.Models;
using WebIde.Worker.Services;

namespace WebIde.Worker.Workers;

public class SubmissionWorker(
    IServiceScopeFactory scopeFactory,
    IConnectionMultiplexer redis,
    SandboxOrchestrator orchestrator,
    ILogger<SubmissionWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Submission worker started");
        var db = redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            RedisValue value;
            try
            {
                value = await db.ListLeftPopAsync("submissions:queue");
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Redis error while polling queue");
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            if (value.IsNull)
            {
                await Task.Delay(250, stoppingToken);
                continue;
            }

            SubmissionJob? job;
            try
            {
                job = JsonSerializer.Deserialize<SubmissionJob>((string)value!, JsonOpts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize job: {Value}", (string?)value);
                continue;
            }

            if (job is null) continue;

            try
            {
                await ProcessJobAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                await MarkInternalErrorAsync(job.SubmissionId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error processing submission {Id}", job.SubmissionId);
                await MarkInternalErrorAsync(job.SubmissionId);
            }
        }
    }

    private async Task ProcessJobAsync(SubmissionJob job, CancellationToken ct)
    {
        logger.LogInformation("Processing submission {Id} ({Lang})", job.SubmissionId, job.Language);

        using var scope = scopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WebIdeDbContext>>();

        // Load problem + test cases from DB
        Problem? problem;
        List<WebIde.Model.TestCase> testCases;
        using (var loadCtx = factory.CreateDbContext())
        {
            problem = await loadCtx.Problems
                .FirstOrDefaultAsync(p => p.Id == job.ProblemId, ct);
            if (problem is null)
            {
                logger.LogError("Problem {ProblemId} not found for submission {SubId}", job.ProblemId, job.SubmissionId);
                await MarkInternalErrorAsync(job.SubmissionId);
                return;
            }
            testCases = await loadCtx.TestCases
                .Where(tc => tc.ProblemId == job.ProblemId)
                .OrderBy(tc => tc.OrderIndex)
                .ToListAsync(ct);
        }

        // Mark Running + notify browser
        await UpdateStatusAsync(factory, job.SubmissionId, SubmissionStatus.Running);
        await PublishStatusAsync(job.SubmissionId, SubmissionStatus.Running);

        // Run sandbox
        var run = await orchestrator.RunAsync(job, problem, testCases, ct);

        // Evaluate
        var evaluator = scope.ServiceProvider.GetRequiredService<SubmissionEvaluator>();
        var result = evaluator.Evaluate(run, testCases);

        // Persist
        await PersistResultAsync(factory, job.SubmissionId, result, testCases);

        // Publish final result to Redis for SignalR bridge
        var evt = new SubmissionResultEvent(
            job.SubmissionId,
            result.Status.ToString(),
            result.Score,
            result.WallTimeMs,
            result.PeakMemoryKb,
            result.CaseResults.Select(cr => new CaseVerdict(cr.Id, cr.Verdict, cr.WallMs, cr.PeakKb)).ToList());

        await redis.GetSubscriber().PublishAsync(
            RedisChannel.Literal($"execution:{job.SubmissionId}"),
            JsonSerializer.Serialize(evt));

        logger.LogInformation("Submission {Id} completed: {Status}", job.SubmissionId, result.Status);
    }

    private const int DbFieldCap = 65_536; // 64 KB per §6a

    private async Task PersistResultAsync(
        IDbContextFactory<WebIdeDbContext> factory, int submissionId,
        EvaluationResult result, IList<WebIde.Model.TestCase> testCases)
    {
        using var ctx = factory.CreateDbContext();
        var submission = await ctx.Submissions.FindAsync(submissionId);
        if (submission is null) return;

        // ExecutionResult is per-test-case in the schema (TestCaseId is a required
        // FK), so write one row per case with that case's verdict + output.
        var execResults = new List<ExecutionResult>();
        if (result.CaseResults.Count > 0)
        {
            foreach (var cr in result.CaseResults)
            {
                var verdict = MapVerdict(cr.Verdict);
                execResults.Add(new ExecutionResult
                {
                    SubmissionId   = submissionId,
                    TestCaseId     = cr.Id,
                    Stdout         = Truncate(cr.Stdout ?? "", DbFieldCap),
                    Stderr         = Truncate(cr.Stderr ?? "", DbFieldCap),
                    ExitCode       = 0,
                    WallTimeMs     = cr.WallMs,
                    PeakMemoryKb   = cr.PeakKb,
                    Verdict        = verdict,
                    TimedOut       = verdict == Verdict.TimeLimitExceeded,
                    MemoryExceeded = verdict == Verdict.MemoryLimitExceeded,
                });
            }
        }
        else if (testCases.Count > 0)
        {
            // No per-case results (compile error / internal error). Attach one
            // diagnostic row to the first test case so the UI can surface stderr.
            var verdict = result.Status == SubmissionStatus.CompileError
                ? Verdict.CompileError : Verdict.RuntimeError;
            execResults.Add(new ExecutionResult
            {
                SubmissionId   = submissionId,
                TestCaseId     = testCases[0].Id,
                Stdout         = "",
                Stderr         = Truncate(result.Stderr ?? "", DbFieldCap),
                ExitCode       = result.Status == SubmissionStatus.CompileError ? 2 : 1,
                WallTimeMs     = 0,
                PeakMemoryKb   = 0,
                Verdict        = verdict,
                TimedOut       = false,
                MemoryExceeded = false,
            });
        }

        if (execResults.Count > 0)
        {
            ctx.ExecutionResults.AddRange(execResults);
            await ctx.SaveChangesAsync();
            // Point the submission at the most representative (worst) case so the
            // single-result UI shows the failing case.
            var primary = execResults.OrderByDescending(er => VerdictRank(er.Verdict)).First();
            submission.ExecutionResultId = primary.Id;
        }

        submission.Status        = result.Status;
        submission.Score         = result.Score;
        submission.WallTimeMs    = result.WallTimeMs;
        submission.PeakMemoryKb  = result.PeakMemoryKb;
        await ctx.SaveChangesAsync();
    }

    private static Verdict MapVerdict(string v) => v switch
    {
        "Accepted"            => Verdict.Accepted,
        "WrongAnswer"         => Verdict.WrongAnswer,
        "TimeLimitExceeded"   => Verdict.TimeLimitExceeded,
        "MemoryLimitExceeded" => Verdict.MemoryLimitExceeded,
        "RuntimeError"        => Verdict.RuntimeError,
        "CompileError"        => Verdict.CompileError,
        _                     => Verdict.Pending,
    };

    private static int VerdictRank(Verdict v) => v switch
    {
        Verdict.MemoryLimitExceeded => 6,
        Verdict.TimeLimitExceeded   => 5,
        Verdict.RuntimeError        => 4,
        Verdict.CompileError        => 4,
        Verdict.WrongAnswer         => 3,
        Verdict.Accepted            => 1,
        _                           => 0,
    };

    private static string Truncate(string s, int maxBytes)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(s) <= maxBytes) return s;
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        return System.Text.Encoding.UTF8.GetString(bytes, 0, maxBytes) + "\n[truncated]";
    }

    private async Task UpdateStatusAsync(IDbContextFactory<WebIdeDbContext> factory, int submissionId, SubmissionStatus status)
    {
        using var ctx = factory.CreateDbContext();
        var submission = await ctx.Submissions.FindAsync(submissionId);
        if (submission is null) return;
        submission.Status = status;
        await ctx.SaveChangesAsync();
    }

    private async Task MarkInternalErrorAsync(int submissionId)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WebIdeDbContext>>();
            await UpdateStatusAsync(factory, submissionId, SubmissionStatus.InternalError);
            await PublishStatusAsync(submissionId, SubmissionStatus.InternalError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark submission {Id} as InternalError", submissionId);
        }
    }

    private async Task PublishStatusAsync(int submissionId, SubmissionStatus status)
    {
        var evt = new SubmissionResultEvent(submissionId, status.ToString(), 0, 0, 0, Array.Empty<CaseVerdict>());
        await redis.GetSubscriber().PublishAsync(
            RedisChannel.Literal($"execution:{submissionId}"),
            JsonSerializer.Serialize(evt));
    }
}
