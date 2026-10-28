#!/usr/bin/env bash
# NIGHT35 (the lead): one round's Player reels, GREEN (rows on) and one combined RED (-skygrindOff35), one invocation per set, serial
# (one heavy job at a time, L-005). Budget: NIGHT35.md lean_rules (22 invocations in all).
#   bash Tools/night/n35-reels.sh <tag> <set...>
#   sets: charge | pickup | teach | wallride | grapple | look | levels1..levels6 | verbs1 | verbs3 | verbs4 | red | redcamp | bosses
#   level ids from the env (A may swap the files): L1 L2 L3 L4 L5 L6 (defaults: NIGHT34's ids and the finale)
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
L1="${L1:-n35-foundry}"; L2="${L2:-n35-iceline}"; L3="${L3:-n35-kiln}"; L4="${L4:-n33-span}"; L5="${L5:-n33-spire}"; L6="${L6:-n35-finale}"
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T) ($*)" >> "$sum"
  NIGHT=n35 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n35-reel-$tag-$side.log 2>/dev/null), exceptions: $(grep -c Exception Logs/player/n35-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    charge) run charge 900 -skygrindMap "$L1" -skygrindReelOnly charge35 ;; # W: the KESTREL hold / tap on level 1
    charge3) run charge3 900 -skygrindMap "$L3" -skygrindReelOnly charge35 ;; # W: the THUNDERCLAP on level 3 (its own Player)
    pickup) run pickup 900 -skygrindMap "$L3" -skygrindReelOnly pickup35 ;; # W: the shotgun pickup, the health pickup (level 3)
    teach) run teach 900 -skygrindMap "$L1" -skygrindReelOnly teach35 ;; # T: each level-1 text in its trigger
    wallride) run wallride 900 -skygrindMap "$L2" -skygrindReelOnly wallride35 ;; # M: the Lucio ride
    grapple) run grapple 900 -skygrindMap "$L4" -skygrindReelOnly grapple35 ;; # M: the pull on level 4
    look) run look 900 -skygrindMap "$L1" -skygrindReelOnly look35 ;; # V: PS2 pass, lights, TV HUD, ride look
    levels1) run levels1 1200 -skygrindMap "$L1" -skygrindReelOnly levels35 ;; # A: boots + room frames (L-043)
    levels2) run levels2 1200 -skygrindMap "$L2" -skygrindReelOnly levels35 ;;
    levels3) run levels3 1200 -skygrindMap "$L3" -skygrindReelOnly levels35 ;;
    levels6) run levels6 1200 -skygrindMap "$L6" -skygrindReelOnly levels35 ;;
    verbs1) run verbs1 1200 -skygrindMap "$L1" -skygrindReelOnly verbs35 ;; # T: every verb, one Player per map (L-046)
    verbs3) run verbs3 1200 -skygrindMap "$L3" -skygrindReelOnly verbs35 ;;
    verbs4) run verbs4 1200 -skygrindMap "$L4" -skygrindReelOnly verbs35 ;;
    red) run red 1500 -skygrindOff35 -skygrindMap "$L1" -skygrindReelOnly charge35,teach35,look35 ;; # the combined RED (NIGHT34's game) on level 1
    redcamp) run redcamp 1500 -skygrindOff35 -skygrindMap "$L3" -skygrindReelOnly pickup35,wallride35 ;; # RED for the level-3 / ride rows
    bosses) run bosses 1800 -skygrindReelOnly bosses32 ;; # the boss pilots with the NIGHT35 rows on (N-11: the bend from level 4)
    *) echo "unknown set $s" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
