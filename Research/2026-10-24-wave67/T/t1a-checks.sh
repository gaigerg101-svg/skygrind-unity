#!/usr/bin/env bash
# NIGHT67 builder T, T1a's proof runs (in series: one dotnet job in this tree): gen-slots --check per level and the level-1 cross-check against
# gen-n65-foundry.cjs. Each output file starts with its command, the tree's sha (+ dirty paths) and the time.
#   bash Research/2026-10-24-wave67/T/t1a-checks.sh [iceline kiln spire cross-old cross-latest cross-latest-run]
cd "$(git rev-parse --show-toplevel)" || exit 1
OUT=Research/2026-10-24-wave67/T
items="${*:-kiln iceline spire cross-old cross-latest cross-latest-run}"
for it in $items; do
  case "$it" in
    cross-old) cmd="node Tools/night/gen-slots.cjs --level foundry --cross-n65 --slots=old" ;;
    cross-latest) cmd="node Tools/night/gen-slots.cjs --level foundry --cross-n65 --slots=latest" ;;
    cross-latest-run) cmd="node Tools/night/gen-slots.cjs --level foundry --cross-n65 --slots=latest --run" ;;
    cross-old-run) cmd="node Tools/night/gen-slots.cjs --level foundry --cross-n65 --slots=old --run" ;;
    *) cmd="node Tools/night/gen-slots.cjs --level $it --check" ;;
  esac
  f="$OUT/t1a-$it.txt"
  { echo "# $cmd"; echo "# sha $(git rev-parse --short HEAD); dirty: $(git status --short -- Tools/night | tr '\n' ' ')"; echo "# start $(date '+%Y-%m-%d %H:%M:%S')"; } > "$f"
  timeout 1800 $cmd >> "$f" 2>&1
  echo "# exit $? at $(date '+%H:%M:%S')" >> "$f"
  tail -1 "$f"; grep -E "^(FAIL|gen-slots)" "$f" | tail -3
done
