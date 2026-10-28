#!/usr/bin/env bash
# NIGHT52 (the lead's; DECIDE52 D-15): the room contract's flow items measured per room in ISOLATION (n46flow --rooms <id>: the walk starts at
# the room and ends when it clears), so an edit upstream in the level cannot move a room's numbers (the chained run carries the pilot's state:
# g7's dwell moved 2.9 -> 10.0 s on seeds 1-10 when only g5 changed). Base = the n47 / n48 level with --off52, candidate = the n52 level; seeds
# 1-10 and 11-20 each. Output: Logs/n52/roomflow/<level>-<room>-{base,cand}-{A,B}.txt; the room check reads them as
#   --flow-before <base-A>,<base-B> --flow <cand-A>,<cand-B>
#   bash Tools/night/n52-room-flow.sh <base level id> <candidate level id> <room> [room ...]
cd "$(git rev-parse --show-toplevel)" || exit 1
base="${1:?base}"; cand="${2:?cand}"; shift 2
out=Logs/n52/roomflow; mkdir -p "$out"
dotnet build Tools/SimHarness -c Release -nologo -v q 2>&1 | grep -E " error |rror\(s\)"
dll=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll
for r in "$@"; do
  dotnet "$dll" n46flow --level "$base" --rooms "$r" --seeds 10 --first 1 --off52 --no-geo > "$out/$cand-$r-base-A.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$base" --rooms "$r" --seeds 10 --first 11 --off52 --no-geo > "$out/$cand-$r-base-B.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$cand" --rooms "$r" --seeds 10 --first 1 > "$out/$cand-$r-cand-A.txt" 2>&1 &
  dotnet "$dll" n46flow --level "$cand" --rooms "$r" --seeds 10 --first 11 --no-geo > "$out/$cand-$r-cand-B.txt" 2>&1 &
done
wait
for r in "$@"; do
  for s in base-A base-B cand-A cand-B; do printf '%-34s ' "$cand-$r-$s"; grep -E "^\s+$r\s+(room|boss)" "$out/$cand-$r-$s.txt" | cut -c1-150; done
done
