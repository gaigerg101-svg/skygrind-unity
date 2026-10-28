#!/usr/bin/env bash
# NIGHT40 builder B5: regenerate level 5 as n40-spire (design.n40.crown40 swaps n38-spire for it): the plan (Tools/night/gen-n40-spire.cjs,
# from n38-spire's plan), the jobs checker, levelgen (the report's ERROR rows), then the copy the game reads (Design/levels/n40-spire.json)
# and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n40-spire-regen.sh
node Tools/night/gen-n40-spire.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n40/spire.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== n40-spire"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n40/spire.plan.json Design/levels/n40/spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    " | cut -c1-400 | head -60
cp Design/levels/n40/spire.json Design/levels/n40-spire.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n40-spire.json Assets/StreamingAssets/Design/levels/n40-spire.json
if cmp -s Design/levels/n40-spire.json Assets/StreamingAssets/Design/levels/n40-spire.json; then echo "  copies identical: n40-spire.json"; else echo "  COPY MISMATCH: n40-spire.json"; exit 1; fi
