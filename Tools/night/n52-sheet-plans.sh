#!/usr/bin/env bash
# NIGHT52 (the lead's): the before / after sheet's plan images for every rebuilt room (Tools/night/n52-svg-png.sh, headless Edge), the
# "before" from the level's old plan (n47 / n46 foundry), the "after" from Design/levels/n52/<level>.plan.json. Writes
# Logs/n52/sheet/plan-<level>-<room>-{before,after}.png, then Tools/night/n52-sheet.py <tag> takes them with the level36 entry frames.
#   bash Tools/night/n52-sheet-plans.sh <level>:<room>,<room> ...      e.g. bash Tools/night/n52-sheet-plans.sh kiln:kb2,kb2c,kb3 iceline:g3
cd "$(git rev-parse --show-toplevel)" || exit 1
declare -A OLD=([kiln]=Design/levels/n47/kiln.plan.json [foundry]=Design/levels/n46/foundry.plan.json [iceline]=Design/levels/n47/iceline.plan.json
                [spire]=Design/levels/n47/spire.plan.json [span]=Design/levels/n47/span.plan.json)
mkdir -p Logs/n52/sheet
for arg in "$@"; do
  lv="${arg%%:*}"; rooms="${arg#*:}"
  [ -n "${OLD[$lv]}" ] || { echo "n52-sheet-plans: unknown level $lv"; continue; }
  for r in ${rooms//,/ }; do
    bash Tools/night/n52-svg-png.sh "${OLD[$lv]}" "$r" "Logs/n52/sheet/plan-$lv-$r-before.png" > /dev/null 2>&1
    bash Tools/night/n52-svg-png.sh "Design/levels/n52/$lv.plan.json" "$r" "Logs/n52/sheet/plan-$lv-$r-after.png" > /dev/null 2>&1
    for s in before after; do f="Logs/n52/sheet/plan-$lv-$r-$s.png"; [ -s "$f" ] && echo "ok $f" || echo "MISSING $f"; done
  done
done
