#!/bin/sh
# Sandbox wrapper for Node.js — implements §6a judging contract.
#
# Args: $1 = /code/solution.js   $2 = /code/cases.json
#
# No compile step. SyntaxError exits non-zero → RuntimeError (intentional per §5).
# Writes /tmp/result.json; prints it to stdout (capped 16 MB).
# Exit codes: 0=handled, 124=wrapper timeout (set externally by Docker).

set -e

SOLUTION="$1"
CASES_JSON="$2"

STDOUT_CAP=4194304
STDERR_CAP=1048576
WRAPPER_OUT_CAP=16777216

# ── Parse meta ────────────────────────────────────────────────────────────────
TIME_LIMIT_MS=$(jq -r '.timeLimitMs // 2000' "$CASES_JSON")
FLOAT_TOL=$(jq -r '.floatTolerance // empty' "$CASES_JSON")
GRACE_MS=500
TIMEOUT_SEC=$(( (TIME_LIMIT_MS + GRACE_MS + 999) / 1000 ))

# ── Output normalization ──────────────────────────────────────────────────────
normalize() {
  printf '%s' "$1" \
    | sed 's/\r//' \
    | sed 's/[[:space:]]*$//' \
    | awk 'BEGIN{n=0} /^[[:space:]]*$/{n++} /[^[:space:]]/{for(i=0;i<n;i++) print ""; n=0; print}'
}

# ── Float comparison ──────────────────────────────────────────────────────────
# Values passed via process.argv — never interpolated into JS source.
floats_equal() {
  node --input-type=module - "$1" "$2" "$3" <<'JSEOF'
const [,, aRaw, eRaw, tolRaw] = process.argv;
const aToks = aRaw.trim().split(/\s+/);
const eToks = eRaw.trim().split(/\s+/);
const tol = parseFloat(tolRaw);
if (aToks.length !== eToks.length) process.exit(1);
for (let i = 0; i < aToks.length; i++) {
  const av = parseFloat(aToks[i]);
  const ev = parseFloat(eToks[i]);
  if (!isNaN(av) && !isNaN(ev)) {
    const diff = Math.abs(av - ev);
    if (!(diff <= tol || diff <= tol * Math.abs(ev))) process.exit(1);
  } else if (aToks[i] !== eToks[i]) {
    process.exit(1);
  }
}
process.exit(0);
JSEOF
}

# ── Cap helper ────────────────────────────────────────────────────────────────
cap_output() {
  content="$1"
  cap="$2"
  actual_len=${#content}
  if [ "$actual_len" -gt "$cap" ]; then
    elided=$(( actual_len - cap ))
    printf '%s\n[truncated: %d bytes elided]\n' "$(printf '%s' "$content" | head -c "$cap")" "$elided"
  else
    printf '%s' "$content"
  fi
}

# ── JSON escape ───────────────────────────────────────────────────────────────
json_escape() {
  printf '%s' "$1" | jq -Rs '.'
}

# ── Iterate test cases ────────────────────────────────────────────────────────
CASE_COUNT=$(jq '.cases | length' "$CASES_JSON")
RESULTS='[]'

i=0
while [ "$i" -lt "$CASE_COUNT" ]; do
  CASE_ID=$(jq -r ".cases[$i].id" "$CASES_JSON")
  STDIN_DATA=$(jq -r ".cases[$i].stdin" "$CASES_JSON")
  EXPECTED=$(jq -r ".cases[$i].expected" "$CASES_JSON")

  # timeout(1) runs the interpreter directly (nothing in between) so the whole
  # process is killed cleanly on TLE — no orphan can keep the container alive.
  # stdin/stdout/stderr use files, never a live $() pipe.
  printf '%s' "$STDIN_DATA" > /tmp/_stdin_$$
  T_START=$(cut -d' ' -f1 /proc/uptime)
  set +e
  timeout "${TIMEOUT_SEC}s" node "$SOLUTION" </tmp/_stdin_$$ >/tmp/_stdout_$$ 2>/tmp/_stderr_$$
  RUN_EXIT=$?
  set -e
  T_END=$(cut -d' ' -f1 /proc/uptime)
  ACTUAL_STDOUT=$(cat /tmp/_stdout_$$ 2>/dev/null || true)
  ACTUAL_STDERR=$(cat /tmp/_stderr_$$ 2>/dev/null || true)

  # Wall time (ms) from monotonic /proc/uptime; peak RSS (KB) from cgroup v2
  # (memory.peak is container-wide + monotonic; submission peak = max over cases).
  WALL_MS=$(awk -v a="$T_START" -v b="$T_END" 'BEGIN{d=(b-a)*1000; if(d<0)d=0; printf "%d",d}')
  PEAK_BYTES=$(cat /sys/fs/cgroup/memory.peak 2>/dev/null || cat /sys/fs/cgroup/memory/memory.max_usage_in_bytes 2>/dev/null || echo 0)
  PEAK_KB=$(( PEAK_BYTES / 1024 ))

  ACTUAL_STDOUT=$(cap_output "$ACTUAL_STDOUT" "$STDOUT_CAP")
  ACTUAL_STDERR=$(cap_output "$ACTUAL_STDERR" "$STDERR_CAP")

  # Exit-code map (busybox): SIGKILL (9/137) = OOM => MLE; SIGTERM from
  # timeout (143) or GNU timeout (124) => TLE; other nonzero => RuntimeError.
  if [ "$RUN_EXIT" -eq 137 ] || [ "$RUN_EXIT" -eq 9 ]; then
    VERDICT="MemoryLimitExceeded"
  elif [ "$RUN_EXIT" -eq 124 ] || [ "$RUN_EXIT" -eq 143 ]; then
    VERDICT="TimeLimitExceeded"
  elif [ "$RUN_EXIT" -ne 0 ]; then
    VERDICT="RuntimeError"
  else
    ACTUAL_NORM=$(normalize "$ACTUAL_STDOUT")
    EXPECT_NORM=$(normalize "$EXPECTED")
    if [ -n "$FLOAT_TOL" ]; then
      if floats_equal "$ACTUAL_NORM" "$EXPECT_NORM" "$FLOAT_TOL"; then
        VERDICT="Accepted"
      else
        VERDICT="WrongAnswer"
      fi
    else
      if [ "$ACTUAL_NORM" = "$EXPECT_NORM" ]; then
        VERDICT="Accepted"
      else
        VERDICT="WrongAnswer"
      fi
    fi
  fi

  STDOUT_JSON=$(json_escape "$ACTUAL_STDOUT")
  STDERR_JSON=$(json_escape "$ACTUAL_STDERR")

  CASE_RESULT=$(printf '{"id":%s,"verdict":"%s","wallMs":%s,"peakKb":%s,"stdout":%s,"stderr":%s}' \
    "$CASE_ID" "$VERDICT" "$WALL_MS" "$PEAK_KB" "$STDOUT_JSON" "$STDERR_JSON")

  RESULTS=$(printf '%s' "$RESULTS" | jq --argjson c "$CASE_RESULT" '. + [$c]')

  rm -f /tmp/_stdin_$$ /tmp/_stdout_$$ /tmp/_stderr_$$
  i=$(( i + 1 ))
done

printf '%s' "$RESULTS" > /tmp/result.json

RESULT_SIZE=$(wc -c < /tmp/result.json)
if [ "$RESULT_SIZE" -gt "$WRAPPER_OUT_CAP" ]; then
  head -c "$WRAPPER_OUT_CAP" /tmp/result.json
else
  cat /tmp/result.json
fi

exit 0
