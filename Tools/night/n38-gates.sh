#!/usr/bin/env bash
# NIGHT38 builder E: the gate checker over every NIGHT38 level plan (or the plans given), one harness run per plan (one heavy job at a time).
#   bash Tools/night/n38-gates.sh                      every Design/levels/n38/*.plan.json and Design/levels/n37/*.plan.json with a gate tag
#   bash Tools/night/n38-gates.sh <plan.json> ...      only these
#   bash Tools/night/n38-gates.sh --selftest           the checker's own planted-gate selftest
# Prints each plan's GREEN / RED line; exits 1 when any plan is RED. The checker: Tools/SimHarness/Harness.N38.Gates.cs (n38 gates).
REPO="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO" || exit 1
if [ "$1" = "--selftest" ]; then
  dotnet run --project Tools/SimHarness -c Release -- n38 gates --selftest 2>&1 | grep -v "warning"
  exit "${PIPESTATUS[0]}"
fi
plans=("$@")
if [ ${#plans[@]} -eq 0 ]; then
  for f in Design/levels/n38/*.plan.json Design/levels/n37/*.plan.json; do
    [ -f "$f" ] && grep -q '"gate"' "$f" && plans+=("$f")
  done
fi
if [ ${#plans[@]} -eq 0 ]; then echo "n38-gates: no plan carries a gate yet"; exit 0; fi
bad=0
for p in "${plans[@]}"; do
  out=$(dotnet run --project Tools/SimHarness -c Release -- n38 gates "$p" 2>&1 | grep -v "warning")
  rc=$?
  echo "$out"
  echo "$out" | tail -1 | grep -q "GREEN" || bad=$((bad + 1))
done
echo "n38-gates: ${#plans[@]} plan(s), $bad RED"
[ "$bad" -eq 0 ]
