#!/usr/bin/env bash
# NIGHT67 builder T, T1c step 2's proof (the lead: the ride pilot's gun for M3): `n65verb ride --shoot` on SHIPPED rooms, the planted RED
# (--weapon gauntlet: the hand the run had, 0 shots) and the GREEN (the best owned gun at that room after the level's awards up to it).
# n65-verbs.sh builds the harness first (one dotnet job in this tree). Output: Research/2026-10-24-wave67/T/t1c-ridegun.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
f=Research/2026-10-24-wave67/T/t1c-ridegun.txt
{
  echo "# sha $(git rev-parse --short HEAD) + the uncommitted Tools/SimHarness/Harness.N65.Verb.cs (V67RideGun); $(date '+%F %T')"
  for c in "n52-iceline g8 --shoot --weapon gauntlet" "n52-iceline g8 --shoot" "n52-iceline g8 --shoot --plant-target auto --weapon gauntlet" "n52-iceline g8 --shoot --plant-target auto" "n52-iceline g7 --shoot" "n57-kiln kb3 --shoot --weapon gauntlet" "n57-kiln kb3 --shoot" "n57-kiln kb3c --shoot" "n52-spire v6 --shoot" "n52-spire v2 --shoot"; do
    set -- $c; lv=$1; room=$2; shift 2
    echo; echo "## bash Tools/night/n65-verbs.sh ride Design/levels/$lv.json $room $*"
    bash Tools/night/n65-verbs.sh ride "Design/levels/$lv.json" "$room" "$@" 2>&1 | grep -E "the ride's gun|^n65verb ride|face [0-9]+ of|TIMEOUT|build failed"
  done
  echo; echo "# done $(date +%T)"
} > "$f" 2>&1
grep -E "^## |the ride's gun|^n65verb ride --shoot" "$f" | cut -c1-260
