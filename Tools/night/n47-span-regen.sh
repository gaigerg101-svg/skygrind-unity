#!/usr/bin/env bash
# NIGHT47 builder L: regenerate level 4 as n47-span (design.n47.span47 swaps n46-span for it), a copy of n46-span-regen.sh: the plan
# (Tools/night/gen-n47-span.cjs: o3 rebuilt as THE LOOM FRAME, with its reach check), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the
# report's ERROR rows), then the copy the game reads (Design/levels/n47-span.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the worktree root:  bash Tools/night/n47-span-regen.sh
node Tools/night/gen-n47-span.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n47/span.plan.json | grep -E "FAIL|GREEN|RED" | cut -c1-300
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n47/span.plan.json Design/levels/n47/span.json 2>&1 | grep -E "levelgen n47|ERROR|error CS|plan checks|level report|problem" | cut -c1-600
cp Design/levels/n47/span.json Design/levels/n47-span.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n47-span.json Assets/StreamingAssets/Design/levels/n47-span.json
if cmp -s Design/levels/n47-span.json Assets/StreamingAssets/Design/levels/n47-span.json; then echo "  copies identical: n47-span.json"; else echo "  COPY MISMATCH: n47-span.json"; exit 1; fi
