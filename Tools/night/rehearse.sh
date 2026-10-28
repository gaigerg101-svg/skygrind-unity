#!/usr/bin/env bash
# NIGHT20 round 5 (before the close): the frozen code's Player checks without the bench: the smoke, -skygrindGolden,
# -skygrindFingerprint (both lines, after FingerprintBaselineN20), -skygrindReplay on the smoke's tape (straight after it).
#   bash rehearse.sh <buildDir> <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
ME=/c/Users/gaige/dev/SKYGRIND-unity/Tools/night
export BDIR="$1"; tag="$2"
bash $ME/smoke.sh "$tag"
run() { # <name> <timeout s> <args...>
  local name="$1" to="$2"; shift 2
  local log="Logs/player/${NIGHT:-n21}-$tag-$name.log"
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
tape=$(grep -m1 "\[smoke\] tape path" "Logs/player/${NIGHT:-n21}-smoke-$tag.log" | sed 's/.*tape path //' | tr -d '\r')
if [ -n "$tape" ] && [ -f "$tape" ]; then cp "$tape" "Logs/player/${NIGHT:-n21}-smoke-$tag.tape"; tape="Logs/player/${NIGHT:-n21}-smoke-$tag.tape"; fi # kept aside: later runs prune the tape folder
if [ -n "$tape" ]; then run Replay 300 -skygrindReplay "$tape"; else echo "no tape path in the smoke log"; fi
run Golden 300 -skygrindGolden
run Fingerprint 300 -skygrindFingerprint
echo "rehearsal $tag done $(date +%H:%M:%S)"
