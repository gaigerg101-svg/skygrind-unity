#!/usr/bin/env bash
# NIGHT36 builder L (NIGHT36.md L1 "look before it counts"): a top-down render of each room of an n36 level (Tools/night/level-svg.cjs: boxes shaded by
# height, ramps, lava, pits, lights, props, spawn, exit, nodes, proofs), then a PNG of each by headless Chrome (as n32-spire-render.sh).
# Usage, from the repo (or worktree) root:  bash Tools/night/n36-render.sh [name] [pngDir]   (default: foundry, Logs/n36-renders)
NAME="${1:-foundry}"
OUT="${2:-Logs/n36-renders}"
PLAN="Design/levels/n36/$NAME.plan.json"
SK="Logs/n36-renders/svg"
mkdir -p "$SK" "$OUT"
CHROME="/c/Program Files/Google/Chrome/Application/chrome.exe"
rooms=$(node -e "console.log(require('./$PLAN').rooms.map(r => r.id).join(' '))")
for g in $rooms all; do
  f="n36-$NAME-$g"
  if [ "$g" = all ]; then node Tools/night/level-svg.cjs "$PLAN" "$SK/$f.svg" --scale 5 || exit 1
  else node Tools/night/level-svg.cjs "$PLAN" "$SK/$f.svg" --rooms "$g" --scale 24 || exit 1; fi
  "$CHROME" --headless --disable-gpu --screenshot="$(cygpath -w "$PWD/$OUT/$f.png")" --window-size=1400,1100 "file:///$(cygpath -m "$PWD/$SK/$f.svg")" 2>/dev/null
  echo "rendered $OUT/$f.png"
done
