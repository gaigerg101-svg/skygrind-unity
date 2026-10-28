#!/usr/bin/env bash
# NIGHT55 (the lead; the pre-mortem's P0-smoke check): the Player smoke with PLANTED focus steals. Runs Tools/night/smoke.sh on
# Builds/<tag>, and at ~60 s and ~200 s after its start opens Notepad (which takes the foreground), waits 3 s, closes it by its own PID
# (CLAUDE.md rule 10), and prints each steal's time, so the smoke log can be read against them ("focus lost t=..., run continued").
#   bash Tools/night/n55-smoke-steal.sh <tag> [steal times in s, default "60 200"]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; times="${2:-60 200}"
out="Logs/n55/smoke-steal-$tag.out"
echo "$(date +%T) steal smoke on Builds/$tag, steals at: $times" > "$out"
BDIR="Builds/$tag" NIGHT=n55 bash Tools/night/smoke.sh "steal-$tag" >> "$out" 2>&1 &
sp=$!
t0=$(date +%s)
for t in $times; do
  while [ $(( $(date +%s) - t0 )) -lt "$t" ]; do kill -0 $sp 2>/dev/null || break 2; sleep 1; done
  np=$(powershell -NoProfile -Command "(Start-Process notepad.exe -PassThru).Id" 2>/dev/null | tr -d '\r')
  echo "$(date +%T) steal at +$(( $(date +%s) - t0 )) s: notepad pid $np" >> "$out"
  sleep 3
  [ -n "$np" ] && taskkill //PID "$np" //F >/dev/null 2>&1 && echo "$(date +%T) notepad $np closed" >> "$out"
done
wait $sp
echo "$(date +%T) steal smoke done" >> "$out"
grep -E "steal|smoke .*(start|end)|\[smoke\].*(FAIL|DONE)|focus" "$out" | head -40
