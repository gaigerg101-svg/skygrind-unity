#!/usr/bin/env bash
# NIGHT41 builder R5 (foundry41): regenerate level 1 as n41-foundry (design.n41.foundry41 swaps n38-foundry for it): the plan
# (Tools/night/gen-n41-foundry.cjs, from n38-foundry's plan), the jobs checker, levelgen (the report's ERROR rows), then the copy the game reads
# (Design/levels/n41-foundry.json) and the Player's copy under Assets/StreamingAssets (cp + cmp). A copy of n38f-regen.sh's steps with the n41 paths.
# Usage, from the repo (or worktree) root:  bash Tools/night/n41-foundry-regen.sh   (then: n38 gates Design/levels/n41/foundry.plan.json --ride40)
node Tools/night/gen-n41-foundry.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n41/foundry.plan.json | tail -1
node Tools/night/n36-jobs.cjs Design/levels/n41/foundry.plan.json > /dev/null || { echo "  n36-jobs RED: n41-foundry"; exit 1; }
echo "== n41-foundry"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n41/foundry.plan.json Design/levels/n41/foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
cp Design/levels/n41/foundry.json Design/levels/n41-foundry.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n41-foundry.json Assets/StreamingAssets/Design/levels/n41-foundry.json
if cmp -s Design/levels/n41-foundry.json Assets/StreamingAssets/Design/levels/n41-foundry.json; then echo "  copies identical: n41-foundry.json"; else echo "  COPY MISMATCH: n41-foundry.json"; exit 1; fi
