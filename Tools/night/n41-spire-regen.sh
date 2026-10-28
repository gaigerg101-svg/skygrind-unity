#!/usr/bin/env bash
# NIGHT41 builder R5 (spire41): regenerate level 5 as n41-spire (design.n41.spire41 swaps n40-spire for it): the plan (Tools/night/gen-n41-spire.cjs,
# from n40-spire's plan), the jobs checker, levelgen (the report's ERROR rows), then the copy the game reads (Design/levels/n41-spire.json)
# and the Player's copy under Assets/StreamingAssets (cp + cmp). A copy of n40-spire-regen.sh with the n41 paths.
# Usage, from the repo (or worktree) root:  bash Tools/night/n41-spire-regen.sh
node Tools/night/gen-n41-spire.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n41/spire.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== n41-spire"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n41/spire.plan.json Design/levels/n41/spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    |grapple nodes" | cut -c1-400 | head -60
cp Design/levels/n41/spire.json Design/levels/n41-spire.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n41-spire.json Assets/StreamingAssets/Design/levels/n41-spire.json
if cmp -s Design/levels/n41-spire.json Assets/StreamingAssets/Design/levels/n41-spire.json; then echo "  copies identical: n41-spire.json"; else echo "  COPY MISMATCH: n41-spire.json"; exit 1; fi
