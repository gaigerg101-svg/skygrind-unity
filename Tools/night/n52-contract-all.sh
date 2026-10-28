#!/usr/bin/env bash
# NIGHT52 (builder K, round 3; the lead's ask): THE ROOM CONTRACT for every rebuilt room in one command, on the current checkout.
# For each room: the isolated flows (Tools/night/n52-room-flow.sh's four n46flow runs: the base level with --off52 vs the n52 level, seeds
# 1-10 and 11-20), hold52 --perches (every reachable top), hold52 --pads (each pad launched and landed), then Tools/night/n52-room-check.py
# with --flow-before / --flow / --hold / --report (/ --grapple on spire and span), and the check on the base plan (the density before).
# Prints ONE Markdown table and writes it to Logs/n52/contract/table.md (the per-room outputs beside it). Exit 0 only when every room PASSes.
#   bash Tools/night/n52-contract-all.sh [level:room,room ...]
#     default: kiln:kb2,kb2c foundry:a5,b1 (+ a3 when n52-foundry's a3 row differs from n46's) iceline:g7,g5,g8 spire:v6,v5,v2 span:p3,p5
#     env JOBS   dotnet processes at once (default 12; every job is one process, the pool keeps JOBS busy)
#     env SEEDS  hold52 seeds per spot (default 3)
#     env TABLE_ONLY=1  skip the jobs: the checks and the table from the outputs already in Logs/n52/contract
# The base level is design.n52.<level>52.levelFrom (Design/unity-config.json); the base plan is Design/levels/n47/<level>.plan.json, foundry's
# Design/levels/n46/foundry.plan.json (n47 / n48-foundry only add data to a9). A missing Reports/LEVEL-n52-<level>-validate.md is made
# by levelgen into a scratch level file (the checkout's level files are never written). Read-only otherwise.
cd "$(git rev-parse --show-toplevel)" || exit 1
T0=$SECONDS
JOBS="${JOBS:-12}"
SEEDS="${SEEDS:-3}"
OUT=Logs/n52/contract
mkdir -p "$OUT"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll

# ---- the room list
if [ $# -gt 0 ]; then LIST=("$@"); else
  LIST=(kiln:kb2,kb2c foundry:a5,b1 iceline:g7,g5,g8 spire:v6,v5,v2 span:p3,p5)
  a3=$(node -e '
    const fs = require("fs");
    const rd = (p) => { try { return JSON.parse(fs.readFileSync(p, "utf8")); } catch (e) { return null; } };
    const a = rd("Design/levels/n52/foundry.plan.json"), b = rd("Design/levels/n46/foundry.plan.json");
    const r = (p) => p && p.rooms.find((x) => x.id === "a3");
    process.stdout.write(a && b && JSON.stringify(r(a)) !== JSON.stringify(r(b)) ? "yes" : "no");')
  [ "$a3" = yes ] && LIST[1]=foundry:a5,b1,a3
fi

basePlan() { [ "$1" = foundry ] && echo Design/levels/n46/foundry.plan.json || echo "Design/levels/n47/$1.plan.json"; }
baseLevel() {
  node -e 'const c = JSON.parse(require("fs").readFileSync("Design/unity-config.json", "utf8")); const r = ((c.design || {}).n52 || {})[process.argv[1] + "52"];
    if (!r || !r.levelFrom) { console.error("no design.n52." + process.argv[1] + "52.levelFrom"); process.exit(1); } process.stdout.write(r.levelFrom);' "$1"
}
grantOf() { case "$1" in span|spire) echo --grapple ;; *) echo "" ;; esac; }

echo "n52-contract-all: ${LIST[*]} (JOBS $JOBS, hold seeds $SEEDS)"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
[ -f "$dll" ] || { echo "n52-contract-all: no $dll"; exit 1; }

# ---- the jobs: per room four flows, the hold probe, the pads probe (longest first, so the pool ends together)
JOBFILE="$OUT/jobs.txt"
: > "$JOBFILE"
ROOMS=()
for item in "${LIST[@]}"; do
  lv="${item%%:*}"; rooms="${item#*:}"
  base=$(baseLevel "$lv") || exit 1
  cand="n52-$lv"
  [ -f "Design/levels/$cand.json" ] || { echo "n52-contract-all: no Design/levels/$cand.json"; exit 1; }
  rep="Reports/LEVEL-$cand-validate.md"
  if [ ! -f "$rep" ]; then
    echo "  $rep missing: levelgen into $OUT/$cand.scratch.json"
    dotnet "$dll" levelgen "Design/levels/n52/$lv.plan.json" "$OUT/$cand.scratch.json" > "$OUT/$cand.levelgen.txt" 2>&1
    rm -f "$OUT/$cand.scratch.json"
  fi
  for r in ${rooms//,/ }; do
    ROOMS+=("$lv:$r:$base")
    p="$OUT/$lv-$r"
    printf '%s\t%s\n' "$p-base-A.txt" "n46flow --level $base --rooms $r --seeds 10 --first 1 --off52 --no-geo" >> "$JOBFILE"
    printf '%s\t%s\n' "$p-base-B.txt" "n46flow --level $base --rooms $r --seeds 10 --first 11 --off52 --no-geo" >> "$JOBFILE"
    printf '%s\t%s\n' "$p-cand-A.txt" "n46flow --level $cand --rooms $r --seeds 10 --first 1" >> "$JOBFILE"
    printf '%s\t%s\n' "$p-cand-B.txt" "n46flow --level $cand --rooms $r --seeds 10 --first 11 --no-geo" >> "$JOBFILE"
  done
done
for rr in "${ROOMS[@]}"; do
  IFS=: read -r lv r base <<< "$rr"
  printf '%s\t%s\n' "$OUT/$lv-$r-hold.txt" "hold52 --level n52-$lv --room $r --perches --seeds $SEEDS" >> "$JOBFILE"
done
for rr in "${ROOMS[@]}"; do
  IFS=: read -r lv r base <<< "$rr"
  printf '%s\t%s\n' "$OUT/$lv-$r-pads.txt" "hold52 --level n52-$lv --room $r --pads" >> "$JOBFILE"
done
NJ=$(wc -l < "$JOBFILE")
echo "  $NJ dotnet jobs for ${#ROOMS[@]} rooms, $JOBS at once"
[ -n "$TABLE_ONLY" ] && echo "  TABLE_ONLY: the jobs' outputs as they are"
[ -n "$TABLE_ONLY" ] || while IFS=$'\t' read -r out args; do
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  dotnet "$dll" $args < /dev/null > "$out" 2>&1 &
done < "$JOBFILE"
wait
echo "  jobs done in $((SECONDS - T0)) s"

# ---- the checks and the table
num() { grep -E "$2" "$1" | head -1 | sed -E "s/.*$2.*/\\1/"; }
TABLE="$OUT/table.md"
{
  echo "| room | size (m) | density | density n47/n46 -> n52 | planes | ride m | pads | dwell before -> after (grace) | clears A / B | min hold hits (spots) | pads PASS | C n/m | verdict |"
  echo "|---|---|---|---|---|---|---|---|---|---|---|---|---|"
} > "$TABLE"
ALL=0
# the room checks (the n52 room with every output, the base plan's room for the density before), in the same pool
for rr in "${ROOMS[@]}"; do
  IFS=: read -r lv r base <<< "$rr"
  p="$OUT/$lv-$r"
  grant=$(grantOf "$lv")
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  python -I Tools/night/n52-room-check.py "Design/levels/n52/$lv.plan.json" "$r" --report "Reports/LEVEL-n52-$lv-validate.md" $grant \
    --hold "$p-hold.txt" --flow-before "$p-base-A.txt,$p-base-B.txt" --flow "$p-cand-A.txt,$p-cand-B.txt" < /dev/null > "$p-check.txt" 2>&1 &
  while [ "$(jobs -rp | wc -l)" -ge "$JOBS" ]; do wait -n; done
  # shellcheck disable=SC2086
  python -I Tools/night/n52-room-check.py "$(basePlan "$lv")" "$r" $grant < /dev/null > "$p-check-base.txt" 2>&1 &
done
wait
for rr in "${ROOMS[@]}"; do
  IFS=: read -r lv r base <<< "$rr"
  p="$OUT/$lv-$r"
  c="$p-check.txt"
  size=$(grep -E " 1 size " "$c" | sed -E 's/.*rect ([0-9.]+) x ([0-9.]+) m.*height ([0-9.]+).*/\1 x \2 x \3/')
  dens=$(num "$c" '= ([0-9.]+) per 100 m2'); pcs=$(num "$c" ' ([0-9]+) pieces \(')
  dbase=$(num "$p-check-base.txt" '= ([0-9.]+) per 100 m2'); [ -n "$dbase" ] || dbase="-"
  planes=$(num "$c" ' ([0-9]+) standing planes')
  ride=$(num "$c" 'longest ([0-9.]+) m'); [ -n "$ride" ] || ride="-"
  pads=$(num "$c" 'upward line +([0-9]+) pad')
  dw=$(grep -E " 7 dwell " "$c" | head -1)
  if echo "$dw" | grep -q "flow dwell"; then
    dwell="$(echo "$dw" | sed -E 's/.*flow dwell ([0-9.]+) -> ([0-9.]+) s.*= ([0-9.]+) s:.*/\1 -> \2 (\3)/')"
  else dwell="n/a"; fi
  ca=$(grep -E "^\s+$r\s+room" "$p-cand-A.txt" | sed -E 's/.*\(([0-9]+\/[0-9]+)[,)].*/\1/' | head -1)
  cb=$(grep -E "^\s+$r\s+room" "$p-cand-B.txt" | sed -E 's/.*\(([0-9]+\/[0-9]+)[,)].*/\1/' | head -1)
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
  echo "| $lv $r | ${size:-?} | ${dens:-?} (${pcs:-?} pc) | $dbase -> ${dens:-?} | ${planes:-?} | $ride | ${pads:-0} | $dwell | ${ca:-?} / ${cb:-?} | ${hmin:-?} ($hn) | $pp/$pt | ${cnm:-?} | ${verdict:-?}${fails:+ ($fails)} |" >> "$TABLE"
done
echo "" >> "$TABLE"
echo "n52-contract-all: ${#ROOMS[@]} rooms, $NJ dotnet jobs ($JOBS at once), $((SECONDS - T0)) s; per room: $OUT/<level>-<room>-{base,cand}-{A,B}.txt, -hold.txt, -pads.txt, -check.txt" >> "$TABLE"
cat "$TABLE"
exit $ALL
