#!/usr/bin/env bash
# NIGHT34 (the lead): one round's Player reels, GREEN (rows on) and one combined RED (-skygrindOff34), one invocation per set, serial
# (one heavy job at a time, L-005). The reel budget is NIGHT34.md lean_rules: per round one GREEN per landed card, one combined RED.
#   bash Tools/night/n34-reels.sh <tag> <set...>
#   sets: bend | hud | lines | linesk | linesi | redlines | flow | red | bosses | redbosses | shop
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T)" >> "$sum"
  NIGHT=n34 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-1800 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n34-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    bend) run bend 900 -skygrindReelOnly bend34 ;; # B: the grapple BEND (f1-f4) + H's overlay
    hud) run hud 900 -skygrindReelOnly hud34 ;; # H: the rail, nomark, the bend's rail step
    lines) run lines 1200 -skygrindMap n34-foundry -skygrindReelOnly lines34 ;; # A: boots the level (L-043), frames of the first fight rooms
    linesk) run linesk 1200 -skygrindMap n34-kiln -skygrindReelOnly lines34 ;;
    linesi) run linesi 1200 -skygrindMap n34-iceline -skygrindReelOnly lines34 ;;
    redlines) run redlines 1200 -skygrindOff34 -skygrindMap n33-foundry -skygrindReelOnly lines34 ;; # RED: NIGHT33's foundry
    flow) run flow 900 -skygrindReelOnly flow34 ;; # M: a bhop line and a grapple release
    red) run red 1500 -skygrindOff34 -skygrindReelOnly bend34,hud34 ;; # the SURVIVAL rows' RED frames in one Player (lines34 RED: redlines, a campaign boot)
    bosses) run bosses 1800 -skygrindReelOnly bosses32 ;; # the boss pilots with the NIGHT34 rows on (reach34; T-45 Loom / Crown)
    redbosses) run redbosses 1800 -skygrindOff34 -skygrindReelOnly bosses32 ;;
    shop) run shop 900 -skygrindMap n33-foundry -skygrindReelOnly shop33 ;; # T-44: the shop's pad path, alone in a fresh Player
    *) echo "unknown set $s" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
