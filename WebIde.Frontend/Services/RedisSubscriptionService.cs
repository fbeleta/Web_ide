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
        // A failure here must not bring down the whole web host: BackgroundService
        // exceptions default to StopHost, so a transient Redis hiccup at startup
        // would otherwise crash the app. Log and exit the loop instead.
        ISubscriber? subscriber = null;
        try
        {
            subscriber = redis.GetSubscriber();

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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RedisSubscriptionService stopped: could not subscribe to execution channel.");
        }
        finally
        {
            if (subscriber is not null)
                await subscriber.UnsubscribeAsync(RedisChannel.Pattern("execution:*"));
        }
    }
}
