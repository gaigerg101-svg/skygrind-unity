#!/usr/bin/env bash
# NIGHT73 (n72-prune.sh's rule for named sides only, so a --keep side of the same tag survives a later set): keeps every 50th frame
# (f0000, f0050, ...) and every shot of the named sides under Logs/feel/<tag>/ and deletes the rest. Only this night's own output (n73-*).
#   bash Tools/night/n73-prune.sh <tag> <side> [<side> ...]      (no side: every side of the tag except those listed in Logs/feel/<tag>/keep.txt)
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:?tag}"; shift
case "$tag" in n73-*) ;; *) echo "refused: only n73-* tags"; exit 1 ;; esac
sides="$*"
if [ -z "$sides" ]; then
  sides=$(grep -E "^== [A-Za-z0-9-]+ [0-9]" "Logs/feel/$tag/summary47.txt" 2>/dev/null | awk '{print $2}' | sort -u)
  [ -f "Logs/feel/$tag/keep.txt" ] && sides=$(echo "$sides" | grep -v -x -F -f "Logs/feel/$tag/keep.txt")
fi
for s in $sides; do
  find "Logs/feel/$tag/$s" -type f -name 'f[0-9]*.jpg' ! -name 'f*00.jpg' ! -name 'f*50.jpg' -delete 2>/dev/null
done
echo "pruned $tag [$(echo $sides)] $(date +%T); C: $(df -BG /c | tail -1 | awk '{print $4}') free"
