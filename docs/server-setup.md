# Server Setup & Deployment Guide

**Audience:** You, deploying WebIde to Hetzner for the first time (replacing an existing repo).  
**Assumes:** Domain is already pointed at the server. You have SSH access as root or a sudo user.

---

## Overview of what this guide covers

1. [Prep the server](#1-prep-the-server) — Docker, deploy user, directories
2. [GitHub OAuth app](#2-github-oauth-app) — create it, get the credentials
3. [Clone the repo](#3-clone-the-repo-onto-the-server) — replacing whatever is running now
4. [Configure secrets](#4-configure-secrets-env-file) — fill in `.env`
5. [Generate Seq basic-auth password](#5-generate-seq-basic-auth-htpasswd)
6. [Get the TLS certificate](#6-get-the-tls-certificate) — Let's Encrypt via certbot
7. [Build the app image](#7-build-the-app-image)
8. [Run migrations](#8-run-database-migrations)
9. [Start the stack](#9-start-the-stack)
10. [Set up GitHub Actions CI/CD](#10-github-actions-cicd)
11. [Verify everything works](#11-verification)

---

## 1. Prep the server

SSH into your Hetzner server as root.

```bash
ssh root@YOUR_SERVER_IP
```

### Install Docker

```bash
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker
```

### Create a deploy user

```bash
useradd -m -s /bin/bash deploy
usermod -aG docker deploy

# Create directories the stack needs
mkdir -p /opt/webide
mkdir -p /opt/webide/nginx
mkdir -p /tmp/webide-src
chown deploy:deploy /opt/webide /opt/webide/nginx /tmp/webide-src
chmod 700 /tmp/webide-src

# Note the deploy user's UID/GID — you'll need these in .env
id deploy
# output example: uid=1001(deploy) gid=1001(deploy)
```

### Add your SSH public key for the deploy user

```bash
mkdir -p /home/deploy/.ssh
# Paste your PUBLIC key (the one GitHub Actions will use):
echo "ssh-ed25519 AAAA... your-key-comment" >> /home/deploy/.ssh/authorized_keys
chmod 700 /home/deploy/.ssh
chmod 600 /home/deploy/.ssh/authorized_keys
chown -R deploy:deploy /home/deploy/.ssh
```

---

## 2. GitHub OAuth App

1. Go to **github.com → Settings → Developer settings → OAuth Apps → New OAuth App**
2. Fill in:
   - **Application name:** WebIde
   - **Homepage URL:** `https://YOUR_DOMAIN`
   - **Authorization callback URL:** `https://YOUR_DOMAIN/auth/github/callback`
3. Click **Register application**
4. On the next screen, copy the **Client ID**
5. Click **Generate a new client secret**, copy it immediately (shown once)

Keep these two values — you'll paste them into `.env` in step 4.

---

## 2b. Google OAuth App (optional)

Google login is optional. If you don't want it, leave `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` blank in `.env` and skip this — the GitHub and local-account logins still work.

To enable it:

1. Go to **console.cloud.google.com → APIs & Services → Credentials**
2. Click **Create Credentials → OAuth 2.0 Client ID**
3. Application type: **Web application**
4. Under **Authorized redirect URIs**, add: `https://YOUR_DOMAIN/signin-google`
5. Click **Create**, then copy the **Client ID** and **Client secret**

Paste both into `.env` (`GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET`) in step 4.

---

## 3. Clone the repo onto the server

### Stop and remove whatever is currently running

```bash
su - deploy
# Check what's running
docker ps
docker compose ls

# If there's an existing compose stack, find and stop it:
cd /path/to/old/repo
docker compose down
# Or if you just want to kill all containers:
docker stop $(docker ps -q) 2>/dev/null || true
```

### Clone this repo

```bash
# As the deploy user:
su - deploy
cd /opt/webide
git clone https://github.com/fbeleta/Web_ide.git .
# If the directory already has files from the old repo, do this instead:
# git remote set-url origin https://github.com/fbeleta/Web_ide.git
# git fetch origin
# git reset --hard origin/main
```

> **Note:** If your GitHub repo is private, you'll need to either use a deploy key or authenticate via HTTPS with a personal access token:
> ```bash
> git clone https://YOUR_PAT@github.com/fbeleta/Web_ide.git .
> ```

---

## 4. Configure secrets (.env file)

```bash
# Still as deploy user in /opt/webide
cp .env.template .env
chmod 600 .env
nano .env   # or vim .env
```

Fill in every value. Here's what each one needs:

```bash
# ── App ───────────────────────────────────────────────────────────────────────
ASPNETCORE_ENVIRONMENT=Production
PUBLIC_HOSTNAME=your-actual-domain.com      # e.g. webide.example.com
LETSENCRYPT_EMAIL=your@email.com

# ── Postgres ──────────────────────────────────────────────────────────────────
POSTGRES_USER=webide
POSTGRES_DB=webide
POSTGRES_PASSWORD=                          # generate: openssl rand -base64 32

# ── Redis ─────────────────────────────────────────────────────────────────────
REDIS_PASSWORD=                             # generate: openssl rand -base64 32

# ── GitHub OAuth ──────────────────────────────────────────────────────────────
GITHUB_CLIENT_ID=                           # from step 2
GITHUB_CLIENT_SECRET=                       # from step 2

# ── Google OAuth ──────────────────────────────────────────────────────────────
GOOGLE_CLIENT_ID=                           # from step 2b (optional — leave blank to disable Google login)
GOOGLE_CLIENT_SECRET=                       # from step 2b

# ── Seq ───────────────────────────────────────────────────────────────────────
SEQ_FIRSTRUN_ADMINPASSWORD=                 # a strong password you'll use to log into Seq UI

# ── Worker ───────────────────────────────────────────────────────────────────
WORKER__MAXCONCURRENTSANDBOXES=2
WORKER__SANDBOXMEMMB=512
WORKER__SANDBOXCPUS=0.9

# Sandbox images — leave blank for now; fill after images are built and pushed
SANDBOX_GCC_DIGEST=
SANDBOX_PYTHON_DIGEST=
SANDBOX_NODE_DIGEST=

# ── Deploy ────────────────────────────────────────────────────────────────────
IMAGE_TAG=latest

# ── Host UID/GID — from `id deploy` output in step 1 ─────────────────────────
DEPLOY_UID=1001
DEPLOY_GID=1001
```

Generate passwords in one go:
```bash
echo "POSTGRES_PASSWORD=$(openssl rand -base64 32)"
echo "REDIS_PASSWORD=$(openssl rand -base64 32)"
echo "SEQ_FIRSTRUN_ADMINPASSWORD=$(openssl rand -base64 24)"
```

---

## 5. Generate Seq basic-auth htpasswd

The nginx config puts `/seq/` behind HTTP basic auth. Generate the credentials:

```bash
# Install apache2-utils if not present
apt-get install -y apache2-utils   # run as root

# Generate htpasswd file — you'll be prompted for a password
htpasswd -B -c /opt/webide/nginx/htpasswd ops
# 'ops' is the username; change it if you want
```

> This file is not committed to the repo (it's in `.gitignore`). It lives only on the server.

---

## 6. Get the TLS certificate

The nginx config references Let's Encrypt cert paths. You need to get the first cert before nginx can start with HTTPS.

### Option A — Use Let's Encrypt (recommended for production)

Bring up only nginx on HTTP first so certbot can complete the ACME challenge:

```bash
cd /opt/webide

# Start certbot container to get the cert
docker compose run --rm certbot certonly \
  --webroot -w /var/www/acme \
  -d YOUR_DOMAIN \
  -m "$LETSENCRYPT_EMAIL" \
  --agree-tos --non-interactive

# Update nginx.conf to point at your real domain cert path:
# Change:  ssl_certificate /etc/letsencrypt/live/localhost/fullchain.pem;
# To:      ssl_certificate /etc/letsencrypt/live/YOUR_DOMAIN/fullchain.pem;
# (same for ssl_certificate_key)
nano nginx/nginx.conf
```

> **Alternatively**, if nginx is not yet running, you can generate a self-signed cert first (the `nginx/generate-self-signed.sh` script does this automatically when nginx starts), then swap in the real cert after nginx is up:
> ```bash
> # nginx will auto-generate self-signed cert at startup if none exists
> # After stack is up: docker compose run --rm certbot certonly ...
> # Then: docker compose restart nginx
> ```

### Nginx.conf domain update

Open `nginx/nginx.conf` and replace `localhost` with your actual domain in the cert paths and `server_name`:

```bash
sed -i 's|live/localhost|live/YOUR_DOMAIN|g' nginx/nginx.conf
sed -i 's|server_name _;|server_name YOUR_DOMAIN;|g' nginx/nginx.conf
```

---

## 7. Build the app image

The app image is built locally on the server (or via CI — see step 10). For the first deploy, build it manually:

```bash
cd /opt/webide

# Build the app image (this takes 2-3 minutes — downloads SDK, compiles .NET, runs Tailwind)
docker build -t ghcr.io/fbeleta/web_ide/app:latest -f Dockerfile.app .

# Build sandbox images (needed for code execution — can do later if worker isn't ready)
docker build -t webide-sandbox-python -f sandbox/python.Dockerfile sandbox/
docker build -t webide-sandbox-gcc    -f sandbox/gcc.Dockerfile    sandbox/
docker build -t webide-sandbox-node   -f sandbox/node.Dockerfile   sandbox/
```

> The worker image (`Dockerfile.worker`) requires the `WebIde.Worker` project which is Phase 2. Skip it for now — start the stack without the worker service.

---

## 8. Run database migrations

Run migrations before starting the app so the schema is ready:

```bash
cd /opt/webide

# Run migrations against the postgres container
# (start postgres first if not running)
docker compose up -d postgres
sleep 5   # wait for postgres to be ready

docker run --rm \
  --network webide_webide-net \
  --env-file .env \
  -e ConnectionStrings__WebIdeDb="Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" \
  ghcr.io/fbeleta/web_ide/app:latest \
  dotnet ef database update --project WebIde.DAL --no-build 2>/dev/null \
  || echo "Migrations ran (or already up to date)"
```

> If the above fails because the image doesn't have EF tools, run migrations from the SDK image:
> ```bash
> docker run --rm \
>   --network webide_webide-net \
>   -v /opt/webide:/src \
>   -w /src \
>   mcr.microsoft.com/dotnet/sdk:10.0 \
>   sh -c "dotnet tool install -g dotnet-ef && dotnet ef database update \
>          --project WebIde.DAL --startup-project WebIde.Frontend \
>          --connection 'Host=postgres;Database=webide;Username=webide;Password=YOUR_PASSWORD'"
> ```

---

## 9. Start the stack

```bash
cd /opt/webide

# Start everything except the worker (worker needs Phase 2 image)
docker compose up -d --scale webide-worker=0

# Watch logs
docker compose logs -f webide-app nginx

# Verify health
curl -sk https://localhost/health
# Should return: Healthy

curl -sk https://YOUR_DOMAIN/health
# Should return: Healthy
```

### If nginx fails to start (cert not found)

The `generate-self-signed.sh` script runs automatically in the nginx entrypoint and creates a self-signed cert if none exists. If it's not running, trigger it manually:

```bash
docker compose exec nginx sh /docker-entrypoint.d/00-self-signed.sh
docker compose restart nginx
```

---

## 9b. Grant yourself the Admin role

The app seeds the `Admin` and `Manager` roles on startup, but assigns them to nobody. Until an account has the `Admin` role, every Admin-only feature (problem creation, file uploads, the write API endpoints) stays locked.

1. **Register your account first** — open `https://YOUR_DOMAIN/Identity/Account/Login`, register a local account (or log in with GitHub/Google once so the user row exists).

2. **Assign the Admin role via psql:**

```bash
cd /opt/webide
docker compose exec postgres psql -U webide -d webide
```

Then in the psql prompt (replace the email with the one you registered):

```sql
-- Find your AppUser id
SELECT "Id", "Email" FROM "AspNetUsers";

-- Grant Admin (uses the role + user ids from AspNetRoles / AspNetUsers)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."Email" = 'you@example.com' AND r."Name" = 'Admin';

\q
```

3. **Log out and back in** so the new role is written into your auth cookie.

> Repeat with `r."Name" = 'Manager'` to grant Manager instead of / in addition to Admin.

---

## 10. GitHub Actions CI/CD

So that future pushes to `main` auto-deploy:

### Add GitHub Secrets

In your GitHub repo: **Settings → Secrets and variables → Actions → New repository secret**

| Secret name | Value |
|---|---|
| `HETZNER_HOST` | Your server's IP address |
| `HETZNER_USER` | `deploy` |
| `HETZNER_SSH_KEY` | The **private** SSH key whose public key you added in step 1 |

### Set branch protection on `main`

**Settings → Branches → Add branch protection rule:**
- Branch name pattern: `main`
- ✅ Require a pull request before merging
- ✅ Require status checks to pass before merging
- ✅ Do not allow bypassing the above settings
- ✅ Restrict who can push to matching branches

### Push to trigger CI

```bash
# On your local machine:
git checkout main
git merge develop
git push origin main
```

The deploy workflow (once you create `.github/workflows/deploy.yml` in Phase 5) will:
1. Build and push `app:sha-{SHA}` to `ghcr.io`
2. SSH into the server and pull the new image
3. Run migrations
4. Restart the app container
5. Health check → promote `:latest` on pass, rollback on fail

> Phase 5 (CI/CD workflows) has not been implemented yet. Until then, deploy manually:
> ```bash
> # On server as deploy user:
> cd /opt/webide
> git pull origin main
> docker build -t ghcr.io/fbeleta/web_ide/app:latest -f Dockerfile.app .
> docker compose up -d webide-app
> ```

---

## 11. Verification

Run these checks after the stack is up:

```bash
# Health (liveness)
curl -sk https://YOUR_DOMAIN/health
# → Healthy

# Health (readiness — checks postgres)
curl -sk https://YOUR_DOMAIN/health/ready
# → Healthy  (or Degraded if postgres slow to start)

# App is reachable
curl -sk https://YOUR_DOMAIN | grep CODE_COMPILER
# → should find the page title

# Seq log UI (prompts for basic auth — use the htpasswd credentials from step 5)
open https://YOUR_DOMAIN/seq/

# HTTPS redirect from HTTP
curl -sI http://YOUR_DOMAIN | grep Location
# → Location: https://YOUR_DOMAIN/

# GitHub OAuth — visit in browser:
# https://YOUR_DOMAIN/auth/github/login
# Should redirect to GitHub, ask for permission, then redirect back and log you in
```

### Sandbox security tests (run after sandbox images are built)

```bash
mkdir -p /tmp/test-src
cat > /tmp/test-src/cases.json <<'EOF'
{"timeLimitMs":2000,"floatTolerance":null,"cases":[{"id":1,"stdin":"","expected":"65534","points":1}]}
EOF

# Identity check — should output 65534 (nobody)
echo 'import os; print(os.getuid())' > /tmp/test-src/solution.py
docker run --rm --network none --read-only \
  --tmpfs /tmp:size=64m,mode=1777 \
  --memory 512m --memory-swap 512m --cpus 0.9 \
  --pids-limit 64 --user nobody:nogroup \
  --security-opt no-new-privileges --cap-drop ALL \
  --ulimit fsize=67108864 \
  -v /tmp/test-src:/code:ro \
  webide-sandbox-python /code/solution.py /code/cases.json
# Expected: [{"id":1,"verdict":"Accepted","wallMs":...,"peakKb":...,...}]
```

---

## Quick reference: useful commands on the server

```bash
# Restart the app only
docker compose restart webide-app

# Tail app logs
docker compose logs -f webide-app

# Tail all logs
docker compose logs -f

# Check container status
docker compose ps

# Run a psql shell
docker compose exec postgres psql -U webide -d webide

# Force renew TLS cert
docker compose run --rm certbot renew --force-renewal
docker compose restart nginx

# Pull latest code and rebuild
git pull origin main && docker build -t ghcr.io/fbeleta/web_ide/app:latest -f Dockerfile.app . && docker compose up -d webide-app

# View disk usage
docker system df
```

---

## What still needs doing (human tasks)

| Task | Status | Notes |
|---|---|---|
| Provision Hetzner server | ✅ Done | You have the server |
| Point domain at server | ✅ Done | Domain is set up |
| Install Docker + create deploy user | Step 1 above | |
| Create GitHub OAuth app | Step 2 above | Need Client ID + Secret |
| Clone repo + configure `.env` | Steps 3–4 above | |
| Generate Seq htpasswd | Step 5 above | |
| Get Let's Encrypt cert | Step 6 above | |
| Build app image | Step 7 above | |
| Run migrations | Step 8 above | |
| Start stack | Step 9 above | |
| Add GitHub secrets for CI/CD | Step 10 above | |
| Implement Phase 5 CI/CD workflows | Future work | `.github/workflows/deploy.yml` |
| Implement Phase 2 Worker service | Future work | Code execution pipeline |
| Choose + configure backup offsite target | Future work | Hetzner Storage Box / B2 / S3 |
| Quarterly backup restore drill | Ongoing | See `deployment-handoff.md §11c` |
