#!/usr/bin/env bash
# NIGHT37 builder W: regenerate n37-foundry (design.n37.wick37 swaps n36-foundry for it): the plan (gen-n36-foundry.cjs --n37), the jobs checker,
# levelgen (its ERROR rows), then the copy the game reads (Design/levels/n37-foundry.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n37w-regen.sh
node Tools/night/gen-n36-foundry.cjs --n37 || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n36/n37-foundry.plan.json > /dev/null || { echo "  n36-jobs RED: n37-foundry"; exit 1; }
echo "  n36-jobs GREEN"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n36/n37-foundry.plan.json Design/levels/n36/n37-foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report" | cut -c1-600
cp Design/levels/n36/n37-foundry.json Design/levels/n37-foundry.json || exit 1
cp Design/levels/n37-foundry.json Assets/StreamingAssets/Design/levels/n37-foundry.json
if cmp -s Design/levels/n37-foundry.json Assets/StreamingAssets/Design/levels/n37-foundry.json; then echo "  copies identical: n37-foundry.json"; else echo "  COPY MISMATCH"; exit 1; fi
