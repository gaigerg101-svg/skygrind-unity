#!/usr/bin/env bash
# NIGHT25: third proof set (all bosses, results, hitch): bash n25-proof3.sh <buildDir> <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
export NIGHT=n25
b="$1"; tag="$2"
f() { grep -E "^\[reel\] [a-z0-9]+:|Exception|timeout|done"; }
bash Tools/night/reel.sh "$b" "$tag" bosses10 900 -skygrindMap level10 -skygrindReelOnly bosses 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" results 900 -skygrindMap tutorial -skygrindReelOnly campresults 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" pad 600 -skygrindReelPad -skygrindReelOnly meleefire,pulse,lesson 2>&1 | f
"$b/SKYGRIND.exe" -skygrindHitch -logFile Logs/player/n25-hitch-$tag.log >/dev/null 2>&1
grep -E "\[hitch\].*cold PLAY|hitch SUMMARY|prewarm25" Logs/player/n25-hitch-$tag.log | cut -c1-260
echo "proof3 $tag finished $(date +%H:%M:%S)"
