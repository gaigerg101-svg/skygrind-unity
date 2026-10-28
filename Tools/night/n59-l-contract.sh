#!/usr/bin/env bash
# NIGHT59 builder L (L1): Tools/night/n57-l-contract.sh with the LEVEL as an argument (n57's hard-coded 'kiln', its plan, its report, its
# row and its slots file). THE ROOM CONTRACT (Tools/night/n52-contract-all.sh's jobs and table, Tools/night/n52-room-check.py unchanged)
# on a generated level <px>-<level> (n57-kiln, n58-span, ...):
#   the candidate  <px>-<level> (Design/levels/<px>-<level>.json; plan Design/levels/<px>/<level>.plan.json; report
#                  Reports/LEVEL-<px>-<level>-validate.md)
#   the base       the level row's levelFrom: the row of Design/unity-config.json design.<px> whose levelTo is <px>-<level>
#                  (design.n57.kiln57: n52-kiln; design.n58.span58: n52-span), run with --off<NN> (<NN> = the prefix's digits: the game
#                  before that night's rows). A NEW room has no row of its own id in the base level: its base room is the room its slot
#                  replaces (or, for a slot that held a link, the fight it follows), named in Design/levels/<px>/slots<NN>.json ("base");
#                  the base flow runs that room and its row is read under the new id (the dwell compared is the slot's, D-10 / D-13's
#                  grace unchanged). No slots file: the base room is the room's own id.
# Per room: four n46flow runs (base / cand x seeds 1-10 / 11-20), hold52 --perches (every reachable top), hold52 --pads, then
# n52-room-check.py --report --hold --flow-before --flow. Exit 0 only when every room PASSes (C 14/14, pads all, no 0-hit spot).
#   bash Tools/night/n59-l-contract.sh <px>-<level> [room[,room ...]]   (default: every kind-room row of the level's plan)
#   bash Tools/night/n59-l-contract.sh <px>-<level> planted             a planted copy of the first room (its health pickup and its pads
#                                                                        out) must FAIL
#   env JOBS (default 12), SEEDS (hold52 seeds per spot, default 3), TABLE_ONLY=1 (the table from the outputs already there),
#   OUT (default Logs/n59/L/contract/<px>-<level>)
cd "$(git rev-parse --show-toplevel)" || exit 1
T0=$SECONDS
CAND="$1"
case "$CAND" in n[0-9]*-[a-z]*) ;; *) sed -n '2,22p' "$0"; exit 2 ;; esac
shift
PX="${CAND%%-*}"
LV="${CAND#*-}"
NN="${PX#n}"
JOBS="${JOBS:-12}"
SEEDS="${SEEDS:-3}"
OUT="${OUT:-Logs/n59/L/contract/$CAND}"
mkdir -p "$OUT"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
PLAN="Design/levels/$PX/$LV.plan.json"
REP="Reports/LEVEL-$CAND-validate.md"
SLOTS="Design/levels/$PX/slots$NN.json"
OFF="--off$NN"
[ -f "$PLAN" ] || { echo "n59-l-contract: no $PLAN"; exit 1; }
BASE=$(node -e 'const c = JSON.parse(require("fs").readFileSync("Design/unity-config.json", "utf8")); const d = (c.design || {})[process.argv[1]] || {};
  const r = Object.values(d).find((x) => x && typeof x === "object" && x.levelTo === process.argv[2]);
  process.stdout.write(r ? r.levelFrom : "")' "$PX" "$CAND")
[ -n "$BASE" ] || { echo "n59-l-contract: no row in design.$PX with levelTo $CAND"; exit 1; }
baseRoom() { node -e 'const fs = require("fs"); const f = process.argv[2]; const s = fs.existsSync(f) ? JSON.parse(fs.readFileSync(f, "utf8")) : {};
  const r = (s.rooms || {})[process.argv[1]]; process.stdout.write(r && r.base ? r.base : process.argv[1]);' "$1" "$SLOTS"; }

if [ "$1" = planted ]; then
  r=$(node -e 'const p = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8")); process.stdout.write(p.rooms.find((x) => x.kind === "room").id)' "$PLAN")
  node -e 'const fs = require("fs"); const p = JSON.parse(fs.readFileSync(process.argv[1], "utf8")); const r = process.argv[2];
    p.props = p.props.filter((q) => !(q.room === r && q.kind === "healthPickup")); p.pads = p.pads.filter((q) => q.zone !== r);
    p.boxes.forEach((b) => { if (b.zone === r) b.why = String(b.why || "").replace(/boost/gi, "lift"); });
    fs.writeFileSync(process.argv[3], JSON.stringify(p));' "$PLAN" "$r" "$OUT/planted.plan.json"
  python -I Tools/night/n52-room-check.py "$OUT/planted.plan.json" "$r" | grep -E "FAIL|ROOM CONTRACT"
  rc=${PIPESTATUS[0]}
  rm -f "$OUT/planted.plan.json"
  [ "$rc" -ne 0 ] && echo "PLANTED contract: $CAND $r without its health and its pads FAILS (trusted)" || echo "PLANTED contract: $CAND $r PASSED with its health out (NOT trusted)"
  [ "$rc" -ne 0 ]; exit $?
fi

if [ $# -gt 0 ]; then IFS=, read -r -a LIST <<< "$1"; else
  read -r -a LIST <<< "$(node -e 'const p = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8")); process.stdout.write(p.rooms.filter((x) => x.kind === "room").map((x) => x.id).join(" "))' "$PLAN")"
fi
echo "n59-l-contract: ${LIST[*]} on $CAND (base $BASE $OFF; JOBS $JOBS, hold seeds $SEEDS)"
[ -n "$TABLE_ONLY" ] || dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
[ -f "$dll" ] || { echo "n59-l-contract: no $dll"; exit 1; }
[ -f "$REP" ] || { echo "n59-l-contract: no $REP (run levelgen on $PLAN first)"; exit 1; }

JOBFILE="$OUT/jobs.txt"
: > "$JOBFILE"
for r in "${LIST[@]}"; do
  b=$(baseRoom "$r")
  p="$OUT/$r"
  printf '%s\t%s\n' "$p-base-A.raw" "n46flow --level $BASE --rooms $b --seeds 10 --first 1 $OFF --no-geo" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-base-B.raw" "n46flow --level $BASE --rooms $b --seeds 10 --first 11 $OFF --no-geo" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-cand-A.txt" "n46flow --level $CAND --rooms $r --seeds 10 --first 1" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-cand-B.txt" "n46flow --level $CAND --rooms $r --seeds 10 --first 11 --no-geo" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-hold.txt" "hold52 --level $CAND --room $r --perches --seeds $SEEDS" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-pads.txt" "hold52 --level $CAND --room $r --pads" >> "$JOBFILE"
done
NJ=$(wc -l < "$JOBFILE")
[ -n "$TABLE_ONLY" ] || while IFS=$'\t' read -r out args; do
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  dotnet "$dll" $args < /dev/null > "$out" 2>&1 &
done < "$JOBFILE"
wait
echo "  $NJ jobs done in $((SECONDS - T0)) s"

TABLE="$OUT/table.md"
{
  echo "| room | base room ($BASE) | size (m) | density | planes | ride m | pads | dwell before -> after (grace) | clears A / B | deaths A | stall A | min hold hits (spots) | pads PASS | C n/m | verdict |"
  echo "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|"
} > "$TABLE"
num() { grep -E "$2" "$1" | head -1 | sed -E "s/.*$2.*/\\1/"; }
ALL=0
for r in "${LIST[@]}"; do
  b=$(baseRoom "$r")
  p="$OUT/$r"
  # the base row read under the new id (the slot's room)
  for s in A B; do sed -E "s/^(\s+)$b(\s+)(room|boss)(\s+\|)/\1$r\2\3\4/" "$p-base-$s.raw" > "$p-base-$s.txt"; done
  python -I Tools/night/n52-room-check.py "$PLAN" "$r" --report "$REP" --hold "$p-hold.txt" \
    --flow-before "$p-base-A.txt,$p-base-B.txt" --flow "$p-cand-A.txt,$p-cand-B.txt" > "$p-check.txt" 2>&1
  c="$p-check.txt"
  size=$(grep -E " 1 size " "$c" | sed -E 's/.*rect ([0-9.]+) x ([0-9.]+) m.*height ([0-9.]+).*/\1 x \2 x \3/')
  dens=$(num "$c" '= ([0-9.]+) per 100 m2'); pcs=$(num "$c" ' ([0-9]+) pieces \(')
  planes=$(num "$c" ' ([0-9]+) standing planes')
  ride=$(num "$c" 'longest ([0-9.]+) m'); [ -n "$ride" ] || ride="-"
  pads=$(num "$c" 'upward line +([0-9]+) pad')
  dw=$(grep -E " 7 dwell " "$c" | head -1)
  if echo "$dw" | grep -q "flow dwell"; then dwell="$(echo "$dw" | sed -E 's/.*flow dwell ([0-9.]+) -> ([0-9.]+) s.*= ([0-9.]+) s:.*/\1 -> \2 (\3)/')"; else dwell="n/a"; fi
  rowA=$(grep -E "^\s+$r\s+room" "$p-cand-A.txt" | head -1)
  ca=$(echo "$rowA" | sed -E 's/.*\(([0-9]+\/[0-9]+)[,)].*/\1/')
  cb=$(grep -E "^\s+$r\s+room" "$p-cand-B.txt" | sed -E 's/.*\(([0-9]+\/[0-9]+)[,)].*/\1/' | head -1)
  dA=$(echo "$rowA" | awk -F'|' '{print $6}' | awk '{print $1}')
  sA=$(echo "$rowA" | awk -F'|' '{print $2}' | awk '{print $1}')
  hmin=$(grep -E "^hold52 $r tier .* hits [0-9]+" "$p-hold.txt" | sed -E 's/.* hits ([0-9]+).*/\1/' | sort -n | head -1)
  hn=$(grep -cE "^hold52 $r tier .* hits [0-9]+" "$p-hold.txt")
  pp=$(grep -cE "^pad52 $r pad .* PASS$" "$p-pads.txt"); pt=$(grep -cE "^pad52 $r pad " "$p-pads.txt")
  cline=$(grep -E "^ROOM CONTRACT $r:" "$c")
  cnm=$(echo "$cline" | sed -E 's/.*: C ([0-9]+\/[0-9]+).*/\1/')
  verdict=$(echo "$cline" | sed -E 's/.*-> (PASS|FAIL).*/\1/')
  fails=$(grep -E "^  FAIL " "$c" | sed -E 's/^  FAIL ([^ ]+( [^ ]+)*)  .*/\1/' | paste -sd ';' -)
  [ "$pt" -gt 0 ] && [ "$pp" -lt "$pt" ] && { verdict=FAIL; fails="${fails:+$fails; }pads $pp/$pt"; }
  [ -n "$hmin" ] && [ "$hmin" -eq 0 ] && { verdict=FAIL; fails="${fails:+$fails; }0-hit spot"; }
  [ "$verdict" = PASS ] || ALL=1
  echo "| $LV $r | $b | ${size:-?} | ${dens:-?} (${pcs:-?} pc) | ${planes:-?} | $ride | ${pads:-0} | $dwell | ${ca:-?} / ${cb:-?} | ${dA:-?} | ${sA:-?} | ${hmin:-?} ($hn) | $pp/$pt | ${cnm:-?} | ${verdict:-?}${fails:+ ($fails)} |" >> "$TABLE"
done
echo "" >> "$TABLE"
echo "n59-l-contract $CAND: ${#LIST[@]} rooms, $NJ dotnet jobs ($JOBS at once), $((SECONDS - T0)) s; per room: $OUT/<room>-{base,cand}-{A,B}.txt, -hold.txt, -pads.txt, -check.txt" >> "$TABLE"
cat "$TABLE"
exit $ALL
