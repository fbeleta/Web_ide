# WebIde

A competitive coding web IDE and evaluator — think LeetCode, but self-hosted and built from scratch as a learning project. Users submit code, it runs in an isolated sandbox, and results are streamed back in real time.

## Status

Early development. The domain model layer (Lab-1) is complete. The web application (ASP.NET MVC backend + React/TypeScript frontend) is next.

## Planned Stack

| Layer | Technology |
|---|---|
| Frontend | React + TypeScript (Vite dev, Nginx prod) |
| Backend | ASP.NET MVC (.NET 10) |
| Real-time | SignalR |
| Database | PostgreSQL |
| Cache / Sessions | Redis |
| Code Execution | Isolated sandbox (Docker) |
| Reverse Proxy | Nginx |

## Project Structure

```
WebIde.Model/       — Domain model: classes, enums, relationships
WebIde.Console/     — Lab-1 console app: data population, LINQ queries, async demo
docs/               — Architecture and design documentation
lab-1/              — AI agent usage logs (course requirement)
```

## Running the Console App (Lab-1)

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project WebIde.Console
```

## Documentation

- [Architecture](docs/architecture.md) — system design, layers, scaling plan
- [Domain Model](docs/domain-model.md) — all classes, enums, and their relationships
