#!/usr/bin/env bash
# NIGHT43 builder L1 (foundry43): regenerate level 1 as n43-foundry (design.n43.foundry43 swaps n42-foundry for it): the plan
# (Tools/night/gen-n43-foundry.cjs, from n42-foundry's plan), the jobs checker, levelgen (the report's ERROR rows), K1's arena42 elements (b1, as
# n42-foundry has them), then the copy the game reads (Design/levels/n43-foundry.json) and the Player's copy under Assets/StreamingAssets (cp + cmp).
# n42-foundry-regen.sh's steps with the n43 paths. One known n36-jobs rule is waived and printed: rule 2 ("lava an enemy can never be launched
# into") on THE GANTRY's two lava pits: they are the player's hazard in a link room with no enemies (GG G1, DECIDE43 D-5, DESIGN-RULES A-4).
# Usage, from the repo (or worktree) root:  bash Tools/night/n43-foundry-regen.sh
#   then: dotnet run --project Tools/SimHarness -c Release -- n43level gates | cross | walk | teach
node Tools/night/gen-n43-foundry.cjs || exit 1
JOBS=$(node Tools/night/n36-jobs.cjs Design/levels/n43/foundry.plan.json)
echo "$JOBS" | tail -1
OTHER=$(echo "$JOBS" | grep "  FAIL " | grep -v "FAIL 2 lava [0-9]* (b2): the room has no enemies")
echo "$JOBS" | grep "  FAIL " | grep "(b2): the room has no enemies" | sed 's/^/  waived (the GANTRY: the player'"'"'s hazard): /'
if [ -n "$OTHER" ]; then echo "$OTHER"; echo "  n36-jobs RED: n43-foundry"; exit 1; fi
echo "== n43-foundry"
dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n43/foundry.plan.json Design/levels/n43/foundry.json 2>&1 | grep -E "ERROR|error CS|plan checks|level report|errors:" | cut -c1-600
node Tools/night/n42-foundry-arena.cjs Design/levels/n43/foundry.json || exit 1 # K1's arena42 elements in b1 (read only while arena42 is on; b1 is n42's)
cp Design/levels/n43/foundry.json Design/levels/n43-foundry.json || exit 1
mkdir -p Assets/StreamingAssets/Design/levels
cp Design/levels/n43-foundry.json Assets/StreamingAssets/Design/levels/n43-foundry.json
if cmp -s Design/levels/n43-foundry.json Assets/StreamingAssets/Design/levels/n43-foundry.json; then echo "  copies identical: n43-foundry.json"; else echo "  COPY MISMATCH: n43-foundry.json"; exit 1; fi
