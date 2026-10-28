#!/usr/bin/env bash
# NIGHT45 builder A1 (foundry45): regenerate level 1 as n45-foundry (design.n45.foundry45 swaps n43-foundry for it): the plan
# (Tools/night/gen-n45-foundry.cjs, from n43-foundry's plan, b1 rebuilt as THE SPILL), the jobs checker, levelgen straight into the copy the game
# reads (Design/levels/n45-foundry.json: no tracked intermediate, AUDIT B4), then the Player's copy under Assets/StreamingAssets (cp + cmp).
# n43-foundry-regen.sh's steps with two changes: arena42's elements are NOT written into b1 (DECIDE45 D-6: n42-foundry-arena.cjs is not run), and
# the same one known n36-jobs waiver (rule 2 on THE GANTRY's two lava pits: the player's hazard in a link room with no enemies, GG G1).
# Usage, from the repo (or worktree) root:  bash Tools/night/n45-foundry-regen.sh
#   then: dotnet run --project Tools/SimHarness -c Release -- n45yard gates | walk
node Tools/night/gen-n45-foundry.cjs || exit 1
JOBS=$(node Tools/night/n36-jobs.cjs Design/levels/n45/foundry.plan.json)
echo "$JOBS" | tail -1
echo "$JOBS" | grep "b1" | sed 's/^/  /'
OTHER=$(echo "$JOBS" | grep "  FAIL " | grep -v "FAIL 2 lava [0-9]* (b2): the room has no enemies")
echo "$JOBS" | grep "  FAIL " | grep "(b2): the room has no enemies" | sed 's/^/  waived (the GANTRY: the player'"'"'s hazard): /'
if [ -n "$OTHER" ]; then echo "$OTHER"; echo "  n36-jobs RED: n45-foundry"; exit 1; fi
echo "== n45-foundry"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n45/foundry.plan.json Design/levels/n45-foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n45-foundry.json Assets/StreamingAssets/Design/levels/n45-foundry.json
if cmp -s Design/levels/n45-foundry.json Assets/StreamingAssets/Design/levels/n45-foundry.json; then echo "  copies identical: n45-foundry.json"; else echo "  COPY MISMATCH: n45-foundry.json"; exit 1; fi
