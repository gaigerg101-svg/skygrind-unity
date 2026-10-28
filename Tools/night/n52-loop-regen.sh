#!/usr/bin/env bash
# NIGHT52 the room loop (the lead's; from n47-iceline-regen.sh / n47-spire-regen.sh / n47-span-regen.sh, which have no finish step): regenerate
# a loop level as n52-<level> from its plan Design/levels/n52/<level>.plan.json (copied from n47's with only `id` changed, then the loop's
# rooms edited): the jobs checker, levelgen straight into the copy the game reads (Design/levels/n52-<level>.json), the Player's copy (cp + cmp),
# then the room check for each room named.   bash Tools/night/n52-loop-regen.sh <iceline|spire|span> [room ...]
cd "$(git rev-parse --show-toplevel)" || exit 1
lv="${1:?level: iceline | spire | span}"; shift
plan="Design/levels/n52/$lv.plan.json"; out="Design/levels/n52-$lv.json"
[ -f "$plan" ] || { echo "n52-loop-regen: no $plan"; exit 1; }
node Tools/night/n36-jobs.cjs "$plan" | grep -E "FAIL|RED" | cut -c1-300
node Tools/night/n36-jobs.cjs "$plan" > /dev/null || echo "  n36-jobs RED (read it: node Tools/night/n36-jobs.cjs $plan)"
dotnet run --project Tools/SimHarness -c Release -- levelgen "$plan" "$out" 2>&1 | grep -E "ERROR|error CS|plan checks|level report|FAIL|problem|levelgen" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp "$out" "Assets/StreamingAssets/$out"
if cmp -s "$out" "Assets/StreamingAssets/$out"; then echo "  copies identical: n52-$lv.json"; else echo "  COPY MISMATCH: n52-$lv.json"; exit 1; fi
grant=""; [ "$lv" = span ] || [ "$lv" = spire ] && grant="--grapple"
for r in "$@"; do python -I Tools/night/n52-room-check.py "$plan" "$r" --report "Reports/LEVEL-n52-$lv-validate.md" $grant; done
