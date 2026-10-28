#!/usr/bin/env bash
# NIGHT56 builder G (G4): `n47human --boss wyrm --level n52-kiln` over seeds 1..40 in FOUR parallel processes of 10 seeds (the harness built
# once first; --no-build after), --per-seed, then one combined line (python: the same median / p90 / deaths / killed n47human prints). Extra
# arguments go to every chunk (e.g. --rows wyrm55). Output: Logs/n56/<tag>.txt (the four chunks' lines, then the combined line).
#   bash Tools/night/n56-g-wyrm.sh <tag> [n47human args ...]
cd "$(dirname "$0")/../.." || exit 1
tag="${1:?tag}"; shift
dotnet build Tools/SimHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "n56-g-wyrm: build failed"; exit 1; }
mkdir -p Logs/n56
pids=()
for f in 1 11 21 31; do
  dotnet run --no-build --project Tools/SimHarness -c Release -- n47human --boss wyrm --level n52-kiln --first "$f" --seeds 10 --per-seed "$@" > "Logs/n56/$tag-$f.part" 2>&1 &
  pids+=($!)
done
for p in "${pids[@]}"; do wait "$p"; done
cat Logs/n56/$tag-1.part Logs/n56/$tag-11.part Logs/n56/$tag-21.part Logs/n56/$tag-31.part > "Logs/n56/$tag.txt"
rm -f Logs/n56/$tag-*.part
python -I Tools/night/n56-g-wyrm-sum.py "Logs/n56/$tag.txt" | tee -a "Logs/n56/$tag.txt"
