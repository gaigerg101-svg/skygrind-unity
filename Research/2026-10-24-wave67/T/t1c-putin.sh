#!/usr/bin/env bash
# NIGHT67 builder T, the put-in loadout fix's planted pairs (the lead's go): skip v2 and n46flow on SHIPPED rooms, --no-put-in-awards (the old
# put-in: the RED) against the fix (the GREEN). The built DLL (dotnet build first, by the caller); one job at a time. Output: t1c-putin.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll; f=Research/2026-10-24-wave67/T/t1c-putin.txt
{
  echo "# sha $(git rev-parse --short HEAD) + the uncommitted put-in fix (Harness.N65.Verb.cs V67PutInAwards, Harness.N46.Flow.cs); DLL $(ls -la --time-style=+%T $D | awk '{print $6}'); $(date '+%F %T')"
  for c in "n52-iceline g7 parry" "n57-kiln kb3 launch"; do
    set -- $c
    for off in "--no-put-in-awards" ""; do
      echo; echo "## dotnet $D n65verb skip --level Design/levels/$1.json --room $2 --plant-verb65 $2:$3 --seeds 2 $off"
      timeout 900 dotnet $D n65verb skip --level Design/levels/$1.json --room $2 --plant-verb65 $2:$3 --seeds 2 $off < /dev/null 2>&1 | grep -E "put-in loadout|seed [0-9]+ (verb-aware|refused-in|walker)|^n65verb skip: (M1|totals)" | sed -E 's/\| STYLE.*walked/| ... walked/; s/(shots [0-9]+ \(charged [0-9]+\)).*/\1/'
    done
  done
  for off in "--no-put-in-awards" ""; do
    echo; echo "## dotnet $D n46flow --level n52-iceline --rooms g7 --seeds 2 --first 1 $off"
    timeout 900 dotnet $D n46flow --level n52-iceline --rooms g7 --seeds 2 --first 1 $off < /dev/null 2>&1 | grep -E "put-in loadout|SUMMARY|^  g7 "
  done
  echo; echo "# done $(date +%T)"
} > "$f" 2>&1
cut -c1-240 "$f"
