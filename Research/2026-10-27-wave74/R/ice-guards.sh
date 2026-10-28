#!/usr/bin/env bash
# NIGHT74 R: every iceline (level 2) room guard of forge71 brief section R on one room file, in one run (the L67 pipeline: gen-slots, a slate with the room's verb).
#   bash Research/2026-10-27-wave74/R/ice-guards.sh <room> <tag> <slots> <verb> [pilot-flags]
#       slots: the list the level is generated with (old = every room's base; g3 = that room latest, the rest base)
#       verb:  the room's verb65 for the slate (ride | launch | pierce | parry | none): ride runs `n65-verbs.sh ride --shoot`
# Writes under Research/2026-10-27-wave74/R/evidence/<room>/<tag>/<room>/ (small text outputs; the generated level stays in the scratchpad).
# Order: gen (--out), n70-red.sh (room check, flow A / B / V, pads, ride), n65-air, the verb pilot, skip v2, area with the skip's touches, deco,
# patch share, the rules meter. The slate is Research/2026-10-27-wave74/R/slate-r.json (id, level, verb), as NIGHT71's brief says for a room no slate holds.
cd "$(git rev-parse --show-toplevel)" || exit 1
room="$1"; tag="$2"; slots="$3"; verb="$4"
SC="C:/Users/gaige/AppData/Local/Temp/claude/C--Users-gaige-dev-SKYGRIND-unity/07ef7707-bc3a-4541-8901-f98169a14875/scratchpad/ice/$room"
E="Research/2026-10-27-wave74/R/evidence/$room"
SLATE="Research/2026-10-27-wave74/R/slate-r.json"
mkdir -p "$SC" "$E"
sha=$(git rev-parse --short HEAD)
head2() { echo "# command: $1"; echo "# sha: $sha (+ the room file as in the tree); time: $(date '+%F %T')"; }
node Tools/night/gen-slots.cjs --level iceline --slots="$slots" --run --out="$SC/$tag" > "$E/gen-$tag.txt" 2>&1 || { echo "gen failed: $E/gen-$tag.txt"; tail -6 "$E/gen-$tag.txt" | cut -c1-300; exit 1; }
tail -3 "$E/gen-$tag.txt" | cut -c1-220
L="$SC/$tag/n67-iceline.json"
SLATE="$SLATE" ROOMS="$room" LEVEL_DIR="$SC/$tag" bash Tools/night/n70-red.sh 6 n67-iceline "$E/$tag" > "$E/$tag-run.txt" 2>&1
O="$E/$tag/$room"
{ head2 "node Tools/night/n65-air.cjs $SC/$tag/iceline.plan.json $room"; node Tools/night/n65-air.cjs "$SC/$tag/iceline.plan.json" "$room" 2>&1; } > "$O/air.txt"
if [ "$verb" = ride ]; then
  { head2 "bash Tools/night/n65-verbs.sh ride $L $room --shoot"; bash Tools/night/n65-verbs.sh ride "$L" "$room" --shoot 2>&1; } > "$O/ride-pilot.txt"
elif [ -n "$verb" ] && [ "$verb" != none ]; then
  { head2 "bash Tools/night/n65-verbs.sh $verb $L $room"; bash Tools/night/n65-verbs.sh "$verb" "$L" "$room" 2>&1; } > "$O/$verb.txt"
fi
{ head2 "bash Tools/night/n65-verbs.sh skip $L $room --seeds 20"; bash Tools/night/n65-verbs.sh skip "$L" "$room" --seeds 20 2>&1; } > "$O/skip.txt"
{ head2 "node Tools/night/n70-area.cjs --level iceline --dir $SC/$tag --touches $O/skip.txt $room"; node Tools/night/n70-area.cjs --level iceline --dir "$SC/$tag" --touches "$O/skip.txt" "$room" 2>&1; } > "$O/area.txt"
{ head2 "L67=iceline node Tools/night/n70-deco.cjs $room --touches $O/skip.txt --file Design/levels/n67/iceline/rooms/$room.json"; L67=iceline node Tools/night/n70-deco.cjs "$room" --touches "$O/skip.txt" --file "Design/levels/n67/iceline/rooms/$room.json" 2>&1; } > "$O/deco.txt"
{ head2 "node Tools/night/n70-patch-share.cjs --level iceline --room Design/levels/n67/iceline/rooms/$room.json $room"; node Tools/night/n70-patch-share.cjs --level iceline --room "Design/levels/n67/iceline/rooms/$room.json" "$room" 2>&1; } > "$O/patch-share.txt"
{ head2 "python -I Tools/night/n74-room-rules.py room Design/levels/n67/iceline/rooms/$room.json"; python -I Tools/night/n74-room-rules.py room "Design/levels/n67/iceline/rooms/$room.json" 2>&1; } > "$O/rules.txt"
echo "== $room $tag ($slots)"
grep -E "PASS|FAIL|n/c|ROOM" "$O/room-check.txt" | cut -c1-200
for f in flow-A flow-B flow-V; do echo "-- $f"; grep -E "SUMMARY|STYLE: |verb65 " "$O/$f.txt" | cut -c1-260; done
grep -E "^n65-air" "$O/air.txt" | cut -c1-260
grep -E "pad52" "$O/pads.txt" | cut -c1-200
grep -E "^n53ridekill ride:" "$O/ride.txt" 2>/dev/null | cut -c1-300
grep -E "^n65verb (ride|launch|pierce|parry)[^:]*:" "$O/ride-pilot.txt" "$O/$verb.txt" 2>/dev/null | cut -c1-300
grep -E "^n65verb skip: M" "$O/skip.txt" | cut -c1-330
grep -E "^n70-area:|^OPEN|^P[0-9]+ " "$O/area.txt" | cut -c1-200
tail -1 "$O/deco.txt" | cut -c1-200
tail -2 "$O/patch-share.txt" | cut -c1-200
grep -E "core" "$O/rules.txt"
