#!/usr/bin/env bash
# NIGHT67 builder T, the verb pilots' put-in (the lead: R's g7 parry M3 RED, the gauntlet in hand): `n65verb parry|launch|pierce` on SHIPPED
# rooms, --no-put-in-awards (the old put-in: the RED) against the fix; level 1 unchanged (a4 parry, a5 launch, a8 pierce on n52-foundry:
# identical apart from the put-in line). The built DLL (built first by the caller); one job at a time. Output: t1c-pilot-putin.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll; f=Research/2026-10-24-wave67/T/t1c-pilot-putin.txt
X="${TMPDIR:-/tmp}/t1c-pilot-$$"; mkdir -p "$X"
{
  echo "# sha $(git rev-parse --short HEAD) + the uncommitted V65Start put-in (Harness.N65.Verb.cs); DLL $(ls -la --time-style=+%T $D | awk '{print $6}'); $(date '+%F %T')"
  for c in "n52-iceline g7 parry" "n57-kiln kb3 launch" "n57-kiln kb3c pierce"; do
    set -- $c
    for off in "--no-put-in-awards" ""; do
      echo; echo "## dotnet $D n65verb $3 --level Design/levels/$1.json --room $2 $off"
      timeout 600 dotnet $D n65verb $3 --level Design/levels/$1.json --room $2 $off < /dev/null 2>&1 | grep -E "put-in loadout|^n65verb $3: (GREEN|RED|UNSURE)|weapon|punch|parried" | cut -c1-260
    done
  done
  echo; echo "## level 1 unchanged: n52-foundry a4 parry / a5 launch / a8 pierce, the old put-in against the fix (the put-in line and wall time aside)"
  for c in "a4 parry" "a5 launch" "a8 pierce"; do
    set -- $c
    timeout 600 dotnet $D n65verb $2 --level Design/levels/n52-foundry.json --room $1 --no-put-in-awards < /dev/null > "$X/old.txt" 2>&1
    timeout 600 dotnet $D n65verb $2 --level Design/levels/n52-foundry.json --room $1 < /dev/null > "$X/new.txt" 2>&1
    if diff <(grep -v -E "put-in loadout|wall time" "$X/old.txt") <(grep -v -E "put-in loadout|wall time" "$X/new.txt") > /dev/null; then echo "IDENTICAL $1 $2 ($(wc -l < "$X/new.txt") lines): $(grep -E "^n65verb $2: " "$X/new.txt" | cut -c1-140)"; else echo "DIFFERS $1 $2:"; diff <(grep -v -E "put-in loadout|wall time" "$X/old.txt") <(grep -v -E "put-in loadout|wall time" "$X/new.txt") | head -6; fi
  done
  echo "# done $(date +%T)"
} > "$f" 2>&1
rm -rf "$X"
cut -c1-260 "$f"
