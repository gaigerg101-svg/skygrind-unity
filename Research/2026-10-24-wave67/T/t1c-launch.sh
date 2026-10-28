#!/usr/bin/env bash
# NIGHT67 builder T, the launch pilot (the lead: kb3 launched nothing, v2's dash-punch killed the husk outright): `n65verb launch` on SHIPPED
# rooms, --launch-v1 (NIGHT65's pilot: the planted RED) against the fix; level 1's launch rooms (n52-foundry a5, b1, a5 --slide) old vs new;
# and the entry-loadout probe (kb3 in the three instruments). The built DLL; one job at a time. Output: t1c-launch.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
D=Temp/SimHarness/bin/Release/net10.0/SimHarness.dll; f=Research/2026-10-24-wave67/T/t1c-launch.txt
X="${TMPDIR:-/tmp}/t1c-launch-$$"; mkdir -p "$X"
{
  echo "# sha $(git rev-parse --short HEAD) + the uncommitted launch pilot / entry probe (Harness.N65.Verb.cs, Harness.N46.Flow.cs); $(date '+%F %T')"
  for c in "n57-kiln kb3" "n52-spire v2" "n57-kiln kb2c" "n52-iceline g8"; do
    set -- $c
    for v in "--launch-v1" ""; do
      echo; echo "## dotnet $D n65verb launch --level Design/levels/$1.json --room $2 $v"
      timeout 300 dotnet $D n65verb launch --level Design/levels/$1.json --room $2 $v < /dev/null 2>&1 | grep -E "entry loadout|the hand:|target chosen|  target |^n65verb launch: (GREEN|RED)" | cut -c1-330
    done
  done
  echo; echo "## level 1 (n52-foundry): --launch-v1 against the fix, the entry / put-in lines and wall time aside"
  for c in "a5" "b1" "a5 --slide"; do
    timeout 300 dotnet $D n65verb launch --level Design/levels/n52-foundry.json --room $c --launch-v1 < /dev/null > "$X/old.txt" 2>&1
    timeout 300 dotnet $D n65verb launch --level Design/levels/n52-foundry.json --room $c < /dev/null > "$X/new.txt" 2>&1
    strip() { grep -v -E "put-in loadout|entry loadout|wall time|target chosen" "$1" | sed -E 's/, [0-9.]+ m off \(reach.*$//'; }
    if diff <(strip "$X/old.txt") <(strip "$X/new.txt") > /dev/null; then echo "IDENTICAL $c: $(grep -E '^n65verb launch: ' "$X/new.txt" | tail -1 | cut -c1-150)"; else echo "DIFFERS $c:"; diff <(strip "$X/old.txt") <(strip "$X/new.txt") | head -6 | cut -c1-250; fi
  done
  echo "# done $(date +%T)"
} > "$f" 2>&1
rm -rf "$X"
cut -c1-300 "$f"
