using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using WebIde.Web.Hubs;

namespace WebIde.Web.Services;

// ── Redis → SignalR bridge ────────────────────────────────────────────────────
// The worker (a separate process) publishes a SubmissionResultEvent to the Redis
// channel "execution:{submissionId}" on every state change. This hosted service is
// the ONLY link that carries those events to the browser: it subscribes to
// "execution:*" and forwards each event to the SignalR group "submission:{id}"
// that ExecutionHub.SubscribeToSubmission joins the connection to.
//
// Scaling caveat: with the SignalR Redis backplane, if more than one webide-app
// instance runs this bridge, each instance's SendAsync fans out through the
// backplane and the browser receives one copy per instance. That is correct for
// the current single-instance deployment. If scaled out, gate this to a single
// instance (leader election / dedicated flag) or have the worker publish through
// the backplane directly.
public sealed class RedisSubscriptionService(
    IConnectionMultiplexer redis,
    IHubContext<ExecutionHub> hub,
    ILogger<RedisSubscriptionService> logger) : BackgroundService
{
    // Mirror of WebIde.Worker/Models/SubmissionResultEvent.cs — same property
    // names/order so System.Text.Json round-trips the worker's payload.
    private sealed record SubmissionResultEvent(
        int SubmissionId,
        string Status,
        int Score,
        int WallTimeMs,
        int PeakMemoryKb,
        IReadOnlyList<CaseVerdict>? CaseResults);

    private sealed record CaseVerdict(int Id, string Verdict, int WallMs, int PeakKb);

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var subscriber = redis.GetSubscriber();
            if (subscriber is null)
            {
                // Happens under the test factory's mock IConnectionMultiplexer,
                // which does not stub GetSubscriber(). Nothing to bridge.
                logger.LogWarning("Redis subscriber unavailable — result bridge disabled.");
                return;
            }

            await subscriber.SubscribeAsync(
                RedisChannel.Pattern("execution:*"),
                (channel, message) => _ = HandleMessageAsync(channel, message));

            logger.LogInformation("Subscribed to Redis channel execution:* — result bridge active.");
        }
        catch (Exception ex)
        {
            // Never take the web process down because the bridge couldn't start.
            logger.LogError(ex, "Failed to start Redis→SignalR result bridge.");
        }
    }

    private async Task HandleMessageAsync(RedisChannel channel, RedisValue message)
    {
        try
        {
            if (message.IsNullOrEmpty) return;

            var evt = JsonSerializer.Deserialize<SubmissionResultEvent>((string)message!, JsonOpts);
            if (evt is null) return;

            // Positional args (not a serialized object) so there is no JSON
            // property-casing ambiguity on the client.
            await hub.Clients.Group($"submission:{evt.SubmissionId}")
                .SendAsync("submissionUpdate",
                    evt.SubmissionId,
                    evt.Status,
                    evt.Score,
                    evt.WallTimeMs,
                    evt.PeakMemoryKb);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bridge result event from channel {Channel}.", channel);
        }
    }
}
