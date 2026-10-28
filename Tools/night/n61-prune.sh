#!/usr/bin/env bash
# NIGHT61 (the forge FORGE61, 2026-10-22; a copy of n60-prune.sh with the n61 names). A reel set writes up to ~10 GB of frames (L-104);
# this keeps every 50th frame (f0000, f0050, ...) and every shot of each FINISHED reel side under Logs/feel/<tag>/ (a side is finished when
# a later side has started, or when summary47.txt says "all done") and deletes the rest. Only this night's own reel output (tags n61-*).
# n61-reels.sh runs it in a loop beside every set. The side pattern takes hyphenated names (L-113).
#   bash Tools/night/n61-prune.sh <tag> [--loop]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"
case "$tag" in n61-*) ;; *) echo "refused: only n61-* tags"; exit 1 ;; esac
prune() {
  local sum="Logs/feel/$tag/summary47.txt" sides last
  [ -f "$sum" ] || return
  sides=$(grep -E "^== [a-z0-9-]+ [0-9]" "$sum" | awk '{print $2}')
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
