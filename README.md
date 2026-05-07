# WebIde

A competitive coding web IDE and evaluator — think LeetCode, but self-hosted and built from scratch as a learning project. Users submit code, it runs in an isolated sandbox, and results are streamed back in real time.

## Status

Lab 3 complete. EF Core with PostgreSQL is wired up, replacing mock in-memory data. Custom attribute routing is applied to all major controllers. Sorting is available on Problems, Submissions, and Leaderboard pages.

| Lab | Status | What was built |
|---|---|---|
| Lab 1 | Done | Domain model (8 classes), LINQ queries, async demo |
| Lab 2 | Done | ASP.NET MVC web layer, mock repositories, Brutalist UI |
| Lab 3 | Done | EF Core + PostgreSQL, migrations, custom routing, sorting, docs |

## Stack

| Layer | Technology |
|---|---|
| Web (MVC) | ASP.NET MVC (.NET 10) |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL (via Npgsql) |
| Real-time (planned) | SignalR |
| Cache / Sessions (planned) | Redis |
| Code Execution (planned) | Isolated sandbox (Docker) |
| Reverse Proxy (planned) | Nginx |

## Project Structure

```
WebIde.Model/       — Domain model: classes, enums, EF annotations
WebIde.DAL/         — EF DbContext, migrations
WebIde.Frontend/    — ASP.NET MVC: controllers, views, repositories
WebIde.Console/     — Lab-1 console app: LINQ queries, async demo
docs/               — Architecture, domain model, semantic model, sitemap, skills
lab-1/              — AI agent usage logs (Lab 1)
lab-3/              — AI agent usage logs (Lab 3)
```

## Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL running on `localhost:5432`

### Database setup

Start PostgreSQL (example with Docker):
```bash
docker run -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

Update the connection string in `WebIde.Frontend/appsettings.json` if needed:
```json
"WebIdeDb": "Host=localhost;Port=5432;Database=webide;Username=postgres;Password=postgres"
```

### Run migrations

```bash
cd WebIde.DAL
dotnet ef migrations add Initial --startup-project ../WebIde.Frontend --context WebIdeDbContext
dotnet ef database update --startup-project ../WebIde.Frontend --context WebIdeDbContext
```

### Run the web app

```bash
dotnet run --project WebIde.Frontend
```

### Run the Lab-1 console app

```bash
dotnet run --project WebIde.Console
```

## Documentation

- [Architecture](docs/architecture.md) — system design, layers, scaling plan
- [Domain Model](docs/domain-model.md) — all classes, enums, EF annotations
- [Semantic Model](docs/semantic-model.md) — DB tables, columns, relationships
- [Sitemap](docs/sitemap.md) — every URL, controller, action, and view
- [EF Skill](docs/skills/ef-skill.md) — guide for adding EF entities and running migrations
