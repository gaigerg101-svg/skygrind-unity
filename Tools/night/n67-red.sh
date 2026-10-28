#!/usr/bin/env bash
# NIGHT67 (the lead's; FORGE66's copy of n67-red.sh, its red team's findings 1 and 3): the sim checks of every slot room (slots65.json, or
# $ROOMS) on one level, no Player; n67-round.sh runs it for each round and for the reds. Per room under <OUTDIR>/<id>/: room-check.txt (n59-l-room.py), flow-A.txt / flow-B.txt (n46flow seeds 1-10 / 11-20, the flags n59-l-contract.sh
# gives a candidate), pads.txt (hold52 --pads), plan.png (n52-svg-png.sh), flow-V.txt (n46flow --verbs65 seeds 1-10: the stall guard on the
# verb-aware pilot); a3 and b2 also ride.txt (n53ridekill ride). Every file opens with its
# command, the sha and the time (L-149). Runs the built SimHarness DLL and never builds it: run it after the flow base, never beside
# another dotnet build in this tree.
#   bash Tools/night/n67-red.sh [JOBS] [LEVEL] [OUTDIR]       defaults: 6, n65-foundry, Logs/n67/red
cd "$(git rev-parse --show-toplevel)" || exit 1
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
[ -f "$dll" ] || { echo "n67-red: no $dll"; exit 1; }
JOBS="${1:-6}"; LEVEL="${2:-n65-foundry}"; OUTDIR="${3:-Logs/n67/red}"
case "$LEVEL" in n52-*) PLAN="Design/levels/n52/${LEVEL#n52-}.plan.json" ;; *) PLAN="Design/levels/${LEVEL%%-*}/${LEVEL#*-}.plan.json" ;; esac
sha=$(git rev-parse --short HEAD); T0=$SECONDS
# NIGHT67 T1b (slate T1b line 1): a level of Tools/night/levels67.json other than level 1 (L67=<iceline|kiln|spire>, or read from LEVEL: the
# level's new id or its base id, e.g. the reds' `n67-red.sh 3 n52-iceline`): ROOMS default to the slate's rooms of that level; each room's
# verb from the slate (SLATE=<file>, n67-round.sh passes its snapshot; default the main tree's Logs/n67/slate.json) against the room row's
# verb65 (Tools/night/n67-verbs-of.cjs): flow-V plants it on a base room (--plant-verb65 <id>:<verb>), the ride pilot runs in every room whose
# verb is ride, and a verb65 / slate MISMATCH stops the red. Level 1 (L67 unset and LEVEL not a levels67 level, or L67=foundry): as before.
MAIN="$(cd "$(git rev-parse --git-common-dir)/.." && pwd -W)"
SLATE="${SLATE:-$MAIN/Logs/n67/slate.json}"
if [ -z "$L67" ]; then L67=$(node -e "const t=require('./Tools/night/levels67.json'); const k=Object.keys(t).find((k) => !k.startsWith('_') && k !== 'foundry' && (t[k].id === process.argv[1] || t[k].base === process.argv[1])); console.log(k || '')" "$LEVEL"); fi
[ "$L67" = foundry ] && L67=""
if [ -n "$L67" ]; then
  node Tools/night/gen-slots.cjs --level "$L67" --info=id > /dev/null || { echo "n67-red: L67=$L67 is not a level of Tools/night/levels67.json"; exit 1; }
  [ -f "$SLATE" ] || { echo "n67-red: no slate $SLATE"; exit 1; }
  [ -n "$ROOMS" ] || ROOMS=$(node Tools/night/n67-verbs-of.cjs "$SLATE" x --rooms-of="$L67")
  [ -n "$ROOMS" ] || { echo "n67-red: no ROOMS and the slate $SLATE has no room of level $L67"; exit 1; }
fi
ROOMS="${ROOMS:-$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")}"
head2() { echo "# command: $1"; echo "# sha: $sha; time: $(date '+%F %T')"; }
# NIGHT67 T1c: LEVEL_DIR (n67-round.sh: the round's Logs/n67/<tag>/, written by gen-n65-foundry.cjs --out) holds the level's three files
# (<px>-<level>.json, <level>.plan.json, openings<NN>.json): every instrument reads them there, none reads the tracked level files
#   n59-l-room.py --dir, n52-svg-png.sh <plan>, n46flow --file, hold52 --file, n53ridekill ride --level <file>
LF="Design/levels/$LEVEL.json"; DIRARG=()
if [ -n "$LEVEL_DIR" ]; then LF="$LEVEL_DIR/$LEVEL.json"; PLAN="$LEVEL_DIR/${LEVEL#*-}.plan.json"; DIRARG=(--dir "$LEVEL_DIR"); [ -f "$LF" ] && [ -f "$PLAN" ] || { echo "n67-red: LEVEL_DIR $LEVEL_DIR lacks $LF or $PLAN"; exit 1; }; fi
FILEARG=""; [ -n "$LEVEL_DIR" ] && FILEARG="--file $LF"
# NIGHT67 T1b: each room's verb (L67 levels): the verbs file (one line per room, saved with the red) and the plants for flow-V
PLANTS=""; RIDES="a3 b2"
if [ -n "$L67" ]; then
  mkdir -p "$OUTDIR"
  { head2 "node Tools/night/n67-verbs-of.cjs $SLATE $LF $ROOMS"; node Tools/night/n67-verbs-of.cjs "$SLATE" "$LF" $ROOMS; } > "$OUTDIR/red-verbs65.txt" 2>&1 || { echo "n67-red: REFUSED: a room's verb65 differs from the slate's verb ($OUTDIR/red-verbs65.txt):"; grep MISMATCH "$OUTDIR/red-verbs65.txt"; exit 1; }
  sed -n '3,$p' "$OUTDIR/red-verbs65.txt" | sed 's/^/  /'
  PLANTS=$(node Tools/night/n67-verbs-of.cjs "$SLATE" "$LF" $ROOMS --plants)
  RIDES=$(node Tools/night/n67-verbs-of.cjs "$SLATE" "$LF" $ROOMS --verb=ride | tr ',' ' ')
fi
plantOf() { local p; for p in ${PLANTS//,/ }; do [ "${p%%:*}" = "$1" ] && { echo "--plant-verb65 $p"; return; }; done; }

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
  printf '%s\t%s\n' "$d/flow-V.txt" "n46flow --level $LEVEL --rooms $r --seeds 10 --first 1 --verbs65 $FILEARG $(plantOf "$r")" >> "$JOBFILE"   # FORGE66 red team 3: stall on the verb-aware pilot (NIGHT67 T1b: the slate's verb planted on a base room)
done
for r in $RIDES; do case " $ROOMS " in *" $r "*) printf '%s\t%s\n' "$OUTDIR/$r/ride.txt" "n53ridekill ride --level $LF --room $r" >> "$JOBFILE" ;; esac; done   # NIGHT67 T1b: every room whose verb is ride (level 1: a3, b2)

while IFS=$'\t' read -r out args; do
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  { head2 "dotnet $dll $args"; dotnet "$dll" $args < /dev/null 2>&1; } > "$out" &
done < "$JOBFILE"
wait
echo "n67-red: $(wc -l < "$JOBFILE") jobs on $LEVEL in $((SECONDS - T0)) s; outputs under $OUTDIR/<room>/ ($(date +%T))"
for r in $ROOMS; do ls "$OUTDIR/$r" | tr '\n' ' ' | sed "s/^/  $r: /"; echo; done
