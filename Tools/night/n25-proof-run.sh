#!/usr/bin/env bash
# NIGHT25: the proof reels of one build, one after another: ON (survival ids), CAMP (campaign map ids), TUT (tutorial), RED (-skygrindOff25), PAD.
#   bash n25-proof-run.sh <buildDir> <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
export NIGHT=n25
b="$1"; tag="$2"
S=shockwave,pulse,lesson,bigpick,meleefire
f() { grep -E "^\[reel\] [a-z0-9]+:|Exception|timeout|done"; }
bash Tools/night/reel.sh "$b" "$tag" on 600 -skygrindReelOnly $S 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" camp 1500 -skygrindMap layer1 -skygrindReelOnly wyrmvoid,wyrmbar,wyrmdies,wyrmaim 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" tut 900 -skygrindMap tutorial -skygrindReelOnly tutorial 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" red 600 -skygrindOff25 -skygrindReelOnly $S 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" redcamp 600 -skygrindOff25 -skygrindMap layer1 -skygrindReelOnly wyrmvoid,wyrmbar 2>&1 | f
bash Tools/night/reel.sh "$b" "$tag" pad 600 -skygrindReelPad -skygrindReelOnly meleefire,pulse,lesson 2>&1 | f
echo "proof run $tag finished $(date +%H:%M:%S)"
