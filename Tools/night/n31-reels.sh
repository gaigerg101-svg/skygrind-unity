#!/usr/bin/env bash
# NIGHT31 (the lead): one round's Player reels, GREEN (rows on) and RED (-skygrindOff31), one invocation per map, serial (one heavy job).
#   bash Tools/night/n31-reels.sh <tag> <set...>
#   sets: surv | foundry | iceline | kiln | span | redsurv | redfoundry | redspan | camp | bosses
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T)" >> "$sum"
  NIGHT=n31 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-1500 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n31-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    surv) run on 1200 -skygrindReelOnly charge31,lt31,edge31 ;;
    foundry) run foundry 1200 -skygrindMap n30-foundry -skygrindReelOnly unlocks31,spawn31,rooms31,arenas31 ;;
    iceline) run iceline 1200 -skygrindMap n30-iceline -skygrindReelOnly unlocks31,rooms31,arenas31 ;;
    kiln) run kiln 1200 -skygrindMap n30-kiln -skygrindReelOnly unlocks31,rooms31,arenas31 ;;
    span) run span 1200 -skygrindMap n30-span -skygrindReelOnly grapple31,ring31,unlocks31,rooms31,arenas31 ;;
    redsurv) run red 900 -skygrindOff31 -skygrindReelOnly charge31,lt31,edge31 ;;
    redfoundry) run redfoundry 900 -skygrindOff31 -skygrindMap n30-foundry -skygrindReelOnly unlocks31,spawn31 ;;
    redspan) run redspan 900 -skygrindOff31 -skygrindMap n30-span -skygrindReelOnly grapple31,ring31 ;;
    camp) run camp 1800 -skygrindReelOnly camp31 ;;
    bosses) run bosses 1800 -skygrindReelOnly lantern31,stag31,wyrm31 ;;
    loom) run loom 900 -skygrindMap n30-span -skygrindReelOnly loom31 ;;
    ride) run ride 600 -skygrindMap n30-span -skygrindReelOnly ride31 ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
