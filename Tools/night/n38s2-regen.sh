#!/usr/bin/env bash
# NIGHT38 builder S2: regenerate n38-iceline (design.n38.iceline38 swaps n37-iceline for it): the plan (Tools/night/gen-n37-iceline-boss.cjs --n38), the jobs
# checker (Tools/night/n36-jobs.cjs: GREEN wanted; every room is new), levelgen (0 errors wanted), the copy the game reads (Design/levels/n38-iceline.json)
# and the Player's copy under Assets/StreamingAssets (cp + cmp). The gates are separate: `n38 gates Design/levels/n38/iceline.plan.json` (builder E's checker)
# and `iceline38 skate` (the skate gap).
# Usage, from the repo (or worktree) root:  bash Tools/night/n38s2-regen.sh
node Tools/night/gen-n37-iceline-boss.cjs --n38 || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n38/iceline.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n38/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n38/iceline.plan.json Design/levels/n38/iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report" | cut -c1-600
cp Design/levels/n38/iceline.json Design/levels/n38-iceline.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n38-iceline.json Assets/StreamingAssets/Design/levels/n38-iceline.json
if cmp -s Design/levels/n38-iceline.json Assets/StreamingAssets/Design/levels/n38-iceline.json; then echo "  copies identical: n38-iceline.json"; else echo "  COPY MISMATCH: n38-iceline.json"; exit 1; fi
