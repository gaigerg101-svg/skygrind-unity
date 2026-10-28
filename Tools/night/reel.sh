#!/usr/bin/env bash
# NIGHT19: the measured reel on a Player build into Logs/feel/<tag>/<side>, closing the Player by its PID after the
# timeout (CLAUDE.md rule 10).   bash reel.sh <buildDir> <tag> <side> [timeout s] [extra args ...]
#   side: a free label (n18 | on | pad); extra args go to the exe (e.g. -skygrindReelPad, -skygrindReelOnly a,b)
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
bdir="$1"; tag="$2"; side="$3"; to="${4:-1500}"; shift 4
D="C:/Users/gaige/dev/SKYGRIND-unity/Logs/feel/$tag/$side"
mkdir -p "Logs/feel/$tag/$side" Logs/player
log="Logs/player/${NIGHT:-n21}-reel-$tag-$side.log"
echo "$(date +%H:%M:%S) reel $bdir $tag $side -> $D ($*)"
"$bdir/SKYGRIND.exe" -skygrindReel "$D" "$@" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile "$log" &
pid=$!
echo "pid $pid"
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge "$to" ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) end ($t s)"
grep -E "^\[reel\]|Exception|NullReference" "$log" | tail -40
