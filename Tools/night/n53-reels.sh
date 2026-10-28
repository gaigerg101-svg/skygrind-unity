#!/usr/bin/env bash
# NIGHT53 (the lead's; from n52-reels.sh): the third-cycle / freeze reel set on one build, serial (one Player at a time, L-005).
# The rooms are edited in place in the n52 levels (the n52 rows are ON in the config); polish53 is forced on in the build copy
# (node Tools/night/n46-build-rows.cjs <tag> on n53.polish53) or stays off until the freeze.
#   bash Tools/night/n53-reels.sh <tag> <set>...   sets: verbs (verbs37 on the five n52 levels), rooms (a452 on the five), vi / vs / vk / vp /
#   vf and ri / rs / rk / rp / rf (one level each), u (a153 on n52-foundry and n52-iceline, on and -skygrindOff53), freeze (verbs rooms u)
# Summary: Logs/feel/<tag>/summary47.txt (n47-reels.sh's), one line per reel.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
R=(bash Tools/night/n47-reels.sh "$tag")
for set in "$@"; do
  case "$set" in
    verbs) "${R[@]}" vf,n52-foundry,verbs37 vi,n52-iceline,verbs37 vk,n52-kiln,verbs37 vp,n52-span,verbs37 vs,n52-spire,verbs37 ;;
    rooms) "${R[@]}" rf,n52-foundry,a452 ri,n52-iceline,a452 rk,n52-kiln,a452 rp,n52-span,a452 rs,n52-spire,a452 ;;
    vf) "${R[@]}" vf,n52-foundry,verbs37 ;; vi) "${R[@]}" vi,n52-iceline,verbs37 ;; vk) "${R[@]}" vk,n52-kiln,verbs37 ;;
    vp) "${R[@]}" vp,n52-span,verbs37 ;; vs) "${R[@]}" vs,n52-spire,verbs37 ;;
    rf) "${R[@]}" rf,n52-foundry,a452 ;; ri) "${R[@]}" ri,n52-iceline,a452 ;; rk) "${R[@]}" rk,n52-kiln,a452 ;;
    rp) "${R[@]}" rp,n52-span,a452 ;; rs) "${R[@]}" rs,n52-spire,a452 ;;
    u) "${R[@]}" u1on,n52-foundry,a153 u1off,n52-foundry,a153,-skygrindOff53 u2on,n52-iceline,a153 u2off,n52-iceline,a153,-skygrindOff53 ;;
    freeze) "$0" "$tag" verbs rooms u ;;
    *) echo "unknown set $set" ;;
  esac
done
grep -E "^\[reel\] (verbs37: verbs37|a452: done|a153)" "Logs/feel/$tag/summary47.txt" | cut -c1-200
