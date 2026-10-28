# FORGE72 decisions (2026-10-27; GG: "i dont wnat to give any input"). Each with the rejected alternative and the evidence.

GG's earlier delegation stands: DECIDE57 D-1 ("1. you decide": ship when better than the baseline and the guards hold) and FORGE71's
"Bots guard, I judge". Calls only GG can make go on DECIDE72.md with the default the night uses.

## D-1 Ship ON by default, each change with a player off switch (E72, the forge experiment)
- Evidence: w61-w71 built about 40 rows and left them OFF; 76 design rows are OFF today (off-rows.txt), most "until a critic gate
  passes" that never ran (off-why.txt). GG after w71: "I asked previously about stuff related colors and animations and consistency and I
  just haven't seen it". The evening of 2026-10-26 shipped eight changes ON (ultra71, bright71, the roof, the l1.wallrun line, the accents,
  the pips) in 2.5 hours, and GG's answer was "keep double jump and do the rest".
- Rule: a row goes ON in the repo config (both copies) when all four hold:
  - (a) its same-build pair exists: the ON side `-skygrindRows72 <row>:1`, the OFF side the repo's config;
  - (b) the blind spotter names a change on that pair;
  - (c) a guard run with the row `:1` holds: verbs37 on the close's sides with no new RED, and the smoke DONE, whose baseline checks are
    the Player fingerprint;
  - (d) the lead looked at the pair and saw no new defect from gg-proxy.md's list.

  No critic per slice. Off switch: `-skygrindNo<Row>72` (the night's rows) or `-skygrindRows72 <row>:0` (any row), which boot like a
  player's game.
- Rejected: OFF rows plus a play-build config (w71: 0 of 8 rows seen); a critic gate per row (w61-w71: the gates did not run, so the rows
  stayed OFF).

## D-2 THE HARVEST first: the built-but-OFF view rows that answer GG's words (harvest.md)
- About 25 candidates, judged by group pairs on one early build (the lead's H train). Rows that pass go ON one commit each. Rows that need
  work go to the owning builder's list. Rows with no visible change in their pair stay OFF with the number.
- Rejected: building new rows first. The harvest is cheap, visible, and was paid for by earlier nights.

## D-3 A generic row switch in the shared commit: `-skygrindRows72 <row>:<0|1>[+<row>:<0|1>...]`
- Rows are joined with `+`, because n72-reels.sh splits a side's arguments on commas.
- Applied in SurvivalGame's Awake right after GameConfig.Load (SurvivalGame.Init.cs:25), before MovementTuning71.Apply and
  Bright71.Create.
- It sets `design.<nNN>.<row>.enabled` in the loaded config for VIEW and LEVEL rows, and NightRows.ReelOverride for ViewOn readers.
- It refuses sim rows (`"sim": true`): a tape bit must not change from a command line.
- `-skygrindNo<Row>72` maps to `<row>:0` for the night's own rows. Older rows use `-skygrindRows72 <row>:0`.
- All of these boot like a player's game (N29Fixes.IsAutomationArg).
- The shared commit also pre-places the reel hooks (FeelReel.N72.cs partials: still72, stuck72, bosskill72), so no builder edits
  FeelReel.N57.cs (red team 8).
- Why: one build then shows any harvest row ON or OFF, so pairs need no rebuild. NightRows.ReelOverride already exists (NIGHT63 D1), but
  only for ViewOn readers.

## D-4 GG's PLAY71 keep / drop: default ON for the four level rows, after the roof is gone and the guards hold
- GG played on 2026-10-26 and named only the 1.3 RIDE roof (its plan still says roof true: Design/levels/n65/foundry.plan.json a3) and
  "the boss fight is fine". The rebuilt rooms are his "complete arena overhaul" ask (2026-10-09).
- Rule (red team 4): N0 assembles the levels in list mode, as n71's play build did. First it re-exports a3, since the old export still
  had the roof (spike.md S3). The slots files stay all old: their writer needs a room GATE, which is out tonight. Each row flips ON on
  its own result, when verbs37 on its level (`:1`) has no RED row that the shipped level passes (`:0`, L-154) and the smoke is DONE.
  Off: `-skygrindRows72 foundry65:0` etc.
- Rejected: waiting for GG's boxes (he gives no input; w71's play build had them ON and he did not object).

## D-5 Four builders by file ownership, plus the lead
- L LIGHT & SCREEN (flicker meter, flicker sources, boss and room darkness, the noise border, the low-health edge, z-fighting).
- C COLOUR & DECOR (the colour table and its offenders, each level's material contrast, the stripes, fixtures).
- B BOSSES & BODIES (the wyrm and THE CROWN read as creatures, their effects off the crosshair, boss text, springs on enemies, kill feel).
- N NAVIGATION & MOVEMENT (the bunny-hop reel window, the reach audit, the room line and stuck hints, the ULTRAKILL camera, the parity lab).
- The lead: the harvest, the trains (build, guards, pairs, spotter, flips), the level rows, the play build, the close.
- Rejected: five builders (L-003: the usage limit stops everyone together); a cleanup builder (w71 did that; not GG's ask tonight).

## D-6 Bots guard, the spotter checks visibility, the lead decides, GG judges
- Per train: verbs37 on the five shipped levels (plus the new level sides once their rows are ON), the smoke, the gates for any Core
  change, and the pairs for each landed slice. The spotter (skygrind-spotter) sees two unlabelled frames. No skygrind-critic per slice.
  One skygrind-reviewer pass on the integrated diff before the freeze; gg-proxy on the freeze frames for the report's "still not good".

## D-7 Movement: extend the ULTRAKILL profile from data only; Mover.cs untouched
- Our ground and air are Quake style (MovementMath.Accelerate and ApplyFriction: ground accel 10, friction 6, air accel 1.5). ULTRAKILL's
  ground closes 25% of the gap per 0.008 s (rate 36/s, half-life 19 ms; scout-movement.md 3), so our stop is about six times slower. A
  profile value (ground.friction about 36, ground.accel about 36, air.accel about 3.6) can come close without code. The parity lab
  measures it first, and a value goes in only if verbs37 holds.
- After any profile change, record a new FingerprintBaseline.Ultra71Hashes from two Player runs with `-skygrindFingerprint` that print
  equal hashes.
- The double jump stays (GG: "keep double jump"). The base FOV stays 74 vertical (cam72 only kicks it on a dash): about 106 horizontal at 16:9, the same as ULTRAKILL's 105
  default if that is horizontal; DECIDE72 Q-4.
- Rejected: Mover code changes for the per-axis air cap (golden-pinned; the profile approximates it).

## D-8 The night's clock: cap 7:30, freeze 6:45, the close from 6:30, code freeze at 6:00
- GG: "6-7.5 hours hard cap". The close check accepts a clock stopped between 7:00 and 7:30 (capMin - 30), and a close started at 6:30
  or later. A usage stop that eats the close gives the minimum close (the tool's existing rule).

## D-9 Text size: new guidance lines at least 22 px at 1080p
- Xbox XAG 101 minimum body height is 18 px on PC and 26 px on console at 1080p (scout-guidance.md 7). CLAUDE.md says 16 px, so lines
  at 22 px meet both readings on PC. Raising the floor is GG's call (DECIDE72 Q-5).

## D-11 body72 is a restyle inside the pill language, not a new creature (red team 14)
- GG: "the boss fight is fine" (2026-10-26); "keep the worm AI the same" (2026-10-13); keep "the pill-shaped enemy concept" (2026-10-16).
  CLAUDE.md rule 7: enemy design is his.
- body72 changes the wyrm's presence (a rim light, value apart from the wall), its pill segments' spring follow-through, and a tell glow
  in the danger colour. The silhouette at rest stays the same pill segments. spring72 moves existing models and does not reshape them.
- DECIDE72 names body72 first, with its pair, as the change most likely to be dropped.
- Rejected: a jaw-and-eyes creature head (the first draft's B1).

## D-12 Trains run on events, and the harvest flips from one combined guard run (red team 2, 3)
- A train is about 90 minutes of the lead's time, not 70. Start one when two builder commits wait, or 80 minutes after the last.
- The H train ends with one guard run that sets every passing harvest row `:1` together (the close's verbs37 sides and the smoke), so its
  rows flip by about 1:30, before the usage window measured on NIGHT66-68 (about 2:20).
- The last pair train picks by 4:50. After that, builders do work that needs no new pair.

## D-10 Not tonight
Enemy kinds and waves (GG's), audio, Mover.cs and `simulate()` order, sim rows, a new level, the HUD layout (the pips stay where
2026-10-26 put them), tonemapping changes (GG's look), the six DECIDE45 D-3 rows, dead-code batches, the room GATE as a verdict.
