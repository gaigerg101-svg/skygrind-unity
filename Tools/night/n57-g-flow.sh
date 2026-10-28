#!/usr/bin/env bash
# NIGHT57 builder G: one n46flow run on the five n52 levels (10 seeds; the slate's measure, bot) into Logs/n57/<name>.txt, from this checkout.
#   bash Tools/night/n57-g-flow.sh <name> [n46flow args: --rows r1,r2 / --set key=value / --first 11 / --off57 ...]
# Needs a built SimHarness (dotnet run --project Tools/SimHarness -c Release -- fingerprint builds it); runs with --no-build so several can run at once.
cd "$(dirname "$0")/../.." || exit 1
name="${1:?usage: n57-g-flow.sh <name> [args]}"; shift
mkdir -p Logs/n57
dotnet run --no-build --project Tools/SimHarness -c Release -- n46flow --level "${LEVELS:-n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire}" --seeds ${SEEDS:-10} "$@" --out "Logs/n57/$name.txt" > "Logs/n57/$name.out" 2>&1
echo "n57-g-flow $name: exit $? $(date +%T)"
