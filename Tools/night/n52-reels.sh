#!/usr/bin/env bash
# NIGHT52 (the lead's; from n51-reels.sh / n47-reels.sh): the midpoint / freeze reel set on one build, serial (one Player at a time, L-005).
# The build copy must have the rows on (node Tools/night/n46-build-rows.cjs <tag> on n52.kiln52 ... n52.look52).
#   bash Tools/night/n52-reels.sh <tag> <set>...   sets: verbs (verbs37 on the five n52 levels), rooms (a452 on the five), u (a152 / a252 kiln
#   + spire / a352, each on and -skygrindOff52), grade (a349 on n52-iceline and n52-span, on and -skygrindOff52), before (level36 on the five
#   old levels + the five n52 levels: the sheet's entry frames), tiers (a349 on n52-foundry, on and off), all, freeze
# Summary: Logs/feel/<tag>/summary47.txt (n47-reels.sh's), one line per reel.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
R=(bash Tools/night/n47-reels.sh "$tag")
for set in "$@"; do
  case "$set" in
    verbs) "${R[@]}" vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n52-kiln,verbs37 vp,n52-span,verbs37 vs,n52-spire,verbs37 ;;
    rooms) "${R[@]}" rf,n52-foundry,a452 ri,n52-iceline,a452 rk,n52-kiln,a452 rp,n52-span,a452 rs,n52-spire,a452 ;;
    u) "${R[@]}" u1on,n48-foundry,a152 u1off,n48-foundry,a152,-skygrindOff52 u2on,n47-kiln,a252 u2off,n47-kiln,a252,-skygrindOff52 \
         u5on,n47-spire,a252 u5off,n47-spire,a252,-skygrindOff52 u3on,n48-foundry,a352 u3off,n48-foundry,a352,-skygrindOff52 ;;
    grade) "${R[@]}" gi-on,n52-iceline,a349 gi-off,n52-iceline,a349,-skygrindOff52 gp-on,n52-span,a349 gp-off,n52-span,a349,-skygrindOff52 ;;
    tiers) "${R[@]}" gf-on,n52-foundry,a349 gf-off,n52-foundry,a349,-skygrindOff52 ;;   # look52's tiers ship on foundry only (D-22)
    before) "${R[@]}" b-foundry,n48-foundry,level36 b-iceline,n47-iceline,level36 b-kiln,n47-kiln,level36 b-span,n47-span,level36 b-spire,n47-spire,level36 \
         a-foundry,n52-foundry,level36 a-iceline,n52-iceline,level36 a-kiln,n52-kiln,level36 a-span,n52-span,level36 a-spire,n52-spire,level36 ;;
    all) "$0" "$tag" verbs rooms u grade ;;
    freeze) "$0" "$tag" verbs rooms u tiers before ;;   # the freeze build's set (grade dropped: look52.gradeLevels [] ships, D-22)
    *) echo "unknown set $set" ;;
  esac
done
grep -E "^\[reel\] (verbs37: verbs37|a452: done|a152|a252|a352|a349)" "Logs/feel/$tag/summary47.txt" | cut -c1-200
