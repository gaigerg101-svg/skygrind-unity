#!/usr/bin/env bash
# NIGHT74 R: every level-1 room guard of forge71 brief section R on one room file, in one run (level 1 has no slate: no SLATE=, no L67).
#   bash Research/2026-10-27-wave74/R/room-guards.sh <room> <tag> <slots> [verb]
#       slots: the full list the level is generated with (b1,a8,<room> for a room on top of GG's two)
#       verb:  pierce | ride | launch | parry | skip-only (the n65verb pilot to run besides skip v2; default none)
# Writes under Research/2026-10-27-wave74/R/evidence/<room>/<tag>/<room>/ (small text outputs; the generated level stays in the scratchpad).
# Order: gen (--out), n70-red.sh (room check, flow A / B / V, pads), n65-air, area (with the skip's touches when skip ran), deco, the verb pilot, skip v2.
cd "$(git rev-parse --show-toplevel)" || exit 1
room="$1"; tag="$2"; slots="$3"; verb="$4"
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/$room"
E="Research/2026-10-27-wave74/R/evidence/$room"
mkdir -p "$SC" "$E"
sha=$(git rev-parse --short HEAD)
head2() { echo "# command: $1"; echo "# sha: $sha (+ the room file as in the tree); time: $(date '+%F %T')"; }
node Tools/night/gen-n65-foundry.cjs --run --slots="$slots" --out="$SC/$tag" > "$E/gen-$tag.txt" 2>&1 || { echo "gen failed: $E/gen-$tag.txt"; tail -6 "$E/gen-$tag.txt" | cut -c1-300; exit 1; }
tail -3 "$E/gen-$tag.txt" | cut -c1-200
ROOMS="$room" LEVEL_DIR="$SC/$tag" bash Tools/night/n70-red.sh 6 n65-foundry "$E/$tag" > "$E/$tag-run.txt" 2>&1
O="$E/$tag/$room"
{ head2 "node Tools/night/n65-air.cjs $SC/$tag/foundry.plan.json $room"; node Tools/night/n65-air.cjs "$SC/$tag/foundry.plan.json" "$room" 2>&1; } > "$O/air.txt"
if [ "$verb" = launch ]; then
  # launch rooms: the 20-seed throw chart first (n65-verbs.sh's M3 reads throws-<room>.txt beside the level file); kept in the evidence too
  bash Tools/night/n70-throws.sh "$SC/$tag/n65-foundry.json" "$room" "$SC/$tag/throws-$room.txt" 20 > /dev/null 2>&1
  cp "$SC/$tag/throws-$room.txt" "$O/throws.txt" 2>/dev/null
fi
if [ -n "$verb" ] && [ "$verb" != skip-only ]; then
  { head2 "bash Tools/night/n65-verbs.sh $verb $SC/$tag/n65-foundry.json $room"; bash Tools/night/n65-verbs.sh "$verb" "$SC/$tag/n65-foundry.json" "$room" 2>&1; } > "$O/$verb.txt"
fi
{ head2 "bash Tools/night/n65-verbs.sh skip $SC/$tag/n65-foundry.json $room --seeds 20"; bash Tools/night/n65-verbs.sh skip "$SC/$tag/n65-foundry.json" "$room" --seeds 20 2>&1; } > "$O/skip.txt"
{ head2 "node Tools/night/n70-area.cjs --level foundry --dir $SC/$tag --touches $O/skip.txt $room"; node Tools/night/n70-area.cjs --level foundry --dir "$SC/$tag" --touches "$O/skip.txt" "$room" 2>&1; } > "$O/area.txt"
{ head2 "L67=foundry node Tools/night/n70-deco.cjs $room --touches $O/skip.txt --file Design/levels/n65/rooms/$room.json"; L67=foundry node Tools/night/n70-deco.cjs "$room" --touches "$O/skip.txt" --file "Design/levels/n65/rooms/$room.json" 2>&1; } > "$O/deco.txt"
echo "== $room $tag ($slots)"
grep -E "PASS|FAIL|n/c|ROOM" "$O/room-check.txt" | cut -c1-200
for f in flow-A flow-B flow-V; do echo "-- $f"; grep -E "SUMMARY|STYLE: |verb65 " "$O/$f.txt" | cut -c1-260; done
grep -E "^n65-air" "$O/air.txt" | cut -c1-260
grep -E "launched|PASS|FAIL" "$O/pads.txt" | cut -c1-200
[ -f "$O/$verb.txt" ] && grep -E "^n65verb $verb:" "$O/$verb.txt" | cut -c1-260
grep -E "^n65verb skip: M" "$O/skip.txt" | cut -c1-330
grep -E "^n70-area:|^OPEN|^P[0-9]+ " "$O/area.txt" | cut -c1-200
tail -1 "$O/deco.txt" | cut -c1-200
