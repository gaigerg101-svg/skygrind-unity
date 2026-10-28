#!/usr/bin/env bash
# NIGHT74 builder S: one room's static guards and labels, after run-red.sh (forge71 brief R step 2: air65, skip v2, area lint, deco, patch share; the
# meter). Run it with run_in_background. Every output opens with its command, the sha and the time (L-149).
#   bash Research/2026-10-27-wave74/S/run-labels.sh <level iceline|kiln|spire> <room> <levelDir> <outDir> <roomFile> [skip]
# levelDir = the gen-slots.cjs --out folder (Windows-style path); outDir = evidence folder (relative to the tree); skip = also run skip v2 (about 3 min)
cd "$(git rev-parse --show-toplevel)" || exit 1
L="$1"; ROOM="$2"; LDIR="$3"; OUT="$4"; RFILE="$5"; SKIP="$6"
mkdir -p "$OUT"
sha=$(git rev-parse --short HEAD)
head2() { echo "# command: $1"; echo "# sha: $sha; time: $(date '+%F %T')"; }
{ head2 "node Tools/night/n65-air.cjs $LDIR/$L.plan.json $ROOM"; node Tools/night/n65-air.cjs "$LDIR/$L.plan.json" "$ROOM" 2>&1; } > "$OUT/air.txt"
{ head2 "node Tools/night/n70-patch-share.cjs --level $L --room $RFILE $ROOM"; node Tools/night/n70-patch-share.cjs --level "$L" --room "$RFILE" "$ROOM" 2>&1; } > "$OUT/patch.txt"
{ head2 "python -I Tools/night/n74-room-rules.py room $RFILE"; python -I Tools/night/n74-room-rules.py room "$RFILE" 2>&1 | sed -n '3,$p'; } > "$OUT/rules.txt"
TOUCH=""
if [ "$7" = throws ]; then   # a launch room: the 20-seed throw chart (n65-verbs.sh reads $LDIR/throws-<room>.txt for skip v2's M3)
  { head2 "bash Tools/night/n70-throws.sh $LDIR/n67-$L.json $ROOM $LDIR/throws-$ROOM.txt 20"; bash Tools/night/n70-throws.sh "$LDIR/n67-$L.json" "$ROOM" "$LDIR/throws-$ROOM.txt" 20 2>&1; } > "$OUT/throws-run.txt"
  cp "$LDIR/throws-$ROOM.txt" "$OUT/throws.txt" 2>/dev/null
fi
if [ "$SKIP" = skip ]; then
  { head2 "bash Tools/night/n65-verbs.sh skip $LDIR/n67-$L.json $ROOM --seeds 20 --bars <main tree>/Logs/n70/slate.json"; bash Tools/night/n65-verbs.sh skip "$LDIR/n67-$L.json" "$ROOM" --seeds 20 --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json 2>&1; } > "$OUT/skip.txt"
  TOUCH="--touches $OUT/skip.txt"
fi
{ head2 "node Tools/night/n70-area.cjs --level $L --dir $LDIR $TOUCH $ROOM"; node Tools/night/n70-area.cjs --level "$L" --dir "$LDIR" $TOUCH "$ROOM" 2>&1; } > "$OUT/area.txt"
{ head2 "L67=$L node Tools/night/n70-deco.cjs $ROOM $TOUCH --file $RFILE"; L67="$L" node Tools/night/n70-deco.cjs "$ROOM" $TOUCH --file "$RFILE" 2>&1; } > "$OUT/deco.txt"
echo "run-labels: $L $ROOM done $(date +%T)" >> "$OUT/labels-done.txt"
