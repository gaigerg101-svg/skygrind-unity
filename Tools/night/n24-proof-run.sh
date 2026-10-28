#!/usr/bin/env bash
# NIGHT24: the proof reels of one build, sequential (one heavy job at a time): ON keyboard (survival ids, layer1 ids), RED (-skygrindOff24, both),
# PAD (padweapons). Logs: Logs/player/n24-reel-<tag>-*.log, frames Logs/feel/<tag>/<side>.   bash n24-proof-run.sh <buildDir> <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
export NIGHT=n24
b="$1"; tag="$2"
S=remove,title,shotgun,fist,reticle,bigpick,wyrm
L=room,limit,layer1,cue,lit
bash Tools/night/reel.sh "$b" "$tag" on 900 -skygrindReelOnly $S 2>&1 | grep -E "^\[reel\] [a-z]+:|Exception|timeout|done"
bash Tools/night/reel.sh "$b" "$tag" camp 1500 -skygrindMap layer1 -skygrindReelOnly $L 2>&1 | grep -E "^\[reel\] [a-z]+:|Exception|timeout|done"
bash Tools/night/reel.sh "$b" "$tag" red 900 -skygrindOff24 -skygrindReelOnly $S 2>&1 | grep -E "^\[reel\] [a-z]+:|Exception|timeout|done"
bash Tools/night/reel.sh "$b" "$tag" redcamp 900 -skygrindOff24 -skygrindMap layer1 -skygrindReelOnly $L 2>&1 | grep -E "^\[reel\] [a-z]+:|Exception|timeout|done"
bash Tools/night/reel.sh "$b" "$tag" pad 600 -skygrindReelPad -skygrindReelOnly padweapons,bigpick,reticle,fist 2>&1 | grep -E "^\[reel\] [a-z]+:|Exception|timeout|done"
echo "proof run $tag finished $(date +%H:%M:%S)"
