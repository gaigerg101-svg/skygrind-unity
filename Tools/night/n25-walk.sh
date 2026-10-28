#!/usr/bin/env bash
# NIGHT25: levelwalk on every campaign level of a build, one after another.   bash n25-walk.sh <buildDir> <tag> [maps...]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
export NIGHT=n25
b="$1"; tag="$2"; shift 2
maps="${@:-tutorial layer1 level3 level4 level5 level06 level07 level08 level09 level10}"
for m in $maps; do
  echo "== $m"
  bash Tools/night/reel.sh "$b" "$tag" "walk-$m" 600 -skygrindMap $m -skygrindReelOnly levelwalk 2>&1 | grep -E "^\[reel\] levelwalk|Exception|timeout"
done
echo "walk $tag finished $(date +%H:%M:%S)"
