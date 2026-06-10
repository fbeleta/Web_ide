using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace WebIde.Web.Services;

public class WorkerHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync("worker:heartbeat");
            if (value.IsNull)
                return HealthCheckResult.Unhealthy("Worker heartbeat key missing — worker may not be running");

            if (!long.TryParse((string?)value, out var unixSeconds))
                return HealthCheckResult.Unhealthy("Worker heartbeat value malformed");

            var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - unixSeconds;
            if (age > 30)
                return HealthCheckResult.Unhealthy($"Worker heartbeat stale ({age}s ago)");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Worker health check failed", ex);
        }
    }
}
