#!/usr/bin/env bash
# NIGHT30 (the lead): run one Player check (a -skygrind... flag that quits by itself) and close the Player by its PID after a timeout (CLAUDE.md rule 10).
#   bash Tools/night/n30-check.sh <buildTag> <name> <timeout s> <args...>      -> Logs/player/n30-check-<buildTag>-<name>.log
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="$1"; name="$2"; to="$3"; shift 3
log="Logs/player/n30-check-$tag-$name.log"
"Builds/$tag/SKYGRIND.exe" "$@" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$log" &
pid=$!
for i in $(seq 1 "$to"); do kill -0 $pid 2>/dev/null || break; sleep 1; done
if kill -0 $pid 2>/dev/null; then
  wp=$(ps -W | awk -v p=$pid '$1==p {print $4}')
  taskkill //PID "$wp" //F >/dev/null 2>&1; echo "[check] $name: timeout after $to s (closed PID $wp)"
fi
grep -E "PASS|FAIL|RED|GREEN" "$log" | grep -iv "warning" | tail -12 | cut -c1-400
