#!/usr/bin/env bash
# NIGHT38 builder C2: regenerate level 5 rebuilt for flight (design.n38.spire38 swaps n37-spire for n38-spire): the plan
# (Tools/night/gen-n38-spire.cjs), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game
# reads (Design/levels/n38-spire.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n38c-regen.sh
node Tools/night/gen-n38-spire.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n38/spire.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== n38-spire"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n38/spire.plan.json Design/levels/n38/spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    " | cut -c1-400 | head -60
cp Design/levels/n38/spire.json Design/levels/n38-spire.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n38-spire.json Assets/StreamingAssets/Design/levels/n38-spire.json
if cmp -s Design/levels/n38-spire.json Assets/StreamingAssets/Design/levels/n38-spire.json; then echo "  copies identical: n38-spire.json"; else echo "  COPY MISMATCH: n38-spire.json"; exit 1; fi
