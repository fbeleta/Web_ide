#!/bin/sh
# Sandbox wrapper for C/C++ — implements §6a judging contract.
#
# Args: $1 = /code/solution.{c,cpp}   $2 = /code/cases.json
#
# cases.json format:
#   { "timeLimitMs": 2000, "floatTolerance": null,
#     "cases": [{ "id": 1, "stdin": "...", "expected": "...", "points": 10 }] }
#
# Writes /tmp/result.json; prints it to stdout (capped 16 MB).
# Exit codes: 0=handled, 2=compile error, 124=wrapper timeout (set externally).

set -e

SOLUTION="$1"
CASES_JSON="$2"

STDOUT_CAP=4194304      # 4 MB per case
STDERR_CAP=1048576      # 1 MB per case
WRAPPER_OUT_CAP=16777216  # 16 MB total JSON output

TRUNCATE_MARKER_STDOUT="[truncated: %d bytes elided]"
TRUNCATE_MARKER_STDERR="[truncated: %d bytes elided]"

# ── Parse meta from cases.json ────────────────────────────────────────────────
TIME_LIMIT_MS=$(jq -r '.timeLimitMs // 2000' "$CASES_JSON")
FLOAT_TOL=$(jq -r '.floatTolerance // empty' "$CASES_JSON")  # empty string if null
GRACE_MS=500
TIMEOUT_SEC=$(( (TIME_LIMIT_MS + GRACE_MS + 999) / 1000 ))

# ── Detect language and compile ───────────────────────────────────────────────
case "$SOLUTION" in
  *.cpp) COMPILER="g++" ;;
  *.c)   COMPILER="gcc" ;;
  *) echo "Unknown extension: $SOLUTION" >&2; exit 1 ;;
esac

COMPILE_STDERR=$(/usr/bin/time -v "$COMPILER" -O2 -o /tmp/a.out "$SOLUTION" 2>&1) || {
  # Compile failed — exit 2 per spec
  echo "$COMPILE_STDERR" >&2
  exit 2
}

# ── Output normalization ──────────────────────────────────────────────────────
# Applied to both actual and expected before comparison.
# 1) CRLF → LF  2) trailing whitespace per line  3) trailing blank lines
normalize() {
  printf '%s' "$1" \
    | sed 's/\r//' \
    | sed 's/[[:space:]]*$//' \
    | awk 'BEGIN{n=0} /^[[:space:]]*$/{n++} /[^[:space:]]/{for(i=0;i<n;i++) print ""; n=0; print}'
}

# ── Float comparison ──────────────────────────────────────────────────────────
# Returns 0 if equal within tolerance, 1 if not.
# Values are passed as positional argv — never interpolated into Python source.
floats_equal() {
  python3 - "$1" "$2" "$3" <<'PYEOF'
import sys
a_toks = sys.argv[1].split()
e_toks = sys.argv[2].split()
tol = float(sys.argv[3])
if len(a_toks) != len(e_toks):
    sys.exit(1)
for a, e in zip(a_toks, e_toks):
    try:
        av, ev = float(a), float(e)
        diff = abs(av - ev)
        if not (diff <= tol or diff <= tol * abs(ev)):
            sys.exit(1)
    except ValueError:
        if a != e:
            sys.exit(1)
sys.exit(0)
PYEOF
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

# ── JSON escape helper ────────────────────────────────────────────────────────
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

  # Wall-time start (milliseconds via CLOCK_MONOTONIC via date +%s%3N)
  T_START=$(date +%s%3N)

  # Run binary with timeout; capture stdout and stderr separately
  ACTUAL_STDOUT=$( \
    printf '%s' "$STDIN_DATA" \
    | timeout "${TIMEOUT_SEC}s" /usr/bin/time -v /tmp/a.out 2>/tmp/_stderr_$$ \
    ; true
  )
  RUN_EXIT=$?
  ACTUAL_STDERR=$(cat /tmp/_stderr_$$ 2>/dev/null || true)

  T_END=$(date +%s%3N)
  WALL_MS=$(( T_END - T_START ))

  # Parse peak memory from /usr/bin/time -v output (in KB on Linux)
  PEAK_KB=$(printf '%s' "$ACTUAL_STDERR" | grep 'Maximum resident set size' | awk '{print $NF}')
  PEAK_KB=${PEAK_KB:-0}
  # Strip /usr/bin/time output from stderr so user only sees program stderr
  ACTUAL_STDERR=$(printf '%s' "$ACTUAL_STDERR" | grep -v 'Command being timed\|wall clock\|Maximum resident\|Major.*page\|Minor.*page\|Voluntary\|Involuntary\|Swaps\|File system\|Socket\|Signals\|Page size\|Percent of CPU\|Elapsed.*wall\|Maximum.*kilobytes\|Average.*kilobytes\|Average.*shared\|Average.*unshared\|Average.*stack\|Average.*total\|Exit status' || true)

  # ── Cap stdout/stderr ──────────────────────────────────────────────────────
  ACTUAL_STDOUT=$(cap_output "$ACTUAL_STDOUT" "$STDOUT_CAP")
  ACTUAL_STDERR=$(cap_output "$ACTUAL_STDERR" "$STDERR_CAP")

  # ── Determine verdict ──────────────────────────────────────────────────────
  # Precedence: MLE > TLE > RuntimeError > WrongAnswer > Accepted

  # MLE: peak memory exceeds limit (Docker enforces via cgroup; wrapper records it)
  # The --memory flag kills the container via OOM; if we reach here, check peak vs limit
  # (Docker's OOM killer exits 137; we treat that as MLE)
  if [ "$RUN_EXIT" -eq 137 ]; then
    VERDICT="MemoryLimitExceeded"
  elif [ "$RUN_EXIT" -eq 124 ]; then
    # timeout(1) exits 124 when it kills the process
    VERDICT="TimeLimitExceeded"
  elif [ "$RUN_EXIT" -ne 0 ]; then
    VERDICT="RuntimeError"
  else
    # Compare output (normalization + optional float tolerance)
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

  # ── Build per-case JSON ────────────────────────────────────────────────────
  STDOUT_JSON=$(json_escape "$ACTUAL_STDOUT")
  STDERR_JSON=$(json_escape "$ACTUAL_STDERR")

  CASE_RESULT=$(printf '{"id":%s,"verdict":"%s","wallMs":%s,"peakKb":%s,"stdout":%s,"stderr":%s}' \
    "$CASE_ID" "$VERDICT" "$WALL_MS" "$PEAK_KB" "$STDOUT_JSON" "$STDERR_JSON")

  RESULTS=$(printf '%s' "$RESULTS" | jq --argjson c "$CASE_RESULT" '. + [$c]')

  rm -f /tmp/_stderr_$$
  i=$(( i + 1 ))
done

# ── Write and emit result.json ─────────────────────────────────────────────────
printf '%s' "$RESULTS" > /tmp/result.json

# Cap total wrapper output at 16 MB
RESULT_SIZE=$(wc -c < /tmp/result.json)
if [ "$RESULT_SIZE" -gt "$WRAPPER_OUT_CAP" ]; then
  head -c "$WRAPPER_OUT_CAP" /tmp/result.json
else
  cat /tmp/result.json
fi

exit 0
