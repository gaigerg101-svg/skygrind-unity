#!/usr/bin/env bash
# NIGHT52 (the lead, P0; from n47-flow-table.sh, which hardcodes the n46 / n47 ids): the flow table for the levels named on the command line.
# Each argument is <base>[:<cand>]: the base level runs with --off52 (NIGHT51's game), the candidate (a new n52 level id) with the config's rows;
# both on seeds 1-10 and 11-20 (the band: the base's seeds 1-10 against 11-20). With no candidate (P0) only the base runs.
#   bash Tools/night/n52-flow-table.sh <outDir> <base>[:<cand>] ...
#   e.g. bash Tools/night/n52-flow-table.sh Logs/n52/flow-p0 n47-kiln n48-foundry
#        bash Tools/night/n52-flow-table.sh Logs/n52/flow-k1 n47-kiln:n52-kiln
# ROWS=r1,r2 forces sim rows on in both sides (n46flow --rows). About 2 min a level, all runs in parallel; nothing heavy beside it (L-005).
out="${1:?outDir}"; shift
[ $# -ge 1 ] || { echo "n52-flow-table: name at least one level"; exit 2; }
[ -f Tools/SimHarness/Harness.N46.Flow.cs ] || { echo "n52-flow-table: run from a checkout root that has Tools/SimHarness/Harness.N46.Flow.cs"; exit 2; }
mkdir -p "$out"
rowsArg=(); [ -n "$ROWS" ] && rowsArg=(--rows "$ROWS")
echo "n52-flow-table: $(date +%T) build ($(git rev-parse --short HEAD 2>/dev/null))"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
[ -f "$dll" ] || { echo "n52-flow-table: no $dll"; exit 2; }
pairs=()
for a in "$@"; do
  b="${a%%:*}"; c=""; [ "$a" != "$b" ] && c="${a#*:}"
  pairs+=("$b:$c")
  dotnet "$dll" n46flow --level "$b" --seeds 10 --first 1 --off52 "${rowsArg[@]}" > "$out/base-A-$b.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$b" --seeds 10 --first 11 --off52 --no-geo "${rowsArg[@]}" > "$out/base-B-$b.txt" 2>&1 &
  if [ -n "$c" ]; then
    dotnet "$dll" n46flow --level "$c" --seeds 10 --first 1 "${rowsArg[@]}" > "$out/cand-A-$c.txt" 2>&1 &
    dotnet "$dll" n46flow --level "$c" --seeds 10 --first 11 --no-geo "${rowsArg[@]}" > "$out/cand-B-$c.txt" 2>&1 &
  fi
done
wait
echo "n52-flow-table: $(date +%T) runs done"
node "$(dirname "$0")/n52-flow-table.cjs" "$out" "$(git rev-parse --short HEAD 2>/dev/null)" "${pairs[@]}" | tee "$out/table.md"
