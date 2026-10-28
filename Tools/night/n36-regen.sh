#!/usr/bin/env bash
# NIGHT36 builder L: regenerate the n36 levels (design.n36.level36 swaps n35-foundry for n36-foundry): the plan (Tools/night/gen-n36-<name>.cjs),
# the jobs checker (Tools/night/n36-jobs.cjs: every solid has a job, lava an enemy can reach, a climb route per raised fight room), levelgen (the
# report's ERROR rows), then the copy the game reads (Design/levels/n36-<name>.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# No dressing pass: every solid in an n36 level is the generator's, with a job (PLAN.md 3.3).
# Usage, from the repo (or worktree) root:  bash Tools/night/n36-regen.sh [foundry ...]
if [ $# -gt 0 ]; then names="$*"; else names=$(ls Tools/night/gen-n36-*.cjs 2>/dev/null | sed -e 's|.*gen-n36-||' -e 's|\.cjs$||'); fi
for g in $names; do
  node "Tools/night/gen-n36-$g.cjs" || exit 1
  node Tools/night/n36-jobs.cjs "Design/levels/n36/$g.plan.json" | tail -1
  node Tools/night/n36-jobs.cjs "Design/levels/n36/$g.plan.json" > /dev/null || { echo "  n36-jobs RED: $g"; exit 1; }
  echo "== $g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n36/$g.plan.json" "Design/levels/n36/$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report" | cut -c1-600
  cp "Design/levels/n36/$g.json" "Design/levels/n36-$g.json" || exit 1
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n36-$g.json" "Assets/StreamingAssets/Design/levels/n36-$g.json"
  if cmp -s "Design/levels/n36-$g.json" "Assets/StreamingAssets/Design/levels/n36-$g.json"; then echo "  copies identical: n36-$g.json"; else echo "  COPY MISMATCH: n36-$g.json"; exit 1; fi
done
