#!/usr/bin/env bash
# NIGHT66 (the forge FORGE62, 2026-10-22; a copy of n60-prune.sh with the n66 names). A reel set writes up to ~10 GB of frames (L-104);
# this keeps every 50th frame (f0000, f0050, ...) and every shot of each FINISHED reel side under Logs/feel/<tag>/ (a side is finished when
# a later side has started, or when summary47.txt says "all done") and deletes the rest. Only this night's own reel output (tags n66-*).
# n66-reels.sh runs it in a loop beside every set. The side pattern takes hyphenated names (L-113) and capitals (FORGE64: NIGHT63-LOG.md "Disk").
#   bash Tools/night/n66-prune.sh <tag> [--loop]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"
case "$tag" in n66-*) ;; *) echo "refused: only n66-* tags"; exit 1 ;; esac
prune() {
  local sum="Logs/feel/$tag/summary47.txt" sides last
  [ -f "$sum" ] || return
  sides=$(grep -E "^== [A-Za-z0-9-]+ [0-9]" "$sum" | awk '{print $2}')
  last=$(echo "$sides" | tail -1)
  grep -q "^== all done" "$sum" && last=""
  for s in $sides; do
    [ "$s" = "$last" ] && continue
    find "Logs/feel/$tag/$s" -type f -name 'f[0-9]*.jpg' ! -name 'f*00.jpg' ! -name 'f*50.jpg' -delete 2>/dev/null
  done
}
if [ "$2" = "--loop" ]; then
  # FORGE62b (red team 2h, finding 13): a tag holds several sets; stop on a NEW "== all done" line, not on an earlier set's
  done0=$(grep -c "^== all done" "Logs/feel/$tag/summary47.txt" 2>/dev/null); done0=${done0:-0}
  while :; do prune; d=$(grep -c "^== all done" "Logs/feel/$tag/summary47.txt" 2>/dev/null); [ "${d:-0}" -gt "$done0" ] && { prune; break; }; sleep 20; done
else prune; fi
echo "pruned $tag $(date +%T); C: $(df -BG /c | tail -1 | awk '{print $4}') free"
