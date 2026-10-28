#!/usr/bin/env bash
# NIGHT74 R: the quick static read of a level-1 room-file variant (no sim, a second): swap it in, write the PLAN only (--out, no levelgen), put the
# file back, print the n65-air line and the rules meter's verdict lines. Never writes a tracked file.
#   bash Research/2026-10-27-wave74/R/room-plan.sh <room> <variant.json> [slots]       slots default b1,a8,a2,a5,<room>
cd "$(git rev-parse --show-toplevel)" || exit 1
room="$1"; var="$2"; slots="${3:-b1,a8,a2,a5,$room}"
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/$room"
mkdir -p "$SC"
F="Design/levels/n65/rooms/$room.json"
cp "$F" "$SC/$room.restore.json" || exit 1
cp "$var" "$F"
node Tools/night/gen-n65-foundry.cjs --slots="$slots" --out="$SC/planonly" > "$SC/planonly.txt" 2>&1; rc=$?
cp "$SC/$room.restore.json" "$F"
[ $rc -eq 0 ] || { echo "plan failed ($rc)"; tail -4 "$SC/planonly.txt" | cut -c1-300; exit 1; }
node Tools/night/n65-air.cjs "$SC/planonly/foundry.plan.json" "$room" 2>&1 | grep -E "^n65-air $room" | cut -c1-260
python -I Tools/night/n74-room-rules.py room "$var" 2>&1 | grep -E "PLANES|COVER|LOOPS|REAR|VIEW|CROSS|EXPOSE|CLIMB|core" | cut -c1-200
