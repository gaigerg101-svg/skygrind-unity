#!/usr/bin/env bash
# NIGHT62 (the forge FORGE62, 2026-10-22; L-104, L-113): the night's disk watcher. Start it once in the bearings as a BACKGROUND command
# (the Bash tool's run_in_background): it keeps one line a minute in Logs/n62/disk.log and EXITS when C: falls under the warning line, so
# the background task's exit wakes the lead with the reason (a /loop cannot do this: it fires only between turns). It also exits when
# Logs/n62/diskwatch.stop exists (the close creates it) or after its hour cap.
#   bash Tools/night/n62-diskwatch.sh [warn_gb=15] [hours=8]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
warn="${1:-15}"; hours="${2:-8}"
mkdir -p Logs/n62
rm -f Logs/n62/diskwatch.stop
end=$(( $(date +%s) + hours * 3600 ))
while :; do
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  echo "$(date +%T) C: ${free} GB free" >> Logs/n62/disk.log
  if [ "${free:-0}" -lt "$warn" ]; then
    echo "DISK LOW $(date +%T): C: ${free} GB free (< ${warn}). Prune your own reel frames (bash Tools/night/n62-prune.sh <tag>) before the next build or reel; the guard refuses them under 12 GB." | tee -a Logs/n62/disk.log
    exit 3
  fi
  [ -f Logs/n62/diskwatch.stop ] && { echo "diskwatch stopped $(date +%T) (stop file)"; exit 0; }
  [ "$(date +%s)" -ge "$end" ] && { echo "diskwatch ended $(date +%T) (${hours} h cap)"; exit 0; }
  sleep 60
done
