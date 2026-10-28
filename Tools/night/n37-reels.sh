#!/usr/bin/env bash
# NIGHT37 (the lead): one round's Player reels, one invocation per set, serial (one heavy job at a time, L-005). Budget: NIGHT37.md
# lean_rules (26 invocations in all).
#   bash Tools/night/n37-reels.sh <tag> <set...>
#   sets: wick | stag | loom | crown (each boss's own reel on its new level) | verbs1 | verbs2 | verbs4 | verbs5 (verbs37, one Player per
#         map) | redwick | redstag | redloom | redcrown (-skygrindOff37: NIGHT36's boss on the old level) | boot3 | boot6 (levels 3 and 6
#         still boot: verbs37 on them)
# Results: Logs/feel/<tag>/<side>/..., the [reel] lines into Logs/feel/<tag>/summary.txt
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
mkdir -p "Logs/feel/$tag"
sum="Logs/feel/$tag/summary.txt"
run() { # side, timeout, args...
  local side="$1" to="$2"; shift 2
  echo "== $side $(date +%T) ($*)" >> "$sum"
  NIGHT=n37 bash Tools/night/reel.sh "Builds/$tag" "$tag" "$side" "$to" "$@" 2>&1 | grep -E "^\[reel\] [a-z0-9]+:|done:" | cut -c1-2400 >> "$sum"
  echo "   NRE count: $(grep -c NullReference Logs/player/n37-reel-$tag-$side.log 2>/dev/null), exceptions: $(grep -c Exception Logs/player/n37-reel-$tag-$side.log 2>/dev/null)" >> "$sum"
}
for s in "$@"; do
  case "$s" in
    wick) run wick 1500 -skygrindMap n37-foundry -skygrindReelOnly wick37 ;;
    stag) run stag 1500 -skygrindMap n37-iceline -skygrindReelOnly stag37 ;;
    loom) run loom 1500 -skygrindMap n37-span -skygrindReelOnly loom37 ;;
    crown) run crown 1500 -skygrindMap n37-spire -skygrindReelOnly crown37 ;;
    arena1) run arena1 1200 -skygrindMap n37-foundry -skygrindReelOnly level36 ;; # the arena rooms' frames36 (L's level36 reel)
    arena2) run arena2 1200 -skygrindMap n37-iceline -skygrindReelOnly level36 ;;
    arena4) run arena4 1200 -skygrindMap n37-span -skygrindReelOnly level36 ;;
    arena5) run arena5 1200 -skygrindMap n37-spire -skygrindReelOnly level36 ;;
    verbs1) run verbs1 1500 -skygrindMap n37-foundry -skygrindReelOnly verbs37 ;;
    verbs2) run verbs2 1500 -skygrindMap n37-iceline -skygrindReelOnly verbs37 ;;
    verbs4) run verbs4 1500 -skygrindMap n37-span -skygrindReelOnly verbs37 ;;
    verbs5) run verbs5 1500 -skygrindMap n37-spire -skygrindReelOnly verbs37 ;;
    boot3) run boot3 1500 -skygrindMap n35-kiln -skygrindReelOnly verbs37 ;;
    boot6) run boot6 1500 -skygrindMap n35-finale -skygrindReelOnly verbs37 ;;
    redwick) run redwick 1500 -skygrindOff37 -skygrindMap n36-foundry -skygrindReelOnly wick37 ;;
    redstag) run redstag 1500 -skygrindOff37 -skygrindMap n35-iceline -skygrindReelOnly stag37 ;;
    redloom) run redloom 1500 -skygrindOff37 -skygrindMap n33-span -skygrindReelOnly loom37 ;;
    redcrown) run redcrown 1500 -skygrindOff37 -skygrindMap n33-spire -skygrindReelOnly crown37 ;;
    *) echo "unknown set $s" >> "$sum" ;;
  esac
done
echo "== all done $(date +%T)" >> "$sum"
