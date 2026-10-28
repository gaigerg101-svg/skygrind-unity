#!/usr/bin/env bash
# NIGHT50 reels (the lead; from n49-reels.sh): Player reels, one at a time (L-005). The build copy's n50 view rows are turned ON with
# n39-build-rows.cjs (the repo's configs are never touched); an OFF side passes -skygrindOff50 (every n50 row off: the old game).
#   bash Tools/night/n50-reels.sh <tag> <set>...        sets: h (a150 gun / charge both sizes), x (a249 kit, the kill wash at n47-iceline),
#                                                         v (a349 per level: the grade on / off), all
# Summary: Logs/feel/<tag>/summary50.txt (the [reel] lines, NRE / exception counts per side). Frames: Logs/feel/<tag>/<side>/.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
sum="Logs/feel/$tag/summary50.txt"
mkdir -p "Logs/feel/$tag"
LEVELS=(n48-foundry n47-iceline n47-kiln n47-span n47-spire)
run() { # side, map, phase, extra args...
  local side="$1" map="$2" phase="$3"; shift 3
  free=$(df -BG /c | tail -1 | awk '{print $4}' | tr -d G)
  if [ "${free:-0}" -lt 10 ]; then echo "== $side SKIPPED: C: has ${free} GB free" >> "$sum"; return; fi
  echo "== $side $(date +%T) (map $map, phase $phase $*; C: ${free} GB free)" >> "$sum"
  NIGHT=n50 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" 1500 "$@" -skygrindMap "$map" -skygrindReelOnly "$phase" 2>&1 \
    | grep -E "^\[reel\] |done:|Exception|NullReference" | cut -c1-900 >> "$sum"
  echo "   NRE $(grep -c NullReference Logs/player/n50-reel-$tag-$side.log 2>/dev/null), exceptions $(grep -c Exception Logs/player/n50-reel-$tag-$side.log 2>/dev/null), frames $(ls Logs/feel/$tag/$side 2>/dev/null | wc -l)" >> "$sum"
}
for set in "$@"; do
  case "$set" in
    h) run h1080 n47-iceline a150 -screen-width 1920 -screen-height 1080
       run h720 n47-iceline a150 -screen-width 1280 -screen-height 720 ;;
    x) run x1080 n47-iceline a249 -screen-width 1920 -screen-height 1080 ;;
    v) for L in "${LEVELS[@]}"; do run "v-$L" "$L" a349; done ;;
    all) "$0" "$tag" h x v ;;
    *) echo "unknown set $set" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
