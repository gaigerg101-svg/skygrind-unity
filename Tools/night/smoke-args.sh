#!/usr/bin/env bash
# NIGHT24: the Player smoke with extra exe arguments (smoke.sh has none).   bash smoke-args.sh <buildDir> <tag> [args...]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
bdir="$1"; tag="$2"; shift 2
log="Logs/player/${NIGHT:-n24}-smoke-$tag.log"
echo "$(date +%H:%M:%S) smoke $tag start ($*)"
"$bdir/SKYGRIND.exe" -skygrindSmoke "$@" -logFile "$log" &
pid=$!
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge 720 ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) smoke $tag end ($t s)"
grep -E "\[smoke\].*(FAIL|DONE)|Exception" "$log" | sort -u | cut -c1-260 | head -30
