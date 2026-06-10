# Phase 0 Implementation Session — 2026-06-06

## Session Summary

**Goal:** Implement Phase 0 (Infrastructure) per `docs/deployment-handoff.md` and `docs/phase-0-prompt.md`.  
**Model:** Claude Sonnet 4.6  
**Branch:** develop

---

## Files Created

| File | Purpose |
|---|---|
| `.env.template` | All env vars (§9a) with safe local-dev defaults |
| `tailwind/tailwind.config.js` | Extracted from _Layout.cshtml inline config; adds content scan + forms plugin |
| `tailwind/input.css` | Tailwind entry point for CLI compilation |
| `tailwind/package.json` | npm deps for Tailwind CLI build stage |
| `sandbox/gcc.Dockerfile` | alpine:3.20 + gcc/g++ + procps + jq + nobody user |
| `sandbox/python.Dockerfile` | python:3.12-alpine + procps + jq + nobody user |
| `sandbox/node.Dockerfile` | node:22-alpine + procps + jq + nobody user |
| `sandbox/compile-and-run.sh` | §6a C/C++ wrapper: compile → iterate cases → verdict |
| `sandbox/run-python.sh` | §6a Python wrapper: iterate cases → verdict |
| `sandbox/run-node.sh` | §6a Node.js wrapper: iterate cases → verdict |
| `nginx/nginx.conf` | TLS, rate-limit zones, WebSocket upgrade, Seq basic-auth |
| `nginx/proxy_pass.conf` | Common proxy headers snippet |
| `nginx/generate-self-signed.sh` | Self-signed cert for local dev at certbot-compatible paths |
| `Dockerfile.app` | 3-stage: node:22-alpine (Tailwind) → sdk:10 (build) → aspnet:10 (runtime) |
| `Dockerfile.worker` | 2-stage: sdk:10 (build) → aspnet:10 (runtime) — requires Phase 2 Worker project |
| `docker-compose.yml` | 8 services with resource limits, log rotation, internal networking |

---

## Key Design Decisions

### cases.json format (§6a ambiguity)
Spec says `[{ id, stdin, expected }]` but wrappers need `timeLimitMs` and `floatTolerance`. Extended to:
```json
{
  "timeLimitMs": 2000,
  "floatTolerance": null,
  "cases": [{ "id": 1, "stdin": "...", "expected": "...", "points": 10 }]
}
```
**Phase 2 worker must write cases.json in this format.**

### Memory measurement
Used `procps` package for `/usr/bin/time -v` to get "Maximum resident set size" in KB. The `/usr/bin/time` output lines are stripped from stderr before returning it to the user.

### nginx self-signed cert
`generate-self-signed.sh` is placed in `/docker-entrypoint.d/` so nginx's entrypoint runs it before the main process. Cert generated at `/etc/letsencrypt/live/localhost/` — same path structure as Let's Encrypt, so prod needs zero nginx.conf changes.

### Worker service in compose
`webide-worker` is defined but its image (`ghcr.io/fbeleta/web_ide/worker:latest`) doesn't exist until Phase 2. `docker compose up --scale webide-worker=0` can be used locally to skip it until then.

---

## Phase 0 Acceptance Checklist

Run after building sandbox images locally:

```bash
# 1. Setup
cp .env.template .env
# Edit .env: set ASPNETCORE_ENVIRONMENT=Development, fill POSTGRES_PASSWORD, REDIS_PASSWORD,
#            SEQ_FIRSTRUN_ADMINPASSWORD, set SANDBOX_*_DIGEST to local tag names

# 2. Build sandbox images
docker build -t webide-sandbox-gcc    -f sandbox/gcc.Dockerfile    sandbox/
docker build -t webide-sandbox-python -f sandbox/python.Dockerfile sandbox/
docker build -t webide-sandbox-node   -f sandbox/node.Dockerfile   sandbox/

# 3. Build app image
docker build -t webide-app -f Dockerfile.app .

# 4. Bring stack up (skip worker until Phase 2)
docker compose up -d --scale webide-worker=0

# 5. Health check
curl -k https://localhost/health   # expect 200

# 6. Adversarial tests (§13b)
mkdir -p /tmp/test-src

# Fork bomb
echo 'import os
while True: os.fork()' > /tmp/test-src/solution.py
cat > /tmp/test-src/cases.json <<'EOF'
{"timeLimitMs":2000,"floatTolerance":null,"cases":[{"id":1,"stdin":"","expected":"","points":1}]}
EOF
docker run --rm --network none --read-only \
  --tmpfs /tmp:size=64m,mode=1777 \
  --memory 512m --memory-swap 512m --cpus 0.9 \
  --pids-limit 64 --user nobody:nogroup \
  --security-opt no-new-privileges --cap-drop ALL \
  --ulimit fsize=67108864 \
  -v /tmp/test-src:/code:ro \
  webide-sandbox-python /code/solution.py /code/cases.json
# Expect: TLE or process killed; host ps count unaffected

# Identity check
echo 'import os; print(os.getuid())' > /tmp/test-src/solution.py
docker run --rm --network none --read-only \
  --tmpfs /tmp:size=64m,mode=1777 \
  --memory 512m --memory-swap 512m --cpus 0.9 \
  --pids-limit 64 --user nobody:nogroup \
  --security-opt no-new-privileges --cap-drop ALL \
  --ulimit fsize=67108864 \
  -v /tmp/test-src:/code:ro \
  webide-sandbox-python /code/solution.py /code/cases.json
# Expect: verdict Accepted, stdout "65534"
```

---

## Human-Only Tasks (Status: Pending)

1. [ ] Provision Hetzner VPS and run §11 server setup
2. [ ] Create GitHub OAuth app per §12; copy credentials to `/opt/webide/.env`
3. [ ] Set GitHub branch protection on `main`; add `HETZNER_HOST`, `HETZNER_USER`, `HETZNER_SSH_KEY` secrets
4. [ ] Run `certbot certonly --webroot ...` to mint the first cert (§11b)
5. [ ] Choose backup offsite target and provision credentials (§11c)
6. [ ] Generate `POSTGRES_PASSWORD`, `REDIS_PASSWORD`, `SEQ_FIRSTRUN_ADMINPASSWORD`, `SEQ_BASIC_AUTH_HTPASSWD`

---

## Open Questions for Phase 1

1. `PUBLIC_HOSTNAME` value for the GitHub OAuth callback URL registration?
2. Should `/auth/github/login` preserve a `returnUrl` query param?
3. `_Sidebar.cshtml` has `RANK: #1` hardcoded — replace with actual user rank query, or remove for Phase 1?
