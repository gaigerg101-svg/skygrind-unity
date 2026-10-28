#!/usr/bin/env bash
# NIGHT38 builder O2: regenerate level 4 as n38-span (design.n38.span38 swaps n37-span for it): the plan (Tools/night/gen-n38-span.cjs, with its
# grapple-reach check), the jobs checker (Tools/night/n36-jobs.cjs), levelgen (the report's ERROR rows), then the copy the game reads
# (Design/levels/n38-span.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Then builder E's gate checker (n38 gates) on the plan.
# Usage, from the repo (or worktree) root:  bash Tools/night/n38o-regen.sh
node Tools/night/gen-n38-span.cjs || exit 1
node Tools/night/n36-jobs.cjs Design/levels/n38/span.plan.json | grep -E "FAIL|GREEN|RED" | cut -c1-300
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n38/span.plan.json Design/levels/n38/span.json 2>&1 | grep -E "levelgen n38|ERROR|error CS|plan checks|level report|problem" | cut -c1-600
cp Design/levels/n38/span.json Design/levels/n38-span.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n38-span.json Assets/StreamingAssets/Design/levels/n38-span.json
if cmp -s Design/levels/n38-span.json Assets/StreamingAssets/Design/levels/n38-span.json; then echo "  copies identical: n38-span.json"; else echo "  COPY MISMATCH: n38-span.json"; exit 1; fi
# NIGHT38 E's gate checker on the plan (every gate PASS before span38 is on), with and without ride38
dotnet run --project Tools/SimHarness -c Release -- n38 gates Design/levels/n38/span.plan.json 2>&1 | grep -E "^n38 gates n38-span: (GREEN|RED)"
dotnet run --project Tools/SimHarness -c Release -- n38 gates Design/levels/n38/span.plan.json --ride38 2>&1 | grep -E "^n38 gates n38-span: (GREEN|RED)" | sed 's/$/ (ride38)/'
