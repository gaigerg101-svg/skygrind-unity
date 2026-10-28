#!/usr/bin/env bash
# NIGHT74 builder S: one room's red chain, no Player (forge71 brief R step 2; Tools/night/n70-red.sh). Run it with run_in_background.
#   bash Research/2026-10-27-wave74/S/run-red.sh <level n67-iceline|n67-kiln|n67-spire> <room> <levelDir> <outDir> [JOBS]
# levelDir = the folder gen-slots.cjs --run --out wrote (Windows-style path); outDir = where n70-red.sh writes (relative to the tree).
cd "$(git rev-parse --show-toplevel)" || exit 1
LEVEL="$1"; ROOM="$2"; LDIR="$3"; OUT="$4"; JOBS="${5:-4}"
mkdir -p "$OUT"
SLATE=Research/2026-10-27-wave74/S/slate-s.json LEVEL_DIR="$LDIR" ROOMS="$ROOM" bash Tools/night/n70-red.sh "$JOBS" "$LEVEL" "$OUT" > "$OUT-run.txt" 2>&1
echo "run-red: $LEVEL $ROOM $LDIR -> $OUT rc=$? ($(date +%T))" >> "$OUT-run.txt"
