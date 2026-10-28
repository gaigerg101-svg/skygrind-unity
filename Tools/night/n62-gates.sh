#!/usr/bin/env bash
# NIGHT62 gates (the forge FORGE62, 2026-10-22; n61-gates.sh with the n62 names, --off62 and a bounded Editor step): the integration gates,
# in order: config copies identical, golden, fingerprint plain / --off57 .. --off62 (EQUAL), the Editor compile (stops Play mode first) with
# "compilationFailed": false AND 0 console errors. One line per gate; exits 1 at the first failure.
#   bash Tools/night/n62-gates.sh [--no-editor] [--flow]      the gates
#   bash Tools/night/n62-gates.sh --flow-base                  writes the --flow baseline Logs/n62/flow-base.txt once (~5 min, bearings)
# The Editor step is bounded (NIGHT61's close: `unity-loop.sh compile` polled a deaf Editor for ~80 min): editor_status must answer in 60 s
# and the compile in 300 s, else "GATE FAIL: editor compile (... not checked)". It checks the tree it is run from (`git rev-parse
# --show-toplevel`), so a builder's worktree checks itself; builders pass --no-editor (only the lead drives the Editor).
# --flow: n46flow on n52-foundry / n52-iceline / n57-kiln / n52-span / n52-spire, seeds 1-10, with --off62 must equal Logs/n62/flow-base.txt
# line for line, wall-time and header lines aside. A level-data slice changes its own level's lines: re-make the base in its commit, say so.
cd "$(git rev-parse --show-toplevel)" || exit 1
fail() { echo "GATE FAIL: $*"; exit 1; }
levels=n52-foundry,n52-iceline,n57-kiln,n52-span,n52-spire
if [ "$1" = "--flow-base" ]; then
  mkdir -p Logs/n62
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level "$levels" --seeds 10 --out Logs/n62/flow-base.txt > /dev/null 2>&1
  [ -f Logs/n62/flow-base-a0.txt ] || cp Logs/n62/flow-base.txt Logs/n62/flow-base-a0.txt   # the night's first base, kept: slices are judged against it
  echo "flow base: Logs/n62/flow-base.txt, $(grep -c 'SUMMARY' Logs/n62/flow-base.txt) level summaries, $(date +%T)"
  exit 0
fi
if [ "$1" = "--flow" ] || [ "$2" = "--flow" ]; then
  [ -f Logs/n62/flow-base.txt ] || fail "no Logs/n62/flow-base.txt (run --flow-base first)"
  rm -f Logs/n62/flow-off62.txt   # never compare a stale run
  dotnet run --project Tools/SimHarness -c Release -- n46flow --level "$levels" --seeds 10 --off62 --out Logs/n62/flow-off62.txt > /dev/null 2>&1
  [ -s Logs/n62/flow-off62.txt ] || fail "n46flow wrote no Logs/n62/flow-off62.txt"
  if diff <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n62/flow-base.txt) <(grep -v -i -E 'wall time|^n46flow: seeds' Logs/n62/flow-off62.txt) > /dev/null; then echo "gate flow --off62: EQUAL to flow-base"; else fail "flow --off62 differs from flow-base (Logs/n62/flow-off62.txt)"; fi
fi
cmp -s Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && echo "gate config: the two copies identical" || fail "config copies differ"
g=$(dotnet run --project Tools/CoreHarness -c Release 2>&1 | grep "golden total")
echo "gate golden: $g"; echo "$g" | grep -q "0 failing, 0 mismatches" || fail golden
for o in "" "--off57" "--off58" "--off59" "--off60" "--off61" "--off62"; do
  f=$(dotnet run --project Tools/SimHarness -c Release -- fingerprint $o 2>&1 | grep "fingerprint seed")
  echo "gate fingerprint $o: $f"; echo "$f" | grep -q "EQUAL" || fail "fingerprint $o"
done
if [ "$1" != "--no-editor" ] && [ "$2" != "--no-editor" ]; then
  s=$(timeout 60 bash Tools/unity-loop.sh status 2>&1)
  echo "$s" | grep -q '"playMode"' || fail "editor compile (editor_status gave no answer in 60 s: not checked)"
  c=$(timeout 300 bash Tools/unity-loop.sh compile 2>&1); rc=$?
  [ "$rc" -eq 124 ] && fail "editor compile (no answer in 300 s: not checked)"
  echo "$c" | tail -1
  e=$(timeout 120 bash Tools/unity-loop.sh errors 2>&1 | grep -E '"compilationFailed"|"consoleErrors"' | head -2 | tr -d ' \n')
  echo "gate editor: $e"
  echo "$e" | grep -q '"compilationFailed":false' || fail "editor compile"
  echo "$e" | grep -qE '"consoleErrors":0(,|$)' || fail "editor console errors"
fi
echo "gates: all GREEN $(date +%T)"
