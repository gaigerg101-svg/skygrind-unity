#!/usr/bin/env bash
# PG3: Tools/night/n74-reels.sh for the PG3 builds (pg3-* tags; the night tool takes n74-* only: NIGHT74 is closed and its caps spent).
# Same reel runner, same summary, same prune; the guard hook still refuses the set under 12 GB free.
#   bash Tools/pg3/reels.sh [--keep] <tag> <side>,<level>,<scenario>[,<extra arg>...] ...
# Results: Logs/feel/<tag>/<side>/..., Logs/feel/<tag>/summary47.txt; each side's full Player log is Logs/player/n47-reel-<tag>-<side>.log
# (the [sound74] / [music74] lines are copied into the summary too).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
keep=0
if [ "$1" = "--keep" ]; then keep=1; shift; fi
tag="${1:?tag}"; shift
case "$tag" in pg3-*) ;; *) echo "refused: only pg3-* tags"; exit 1 ;; esac
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
  grep -E "^\[(sound74|music74|shot74|charge74|trail74|rim74|pop74|tripo)\]" "$log" 2>/dev/null | tail -6 | cut -c1-900 >> "$sum"
  echo "   NRE count: $(grep -c NullReference "$log" 2>/dev/null), exceptions: $(grep -c Exception "$log" 2>/dev/null), frames: $(ls "Logs/feel/$tag/$side" 2>/dev/null | wc -l)" >> "$sum"
done
echo "== all done $(date +%T)" >> "$sum"
mine=(); for spec in "$@"; do mine+=("${spec%%,*}"); done
[ "$keep" = 1 ] || bash Tools/night/n74-prune.sh "$tag" "${mine[@]}"
