#!/usr/bin/env bash
# NIGHT56 builder G: n46flow on the five n52 levels, one process per level IN PARALLEL (the harness built once first; --no-build after), 10 seeds,
# --no-geo; each level's table to Logs/n56/<tag>-<level>.txt, then one combined file Logs/n56/<tag>.txt in the levels' order. Extra arguments go to
# every run (e.g. --rows tokens56 or --set design.n56.tokens56.numbers.waitS=2).
#   bash Tools/night/n56-g-flow.sh <tag> [n46flow args ...]
cd "$(dirname "$0")/../.." || exit 1
tag="${1:?tag}"; shift
dotnet build Tools/SimHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "n56-g-flow: build failed"; exit 1; }
mkdir -p Logs/n56
pids=()
for L in foundry iceline kiln span spire; do
  dotnet run --no-build --project Tools/SimHarness -c Release -- n46flow --level "n52-$L" --seeds 10 --no-geo --out "Logs/n56/$tag-$L.txt" "$@" > "Logs/n56/$tag-$L.out" 2>&1 &
  pids+=($!)
done
for p in "${pids[@]}"; do wait "$p"; done
: > "Logs/n56/$tag.txt"
for L in foundry iceline kiln span spire; do cat "Logs/n56/$tag-$L.txt" >> "Logs/n56/$tag.txt"; rm -f "Logs/n56/$tag-$L.txt" "Logs/n56/$tag-$L.out"; done
grep -E "SUMMARY|STYLE|TOKENS|RANGED|IDLE" "Logs/n56/$tag.txt"
