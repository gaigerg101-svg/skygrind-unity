#!/usr/bin/env bash
# NIGHT47 builder B (iceline47): regenerate level 2 as n47-iceline (design.n47.iceline47 swaps n46-iceline for it): the plan
# (Tools/night/gen-n47-iceline.cjs, from n46-iceline's plan: the charger route and THE STAMPEDE), the jobs checker, levelgen (0 errors wanted)
# straight into the copy the game reads (Design/levels/n47-iceline.json), then the Player's copy under Assets/StreamingAssets (cp + cmp).
# n46-iceline-regen.sh's steps. Usage, from the worktree root:  bash Tools/night/n47-iceline-regen.sh
node Tools/night/gen-n47-iceline.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n47/iceline.plan.json | grep -E "FAIL|GREEN|RED| g1| g2| g5| g8"
if ! node Tools/night/n36-jobs.cjs Design/levels/n47/iceline.plan.json > /dev/null; then echo "  n36-jobs RED"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n47/iceline.plan.json Design/levels/n47-iceline.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n47-iceline.json Assets/StreamingAssets/Design/levels/n47-iceline.json
if cmp -s Design/levels/n47-iceline.json Assets/StreamingAssets/Design/levels/n47-iceline.json; then echo "  copies identical: n47-iceline.json"; else echo "  COPY MISMATCH: n47-iceline.json"; exit 1; fi
