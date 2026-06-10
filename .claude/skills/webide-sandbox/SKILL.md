---
name: webide-sandbox
description: Use this skill when writing or modifying any sandbox-related file in WebIde — sandbox Dockerfiles (sandbox/*.Dockerfile), wrapper scripts (sandbox/*.sh), SandboxOrchestrator, or SubmissionEvaluator. It encodes the §6 security flags (which are non-negotiable) and the §6a judging contract (which has subtle traps that produce wrong-answer disputes).
---

# WebIde Sandbox & Judging

Reference: §6 and §6a of `docs/deployment-handoff.md`.

## §6 docker run flags are exact

Every sandbox `docker run` MUST use these flags. If you want to change any of them, stop and ask:

- `--network none` — zero network access
- `--read-only` + `--tmpfs /tmp:size=64m,mode=1777` — only `/tmp` writable
- `--memory $MEM` + `--memory-swap $MEM` — equal → disable swap (no swap escape)
- `--cpus 0.9`
- `--pids-limit 64` — **NOT** `--ulimit nproc`. `nproc` is a per-UID rlimit at the host level, aggregates across containers; `pids-limit` is cgroup-enforced per-container.
- `--user nobody:nogroup` — non-root inside the container. Defense in depth.
- `--security-opt no-new-privileges`
- `--cap-drop ALL`
- `--ulimit fsize=67108864` — 64 MB max file
- Image pinned by `@sha256:<digest>`, NEVER `:latest`
- Source mount: `-v /tmp/webide-src/{id}:/code:ro` (read-only)

## §6a judging contract — traps that produce WRONG_ANSWER disputes

### Output normalization (apply in this exact order)
1. CRLF → LF
2. Strip trailing whitespace on each line
3. Strip trailing blank lines
4. **Preserve internal whitespace** — do NOT collapse runs of spaces

### Float comparison
- `Problem.FloatTolerance` is nullable. If null → exact string match after normalization.
- If non-null → tokenize on whitespace, lengths must match, each pair compared as `|a-b| ≤ tol OR |a-b| ≤ tol × |b|` (combined absolute + relative).

### Per-case verdict precedence
`MemoryLimitExceeded > TimeLimitExceeded > RuntimeError > WrongAnswer > Accepted`

### Submission-level
- `Status` = worst per-case verdict by the precedence above. `Accepted` only if every case is.
- `Score` = Σ `TestCase.Points` over Accepted cases (partial credit).

### Stdout/stderr caps
- Per case: 4 MB stdout, 1 MB stderr. Truncate with marker `\n[truncated: N bytes elided]\n`.
- Wrapper-to-worker JSON: 16 MB max.
- DB persistence: 64 KB per field.

### Wall time
- Measured by the wrapper around each user-process invocation (`clock_gettime CLOCK_MONOTONIC`).
- NOT around `docker run` — that includes 100-500ms of container startup overhead.

### Peak memory
- `getrusage(RUSAGE_CHILDREN).ru_maxrss` per subprocess (KB on Linux).
- Or cgroup `memory.peak` for multi-process programs.
- NOT `InspectContainerAsync` — it doesn't return memory stats.

## Exit code mapping

| Lang | Exit 0 | Exit 1 | Exit 2 | Exit 124 |
|---|---|---|---|---|
| C/C++ | Ran (check per-case verdicts) | RuntimeError | **CompileError** | Wrapper-side timeout |
| Python | Ran | **RuntimeError** (incl. SyntaxError) | unused | Wrapper-side timeout |
| Node | Ran | **RuntimeError** (incl. parse error) | unused | Wrapper-side timeout |

Python/Node syntax errors map to `RuntimeError`, **not** `CompileError`. This is intentional — they have no separate compile phase. Surface stderr so users see the actual error message.

## Common bash wrapper mistakes

- Forgetting `set -euo pipefail`. Wrappers must be strict.
- Reading stdin with `read` loses binary and multi-line — pipe the test case stdin directly into the subprocess.
- Python buffers stdout: invoke with `python -u` or `PYTHONUNBUFFERED=1`, otherwise output gets lost when the process is killed for TLE.
- `head -c 4M` does NOT exist in busybox. Use `head -c 4194304`.
- Alpine `adduser` syntax differs from Debian: `adduser -D -H -u 65534 nobody 2>/dev/null || true && addgroup -S nogroup 2>/dev/null || true`.
- `timeout` on busybox does not have `--kill-after`. Use `timeout -s KILL <seconds>` or layer two `timeout` calls.

## Sandbox image hygiene

- Dockerfile final stage: `USER nobody:nogroup` AFTER copying the wrapper script.
- Wrapper scripts owned by root, mode 555 (readable, executable, not writable).
- `/code` is mounted read-only — wrapper output goes to `/tmp/result.json`.
- Pin base images by digest in the Dockerfile (`FROM python:3.12-alpine@sha256:...`).

## Verify after every sandbox change

Run the §13b adversarial tests:
- Fork bomb → container dies, host unaffected
- Memory bomb → MLE
- FS write outside `/tmp` → PermissionError
- Outbound network → connection refused
- `os.getuid()` → 65534, not 0
- 100 MB stdout → truncated with marker, no host OOM

All six must produce the expected failure mode before merging.
