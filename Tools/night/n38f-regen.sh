#!/usr/bin/env bash
# NIGHT38 builder F: regenerate n38-foundry (design.n38.foundry38 swaps n37-foundry for it) and / or n38-finale (design.n38.finale38 swaps n35-finale):
# the plan (gen-n36-foundry.cjs --n38 / gen-n38-finale.cjs), the jobs checker (n36-jobs.cjs), levelgen (its ERROR rows and the report), then the copy the
# game reads (Design/levels/n38-<name>.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# Usage, from the repo (or worktree) root:  bash Tools/night/n38f-regen.sh [foundry] [finale]   (no argument: both)
names="$*"; [ -z "$names" ] && names="foundry finale"
for g in $names; do
  case "$g" in
    foundry) node Tools/night/gen-n36-foundry.cjs --n38 || exit 1 ;;
    finale) node Tools/night/gen-n38-finale.cjs || exit 1 ;;
    *) echo "unknown level $g"; exit 1 ;;
  esac
  plan="Design/levels/n38/n38-$g.plan.json"
  node Tools/night/n36-jobs.cjs "$plan" | tail -1
  node Tools/night/n36-jobs.cjs "$plan" > /dev/null || { echo "  n36-jobs RED: n38-$g"; exit 1; }
  echo "== n38-$g"
  dotnet run --project Tools/SimHarness -c Release -- levelgen "$plan" "Design/levels/n38/n38-$g.json" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
  cp "Design/levels/n38/n38-$g.json" "Design/levels/n38-$g.json" || exit 1
  mkdir -p Assets/StreamingAssets/Design/levels
  cp "Design/levels/n38-$g.json" "Assets/StreamingAssets/Design/levels/n38-$g.json"
  if cmp -s "Design/levels/n38-$g.json" "Assets/StreamingAssets/Design/levels/n38-$g.json"; then echo "  copies identical: n38-$g.json"; else echo "  COPY MISMATCH: n38-$g.json"; exit 1; fi
done
