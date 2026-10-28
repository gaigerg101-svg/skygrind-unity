#!/usr/bin/env bash
# NIGHT30 (the lead): one round's Player reels, GREEN (rows on) and RED (-skygrindOff30), one invocation per map, serial (one heavy job).
#   bash Tools/night/n30-reels.sh <tag> <set...>     sets: surv | foundry | iceline | span | kiln | redsurv | redfoundry
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T)" >> "$sum"
  NIGHT=n30 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-1500 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n30-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    surv) run on 1500 -skygrindReelOnly hudfx30,menus30,layout30,weaponhud30,rules30,lantern30,glacier30 ;;
    foundry) run foundry 1500 -skygrindMap n30-foundry -skygrindReelOnly lava30,rooms30,look30,lesson30 ;;
    iceline) run iceline 1200 -skygrindMap n30-iceline -skygrindReelOnly glide30,rooms30,lesson30 ;;
    kiln) run kiln 1200 -skygrindMap n30-kiln -skygrindReelOnly rooms30,wyrm30 ;;
    span) run span 1200 -skygrindMap n30-span -skygrindReelOnly span30,arcline30,loom30,rooms30 ;;
    redsurv) run red 1200 -skygrindOff30 -skygrindReelOnly hudfx30,menus30,layout30,weaponhud30,rules30 ;;
    pad) run pad 900 -skygrindReelOnly menus30,weaponhud30 ;;
    redfoundry) run redfoundry 1200 -skygrindOff30 -skygrindMap n30-foundry -skygrindReelOnly lava30,look30,lesson30 ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
