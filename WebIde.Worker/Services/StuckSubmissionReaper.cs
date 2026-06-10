using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebIde.DAL;
using WebIde.Model.Enums;

namespace WebIde.Worker.Services;

// Runs once at startup to mark Running submissions left over from a prior worker crash.
public class StuckSubmissionReaper(
    IServiceScopeFactory scopeFactory,
    ILogger<StuckSubmissionReaper> logger,
    SandboxOrchestrator orchestrator) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WebIdeDbContext>>()
                      .CreateDbContext();

        // Any Running submission not in the active set and older than 5 min is stuck.
        var cutoff = DateTime.UtcNow.AddMinutes(-5);
        var stuck = await db.Submissions
            .Where(s => s.Status == SubmissionStatus.Running && s.SubmittedAt < cutoff)
            .ToListAsync(ct);

        var active = orchestrator.ActiveSubmissionIds;
        var reaped = 0;

        foreach (var s in stuck)
        {
            if (active.Contains(s.Id)) continue;
            s.Status = SubmissionStatus.InternalError;
            reaped++;
        }

        if (reaped > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Reaped {Count} stuck Running submission(s)", reaped);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
