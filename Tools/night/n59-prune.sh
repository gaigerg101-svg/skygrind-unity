#!/usr/bin/env bash
# NIGHT59 (the lead; a copy of n57-prune.sh with the n58 names): the disk is shared and near full (C: 14 GB at NIGHT59's t0; a reel set
# writes up to ~10 GB of frames and reels refuse under 10 GB silently, L-104). Keeps every 50th frame (f0000, f0050, ...) of each FINISHED
# reel side under Logs/feel/<tag>/ (a side is finished when a later side has started, or when summary47.txt says "all done"), deletes the
# rest. Only this night's own reel output (tags n59-*). Run it after EVERY reel batch, or in a loop beside one.
#   bash Tools/night/n59-prune.sh <tag> [--loop]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"
case "$tag" in n59-*) ;; *) echo "refused: only n59-* tags"; exit 1 ;; esac
prune() {
  local sum="Logs/feel/$tag/summary47.txt" sides last
  [ -f "$sum" ] || return
  sides=$(grep -E "^== [a-z0-9]+ [0-9]" "$sum" | awk '{print $2}')
  last=$(echo "$sides" | tail -1)
  grep -q "^== all done" "$sum" && last=""
  for s in $sides; do
    [ "$s" = "$last" ] && continue
    find "Logs/feel/$tag/$s" -type f -name 'f[0-9]*.jpg' ! -name 'f*00.jpg' ! -name 'f*50.jpg' -delete 2>/dev/null
  done
}
if [ "$2" = "--loop" ]; then
  while :; do prune; grep -q "^== all done" "Logs/feel/$tag/summary47.txt" 2>/dev/null && { prune; break; }; sleep 20; done
else prune; fi
echo "pruned $tag $(date +%T); C: $(df -BG /c | tail -1 | awk '{print $4}') free"
