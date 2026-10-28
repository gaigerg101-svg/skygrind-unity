#!/usr/bin/env bash
# NIGHT36 (the lead): one round's Player reels, GREEN (rows on) and one combined RED (-skygrindOff36), one invocation per set, serial
# (one heavy job at a time, L-005). Budget: NIGHT36.md lean_rules (22 invocations in all).
#   bash Tools/night/n36-reels.sh <tag> <set...>
#   sets: guns | core | wallgrip | text | level | boss | verbs1 | red | redboss
#   L1 from the env (defaults to the from-scratch n36-foundry; NIGHT35's n35-foundry until level36 lands)
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
L1="${L1:-n36-foundry}"
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T) ($*)" >> "$sum"
  NIGHT=n36 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n36-reel-$tag-$side.log 2>/dev/null), exceptions: $(grep -c Exception Logs/player/n36-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    guns) run guns 900 -skygrindMap "$L1" -skygrindReelOnly tap36,crunch36,fist36 ;; # F: the tap, the crunch, the fist
    core) run core 900 -skygrindMap "$L1" -skygrindReelOnly parry36,launch36,heal36 ;; # P: parry, launch, heal
    wallgrip) run wallgrip 900 -skygrindMap "$L1" -skygrindReelOnly wallgrip36 ;; # M: the grip
    text) run text 900 -skygrindMap "$L1" -skygrindReelOnly text36 ;; # M: the text's place
    level) run level 1200 -skygrindMap "$L1" -skygrindReelOnly level36 ;; # L: every room's entry / middle / exit frames
    boss) run boss 1200 -skygrindMap "$L1" -skygrindReelOnly boss36 ;; # B: the walk-in, the tells, the parried orb, the stun
    verbs1) run verbs1 1500 -skygrindMap "$L1" -skygrindReelOnly verbs36 ;; # M: every verb on level 1 (L-046: one Player per map)
    red) run red 1500 -skygrindOff36 -skygrindMap n35-foundry -skygrindReelOnly tap36,crunch36,parry36,wallgrip36,text36 ;; # the combined RED (NIGHT35's game)
    redboss) run redboss 1500 -skygrindOff36 -skygrindMap n35-foundry -skygrindReelOnly boss36 ;; # RED: NIGHT35's LANTERN (the teleport)
    *) echo "unknown set $s" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
