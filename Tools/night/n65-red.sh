#!/usr/bin/env bash
# NIGHT65 (the lead's; NIGHT65.md section 1 step 6): the sim red of the six rebuilt slots on n52-foundry, no Player. Per room under
# Logs/n65/red/<id>/: room-check.txt (n59-l-room.py), flow-A.txt / flow-B.txt (n46flow seeds 1-10 / 11-20, the flags n59-l-contract.sh
# gives a candidate), pads.txt (hold52 --pads), plan.png (n52-svg-png.sh); a3 also ride.txt (n53ridekill ride). Every file opens with its
# command, the sha and the time (L-149). Runs the built SimHarness DLL and never builds it: run it after the flow base, never beside
# another dotnet build in this tree.
#   bash Tools/night/n65-red.sh [JOBS] [LEVEL] [OUTDIR]       defaults: 6, n52-foundry, Logs/n65/red
cd "$(git rev-parse --show-toplevel)" || exit 1
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
[ -f "$dll" ] || { echo "n65-red: no $dll"; exit 1; }
JOBS="${1:-6}"; LEVEL="${2:-n52-foundry}"; OUTDIR="${3:-Logs/n65/red}"
case "$LEVEL" in n52-*) PLAN="Design/levels/n52/${LEVEL#n52-}.plan.json" ;; *) PLAN="Design/levels/${LEVEL%%-*}/${LEVEL#*-}.plan.json" ;; esac
sha=$(git rev-parse --short HEAD); T0=$SECONDS
ROOMS="a2 a3 a5 b1 a8 a4"
head2() { echo "# command: $1"; echo "# sha: $sha; time: $(date '+%F %T')"; }

for r in $ROOMS; do
  d="$OUTDIR/$r"; mkdir -p "$d"
  { head2 "python -I Tools/night/n59-l-room.py $LEVEL room $r"; python -I Tools/night/n59-l-room.py "$LEVEL" room "$r" 2>&1; } > "$d/room-check.txt"
  bash Tools/night/n52-svg-png.sh "$PLAN" "$r" "$d/plan.png" > /dev/null 2>&1
done

JOBFILE="$OUTDIR/jobs.txt"; : > "$JOBFILE"
for r in $ROOMS; do
  d="$OUTDIR/$r"
  printf '%s\t%s\n' "$d/flow-A.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 1" >> "$JOBFILE"
  printf '%s\t%s\n' "$d/flow-B.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 11 --no-geo" >> "$JOBFILE"
  printf '%s\t%s\n' "$d/pads.txt" "hold52 --level $LEVEL --room $r --pads" >> "$JOBFILE"
done
printf '%s\t%s\n' "$OUTDIR/a3/ride.txt" "n53ridekill ride --level Design/levels/$LEVEL.json --room a3" >> "$JOBFILE"

while IFS=$'\t' read -r out args; do
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  { head2 "dotnet $dll $args"; dotnet "$dll" $args < /dev/null 2>&1; } > "$out" &
done < "$JOBFILE"
wait
echo "n65-red: $(wc -l < "$JOBFILE") jobs on $LEVEL in $((SECONDS - T0)) s; outputs under $OUTDIR/<room>/ ($(date +%T))"
for r in $ROOMS; do ls "$OUTDIR/$r" | tr '\n' ' ' | sed "s/^/  $r: /"; echo; done
