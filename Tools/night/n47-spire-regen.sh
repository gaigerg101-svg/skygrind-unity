#!/usr/bin/env bash
# NIGHT47 builder L (spire47): regenerate level 5 as n47-spire (design.n47.spire47 swaps n46-spire for it): the plan (Tools/night/gen-n47-spire.cjs, from
# n46-spire's plan: THE CROWN'S WINDOWS added), the jobs checker, levelgen (0 new errors wanted) straight into the copy the game reads
# (Design/levels/n47-spire.json), then the Player's copy under Assets/StreamingAssets (cp + cmp). A copy of n46-spire-regen.sh with the n47-spire paths.
# levelgen also rewrites Reports/LEVEL-n47-spire-validate.md.
# Usage, from the repo (or worktree) root:  bash Tools/night/n47-spire-regen.sh
node Tools/night/gen-n47-spire.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n47/spire.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n47/spire.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n47/spire.plan.json Design/levels/n47-spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n47-spire.json Assets/StreamingAssets/Design/levels/n47-spire.json
if cmp -s Design/levels/n47-spire.json Assets/StreamingAssets/Design/levels/n47-spire.json; then echo "  copies identical: n47-spire.json"; else echo "  COPY MISMATCH: n47-spire.json"; exit 1; fi
