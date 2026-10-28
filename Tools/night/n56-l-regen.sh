#!/usr/bin/env bash
# NIGHT56 builder L: regenerate the n52 levels from their plans the way each was last generated (L-091: the pipeline, not a shortcut):
#   kiln, foundry   levelgen into Design/levels/n52/<level>.json --walk, then gen-n52-<level>.cjs --finish (the name / a9 keys and both copies);
#                   kiln's intermediate file is not tracked and is removed after the finish
#   iceline, spire, span   Tools/night/n52-loop-regen.sh (n36-jobs, levelgen into the copy the game reads, the Player's copy, cmp)
# then the level diff against HEAD (only the plan's edited rows may differ).   bash Tools/night/n56-l-regen.sh [level ...]
cd "$(dirname "$0")/../.." || exit 1
levels=("$@"); [ ${#levels[@]} -gt 0 ] || levels=(kiln foundry iceline spire span)
for lv in "${levels[@]}"; do
  echo "== $lv"
  case "$lv" in
    kiln|foundry)
      dotnet run --project Tools/SimHarness -c Release -- levelgen "Design/levels/n52/$lv.plan.json" "Design/levels/n52/$lv.json" --walk 2>&1 \
        | grep -E "ERROR|error CS|plan checks|level report|FAIL|problem|  ground|  air" | cut -c1-300
      node "Tools/night/gen-n52-$lv.cjs" --finish 2>&1 | tail -2 | cut -c1-300
      [ "$lv" = kiln ] && rm -f Design/levels/n52/kiln.json
      cmp -s "Design/levels/n52-$lv.json" "Assets/StreamingAssets/Design/levels/n52-$lv.json" && echo "  copies identical: n52-$lv.json" || echo "  COPY MISMATCH: n52-$lv.json"
      ;;
    *)
      bash Tools/night/n52-loop-regen.sh "$lv" 2>&1 | grep -v "NOT WRITTEN" | cut -c1-300
      ;;
  esac
  python -I Tools/night/n56-l-leveldiff.py "$lv" | head -1 | cut -c1-600
done
