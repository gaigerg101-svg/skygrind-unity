#!/usr/bin/env bash
# NIGHT58 gates (the lead; from n57-gates.sh): the integration gates, in order: config copies identical, golden, fingerprint plain / --off57 /
# --off58 (EQUAL), the Editor compile (stops Play mode first), the console's compile errors. One line per gate; exits 1 at the first failure.
#   bash Tools/night/n58-gates.sh [--no-editor] [--flow]
# --flow: n46flow on n52-foundry / iceline / span / spire with --off58 must equal Logs/n58/flow-base.txt line for line, wall-time and header
# lines aside (~2 min). n57-kiln is OUT of the list (NIGHT58.md section 3): L0 edits n57-kiln's data behind kiln57 (already ON) and has no
# n58 off switch; L0 carries its own before / after (Logs/n58/kiln-base*.txt).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
fail() { echo "GATE FAIL: $*"; exit 1; }
if [ "$1" = "--flow" ] || [ "$2" = "--flow" ]; then
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-foundry,n52-iceline,n52-span,n52-spire --seeds 10 --off58 --out Logs/n58/flow-off58.txt > /dev/null 2>&1
  if diff <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n58/flow-base.txt) <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n58/flow-off58.txt) > /dev/null; then echo "gate flow --off58: EQUAL to flow-base"; else fail "flow --off58 differs from flow-base (Logs/n58/flow-off58.txt)"; fi
fi
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "gate config: the two copies identical" || fail "config copies differ"
g=$(dotnet run --project Tools/CoreHarness -c Release 2>&1 | grep "golden total")
echo "gate golden: $g"; echo "$g" | grep -q "0 failing, 0 mismatches" || fail golden
for o in "" "--off57" "--off58"; do
  f=$(dotnet run --project Tools/SimHarness -c Release -- fingerprint $o 2>&1 | grep "fingerprint seed")
  echo "gate fingerprint $o: $f"; echo "$f" | grep -q "EQUAL" || fail "fingerprint $o"
done
if [ "$1" != "--no-editor" ]; then
  bash Tools/unity-loop.sh compile 2>&1 | tail -1
  e=$(bash Tools/unity-loop.sh errors 2>&1 | grep -E '"compilationFailed"|"consoleErrors"' | head -2 | tr -d ' \n')
  echo "gate editor: $e"; echo "$e" | grep -q '"compilationFailed":false' || fail "editor compile"
fi
echo "gates: all GREEN $(date +%T)"
