#!/usr/bin/env bash
# NIGHT35 builder A: regenerate the n35 levels (design.n35.campaign6 swaps n34-* for n35-* and adds n35-finale): the plan (Tools/night/gen-n35-<name>.cjs),
# builder K's dressing pass on it (dress33.cjs), levelgen (the report's ERROR rows), then the copy the game reads (Design/levels/n35-<name>.json) and the
# Player's copy under this checkout's Assets/StreamingAssets (cp + cmp), as n34-regen.sh.
# Usage, from the repo (or worktree) root:
#   bash Tools/night/n35-regen.sh [foundry|iceline|kiln|finale ...]   (no names: every gen-n35-*.cjs there is)
if [ $# -gt 0 ]; then names="$*"; else names=$(ls Tools/night/gen-n35-*.cjs 2>/dev/null | sed -e 's|.*gen-n35-||' -e 's|\.cjs$||'); fi
for g in $names; do
  node "Tools/night/gen-n35-$g.cjs" || exit 1
  node Tools/night/dress33.cjs "Design/levels/n35/$g.plan.json" || exit 1
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n35/$g.plan.json" "Design/levels/n35/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n35/$g.json" "Design/levels/n35-$g.json" || exit 1
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n35-$g.json" "Assets/StreamingAssets/Design/levels/n35-$g.json"
  if cmp -s "Design/levels/n35-$g.json" "Assets/StreamingAssets/Design/levels/n35-$g.json"; then echo "  copies identical: n35-$g.json"; else echo "  COPY MISMATCH: n35-$g.json"; exit 1; fi
done
