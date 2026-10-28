#!/usr/bin/env bash
# NIGHT41 builder L6: regenerate n41-finale (the level 6 greybox; design.n41.finale41 OFF): the plan (Tools/night/gen-n41-finale.cjs), the jobs checker
# (Tools/night/n36-jobs.cjs: GREEN wanted), levelgen (0 errors wanted), the copy the game reads (Design/levels/n41-finale.json) and the Player's copy under
# Assets/StreamingAssets (cp + cmp). The walk: dotnet run --project Tools/SimHarness -c Release -- n41fin walk --seeds 10
# Usage, from the repo (or worktree) root:  bash Tools/night/n41-finale-regen.sh
node Tools/night/gen-n41-finale.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n41/finale.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n41/finale.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n41/finale.plan.json Design/levels/n41/finale.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL" | cut -c1-600
cp Design/levels/n41/finale.json Design/levels/n41-finale.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n41-finale.json Assets/StreamingAssets/Design/levels/n41-finale.json
if cmp -s Design/levels/n41-finale.json Assets/StreamingAssets/Design/levels/n41-finale.json; then echo "  copies identical: n41-finale.json"; else echo "  COPY MISMATCH: n41-finale.json"; exit 1; fi
