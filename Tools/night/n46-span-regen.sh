#!/usr/bin/env bash
# NIGHT46 builder A1: regenerate level 4 as n46-span (design.n46.span46 swaps n40-span for it), a copy of n40-span-regen.sh: the plan
# (Tools/night/gen-n46-span.cjs, with its reach check), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows),
# then the copy the game reads (Design/levels/n46-span.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Then builder E's gate checker (n38 gates) on the plan.
# Usage, from the worktree root:  bash Tools/night/n46-span-regen.sh
node Tools/night/gen-n46-span.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n46/span.plan.json | grep -E "FAIL|GREEN|RED" | cut -c1-300
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n46/span.plan.json Design/levels/n46/span.json 2>&1 | grep -E "levelgen n46|ERROR|error CS|plan checks|level report|problem" | cut -c1-600
cp Design/levels/n46/span.json Design/levels/n46-span.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n46-span.json Assets/StreamingAssets/Design/levels/n46-span.json
if cmp -s Design/levels/n46-span.json Assets/StreamingAssets/Design/levels/n46-span.json; then echo "  copies identical: n46-span.json"; else echo "  COPY MISMATCH: n46-span.json"; exit 1; fi
dotnet run --project Tools/SimHarness -c Release -- n38 gates Design/levels/n46/span.plan.json 2>&1 | grep -E "^n38 gates n46-span: (GREEN|RED)"
