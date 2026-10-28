#!/usr/bin/env bash
# NIGHT42 builder L1 (foundry42): regenerate level 1 as n42-foundry (design.n42.foundry42 swaps n41-foundry for it): the plan
# (Tools/night/gen-n42-foundry.cjs, from n41-foundry's plan), the jobs checker, levelgen (the report's ERROR rows), then the copy the game reads
# (Design/levels/n42-foundry.json) and the Player's copy under Assets/StreamingAssets (cp + cmp). n41-foundry-regen.sh's steps with the n42 paths.
# Usage, from the repo (or worktree) root:  bash Tools/night/n42-foundry-regen.sh
#   then: n38 gates Design/levels/n42/foundry.plan.json --ride40 ; n42level sweep Design/levels/n42/foundry.plan.json ; n42level walk
node Tools/night/gen-n42-foundry.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n42/foundry.plan.json | tail -1
node Tools/night/n36-jobs.cjs Design/levels/n42/foundry.plan.json > /dev/null || { echo "  n36-jobs RED: n42-foundry"; exit 1; }
echo "== n42-foundry"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n42/foundry.plan.json Design/levels/n42/foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
node Tools/night/n42-foundry-arena.cjs Design/levels/n42/foundry.json || exit 1 # K1's arena42 elements in b1 (read only while arena42 is on)
cp Design/levels/n42/foundry.json Design/levels/n42-foundry.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n42-foundry.json Assets/StreamingAssets/Design/levels/n42-foundry.json
if cmp -s Design/levels/n42-foundry.json Assets/StreamingAssets/Design/levels/n42-foundry.json; then echo "  copies identical: n42-foundry.json"; else echo "  COPY MISMATCH: n42-foundry.json"; exit 1; fi
