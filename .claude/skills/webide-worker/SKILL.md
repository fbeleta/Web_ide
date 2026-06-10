---
name: webide-worker
description: Use this skill when working in WebIde.Worker/ — creating the project, writing SubmissionWorker, SandboxOrchestrator, SubmissionEvaluator, HeartbeatService, StuckSubmissionReaper, or wiring its Program.cs. It encodes the DI lifetime rules (IDbContextFactory + DockerClient + IConnectionMultiplexer), graceful shutdown for the BackgroundService, and the Docker.DotNet memory-stats and DooD pitfalls from §10 and §14 of docs/deployment-handoff.md.
---

# WebIde Worker Service

Reference: §7 Phase 2, §10, §14 of `docs/deployment-handoff.md`. If you're writing wrapper scripts or the evaluator's normalization/comparison logic, also load `webide-sandbox`.

## DI lifetime cheat sheet (wrong lifetime = startup exception)

| Service | Lifetime | Why |
|---|---|---|
| `IConnectionMultiplexer` (StackExchange.Redis) | **Singleton** | Designed as a single shared connection. Scoped is a well-known footgun. |
| `DockerClient` (Docker.DotNet) | **Singleton** | Thread-safe; expensive to recreate. |
| `WebIdeDbContext` | **Use `IDbContextFactory<WebIdeDbContext>`** | Hosted services are singletons. Injecting `WebIdeDbContext` directly throws a DI lifetime exception at startup. Call `factory.CreateDbContext()` per job; dispose after. |
| `SandboxOrchestrator` | **Singleton** | Holds `SemaphoreSlim` slot state. |
| `SubmissionEvaluator` | **Scoped** | Stateless. |
| `HeartbeatService`, `StuckSubmissionReaper`, `SubmissionWorker`, `RedisSubscriptionService` | **Singleton** via `AddHostedService<T>` |

```csharp
services.AddDbContextFactory<WebIdeDbContext>(opts =>
    opts.UseNpgsql(connStr, o => o.MigrationsAssembly("WebIde.DAL")));
services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));
services.AddSingleton<DockerClient>(
    new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient());
services.AddSingleton<SandboxOrchestrator>();
services.AddScoped<SubmissionEvaluator>();
services.AddHostedService<HeartbeatService>();
services.AddHostedService<StuckSubmissionReaper>();
services.AddHostedService<SubmissionWorker>();
```

## Graceful shutdown (don't skip)

Worker is a long-lived `BackgroundService`. On `docker compose up -d webide-worker` redeploy, the old container gets SIGTERM with ~10s grace.

**DooD consequence:** the sandbox container is a **sibling**, not a child of the worker. Killing the worker does NOT kill the sandbox. The orchestrator must `RemoveContainerAsync(force: true)` in a `finally` block, even on cancellation.

```csharp
public class SubmissionWorker(IHostApplicationLifetime lifetime, ...) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            var jobJson = await redis.ListLeftPopAsync("submissions:queue", ...);
            if (jobJson == null) { await Task.Delay(1000, stoppingToken); continue; }

            using var jobScope = scopeFactory.CreateScope();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            try {
                await orchestrator.Run(job, cts.Token);
            } catch (OperationCanceledException) {
                await MarkInternalError(job.SubmissionId);
                throw;
            }
        }
    }
}
```

Use BLPOP with a short timeout (e.g., 1s) and a delay-loop, OR use the StackExchange.Redis async APIs. Don't block indefinitely — that prevents shutdown.

## Heartbeat

Every 5s, write `worker:heartbeat` to Redis with TTL 30s. Frontend's `/health/ready` check fails when stale. Without this, the queue silently grows when the worker crashes.

```csharp
public class HeartbeatService(IConnectionMultiplexer redis) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct) {
        var db = redis.GetDatabase();
        while (!ct.IsCancellationRequested) {
            await db.StringSetAsync("worker:heartbeat",
                DateTimeOffset.UtcNow.ToString("o"),
                TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

## Stuck submission reaper

On startup, scan for `Status = Running` rows older than `maxTimeLimit + 1m`. Mark them `InternalError`. Without this, a worker crash leaves rows stuck forever.

Run as an `IHostedService.StartAsync` task — not in `ExecuteAsync`, since you want it to complete before the worker starts processing.

## SandboxOrchestrator state

- Holds `SemaphoreSlim(maxSlots, maxSlots)` — caps concurrent sandboxes.
- Holds `ConcurrentDictionary<int, byte> activeSubmissionIds` — the startup cleanup pass for `/tmp/webide-src/*` must skip these to avoid racing the worker.
- All `docker run` calls go through this class. It's the only place that talks to `DockerClient`.

## Docker.DotNet pitfalls

### Memory stats
`InspectContainerAsync` does NOT return memory stats. Two options:
1. `GetContainerStatsAsync` — streaming endpoint. Subscribe until the container exits, take max `MemoryStats.Usage` observed.
2. **Preferred**: have the wrapper script (see `webide-sandbox`) read `memory.peak` from inside the container and emit it in `result.json`. More accurate; no race with container exit.

Set `AutoRemove = false` so the container is still inspectable after exit. Worker explicitly removes after collecting stats.

### Wall time
Don't measure around `docker run` — includes 100-500ms container creation. Wrapper measures per-subprocess inside. Trust `result.json`.

### Container creation flags
The Docker.DotNet `CreateContainerParameters` shape doesn't map 1:1 to CLI flags. Mapping:

| CLI | Docker.DotNet |
|---|---|
| `--network none` | `HostConfig.NetworkMode = "none"` |
| `--read-only` | `HostConfig.ReadonlyRootfs = true` |
| `--tmpfs /tmp:...` | `HostConfig.Tmpfs = new Dictionary<string,string>{ ["/tmp"] = "size=64m,mode=1777" }` |
| `--memory` | `HostConfig.Memory = bytes` |
| `--memory-swap` | `HostConfig.MemorySwap = bytes` (same value) |
| `--cpus 0.9` | `HostConfig.NanoCPUs = 900_000_000` |
| `--pids-limit 64` | `HostConfig.PidsLimit = 64` |
| `--user nobody:nogroup` | `Config.User = "nobody:nogroup"` |
| `--security-opt no-new-privileges` | `HostConfig.SecurityOpt = new[]{ "no-new-privileges" }` |
| `--cap-drop ALL` | `HostConfig.CapDrop = new[]{ "ALL" }` |
| `-v src:dst:ro` | `HostConfig.Mounts` with `ReadOnly = true` |

## Source dir handling

- Worker creates `/tmp/webide-src/{submissionId}/` per job, mode 700.
- Writes `solution.{ext}` and `cases.json`.
- Mounts read-only into container at `/code`.
- `finally` block: `Directory.Delete(dir, recursive: true)`.
- Worker container runs as the host's `deploy` UID (set `user: "${DEPLOY_UID}:${DEPLOY_GID}"` in compose). The host dir must be owned by that UID.

## Result publication

After evaluation, publish to Redis channel `execution:{submissionId}`. The Frontend's `RedisSubscriptionService` (Phase 3) does pattern sub on `execution:*` and bridges to `Clients.Group("submission:{id}")`.

```csharp
await redis.GetSubscriber().PublishAsync(
    RedisChannel.Literal($"execution:{submissionId}"),
    JsonSerializer.Serialize(resultEvent));
```
