#!/usr/bin/env bash
# NIGHT17: the Player smoke on Builds/Stage1, with the machine's idle time before it (a person at the machine fails the
# pad steps: HANDOFF section 6). Closes the Player by its PID after the timeout (CLAUDE.md rule 10).
#   bash smoke.sh <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:-smoke}"
log="Logs/player/${NIGHT:-n21}-smoke-$tag.log"
idle=$(powershell -NoProfile -Command "Add-Type @'
using System; using System.Runtime.InteropServices;
public static class IdleN17 { [StructLayout(LayoutKind.Sequential)] public struct L { public uint cb; public uint t; }
[DllImport(\"user32.dll\")] static extern bool GetLastInputInfo(ref L l);
public static uint Sec() { var l = new L(); l.cb = (uint)Marshal.SizeOf(l); GetLastInputInfo(ref l); return ((uint)Environment.TickCount - l.t) / 1000; } }
'@; [IdleN17]::Sec()" 2>/dev/null)
echo "$(date +%H:%M:%S) smoke $tag start; idle ${idle} s"
"${BDIR:-Builds/Stage1}"/SKYGRIND.exe -skygrindSmoke -logFile "$log" &
pid=$!
t=0
while kill -0 $pid 2>/dev/null; do
  sleep 3; t=$((t+3))
  if [ $t -ge 720 ]; then echo "timeout: closing pid $pid"; taskkill //PID $pid //F >/dev/null 2>&1; break; fi
done
echo "$(date +%H:%M:%S) smoke $tag end ($t s)"
grep -E "\[smoke\].*(FAIL|DONE)|Exception" "$log" | head -30
