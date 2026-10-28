# NIGHT56 gate r5, part B (GAMEPLAY, ENEMIES, COLOUR, BOSSES): the evidence for the batched critic (the lead's, 05:1x)

The final gate of the night, split in two (NIGHT56.md section 6); part A judges the levels. Build `Builds/n56-r5` from port/stage3 b6ab1f10
(every builder commit of the night). Its copy's config has ON: design.n55 grade55, wick55, step55, decor55, kill55, brief55, clear55,
wyrm55; design.n56 rank56, colour56, rim56, yellow56, tokens56. OFF sides: `-skygrindOff55` (NIGHT54's game) / `-skygrindOff56` (NIGHT55's).
All paths are under `C:\Users\gaige\dev\SKYGRIND-unity\`. Reel lines: `Logs/feel/n56-r5/summary47.txt`; full lines
`Logs/player/n47-reel-n56-r5-<side>.log`; earlier builds `Logs/feel/n56-r1` .. `n56-r4`. The r2 verdicts: `Research/2026-10-20-wave56/gate-r2.md`
(judge fresh; say where r5 changed a verdict). Rubric and output: `Research/2026-10-20-wave56/critic-gate.md`. Write
`Research/2026-10-20-wave56/gate-r5b.md`. At most 30 images (15 listed). Slates: `Logs/n56/slate.json`, `Logs/n55/slate.json`. Bot numbers
are labelled bot. This is each entry's last attempt: a FAIL closes it SHORT.

## B1. G1 rank56 (sim row; ships OFF for GG to try)
- doneWhen: the slate. Evidence (bot, the lead's runs at a59e54ac, 10 seeds, n52 levels): `Logs/n56/bot-r4-rank/flow.out` (rank56 forced on)
  vs `Logs/n56/bot-r4/flow.out` (off): the STYLE / PAYOFFS lines; a256 sides g1on / g1off on r4 (`Logs/feel/n56-r4/summary47.txt`) and g1on
  on r5. Golden / fingerprints: NIGHT56-LOG.md's gate lines (every pick). StyleSim's golden records: the golden total line (39 sets).
- Frame (1): the a256 frame the rank leaves DULL, if the r5 g1on folder has one (`Logs/feel/n56-r5/g1on/a256/`; list it).

## B2. G2 kill55's fix (= NIGHT55 P1-kill), attempt 2
- doneWhen: the slate (G2) and Logs/n55/slate.json (P1-kill). r2 FAILED: the reel RED (marks 0.12 %), the 3rd+ holds 4 frames (0.067 s), a354
  jitter 0.212. The fix: chainS 0.08 -> 0.09 (5 frames at 60 fps), marks darker / larger.
- Evidence: sides k5on / k5off (a355) and m5on (a354, kill55 on) on r5; a354's off side is r2's m2off (`Logs/feel/n56-r2/summary47.txt`). The
  r2 blind pair on the frame 1.5 s after a chain (Logs/n56/pairs-r2-kill, judged blind at r2: ON picked in both orders) stands.
- Frames (2): `Logs/feel/n56-r5/k5on/a355/shot-N55-K-hit-next.jpg`, `.../k5on/a355/shot-N55-K-hit-after.jpg`.

## B3. G3 tokens56 (sim row)
- doneWhen: the slate ((bot) >= 80 % of attack starts inside the sim cone; ranged hit rate at dash speed <= 50 % of standing; per-enemy
  idle-in-place <= 20 %; deaths per level inside the flow table's band vs flow-base; golden / fingerprints EQUAL off).
- Evidence (bot, the lead's runs at a59e54ac; tokens56's code unchanged since): `Logs/n56/bot-r4-tokens/flow.out` (on) vs
  `Logs/n56/bot-r4/flow.out` (off): the TOKENS / RANGED / IDLE / SUMMARY lines; `Logs/n56/flow-base.out`. No frames (a sim rule).

## B4. G4 THE KILN WYRM's tail (design.n55.wyrm55, sim)
- doneWhen: the slate (n47human --boss wyrm --level n52-kiln --seeds 40 vs the A0 baseline: 40 / 40, median 60-120 s, p90 <= 180 s, deaths
  <= 10; every attack pinned by a test). Evidence (bot): `Logs/n56/wyrm-base.txt` (A0, row off) vs `Logs/n56/wyrm-r5-on.txt` (the lead's run on
  b6ab1f10, --rows wyrm55). The pinning test: say "not evidenced" unless a test run is in NIGHT56-LOG.md. No frames.

## B5. V1 colour56 (attempt 3; P0-decor folds into it, D-18)
- doneWhen: the slate. r2 FAILED (1, 4): iceline / span whole-frame tint; the world's saturation above the tells.
- Evidence: sides v5f / v5i / v5k / v5s / v5v (on) and v5foff / v5voff (-skygrindOff56), scenario a356; run `python -I Tools/night/n56-colour.py
  Logs/feel/n56-r5/<side>/a356 --kinds entry,mid` on each. The r2 blind pairs (foundry b1, kiln kb2 entries: ON picked in both orders) stand.
- Frames (4): `Logs/feel/n56-r5/v5i/a356/g2/shot-N56-n52-iceline-g2-entry.png`, `Logs/feel/n56-r5/v5s/a356/` one span entry frame (list it),
  `Logs/feel/n56-r5/v5f/a356/b1/shot-N56-n52-foundry-b1-entry.png` and `Logs/feel/n56-r5/v5foff/a356/b1/shot-N56-n52-foundry-b1-entry.png`.

## B6. V2 rim56
- doneWhen: the slate (on a452 mid frames the enemy rects' mean V and S above the world's p90 on >= 90 % of enemies in view; red-orange theirs).
- Evidence: the meter's V2 line on the r5 a356 sides (and on r2's v1* sides: rim56 off then). Frames (2): one kiln and one span MID frame
  with enemies (`Logs/feel/n56-r5/v5k/a356/`, `.../v5s/a356/`: list and open one each).

## B7. V3 yellow56
- doneWhen: the slate (yellow 45-70 deg S > 0.5 outside parry tells and enemy / boss rects <= 0.5 % per frame on verbs37 x5).
- Evidence: the meter on the verbs37 JPGs: `python -I Tools/night/n56-colour.py Logs/feel/n56-r5/v<f|i|k|s|p>/verbs37` (each), and on the a356
  sides (its V3 line). Boss / enemy yellow parts are GG's design (not counted). Frames (2): `Logs/feel/n56-r5/v5f/a356/a5/shot-N56-n52-foundry-a5-entry.png`,
  `Logs/feel/n56-r5/v5v/a356/v5/shot-N56-n52-spire-v5-mid.png`.

## B8. V4 the boss palettes' second round (= NIGHT55 P0-wickGrade, P0-hotWall, P1-wyrmTurn)
- doneWhen: the slate (V4) and Logs/n55/slate.json (P0-wickGrade, P0-hotWall, P1-wyrmTurn). THE WYRM is the orange boss by GG's own word
  (NIGHT47: "the same yellow-red-orange color theme"): its line is the turn's wall hue inside 15-55 deg (D-24), not "walls >= 30 deg from
  danger".
- Evidence: sides b1on (THE WICK, n52-foundry), b2on (THE CROWN, n52-spire), b3on (THE WYRM, n52-kiln) on r5: the `a155:` lines (GRADE / HOT
  WALL / the ride panels' cyan); the OFF sides: r2's b1off / b2off and r4's b3off.
- Frames (4): `Logs/feel/n56-r5/b2on/a155/shot-N55-crown37-p1.jpg`, `Logs/feel/n56-r2/b2off/a155/shot-N55-crown37-p1.jpg`,
  `Logs/feel/n56-r5/b1on/a155/shot-N55-wick-p1.jpg`, `Logs/feel/n56-r5/b3on/a155/shot-N55-wyrm-turn.jpg` (list the folder if the name differs).

## B9. P1-wickWindow (NIGHT55 wick55)
- Evidence: the bot (`Logs/n56/wick/`, `Logs/n56/wick-bot.out`); Wick55Tests 4 / 4 in the Editor at 02:27 (NIGHT56-LOG.md r3; was 3 / 4: the
  test compared the raw design.n37 legs); FIGHT MODE on r4 (bfon / bfoff in `Logs/feel/n56-r4/summary47.txt`). No frames.

## B10. P1-teachText (NIGHT55 brief55), attempt 2
- Evidence: `node Tools/night/n55k-teach.cjs Logs/feel/n56-r5/vf/verbs37/telemetry.jsonl foundry Logs/feel/n56-r5/vi/verbs37/telemetry.jsonl iceline
  Logs/feel/n56-r5/vk/verbs37/telemetry.jsonl kiln Logs/feel/n56-r5/vs/verbs37/telemetry.jsonl span Logs/feel/n56-r5/vp/verbs37/telemetry.jsonl spire`.

## B11. P0-smoke (NIGHT55), attempt 3
- r2 FAILED (1): "one test ... that drops focus while Playing and asserts flow -> Paused. Then one steal smoke with no agent running."
- Evidence: `Assets/Tests/EditMode/Game/N56FocusPauseTests.cs` (3 / 3 in the Editor at 04:29; commit 2d38852e: the rule as
  SurvivalGame.PausesOnLockDrop, behaviour unchanged); the steal smoke on n56-r5 with no builder agent running (`Logs/n56/smoke-steal-n56-r5.out`,
  `Logs/player/n56-smoke-steal-n56-r5.log`: the steals' "took True" lines, the `focus LOST` lines, the DONE line). No frames.
