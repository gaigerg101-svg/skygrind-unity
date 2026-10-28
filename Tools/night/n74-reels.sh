#!/usr/bin/env bash
# NIGHT74 (FORGE74, n74-tools.cjs): a copy of n73-reels.sh with the n74 names.
# NIGHT73 (a copy of n72-reels.sh with the n74 names and --keep). Player reels, one invocation per set, serial (one heavy job at a time, L-005).
# The guard hook refuses the whole set under 12 GB free; between sides this script skips a side under 10 GB (a SKIPPED reel marks its entry RED).
#   bash Tools/night/n74-reels.sh [--keep] <tag> <side>,<level>,<scenario>[,<extra arg>...] ...
# --keep: no prune at all (every rendered frame stays: the motion strips, n74-strip.py, are cut from these sides). Without it, n72-prune's
# rule (every 50th frame) runs at the end through n74-prune.sh. A kept side is about 0.5-1.5 GB; prune it by hand after its strip is cut.
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary47.txt (cut at 2400 chars; the full line is in
# Logs/player/n47-reel-<tag>-<side>.log).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
keep=0
if [ "$1" = "--keep" ]; then keep=1; shift; fi
tag="${1:?tag}"; shift
case "$tag" in n74-*) ;; *) echo "refused: only n74-* tags"; exit 1 ;; esac
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary47.txt"
for spec in "$@"; do
  IFS=',' read -r -a a <<< "$spec"
  side="${a[0]}"; level="${a[1]}"; scen="${a[2]}"; extra=("${a[@]:3}")
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 10 ]; then echo "== $side SKIPPED: C: has ${free} GB free (< 10)" >> "$sum"; continue; fi
  echo "== $side $(date +%T) (map $level, scenario $scen ${extra[*]}; C: ${free} GB free; keep $keep)" >> "$sum"
  NIGHT=n47 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 420 "${extra[@]}" -skygrindMap "$level" -skygrindReelOnly "$scen" 2>&1 \
    | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  log="Logs/player/n47-reel-$tag-$side.log"
  echo "   NRE count: $(grep -c NullReference "$log" 2>/dev/null), exceptions: $(grep -c Exception "$log" 2>/dev/null), frames: $(ls "Logs/feel/$tag/$side" 2>/dev/null | wc -l)" >> "$sum"
done
echo "== all done $(date +%T)" >> "$sum"
# only this set's own sides (a --keep side of an earlier set in the same tag survives: the 18:03 set a2 pruned set a1's kept frames)
mine=(); for spec in "$@"; do mine+=("${spec%%,*}"); done
[ "$keep" = 1 ] || bash Tools/night/n74-prune.sh "$tag" "${mine[@]}"
