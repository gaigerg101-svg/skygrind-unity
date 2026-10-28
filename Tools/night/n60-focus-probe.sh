#!/usr/bin/env bash
# NIGHT60 builder L (B9, T-138: the focus pause outside the smoke). One Player run of the focus probe (FocusProbe60.cs) with ONE planted
# focus steal that really takes the foreground (Tools/night/n56-foreground.ps1, L-099):
#   1. starts Builds/<tag>/SKYGRIND.exe -skygrindFocusProbe60 [extra args] windowed, with its own -logFile
#   2. brings the Player's window to the front (Tools/night/n60-focus-player.ps1 by its Windows PID): the probe's ready line needs it focused
#   3. waits STEAL_AFTER s (default 7; the log's ready line arrives too late to time it: see below), then the steal (a window of the
#      script's own, held 3 s, closed)
#   4. waits for "[focus60] DONE" (the probe quits UnityTuning.FocusProbe60Seconds after ready), closes the Player by its PID if it is still
#      up (CLAUDE.md rule 10: the process name checked first), and prints the [focus60] lines and the steal's lines
#   bash Tools/night/n60-focus-probe.sh <tag> <side> [extra Player args ...]
#   GREEN: bash Tools/night/n60-focus-probe.sh n60-b9 green                                  -> "[focus60] DONE: paused on the focus loss True"
#   RED:   bash Tools/night/n60-focus-probe.sh n60-b9 red -skygrindFocusProbe60NoPause       -> "... False" (the planted probe rides through)
# Output: Logs/player/n60-focus-<tag>-<side>.log (the Player log), Logs/n60/b9/focus-<tag>-<side>.txt (this script's lines). Run it with no
# other window stealing the focus (memory: player-smoke-focus-pause) and one Player at a time.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; side="${2:?side}"; shift 2
exe="Builds/$tag/SKYGRIND.exe"
[ -f "$exe" ] || { echo "n60-focus-probe: no $exe"; exit 1; }
mkdir -p Logs/player Logs/n60/b9
log="Logs/player/n60-focus-$tag-$side.log"
out="Logs/n60/b9/focus-$tag-$side.txt"
rm -f "$log"
"$exe" -skygrindFocusProbe60 "$@" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$log" &
pid=$!
sleep 1
winpid=$(cat "/proc/$pid/winpid" 2>/dev/null)
echo "$(date +%T) n60-focus-probe $tag $side: $exe -skygrindFocusProbe60 $* (bash pid $pid, windows pid $winpid) -> $log" | tee "$out"
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/night/n60-focus-player.ps1 -WinPid "$winpid" 2>&1 | tee -a "$out"
# NIGHT60 lead (05:58): the steal comes STEAL_AFTER s (default 7) after the focus step, not after the log's ready line. The Player's log
# reaches the file late (the ready line showed 6-14 s after the run's start, which the tape's file name dates), so a steal timed off the log
# fell after the probe's 20 s window in three runs on n60-b8 (Logs/n60/b9/focus-n60-b8-{on,off,planted}.txt: "focus losses 0"); the run
# starts 1-4 s after the launch and the steal takes ~3 s to land, so 7 s lands inside the window. A steal that still misses prints MISSED below.
sleep "${STEAL_AFTER:-7}"
if kill -0 "$pid" 2>/dev/null; then
  echo "$(date +%T) the steal (n56-foreground.ps1 -HoldS 3), ${STEAL_AFTER:-7} s after the focus step:" | tee -a "$out"
  powershell -NoProfile -ExecutionPolicy Bypass -File Tools/night/n56-foreground.ps1 -HoldS 3 2>&1 | tee -a "$out"
else
  echo "$(date +%T) the Player exited before the steal: no steal planted" | tee -a "$out"
fi
t=0
until grep -q "^\[focus60\] DONE" "$log" 2>/dev/null; do
  kill -0 "$pid" 2>/dev/null || break
  [ "$t" -ge 60 ] && break
  sleep 1; t=$((t + 1))
done
sleep 2
if kill -0 "$pid" 2>/dev/null; then
  name=$(tasklist //FI "PID eq $winpid" //FO CSV //NH 2>/dev/null | head -1 | cut -d, -f1 | tr -d '"')
  if [ "$name" = "SKYGRIND.exe" ]; then
    taskkill //PID "$winpid" //F > /dev/null 2>&1
    echo "$(date +%T) closed $name (windows pid $winpid)" | tee -a "$out"
  else
    echo "$(date +%T) windows pid $winpid is '$name', not SKYGRIND.exe: not closed" | tee -a "$out"
  fi
fi
echo "-- the Player's [focus60] lines:" | tee -a "$out"
grep -E "^\[focus60\]" "$log" 2>/dev/null | tee -a "$out"
grep -qE "^\[focus60\] DONE" "$log" 2>/dev/null || echo "n60-focus-probe: no DONE line (exceptions in the log: $(grep -c Exception "$log" 2>/dev/null))" | tee -a "$out"
grep -qE "^\[focus60\] summary: .*focus losses 0," "$log" 2>/dev/null && echo "n60-focus-probe: MISSED: the probe saw no focus loss after its ready line (the steal fell outside its window): this run is no verdict" | tee -a "$out"
