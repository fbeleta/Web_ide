---
name: webide-spec
description: Use this skill at the start of any implementation session for the WebIde competitive coding platform — when editing any .NET project (WebIde.Frontend, WebIde.DAL, WebIde.Model, WebIde.Worker), Docker config, GitHub workflow, nginx config, or sandbox script. It orients you to the project state, the source-of-truth spec, and the phased implementation contract.
---

# WebIde Implementation Orientation

WebIde is an ASP.NET MVC competitive coding platform being taken from a read-only local app to a production-grade deployment on Hetzner.

## Source of truth

**The complete spec is at `docs/deployment-handoff.md`** (~750 lines). Read it before doing anything. Every other doc in this repo (including this skill) is a pointer to a section of that spec, not a replacement.

If a skill and the handoff doc disagree, the handoff doc wins. If you change behavior covered by the handoff doc, update the doc in the same PR.

## Phased implementation contract

| Phase | Focus | Prompt file |
|---|---|---|
| 0 | Infrastructure (Docker, nginx, sandbox images, tailwind build) | `docs/phase-0-prompt.md` |
| 1 | Auth + HTTPS wiring + Program.cs rewrite | (write when Phase 0 lands) |
| 2 | Worker service (judging engine) | — |
| 3 | Submission endpoint + SignalR + Monaco editor | — |
| 4 | End-to-end smoke + adversarial verification | — |
| 5 | CI/CD | — |

**Stay in your assigned phase.** Cross-phase scope creep has burned the project before. If the current session is Phase 0, do NOT touch `Program.cs`. If it's Phase 1, do NOT create the Worker project.

Each phase has a "Done when" acceptance criterion in §7 of the handoff doc. Hit it before declaring complete.

## Current codebase snapshot (as of project start)

- .NET 10, EF Core 9, PostgreSQL. Solution at `WebIde.slnx`.
- Projects: `WebIde.Model`, `WebIde.DAL`, `WebIde.Frontend`, `WebIde.Console`. `WebIde.Api` dir is empty — ignore.
- `WebIde.Frontend/Program.cs` has no auth, no Redis, no SignalR, no rate limiter.
- `User` has no `GitHubId`/`AvatarUrl` yet (added in Phase 1).
- `Problem` has no `FloatTolerance` yet (added in Phase 1).
- `_Sidebar.cshtml` has a hardcoded `ROOT_USER` string + single-letter "R" avatar (replaced in Phase 1).
- Tailwind currently served via Play CDN — replaced with build-time compile in Phase 0.
- No tests project. No CI. No Docker yet.

## Companion skills to load by area

| When working on… | Also load |
|---|---|
| `sandbox/*` files, `SubmissionEvaluator`, anything judging-related | `webide-sandbox` |
| `WebIde.Frontend/Program.cs`, auth, middleware, SignalR | `webide-aspnet-prod` |
| Anything in `WebIde.Worker/` | `webide-worker` |
| `.github/workflows/*.yml`, deploy mechanics, migrations | `webide-deploy` |

## Hard rules across all phases

1. **No `:latest` tags in production references.** Sandbox images pinned by `@sha256:<digest>`.
2. **HTTPS day 1.** GitHub OAuth does not work over HTTP. Cookie auth requires `Secure=Always`.
3. **No port publishing for Postgres/Redis/Seq** — internal Docker network only. Only nginx exposes 80/443.
4. **Sandbox `docker run` flags from §6 are exact.** Don't modify them without asking.
5. **Judging contract §6a is exact.** Output normalization, float tolerance, verdict precedence are written down for a reason.
6. **Migrations are additive only per deploy** (§10a). Two-deploy pattern for destructive changes.
