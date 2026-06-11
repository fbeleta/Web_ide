# Local Dev Setup

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for Redis)
- PostgreSQL running locally on port 5432

## First-time setup after cloning

### 1. Start Redis

```bash
docker compose -f docker-compose.dev.yml up -d
```

This starts Redis on `localhost:6379` (no password).  
If you don't have a local PostgreSQL, uncomment the `postgres` service in `docker-compose.dev.yml` first.

### 2. Create the database and user

```bash
psql postgres -c "CREATE ROLE webide WITH LOGIN PASSWORD 'webide_dev';"
psql postgres -c "CREATE DATABASE webide OWNER webide;"
```

If the database already exists but was owned by a different user, drop and recreate it:

```bash
psql postgres -c "DROP DATABASE webide;"
psql postgres -c "CREATE DATABASE webide OWNER webide;"
```

### 3. Restore dotnet tools and run migrations

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project WebIde.DAL/WebIde.DAL.csproj \
  --startup-project WebIde.Frontend/WebIde.Frontend.csproj
```

### 4. Add FloatTolerance column (one-time fix)

This column was lost during a migration cleanup and must be added manually:

```bash
psql -U webide webide -c 'ALTER TABLE "Problems" ADD COLUMN IF NOT EXISTS "FloatTolerance" double precision;'
```

### 5. Run the app

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project WebIde.Frontend/WebIde.Frontend.csproj
```

The app will be available at `http://localhost:5000`.

## Notes

- GitHub/Google OAuth is optional for local dev. Leave `GitHub:ClientId` and `Google:ClientId` empty in `appsettings.json` to skip OAuth and use cookie auth only.
- The worker service is not needed for local testing of the frontend.
- Seq log aggregation is not started locally — logs go to stdout only.
- Redis has no password in dev (matches `appsettings.Development.json`).
