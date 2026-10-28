#!/usr/bin/env bash
# NIGHT20 close (U3-34.12): the final build's Player checks in order, each Player closed by its PID after a timeout
# (CLAUDE.md rule 10): the smoke alone (smoke.sh final), -skygrindGolden, -skygrindFingerprint (both lines),
# -skygrindReplay on the smoke's own tape, then the bench x2 (bench.sh). Nothing else may run meanwhile.
#   bash close.sh <buildDir>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
ME=/c/Users/gaige/dev/SKYGRIND-unity/Tools/night
export BDIR="${1:-Builds/Stage1}"
bash $ME/smoke.sh final
tape=$(grep -m1 "\[smoke\] tape path" Logs/player/${NIGHT:-n21}-smoke-final.log | sed 's/.*tape path //' | tr -d '\r')
if [ -n "$tape" ] && [ -f "$tape" ]; then cp "$tape" "Logs/player/${NIGHT:-n21}-smoke-final.tape"; tape="Logs/player/${NIGHT:-n21}-smoke-final.tape"; fi # kept aside at once: later runs prune the tape folder
run() { # <name> <timeout s> <args...>
  local name="$1" to="$2"; shift 2
  local log="Logs/player/${NIGHT:-n21}-final-$name.log"
  echo "$(date +%H:%M:%S) start $name"
  "$BDIR"/SKYGRIND.exe "$@" -batchmode -nographics -logFile "$log" &
  local pid=$! t=0
  while kill -0 $pid 2>/dev/null; do
    sleep 2; t=$((t+2))
    if [ $t -ge "$to" ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
  done
  echo "$(date +%H:%M:%S) end $name ($t s)"
  grep -E "^\[(golden|fingerprint|replay)\]" "$log" | grep -vE "step [0-9]+ hash" | head -8
}
run Golden 300 -skygrindGolden
run Fingerprint 300 -skygrindFingerprint
if [ -n "$tape" ]; then run Replay 300 -skygrindReplay "$tape"; else echo "no tape path in the final smoke log"; fi
bash $ME/bench.sh final-1
bash $ME/bench.sh final-2
echo "close checks done $(date +%H:%M:%S)"
