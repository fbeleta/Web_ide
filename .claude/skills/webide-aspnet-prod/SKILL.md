---
name: webide-aspnet-prod
description: Use this skill when editing WebIde.Frontend/Program.cs, AuthController, ExecutionHub, SubmissionController, RedisSubscriptionService, or anything auth/SignalR/middleware-related. It encodes the production middleware order, DataProtection-on-Redis (without which every deploy logs every user out), cookie policy, antiforgery for JSON POST, and rate limiter config from §10 of docs/deployment-handoff.md.
---

# WebIde ASP.NET Production Configuration

Reference: §10, §11b of `docs/deployment-handoff.md`.

## Middleware order is load-bearing

Wrong order = silent breakage (auth bypassed, https detection broken, etc). Required order:

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { /* nginx subnet */ }, KnownProxies = { /* nginx IP */ }
});
app.UseHsts();                  // Production only
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();        // MUST be before UseAuthorization
app.UseAuthorization();
app.UseSession();
app.MapHub<ExecutionHub>("/hubs/execution").RequireRateLimiting("hub");
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
app.MapControllerRoute(/* ... */);
```

**ForwardedHeaders BEFORE auth** — otherwise `Request.Scheme == "http"` and auth redirects break.

## DataProtection MUST persist to Redis

Without this, every container restart (every deploy) wipes the key ring → every user logged out, every antiforgery token invalid, every in-flight OAuth callback fails. Three lines:

```csharp
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("WebIde");
```

`IConnectionMultiplexer` MUST be singleton. Scoped is a well-known StackExchange.Redis footgun.

## Cookie policy

```csharp
services.ConfigureApplicationCookie(o => {
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;   // Lax — see below
    o.ExpireTimeSpan = TimeSpan.FromDays(14);
    o.SlidingExpiration = true;
    o.LoginPath = "/auth/github/login";
});
```

SameSite choices and why:
- `None` — breaks OAuth correlation cookie tracking.
- `Strict` — breaks the OAuth callback redirect (cross-site).
- `Lax` — works. Use this.

## GitHub OAuth registration

```csharp
services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie()
.AddGitHub("GitHub", options => {
    options.ClientId = config["GITHUB_CLIENT_ID"]!;
    options.ClientSecret = config["GITHUB_CLIENT_SECRET"]!;
    options.CallbackPath = "/auth/github/callback";
    options.Scope.Add("read:user");
    options.SaveTokens = false;
    options.Events.OnCreatingTicket = async ctx => {
        // Upsert User by GitHubId, set Name/AvatarUrl, attach claims
    };
});
```

Callback path in app config MUST match the GitHub OAuth app's "Authorization callback URL" (§12). Trailing-slash differences will silently fail.

## Antiforgery for JSON POST

Default `[ValidateAntiForgeryToken]` expects form data. The submission endpoint is `fetch` + JSON, so use the explicit API:

```csharp
[HttpPost, Authorize, EnableRateLimiting("submission")]
public async Task<IActionResult> Submit(
    [FromBody] SubmitDto dto,
    [FromServices] IAntiforgery antiforgery)
{
    await antiforgery.ValidateRequestAsync(HttpContext);
    /* ... */
}
```

Razor view emits the token; JS reads it and sends in header:

```html
@Html.AntiForgeryToken()
<script>
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
  await fetch('/submissions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
    body: JSON.stringify({ problemId, language, sourceCode })
  });
</script>
```

## Rate limiter

```csharp
services.AddRateLimiter(o => {
    o.AddFixedWindowLimiter("submission", opt => {
        opt.PermitLimit = 5; opt.Window = TimeSpan.FromMinutes(1);
    });
    o.AddTokenBucketLimiter("auth", opt => {
        opt.TokenLimit = 10; opt.TokensPerPeriod = 10;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
    });
    o.RejectionStatusCode = 429;
});
```

Apply with `[EnableRateLimiting("submission")]` on `SubmissionController.Submit`, `[EnableRateLimiting("auth")]` on `AuthController` actions.

## Health check split (deploy pipeline depends on this)

- `/health` (liveness): always 200 if the process is up. No DB or Redis check. A flapping DB should not take the load balancer down.
- `/health/ready` (readiness): checks Postgres connectivity, Redis connectivity, AND worker heartbeat (`worker:heartbeat` key in Redis, must be < 30s old). 503 on any fail. The deploy pipeline polls this, NOT `/health`.

```csharp
services.AddHealthChecks()
    .AddNpgSql(pgConn, tags: new[] { "ready" })
    .AddRedis(redisConn, tags: new[] { "ready" })
    .AddCheck<WorkerHealthCheck>("worker", tags: new[] { "ready" });
```

## SignalR backplane (load-bearing, not optional)

```csharp
services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
```

The worker is a separate process. The bridge Redis pub/sub → SignalR groups is the ONLY way the browser hears about results. Not "for future scaling" — for THIS architecture.

## ExecutionHub authorization

Hub must enforce that the connecting user owns the submission they're subscribing to:

```csharp
[Authorize]
public class ExecutionHub : Hub {
    public async Task SubscribeToSubmission(int submissionId) {
        var userId = int.Parse(Context.User!.FindFirst("sub")!.Value);
        var owns = await _submissionRepo.IsOwnedBy(submissionId, userId);
        if (!owns) throw new HubException("forbidden");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"submission:{submissionId}");
    }
}
```

Without ownership check, any logged-in user can subscribe to anyone's submission stream.
