# WebIde — Deployment Handoff Document

**Date:** 2026-06-06 (rev. 2026-06-06 — production-grade pass)
**Author:** Claude Code
**Status:** Plan approved — ready for implementation
**Branch:** develop → main

---

## 1. What This Document Is

A complete handoff specification for taking WebIde from a read-only local .NET app to a production-ready, auto-deploying competitive coding platform on Hetzner. Any engineer picking this up should be able to implement end-to-end from this document alone.

This revision treats the platform as **production-grade**: real users, hostile submissions, HTTPS required, persistent data, recoverable from a single-host failure.

---

## 2. Current State

| Area | Status |
|---|---|
| ASP.NET MVC web app | ✅ Working (read-only, no auth) |
| PostgreSQL + EF Core | ✅ Fully integrated, migrations exist |
| Tailwind CSS + Razor views | ✅ Brutalist design system applied (via Tailwind Play CDN — must be replaced, see §7 Phase 0) |
| Code execution | ❌ Data model exists, no implementation |
| Authentication | ❌ None |
| Docker / docker-compose | ❌ Not created |
| CI/CD | ❌ Not created |
| Nginx + HTTPS | ❌ Not created |

**Scope note for v1:** `ProblemSet`, `Organization`, and `Tag` entities exist in the model but ship as **read-only seeded data**. No admin UI in v1; managed via SQL until the admin-UI phase (§15).

---

## 3. Target Architecture

```
                    ┌──────────────────────────────────────────────┐
                    │              Hetzner VPS                     │
                    │         (4 cores / 8 GB RAM)                 │
                    │                                              │
Internet ──HTTPS──▶ │  nginx :80/:443  (TLS termination, HSTS)     │
                    │      │                                       │
                    │      ├──/auth/* /submissions/* (rate limited)│
                    │      ├──/hubs/* (WebSocket upgrade)          │
                    │      └──/seq/* (basic-auth gated)            │
                    │      │                                       │
                    │      ▼                                       │
                    │  webide-app :8080  (ASP.NET MVC + SignalR)   │
                    │      │  ▲                                    │
                    │      │  │ pub/sub (SignalR backplane)        │
                    │   queue│ │                                   │
                    │      ▼  │                                    │
                    │  webide-worker (.NET Worker Service)         │
                    │      │                                       │
                    │      ▼ Docker socket (DooD)                  │
                    │  sandbox containers (sibling, ephemeral)     │
                    │      gcc / python / node                     │
                    │                                              │
                    │  postgres  redis  seq   (Docker net only)    │
                    │  (no published ports)                        │
                    └──────────────────────────────────────────────┘
```

### Why Docker-out-of-Docker (DooD)?
The worker mounts `/var/run/docker.sock` from the host so sandbox containers run as siblings (not children) on the host daemon. This avoids the `--privileged` requirement of Docker-in-Docker and is simpler to operate on a single-node deployment. The worker container itself never runs user code; sandbox containers have zero permissions (§6).

### Network exposure
- Only nginx publishes ports (80, 443) to the host.
- `postgres`, `redis`, `seq` listen on the internal `webide-net` Docker network only — **no `ports:` directive** in compose. Inspect Postgres with `docker compose exec postgres psql`; access Seq via `https://YOUR_DOMAIN/seq/` behind nginx basic auth.

---

## 4. Resource Allocation

| Service | CPU limit | RAM limit | Notes |
|---|---|---|---|
| nginx | 0.1 | 64 MB | Reverse proxy + TLS |
| webide-app | 0.5 | 512 MB | ASP.NET MVC + SignalR |
| webide-worker | 0.3 | 256 MB | Manages containers, doesn't run code |
| postgres | 0.5 | 1 GB | Primary datastore (tune `shared_buffers=256MB`) |
| redis | 0.1 | 128 MB | Queue + sessions + SignalR backplane + DataProtection keys |
| seq | 0.3 | 512 MB | Log aggregation (14-day retention) |
| postgres-backup | 0.1 | 64 MB | Nightly `pg_dump`, rsync to offsite |
| **sandbox slot 1** | **0.9** | **512 MB** | Per-run limit, enforced by Docker |
| **sandbox slot 2** | **0.9** | **512 MB** | Max 2 concurrent at once |
| OS overhead | 0.3 | 512 MB | — |
| **Total** | **4.0** | **~4 GB** | Leaves ~4 GB headroom |

Sandbox CPU intentionally set to 0.9 (not 1.0) so the app/worker keep responsiveness under burst load — CFS throttling on the user-facing path is worse than slightly slower judging.

**Dynamic slot calculation** (for future server upgrades):
```
slots = min(
  floor((totalRam     - reservedRam) / sandboxMemMb),
  floor((totalCpu     - reservedCpu) / sandboxCpu)
)
```
Both bounds matter. RAM-only is wrong on small-core boxes; CPU-only is wrong on small-RAM boxes. Controlled by `Worker__MaxConcurrentSandboxes` env var (override if you want fewer slots than the formula allows).

---

## 5. End-to-End Submission Flow

```
1. User → GET /problems/{id}
         └─ Monaco editor (CDN), language picker, Submit button

2. User clicks Submit
         └─ fetch POST /submissions (JSON: problemId, language, sourceCode)
            with header RequestVerificationToken: <antiforgery>
         └─ Rate limiter: 5/min per user (429 on excess)
         └─ Antiforgery validated via IAntiforgery.ValidateRequestAsync
         └─ Submission record created (status = Pending)
         └─ SubmissionJob JSON pushed to Redis list "submissions:queue"
         └─ Response: { submissionId: 42 }

3. Browser opens SignalR connection
         └─ hub.invoke("SubscribeToSubmission", 42)
         └─ Status chip shows: PENDING (spinner)

4. Worker (BLPOP)
         └─ Deserializes SubmissionJob
         └─ Acquires SemaphoreSlim slot (blocks if both slots busy)
         └─ Updates Submission.Status → Running
         └─ Publishes Running event → browser shows: RUNNING

5. Worker spawns ONE sandbox container per submission
         └─ Writes source to /tmp/webide-src/{id}/solution.{ext}
         └─ Writes test cases to /tmp/webide-src/{id}/cases.json
         └─ docker run [see §6 for full flags]
         └─ Wrapper iterates cases internally (see §6a)
         └─ Wrapper exits with summary; per-case results in /tmp/result.json
         └─ Worker reads results from stdout (capped) + container exit

6. Container finishes (or is killed)
         └─ Worker collects per-case verdicts, wall time, peak memory
         └─ Evaluator computes final status + score (see §6a)
         └─ Writes ExecutionResult + updates Submission.Status
         └─ Publishes result to Redis channel "execution:42"

7. RedisSubscriptionService (IHostedService in app)
         └─ Receives from "execution:*" pattern subscription
         └─ Pushes to IHubContext<ExecutionHub>.Clients.Group("submission:42")

8. Browser receives ExecutionResult via SignalR
         └─ Status chip: ACCEPTED / WRONG_ANSWER / TIME_LIMIT / COMPILE_ERROR / RUNTIME_ERROR
         └─ Wall time + memory + per-case verdicts displayed
```

**Exit codes from sandbox wrapper (final result):**

| Exit | Meaning | Maps to status |
|---|---|---|
| 0 | All cases handled (per-case verdicts in JSON) | Computed from JSON: Accepted/WrongAnswer/TLE/MLE/RuntimeError |
| 2 | Compile failed (C/C++ only) | CompileError |
| 124 | Wrapper itself timed out (defense-in-depth) | TimeLimitExceeded |
| ≠ 0,2,124 | Wrapper crash | InternalError |

Note: Python and Node have no compile step. A `SyntaxError` at runtime exits 1 and is mapped to `RuntimeError`, **not** `CompileError`. This is intentional: there is no separate compile phase to fail in, so a parse error is a runtime error from the platform's perspective. UI should still surface the stderr so users see the syntax error.

---

## 6. Sandbox Security Model

Every sandbox container runs with these exact Docker flags — **non-negotiable**:

```bash
docker run --rm \
  --network none \              # zero network access
  --read-only \                 # root filesystem immutable
  --tmpfs /tmp:size=64m,mode=1777,exec \  # exec required — C/C++ runs /tmp/a.out (Docker tmpfs is noexec by default)
  --memory $MEMORY_MB \         # min(problem.MemoryLimitKb/1024, 512)
  --memory-swap $MEMORY_MB \    # disable swap (no swap escape)
  --cpus 0.9 \                  # matches §4 slot allocation
  --pids-limit 64 \             # cgroup PIDs cap — defeats fork bombs
  --user nobody:nogroup \       # non-root inside container
  --security-opt no-new-privileges \
  --cap-drop ALL \              # drop all Linux capabilities
  --ulimit fsize=67108864 \     # 64 MB max file size (defense in depth)
  -v /tmp/webide-src/{id}:/code:ro \  # source + cases: read-only
  webide-sandbox-{lang}@sha256:{digest} \
  /code/solution.{ext} /code/cases.json
```

### Why `--pids-limit`, not `--ulimit nproc`
`--ulimit nproc` is a *per-UID rlimit*, applied at the host level. With containers sharing UIDs, nproc aggregates across containers and the host, so a fork bomb can exhaust the host's UID budget even when contained. `--pids-limit` is a *cgroup pids controller* — strictly per-container — and is the correct primitive.

### Why `--user nobody:nogroup`
Defense in depth. Even with `--cap-drop ALL` and `no-new-privileges`, root inside a namespace has more attack surface (mount, ptrace, specific syscalls). `nobody:nogroup` costs nothing and reduces it. Each sandbox Dockerfile must add `RUN adduser -D -H -u 65534 nobody 2>/dev/null || true && addgroup -S nogroup 2>/dev/null || true` (Alpine).

### Why pin image digests
`@sha256:{digest}` rather than `:latest` makes the sandbox behavior reproducible across deploys. Without it, a base-image refresh changes language minor versions silently and breaks judging determinism.

**What code CANNOT do:**
- Make network requests (outbound or inbound) — `--network none`
- Write to filesystem (except `/tmp`, capped 64 MB) — `--read-only`
- Fork more than 64 processes — `--pids-limit`
- Escalate privileges — `--cap-drop ALL` + `no-new-privileges`
- Read other submissions' source files — isolated host dir + read-only mount
- Communicate with any other container — no network
- Swap to disk — `--memory-swap` matches `--memory`

**C/C++ compile strategy:** A `compile-and-run.sh` wrapper inside the gcc image compiles to `/tmp/a.out` then runs it against each test case. Exit 2 = compile failure. For Python/Node, the wrapper directly invokes the interpreter per case.

---

## 6a. Judging Contract

Without this section, "WRONG_ANSWER" disputes will dominate the bug tracker. These rules are normative.

### Multi-test-case execution model
- One sandbox container per submission.
- Wrapper script reads `/code/cases.json` (array of `{ id, stdin, expected }`).
- For each case: spawn the user program as a subprocess, pipe `stdin`, capture stdout/stderr, time it, kill at `problem.TimeLimitMs + 500ms` grace.
- Wrapper writes `/tmp/result.json` with `[{ id, verdict, wallMs, peakKb, stdout, stderr }, …]`.
- Wrapper prints `result.json` to stdout (capped, see below) for the worker to ingest.

### Output normalization (applied before comparison)
1. Convert CRLF → LF.
2. Strip trailing whitespace on each line.
3. Strip trailing blank lines.
4. Internal whitespace preserved (do not collapse).

### Float comparison
- Per-problem flag `Problem.FloatTolerance` (double, nullable). If null → exact string match after normalization.
- If non-null → tokenize both expected and actual on whitespace; lengths must match; each pair compared via `|a - b| ≤ tol ∨ |a - b| ≤ tol × |b|` (combined absolute + relative).
- Schema change: add `double? FloatTolerance` to `Problem`. Default null. Migration: `AddFloatToleranceToProblem`.

### Per-case verdict precedence
For a single case: `MemoryLimitExceeded` > `TimeLimitExceeded` > `RuntimeError` > `WrongAnswer` > `Accepted`.

### Submission-level scoring
- `Submission.Score = Σ TestCase.Points` over accepted cases.
- `Submission.Status` is the worst per-case verdict by the precedence above. (`Accepted` only if every case is Accepted.)

### Stdout/stderr caps
- Per-case: 4 MB stdout, 1 MB stderr. Truncate with marker `\n[truncated: N bytes elided]\n`.
- Wrapper-level output to worker: 16 MB JSON max. Worker truncates `stdout`/`stderr` fields in DB to 64 KB.

### Wall time and memory measurement
- Wall time: measured by the wrapper around each user-process invocation (clock_gettime CLOCK_MONOTONIC), **not** Docker daemon timestamps for the whole container — that includes interpreter warm-up and is unfair.
- Peak memory: `getrusage(RUSAGE_CHILDREN).ru_maxrss` after each subprocess exit. On Linux this is in KB and is per-process; for multi-process programs use cgroup `memory.peak` from inside the container if available.

---

## 7. Files To Create (Complete List)

Phases proceed top-to-bottom. Each phase has an explicit **Done when** acceptance criterion.

### Phase 0 — Infrastructure
**Done when:** Local `docker compose up` brings the full stack up, `/health` returns 200, HTTPS works against a self-signed cert, sandbox image fork-bomb test (§13b) passes.

| File | Description |
|---|---|
| `.env.template` | All env vars (see §9a) with safe defaults for local dev |
| `docker-compose.yml` | 8 services with explicit resource limits + log rotation |
| `nginx/nginx.conf` | TLS termination, `/hubs/` WebSocket upgrade, rate limit zones, basic-auth on `/seq/` |
| `nginx/htpasswd` | (Not committed; generated on server) — for Seq access |
| `Dockerfile.app` | Multi-stage: `node:22-alpine` (Tailwind build) → `sdk:10.0` (dotnet build) → `aspnet:10.0` (runtime) |
| `Dockerfile.worker` | Multi-stage `sdk:10.0` → `aspnet:10.0`, targets WebIde.Worker |
| `sandbox/gcc.Dockerfile` | `alpine:3.20` + gcc/g++ + `nobody` user + entrypoint wrapper |
| `sandbox/python.Dockerfile` | `python:3.12-alpine` + `nobody` user + entrypoint wrapper |
| `sandbox/node.Dockerfile` | `node:22-alpine` + `nobody` user + entrypoint wrapper |
| `sandbox/compile-and-run.sh` | gcc compile+iterate cases; exit 2 on compile error |
| `sandbox/run-python.sh` | Iterate cases; emit result.json |
| `sandbox/run-node.sh` | Iterate cases; emit result.json |
| `tailwind/input.css` + `tailwind.config.js` | Replace Tailwind Play CDN; CLI compiled to `wwwroot/css/site.tailwind.css` |
| `WebIde.Frontend/wwwroot/css/site.tailwind.css` | Build artifact (gitignored; produced by Dockerfile.app first stage) |

### Phase 1 — Authentication + HTTPS wiring
**Done when:** User can complete GitHub OAuth round-trip against a real HTTPS endpoint, a row is created in `users` with `GitHubId`, and the sidebar renders `User.Identity.Name` instead of `ROOT_USER`. Rate limiting active on `/auth/*`.

| File | Change |
|---|---|
| `WebIde.Model/User.cs` | Add `string? GitHubId`, `string? AvatarUrl` |
| `WebIde.Model/Problem.cs` | Add `double? FloatTolerance` (for §6a) |
| `WebIde.DAL/Migrations/` | New migration: `AddGitHubFieldsToUserAndFloatToleranceToProblem` |
| `WebIde.Frontend/WebIde.Frontend.csproj` | Add packages (see §9) |
| `WebIde.Frontend/Program.cs` | Full rewrite: Redis, DataProtection-on-Redis, Session, ForwardedHeaders, HTTPS redirect, HSTS, GitHub OAuth, cookie policy, SignalR, rate limiter, antiforgery, Health, Serilog |
| `WebIde.Frontend/Repositories/UserRepository.cs` | Add `GetByGitHubIdAsync`, `UpsertGitHubUserAsync` |
| `WebIde.Frontend/Controllers/AuthController.cs` | NEW: `/auth/github/login`, `/auth/github/callback`, `/auth/logout` |
| `WebIde.Frontend/Views/Shared/_Layout.cshtml` | Remove Tailwind Play CDN; replace hardcoded avatar with login/logout conditional |
| `WebIde.Frontend/Views/Shared/_Sidebar.cshtml` | Replace `ROOT_USER` with `User.Identity.Name`; show `AvatarUrl` when present |

### Phase 2 — Worker Service
**Done when:** A SubmissionJob pushed to Redis is consumed, a sandbox runs, results are written, a Redis publish fires, and Submission row reaches a terminal status. Worker survives SIGTERM mid-job (finishes or marks `InternalError`). Heartbeat key visible in Redis.

| File | Description |
|---|---|
| `WebIde.Worker/WebIde.Worker.csproj` | Worker SDK; refs Model + DAL; Docker.DotNet, Redis, Serilog |
| `WebIde.Worker/Program.cs` | `IDbContextFactory`, singleton `DockerClient` (unix socket), singleton `IConnectionMultiplexer`, service registrations, `IHostApplicationLifetime` graceful shutdown |
| `WebIde.Worker/Models/SubmissionJob.cs` | Queue message contract (record) |
| `WebIde.Worker/Models/SandboxResult.cs` | Per-case execution output model |
| `WebIde.Worker/Models/SubmissionResultEvent.cs` | Redis pub/sub payload |
| `WebIde.Worker/Models/WorkerOptions.cs` | Typed config from `Worker:*` env vars |
| `WebIde.Worker/Services/SandboxOrchestrator.cs` | `SemaphoreSlim` + Docker.DotNet lifecycle; tracks active submission IDs |
| `WebIde.Worker/Services/SubmissionEvaluator.cs` | Output normalization + float tolerance + per-case verdict precedence + score |
| `WebIde.Worker/Services/HeartbeatService.cs` | `IHostedService`; writes `worker:heartbeat = now` every 5s with TTL 30s |
| `WebIde.Worker/Services/StuckSubmissionReaper.cs` | `IHostedService`; on startup, scans `Running` rows older than max-time-limit + 1m, marks `InternalError` |
| `WebIde.Worker/Workers/SubmissionWorker.cs` | `BackgroundService`: BLPOP → execute → evaluate → persist → publish |
| `WebIde.slnx` | Add Worker project reference |

### Phase 3 — Submission Endpoint + SignalR + Editor
**Done when:** Logged-in user can submit Python and C++ from Monaco, sees status transitions live, sees final verdict with per-case breakdown.

| File | Change |
|---|---|
| `WebIde.Frontend/Hubs/ExecutionHub.cs` | NEW: `[Authorize]`, `SubscribeToSubmission(int id)` with ownership check |
| `WebIde.Frontend/Services/RedisSubscriptionService.cs` | NEW: `IHostedService`, Redis pattern-sub → SignalR bridge |
| `WebIde.Frontend/Services/WorkerHealthCheck.cs` | NEW: reads `worker:heartbeat`; unhealthy if stale > 30s |
| `WebIde.Frontend/Controllers/SubmissionController.cs` | Add `POST [Authorize]` endpoint with antiforgery + rate limit |
| `WebIde.Frontend/Views/Problem/Details.cshtml` | Monaco editor (CDN) + language picker + submit form (hidden textarea sync) + SignalR JS |

### Phase 4 — End-to-end smoke
**Done when:** All happy-path verification cases in §13 pass against a locally running stack via `docker compose up`. Adversarial verification §13b passes.

No new files; this phase is about running the lists, fixing bugs found, and capturing learnings.

### Phase 5 — CI/CD
**Done when:** PR to `main` runs CI gate, push to `main` builds + pushes images, SSH-deploys, runs migrations, verifies health, promotes `:latest` only on success.

| File | Description |
|---|---|
| `.github/workflows/ci.yml` | Build + test gate (PR to main; push to develop) |
| `.github/workflows/deploy.yml` | Build images → push `:sha-{SHA}` → SSH deploy → migrate → health check → promote `:latest` |
| `.github/workflows/sandbox.yml` | Triggered on `sandbox/**` changes: build sandbox images, push to ghcr.io with `:sha-{SHA}` |

---

## 8. GitHub Actions Deployment Pipeline

### CI Gate (`.github/workflows/ci.yml`)
Triggers: push to `develop`, PR to `main`

```
checkout → setup-dotnet 10.x → dotnet restore → dotnet build → dotnet test
```

Branch protection rule on `main`: this workflow must pass before merge (see §12a).

### Sandbox Pipeline (`.github/workflows/sandbox.yml`)
Triggers: push to `main` where `sandbox/**` changed.

```
1. Login to ghcr.io
2. For each lang in [gcc, python, node]:
   - docker buildx build sandbox/{lang}.Dockerfile
   - Push ghcr.io/fbeleta/web_ide/sandbox-{lang}:sha-{SHA}
3. Output digests as workflow artifact for deploy.yml consumption
```

Sandbox images are pinned by digest in deploy. They are **not** rebuilt on every deploy — only when their source changes.

### Deploy Pipeline (`.github/workflows/deploy.yml`)
Triggers: push to `main`

```
1. Login to ghcr.io
2. Capture rollback target:
     PREV_DIGEST=$(docker buildx imagetools inspect ghcr.io/.../app:latest --format '{{json .Manifest.Digest}}')
     # Saved for rollback even if :latest gets overwritten
3. Build + push app:sha-{SHA}, worker:sha-{SHA}
     (Do NOT push :latest yet.)
4. SSH into Hetzner (appleboy/ssh-action):
   a. cd /opt/webide
   b. IMAGE_TAG=sha-{SHA} docker compose pull webide-app webide-worker
   c. Run migrations (see §10a):
        docker run --rm --env-file .env --network webide_webide-net \
          ghcr.io/fbeleta/web_ide/app:sha-{SHA} \
          dotnet ef database update --no-build
   d. IMAGE_TAG=sha-{SHA} docker compose up -d webide-app webide-worker
   e. Poll GET https://localhost/health (30 attempts × 2s = 60s timeout)
        Also poll /health/ready which checks worker heartbeat.
   f. ✅ Pass:
        - From CI: re-tag and push ghcr.io/.../app:sha-{SHA} as :latest
        - docker image prune -f
   g. ❌ Fail:
        IMAGE_TAG=${PREV_DIGEST} docker compose up -d
        (Rolls back to the digest captured in step 2 — not :latest, which may have been overwritten.)
```

**Required GitHub Secrets:**

| Secret | Value |
|---|---|
| `HETZNER_HOST` | Server IP address |
| `HETZNER_USER` | SSH username (e.g. `deploy`) |
| `HETZNER_SSH_KEY` | Private SSH key (Ed25519 preferred) |

**Blue-green mechanism:** `IMAGE_TAG` env var controls which image Docker Compose uses. The `:latest` tag is promoted **only after** the health check confirms the new container is healthy. On failure, rollback uses the digest captured at step 2 (saved as `PREV_DIGEST`), guaranteeing we restore the previous known-good image even if step 3 had partially overwritten registry tags.

**Important:** The migration `docker run` must use `--network webide_webide-net` so it can reach the `postgres` service by hostname. Migrations follow the discipline in §10a — they must succeed against both the previous-version app and the new-version app to allow rollback after migration.

---

## 9. NuGet Packages to Add

### `WebIde.Frontend.csproj`
```xml
<PackageReference Include="AspNet.Security.OAuth.GitHub" Version="9.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.DataProtection.StackExchangeRedis" Version="9.*" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="9.*" />
<PackageReference Include="StackExchange.Redis" Version="2.*" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.*" />
<PackageReference Include="Serilog.AspNetCore" Version="9.*" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="9.*" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="9.*" />
```

Rate limiter and ForwardedHeaders are built into ASP.NET Core 7+ — no package needed.

### `WebIde.Worker.csproj`
```xml
<PackageReference Include="Docker.DotNet" Version="3.*" />
<PackageReference Include="StackExchange.Redis" Version="2.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*" PrivateAssets="all" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="9.*" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
```

---

## 9a. Secret Inventory

Every env var the stack consumes. `.env.template` ships with local-safe defaults for the **non-secret** rows; secrets ship empty and must be filled per environment.

| Name | Owner | Class | Example / default | Notes |
|---|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | app, worker | config | `Production` | `Development` locally |
| `PUBLIC_HOSTNAME` | app, nginx | config | `webide.example.com` | Used for OAuth redirect, HSTS |
| `LETSENCRYPT_EMAIL` | certbot | config | `ops@example.com` | For ACME registration |
| `POSTGRES_USER` | postgres, app, worker | config | `webide` | |
| `POSTGRES_DB` | postgres, app, worker | config | `webide` | |
| `POSTGRES_PASSWORD` | postgres, app, worker | **secret** | (generated) | 32+ char random |
| `REDIS_PASSWORD` | redis, app, worker | **secret** | (generated) | Set as `requirepass` |
| `GITHUB_CLIENT_ID` | app | config | from §12 | |
| `GITHUB_CLIENT_SECRET` | app | **secret** | from §12 | |
| `SEQ_FIRSTRUN_ADMINPASSWORD` | seq | **secret** | (one-time) | Seq UI is also behind nginx basic auth |
| `SEQ_BASIC_AUTH_HTPASSWD` | nginx | **secret** | generated `htpasswd -B` | Mounted at `/etc/nginx/htpasswd` |
| `WORKER__MAXCONCURRENTSANDBOXES` | worker | config | `2` | See §4 formula |
| `WORKER__SANDBOXMEMMB` | worker | config | `512` | Per-sandbox cap |
| `WORKER__SANDBOXCPUS` | worker | config | `0.9` | |
| `SANDBOX_GCC_DIGEST` | worker | config | `sha256:...` | Set by sandbox.yml workflow |
| `SANDBOX_PYTHON_DIGEST` | worker | config | `sha256:...` | |
| `SANDBOX_NODE_DIGEST` | worker | config | `sha256:...` | |
| `BACKUP_OFFSITE_TARGET` | postgres-backup | **secret** | `rsync://...` or S3 URL | See §11c |
| `BACKUP_OFFSITE_PASSWORD` | postgres-backup | **secret** | (provider-specific) | |

`.env` lives at `/opt/webide/.env`, mode 600, owned by `deploy`. `.env.template` lives in the repo with secrets blanked.

---

## 10. Key Implementation Notes

### `Program.cs` middleware order (critical)
```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { /* nginx subnet */ }, KnownProxies = { /* nginx IP */ }
});
app.UseHsts();              // Production only
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();    // MUST be before Authorization
app.UseAuthorization();
app.UseSession();
app.MapHub<ExecutionHub>("/hubs/execution").RequireRateLimiting("hub");
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllerRoute(/* ... */);
```

### DataProtection on Redis (critical)
Without this, every container restart logs every user out and invalidates every antiforgery token (keys are regenerated in the container's ephemeral filesystem).

```csharp
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("WebIde");
```

### Cookie policy (critical)
```csharp
services.ConfigureApplicationCookie(o => {
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;   // Lax: OAuth callback needs it
    o.ExpireTimeSpan = TimeSpan.FromDays(14);
    o.SlidingExpiration = true;
    o.LoginPath = "/auth/github/login";
});
```

### Rate limiter
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
Apply via `[EnableRateLimiting("submission")]` on `SubmissionController.Submit`, `[EnableRateLimiting("auth")]` on `AuthController`.

### Antiforgery for JSON POST (important)
Default `[ValidateAntiForgeryToken]` only validates the form-encoded token. For fetch + JSON, do:
```csharp
[HttpPost]
[Authorize]
[EnableRateLimiting("submission")]
public async Task<IActionResult> Submit([FromBody] SubmitDto dto, [FromServices] IAntiforgery antiforgery) {
    await antiforgery.ValidateRequestAsync(HttpContext);  // reads token from header
    /* ... */
}
```
Razor view emits the token:
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

### DI lifetime cheat sheet (critical)
| Service | Lifetime | Reason |
|---|---|---|
| `IConnectionMultiplexer` | Singleton | StackExchange.Redis is designed as a single shared connection |
| `DockerClient` | Singleton | Docker.DotNet thread-safe; expensive to recreate |
| `WebIdeDbContext` | Scoped (via `IDbContextFactory`) | Hosted services are singletons; injecting scoped directly throws at startup |
| `SandboxOrchestrator` | Singleton | Holds `SemaphoreSlim` slot state |
| `SubmissionEvaluator` | Scoped | Stateless; scoped is conventional |
| `HeartbeatService`, `StuckSubmissionReaper`, `SubmissionWorker`, `RedisSubscriptionService` | Singleton (hosted) | They are `IHostedService` |

### Memory stats collection (critical)
Set `AutoRemove = false`. Stream stats via `GetContainerStatsAsync` until the container exits; take the max `MemoryStats.Usage` (or `MaxUsage` if exposed) observed. On cgroupv2, prefer reading `memory.peak` from inside the container via the wrapper (more accurate than Docker's polled stats). After the container exits, call `RemoveContainerAsync`. The "inspect before remove" approach in the prior revision was wrong — `InspectContainerAsync` does not return memory stats.

### Wall-time measurement (critical)
Measured by the wrapper script per user-process invocation (`clock_gettime`), **not** wall-clock around `docker run` (which includes 100-500ms of container creation). See §6a.

### Redis SignalR backplane (load-bearing, not optional)
This is **not** "in case you scale later." The worker is a separate process. The bridge from Redis pub/sub to SignalR groups is the only path from worker → browser. Register with `.AddSignalR().AddStackExchangeRedis(connectionString)`.

### Monaco Editor (CDN)
The project has no JS build pipeline. Use AMD loader from CDN:
```html
<script src="https://cdn.jsdelivr.net/npm/monaco-editor@0.47.0/min/vs/loader.js"></script>
```
Language picker drives Monaco's model language:
```js
const langMap = { cpp: 'cpp', python: 'python', javascript: 'javascript' };
monaco.editor.setModelLanguage(editor.getModel(), langMap[selectedLang]);
```
On submit, sync Monaco value to a hidden `<textarea name="sourceCode">` and include the language. Antiforgery token in header per above.

### Tailwind (not via CDN)
Tailwind Play CDN is documented by Tailwind as **not for production**. The Dockerfile.app first stage runs `npx tailwindcss -i tailwind/input.css -o WebIde.Frontend/wwwroot/css/site.tailwind.css --minify`. The `<script src="https://cdn.tailwindcss.com">` tag in `_Layout.cshtml` is replaced by `<link rel="stylesheet" href="~/css/site.tailwind.css" asp-append-version="true">`.

### Health checks
- `/health` (liveness): always 200 if the process is up. No DB check (a flapping DB shouldn't take down the load balancer).
- `/health/ready` (readiness): checks Postgres connectivity, Redis connectivity, worker heartbeat (must be < 30s old). Returns 503 if any fail. Used by deploy pipeline.

### nginx full config sketch
```nginx
limit_req_zone $binary_remote_addr zone=auth:10m rate=30r/m;
limit_req_zone $binary_remote_addr zone=submit:10m rate=10r/m;
server {
    listen 443 ssl http2;
    server_name webide.example.com;
    ssl_certificate     /etc/letsencrypt/live/.../fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/.../privkey.pem;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    gzip on; gzip_types text/css application/javascript application/json;
    client_max_body_size 1m;

    location /auth/      { limit_req zone=auth burst=10; proxy_pass http://webide-app:8080; include /etc/nginx/proxy_pass.conf; }
    location /submissions { limit_req zone=submit burst=5; proxy_pass http://webide-app:8080; include /etc/nginx/proxy_pass.conf; }
    location /hubs/ {
        proxy_pass http://webide-app:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 86400;
        include /etc/nginx/proxy_pass.conf;
    }
    location /seq/ { auth_basic "Seq"; auth_basic_user_file /etc/nginx/htpasswd; proxy_pass http://seq:80; include /etc/nginx/proxy_pass.conf; }
    location /    { proxy_pass http://webide-app:8080; include /etc/nginx/proxy_pass.conf; }
}
server { listen 80; server_name webide.example.com; return 301 https://$host$request_uri; }
```
`/etc/nginx/proxy_pass.conf`:
```nginx
proxy_set_header Host $host;
proxy_set_header X-Real-IP $remote_addr;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

---

## 10a. Migration Discipline

Migrations run **before** the new app container starts (§8 step c). This creates an asymmetric window: if migration succeeds but the app health check fails, rollback brings back code that doesn't know the new schema. To make this safe:

### Rules for every migration
1. **Additive only in a single deploy.** Add nullable columns; never rename or drop a column in the same deploy as the code that uses it. Use the two-deploy pattern:
   - Deploy A: add new column (nullable), have code write to both old and new.
   - Deploy B (later): switch reads to new column, stop writing to old.
   - Deploy C: drop old column.
2. **No `[Required]` on new properties** without a server default — backfill in a data migration first.
3. **Forward-only.** No down migrations. Rollback for a failed deploy that already migrated = restore the most recent Postgres backup (§11c) and redeploy the previous image. This is documented in the runbook, not automated, because each case needs human judgment.

### What this trades off
Compared to fully online schema changes (concurrent index builds, blue-green schema, etc.) we accept a possible brief outage if a deploy ships an incompatible migration. This is acceptable given the user scale and the explicit pattern above.

---

## 11. Server Setup (One-Time, Manual)

Run on the Hetzner server before first deploy:

```bash
# Install Docker
curl -fsSL https://get.docker.com | sh

# Create deploy user
useradd -m -s /bin/bash deploy
usermod -aG docker deploy

# Create app directory
mkdir -p /opt/webide
chown deploy:deploy /opt/webide

# Create source temp dir (worker bind mount) — mode 700, owned by deploy
mkdir -p /tmp/webide-src
chown deploy:deploy /tmp/webide-src
chmod 700 /tmp/webide-src
# Worker container must run as the host's deploy UID to read/write this dir.
# Set `user: "${DEPLOY_UID}:${DEPLOY_GID}"` in compose for webide-worker.

# Copy .env from template (fill in secrets — see §9a)
cp .env.template /opt/webide/.env
chmod 600 /opt/webide/.env

# Generate Seq basic-auth credentials for nginx
htpasswd -B -c /opt/webide/nginx/htpasswd ops    # prompts for password

# Add SSH public key for deploy user (GitHub Actions will use this)
mkdir -p /home/deploy/.ssh
echo "YOUR_PUBLIC_KEY" >> /home/deploy/.ssh/authorized_keys
chmod 700 /home/deploy/.ssh
chmod 600 /home/deploy/.ssh/authorized_keys
```

Sandbox images are NOT pulled manually here. They are built by `.github/workflows/sandbox.yml` and pinned by digest via the `SANDBOX_*_DIGEST` env vars (§9a). The worker pulls them on first job if missing.

---

## 11b. HTTPS / TLS (Day 1)

HTTPS is **required from day 1**. GitHub OAuth's correlation cookie defaults to `Secure=true` after the redirect; cookie auth requires `Secure=Always` in production. Without HTTPS, auth literally does not function.

### Approach: Certbot in a sidecar container
```yaml
# excerpt of docker-compose.yml
certbot:
  image: certbot/certbot
  volumes:
    - certbot-etc:/etc/letsencrypt
    - certbot-var:/var/lib/letsencrypt
    - ./nginx/acme-challenge:/var/www/acme:rw
  entrypoint: ["/bin/sh", "-c"]
  command: |
    "trap exit TERM; while :; do
       certbot renew --webroot -w /var/www/acme --quiet;
       sleep 12h & wait $${!};
     done;"

nginx:
  # mount certbot-etc:/etc/letsencrypt:ro
  # serve /var/www/acme via location /.well-known/acme-challenge/
```

### Initial bootstrap (one-time, on server)
```bash
docker compose run --rm certbot certonly --webroot \
  -w /var/www/acme \
  -d webide.example.com \
  -m "$LETSENCRYPT_EMAIL" --agree-tos --non-interactive
docker compose restart nginx
```

### App-side requirements
- `app.UseForwardedHeaders(...)` **before** any auth middleware so `Request.Scheme = https`.
- `app.UseHsts()` in production.
- Cookie policy with `SecurePolicy = CookieSecurePolicy.Always` (§10).
- OAuth callback registered as `https://YOUR_DOMAIN/auth/github/callback`.

---

## 11c. Postgres Backup

Single-VPS production without backup = guaranteed data loss event. Required day 1.

### In-compose backup service
Use `prodrigestivill/postgres-backup-local` (well-maintained, configurable cron, retention):
```yaml
postgres-backup:
  image: prodrigestivill/postgres-backup-local
  environment:
    POSTGRES_HOST: postgres
    POSTGRES_DB: ${POSTGRES_DB}
    POSTGRES_USER: ${POSTGRES_USER}
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    SCHEDULE: "@daily"
    BACKUP_KEEP_DAYS: 7
    BACKUP_KEEP_WEEKS: 4
    BACKUP_KEEP_MONTHS: 6
  volumes:
    - postgres-backups:/backups
  depends_on: [postgres]
```

### Offsite (REQUIRED — local backup is not a backup)
Add a small sidecar that rsyncs `postgres-backups/` to a Hetzner Storage Box or S3-compatible bucket nightly:
```yaml
backup-offsite:
  image: instrumentisto/rsync-ssh
  volumes:
    - postgres-backups:/backups:ro
    - ./backup-ssh-key:/root/.ssh/id_ed25519:ro
  environment:
    SCHEDULE: "0 3 * * *"
    REMOTE: "${BACKUP_OFFSITE_TARGET}"
  # entrypoint runs rsync on cron schedule
```
(Or wire a Restic container against a B2/S3 endpoint.)

### Restore drill
A backup that has never been restored is not a backup. **Quarterly**: pull the latest offsite dump to a scratch VPS, restore into a fresh Postgres, smoke-test. Capture the procedure in a runbook.

---

## 12. GitHub OAuth App Setup

1. Go to GitHub → Settings → Developer settings → OAuth Apps → New OAuth App
2. **Application name:** WebIde
3. **Homepage URL:** `https://YOUR_DOMAIN`
4. **Authorization callback URL:** `https://YOUR_DOMAIN/auth/github/callback`
5. Copy Client ID and Client Secret → add to `/opt/webide/.env` (`GITHUB_CLIENT_ID`, `GITHUB_CLIENT_SECRET`)

---

## 12a. GitHub Repo Setup (One-Time)

Easy to forget; without these, CI is theatre.

- Branch protection on `main`:
  - Require status check: `ci.yml / build-and-test`
  - Require pull request review before merge
  - Disallow force push
  - Disallow deletion
- Actions permissions:
  - Workflows: read & write
  - GITHUB_TOKEN can push to ghcr.io (default `write` for own packages)
- Secrets: `HETZNER_HOST`, `HETZNER_USER`, `HETZNER_SSH_KEY` (see §8)
- Variables (optional, non-secret): `GHCR_NAMESPACE = fbeleta/web_ide`

---

## 13. Verification Checklist

### Infrastructure smoke test
- [ ] `docker compose up -d` brings the full stack up
- [ ] `curl https://localhost/health` returns 200 (with `-k` against a local self-signed cert)
- [ ] `curl https://localhost/health/ready` returns 200 (DB + Redis + worker heartbeat all green)
- [ ] Seq UI at `https://localhost/seq/` prompts for basic auth, then renders
- [ ] Stopping postgres → `/health/ready` 503; `/health` still 200
- [ ] Stopping worker → `/health/ready` 503 within 30s

### Worker integration test
```bash
redis-cli -a "$REDIS_PASSWORD" RPUSH submissions:queue \
  '{"SubmissionId":1,"ProblemId":1,"Language":"python","SourceCode":"print(input())","TimeLimitMs":2000,"MemoryLimitKb":524288}'
# Check Seq for worker logs
# Check DB: SELECT status, wall_time_ms FROM submissions WHERE id = 1;
```

### Full flow test
- [ ] Login via GitHub OAuth → user record created in DB with `GitHubId`
- [ ] Submit correct Python solution → ACCEPTED, wall time displayed, per-case verdicts shown
- [ ] Submit infinite loop → TIME_LIMIT_EXCEEDED after ~`TimeLimitMs + 500ms`
- [ ] Submit wrong output → WRONG_ANSWER
- [ ] Submit C++ with syntax error → COMPILE_ERROR with stderr shown
- [ ] Submit Python with SyntaxError → RUNTIME_ERROR (intentional per §5)
- [ ] Submit while 2 jobs are running → third queues, executes when slot frees
- [ ] Submit > 5 times in 1 minute → 429 from rate limiter

### CI/CD test
- [ ] Open PR to main with broken build → CI fails, merge blocked
- [ ] Push to main → images built, pushed to ghcr.io with `:sha-{SHA}`, deployed
- [ ] After successful deploy → `:latest` updated; before, it points to previous good
- [ ] Simulate unhealthy app (set health check to fail) → rollback to previous digest triggers

---

## 13a. Local Dev Quick-Start

For a fresh clone:

```bash
cp .env.template .env                # local-safe defaults; no real secrets needed
docker compose up -d postgres redis seq
cd WebIde.Frontend
dotnet ef database update --project ../WebIde.DAL
dotnet run                            # http://localhost:5xxx
```

Worker locally:
```bash
cd WebIde.Worker
dotnet run
```

Sandbox images locally:
```bash
docker build -t webide-sandbox-python -f sandbox/python.Dockerfile sandbox/
docker build -t webide-sandbox-gcc    -f sandbox/gcc.Dockerfile    sandbox/
docker build -t webide-sandbox-node   -f sandbox/node.Dockerfile   sandbox/
```
Set `SANDBOX_*_DIGEST` to local tags (`webide-sandbox-python` etc.) for local dev.

OAuth in local dev: the GitHub OAuth app needs a separate "WebIde-Dev" registration with callback `http://localhost:5xxx/auth/github/callback` and a relaxed cookie policy (Development env auto-disables `SecurePolicy.Always`).

---

## 13b. Adversarial Verification

Run these against the deployed stack on a regular cadence (after any sandbox image change, before each release).

```bash
# Fork bomb — should die without disturbing the host
echo 'import os
while True: os.fork()' > /tmp/test-src/solution.py
docker run [...same flags as §6...] -v /tmp/test-src:/code:ro webide-sandbox-python ...
# Expect: container killed quickly; host `ps` count unaffected.

# Memory bomb
echo 'a = bytearray(2*1024*1024*1024)' > /tmp/test-src/solution.py
# Expect: MemoryLimitExceeded, container killed.

# Filesystem escape attempt
echo 'open("/etc/passwd", "w").write("hi")' > /tmp/test-src/solution.py
# Expect: PermissionError (read-only fs) — NOT a write.

# Network egress
echo 'import urllib.request; print(urllib.request.urlopen("https://example.com").read())' > /tmp/test-src/solution.py
# Expect: connection error (--network none).

# Identity check
echo 'import os; print(os.getuid())' > /tmp/test-src/solution.py
# Expect: 65534 (nobody), NOT 0.

# Long stdout
echo 'print("x" * 100_000_000)' > /tmp/test-src/solution.py
# Expect: truncated to 4 MB with marker, NOT host OOM.

# Worker SIGTERM mid-job
# Start a long submission, then `docker compose restart webide-worker`
# Expect: submission either completes or transitions to InternalError; not stuck Running forever.

# OAuth state missing
# Hit /auth/github/callback?code=foo&state=bar with no correlation cookie
# Expect: 400 with user-friendly message, NOT 500.
```

---

## 14. Risks & Mitigations

| Risk | Severity | Mitigation |
|---|---|---|
| Sandbox container escape via Docker socket | Medium | Worker uses hard-coded image names by language enum; sandbox containers have `--network none` + `--cap-drop ALL` + `--user nobody` — cannot reach the socket. Worker code is the trust boundary; treat changes to it as security-sensitive. |
| Fork bomb / process exhaustion | Low | `--pids-limit 64` (not `--ulimit nproc`) — cgroup-enforced, per-container. |
| `/tmp/webide-src` stale dirs on crash | Low | `finally` block in orchestrator always deletes. Startup cleanup of dirs older than `maxTimeLimit + 1min`, skipping any submission ID currently in the in-memory active set. |
| Memory stats unavailable after removal | Low | `AutoRemove = false`; stream `GetContainerStatsAsync` until exit; prefer wrapper-side `memory.peak`. |
| EF migration fails mid-deploy | Low | Migration runs before `compose up`; non-zero exit halts deploy, old containers keep serving. |
| EF migration succeeds, app fails to start | Medium | Migration discipline (§10a): additive only; rollback procedure documented (restore backup + redeploy previous image). |
| Worker dies mid-job → stuck `Running` row | Low | `StuckSubmissionReaper` on startup transitions rows older than `maxTimeLimit + 1min` to `InternalError`. SIGTERM handler drains in flight or marks `InternalError`. |
| Worker hung → queue silently grows | Low | Worker writes `worker:heartbeat` to Redis every 5s (TTL 30s); `/health/ready` fails if stale. |
| SignalR falls back to long-poll | Low | `proxy_read_timeout 86400` on nginx; long-poll still works. |
| Both sandbox slots occupied | By design | Redis queue serializes work; jobs wait. |
| DooD requires `/var/run/docker.sock` | Low | Documented in §11; standard on any Docker host. |
| Postgres data loss on disk failure | Medium | Daily local backup + nightly offsite rsync (§11c); quarterly restore drill. |
| Disk filling from logs | Low | Docker `json-file` driver with `max-size: 10m, max-file: 3`; Seq retention 14 days. |
| HTTPS cert expiry | Low | Certbot sidecar renews every 12h; nginx reloads on cert change. Monitor cert age via `/health/ready` extension (future). |
| Abusive submission spam | Medium | Rate limiter on `/submissions` (5/min/user) and `/auth/*` (10/min/IP); GitHub OAuth raises the cost of throwaway accounts. |

---

## 15. Future Work (Post-v1)

| Feature | Notes |
|---|---|
| Admin UI | Problem + test case + ProblemSet + Organization + Tag CRUD (currently DB-seeded only) |
| Per-test-case execution as separate containers | Tighter isolation; rejected for v1 due to ~200 ms overhead per case |
| Redis Streams queue | Upgrade from BLPOP list for consumer groups + replay |
| Horizontal worker scaling | Remove in-process semaphore; use Redis for distributed slot tracking |
| `WebIde.Tests` project | Currently no test project; CI `dotnet test` passes vacuously. Should backfill before scaling team. |
| Metrics / alerting | Prometheus + Grafana or hosted (Uptime Kuma, Better Stack). Currently logs-only via Seq. |
| Multi-region / HA | Single VPS = SPOF. Acceptable for v1; revisit when usage warrants. |
| Staging environment | Currently develop → main → prod. No environment to test deploy mechanics. |
| OAuth providers beyond GitHub | Google, GitLab as student access broadens |
| Editorial / hint system | Per-problem rich content beyond the markdown description |

---

## Appendix A — What Changed from Initial Plan

This revision folded a gap-analysis pass focused on production-grade readiness. Highlights:

- HTTPS moved from "future work" to Phase 0 / §11b — required for OAuth to function.
- DataProtection keys persisted to Redis (§10) — otherwise every deploy logs every user out.
- Redis `requirepass` added (§9a, §11) — was unprotected.
- Postgres/Redis/Seq port publication removed from compose; Seq behind nginx basic auth (§3, §11).
- Sandbox `--ulimit nproc` replaced with `--pids-limit`; `--user nobody:nogroup` added; image digest pinning (§6).
- Judging contract specified explicitly (§6a): output normalization, float tolerance, multi-case model, scoring, stdout caps.
- Deploy pipeline rewritten so `:latest` is promoted only after health-check pass; rollback uses captured digest, not `:latest` (§8).
- Migration discipline section added (§10a): additive-only rule + manual rollback procedure with backup restore.
- Postgres backup with offsite target added (§11c) + restore drill.
- Worker SIGTERM handling, heartbeat, stuck-submission reaper specified (§7 Phase 2, §10, §14).
- Rate limiter moved from future work to Phase 1 (§10).
- Antiforgery wiring for JSON POST specified (§10).
- nginx config expanded with `X-Forwarded-Proto`, rate-limit zones, gzip, Seq auth (§10).
- Adversarial verification suite added (§13b).
- Local dev quick-start added (§13a).
- Secret inventory enumerated (§9a).
- Tailwind moved from Play CDN to build-time compile (§7 Phase 0, §10).
- Phase numbering fixed; per-phase acceptance criteria added (§7).
