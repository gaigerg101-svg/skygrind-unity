#!/usr/bin/env bash
# NIGHT74 R: measure an iceline room-file variant without committing it: swap it in, generate the level into the scratchpad (--out), put the file
# back, run n46flow on the generated level: the walk pilot seeds 1-10 (A) and 11-20 (B) and the verb-aware pilot (V) on 1-10.
#   bash Research/2026-10-27-wave74/R/ice-exp.sh <room> <tag> <variant.json> <slots> [abv]       abv: which of A / B / V to run (default AV)
cd "$(git rev-parse --show-toplevel)" || exit 1
room="$1"; tag="$2"; var="$3"; slots="$4"; which="${5:-AV}"
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/ice/$room"
mkdir -p "$SC"
F="Design/levels/n67/iceline/rooms/$room.json"
cp "$F" "$SC/$room.restore.json" || exit 1
cp "$var" "$F"
node Tools/night/gen-slots.cjs --level iceline --slots="$slots" --run --out="$SC/$tag" > "$SC/$tag.gen.txt" 2>&1; rc=$?
cp "$SC/$room.restore.json" "$F"
[ $rc -eq 0 ] || { echo "gen failed ($rc): $SC/$tag.gen.txt"; tail -5 "$SC/$tag.gen.txt" | cut -c1-300; exit 1; }
DLL=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
L="$SC/$tag/n67-iceline.json"
echo "== $room $tag"
python -I Tools/night/n74-room-rules.py room "$var" 2>&1 | grep -E "core"
case "$which" in *A*) dotnet "$DLL" n46flow --level n67-iceline --rooms "$room" --seeds 10 --first 1 --no-geo --file "$L" < /dev/null 2>&1 | grep -E "SUMMARY|STYLE: " | cut -c1-250 | sed 's/^/A /';; esac
case "$which" in *B*) dotnet "$DLL" n46flow --level n67-iceline --rooms "$room" --seeds 10 --first 11 --no-geo --file "$L" < /dev/null 2>&1 | grep -E "SUMMARY|STYLE: " | cut -c1-250 | sed 's/^/B /';; esac
case "$which" in *V*) dotnet "$DLL" n46flow --level n67-iceline --rooms "$room" --seeds 10 --first 1 --verbs65 --no-geo --file "$L" < /dev/null 2>&1 | grep -E "SUMMARY|STYLE: " | cut -c1-250 | sed 's/^/V /';; esac
