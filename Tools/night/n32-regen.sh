#!/usr/bin/env bash
# NIGHT32 builder A (a copy of n30-regen.sh for the arenas32 levels): regenerate the n32 plans, run levelgen on each (the report prints ERROR rows only), and copy each
# map to Design/levels/n32-<name>.json (the file the game reads with design.n32.arenas32 on) and its StreamingAssets copy. Usage, from the repo (or worktree) root:
#   bash Tools/night/n32-regen.sh [foundry|iceline|kiln ...]
names="${*:-foundry kiln iceline}"
for g in $names; do
  node "Tools/night/gen-n32-$g.cjs" || exit 1
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n32/$g.plan.json" "Design/levels/n32/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n32/$g.json" "Design/levels/n32-$g.json"
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n32-$g.json" "Assets/StreamingAssets/Design/levels/n32-$g.json"
  cmp -s "Design/levels/n32-$g.json" "Assets/StreamingAssets/Design/levels/n32-$g.json" && echo "  copies identical: n32-$g.json"
done
