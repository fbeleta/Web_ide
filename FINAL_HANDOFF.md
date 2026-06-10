# WebIde — Final Handoff

Live at **https://zetesis.cc** (HTTP 200 as of 2026-06-11).

---

## What's Working Now

- HTTPS with Let's Encrypt (auto-renews via certbot container)
- ASP.NET app + Worker service running in Docker
- PostgreSQL with all migrations applied
- GitHub OAuth login (configured via `.env`)
- Home page, Problems, Leaderboard, Orgs, Tags pages load

---

## What Needs Fixing / Completing

### 1. Remove Seed Data (fake users + problems)

**File:** `WebIde.DAL/WebIdeDbContext.cs`

Remove all `HasData(...)` calls in `OnModelCreating` — the fake users (`ana_k`, `mario_b`, `prof_hr`, `admin`), problems, test cases, submissions, orgs, etc. Replace with real data entered through the UI.

After removing, create a new migration and run it. The existing seed rows in the DB need to be manually deleted or the DB wiped and remigrated.

**Server command to wipe and remigrate:**
```bash
docker compose down -v
docker compose up -d
docker run --rm --network webide_webide-net -v /opt/webide:/src -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  bash -c "dotnet tool restore && dotnet ef database update \
    --project WebIde.DAL/WebIde.DAL.csproj \
    --startup-project WebIde.Frontend/WebIde.Frontend.csproj"
docker compose restart webide-app webide-worker
```

---

### 2. Auth Gating — Lock Pages Behind Login

**File:** `WebIde.Frontend/Controllers/*.cs`

Add `[Authorize]` to controllers/actions that should require login. Suggested minimum:
- `OrganizationController` — all actions
- `SubmissionController` — already has `[Authorize]` on POST, add to GET views too
- `UserController` — profile/edit actions

The login path is `/auth/github/login` (GitHub OAuth) or `/Identity/Account/Login` (username/password).

---

### 3. Real-Time Results (Redis → SignalR Bridge)

**Status:** Worker publishes results to Redis channel `execution:{submissionId}` but nothing forwards them to browser clients.

**File to create:** `WebIde.Frontend/Services/RedisSubscriptionService.cs`

This should be a `BackgroundService` that:
1. Subscribes to Redis channel `execution:*`
2. On message received, calls `IHubContext<ExecutionHub>.Clients.Group(submissionId).SendAsync("result", payload)`

The `ExecutionHub` already exists at `WebIde.Frontend/Hubs/ExecutionHub.cs`. The hub groups are keyed by submission ID — clients join on page load.

---

### 4. Build Sandbox Images and Set Digests

The Worker needs sandbox image digests in `.env` to run submissions. Currently `SANDBOX_GCC_DIGEST`, `SANDBOX_PYTHON_DIGEST`, `SANDBOX_NODE_DIGEST` are blank.

**Option A — CI (recommended):** The `sandbox.yml` GitHub Actions workflow builds and pushes sandbox images and sets the digests. Trigger it by pushing to `main`.

**Option B — Manual:** Build on the server:
```bash
cd /opt/webide
docker build -f sandbox/gcc.Dockerfile -t webide-sandbox-gcc sandbox/
docker build -f sandbox/python.Dockerfile -t webide-sandbox-python sandbox/
docker build -f sandbox/node.Dockerfile -t webide-sandbox-node sandbox/
```
Then set in `.env`:
```
SANDBOX_GCC_DIGEST=webide-sandbox-gcc
SANDBOX_PYTHON_DIGEST=webide-sandbox-python
SANDBOX_NODE_DIGEST=webide-sandbox-node
```
And restart the worker: `docker compose restart webide-worker`

---

### 5. Code Quality Fixes (commit to server-fixes, then merge)

**a) Remove hardcoded prod password from committed files**

`WebIde.Frontend/appsettings.json` — revert `WebIdeDb` connection string to:
```
Host=localhost;Port=5432;Database=webide;Username=webide;Password=webide_dev
```

`WebIde.DAL/WebIdeDbContextFactory.cs` — revert to:
```csharp
"Host=localhost;Port=5432;Database=webide;Username=postgres;Password=postgres"
```

The real password is injected at runtime via docker-compose env vars — it must NOT be committed.

**b) Conditional GitHub/Google OAuth registration**

`WebIde.Frontend/Program.cs:69` — `DefaultChallengeScheme` is hardwired to GitHub. If `GITHUB_CLIENT_ID` is ever empty the app crashes on every request. Guard it:
```csharp
var githubClientId = config["GitHub:ClientId"] ?? "";
options.DefaultChallengeScheme = string.IsNullOrEmpty(githubClientId)
    ? CookieAuthenticationDefaults.AuthenticationScheme
    : GitHubAuthenticationDefaults.AuthenticationScheme;
```
Wrap `.AddGitHub(...)` and `.AddGoogle(...)` blocks with `if (!string.IsNullOrEmpty(...))` guards.

---

### 6. Add Missing EF Migration for ExecutionResults Columns

The columns `PeakMemoryKb`, `SubmissionId`, `TestCaseId`, `Verdict`, `WallTimeMs` were added to the DB manually via `psql`. A proper migration file is missing, so `dotnet ef database update` thinks the DB is up to date but the migration history is incomplete.

Create `WebIde.DAL/Migrations/20260611000000_AddExecutionResultColumns.cs`:
```csharp
public partial class AddExecutionResultColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("PeakMemoryKb", "ExecutionResults", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("SubmissionId",  "ExecutionResults", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("TestCaseId",    "ExecutionResults", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("Verdict",       "ExecutionResults", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("WallTimeMs",    "ExecutionResults", nullable: false, defaultValue: 0);
        migrationBuilder.CreateIndex("IX_ExecutionResults_TestCaseId", "ExecutionResults", "TestCaseId");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_ExecutionResults_TestCaseId", "ExecutionResults");
        migrationBuilder.DropColumn("PeakMemoryKb", "ExecutionResults");
        migrationBuilder.DropColumn("SubmissionId",  "ExecutionResults");
        migrationBuilder.DropColumn("TestCaseId",    "ExecutionResults");
        migrationBuilder.DropColumn("Verdict",       "ExecutionResults");
        migrationBuilder.DropColumn("WallTimeMs",    "ExecutionResults");
    }
}
```
Insert a row into `__EFMigrationsHistory` on the server so EF doesn't try to re-apply it:
```bash
docker compose exec postgres psql -U webide -d webide -c \
  "INSERT INTO \"__EFMigrationsHistory\" VALUES ('20260611000000_AddExecutionResultColumns', '9.0.16');"
```

---

### 7. Organizations / Groups — Add Users to Orgs

The `OrganizationController` and `OrganizationRepository` already exist. Users can be added to orgs via the existing many-to-many `OrganizationUser` join table.

What's missing: a UI action for admins to add/remove members. Add an `AddMember` POST endpoint to `OrganizationController` and a corresponding form in the org detail view.

---

## Suggested Order of Work

1. Remove seed data + wipe DB (§1)
2. Build sandbox images + set digests (§4)  
3. Fix hardcoded passwords + conditional OAuth (§5)
4. Add the missing migration file + history entry (§6)
5. Implement RedisSubscriptionService (§3)
6. Add auth gating to controllers (§2)
7. Add org member management UI (§7)

---

## Server Quick Reference

| Task | Command |
|------|---------|
| View logs | `docker compose logs webide-app --tail 50` |
| Restart app | `docker compose restart webide-app` |
| Run migrations | `docker run --rm --network webide_webide-net -v /opt/webide:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 bash -c "dotnet tool restore && dotnet ef database update --project WebIde.DAL/WebIde.DAL.csproj --startup-project WebIde.Frontend/WebIde.Frontend.csproj"` |
| psql shell | `docker compose exec postgres psql -U webide -d webide` |
| Pull latest code | `git -C /opt/webide pull origin server-fixes` |
| Rebuild + restart | `docker compose up -d --force-recreate webide-app webide-worker` |
