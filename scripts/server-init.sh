#!/usr/bin/env bash
# Runs on the server after pulling a fresh branch.
# Does three things:
#   1. Builds sandbox images and writes their names into .env
#   2. Wipes the database and runs all EF migrations from scratch
#   3. Starts (or restarts) the full Docker Compose stack
#
# Usage (from /opt/webide):
#   bash scripts/server-init.sh
#
# Requires:
#   - .env exists and contains WEBIDE_DB_PASSWORD
#   - docker and docker compose are installed
#   - Port 443 is already set up (nginx / certbot)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(dirname "$SCRIPT_DIR")"
cd "$ROOT"

# ── Colours ───────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
step()  { echo -e "${GREEN}==> $*${NC}"; }
warn()  { echo -e "${YELLOW}[warn] $*${NC}"; }
die()   { echo -e "${RED}[error] $*${NC}"; exit 1; }

# ── Guard: .env must exist ────────────────────────────────────────────────────
[[ -f .env ]] || die ".env not found. Copy .env.template and fill in secrets first."
# shellcheck disable=SC1091
set -a; source .env; set +a
[[ -n "${WEBIDE_DB_PASSWORD:-}" ]] || die "WEBIDE_DB_PASSWORD is not set in .env"

# ── Step 1: Build sandbox images ──────────────────────────────────────────────
step "Building sandbox images..."
docker build -f sandbox/gcc.Dockerfile    -t webide-sandbox-gcc    sandbox/
docker build -f sandbox/python.Dockerfile -t webide-sandbox-python sandbox/
docker build -f sandbox/node.Dockerfile   -t webide-sandbox-node   sandbox/

step "Writing sandbox image names into .env..."
# sed -i on Linux; macOS needs an empty string after -i (handled by falling back)
_sed() { sed -i "$@" 2>/dev/null || sed -i '' "$@"; }
_sed "s|^SANDBOX_GCC_DIGEST=.*|SANDBOX_GCC_DIGEST=webide-sandbox-gcc|"    .env
_sed "s|^SANDBOX_PYTHON_DIGEST=.*|SANDBOX_PYTHON_DIGEST=webide-sandbox-python|" .env
_sed "s|^SANDBOX_NODE_DIGEST=.*|SANDBOX_NODE_DIGEST=webide-sandbox-node|"  .env

# ── Step 2: Wipe DB and remigrate ─────────────────────────────────────────────
step "Stopping stack and wiping volumes (all data will be erased)..."
docker compose down -v

step "Starting postgres and redis..."
docker compose up -d postgres redis

step "Waiting for postgres to accept connections..."
RETRIES=30
until docker compose exec -T postgres pg_isready -U webide -d webide -q 2>/dev/null; do
  RETRIES=$((RETRIES - 1))
  [[ $RETRIES -gt 0 ]] || die "Postgres did not become ready in time."
  sleep 2
done
echo "  postgres is ready."

step "Running EF migrations..."
CONN_STR="Host=postgres;Port=5432;Database=webide;Username=webide;Password=${WEBIDE_DB_PASSWORD}"
# Determine the docker network name (project name prefix varies by compose version)
NETWORK=$(docker network ls --format '{{.Name}}' | grep -E 'webide.webide-net|webide_webide-net' | head -1)
[[ -n "$NETWORK" ]] || die "Could not find webide network. Is docker compose up for postgres/redis?"

docker run --rm \
  --network "$NETWORK" \
  -v "$(pwd):/src" -w /src \
  -e "ConnectionStrings__WebIdeDb=${CONN_STR}" \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  bash -c "dotnet tool restore && dotnet ef database update \
    --project WebIde.DAL/WebIde.DAL.csproj \
    --startup-project WebIde.Frontend/WebIde.Frontend.csproj \
    --no-build"

# ── Step 3: Start full stack ──────────────────────────────────────────────────
step "Starting full stack..."
docker compose up -d

step "Done!"
echo ""
echo "  Stack is up. Useful commands:"
echo "    docker compose logs webide-app --tail 50"
echo "    docker compose logs webide-worker --tail 50"
echo "    docker compose ps"
