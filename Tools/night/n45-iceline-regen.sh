#!/usr/bin/env bash
# NIGHT45 builder A2 (iceline45): regenerate level 2 as n45-iceline (design.n45.iceline45 swaps n43-iceline for it): the plan
# (Tools/night/gen-n45-iceline.cjs, from n43-iceline's plan: g7 THE MIX rebuilt as THE THAW LOCK), the jobs checker, levelgen (0 errors wanted) straight
# into the copy the game reads (Design/levels/n45-iceline.json: no intermediate under Design/levels/n45/, the audit's fluff B4), then the Player's copy
# under Assets/StreamingAssets (cp + cmp). levelgen also rewrites Reports/LEVEL-n45-iceline-validate.md (the level's report).
# Usage, from the repo (or worktree) root:  bash Tools/night/n45-iceline-regen.sh
#   then: dotnet run --project Tools/SimHarness -c Release -- n45mix all --level n45-iceline
node Tools/night/gen-n45-iceline.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n45/iceline.plan.json | grep -E "FAIL|GREEN|RED"
if ! node Tools/night/n36-jobs.cjs Design/levels/n45/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n45/iceline.plan.json Design/levels/n45-iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n45-iceline.json Assets/StreamingAssets/Design/levels/n45-iceline.json
if cmp -s Design/levels/n45-iceline.json Assets/StreamingAssets/Design/levels/n45-iceline.json; then echo "  copies identical: n45-iceline.json"; else echo "  COPY MISMATCH: n45-iceline.json"; exit 1; fi
