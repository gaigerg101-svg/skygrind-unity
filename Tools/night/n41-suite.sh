#!/usr/bin/env bash
# NIGHT41 (the lead; INVENTORY A6): the whole EditMode suite IN the live Editor, in batches (no second Unity process; the batch-mode suite
# stopped for low memory in NIGHT39 and NIGHT40). The run_tests filter matches a test's full name by substring and takes one string, so
# a batch is a name prefix: each namespace (Skygrind.Core.Tests, Skygrind.Game.Tests) by the first letter of its classes, each call under
# the tool's 600 s timeout. A batch that reports no JSON or fails with an empty message is re-run once alone. Never in Play mode, never
# beside a build or a reel (one heavy job at a time).
# 2026-10-21 (after NIGHT56): when the CLI's 600 s timeout fires, the Editor keeps running that batch, and every later call queues
# behind it: Game M has never reported since NIGHT41, and N / P / R / S timed out behind it (20 min each, NIGHT56 close). So the
# runner now waits for the Editor to answer editor_status before each batch, never re-runs a batch that timed out (it waits out the
# leftover instead), prints each batch's seconds, and can run one class per batch ("class") to find the slow one.
#   bash Tools/night/n41-suite.sh <tag> [Core|Game] [letters] [letter|class]
#   e.g.  bash Tools/night/n41-suite.sh freeze   |   ... r2 Game NOPQ   |   ... split Game MNPRSTUVWZ class
# Output: Logs/n41-suite-<tag>.txt (one line per batch + every failure), and the totals at the end.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; only="${2:-}"; letters="${3:-ABCDEFGHIJKLMNOPQRSTUVWXYZ}"; by="${4:-letter}"
out="Logs/n41-suite-$tag.txt"
echo "== n41-suite $tag start $(date +%T) (only=${only:-all} letters=$letters by=$by)" >> "$out"
P=0; T=0; F=0
drain() { # wait (up to 60 min) until the Editor answers editor_status, i.e. no test run is holding its main thread
  local t0=$SECONDS
  for _ in $(seq 1 120); do
    if timeout 45 unity command editor_status --caller plugin --skill unity-cli --no-banner --format json --result-only 2>&1 | grep -q '"playMode"'; then
      [ $((SECONDS - t0)) -gt 5 ] && echo "   (waited $((SECONDS - t0)) s for the Editor to go idle)" >> "$out"
      return 0
    fi
    sleep 30
  done
  echo "   (the Editor did not go idle in 60 min)" >> "$out"; return 1
}
run() { # $1 = filter prefix
  drain || return
  local t0=$SECONDS r; r=$(bash Tools/unity-loop.sh tests "$1" 2>&1)
  if grep -q "command 'run_tests' timed out" Logs/tests-last.json 2>/dev/null; then # the CLI's own error, not a test's message
    echo "$(date +%T) $1: TIMEOUT after $((SECONDS - t0)) s (not re-run; the Editor keeps running it)" >> "$out"
    echo "   (no totals for $1: not counted)" >> "$out"; return
  fi
  if echo "$r" | grep -q "no JSON" || { echo "$r" | grep -q " FAIL " && ! echo "$r" | grep -q " FAIL .*: ."; }; then
    echo "   retry $1 (no JSON / empty failure message)" >> "$out"; drain || return; r=$(bash Tools/unity-loop.sh tests "$1" 2>&1)
  fi
  local line; line=$(echo "$r" | grep "^tests:")
  echo "$(date +%T) $1 ($((SECONDS - t0)) s): ${line:-$(echo "$r" | head -c 300)}" >> "$out"
  echo "$r" | grep " FAIL " >> "$out"
  local p t f; p=$(echo "$line" | awk '{print $2}'); t=$(echo "$line" | awk '{print $4}'); f=$(echo "$line" | awk '{print $6}')
  case "$p$t$f" in *[!0-9]*|"") echo "   (no totals for $1: not counted)" >> "$out"; return ;; esac # NIGHT41: a no-JSON line ("no JSON: {") broke the sums and ended the loop
  [ "$t" = 0 ] && echo "   (0 tests matched $1: check the class name)" >> "$out"
  P=$((P + p)); T=$((T + t)); F=$((F + f))
}
for ns in Core Game; do
  [ -n "$only" ] && [ "$only" != "$ns" ] && continue
  dir="Assets/Tests/EditMode"; [ "$ns" = Game ] && dir="Assets/Tests/EditMode/Game"
  present=$(ls "$dir"/*.cs 2>/dev/null | sed 's#.*/##' | cut -c1 | sort -u | tr -d '\n')
  for ((i = 0; i < ${#letters}; i++)); do
    L="${letters:$i:1}"
    case "$present" in *"$L"*) ;; *) continue ;; esac
    if [ "$by" = class ]; then # one batch per test file; the trailing dot keeps the filter to that one class
      for f in "$dir/$L"*.cs; do run "Skygrind.$ns.Tests.$(basename "$f" .cs)."; done
    else
      run "Skygrind.$ns.Tests.$L"
    fi
  done
done
echo "== n41-suite $tag end $(date +%T): $P / $T passed, $F failed" >> "$out"
tail -1 "$out"
