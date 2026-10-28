#!/usr/bin/env bash
# NIGHT37 builder C: regenerate level 5 with THE CROWN's descent (design.n37.crown37 swaps n33-spire for n37-spire): the plan
# (Tools/night/gen-n37-spire-boss.cjs), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game
# reads (Design/levels/n37-spire.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n37c-regen.sh
node Tools/night/gen-n37-spire-boss.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n37/spire.plan.json | grep -E "FAIL|GREEN|RED" | head -20
echo "== spire"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n37/spire.plan.json Design/levels/n37/spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|problem|    " | cut -c1-400 | head -40
cp Design/levels/n37/spire.json Design/levels/n37-spire.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n37-spire.json Assets/StreamingAssets/Design/levels/n37-spire.json
if cmp -s Design/levels/n37-spire.json Assets/StreamingAssets/Design/levels/n37-spire.json; then echo "  copies identical: n37-spire.json"; else echo "  COPY MISMATCH: n37-spire.json"; exit 1; fi
