# Server Status — pick up here tomorrow

> **UPDATE (2026-06-15): The migration chain has been fixed.** The migrations were squashed to a
> single clean `InitialCreate` baseline (includes `FloatTolerance`, the `ExecutionResults`
> columns, and Identity tables; no seed data). The manual `psql` column fixes and
> `__EFMigrationsHistory` inserts described below are **obsolete** — do not run them. For a clean
> server, just run `bash scripts/server-init.sh` (wipes the DB and applies the single migration).
> The CI deploy now ships a self-contained EF migration bundle instead of running `dotnet ef`
> inside the runtime image. The text below is kept for historical context only.

**Date:** 2026-06-11  
**Branch:** server-fixes (pushed, up to date)  
**Site:** https://zetesis.cc

---

## What was done today

All 7 FINAL_HANDOFF items implemented and pushed to `server-fixes`:

- Hardcoded prod password removed from `appsettings.json` and `WebIdeDbContextFactory.cs`
- `Program.cs` — GitHub/Google OAuth wrapped in null guards; DefaultChallengeScheme no longer hardwired
- Seed data removed from `WebIdeDbContext.cs`
- `20260611000000_AddExecutionResultColumns.cs` migration created
- `RedisSubscriptionService.cs` created (subscribes to `execution:*`, forwards to ExecutionHub)
- `[Authorize]` added to OrganizationController (class), SubmissionController (most actions), UserController (edit/delete)
- `OrganizationController.AddMember` + form in org Details view
- `scripts/server-init.sh` created

---

## Current server state (broken)

### What happened

`server-init.sh` ran and wiped the DB. The migration step had `--no-build` which failed
because there are no compiled binaries in the source directory. `set -euo pipefail` caught
the error and the script exited before `docker compose up -d`. The app was started manually
against a partially-migrated or empty DB.

### Symptom
- Orgs page: **works**
- Submissions page: **500**
- Other pages: unknown (likely working after a psql column fix was applied)

The root cause of the original "no pages but orgs works": `ExecutionResults` table was
missing `PeakMemoryKb`, `SubmissionId`, `TestCaseId`, `Verdict`, `WallTimeMs` columns.
A psql fix was applied to add those columns manually. Submissions is still 500 — the
**actual exception is unknown** (logs were not checked before going to sleep).

---

## First thing to do tomorrow

### 1. Check logs
```bash
docker compose -f /opt/webide/docker-compose.yml logs webide-app --tail 50
```

### 2. Run migrations properly (fixes the root cause)
```bash
git -C /opt/webide pull origin server-fixes

source /opt/webide/.env

docker run --rm \
  --network webide_webide-net \
  -v /opt/webide:/src -w /src \
  -e "ConnectionStrings__WebIdeDb=Host=postgres;Port=5432;Database=webide;Username=webide;Password=${POSTGRES_PASSWORD}" \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  bash -c "dotnet tool restore && dotnet ef database update \
    --project WebIde.DAL/WebIde.DAL.csproj \
    --startup-project WebIde.Frontend/WebIde.Frontend.csproj"

docker compose -f /opt/webide/docker-compose.yml restart webide-app webide-worker
```

### 3. If submissions is still 500 after that
Paste the app logs into the chat. The exception will tell us exactly what's wrong.

---

## Possible remaining causes of submissions 500

1. **DB has no seed data** — migrations never ran, tables are empty or missing.
   Fix: the migration command above.

2. **Null reference in the view** — `sub.User` or `sub.Problem` is null because a
   Submission exists with a UserId/ProblemId that has no matching row.
   Fix: check if there are any submissions with dangling FKs:
   ```sql
   SELECT s.* FROM "Submissions" s
   LEFT JOIN "DomainUsers" u ON s."UserId" = u."Id"
   WHERE u."Id" IS NULL AND s."DeletedAt" IS NULL;
   ```

3. **Auth challenge loop** — unauthenticated user hits `/submissions` (now [Authorize]),
   gets redirected to `/auth/github/login`, which throws if GitHub OAuth isn't registered.
   Would show as 500 in browser. Check if you're logged in when hitting /submissions.

---

## What still needs doing (non-code, server-side)

- [ ] Sandbox images need to be built and digests set in `.env`
  Either run `bash /opt/webide/scripts/server-init.sh` (full wipe + remigrate) or manually:
  ```bash
  cd /opt/webide
  docker build -f sandbox/gcc.Dockerfile    -t webide-sandbox-gcc    sandbox/
  docker build -f sandbox/python.Dockerfile -t webide-sandbox-python sandbox/
  docker build -f sandbox/node.Dockerfile   -t webide-sandbox-node   sandbox/
  # then edit .env: set SANDBOX_*_DIGEST=webide-sandbox-*
  docker compose restart webide-worker
  ```

- [ ] Once everything is stable, consider merging server-fixes → main and running
  `server-init.sh` from scratch for a clean state.
