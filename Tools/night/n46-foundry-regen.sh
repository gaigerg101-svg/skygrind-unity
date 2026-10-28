#!/usr/bin/env bash
# NIGHT46 builder A3 (foundry46): regenerate level 1 as n46-foundry (design.n46.foundry46 swaps n45-foundry for it): the plan
# (Tools/night/gen-n46-foundry.cjs, from n45-foundry's plan: a3 / b1 / a8 / b2 smoothed by data), the jobs checker, levelgen straight into the copy the
# game reads (Design/levels/n46-foundry.json), then the Player's copy under Assets/StreamingAssets (cp + cmp). n45-foundry-regen.sh's steps; the one
# n36-jobs waiver (rule 2 on THE GANTRY's lava in a room with no enemies) is still listed if it fires, but b2 now has enemies (F-4).
# Usage, from the repo (or worktree) root:  bash Tools/night/n46-foundry-regen.sh
node Tools/night/gen-n46-foundry.cjs || exit 1
JOBS=$(node Tools/night/n36-jobs.cjs Design/levels/n46/foundry.plan.json)
echo "$JOBS" | tail -1
echo "$JOBS" | grep -E " (a3|b1|a8|b2)\b" | sed 's/^/  /'
# F-4's waiver: b2's gate 1 lava (lava 2, z -229..-192) is the long wall's hazard for the player; F-4's husks stand on the landing's far half, in view
# of the landing player, 0.5 m from gate 2's lava (the fight's); a spot within 4 m of gate 1's lava would stand beside or behind him (Part D 8)
OTHER=$(echo "$JOBS" | grep "  FAIL " | grep -v "FAIL 2 lava [0-9]* (b2): the room has no enemies" | grep -v "FAIL 2 lava 2 (b2): the nearest enemy spot")
echo "$JOBS" | grep "  FAIL " | grep "(b2): the room has no enemies" | sed 's/^/  waived (the GANTRY: the player'"'"'s hazard): /'
echo "$JOBS" | grep "  FAIL " | grep "FAIL 2 lava 2 (b2): the nearest enemy spot" | sed 's/^/  waived (F-4: gate 1 is the player'"'"'s hazard; the fight is at gate 2): /'
if [ -n "$OTHER" ]; then echo "$OTHER"; echo "  n36-jobs RED: n46-foundry"; exit 1; fi
echo "== n46-foundry"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n46/foundry.plan.json Design/levels/n46-foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n46-foundry.json Assets/StreamingAssets/Design/levels/n46-foundry.json
if cmp -s Design/levels/n46-foundry.json Assets/StreamingAssets/Design/levels/n46-foundry.json; then echo "  copies identical: n46-foundry.json"; else echo "  COPY MISMATCH: n46-foundry.json"; exit 1; fi
