#!/usr/bin/env bash
# NIGHT33 builder R: regenerate the n33 levels (design.n33.kit33 swaps n32-* for n33-*): the plan (Tools/night/gen-n33-<name>.cjs, R's room shapes), then builder K's
# dressing pass on the plan IF Tools/night/dress33.cjs exists (node Tools/night/dress33.cjs <plan>), then levelgen (the report prints ERROR rows only), then the copy
# the game reads (Design/levels/n33-<name>.json) and its Player copy under this checkout's Assets/StreamingAssets (cp + cmp; n24-sync.sh copies the MAIN repo's files).
# Usage, from the repo (or worktree) root:
#   bash Tools/night/n33-regen.sh [foundry|iceline|kiln|span|spire ...]   (no names: all five)
names="${*:-foundry iceline kiln span spire}"
for g in $names; do
  node "Tools/night/gen-n33-$g.cjs" || exit 1
  if [ -f Tools/night/dress33.cjs ]; then
    node Tools/night/dress33.cjs "Design/levels/n33/$g.plan.json" || exit 1
  fi
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n33/$g.plan.json" "Design/levels/n33/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n33/$g.json" "Design/levels/n33-$g.json" || exit 1
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n33-$g.json" "Assets/StreamingAssets/Design/levels/n33-$g.json"
  if cmp -s "Design/levels/n33-$g.json" "Assets/StreamingAssets/Design/levels/n33-$g.json"; then echo "  copies identical: n33-$g.json"; else echo "  COPY MISMATCH: n33-$g.json"; exit 1; fi
done
