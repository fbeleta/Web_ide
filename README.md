# WebIde

A competitive coding web IDE and evaluator — think LeetCode, but self-hosted and built from scratch as a learning project. Users submit code, it runs in an isolated sandbox, and results are streamed back in real time.

## Status

Lab 4 complete. Full CRUD with AJAX search, autocomplete dropdowns, client+server validation, a custom date picker partial, soft delete, and animations across all entities.

| Lab | Status | What was built |
|---|---|---|
| Lab 1 | Done | Domain model (8 classes), LINQ queries, async demo |
| Lab 2 | Done | ASP.NET MVC web layer, mock repositories, Brutalist UI |
| Lab 3 | Done | EF Core + PostgreSQL, migrations, custom routing, sorting |
| Lab 4 | Done | Full CRUD, AJAX search, autocomplete, validation, datepicker, animations |

## Stack

| Layer | Technology |
|---|---|
| Web (MVC) | ASP.NET MVC (.NET 10) |
| API | ASP.NET Core Web API (`WebIde.Api`) |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL 16 (via Npgsql) |
| Real-time (planned) | SignalR |
| Cache / Sessions (planned) | Redis |
| Code Execution (planned) | Isolated sandbox (Docker) |
| Reverse Proxy (planned) | Caddy |

## Project Structure

```
WebIde.Model/       — Domain model: classes, enums, EF annotations
WebIde.DAL/         — EF DbContext, migrations, seed data
WebIde.Frontend/    — ASP.NET MVC: controllers, views, repositories, models
WebIde.Api/         — REST API with DTOs for Problems
WebIde.Console/     — Lab-1 console app: LINQ queries, async demo
docs/               — Architecture, domain model, semantic model, sitemap
lab-4/              — AI agent usage logs (Lab 4)
```

## Running Locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop (for PostgreSQL + pgAdmin)

### 1. Start the database

```bash
docker compose up -d
```

Starts:
- `webide_postgres` on port **15432** (mapped to avoid conflicts with local Postgres)
- `webide_pgadmin` on port **5050**

pgAdmin login: `admin@webide.com` / `admin`

> **Note:** Ports 5432–5434 are reserved for local Homebrew PostgreSQL instances on this machine. The Docker container is mapped to 15432 and `appsettings.json` reflects this.

### 2. Apply migrations

From the repo root:

```bash
dotnet ef database update --project WebIde.DAL --startup-project WebIde.Frontend
```

Applies 3 migrations and seeds the database with sample data (5 problems, 4 users, 2 organizations, 8 submissions, 6 tags, 3 problem sets).

### 3. Run the web app

```bash
dotnet run --project WebIde.Frontend
```

App runs at **http://localhost:5197**

### 4. Run the console app (Lab 1)

```bash
dotnet run --project WebIde.Console
```

## Key Pages

| URL | Description |
|---|---|
| `/` | Dashboard / home |
| `/Problem` | Problem library with AJAX search + difficulty filter |
| `/Submission` | All submissions with AJAX search |
| `/User` | User management (CRUD) |
| `/Organization` | Organizations (CRUD) |
| `/Tag` | Tags (CRUD) |
| `/ProblemSet` | Problem sets (CRUD) |
| `/Leaderboard` | Ranking by accepted submissions |

## Lab 4 Features

- **Full CRUD** — Create, Edit, soft-delete (via `DeletedAt`) for all 7 entities, with confirmation modals
- **AJAX search** — Live search on every list page with skeleton loading and stagger animations
- **Autocomplete dropdown** — Reusable partial (`_AutocompleteDropdown.cshtml`) with debounced AJAX fetch, used on foreign-key fields in forms
- **Validation** — `[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]` on all form models; `ModelState.IsValid` on all POST actions; client-side via `jquery.validate` with on-blur trigger
- **Date picker** — Flatpickr-based partial (`_DatePicker.cshtml`) applied to all date fields, supports `hr` and `en` browser locales
- **Animations** — Row stagger on page load, fade-in on search results, flash notifications

## Documentation

- [Architecture](docs/architecture.md) — system design, layers, scaling plan
- [Domain Model](docs/domain-model.md) — all classes, enums, EF annotations
- [Semantic Model](docs/semantic-model.md) — DB tables, columns, relationships
- [Sitemap](docs/sitemap.md) — every URL, controller, action, and view
