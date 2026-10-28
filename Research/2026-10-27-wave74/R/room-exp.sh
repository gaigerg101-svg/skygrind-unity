#!/usr/bin/env bash
# NIGHT74 R: measure a room file variant without committing it: swap the variant in, generate the level into the scratchpad (--out), put the
# file back, run n46flow on the generated level (walk pilot, seeds 1-10 and 11-20, or the verb pilot with V=1).
#   bash Research/2026-10-27-wave74/R/room-exp.sh <room> <tag> <variant.json> [slots]       slots default b1,a8,<room>
# Prints the room's flow lines. Never writes a tracked file (the room file is restored from a copy; check git status after).
cd "$(git rev-parse --show-toplevel)" || exit 1
room="$1"; tag="$2"; var="$3"; slots="${4:-b1,a8,$room}"
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/$room"
mkdir -p "$SC"
F="Design/levels/n65/rooms/$room.json"
cp "$F" "$SC/$room.restore.json" || exit 1
cp "$var" "$F"
node Tools/night/gen-n65-foundry.cjs --run --slots="$slots" --out="$SC/$tag" > "$SC/$tag.gen.txt" 2>&1; rc=$?
cp "$SC/$room.restore.json" "$F"
[ $rc -eq 0 ] || { echo "gen failed ($rc): $SC/$tag.gen.txt"; tail -5 "$SC/$tag.gen.txt" | cut -c1-300; exit 1; }
DLL=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
pyroom() { python -I Tools/night/n59-l-room.py n65-foundry room "$room" --dir "$SC/$tag" 2>&1 | grep -E "PASS|FAIL|n/c|ROOM" | cut -c1-200; }
echo "== $room $tag"
pyroom
for first in 1 11; do
  extra=""; [ "$first" = 11 ] && extra="--no-geo"
  [ -n "$V" ] && extra="$extra --verbs65"
  dotnet "$DLL" n46flow --level n65-foundry --rooms "$room" --seeds 10 --first $first $extra --file "$SC/$tag/n65-foundry.json" < /dev/null 2>&1 | grep -E "^  $room |SUMMARY|STYLE: |verb65 " | cut -c1-260
done
