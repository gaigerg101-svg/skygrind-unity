#!/usr/bin/env bash
# NIGHT70 builder T, slice Z3's planted pair: skip v2 on the SHIPPED kb3 (n57-kiln) and v2 (n52-spire), seeds 1-3, planted verb65 launch (the
# reds' own command, as NIGHT69 made them), run from a given harness build (a copy of Temp/SimHarness/bin/Release/net10.0), one at a time.
#   bash Research/2026-10-25-wave70/T/z3-pair.sh <dll dir> <tag> [seeds, default 3] [extra n65verb args ...]
# Writes Research/2026-10-25-wave70/T/Z3-skip-<room>-<tag>.txt (the harness's lines, as n65-verbs.sh keeps them: notes and warnings dropped).
cd "$(git rev-parse --show-toplevel)" || exit 1
dll="${1:?dll dir}"; tag="${2:?tag}"; n="${3:-3}"; shift 3 2>/dev/null
for r in "n57-kiln kb3" "n52-spire v2"; do
  set -- $r "$@"; lv="$1"; room="$2"; shift 2
  out="Research/2026-10-25-wave70/T/Z3-skip-$room-$tag.txt"
  { echo "# command: dotnet $dll/SimHarness.dll n65verb skip --level Design/levels/$lv.json --room $room --seeds $n --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json --plant-verb65 $room:launch $*"
    echo "# sha: $(git rev-parse --short HEAD) (the build in $dll); time: $(date '+%F %T')"
    timeout 900 dotnet "$dll/SimHarness.dll" n65verb skip --level "Design/levels/$lv.json" --room "$room" --seeds "$n" --bars C:/Users/gaige/dev/SKYGRIND-unity/Logs/n70/slate.json --plant-verb65 "$room:launch" "$@" 2>&1 | grep -v -E "^  note:|warning (CS|MSB)"
    echo "exit ${PIPESTATUS[0]} at $(date +%T)"; } > "$out"
  echo "$out: $(grep -c '' "$out") lines, $(tail -1 "$out")"
done
