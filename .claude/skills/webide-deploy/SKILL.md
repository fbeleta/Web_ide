---
name: webide-deploy
description: Use this skill when working in .github/workflows/ (ci.yml, deploy.yml, sandbox.yml) or anything touching the deploy pipeline, EF migrations, blue-green rollback, or registry tagging. It encodes the §8 blue-green correctness fix (the :latest gotcha) and the §10a migration discipline from docs/deployment-handoff.md.
---

# WebIde Deploy Pipeline

Reference: §8 and §10a of `docs/deployment-handoff.md`.

## The `:latest` gotcha (don't reintroduce this bug)

The naive pattern — push `:sha-X` AND `:latest`, then deploy, then rollback to `:latest` on failure — is **broken**. If the deploy fails, `:latest` was just overwritten with the failing image, so rollback restores the failure.

**Correct sequence:**

```
1. Capture rollback target FIRST (before any push):
   PREV_DIGEST=$(docker buildx imagetools inspect \
     ghcr.io/.../app:latest \
     --format '{{json .Manifest.Digest}}')

2. Build and push :sha-X ONLY. Do NOT push :latest.

3. SSH deploy: pull :sha-X, run migrations, compose up :sha-X.

4. Poll https://localhost/health/ready (NOT /health) for up to 60s.

5. On success:
   - Promote :sha-X to :latest in the registry:
     docker buildx imagetools create \
       --tag ghcr.io/.../app:latest \
       ghcr.io/.../app:sha-X
   - docker image prune -f on the server.

6. On failure:
   IMAGE_TAG=$PREV_DIGEST docker compose up -d
   # Rolls back to the digest captured in step 1.
```

If you find yourself writing "push `:latest`" before step 5, stop. That's the bug.

## Poll `/health/ready`, NOT `/health`

`/health` is liveness — 200 as soon as the process is up. It doesn't catch DB/Redis/worker problems. Use `/health/ready` for deploy verification.

## Migration discipline (§10a)

Migrations run BEFORE the new app container starts. Asymmetric window: if migration succeeds and the app health check fails, rollback brings back code that doesn't know the new schema.

**Rules:**
1. **Additive only per deploy.** Add nullable columns. Never rename or drop a column in the same deploy as the code that uses it. Two-deploy pattern for destructive changes:
   - Deploy A: add new column nullable; code writes to both old and new.
   - Deploy B: switch reads to new; stop writing to old.
   - Deploy C: drop old column.
2. **No `[Required]`** on new properties without server default + data backfill.
3. **No down migrations.** Rollback after a migrated deploy = restore Postgres backup (§11c) + redeploy previous image. Manual; documented in a runbook.

Code review responsibility: flag any migration that renames/drops in the PR that adds it.

## SSH deploy step requirements

Using `appleboy/ssh-action`:

```yaml
- uses: appleboy/ssh-action@v1
  with:
    host: ${{ secrets.HETZNER_HOST }}
    username: ${{ secrets.HETZNER_USER }}
    key: ${{ secrets.HETZNER_SSH_KEY }}
    envs: IMAGE_TAG,PREV_DIGEST
    script: |
      set -euo pipefail
      cd /opt/webide
      IMAGE_TAG=$IMAGE_TAG docker compose pull webide-app webide-worker

      # Migration container must:
      #  - use --env-file .env (DB creds)
      #  - use --network webide_webide-net (reach postgres by hostname)
      #  - use --no-build (image was pulled, don't try to rebuild)
      docker run --rm \
        --env-file .env \
        --network webide_webide-net \
        ghcr.io/.../app:$IMAGE_TAG \
        dotnet ef database update --no-build

      IMAGE_TAG=$IMAGE_TAG docker compose up -d webide-app webide-worker

      # Health check
      for i in $(seq 1 30); do
        if curl -fsS -k https://localhost/health/ready; then exit 0; fi
        sleep 2
      done

      # Rollback
      IMAGE_TAG=$PREV_DIGEST docker compose up -d webide-app webide-worker
      exit 1
```

## Sandbox image pipeline is separate

`.github/workflows/sandbox.yml` triggers on push to `main` where `sandbox/**` changed. It builds and pushes sandbox images with `:sha-X` tags, then outputs digests as workflow artifacts. The deploy workflow consumes these via `SANDBOX_*_DIGEST` env vars (see §9a of the handoff doc).

Sandbox images are NOT rebuilt on every app deploy. They change rarely (language version updates) and changing them should be deliberate.

## Required secrets

In repo settings (§12a of the handoff doc):
- `HETZNER_HOST` — server IP
- `HETZNER_USER` — SSH user (`deploy`)
- `HETZNER_SSH_KEY` — Ed25519 private key

`GITHUB_TOKEN` is built-in. Ensure workflow permissions include `packages: write` for ghcr.io push.

## Branch protection

CI must be a required check on `main`. This is configured in repo settings UI, NOT in YAML — easy to forget. Verify before going live, otherwise anyone can merge a broken build.

## CI gate (`ci.yml`)

Triggers: PR to `main`, push to `develop`. Steps:

```
checkout → setup-dotnet 10.x → dotnet restore → dotnet build → dotnet test
```

`dotnet test` passes vacuously today (no test project exists). It's still required so the gate is in place when tests land.

## Don't add features the spec doesn't ask for

The deploy pipeline is fragile by nature. Stick to §8 exactly. If you want to add Slack notifications, smoke tests, multi-region deploys, or anything else — propose it in a separate PR with the user's sign-off. Production deploy code is high-blast-radius.
