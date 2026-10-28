#!/usr/bin/env bash
# NIGHT46 builder A2 (spire46): regenerate level 5 as n46-spire (design.n46.spire46 swaps n41-spire for it): the plan (Tools/night/gen-n46-spire.cjs, from
# n41-spire's plan: v2 / v6 / the belfry gates rebuilt from their NIGHT46 identity cards), the jobs checker, levelgen (0 errors wanted) straight into the
# copy the game reads (Design/levels/n46-spire.json), then the Player's copy under Assets/StreamingAssets (cp + cmp). A copy of n45-iceline-regen.sh with
# the n46-spire paths. levelgen also rewrites Reports/LEVEL-n46-spire-validate.md.
# Usage, from the repo (or worktree) root:  bash Tools/night/n46-spire-regen.sh
node Tools/night/gen-n46-spire.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n46/spire.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n46/spire.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n46/spire.plan.json Design/levels/n46-spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n46-spire.json Assets/StreamingAssets/Design/levels/n46-spire.json
if cmp -s Design/levels/n46-spire.json Assets/StreamingAssets/Design/levels/n46-spire.json; then echo "  copies identical: n46-spire.json"; else echo "  COPY MISMATCH: n46-spire.json"; exit 1; fi
