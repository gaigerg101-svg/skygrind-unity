#!/usr/bin/env bash
# NIGHT18 M12: the Player bench (-skygrindBench 60, 24 husks) on Builds/Stage1, the power plan and CPU load before and
# after (never during: the 2 s poller was the noise in NIGHT15), closing the Player by its PID after 300 s.
#   bash bench.sh <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="$1"
log="Logs/player/${NIGHT:-n21}-bench-$tag.log"
state() { powershell -NoProfile -Command "(powercfg -getactivescheme) -replace '.*\(',''; (Get-CimInstance Win32_Battery).BatteryStatus; (Get-Counter '\Processor(_Total)\% Processor Time' -SampleInterval 1 -MaxSamples 2).CounterSamples.CookedValue -join ' '" 2>/dev/null | tr '\n' ' '; }
before="$(state)"
echo "$(date +%H:%M:%S) bench $tag before: $before"
# NIGHT21 (wave-forge): never bench on a busy machine (NIGHT20 final: CPU 100 % before both runs, p99 17 / 114 ms)
cpu=$(echo "$before" | awk '{printf "%d", ($(NF-1) + $NF) / 2}')
if [ "${FORCE:-0}" != 1 ] && [ "${cpu:-100}" -gt 30 ]; then echo "SKIP: CPU ${cpu} % > 30 % (FORCE=1 overrides); the bench is not run"; exit 3; fi
"${BDIR:-Builds/Stage1}"/SKYGRIND.exe -skygrindBench 60 -logFile "$log" &
pid=$!
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge 300 ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) bench $tag after ($t s): $(state)"
grep -E "\[bench\]" "$log" | tail -8
