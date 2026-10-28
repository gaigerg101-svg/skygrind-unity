#!/usr/bin/env bash
# NIGHT46 (the lead): resolve a docs/ROOMS.md cherry-pick conflict where both sides appended blocks under "## NIGHT46 identity cards":
# keep both sides, drop the markers and the second copy of the section heading, then continue the pick and log it.
#   bash Tools/night/n46-rooms-resolve.sh <builder>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
b="${1:?builder}"
c=$(git rev-parse CHERRY_PICK_HEAD) || exit 1
awk 'BEGIN{inb=0} /^<<<<<<< /{next} /^=======$/{inb=1; next} /^>>>>>>> /{inb=0; next}
     inb && /^## NIGHT46 identity cards/ {inb=2; next} {print}' docs/ROOMS.md > docs/ROOMS.md.tmp && mv docs/ROOMS.md.tmp docs/ROOMS.md
n=$(grep -c '^<<<<<<<\|^=======$\|^>>>>>>>' docs/ROOMS.md); h=$(grep -c '^## NIGHT46 identity cards' docs/ROOMS.md)
[ "$n" = 0 ] && [ "$h" = 1 ] || { echo "resolve: markers $n, headings $h: fix by hand"; exit 1; }
git status --short | grep -E '^(UU|AA)' | grep -v docs/ROOMS.md && { echo "other conflicts"; exit 1; }
git add docs/ROOMS.md && GIT_EDITOR=true git cherry-pick --continue >/dev/null && echo "$c $b" >> Logs/night46-picked.txt && echo "resolved $c"
