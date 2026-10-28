#!/usr/bin/env bash
# NIGHT37 builder O: regenerate level 4 as n37-span (design.n37.loom37 swaps n33-span for it): the plan (Tools/night/gen-n37-span-boss.cjs, with
# its grapple-reach check), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game reads
# (Design/levels/n37-span.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n37o-regen.sh
node Tools/night/gen-n37-span-boss.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n37/span.plan.json | grep -E "FAIL|GREEN|RED" | cut -c1-300
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n37/span.plan.json Design/levels/n37/span.json 2>&1 | grep -E "levelgen n37|ERROR|error CS|plan checks|level report|problem" | cut -c1-600
cp Design/levels/n37/span.json Design/levels/n37-span.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n37-span.json Assets/StreamingAssets/Design/levels/n37-span.json
if cmp -s Design/levels/n37-span.json Assets/StreamingAssets/Design/levels/n37-span.json; then echo "  copies identical: n37-span.json"; else echo "  COPY MISMATCH: n37-span.json"; exit 1; fi
