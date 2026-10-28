#!/usr/bin/env bash
# NIGHT57 gates (the lead; from n56-gates.sh): the integration gates, in order: config copies identical, golden, fingerprint plain / --off56 /
# --off57 (EQUAL), the Editor compile (stops Play mode first), the console's compile errors. One line per gate; exits 1 at the first failure.
#   bash Tools/night/n57-gates.sh [--no-editor] [--flow]
# --flow (the red team: campaign-only rows such as tokens56 / press57 are invisible to the survival fingerprint): n46flow on the five n52
# levels with --off57 must equal Logs/n57/flow-base.txt line for line, wall-time lines aside (~5 min; at each pick, never beside a Player).
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
fail() { echo "GATE FAIL: $*"; exit 1; }
if [ "$1" = "--flow" ] || [ "$2" = "--flow" ]; then
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 10 --off57 --out Logs/n57/flow-off57.txt > /dev/null 2>&1
  # the header line "n46flow: seeds ..." names the forced rows / off switch, so it always differs (builder G, 10:0x): data lines only
  if diff <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n57/flow-base.txt) <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n57/flow-off57.txt) > /dev/null; then echo "gate flow --off57: EQUAL to flow-base"; else fail "flow --off57 differs from flow-base (Logs/n57/flow-off57.txt)"; fi
fi
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "gate config: the two copies identical" || fail "config copies differ"
g=$(dotnet run --project Tools/CoreHarness -c Release 2>&1 | grep "golden total")
echo "gate golden: $g"; echo "$g" | grep -q "0 failing, 0 mismatches" || fail golden
for o in "" "--off56" "--off57"; do
  f=$(dotnet run --project Tools/SimHarness -c Release -- fingerprint $o 2>&1 | grep "fingerprint seed")
  echo "gate fingerprint $o: $f"; echo "$f" | grep -q "EQUAL" || fail "fingerprint $o"
done
if [ "$1" != "--no-editor" ]; then
  bash Tools/unity-loop.sh compile 2>&1 | tail -1
  e=$(bash Tools/unity-loop.sh errors 2>&1 | grep -E '"compilationFailed"|"consoleErrors"' | head -2 | tr -d ' \n')
  echo "gate editor: $e"; echo "$e" | grep -q '"compilationFailed":false' || fail "editor compile"
fi
echo "gates: all GREEN $(date +%T)"
