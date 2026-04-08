# Deployment Plan: Hetzner CX33 Monolith (Full Production Stack)

## Context
WebIde is a kids' coding platform where students submit code that gets compiled/run server-side. Deployed as a monolith on Hetzner **CX33** (x86, 2 vCPU, 8GB RAM, 80GB disk). Currently no deployment config exists — only a dev `docker-compose.yml` for PostgreSQL/pgAdmin.

## Architecture

```
Internet → Cloudflare (DDoS protection, CDN, DNS)
         → Hetzner Firewall (ports 80, 443, SSH)
         → CX33 Server (UFW backup firewall)
         → Caddy (Cloudflare origin cert, rate limiting, security headers)
            ├── /api/* → .NET API (port 5000)
            └── /* → Frontend static files
                      ↓
                 PostgreSQL 16 (Docker volume)
                      ↓
                 Sandbox containers (user code, isolated)
```

---

## Phase 0: Domain & DNS (Manual — Before Deployment)

### 0.1 Purchase Domain
- Buy from Porkbun, Namecheap, or Cloudflare Registrar (cheapest if using CF anyway)
- Point nameservers to Cloudflare (free plan)

### 0.2 Cloudflare Setup
- Add site to Cloudflare free tier
- Set DNS A record → CX33 public IPv4
- Set DNS AAAA record → CX33 IPv6
- Enable **Proxy mode** (orange cloud) for DDoS protection + CDN
- SSL/TLS mode: **Full (Strict)** — Cloudflare ↔ Caddy uses origin certificate
- Generate **Cloudflare Origin Certificate** (valid 15 years), download cert + key
- Enable: Bot Fight Mode, Under Attack Mode (toggle during incidents), Browser Integrity Check

### 0.3 Caddy with Cloudflare Origin Cert
- Caddy uses the Cloudflare origin cert instead of Let's Encrypt
- This ensures traffic is encrypted end-to-end (Cloudflare → Caddy)
- Origin cert + key stored as Docker secrets

---

## Phase 1: Dockerization & Compose

### 1.1 API Dockerfile
- **Create:** `WebIde.Api/Dockerfile`
- Multi-stage: `dotnet/sdk:10.0` build → `dotnet/aspnet:10.0` runtime
- Publish as single-file, expose port 5000

### 1.2 Frontend Dockerfile
- **Create:** `WebIde.Frontend/Dockerfile`
- Node 22 build stage → static `dist/` output
- Final output copied into Caddy service volume

### 1.3 Production Compose
- **Create:** `docker-compose.prod.yml`
- Services: `caddy`, `api`, `postgres`
- No pgAdmin in prod
- Internal network for service-to-service comms
- Named volumes for postgres data and caddy data/config

### 1.4 Dockerignore & Env
- **Create:** `WebIde.Api/.dockerignore`, `WebIde.Frontend/.dockerignore`
- **Create:** `.env.example` — template for all prod secrets
- **Modify:** `WebIde.Api/appsettings.json` — connection string from env var
- **Modify:** `WebIde.Api/Program.cs` — env var fallback for connection string

---

## Phase 2: Reverse Proxy & TLS

### 2.1 Caddy Configuration
- **Create:** `Caddyfile`
- TLS via Cloudflare Origin Certificate (not Let's Encrypt — Cloudflare terminates public TLS)
- Reverse proxy `/api/*` → `api:5000`
- Serve frontend static files from `/srv/frontend`
- Security headers: `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, CSP
- Rate limiting on `/api/*` endpoints (especially code submission)
- Only accept connections from Cloudflare IP ranges (reject direct-to-IP requests)

---

## Phase 3: Authentication & Authorization

### 3.1 ASP.NET Identity Setup
- **Modify:** `WebIde.Api.csproj` — add `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- **Modify:** `AppDbContext.cs` — inherit from `IdentityDbContext`, add identity tables
- **Create:** `WebIde.Api/Controllers/AuthController.cs`
  - `POST /api/auth/register` — student/teacher registration
  - `POST /api/auth/login` — returns JWT
  - `POST /api/auth/refresh` — token refresh
- **Create:** `WebIde.Api/Services/TokenService.cs` — JWT generation/validation

### 3.2 JWT Configuration
- **Modify:** `Program.cs` — add authentication/authorization middleware
- JWT secret from environment variable
- Role-based auth: `Student`, `Teacher`, `Admin`
- Protect existing endpoints (`[Authorize]` attributes)

### 3.3 Auth Models
- Leverage existing `User` model in WebIde.Model, map to ASP.NET Identity
- Add EF migration for identity tables

---

## Phase 4: Code Sandbox Security

### 4.1 Sandbox Container Config
- **Create:** `WebIde.Api/Services/SandboxService.cs` — orchestrates container lifecycle
- **Create:** `sandbox/Dockerfile` — base image with GCC, JDK, Node pre-installed
- Containers run with:
  - `--network none` (no internet access)
  - `--memory 256m --cpus 0.5` (resource limits)
  - `--read-only` filesystem (tmpfs for `/tmp` only)
  - `--user nobody` (non-root)
  - `--security-opt seccomp=sandbox-seccomp.json` (restricted syscalls)
  - Timeout: kill after 10 seconds
- **Create:** `sandbox/seccomp-profile.json` — allowlist of safe syscalls

### 4.2 Docker Socket Access
- API container gets read/write access to Docker socket (sibling containers pattern)
- Consider using Docker SDK for .NET (`Docker.DotNet`) to manage sandbox lifecycle

---

## Phase 5: Server Hardening

### 5.1 Firewall
- **Create:** `deploy/setup-firewall.sh`
- Hetzner Cloud Firewall: allow only 80, 443, SSH (from your IP)
- UFW on server as backup: same rules
- Note: Docker bypasses UFW by default — use `DOCKER_OPTS="--iptables=false"` or Hetzner's firewall as primary

### 5.2 SSH Hardening
- **Create:** `deploy/setup-ssh.sh`
- Disable password auth, root login
- Key-only authentication
- Install and configure `fail2ban`

### 5.3 Secrets Management
- All secrets via `.env` file with `chmod 600`
- Docker secrets for sensitive values in compose
- Never bake credentials into images
- Rotate JWT secret and DB password on initial deploy

---

## Phase 6: Backups & Monitoring

### 6.1 Database Backups
- **Create:** `deploy/backup-db.sh`
- `pg_dump` via cron (daily)
- Store backups to Hetzner Object Storage (S3-compatible) or local with rotation (keep 7 days)
- Test restore procedure documented

### 6.2 Monitoring (lightweight)
- Docker healthchecks on all services in compose
- **Create:** `deploy/healthcheck.sh` — simple HTTP check, alert via webhook on failure
- Docker log rotation configured in compose (`json-file` driver, max 10MB, 3 files)

---

## Files Summary

| Action | File |
|--------|------|
| Create | `WebIde.Api/Dockerfile` |
| Create | `WebIde.Api/.dockerignore` |
| Create | `WebIde.Frontend/Dockerfile` |
| Create | `WebIde.Frontend/.dockerignore` |
| Create | `docker-compose.prod.yml` |
| Create | `Caddyfile` |
| Create | `.env.example` |
| Create | `WebIde.Api/Controllers/AuthController.cs` |
| Create | `WebIde.Api/Services/TokenService.cs` |
| Create | `WebIde.Api/Services/SandboxService.cs` |
| Create | `sandbox/Dockerfile` |
| Create | `sandbox/seccomp-profile.json` |
| Create | `deploy/setup-firewall.sh` |
| Create | `deploy/setup-ssh.sh` |
| Create | `deploy/backup-db.sh` |
| Create | `deploy/healthcheck.sh` |
| Modify | `WebIde.Api/WebIde.Api.csproj` — add Identity + Docker.DotNet packages |
| Modify | `WebIde.Api/Data/AppDbContext.cs` — inherit IdentityDbContext |
| Modify | `WebIde.Api/Program.cs` — add auth middleware, env var config |
| Modify | `WebIde.Api/appsettings.json` — env var connection string |
| Modify | `WebIde.Api/Controllers/ProblemsController.cs` — add `[Authorize]` |

## Verification
1. `docker compose -f docker-compose.prod.yml build` — all images build
2. `docker compose -f docker-compose.prod.yml up` — stack starts
3. `curl https://localhost/api/auth/register` — registration works
4. `curl -H "Authorization: Bearer <token>" https://localhost/api/problems` — auth-protected endpoint works
5. Submit test code → sandbox container runs and returns result within timeout
6. Verify sandbox has no network, limited memory, and gets killed after timeout
7. Verify `pg_dump` backup script produces valid dump
