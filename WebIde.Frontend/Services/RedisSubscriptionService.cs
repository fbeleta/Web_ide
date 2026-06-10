using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using WebIde.Web.Hubs;

namespace WebIde.Web.Services;

public sealed class RedisSubscriptionService(
    IConnectionMultiplexer redis,
    IHubContext<ExecutionHub> hubContext,
    ILogger<RedisSubscriptionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Pattern("execution:*"),
            async (channel, message) =>
            {
                var channelStr = channel.ToString();
                // channel format: "execution:{submissionId}"
                var parts = channelStr.Split(':');
                if (parts.Length != 2 || string.IsNullOrEmpty(message))
                    return;

                var submissionId = parts[1];
                try
                {
                    await hubContext.Clients
                        .Group($"submission:{submissionId}")
                        .SendAsync("result", message.ToString(), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to forward result for submission {SubmissionId}", submissionId);
                }
            });

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        await subscriber.UnsubscribeAsync(RedisChannel.Pattern("execution:*"));
    }
}
