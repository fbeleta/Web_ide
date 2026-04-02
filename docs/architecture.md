# Architecture

## Overview

WebIde is a self-hosted competitive coding platform. Users write code in a browser-based editor, submit it, and get results streamed back in real time. The system separates concerns into distinct layers: presentation, business logic, and code execution.

## Current State

The domain model layer (`WebIde.Model`) is implemented as a .NET class library. A console app (`WebIde.Console`) exercises the model with LINQ queries and async patterns. No web layer exists yet.

## Target Architecture

```
Browser
  └── React/TypeScript SPA
        │
        │ HTTP /api/*
        ▼
  ASP.NET MVC (.NET 10)          ← controllers, services, EF Core
        │
        ├── PostgreSQL            ← persistent storage
        ├── Redis                 ← sessions, caching
        └── Sandbox Worker        ← isolated code execution (Docker)
              │
              │ SignalR
              ▼
        Browser (real-time execution output stream)
```

Nginx sits in front of everything as a reverse proxy:
- `/app/*` → React frontend (Nginx static in prod, Vite dev server in dev)
- `/api/*` → ASP.NET MVC backend

## Layers

### WebIde.Model
Pure C# class library. No dependencies on any framework. Contains:
- Domain entity classes (Problem, Submission, User, etc.)
- Enums (DifficultyLevel, SubmissionStatus, UserRole)

This library is referenced by any layer that needs to work with the domain — backend API, tests, etc.

### Backend (planned)
ASP.NET MVC (.NET 10). Responsibilities:
- REST API for the frontend
- Business logic (submission scoring, leaderboard updates)
- EF Core ORM talking to PostgreSQL
- SignalR hub for streaming execution output

### Frontend (planned)
React + TypeScript, bundled with Vite. Responsibilities:
- Code editor (Monaco Editor)
- Problem browser, submission history, leaderboard
- SignalR client for real-time output

### Sandbox (planned)
Isolated execution environment for untrusted code. Each submission gets:
- A container with strict CPU, memory, and time limits
- No network access
- Results (stdout, stderr, exit code, wall time, peak memory) written back to the backend

## Scaling Plan

| Stage | Users | Infrastructure |
|---|---|---|
| 1 | ~100 | Single VPS (4 vCPU / 8 GB), 2 sandbox runners |
| 2 | ~500 | App node + dedicated worker node, managed PostgreSQL |
| 3+ | 5 000+ | Kubernetes, auto-scaling worker pools, read replicas |

The bottleneck at scale is the sandbox, not the API or database. C/C++ submissions take ~1s compile + ~2s run, so sandbox runners need dedicated CPU for deterministic timing.
