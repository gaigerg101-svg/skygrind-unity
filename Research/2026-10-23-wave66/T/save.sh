#!/usr/bin/env bash
# NIGHT66 T: run a command from the worktree root and save its output under Research/2026-10-23-wave66/T/<name>.txt with the command, the sha
# and the time at its top (L-149). Usage: bash Research/2026-10-23-wave66/T/save.sh <name> <command...>; prints the file's tail and the exit.
cd "$(git rev-parse --show-toplevel)" || exit 1
name="$1"; shift
out="Research/2026-10-23-wave66/T/$name.txt"
{
  echo "# command: $*"
  echo "# sha: $(git rev-parse --short HEAD) (tree $(git status --short | wc -l) dirty paths), $(date '+%Y-%m-%d %H:%M:%S %Z'), $(pwd)"
  echo
} > "$out"
export MSBUILDDISABLENODEREUSE=1
"$@" >> "$out" 2>&1
rc=$?
echo "# exit $rc, $(date '+%H:%M:%S')" >> "$out"
echo "saved $out (exit $rc)"
exit $rc
