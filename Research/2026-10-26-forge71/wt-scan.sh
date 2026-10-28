#!/usr/bin/env bash
# Read-only scan of the repo's git worktrees: path, branch, HEAD date, dirty count (tracked changes), untracked count.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
git worktree list --porcelain | awk '/^worktree /{print substr($0,10)}' | while read -r wt; do
  [ "$wt" = "C:/Users/gaige/dev/SKYGRIND-unity" ] && continue
  [ -d "$wt" ] || { echo "MISSING	$wt"; continue; }
  br=$(git -C "$wt" branch --show-current 2>/dev/null); [ -z "$br" ] && br="(detached)"
  d=$(git -C "$wt" log -1 --format=%cs 2>/dev/null)
  mod=$(git -C "$wt" status --porcelain --untracked-files=no 2>/dev/null | wc -l)
  unt=$(git -C "$wt" status --porcelain 2>/dev/null | grep -c '^??')
  echo "$d	$mod	$unt	$br	$wt"
done
