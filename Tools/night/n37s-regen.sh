#!/usr/bin/env bash
# NIGHT37 builder S: regenerate n37-iceline (design.n37.stag37 swaps n35-iceline for it): the plan (Tools/night/gen-n37-iceline-boss.cjs), the jobs
# checker (Tools/night/n36-jobs.cjs: rule 1 (every solid a job + why) must pass; rule 2's lava-reach failures in the COPIED n35 rooms i1r / i4 / i6 are
# n35-iceline's own and are printed, not fixed: the rooms are copied as they are), levelgen (0 errors wanted), then the copy the game reads
# (Design/levels/n37-iceline.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n37s-regen.sh
node Tools/night/gen-n37-iceline-boss.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n37/iceline.plan.json | grep -E "FAIL|GREEN|RED"
if node Tools/night/n36-jobs.cjs Design/levels/n37/iceline.plan.json | grep -E "FAIL (1|3) |FAIL 2 lava [0-9]+ \((s[0-9])" ; then echo "  n36-jobs RED in a new room or rule 1/3"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n37/iceline.plan.json Design/levels/n37/iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report" | cut -c1-600
cp Design/levels/n37/iceline.json Design/levels/n37-iceline.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n37-iceline.json Assets/StreamingAssets/Design/levels/n37-iceline.json
if cmp -s Design/levels/n37-iceline.json Assets/StreamingAssets/Design/levels/n37-iceline.json; then echo "  copies identical: n37-iceline.json"; else echo "  COPY MISMATCH: n37-iceline.json"; exit 1; fi
