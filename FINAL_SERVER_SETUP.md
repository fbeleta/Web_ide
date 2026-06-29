# FINAL_SERVER_SETUP.md — Deploy WebIde to your existing server

This is the **single authoritative checklist** for cloning WebIde onto a server you already
control and bringing the full stack (web app, judge worker, Postgres, Redis, nginx/TLS) up so
that users can **write code, have it compiled, executed in a sandbox, and see the result on the
site**.

It is the condensed "do this in order" path. For deeper explanations of any step, see
[`docs/server-setup.md`](docs/server-setup.md) and the source-of-truth spec
[`docs/deployment-handoff.md`](docs/deployment-handoff.md).

The end-to-end submission pipeline (queue → worker → Docker sandbox → DB → live result via
SignalR) has been verified working. The sandbox has hard requirements — see
[§0 Prerequisites](#0-prerequisites-read-first), they are the things most likely to bite you.

---

## 0. Prerequisites (read first)

| Requirement | Why | How to check |
|---|---|---|
| **Linux host with cgroup v2** | The sandbox reads `/sys/fs/cgroup/memory.peak` for per-run memory and relies on cgroup limits for MLE. | `stat -fc %T /sys/fs/cgroup` → must print `cgroup2fs`. Standard on Ubuntu 22.04+/Debian 12+. |
| **Docker Engine + Compose v2** | Runs the whole stack; the worker spawns sandbox containers as siblings (Docker-out-of-Docker). | `docker --version && docker compose version` |
| **The worker can reach the Docker socket** | It launches sandbox containers via `/var/run/docker.sock` (already mounted in compose). | n/a (compose handles it) |
| **`/tmp/webide-src` exists, owned by the deploy user, mode 700** | Worker writes each submission's source + test cases here and bind-mounts it read-only into the sandbox. | created in §2 |
| **`DEPLOY_UID`/`DEPLOY_GID` in `.env` match the deploy user** | The worker container runs as that UID so it can read/write `/tmp/webide-src`. | `id deploy` |
| **`sandbox/seccomp-profile.json` present in the repo** | The worker reads this file and passes its **contents** to each sandbox container. It is mounted into the worker at `/sandbox/seccomp-profile.json` by compose. | it's committed; don't delete it |
| **A domain pointed at the server + ports 80/443 open** | HTTPS is required from day 1 (GitHub OAuth and secure cookies do not work over HTTP). | DNS A record + firewall |

> **Sandbox gotchas already handled in code** (FYI — you don't need to do anything, but don't
> "revert" them): the worker inlines the seccomp JSON (the Docker API rejects a file *path*); the
> seccomp profile allows `kill` (busybox `timeout` needs it or time-limit kills hang); and the
> sandbox `/tmp` is mounted `exec` (Docker tmpfs is `noexec` by default, which would break every
> C/C++ submission). These live in `WebIde.Worker/Services/SandboxOrchestrator.cs`,
> `sandbox/seccomp-profile.json`, and the sandbox wrapper scripts.

---

## 1. Install Docker & harden the host

```bash
ssh root@YOUR_SERVER_IP

# Docker
curl -fsSL https://get.docker.com | sh
systemctl enable --now docker

# Firewall: allow only 22, 80, 443 (use Hetzner Cloud Firewall or ufw)
ufw allow 22,80,443/tcp && ufw enable     # if using ufw

# SSH hardening — in /etc/ssh/sshd_config set:
#   PasswordAuthentication no
#   PermitRootLogin no
systemctl reload sshd

apt install -y fail2ban unattended-upgrades && systemctl enable --now fail2ban
```

Full hardening detail: [`docs/server-setup.md` §2](docs/server-setup.md).

---

## 2. Create the deploy user & directories

```bash
useradd -m -s /bin/bash deploy
usermod -aG docker deploy

mkdir -p /opt/webide /tmp/webide-src
chown deploy:deploy /opt/webide /tmp/webide-src
chmod 700 /tmp/webide-src

id deploy        # <-- note the uid/gid; you need them for DEPLOY_UID/DEPLOY_GID in .env

# SSH key the GitHub Actions deploy will use (paste the PUBLIC key):
mkdir -p /home/deploy/.ssh
echo "ssh-ed25519 AAAA... your-ci-key" >> /home/deploy/.ssh/authorized_keys
chmod 700 /home/deploy/.ssh && chmod 600 /home/deploy/.ssh/authorized_keys
chown -R deploy:deploy /home/deploy/.ssh
```

---

## 3. Create the OAuth app(s)

**GitHub (required for GitHub login):** github.com → Settings → Developer settings → OAuth Apps → New
- Homepage URL: `https://YOUR_DOMAIN`
- Authorization callback URL: `https://YOUR_DOMAIN/auth/github/callback`
- Copy the **Client ID** and a generated **Client Secret**.

**Google (optional):** console.cloud.google.com → Credentials → OAuth 2.0 Client ID (Web)
- Authorized redirect URI: `https://YOUR_DOMAIN/signin-google`
- Leave the `GOOGLE_*` vars blank to disable; GitHub + local accounts still work.

---

## 4. Clone the repo (replacing whatever runs now)

```bash
su - deploy

# Stop the old stack if one is running
docker ps && docker compose ls
# cd /path/to/old/repo && docker compose down   # (adjust to your current setup)

cd /opt/webide
git clone https://github.com/fbeleta/Web_ide.git .
# If /opt/webide already has the old repo:
#   git remote set-url origin https://github.com/fbeleta/Web_ide.git
#   git fetch origin && git reset --hard origin/main
# Private repo? use a PAT: git clone https://YOUR_PAT@github.com/fbeleta/Web_ide.git .
```

---

## 5. Configure `.env`

```bash
cd /opt/webide
cp .env.template .env
chmod 600 .env
nano .env
```

Fill in (see `.env.template` for the full annotated list):

```bash
ASPNETCORE_ENVIRONMENT=Production
PUBLIC_HOSTNAME=YOUR_DOMAIN
LETSENCRYPT_EMAIL=you@example.com

POSTGRES_USER=webide
POSTGRES_DB=webide
POSTGRES_PASSWORD=         # openssl rand -base64 32
REDIS_PASSWORD=           # openssl rand -base64 32

GITHUB_CLIENT_ID=         # from §3
GITHUB_CLIENT_SECRET=
GOOGLE_CLIENT_ID=         # optional
GOOGLE_CLIENT_SECRET=

WORKER__MAXCONCURRENTSANDBOXES=2
WORKER__SANDBOXMEMMB=512
WORKER__SANDBOXCPUS=0.9

# Sandbox images: leave blank to use the locally-built tags (webide-sandbox-*),
# OR set to ghcr digests after CI builds them (see §9).
SANDBOX_GCC_DIGEST=
SANDBOX_PYTHON_DIGEST=
SANDBOX_NODE_DIGEST=

IMAGE_TAG=latest
DEPLOY_UID=1000          # <-- from `id deploy`
DEPLOY_GID=1000          # <-- from `id deploy`
```

Generate the secrets quickly:
```bash
echo "POSTGRES_PASSWORD=$(openssl rand -base64 32)"
echo "REDIS_PASSWORD=$(openssl rand -base64 32)"
```

> **Important — `docker-compose.override.yml`:** the committed override publishes Postgres (5433)
> and Redis (6379) to the host for *local development*. **On the server you do not want this** —
> Postgres/Redis must stay internal-only. Either delete `docker-compose.override.yml` on the
> server, or always deploy with `docker compose -f docker-compose.yml ...` (explicit base file
> only). The CI deploy pipeline uses the base file.

---

## 6. TLS certificate

nginx auto-generates a self-signed cert on first start (`nginx/generate-self-signed.sh`), so the
stack can come up immediately. Swap in a real Let's Encrypt cert:

```bash
cd /opt/webide
# Point nginx.conf at your domain
sed -i 's|live/localhost|live/YOUR_DOMAIN|g' nginx/nginx.conf
sed -i 's|server_name _;|server_name YOUR_DOMAIN;|g' nginx/nginx.conf

# After the stack is up (§8), obtain the cert and reload:
docker compose run --rm certbot certonly --webroot -w /var/www/acme \
  -d YOUR_DOMAIN -m "$LETSENCRYPT_EMAIL" --agree-tos --non-interactive
docker compose restart nginx
```

---

## 7. Build the images (first deploy)

CI builds these for you (§9), but for the very first bring-up build them on the server:

```bash
cd /opt/webide

# App + worker
docker build -t ghcr.io/fbeleta/web_ide/app:latest    -f Dockerfile.app    .
docker build -t ghcr.io/fbeleta/web_ide/worker:latest -f Dockerfile.worker .

# Sandbox images (REQUIRED for code execution)
docker build -t webide-sandbox-python -f sandbox/python.Dockerfile sandbox/
docker build -t webide-sandbox-gcc    -f sandbox/gcc.Dockerfile    sandbox/
docker build -t webide-sandbox-node   -f sandbox/node.Dockerfile   sandbox/
```

With `SANDBOX_*_DIGEST` left blank in `.env`, the worker uses these local `webide-sandbox-*` tags
directly. (Once CI pushes digest-pinned images to ghcr.io, set the `SANDBOX_*_DIGEST` vars to the
`sha256:...` digests for reproducible production runs.)

---

## 8. Migrate the DB & start the stack

```bash
cd /opt/webide

# Postgres first, then migrate
docker compose -f docker-compose.yml up -d postgres
sleep 5
docker run --rm --network webide_webide-net --env-file .env \
  -e ConnectionStrings__WebIdeDb="Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" \
  ghcr.io/fbeleta/web_ide/app:latest \
  /app/efbundle --connection "Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"

# Bring everything up
docker compose -f docker-compose.yml up -d
docker compose logs -f webide-app webide-worker nginx
```

Grant yourself Admin (needed to create problems / upload files): register an account at
`https://YOUR_DOMAIN`, then follow [`docs/server-setup.md` §9b](docs/server-setup.md).

---

## 9. CI/CD (GitHub Actions)

Three workflows are ready in `.github/workflows/`: `ci.yml` (build+test), `deploy.yml` (build →
push → SSH deploy → migrate → health-check → promote/rollback), `sandbox.yml` (build+push sandbox
images on `sandbox/**` changes).

Add repo secrets (**Settings → Secrets and variables → Actions**):

| Secret | Value |
|---|---|
| `HETZNER_HOST` | server IP |
| `HETZNER_USER` | `deploy` |
| `HETZNER_SSH_KEY` | the **private** key matching §2's public key |

Set branch protection on `main` (require CI + PR review). After `sandbox.yml` runs, copy the
pushed image **digests** into the server `.env` (`SANDBOX_*_DIGEST=sha256:...`) so production pins
sandbox versions by digest.

---

## 10. Verify it actually works

```bash
# Liveness + readiness (readiness checks Postgres, Redis, AND the worker heartbeat)
curl -sk https://YOUR_DOMAIN/health         # → Healthy
curl -sk https://YOUR_DOMAIN/health/ready   # → Healthy

# HTTP → HTTPS redirect
curl -sI http://YOUR_DOMAIN | grep Location

# GitHub login round-trip (browser): https://YOUR_DOMAIN/auth/github/login
```

**Sandbox identity check** (should print uid 65534 = nobody, proving isolation):

```bash
mkdir -p /tmp/test-src
echo 'import os; print(os.getuid())' > /tmp/test-src/solution.py
echo '{"timeLimitMs":2000,"cases":[{"id":1,"stdin":"","expected":"65534","points":1}]}' > /tmp/test-src/cases.json
docker run --rm --network none --read-only \
  --tmpfs /tmp:size=64m,mode=1777,exec \
  --memory 512m --memory-swap 512m --cpus 0.9 --pids-limit 64 \
  --user nobody:nogroup --security-opt no-new-privileges \
  --security-opt seccomp=sandbox/seccomp-profile.json \
  --cap-drop ALL --ulimit fsize=67108864 \
  -v /tmp/test-src:/code:ro \
  webide-sandbox-python /code/solution.py /code/cases.json
# Expect: [{"id":1,"verdict":"Accepted",...}]
```

**Full judge smoke test:** log in, open a problem, submit a solution in the Monaco editor, and
confirm the status chip goes PENDING → RUNNING → a final verdict (ACCEPTED / WRONG_ANSWER /
TIME_LIMIT_EXCEEDED / COMPILE_ERROR / RUNTIME_ERROR) with per-case results — live, without a page
reload. If it stays PENDING forever, the worker isn't consuming the queue: check
`docker compose logs webide-worker` and that `worker:heartbeat` exists in Redis
(`docker compose exec redis redis-cli -a "$REDIS_PASSWORD" GET worker:heartbeat`).

---

## 11. Day-2 quick reference

```bash
docker compose -f docker-compose.yml ps                 # status
docker compose -f docker-compose.yml logs -f webide-worker
docker compose -f docker-compose.yml exec postgres psql -U webide -d webide
docker compose -f docker-compose.yml run --rm certbot renew --force-renewal && docker compose restart nginx
docker system df                                         # disk usage
```

**Still to wire up later** (non-blocking): offsite Postgres backup target
(`BACKUP_OFFSITE_TARGET`), Seq log aggregation, and the quarterly backup-restore drill — see
[`docs/deployment-handoff.md` §11c](docs/deployment-handoff.md).
