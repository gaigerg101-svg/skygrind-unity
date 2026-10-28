#!/usr/bin/env bash
# NIGHT66 T1c: `n65verb ride` without and with --shoot, and with a planted target in reach of the wall, on the latest a3 (a level made by
# gen-n65-foundry.cjs --slots=latest --run --out=<scratch>, NIGHT65's final rooms) and on the old n52-foundry b1 (its crane rails).
#   bash Research/2026-10-23-wave66/T/t1c-ride.sh <the latest level file, C:/...>
cd "$(git rev-parse --show-toplevel)" || exit 1
L="${1:?the latest level}"
echo "# the latest level: $L, sha256 $(sha256sum "$L" | cut -c1-16)"
H="dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb ride"
for lv in "$L a3" "Design/levels/n52-foundry.json b1"; do
  set -- $lv
  for a in "" "--shoot" "--shoot --plant-target auto"; do
    echo "== n65verb ride --level $1 --room $2 $a"
    $H --level "$1" --room "$2" $a 2>&1 | grep -v -E "^  note:|warning (CS|MSB)"
    echo "   exit ${PIPESTATUS[0]}"
  done
done
