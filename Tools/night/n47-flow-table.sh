#!/usr/bin/env bash
# NIGHT47 (the lead; from n46-flow-table.sh): baseline = the n46 levels with --off47 (NIGHT46's game), candidate = the n47 levels + rows (the lead re-runs it at the freeze). Run from the root of ANY checkout (the
# main one, a worktree, a detached scratch worktree of port/stage3): it builds that checkout's SimHarness once, then runs F1's meter (n46flow,
# Tools/SimHarness/Harness.N46.Flow.cs) 20 times in parallel:
#   baseline   n45-foundry n45-iceline n45-kiln n40-span n41-spire, --off46 (NIGHT45's game, the n45-r4 levels), seeds 1-10 and 11-20
#   candidate  n46-foundry n46-iceline n46-kiln n46-span n46-spire, ROWS forced on (default: retry46,overlap46,teach46,stuck46,walkin46,wyrm46),
#              seeds 1-10 and 11-20 (the n46 level files load as themselves: no sim code reads a level row's enabled flag)
# then writes the table (node Tools/night/n46-flow-table.cjs): per level walks, stall %, longest stall, dwell, dead, deaths, clear, baseline ->
# candidate (the mean of the two seed sets, each set in brackets), a check past the band, a cross for worse past it; the band = this run's own
# baseline seeds 1-10 against 11-20 (the largest gap over the five levels, per metric).
#   bash Tools/night/n46-flow-table.sh [outDir] [rows]
#   e.g. bash Tools/night/n46-flow-table.sh Logs/feel/n46-flow/freeze retry46,overlap46,teach46,stuck46,walkin46,wyrm46
#   (from another checkout's root: bash /path/to/this/n46-flow-table.sh <absolute outDir>; the .cjs is read beside this script)
# About 3-4 min on the 24-core laptop (20 runs of ~1-2 min each in parallel; nothing else heavy beside it: L-005).
out="${1:-Logs/feel/n46-flow/table}"
rows="${2:-wick47}"
[ -f Tools/SimHarness/Harness.N46.Flow.cs ] || { echo "n46-flow-table: run from a checkout root that has Tools/SimHarness/Harness.N46.Flow.cs"; exit 2; }
mkdir -p "$out"
echo "n46-flow-table: $(date +%T) build ($(git rev-parse --short HEAD 2>/dev/null))"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
[ -f "$dll" ] || { echo "n46-flow-table: no $dll"; exit 2; }
for l in n46-foundry n46-iceline n46-kiln n46-span n46-spire; do
  dotnet "$dll" n46flow --level "$l" --seeds 10 --first 1 --off47 > "$out/base-A-$l.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$l" --seeds 10 --first 11 --off47 --no-geo > "$out/base-B-$l.txt" 2>&1 &
done
for l in n47-foundry n47-iceline n47-kiln n47-span n47-spire; do
  dotnet "$dll" n46flow --level "$l" --seeds 10 --first 1 --rows "$rows" > "$out/cand-A-$l.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$l" --seeds 10 --first 11 --rows "$rows" --no-geo > "$out/cand-B-$l.txt" 2>&1 &
done
wait
echo "n46-flow-table: $(date +%T) runs done"
node "$(dirname "$0")/n47-flow-table.cjs" "$out" "$rows" "$(git rev-parse --short HEAD 2>/dev/null)" | tee "$out/table.md"
