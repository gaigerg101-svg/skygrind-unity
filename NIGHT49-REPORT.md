# NIGHT49 REPORT (for GG)

**Outcome first.** The charge bar is NOT back. In three blind pairs, the old arc40 (the one you removed on 2026-10-18) beat every new
candidate three times out of three, so the element is not shipped and the call is yours. The environment grade (the colour and the
arenas' look) is NOT shipped either: it passed on two of five levels by the meter and won or tied 2 of 5 in the blind sheet. What did
ship is the level lights: the landmark light is the room's strongest light on all five levels, and the cyan and the pale white now sit
only on usable surfaces and the boundary. The impact kit (one set of effects) won its blind pair 3-0 but still fails the screen-wash
rule on the enemy kill, so it is off. Build n49-freeze (a9b1442d) passes verbs37 (26 / 20 / 25 / 30 / 30), the smoke, and golden.

## What to look at first
1. **The charge element (G1, SHORT).** Your call: arc40 back as the element, or a new design. Frames: Logs/n49/sheets/h49-pairs-all.png
   (arc40 beside each candidate), Logs/n49/sheets/r2-h-c3-centre.png (candidate 3's states).
2. **The level lights (G2, GREEN on the tie).** Play the five arenas on Builds/n49-freeze: the landmark should read as the room's light,
   and the cyan should mean "usable". The blind sheet tied, so the change is subtle; tell me if it reads as nothing.
3. **The grade (G4, SHORT).** Frames: Logs/feel/n49-r3/v-<level>/a349/shot-N49-V-<level>-combat-{off,on}.png; the sheet
   Logs/n49/judge/v49-sheet/sheet.png. The new grade won kiln and spire; the old look won foundry, iceline and span. The palette tokens
   are in UnityTuning.Look49.cs and the per-level values in UnityTuning.V49.cs; look49 is OFF.
4. **The effects kit (G5, SHORT, fx49 OFF).** Its pair won 3-0 for the kit. Five of six effects now pass the wash rule; the enemy kill
   does not (+0.34, the same as before), because its wash comes from the web death burst in SurvivalGame.Init.cs (an enemy colour, not
   touched). Frames: Logs/feel/n49-r3/x1080/a249/.

## Ships by default (Builds/n49-freeze; commit a9b1442d)
- ON: lights49 (the new level lights); the NIGHT48 and NIGHT47 rows climbTop48 (the WICK's climb, level 1 is now n48-foundry), foes48
  (the warden plate and the swarmer; the drone part stays OFF), loomHull48 (THE LOOM's hull), wickRideParry47, wickIntro47, wickFinale47.
- OFF: charge49, look49, fx49, drone49, blast48, guns48, wyrmTell48, wyrmDive48, kilnBoost48, finale47, stampede47, flame48, loomReach48.
- To switch off tonight's new row: `-skygrindOff49` (the Player) or `--off49` (the harness) gives NIGHT48's game.
  `-skygrindOff48` gives NIGHT47's game.

## Numbers (from tonight's runs)
- verbs37 on the freeze build: 26 / 20 / 25 / 30 / 30, RED rows none, NRE 0 on all five.
- Smoke on the freeze build: "DONE: all steps passed" (237.5 s), NRE 0.
- Batch EditMode suite on a9b1442d: 2208 run, 2206 passed, 2 failed. Both failures were test fixes (4c5b99bd: an off-switch leak in my
  N49 test, and an N47 assertion that P0's climbTop48 had changed); the shared tests re-run 105 / 105. The full suite was not re-run.
- Flow table (commit 4c5b99bd, n47-flow-table.sh): walks 20/20 on all five levels. Worse past the band: L2 clear time (184.8 -> 220.4 s)
  and L3 stall share (31.7 -> 40.3 %); both were worse at NIGHT47 too. Level 1 in this table is still n47-foundry: the n48-foundry that
  climbTop48 ships is NOT measured (not checked).
- Worm band (n47human, 40 seeds): with the worm rows off, median 122.6 s, p90 256.9 s; forced on, median 148.7 s, p90 220.2 s. The p90
  is over the 180 s limit either way, so the worm rows stay OFF.
- Golden 22 / 22 exact, 0 mismatches; fingerprint plain, --off47, --off48 and --off49 EQUAL on every commit.
- Blind judges: 18 valid votes across five pairs and sheets (black frames from my wrong crop window were discarded, and I fixed the key
  line after the first pair). The per-row keys are in Logs/n49/judge-keys/.

## What was not verified
- The look49 grade on the playable build is not shipped, so its frames were only judged in the reel sheets.
- The n48-foundry flow (climbTop48's level) is not in the flow table.
- The full EditMode suite was not re-run after the two test fixes.
- The worm's tail (p90) was not fixed; the worm rows are OFF.
- The kill's wash: the web death burst is not in this repo's X scope (SurvivalGame.Init.cs), so the fix is open.

## Forge experiment (the one change this forge made)
Every look row's acceptance was a frame-meter number with a baseline AND a 3-judge blind pair, set before the run. Result: 5 of 6 look
rows (G1-G5) closed with both a meter and a pair; G6 was not built. The method also caught a mistake: the first pair's key line was inverted
and the first pair's crop was black. Both are fixed. Next forge: the pair protocol checks each image's content before the judges see it.

Logs and frames: Logs/feel/n49-r1, n49-r2, n49-r3, n49-freeze; sheets Logs/n49/sheets/; judge keys Logs/n49/judge-keys/. Logs/ is ignored.
