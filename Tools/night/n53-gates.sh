#!/usr/bin/env bash
# NIGHT53 gates (the lead; from n52-gates.sh): the integration gates, in order: config copies identical, golden, fingerprint plain / --off52 /
# --off53 (EQUAL), the Editor compile (stops Play mode first), the console's compile errors. One line per gate; exits 1 at the first failure.
#   bash Tools/night/n53-gates.sh [--no-editor]
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
fail() { echo "GATE FAIL: $*"; exit 1; }
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "gate config: the two copies identical" || fail "config copies differ"
g=$(dotnet run --project Tools/CoreHarness -c Release 2>&1 | grep "golden total")
echo "gate golden: $g"; echo "$g" | grep -q "0 failing, 0 mismatches" || fail golden
for o in "" "--off52" "--off53"; do
  f=$(dotnet run --project Tools/SimHarness -c Release -- fingerprint $o 2>&1 | grep "fingerprint seed")
  echo "gate fingerprint $o: $f"; echo "$f" | grep -q "EQUAL" || fail "fingerprint $o"
done
if [ "$1" != "--no-editor" ]; then
  bash Tools/unity-loop.sh compile 2>&1 | tail -1
  e=$(bash Tools/unity-loop.sh errors 2>&1 | grep -E '"compilationFailed"|"consoleErrors"' | head -2 | tr -d ' \n')
  echo "gate editor: $e"; echo "$e" | grep -q '"compilationFailed":false' || fail "editor compile"
fi
echo "gates: all GREEN $(date +%T)"
