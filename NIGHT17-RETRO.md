# NIGHT17-RETRO: what the overhaul night proved, and what it didn't (written 2026-10-06, after `u3-n17`)

This is the input to NIGHT18 (the feel-in-motion night). The sources are `NIGHT17-LOG.md`, `git log u3-n16..u3-n17` and
the captures in `Logs/shots/n17-*`. Every number below was measured or read from those; nothing here was played by a
person.

## 1. The numbers

- **Output:**
  - 185 commits. By builder: H 42, X 38, N 32, V 23, M 8; the lead and docs 44.
  - 125 script / test files changed, +14,782 / -2,279 lines.
  - Core: 7 files, +332 lines (stamina and the freeze length).
- **Gates held all night:**
  - golden 0 mismatches and 22 / 22 replays exact at every gate;
  - the composed fingerprint EQUAL (re-recorded once, on purpose: `964899b`);
  - the suite grew 850 -> 969 tests, all passing;
  - build 6's smoke passed on its re-run.
- **Six Player builds and four Play checks.** Builds 2-5 were rubric rounds; build 6 was the close's (a 582 s full
  rebuild).
- **The rubric (10 rows x 5, out of 50):**

  | Round (build) | The lead | A fresh reviewer |
  | --- | --- | --- |
  | 0 (n16) | 17 | none |
  | 1 (build 2) | 30 | 25 |
  | 2 (build 3) | 35 | 23 |
  | 3 (build 4) | 37 | 27 |
  | 4 (build 5) | 34 | 27 |
  | final (build 6) | 35 | none |

- **The fresh reviewers' rows, rounds 1 -> 4:**

  | Row | Rounds 1-4 | Change |
  | --- | --- | --- |
  | Kill crunch | 2, 2, 3, 2 | none |
  | Hit feedback | 3, 2, 3, 3 | none |
  | Weapon feel | 3, 2, 3, 3 | none |
  | **Movement feel** | **2, 2, 2, 2** | none |
  | Score energy | 2, 2, 3, 2 | none |
  | Style meter | 3, 3, 2, 3 | none |
  | HUD clarity | 2, 2, 3, 3 | +1 |
  | **Menu simplicity** | **3, 3, 4, 4** | **+1** |
  | Identity | 3, 3, 2, 3 | none |
  | **Motion** | **2, 2, 2, 2** | none |

## 2. What went well (keep it)

1. **Parallel builders by file ownership, tests first, integrated by cherry-pick.**
   - Five builders landed about 140 commits with no merge conflict.
   - Every integration compiled.
   - Red-then-green test commits caught real mistakes (ImpactRulesTests encoding a stale assumption, HandsTests'
     overlap floor).
2. **The determinism wall.** No effect leaked into the sim: the fingerprint stayed EQUAL across about 150 effect
   commits. "Effects are pictures" plus a fingerprint gate is a pattern worth keeping.
3. **What can be judged from a still improved for real:**
   - menus cut from walls to 4 items;
   - one theme (`UiTheme`, IDENTITY.md);
   - RESULTS / SETTINGS / TITLE restyled;
   - the style meter on a plate;
   - drawn key chips;
   - TITLE now shows the dusk.

   Every reviewer agreed on these.
4. **A fresh critic each round found what the lead stopped seeing:**
   - the 10 px speed readout (a hard-rule break, missed by the lead for 3 rounds);
   - overprinting pops;
   - two lit buttons on RESULTS;
   - gold used on non-points mods.
5. **The Editor reel as an emergency loop.** When builds ran out, one guarded eval ran the reel in Play mode in about
   1 minute, which gave two more fix rounds.
6. **Diagnosing the environment instead of blaming the build:**
   - the smoke's failures were focus pauses (`SurvivalGame.cs:279`);
   - the Editor's TITLE pad steps fail because the attract runs there (the Editor isn't automation);
   - the Editor plays GG's saved map (DRYDOCK).

## 3. What did not work (change it)

1. **"Implemented" was counted as "felt".** The clearest case:
   - Hit tilt (11 deg in 25 ms, back in 70 ms) and knock-back (heavy / melee / parry only) were built and unit-tested
     in round 1.
   - Every reviewer still wrote "the capsule never moves or tilts". On a symmetric capsule an 11 deg, 95 ms tilt barely
     changes the silhouette, and a revolver body hit gets no knock-back at all.
   - The night had no check that asked "did the screen change?". Builders were told adjectives ("tilt it", "make it
     read") and returned code that met the adjective in the code, not on the screen.
2. **Stills judged motion.**
   - The rows that never moved (movement 2, motion 2, kill crunch 2-3) are exactly the ones a contact sheet can't show.
   - The strips sampled every 2nd-4th frame at 1/3 scale. Reviewers measured a 160 x 9 px bar as "75 x 4". A 2-frame
     burst fell between sampled frames. The sprint strip started after the FOV widen had finished.
   - Feel needs per-frame telemetry and full-resolution frame sequences, not a mosaic.
3. **One reviewer per round is a noisy instrument.**
   - The same build quality scored 23-27 depending on the reviewer.
   - An absolute 1-5 score with no anchored examples drifts.
   - The lead's own scores ran 5-12 points higher every round (self-preference).
   - Pairwise "before vs after on the same frames" was never used, although it's the more reliable judgement.
4. **Symptoms were patched instead of experiences being designed.** Fix lists turned each reviewer bullet into a task
   ("the decay bar 8 px", "the glint seated or gone"), so the night chased about 60 small items. Nobody owned the one
   question GG asked: is a kill satisfying?
5. **Scenario design hid effects:**
   - kills at whatever distance the scenario happened to give (7-20 m);
   - no pre-roll before a sprint or a boost;
   - the Editor on DRYDOCK;
   - the horde scenario killed one of three bodies;
   - the kill flourish played after the strip ended, so it was never seen.
6. **Time:**
   - A usage-limit stop cost 100 min (00:11-01:51).
   - The 8-hour minimum then forced 75 min of idling (03:40-04:55) once the work was frozen.
   - The final build was a 10-minute full rebuild, where earlier builds took 17-28 s.
   - No backlog existed for spare time.
7. **Caps bit at the end:**
   - captures 150 / 150 and reviewers 4 / 4 left the last two fix rounds unscored by a fresh eye;
   - the final reel had 7 frames.

## 4. Not done, or not built at all

| What | State |
| --- | --- |
| Kill crunch at range (10-20 m) | Built late (round 6's screen-size floors), seen only in the Editor on DRYDOCK; still reads as a ring and sparks in the final KESTREL strip |
| Hit reaction that reads | The tilt and knock-back exist but don't show on screen (above) |
| Movement feel | The FOV widen and edge lines exist, but no strip captured a boost's onset; reviewers scored 2 every round |
| UI motion (no raw pop-ins) | Fixed one by one where a reviewer saw one; no check finds them all |
| The kill flourish (V) | Built, tested, never seen in a capture |
| The low-HP vignette pulse | Not built |
| The lesson card / tips | Still ASCII "[E] BUY GUN" |
| HUD STYLE (ATTACHED) | Not ported (since NIGHT16) |
| The spawn ring | Reads near white, not DANGER |
| The world's off-role hues (cyan trims, lime beam, the guns' gold) | GG's decision (NIGHT17-REPORT.md section 4) |
| Hit feedback / weapon feel rows ≥ 4 | Never reached (3 at best) |

## 5. What NIGHT18 changes because of this

1. **Done means measured on screen.**
   - The game logs per-frame telemetry in its reel: the camera FOV and roll, each effect's screen-space size, the
     victim's screen displacement, the freeze frames, each UI element's scale and alpha on its first frames.
   - A metrics tool reads the logs and the full-resolution frames.
   - Every feel item has a numeric target from the research.
2. **Motion is judged as motion:**
   - every rendered frame at full resolution, at a fixed capture rate, with pre-roll;
   - side-by-side before / after flipbooks and animated previews.
3. **Judges are paired and independent:**
   - two fresh reviewers per round;
   - blind pairwise A/B on identical scenario frames, with sides swapped between the two reviewers;
   - absolute scores against anchored examples;
   - the lead's score is advisory only.
   - A `FeelAB` switch keeps NIGHT17's look in the same build: the A side is the same code with the switch off, and GG
     can toggle it live (F6 in a dev build).
4. **Fewer, deeper goals:** the kill (at every range), the hit, the movement and UI motion. Each is owned end to end by
   one builder against its metric, not a list of symptoms.
5. **Time:** a 5.5 h minimum / 6 h cap, a backlog for spare time, a resume note in the log after every integration,
   and the final build started with a 15-minute allowance.
