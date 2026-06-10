using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace WebIde.Worker.Services;

public class HeartbeatService(IConnectionMultiplexer redis) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var db = redis.GetDatabase();
        while (!ct.IsCancellationRequested)
        {
            await db.StringSetAsync(
                "worker:heartbeat",
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
