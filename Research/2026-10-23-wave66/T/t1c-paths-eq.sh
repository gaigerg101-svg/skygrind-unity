#!/usr/bin/env bash
# NIGHT66 T1c: the round's instruments read a level from a folder: n59-l-room.py --dir and n46flow --file, each against its tracked-path run
# on the SAME bytes (a scratch copy of the tracked n65-foundry level, plan and openings), outputs diffed (n46flow's wall-time and --file
# lines aside). Equal = the path plumbing changes nothing.
#   bash Research/2026-10-23-wave66/T/t1c-paths-eq.sh <scratch dir, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
SC="${1:?scratch dir}"; EQ="$SC/eq"; mkdir -p "$EQ"
cp Design/levels/n65-foundry.json "$EQ/n65-foundry.json"; cp Design/levels/n65/foundry.plan.json "$EQ/foundry.plan.json"; cp Design/levels/n65/openings65.json "$EQ/openings65.json"
for r in b1 a3; do
  python -I Tools/night/n59-l-room.py n65-foundry room $r > "$SC/room-$r-tracked.txt" 2>&1
  python -I Tools/night/n59-l-room.py n65-foundry room $r --dir "$EQ" > "$SC/room-$r-dir.txt" 2>&1
  if cmp -s "$SC/room-$r-tracked.txt" "$SC/room-$r-dir.txt"; then echo "n59-l-room room $r: --dir EQUAL to the tracked paths ($(wc -l < "$SC/room-$r-dir.txt") lines; last: $(tail -1 "$SC/room-$r-dir.txt" | cut -c1-120))"; else echo "n59-l-room room $r: --dir DIFFERS"; diff "$SC/room-$r-tracked.txt" "$SC/room-$r-dir.txt" | head -10; fi
done
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
dotnet "$D" n46flow --level n65-foundry --rooms b1 --seeds 3 > "$SC/flow-tracked.txt" 2>&1
dotnet "$D" n46flow --level n65-foundry --rooms b1 --seeds 3 --file "$EQ/n65-foundry.json" > "$SC/flow-file.txt" 2>&1
if diff <(grep -v -i -E 'wall time|: --file ' "$SC/flow-tracked.txt") <(grep -v -i -E 'wall time|: --file ' "$SC/flow-file.txt") > /dev/null; then echo "n46flow --rooms b1 --seeds 3: --file EQUAL to --level n65-foundry ($(wc -l < "$SC/flow-file.txt") lines; $(grep -m1 SUMMARY "$SC/flow-file.txt" | cut -c1-140))"; else echo "n46flow: --file DIFFERS"; diff <(grep -v -i 'wall time' "$SC/flow-tracked.txt") <(grep -v -i 'wall time' "$SC/flow-file.txt") | head -10; fi
grep -m1 ": --file " "$SC/flow-file.txt"
