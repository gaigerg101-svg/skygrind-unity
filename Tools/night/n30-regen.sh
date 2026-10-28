#!/usr/bin/env bash
# NIGHT30 (builder L; a copy of NIGHT29's n29-regen.sh): regenerate the n30 plans, run levelgen on each (the report prints ERROR rows only), and copy each
# map to Design/levels/n30-<name>.json (the file the game reads; the lead syncs StreamingAssets). Usage, from the repo root:
#   bash Tools/night/n30-regen.sh [foundry|iceline|kiln|span ...]
names="${*:-foundry iceline kiln span}"
for g in $names; do
  node "Tools/night/gen-n30-$g.cjs" || exit 1
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n30/$g.plan.json" "Design/levels/n30/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n30/$g.json" "Design/levels/n30-$g.json"
done
