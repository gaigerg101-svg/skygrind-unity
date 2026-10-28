#!/usr/bin/env bash
# NIGHT46 builder A3 (iceline46): regenerate level 2 as n46-iceline (design.n46.iceline46 swaps n45-iceline for it): the plan
# (Tools/night/gen-n46-iceline.cjs, from n45-iceline's plan: g4's second bridge, g8's charger in view, g3 rebuilt as THE TIPPLE), the jobs checker,
# levelgen (0 errors wanted) straight into the copy the game reads (Design/levels/n46-iceline.json), then the Player's copy under Assets/StreamingAssets
# (cp + cmp). n45-iceline-regen.sh's steps.
# Usage, from the repo (or worktree) root:  bash Tools/night/n46-iceline-regen.sh
node Tools/night/gen-n46-iceline.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n46/iceline.plan.json | grep -E "FAIL|GREEN|RED| g3| g4| g8"
if ! node Tools/night/n36-jobs.cjs Design/levels/n46/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n46/iceline.plan.json Design/levels/n46-iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n46-iceline.json Assets/StreamingAssets/Design/levels/n46-iceline.json
if cmp -s Design/levels/n46-iceline.json Assets/StreamingAssets/Design/levels/n46-iceline.json; then echo "  copies identical: n46-iceline.json"; else echo "  COPY MISMATCH: n46-iceline.json"; exit 1; fi
