#!/usr/bin/env bash
# NIGHT43 builder E1 (iceline43): regenerate level 2 as n43-iceline (design.n43.iceline43 swaps n40-iceline for it): the plan
# (Tools/night/gen-n43-iceline.cjs, from n40-iceline's plan), the jobs checker, levelgen (0 errors wanted), then the copy the game reads
# (Design/levels/n43-iceline.json) and the Player's copy under Assets/StreamingAssets (cp + cmp). n40l2-regen.sh's steps with the n43 paths.
# levelgen also rewrites Reports/LEVEL-n43-iceline-validate.md (the level's report).
# Usage, from the repo (or worktree) root:  bash Tools/night/n43-iceline-regen.sh
#   then: dotnet run --project Tools/SimHarness -c Release -- n43rooms iceline ; n43rooms clear --n43 --levels iceline
node Tools/night/gen-n43-iceline.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n43/iceline.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n43/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n43/iceline.plan.json Design/levels/n43/iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL" | cut -c1-600
cp Design/levels/n43/iceline.json Design/levels/n43-iceline.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n43-iceline.json Assets/StreamingAssets/Design/levels/n43-iceline.json
if cmp -s Design/levels/n43-iceline.json Assets/StreamingAssets/Design/levels/n43-iceline.json; then echo "  copies identical: n43-iceline.json"; else echo "  COPY MISMATCH: n43-iceline.json"; exit 1; fi
