#!/usr/bin/env bash
# NIGHT32 (the lead): one round's Player reels, GREEN (rows on) and RED (-skygrindOff32), one invocation per set, serial (one heavy job).
#   bash Tools/night/n32-reels.sh <tag> <set...>
#   sets: bosses | camp | grapple | redgrapple | hud | ring | arenas | span | spire | boss5 | redbosses
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T)" >> "$sum"
  NIGHT=n32 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-1800 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n32-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    bosses) run bosses 1800 -skygrindReelOnly bosses32 ;;
    redbosses) run redbosses 1800 -skygrindOff32 -skygrindReelOnly lantern31,stag31,wyrm31 ;;
    bosses31) run bosses31 1800 -skygrindReelOnly lantern31,stag31,wyrm31 ;; # NIGHT31's pilots with the NIGHT32 rows on (kill times until bosses32's reel lands)
    camp) run camp 1800 -skygrindReelOnly camp32 ;;
    bossesloom) run bossesloom 900 -skygrindMap n30-span -skygrindReelOnly bosses32 ;; # B: THE LOOM needs its campaign map
    camploom) run camploom 900 -skygrindMap n30-span -skygrindReelOnly camp32 ;;
    grapple) run grapple 900 -skygrindMap n30-span -skygrindReelOnly grapple32 ;;
    redgrapple) run redgrapple 900 -skygrindOff32 -skygrindMap n30-span -skygrindReelOnly grapple32 ;;
    hud) run hud 900 -skygrindReelOnly hud32,balance32 ;;
    ring) run ring 900 -skygrindMap n30-foundry -skygrindReelOnly ring32 ;;
    arenas) run arenas 1200 -skygrindReelOnly arenas32 ;;
    arenasf) run arenasf 900 -skygrindMap n32-foundry -skygrindReelOnly arenas32 ;; # A: f9b, f10
    arenask) run arenask 900 -skygrindMap n32-kiln -skygrindReelOnly arenas32 ;; # A: k2, k2b
    arenasi) run arenasi 900 -skygrindMap n32-iceline -skygrindReelOnly arenas32 ;; # A: i6, i6b
    span) run span 1200 -skygrindMap n32-span -skygrindReelOnly span32 ;;
    spire) run spire 1200 -skygrindMap n32-spire -skygrindReelOnly level5 ;;
    boss5) run boss5 1200 -skygrindReelOnly boss5 ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
