#!/usr/bin/env bash
# NIGHT40 builder L2: regenerate n40-iceline (design.n40.iceline40 swaps n37-iceline for it): the plan (Tools/night/gen-n40-iceline.cjs), the jobs
# checker (Tools/night/n36-jobs.cjs: GREEN wanted), levelgen (0 errors wanted), the copy the game reads (Design/levels/n40-iceline.json) and the
# Player's copy under Assets/StreamingAssets (cp + cmp). The walk: dotnet run --project Tools/SimHarness -c Release -- n40ice walk --seeds 10
# Usage, from the repo (or worktree) root:  bash Tools/night/n40l2-regen.sh
node Tools/night/gen-n40-iceline.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n40/iceline.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n40/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n40/iceline.plan.json Design/levels/n40/iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL" | cut -c1-600
cp Design/levels/n40/iceline.json Design/levels/n40-iceline.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n40-iceline.json Assets/StreamingAssets/Design/levels/n40-iceline.json
if cmp -s Design/levels/n40-iceline.json Assets/StreamingAssets/Design/levels/n40-iceline.json; then echo "  copies identical: n40-iceline.json"; else echo "  COPY MISMATCH: n40-iceline.json"; exit 1; fi
