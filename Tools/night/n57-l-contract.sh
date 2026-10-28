#!/usr/bin/env bash
# NIGHT57 builder L (L0): THE ROOM CONTRACT (Tools/night/n52-contract-all.sh's jobs and table, Tools/night/n52-room-check.py unchanged) on
# n57-kiln. n52-contract-all.sh hard-codes the n52 level, its plan and design.n52.<level>52.levelFrom; this file runs the same jobs on:
#   the candidate  n57-kiln (Design/levels/n57-kiln.json; plan Design/levels/n57/kiln.plan.json; report Reports/LEVEL-n57-kiln-validate.md)
#   the base       design.n57.kiln57.levelFrom (n52-kiln) with --off57 (NIGHT56's game, as Logs/n57/flow-base.txt), the plan
#                  Design/levels/n52/kiln.plan.json. A NEW room has no row of its own id in n52-kiln: its base room is the n52 room its slot
#                  replaces, named in Design/levels/n57/slots57.json ("base"); the base flow runs that room and its row is read under the
#                  new id (the dwell compared is the slot's, D-10 / D-13's grace unchanged).
# Per room: four n46flow runs (base / cand x seeds 1-10 / 11-20), hold52 --perches (every reachable top), hold52 --pads, then
# n52-room-check.py --report --hold --flow-before --flow. Exit 0 only when every room PASSes (C 14/14, pads all, no 0-hit spot).
#   bash Tools/night/n57-l-contract.sh room[,room ...]        (default: every kind-room row of the n57 plan)
#   bash Tools/night/n57-l-contract.sh planted                 a planted copy of the first room (its health pickup and its pads out) must FAIL
#   env JOBS (default 12), SEEDS (hold52 seeds per spot, default 3), TABLE_ONLY=1 (the table from the outputs already there)
cd "$(git rev-parse --show-toplevel)" || exit 1
T0=$SECONDS
JOBS="${JOBS:-12}"
SEEDS="${SEEDS:-3}"
OUT=Logs/n57/L/contract
mkdir -p "$OUT"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
PLAN=Design/levels/n57/kiln.plan.json
BASEPLAN=Design/levels/n52/kiln.plan.json
REP=Reports/LEVEL-n57-kiln-validate.md
CAND=n57-kiln
BASE=$(node -e 'const c = JSON.parse(require("fs").readFileSync("Design/unity-config.json", "utf8")); process.stdout.write(c.design.n57.kiln57.levelFrom)')
baseRoom() { node -e 'const s = JSON.parse(require("fs").readFileSync("Design/levels/n57/slots57.json", "utf8")); const r = (s.rooms || {})[process.argv[1]];
  process.stdout.write(r && r.base ? r.base : process.argv[1]);' "$1"; }

if [ "$1" = planted ]; then
  r=$(node -e 'const p = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8")); process.stdout.write(p.rooms.find((x) => x.kind === "room").id)' "$PLAN")
  node -e 'const fs = require("fs"); const p = JSON.parse(fs.readFileSync(process.argv[1], "utf8")); const r = process.argv[2];
    p.props = p.props.filter((q) => !(q.room === r && q.kind === "healthPickup")); p.pads = p.pads.filter((q) => q.zone !== r);
    p.boxes.forEach((b) => { if (b.zone === r) b.why = String(b.why || "").replace(/boost/gi, "lift"); });
    fs.writeFileSync(process.argv[3], JSON.stringify(p));' "$PLAN" "$r" "$OUT/planted.plan.json"
  python -I Tools/night/n52-room-check.py "$OUT/planted.plan.json" "$r" | grep -E "FAIL|ROOM CONTRACT"
  rc=${PIPESTATUS[0]}
  rm -f "$OUT/planted.plan.json"
  [ "$rc" -ne 0 ] && echo "PLANTED contract: $r without its health and its pads FAILS (trusted)" || echo "PLANTED contract: $r PASSED with its health out (NOT trusted)"
  [ "$rc" -ne 0 ]; exit $?
fi

if [ $# -gt 0 ]; then IFS=, read -r -a LIST <<< "$1"; else
  read -r -a LIST <<< "$(node -e 'const p = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8")); process.stdout.write(p.rooms.filter((x) => x.kind === "room").map((x) => x.id).join(" "))' "$PLAN")"
fi
echo "n57-l-contract: ${LIST[*]} on $CAND (base $BASE --off57; JOBS $JOBS, hold seeds $SEEDS)"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
[ -f "$dll" ] || { echo "n57-l-contract: no $dll"; exit 1; }
[ -f "$REP" ] || { echo "n57-l-contract: no $REP (run levelgen on $PLAN first)"; exit 1; }

JOBFILE="$OUT/jobs.txt"
: > "$JOBFILE"
for r in "${LIST[@]}"; do
  b=$(baseRoom "$r")
  p="$OUT/$r"
  printf '%s\t%s\n' "$p-base-A.raw" "n46flow --level $BASE --rooms $b --seeds 10 --first 1 --off57 --no-geo" >> "$JOBFILE"
  printf '%s\t%s\n' "$p-base-B.raw" "n46flow --level $BASE --rooms $b --seeds 10 --first 11 --off57 --no-geo" >> "$JOBFILE"
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
  echo "| room | base room (n52) | size (m) | density | planes | ride m | pads | dwell before -> after (grace) | clears A / B | deaths A | stall A | min hold hits (spots) | pads PASS | C n/m | verdict |"
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
  echo "| kiln $r | $b | ${size:-?} | ${dens:-?} (${pcs:-?} pc) | ${planes:-?} | $ride | ${pads:-0} | $dwell | ${ca:-?} / ${cb:-?} | ${dA:-?} | ${sA:-?} | ${hmin:-?} ($hn) | $pp/$pt | ${cnm:-?} | ${verdict:-?}${fails:+ ($fails)} |" >> "$TABLE"
done
echo "" >> "$TABLE"
echo "n57-l-contract: ${#LIST[@]} rooms, $NJ dotnet jobs ($JOBS at once), $((SECONDS - T0)) s; per room: $OUT/<room>-{base,cand}-{A,B}.txt, -hold.txt, -pads.txt, -check.txt" >> "$TABLE"
cat "$TABLE"
exit $ALL
