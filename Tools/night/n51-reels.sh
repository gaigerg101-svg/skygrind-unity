#!/usr/bin/env bash
# NIGHT51 reels (the lead; from n50-reels.sh): Player reels, one at a time (L-005). NIGHT51's changes are view-only and on by default
# (no config row); an OFF side passes -skygrindOff51 (NIGHT50's game).
#   bash Tools/night/n51-reels.sh <tag> <set>...     sets: v (a349 per level, ON), o (a349 on n47-kiln and n48-foundry, OFF), x (a249 kit), all
# Summary: Logs/feel/<tag>/summary51.txt. Frames: Logs/feel/<tag>/<side>/.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
sum="Logs/feel/$tag/summary51.txt"
mkdir -p "Logs/feel/$tag"
LEVELS=(n48-foundry n47-iceline n47-kiln n47-span n47-spire)
run() { # side, map, phase, extra args...
  local side="$1" map="$2" phase="$3"; shift 3
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 10 ]; then echo "== $side SKIPPED: C: has ${free} GB free" >> "$sum"; return; fi
  echo "== $side $(date +%T) (map $map, phase $phase $*; C: ${free} GB free)" >> "$sum"
  NIGHT=n51 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 1500 "$@" -skygrindMap "$map" -skygrindReelOnly "$phase" 2>&1 \
    | grep -E "^\[reel\] |done:|Exception|NullReference" | cut -c1-900 >> "$sum"
  echo "   NRE $(grep -c NullReference Logs/player/n51-reel-$tag-$side.log 2>/dev/null), exceptions $(grep -c Exception Logs/player/n51-reel-$tag-$side.log 2>/dev/null), frames $(ls Logs/feel/$tag/$side 2>/dev/null | wc -l)" >> "$sum"
}
for set in "$@"; do
  case "$set" in
    v) for L in "${LEVELS[@]}"; do run "v-$L" "$L" a349; done ;;
    o) for L in n47-kiln n48-foundry; do run "o-$L" "$L" a349 -skygrindOff51; done ;;
    x) run x1080 n47-iceline a249 -screen-width 1920 -screen-height 1080 ;;
    all) "$0" "$tag" v o x ;;
    *) echo "unknown set $set" >> "$sum" ;;
  esac
done
