#!/usr/bin/env bash
# NIGHT56 (the lead; from n55-reels.sh; its floor 6 GB, NIGHT56.md section 3): Player reels, one invocation per set, serial (one heavy job
# at a time, L-005), with n56-prune.sh in a loop beside the set (a proof reel writes ~840 MB). Refuses a reel under 6 GB free (a SKIPPED reel marks its
# entry RED, never "passed").
#   bash Tools/night/n55-reels.sh <tag> <side>,<level>,<scenario>[,<extra arg>...] ...
#   e.g. bash Tools/night/n56-reels.sh n56-r1 l1on,n52-foundry,a156 l1off,n52-foundry,a156,-skygrindOff56
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary47.txt (cut at 2400 chars; the full line is in
# Logs/player/n47-reel-<tag>-<side>.log).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary47.txt"
bash Tools/night/n56-prune.sh "$tag" --loop >/dev/null 2>&1 &
pp=$!
for spec in "$@"; do
  IFS=',' read -r -a a <<< "$spec"
  side="${a[0]}"; level="${a[1]}"; scen="${a[2]}"; extra=("${a[@]:3}")
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 6 ]; then echo "== $side SKIPPED: C: has ${free} GB free (< 6)" >> "$sum"; continue; fi
  echo "== $side $(date +%T) (map $level, scenario $scen ${extra[*]}; C: ${free} GB free)" >> "$sum"
  NIGHT=n47 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 1500 "${extra[@]}" -skygrindMap "$level" -skygrindReelOnly "$scen" 2>&1 \
    | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  log="Logs/player/n47-reel-$tag-$side.log"
  echo "   NRE count: $(grep -c NullReference "$log" 2>/dev/null), exceptions: $(grep -c Exception "$log" 2>/dev/null), frames: $(ls "Logs/feel/$tag/$side" 2>/dev/null | wc -l)" >> "$sum"
done
echo "== all done $(date +%T)" >> "$sum"
sleep 2; kill $pp 2>/dev/null; bash Tools/night/n56-prune.sh "$tag"
