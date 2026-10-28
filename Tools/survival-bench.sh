#!/usr/bin/env bash
# The Survival bench in the built Player (NIGHT8 U2-02): SKYGRIND.exe -skygrindBench <seconds> [play] at 1920 x 1080
# windowed. NIGHT13 U3-26.2 (bench hygiene, audit-perf section 2 item 2): the Windows power plan and the AC state are
# polled once before and once after the run, with their times, never during it (the old poller started a PowerShell +
# WMI query every 2 s, 16 times a run, beside the Player). The plan flips silently on this laptop (the memory note):
# a run counts only if both polls show the same plan. Prints the Player's [skygrind] bench lines.
# usage: bash Tools/survival-bench.sh [seconds=60] [exe=Builds/Stage1/SKYGRIND.exe] [log=Logs/player/bench.log] [scenario: play | autopilot (NIGHT14)]
set -u
cd "$(dirname "$0")/.." || exit 1
SECS="${1:-60}"
EXE="${2:-Builds/Stage1/SKYGRIND.exe}"
LOG="${3:-Logs/player/bench.log}"
SCEN="${4:-}"
mkdir -p "$(dirname "$LOG")"
POLL="$LOG.power"
plan() { powercfg -getactivescheme | tr -d '\r' | sed -n 's/.*(\(.*\)).*/\1/p'; } # "-" switch: MSYS rewrites "/x" as a path
ac() { powershell -NoProfile -Command "(Get-CimInstance -ClassName BatteryStatus -Namespace root/wmi | Select-Object -First 1).PowerOnline" | tr -d '\r'; }
echo "before $(date +%H:%M:%S) $(plan) AC=$(ac)" > "$POLL"
start=$(date +%H:%M:%S)
"$EXE" -skygrindBench "$SECS" $SCEN -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$(cygpath -w "$PWD/$LOG")" > /dev/null &
pid=$!
wait $pid
code=$?
end=$(date +%H:%M:%S)
echo "after $(date +%H:%M:%S) $(plan) AC=$(ac)" >> "$POLL"
echo "bench $start-$end, exit $code, pid $pid, scenario ${SCEN:-ui-hurt}"
cat "$POLL"
grep -E "\[skygrind\] (bench|check)" "$LOG"
