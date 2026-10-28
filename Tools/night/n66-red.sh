#!/usr/bin/env bash
# NIGHT66 (the lead's; FORGE66's copy of n66-red.sh, its red team's findings 1 and 3): the sim checks of every slot room (slots65.json, or
# $ROOMS) on one level, no Player; n66-round.sh runs it for each round and for the reds. Per room under <OUTDIR>/<id>/: room-check.txt (n59-l-room.py), flow-A.txt / flow-B.txt (n46flow seeds 1-10 / 11-20, the flags n59-l-contract.sh
# gives a candidate), pads.txt (hold52 --pads), plan.png (n52-svg-png.sh), flow-V.txt (n46flow --verbs65 seeds 1-10: the stall guard on the
# verb-aware pilot); a3 and b2 also ride.txt (n53ridekill ride). Every file opens with its
# command, the sha and the time (L-149). Runs the built SimHarness DLL and never builds it: run it after the flow base, never beside
# another dotnet build in this tree.
#   bash Tools/night/n66-red.sh [JOBS] [LEVEL] [OUTDIR]       defaults: 6, n65-foundry, Logs/n66/red
cd "$(git rev-parse --show-toplevel)" || exit 1
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
[ -f "$dll" ] || { echo "n66-red: no $dll"; exit 1; }
JOBS="${1:-6}"; LEVEL="${2:-n65-foundry}"; OUTDIR="${3:-Logs/n66/red}"
case "$LEVEL" in n52-*) PLAN="Design/levels/n52/${LEVEL#n52-}.plan.json" ;; *) PLAN="Design/levels/${LEVEL%%-*}/${LEVEL#*-}.plan.json" ;; esac
sha=$(git rev-parse --short HEAD); T0=$SECONDS
ROOMS="${ROOMS:-$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")}"
head2() { echo "# command: $1"; echo "# sha: $sha; time: $(date '+%F %T')"; }
# NIGHT66 T1c: LEVEL_DIR (n66-round.sh: the round's Logs/n66/<tag>/, written by gen-n65-foundry.cjs --out) holds the level's three files
# (<px>-<level>.json, <level>.plan.json, openings<NN>.json): every instrument reads them there, none reads the tracked level files
#   n59-l-room.py --dir, n52-svg-png.sh <plan>, n46flow --file, hold52 --file, n53ridekill ride --level <file>
LF="Design/levels/$LEVEL.json"; DIRARG=()
if [ -n "$LEVEL_DIR" ]; then LF="$LEVEL_DIR/$LEVEL.json"; PLAN="$LEVEL_DIR/${LEVEL#*-}.plan.json"; DIRARG=(--dir "$LEVEL_DIR"); [ -f "$LF" ] && [ -f "$PLAN" ] || { echo "n66-red: LEVEL_DIR $LEVEL_DIR lacks $LF or $PLAN"; exit 1; }; fi
FILEARG=""; [ -n "$LEVEL_DIR" ] && FILEARG="--file $LF"

for r in $ROOMS; do
  d="$OUTDIR/$r"; mkdir -p "$d"
  { head2 "python -I Tools/night/n59-l-room.py $LEVEL room $r ${DIRARG[*]}"; python -I Tools/night/n59-l-room.py "$LEVEL" room "$r" "${DIRARG[@]}" 2>&1; } > "$d/room-check.txt"
  bash Tools/night/n52-svg-png.sh "$PLAN" "$r" "$d/plan.png" > /dev/null 2>&1
done

JOBFILE="$OUTDIR/jobs.txt"; : > "$JOBFILE"
for r in $ROOMS; do
  d="$OUTDIR/$r"
  printf '%s\t%s\n' "$d/flow-A.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 1 $FILEARG" >> "$JOBFILE"
  printf '%s\t%s\n' "$d/flow-B.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 11 --no-geo $FILEARG" >> "$JOBFILE"
  printf '%s\t%s\n' "$d/pads.txt" "hold52 --level $LEVEL --room $r --pads $FILEARG" >> "$JOBFILE"
  printf '%s\t%s\n' "$d/flow-V.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 1 --verbs65 $FILEARG" >> "$JOBFILE"   # FORGE66 red team 3: stall on the verb-aware pilot
done
for r in a3 b2; do case " $ROOMS " in *" $r "*) printf '%s\t%s\n' "$OUTDIR/$r/ride.txt" "n53ridekill ride --level $LF --room $r" >> "$JOBFILE" ;; esac; done

while IFS=$'\t' read -r out args; do
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  { head2 "dotnet $dll $args"; dotnet "$dll" $args < /dev/null 2>&1; } > "$out" &
done < "$JOBFILE"
wait
echo "n66-red: $(wc -l < "$JOBFILE") jobs on $LEVEL in $((SECONDS - T0)) s; outputs under $OUTDIR/<room>/ ($(date +%T))"
for r in $ROOMS; do ls "$OUTDIR/$r" | tr '\n' ' ' | sed "s/^/  $r: /"; echo; done
