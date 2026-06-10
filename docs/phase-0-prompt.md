You are implementing the deployment of a competitive coding platform called WebIde. The complete specification is at /Users/fbeleta/Documents/Web-ide/Web_ide/docs/deployment-handoff.md — read it first, in full, before doing anything else. It is ~750 lines and is the source of truth. Do not improvise around it.

## Repo context

- Working directory: /Users/fbeleta/Documents/Web-ide/Web_ide
- Solution: WebIde.slnx (projects: WebIde.Model, WebIde.DAL, WebIde.Frontend, WebIde.Console; empty WebIde.Api dir — ignore)
- Branch: develop (main is the deploy target)
- .NET 10, EF Core 9, PostgreSQL, Razor MVC. No JS build pipeline currently. No tests project.
- The current Program.cs has no auth, no Redis, no SignalR. The User entity has no GitHubId/AvatarUrl yet. The data model is otherwise complete.
- _Sidebar.cshtml has a hardcoded "ROOT_USER" string and a hardcoded single-letter "R" avatar that need to be replaced.

## What I want you to do this session

**Implement Phase 0 (Infrastructure) only.** Do not start Phase 1. See §7 of the handoff doc for the file list and the "Done when" criterion. That is roughly:
- docker-compose.yml with all 8 services + log rotation + internal-only networking for postgres/redis/seq
- nginx/nginx.conf per §10 with TLS, rate-limit zones, /hubs/ WebSocket upgrade, basic-auth on /seq/
- Dockerfile.app (multi-stage: node tailwind build → sdk build → aspnet runtime)
- Dockerfile.worker
- sandbox/{gcc,python,node}.Dockerfile + the three wrapper scripts implementing §6a's judging contract
- tailwind/input.css + tailwind.config.js to replace the Tailwind Play CDN
- .env.template per §9a
- (Worker .csproj does not exist yet — you'll create it in Phase 2, not now. Compose can reference the future image.)

Stop after Phase 0 is done. Do not move to Phase 1 in this session.

## Non-negotiable rules

1. The §6 docker run flag list is exact. Do not modify it. In particular: `--pids-limit 64` (not `--ulimit nproc`), `--user nobody:nogroup`, `--network none`, `--read-only`, `--cap-drop ALL`, image pinned by digest.
2. The §6a judging contract is the spec for the wrapper scripts. Implement output normalization, float tolerance, multi-case model, stdout/stderr caps, and per-case verdict precedence exactly as written. If anything in §6a is ambiguous, ASK — do not guess. Wrong-answer bugs here are catastrophic.
3. HTTPS is required day 1 (§11b). Self-signed cert for local dev is fine; structure compose and nginx so certbot can take over in prod without code changes.
4. No `:latest` tags in production references. Pin sandbox images by digest via env var (§9a). The deploy pipeline (Phase 5, later) promotes `:latest` only after health check passes.
5. Postgres, Redis, Seq must NOT publish ports to the host. Only nginx publishes 80/443.

## What you should NOT do

- Do NOT make any changes to source code in WebIde.Frontend, WebIde.Model, WebIde.DAL. That's Phase 1.
- Do NOT create the Worker project. That's Phase 2.
- Do NOT attempt: provisioning the Hetzner VPS, creating the GitHub OAuth app, setting branch protection, running certbot's first cert, choosing the backup offsite target, generating production secrets. These are human-only tasks listed below.
- Do NOT add code I didn't ask for (no health endpoints, no controllers, no Program.cs edits in this session).
- Do NOT use the Tailwind Play CDN. The §7 Phase 0 file list replaces it with a build-time compile.

## Human-only tasks (do not attempt; just list them at the end of your work for me)

1. Provision the Hetzner VPS and run §11 server setup.
2. Create the GitHub OAuth app per §12; copy Client ID + Secret to /opt/webide/.env.
3. Set GitHub branch protection on `main` and add HETZNER_HOST, HETZNER_USER, HETZNER_SSH_KEY secrets (§12a).
4. Run the initial `certbot certonly --webroot ...` to mint the first cert (§11b).
5. Decide the backup offsite target (Hetzner Storage Box / B2 / S3) and provision credentials (§11c).
6. Generate POSTGRES_PASSWORD, REDIS_PASSWORD, SEQ_BASIC_AUTH_HTPASSWD per §9a.

## How to work

- Verify each file against the spec before writing the next one. The doc cross-references heavily; missing details in one section are usually pinned down in another (§10, §6a, §9a especially).
- After all Phase 0 files exist, run the Phase 0 acceptance check from §7: `docker compose up` brings the stack up locally, `/health` returns 200, HTTPS works against a self-signed cert, and the fork-bomb test from §13b kills the container without disturbing the host.
- If something in the spec is genuinely unclear or seems wrong, stop and ask me. Don't invent.
- End your session with: (a) list of files created, (b) the Phase 0 acceptance results, (c) the human-only checklist with status, (d) any open questions for Phase 1.
