#!/usr/bin/env bash
# NIGHT45 (the lead; from n44-reels.sh): Player reels, one invocation per set, serial (one heavy job at a time, L-005). Budget: NIGHT42.md lean_rules (38).
#   bash Tools/night/n41-reels.sh <tag> <side>,<level>,<scenario>[,<extra arg>...] ...
#   e.g. bash Tools/night/n41-reels.sh n39-r4 g0kiln,n38-kiln,verbs37 g0loom,n38-span,loom37
#   A campaign level boots only through reel.sh + -skygrindMap + -skygrindReelOnly (L-062). Refuses to start a reel while C: has under
#   10 GB free (NIGHT42.md lean_rules). Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary45.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary45.txt"
for spec in "$@"; do
  IFS=',' read -r -a a <<< "$spec"
  side="${a[0]}"; level="${a[1]}"; scen="${a[2]}"; extra=("${a[@]:3}")
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 10 ]; then echo "== $side SKIPPED: C: has ${free} GB free (< 10)" >> "$sum"; continue; fi
  echo "== $side $(date +%T) (map $level, scenario $scen ${extra[*]}; C: ${free} GB free)" >> "$sum"
  NIGHT=n45 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 1500 "${extra[@]}" -skygrindMap "$level" -skygrindReelOnly "$scen" 2>&1 \
    | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  log="Logs/player/n45-reel-$tag-$side.log"
  echo "   NRE count: $(grep -c NullReference "$log" 2>/dev/null), exceptions: $(grep -c Exception "$log" 2>/dev/null), frames: $(ls "Logs/feel/$tag/$side" 2>/dev/null | wc -l)" >> "$sum"
done
echo "== all done $(date +%T)" >> "$sum"
