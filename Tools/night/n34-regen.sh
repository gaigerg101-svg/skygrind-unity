#!/usr/bin/env bash
# NIGHT34 builder A: regenerate the n34 levels (design.n34.lines34 swaps n33-* for n34-*): the plan (Tools/night/gen-n34-<name>.cjs: the n33 generator
# plus the lines34 pass), builder K's dressing pass on it (dress33.cjs: calm34 filters it at runtime), levelgen (the report's ERROR rows), then the copy the
# game reads (Design/levels/n34-<name>.json) and the Player's copy under this checkout's Assets/StreamingAssets (cp + cmp), as n33-regen.sh.
# Usage, from the repo (or worktree) root:
#   bash Tools/night/n34-regen.sh [foundry|kiln ...]   (no names: every gen-n34-*.cjs there is)
if [ $# -gt 0 ]; then names="$*"; else names=$(ls Tools/night/gen-n34-*.cjs 2>/dev/null | sed -e 's|.*gen-n34-||' -e 's|\.cjs$||'); fi
for g in $names; do
  node "Tools/night/gen-n34-$g.cjs" || exit 1
  node Tools/night/dress33.cjs "Design/levels/n34/$g.plan.json" || exit 1
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n34/$g.plan.json" "Design/levels/n34/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
  cp "Design/levels/n34/$g.json" "Design/levels/n34-$g.json" || exit 1
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n34-$g.json" "Assets/StreamingAssets/Design/levels/n34-$g.json"
  if cmp -s "Design/levels/n34-$g.json" "Assets/StreamingAssets/Design/levels/n34-$g.json"; then echo "  copies identical: n34-$g.json"; else echo "  COPY MISMATCH: n34-$g.json"; exit 1; fi
done
