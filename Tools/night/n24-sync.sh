#!/usr/bin/env bash
# NIGHT24: the lead's copy of the Design files to the Player's StreamingAssets (what SKYGRIND/Sync Port Data does for them), then cmp.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json
for s in levels bosses maps; do
  mkdir -p Assets/StreamingAssets/Design/$s
  for f in Design/$s/*.json; do case $f in *.plan.json) ;; *) cp "$f" Assets/StreamingAssets/Design/$s/;; esac; done
done
cmp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "config copies identical"
for s in levels bosses maps; do for f in Design/$s/*.json; do case $f in *.plan.json) ;; *) cmp -s "$f" "Assets/StreamingAssets/Design/$s/$(basename $f)" || echo "DIFF $f";; esac; done; done
