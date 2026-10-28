#!/usr/bin/env bash
# NIGHT29 (builder R): regenerate the three n29 plans, run levelgen on each (the report prints ERROR rows only), and copy each map to
# Design/levels/n29-<name>.json (the file the game reads). Usage, from the repo root: bash Tools/night/n29-regen.sh [foundry|iceline|kiln ...]
names="${*:-foundry iceline kiln}"
for g in $names; do
  node "Tools/night/gen-n29-$g.cjs" || exit 1
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n29/$g.plan.json" "Design/levels/n29/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n29/$g.json" "Design/levels/n29-$g.json"
done
