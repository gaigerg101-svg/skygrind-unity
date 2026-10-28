#!/usr/bin/env bash
# NIGHT33 (the lead): one round's Player reels, GREEN (rows on) and RED (-skygrindOff33), one invocation per set, serial (one heavy job).
#   bash Tools/night/n33-reels.sh <tag> <set...>
#   sets: hud | redhud | look | redlook | kit | redkit | arenas | ai | redai | feel | redfeel | shop | bosses | redbosses   (map sets: lookf / lookk / kitf ...: see below)
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T)" >> "$sum"
  NIGHT=n33 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-1800 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n33-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    hud) run hud 900 -skygrindReelOnly hud33 ;;
    redhud) run redhud 900 -skygrindOff33 -skygrindReelOnly hud33 ;;
    look) run look 1200 -skygrindReelOnly look33 ;;
    redlook) run redlook 1200 -skygrindOff33 -skygrindReelOnly look33 ;;
    lookf) run lookf 900 -skygrindMap n33-foundry -skygrindReelOnly look33 ;; # kit33 on: the dressed file
    lookk) run lookk 900 -skygrindMap n32-kiln -skygrindReelOnly look33 ;;
    redlookf) run redlookf 900 -skygrindOff33 -skygrindMap n32-foundry -skygrindReelOnly look33 ;;
    redlookk) run redlookk 900 -skygrindOff33 -skygrindMap n32-kiln -skygrindReelOnly look33 ;;
    hudfeel) run hudfeel 1200 -skygrindReelOnly hud33,feel33 ;; # two scenarios, one Player (the reel cap)
    redhudfeel) run redhudfeel 1200 -skygrindOff33 -skygrindReelOnly hud33,feel33 ;;
    kit) run kit 1200 -skygrindMap n33-foundry -skygrindReelOnly kit33 ;; # K: needs kit33 on and the dressed n33 files
    redkit) run redkit 1200 -skygrindOff33 -skygrindMap n32-foundry -skygrindReelOnly kit33 ;;
    kits) run kits 1200 -skygrindMap n33-spire -skygrindReelOnly kit33 ;;
    redkits) run redkits 1200 -skygrindOff33 -skygrindMap n32-spire -skygrindReelOnly kit33 ;;
    arenas) run arenas 1200 -skygrindMap n33-foundry -skygrindReelOnly arenas33 ;; # R: f9, f8 + devices
    arenass) run arenass 1200 -skygrindMap n33-spire -skygrindReelOnly arenas33 ;; # R: v4, v2
    envf) run envf 1500 -skygrindMap n33-foundry -skygrindReelOnly look33,kit33,arenas33 ;; # L + K + R in one Player (the reel cap)
    hudshop) run hudshop 1500 -skygrindMap n33-foundry -skygrindReelOnly hud33,feel33,shop33 ;; # U + W + P in one Player (the reel cap)
    hudc) run hudc 1200 -skygrindMap n33-foundry -skygrindReelOnly hud33,feel33 ;; # U: the campaign pass (door marker, results, title, levels) + W feel
    redarenas) run redarenas 1200 -skygrindOff33 -skygrindReelOnly arenas33 ;;
    ai) run ai 900 -skygrindMap n32-spire -skygrindReelOnly ai33 ;; # E: room v6
    redai) run redai 900 -skygrindOff33 -skygrindMap n32-spire -skygrindReelOnly ai33 ;;
    feel) run feel 900 -skygrindReelOnly feel33 ;;
    redfeel) run redfeel 900 -skygrindOff33 -skygrindReelOnly feel33 ;;
    shop) run shop 900 -skygrindMap n33-foundry -skygrindReelOnly shop33 ;; # P (kit33 on: n33-foundry continues to n33-iceline)
    bosses) run bosses 1800 -skygrindReelOnly bosses32 ;; # NIGHT32's boss pilots with the NIGHT33 rows on (bosses33's kill times)
    redbosses) run redbosses 1800 -skygrindOff33 -skygrindReelOnly bosses32 ;;
    *) echo "unknown set $s" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
