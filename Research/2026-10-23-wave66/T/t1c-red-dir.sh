#!/usr/bin/env bash
# NIGHT66 T1c: n66-red.sh on one room (b1) with LEVEL_DIR = a scratch round folder made by gen --out (the latest assembly): every instrument
# reads the folder's level / plan / openings; the tracked level files' status before and after; each output's head (the file it read).
#   bash Research/2026-10-23-wave66/T/t1c-red-dir.sh <the round folder, C:/...> <out dir, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
LD="${1:?round folder}"; OD="${2:?out dir}"
TR="Design/levels Assets/StreamingAssets/Design/levels Reports"
echo "tracked before: [$(git status --short $TR | tr '\n' ';')]"
ROOMS=b1 LEVEL_DIR="$LD" bash Tools/night/n66-red.sh 4 n65-foundry "$OD" 2>&1 | tail -3
echo "tracked after:  [$(git status --short $TR | tr '\n' ';')]"
for f in room-check.txt flow-A.txt flow-B.txt flow-V.txt pads.txt; do echo "== $f: $(head -1 "$OD/b1/$f" | cut -c1-240)"; grep -m2 -E "^ROOM b1|SUMMARY|--file|pads|n65-foundry: " "$OD/b1/$f" | cut -c1-200; done
ls -la "$OD/b1/plan.png" 2>/dev/null | awk '{print "== plan.png " $5 " bytes"}'
echo "== the same room check on the TRACKED level (all-old b1) for contrast: $(python -I Tools/night/n59-l-room.py n65-foundry room b1 | tail -1)"
