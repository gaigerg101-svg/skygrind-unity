#!/usr/bin/env bash
# NIGHT32 (builder F): regenerate LEVEL 5 THE SPIRE: the plan (gen-n32-spire.cjs), levelgen (the report prints ERROR rows only), and the copy the game reads
# (Design/levels/n32-spire.json) plus its Player copy under Assets/StreamingAssets (this worktree's; n24-sync.sh copies the MAIN repo's files). Usage, from the repo root:
#   bash Tools/night/n32-spire-regen.sh
node Tools/night/gen-n32-spire.cjs || exit 1
echo "== spire"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n32/spire.plan.json Design/levels/n32/spire.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|^    [a-z]" | cut -c1-600
cp Design/levels/n32/spire.json Design/levels/n32-spire.json
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n32-spire.json Assets/StreamingAssets/Design/levels/n32-spire.json
cmp -s Design/levels/n32-spire.json Assets/StreamingAssets/Design/levels/n32-spire.json && echo "spire copies identical"
