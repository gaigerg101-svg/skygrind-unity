#!/usr/bin/env bash
# NIGHT56 (the lead; from n55-smoke-steal.sh; gate-r1 P0-smoke: the r1 steals never took focus: the smoke printed "focus changes 0").
# The Player smoke with PLANTED focus steals that really take the foreground: Tools/night/n56-foreground.ps1 shows a window from its own
# process and forces it to the front (ALT tap + AttachThreadInput), holds it 3 s and closes it (no Notepad: Windows 11's Notepad shares one
# process with the owner's own tabs). Each steal prints whether it took ("took True").
#   bash Tools/night/n56-smoke-steal.sh <tag> [steal times in s, default "60 200"]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; times="${2:-60 200}"
out="Logs/n56/smoke-steal-$tag.out"
echo "$(date +%T) steal smoke on Builds/$tag, steals at: $times" > "$out"
BDIR="Builds/$tag" NIGHT=n56 bash Tools/night/smoke.sh "steal-$tag" >> "$out" 2>&1 &
sp=$!
t0=$(date +%s)
for t in $times; do
  while [ $(( $(date +%s) - t0 )) -lt "$t" ]; do kill -0 $sp 2>/dev/null || break 2; sleep 1; done
  echo "$(date +%T) steal at +$(( $(date +%s) - t0 )) s:" >> "$out"
  powershell -NoProfile -ExecutionPolicy Bypass -File Tools/night/n56-foreground.ps1 -HoldS 3 >> "$out" 2>&1
done
wait $sp
echo "$(date +%T) steal smoke done" >> "$out"
grep -E "steal|smoke .*(start|end)|\[smoke\].*(FAIL|DONE)|focus" "$out" | head -40
