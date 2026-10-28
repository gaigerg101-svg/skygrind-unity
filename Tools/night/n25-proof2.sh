#!/usr/bin/env bash
# NIGHT25: second proof set: the worm path, the bosses, the campaign results, a level walk each.   bash n25-proof2.sh <buildDir> <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
export NIGHT=n25
b="$1"; tag="$2"
f() { grep -E "^\[reel\] [a-z0-9]+:|Exception|timeout|done"; }
bash Tools/night/reel.sh "$b" "$tag" camp 1500 -skygrindMap layer1 -skygrindReelOnly wyrmdies,wyrmaim,room,limit,layer1,cue 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" bosses6 900 -skygrindMap level06 -skygrindReelOnly bosses 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" results 900 -skygrindMap tutorial -skygrindReelOnly campresults 2>&1 | f
echo "proof2 $tag finished $(date +%H:%M:%S)"
