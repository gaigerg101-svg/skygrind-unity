#!/usr/bin/env bash
# NIGHT32 (builder F): top-down renders of LEVEL 5 THE SPIRE (DESIGN32 rule 3): one SVG per room group into Design/levels/n32/sketches/ (level-svg.cjs), then a
# 1600 x 1200 PNG of each by headless Chrome into the directory given (default: Logs/shots/n32-spire). Usage, from the repo root:
#   bash Tools/night/n32-spire-render.sh [pngDir]
OUT="${1:-Logs/shots/n32-spire}"
mkdir -p Design/levels/n32/sketches "$OUT"
CHROME="/c/Program Files/Google/Chrome/Application/chrome.exe"
for g in v1,v2 v3,v4 v5 v6,v7 v8 v9 all; do
  f="n32-spire-${g//,/-}"
  if [ "$g" = all ]; then node Tools/night/level-svg.cjs Design/levels/n32/spire.plan.json "Design/levels/n32/sketches/$f.svg" --scale 4 || exit 1
  else node Tools/night/level-svg.cjs Design/levels/n32/spire.plan.json "Design/levels/n32/sketches/$f.svg" --rooms "$g" --scale 22 || exit 1; fi
  "$CHROME" --headless --disable-gpu --screenshot="$(cygpath -w "$OUT/$f.png")" --window-size=1600,1200 "file:///$(cygpath -m "$PWD/Design/levels/n32/sketches/$f.svg")" 2>/dev/null
  echo "rendered $OUT/$f.png"
done
