#!/usr/bin/env bash
# NIGHT43 builder K1 (kiln43): regenerate level 3 as n43-kiln (design.n43.kiln43 swaps n41-kiln for it): the plan (Tools/night/gen-n43-kiln.cjs),
# the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the finish step (data.name n40-kiln; the copy the game
# reads, Design/levels/n43-kiln.json, and the Player's copy under Assets/StreamingAssets; cmp). n40 / n41-kiln's files are never written here.
# Usage, from the repo (or worktree) root:  bash Tools/night/n43k-regen.sh
#   then: n43kiln gates ; n43kiln walk ; n43kiln steps  (dotnet run --project Tools/SimHarness -c Release -- ...)
node Tools/night/gen-n43-kiln.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n43/kiln.plan.json | grep -E "FAIL|GREEN|RED" | head -20
node Tools/night/n36-jobs.cjs Design/levels/n43/kiln.plan.json > /dev/null || { echo "  n36-jobs RED: n43-kiln"; exit 1; }
echo "== n43-kiln"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n43/kiln.plan.json Design/levels/n43/kiln.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:|problem|^    " | cut -c1-500 | head -40
node Tools/night/gen-n43-kiln.cjs --finish Design/levels/n43/kiln.json || exit 1
if cmp -s Design/levels/n43-kiln.json Assets/StreamingAssets/Design/levels/n43-kiln.json; then echo "  copies identical: n43-kiln.json"; else echo "  COPY MISMATCH: n43-kiln.json"; exit 1; fi
