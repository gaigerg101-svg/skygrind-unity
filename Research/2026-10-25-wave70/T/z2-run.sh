#!/usr/bin/env bash
# NIGHT70 builder T, slice Z2's pair: one skip v2 run (seeds 1-N, planted verb65 launch when the room's row has none) from a given harness
# build (a scratch copy of Temp/SimHarness/bin/Release/net10.0), its lines kept as n65-verbs.sh keeps them (notes and warnings dropped).
#   bash Research/2026-10-25-wave70/T/z2-run.sh <dll dir> <level.json> <room> <out> [seeds, default 3] [extra n65verb args ...]
cd "$(git rev-parse --show-toplevel)" || exit 1
dll="${1:?dll}"; lv="${2:?level}"; room="${3:?room}"; out="${4:?out}"; n="${5:-3}"; shift 5 2>/dev/null
{ echo "# command: dotnet $dll/SimHarness.dll n65verb skip --level $lv --room $room --seeds $n --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json $*"
  echo "# sha: $(git rev-parse --short HEAD) + the working copy (the build in $dll); time: $(date '+%F %T')"
  timeout 900 dotnet "$dll/SimHarness.dll" n65verb skip --level "$lv" --room "$room" --seeds "$n" --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json "$@" 2>&1 | grep -v -E "^  note:|warning (CS|MSB)"
  echo "exit ${PIPESTATUS[0]} at $(date +%T)"; } > "$out"
echo "$out: $(grep -c '' "$out") lines, $(tail -1 "$out")"
