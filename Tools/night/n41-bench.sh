#!/usr/bin/env bash
# NIGHT41 (the lead; INVENTORY C8): the Player bench (-skygrindBench 60: director.maxAlive husks chasing a god-mode player) on a campaign
# map booted by -skygrindMap (n40-iceline: the night's heaviest look, the frost props and the new views), the power plan and the CPU load
# before and after only (NIGHT15: a poller during the run was the noise), alone (L-005), closing the Player by its PID after 300 s.
# models41 is switched in the BUILD's config copy (n39-build-rows.cjs) between runs; the repo's config is untouched.
#   bash Tools/night/n41-bench.sh <buildTag> <runName> [map]      e.g. bash Tools/night/n41-bench.sh n41-r7 off1 n40-iceline
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?build tag}"; run="${2:?run name}"; map="${3:-n40-iceline}"
log="Logs/player/n41-bench-$tag-$run.log"
state() { powershell -NoProfile -Command "(powercfg -getactivescheme) -replace '.*\(',''; (Get-CimInstance Win32_Battery).BatteryStatus; (Get-Counter '\Processor(_Total)\% Processor Time' -SampleInterval 1 -MaxSamples 2).CounterSamples.CookedValue -join ' '" 2>/dev/null | tr '\n' ' '; }
before="$(state)"
echo "$(date +%H:%M:%S) bench $tag $run on $map before: $before"
cpu=$(echo "$before" | awk '{printf "%d", ($(NF-1) + $NF) / 2}')
if [ "${FORCE:-0}" != 1 ] && [ "${cpu:-100}" -gt 30 ]; then echo "SKIP: CPU ${cpu} % > 30 % (FORCE=1 overrides)"; exit 3; fi
"Builds/$tag/SKYGRIND.exe" -skygrindBench 60 -skygrindMap "$map" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile "$log" &
pid=$!
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge 300 ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) bench $tag $run after ($t s): $(state)"
grep -E "\[bench\]" "$log" | tail -6 | cut -c1-300
