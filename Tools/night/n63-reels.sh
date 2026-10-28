#!/usr/bin/env bash
# NIGHT63 (the forge FORGE62, 2026-10-22; a copy of n60-reels.sh with n63-prune.sh in its prune loop). Player reels, one invocation per
# set, serial (one heavy job at a time, L-005), with n63-prune.sh in a loop beside the set. The guard hook refuses the whole set under
# 12 GB free; between sides this script skips a side under 10 GB (a SKIPPED reel marks its entry RED, never "passed").
#   bash Tools/night/n63-reels.sh <tag> <side>,<level>,<scenario>[,<extra arg>...] ...
#   e.g. bash Tools/night/n63-reels.sh n63-b1 v1on,n57-kiln,a157,-skygrindClearShots60 v1off,n57-kiln,a157,-skygrindClearShots60,-skygrindNoExit58
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary47.txt (cut at 2400 chars; the full line is in
# Logs/player/n47-reel-<tag>-<side>.log).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary47.txt"
bash Tools/night/n63-prune.sh "$tag" --loop >/dev/null 2>&1 &
pp=$!
for spec in "$@"; do
  IFS=',' read -r -a a <<< "$spec"
  side="${a[0]}"; level="${a[1]}"; scen="${a[2]}"; extra=("${a[@]:3}")
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 10 ]; then echo "== $side SKIPPED: C: has ${free} GB free (< 10)" >> "$sum"; continue; fi
  echo "== $side $(date +%T) (map $level, scenario $scen ${extra[*]}; C: ${free} GB free)" >> "$sum"
  # FORGE62b (red team 2h, finding 2): 420 s per side, not 1500: a reel takes 63-121 s (NIGHT60), and a hung side must not hold a 2-hour run
  NIGHT=n47 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 420 "${extra[@]}" -skygrindMap "$level" -skygrindReelOnly "$scen" 2>&1 \
    | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  log="Logs/player/n47-reel-$tag-$side.log"
  echo "   NRE count: $(grep -c NullReference "$log" 2>/dev/null), exceptions: $(grep -c Exception "$log" 2>/dev/null), frames: $(ls "Logs/feel/$tag/$side" 2>/dev/null | wc -l)" >> "$sum"
done
echo "== all done $(date +%T)" >> "$sum"
sleep 2; kill $pp 2>/dev/null; bash Tools/night/n63-prune.sh "$tag"
