#!/usr/bin/env bash
# NIGHT33 (the lead): one Player check (not a reel) on a build, closing the Player by its PID after the timeout (CLAUDE.md rule 10).
#   bash Tools/night/n33-check.sh <tag> <name> <timeout s> <exe args...>     e.g.  n33-r6 levelselect 900 -skygrindLevelSelect29
# Log: Logs/player/n33-check-<tag>-<name>.log; prints the check's own result lines and the exception count.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="$1"; name="$2"; to="$3"; shift 3
log="Logs/player/n33-check-$tag-$name.log"
echo "$(date +%H:%M:%S) check $tag $name ($*)"
"Builds/$tag/SKYGRIND.exe" "$@" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile "$log" &
pid=$!
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge "$to" ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) end ($t s)"
grep -E "\[(levelselect29|doorfree29|smoke)\].*(PASS|FAIL|DONE)|^\[levelselect29\] DONE|^\[doorfree29\]" "$log" | sort -u | cut -c1-260 | tail -12
echo "exceptions: $(grep -c 'Exception' "$log")  NRE: $(grep -c 'NullReference' "$log")"
