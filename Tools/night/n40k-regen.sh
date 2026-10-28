#!/usr/bin/env bash
# NIGHT40 builder S1 (kiln40): regenerate level 3 as n40-kiln (design.n40.kiln40 swaps n38-kiln for it): the plan (Tools/night/gen-n40-kiln.cjs),
# the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game reads (Design/levels/n40-kiln.json) and
# the Player's copy under Assets/StreamingAssets (cp + cmp). n38-kiln's files are never written here (--off40 loads them).
# Usage, from the repo (or worktree) root:  bash Tools/night/n40k-regen.sh
node Tools/night/gen-n40-kiln.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n40/kiln.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== n40-kiln"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n40/kiln.plan.json Design/levels/n40/kiln.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    " | cut -c1-400 | head -40
cp Design/levels/n40/kiln.json Design/levels/n40-kiln.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n40-kiln.json Assets/StreamingAssets/Design/levels/n40-kiln.json
if cmp -s Design/levels/n40-kiln.json Assets/StreamingAssets/Design/levels/n40-kiln.json; then echo "  copies identical: n40-kiln.json"; else echo "  COPY MISMATCH: n40-kiln.json"; exit 1; fi
