#!/usr/bin/env bash
# NIGHT38 builder K3: regenerate level 3 (design.n38.kiln38 swaps n35-kiln for n38-kiln): the plan (Tools/night/gen-n38-kiln.cjs), the jobs checker
# (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game reads (Design/levels/n38-kiln.json) and the Player's copy
# under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n38k-regen.sh
node Tools/night/gen-n38-kiln.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n38/kiln.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== kiln"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n38/kiln.plan.json Design/levels/n38/kiln.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    " | cut -c1-400 | head -40
cp Design/levels/n38/kiln.json Design/levels/n38-kiln.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n38-kiln.json Assets/StreamingAssets/Design/levels/n38-kiln.json
if cmp -s Design/levels/n38-kiln.json Assets/StreamingAssets/Design/levels/n38-kiln.json; then echo "  copies identical: n38-kiln.json"; else echo "  COPY MISMATCH: n38-kiln.json"; exit 1; fi
