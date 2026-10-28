#!/usr/bin/env bash
# NIGHT61 gates (the forge FORGE61, 2026-10-22; a copy of n60-gates.sh with the n61 names): the integration gates, in order: config copies
# identical, golden, fingerprint plain / --off57 / --off58 / --off59 / --off60 / --off61 (EQUAL), the Editor compile (stops Play mode first)
# with "compilationFailed": false AND 0 console errors. One line per gate; exits 1 at the first failure.
#   bash Tools/night/n61-gates.sh [--no-editor] [--flow]      the gates
#   bash Tools/night/n61-gates.sh --flow-base                  writes the --flow baseline Logs/n61/flow-base.txt once (~5 min, bearings)
# It checks the tree it is run from (`git rev-parse --show-toplevel`), so a builder's worktree checks itself; builders pass --no-editor
# (only the lead drives the Editor, whose gate always reads the main project's Editor).
# --flow: n46flow on n52-foundry / n52-iceline / n57-kiln / n52-span / n52-spire, seeds 1-10, with --off61 must equal Logs/n61/flow-base.txt
# line for line, wall-time and header lines aside (NIGHT57.md section 0's flow gate). A level-data slice changes its own level's lines:
# re-make the base in that slice's commit and say so (NIGHT60 be31dac3 is the model).
cd "$(git rev-parse --show-toplevel)" || exit 1
fail() { echo "GATE FAIL: $*"; exit 1; }
levels=n52-foundry,n52-iceline,n57-kiln,n52-span,n52-spire
if [ "$1" = "--flow-base" ]; then
  mkdir -p Logs/n61
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level "$levels" --seeds 10 --out Logs/n61/flow-base.txt > /dev/null 2>&1
  [ -f Logs/n61/flow-base-a0.txt ] || cp Logs/n61/flow-base.txt Logs/n61/flow-base-a0.txt   # the night's first base, kept: slices are judged against it
  echo "flow base: Logs/n61/flow-base.txt, $(grep -c 'SUMMARY' Logs/n61/flow-base.txt) level summaries, $(date +%T)"
  exit 0
fi
if [ "$1" = "--flow" ] || [ "$2" = "--flow" ]; then
  [ -f Logs/n61/flow-base.txt ] || fail "no Logs/n61/flow-base.txt (run --flow-base first)"
  rm -f Logs/n61/flow-off61.txt   # never compare a stale run (FORGE61 red team 20)
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level "$levels" --seeds 10 --off61 --out Logs/n61/flow-off61.txt > /dev/null 2>&1
  [ -s Logs/n61/flow-off61.txt ] || fail "n46flow wrote no Logs/n61/flow-off61.txt"
  if diff <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n61/flow-base.txt) <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n61/flow-off61.txt) > /dev/null; then echo "gate flow --off61: EQUAL to flow-base"; else fail "flow --off61 differs from flow-base (Logs/n61/flow-off61.txt)"; fi
fi
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "gate config: the two copies identical" || fail "config copies differ"
g=$(dotnet run --project Tools/CoreHarness -c Release 2>&1 | grep "golden total")
echo "gate golden: $g"; echo "$g" | grep -q "0 failing, 0 mismatches" || fail golden
for o in "" "--off57" "--off58" "--off59" "--off60" "--off61"; do
  f=$(dotnet run --project Tools/SimHarness -c Release -- fingerprint $o 2>&1 | grep "fingerprint seed")
  echo "gate fingerprint $o: $f"; echo "$f" | grep -q "EQUAL" || fail "fingerprint $o"
done
if [ "$1" != "--no-editor" ] && [ "$2" != "--no-editor" ]; then
  bash Tools/unity-loop.sh compile 2>&1 | tail -1
  e=$(bash Tools/unity-loop.sh errors 2>&1 | grep -E '"compilationFailed"|"consoleErrors"' | head -2 | tr -d ' \n')
  echo "gate editor: $e"
  echo "$e" | grep -q '"compilationFailed":false' || fail "editor compile"
  echo "$e" | grep -qE '"consoleErrors":0(,|$)' || fail "editor console errors"
fi
echo "gates: all GREEN $(date +%T)"
