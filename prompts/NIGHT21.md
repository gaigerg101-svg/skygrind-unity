# NIGHT21.md: SKYGRIND in Unity: THE MOVES AND THE MOMENTS (a 5-6 h building night: five new features and a moonshot, behind switches)

<library_header>
id: skygrind-n21 | version: 2 (red-teamed) | date: 2026-10-07 | model / effort: claude-opus-5-5 / high
raw request: game-prompt-playbook/library/2026-10-07_skygrind_night21-request_master-prompt-forge.md | outcome: (after the run)
planning set: Research/2026-10-07-wave21/ (RESEARCH.md, IDEAS-POOL.md, DECIDE21.md, research/*.md) | forge: wave-forge
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic
120 Hz C# sim with its own collider and an input tape that replays runs bit for bit), in
`C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`, base tag `u3-n20`.

**Run length: 5 to 6 hours from your first `date`.** The close table prints at 5:00 or later, and 6:00 is the hard
stop. The only early stop is the close table with every row DONE or SHORT.

GG asked, verbatim (2026-10-07):
- "I WANT BUILDING new fetaures that match and ideas, innovate, resreach"
- "I want to see notable changes a knot changes as in cutting things but additions innovations role played iterate"

After NIGHT19 he wrote: "I need a lot of building; I'm not seeing enough building."

**Why this shape.** The forge's research (`Research/2026-10-07-wave21/RESEARCH.md`) found three things:
- AI-built game features fail by being "defined but not wired" and by self-grading. The fix is a JSON feature list
  with a `passes` flag flipped only on evidence, and a contract written before code.
- Blind pairwise picks never predicted GG; veto findings did.
- Six builders hit the spend limit at ~3 h on NIGHT19 and NIGHT20.

The forge also found SKYGRIND's own "defined but not wired": the web's ARCLINE grapple is ported into the mover but
never switched on in Survival, and the web's endless mode is still "Missing". So tonight is five playable slices and a
moonshot, three builders, contracts with `passes` flags, persona triage, veto reviewers, and at least 70 % of the
agent-minutes spent building.

NIGHT21 does these things, in order:
1. **Start and the shared commit:** contracts, the N21 switches, tape format 6, the pre-placed harness and reel
   partials, and three builders launched by 0:55.
2. **THE ARCLINE in Survival** (builder M): the web's 5500 grapple gadget. Swing on nodes and surfaces, zip to big
   enemies, yank small ones on a long cooldown.
3. **SLIDE BITES** (M): the slide hits what it passes through.
4. **THE STAMP** (builder S): the magenta rank letter fixed first, then round grades with a round P and an inked rank
   stamp. The RESULTS grade card stops covering POINTS.
5. **DEADLINE** (S): a kill-or-die timer twist round.
6. **THE GRIND** (builder W): the web's endless mode (`endless.js`, "Missing" in PORT-STATUS) on SURVIVAL's core, which
   reshapes every wave.
7. **The moonshot, INSTANT REPLAY** (W, 90-minute cap, on NIGHT14's ghost recorder); then the beyond rows.
8. **The loop:** four rounds, persona triage, veto reviewers, fix lists.
9. **The close.**
</role_and_goal>

<facts_already_measured>
Read 2026-10-07 07:45-08:12 at `0ca1a3c` (`u3-n20` + 1). Re-check them; don't trust them blindly.
- **The gates:**
  - The suite is 1569 / 1569, replays 22 / 22, golden 97 / 97; it took 22 min on a quiet Editor and 36 min under load.
  - `fingerprint` EQUAL FingerprintBaselineN20 (N19OnPath true; step 7200: round 4, 4775 earned, 8 alive);
    `--off19` EQUAL FingerprintBaselineN18. CoreHarness golden 0 / 22 exact.
- **The ARCLINE:**
  - `SurvivalSim.cs:161` reads `ropeEnabled = false; // the ARCLINE is a 5500 gadget in Survival (not owned at start)`
    and nothing sets it true.
  - `Core/GrappleAim.cs` has no enemy branch.
  - The reference is the web's `src/combat/arcline.js` (yank small / zip big / node / surface) and
    `src/survival/gadgets.js` (the terminal).
  - `PortExport/config.json` `arcline`: price 5500, cooldown 0.4, softCap 42, pullKinds husk / swarmer / drone, zip.
  - `PortExport/maps/survival.json` has 17 `grappleNodes`. The tape records `grapple` (RunTape.cs:80).
  - The v3 pad has Y = SPECIAL + ARCLINE (`Core/Gamepad.cs`).
- **The style grades:** the run-level P grade exists (`design.grade.p`, `Core/Payoff.cs`, `Game/UiPayoff.cs`). The
  magenta square is on tonight's rank letter / grade badge (6 of 6 reviewers; `Logs/feel/final20/on/hud-fight/f0060.jpg`).
  The UI files are `Game/UiStyleMeter.cs`, `RankWorld.cs`, `StyleTile.cs`. T-11 (aged 3 nights): the RESULTS grade card
  covers POINTS.
- **Endless:**
  - The web's `src/modes/endless.js`: a 4 s break (full heal + stamina), the WAVE banner, the reshape hook, telegraphed
    spawns, death ends the run, score = style + 100 x waves; events `waveBreak`, `waveStart`, `waveClear`, `runEnd`.
  - `PortExport/config.json` `endless` has breakTime 4, bannerTime 1.6, reshapeTime 1.5, pointsPerWave 100.
  - `Core/CoreGrid.cs` has `SetHeights` (:264) and `PresetHeights(...)`. SURVIVAL's core stays flat in its own mode.
  - TITLE's MAP label "ARENA" is the default map (`UnityTuning.Maps.cs`), not the training `Arena.unity`.
- **Already built (don't rebuild):** THE GHOST and SPLITS (NIGHT14: `Game/Route/GhostTrack.cs`, `GhostRunner.cs`,
  `GhostStore.cs`, `UiGhost.cs`, `design.ghost` on).
- **Twists:** `Core/Enemies/SpecialRounds.cs` (FOG, SWARMAGEDDON, LETHAL, GOLDEN under `design.adds.twists`).
- **The tape:** `RunTape.FormatVersion` 5 carries the N19 / N20 block bits, so N21 needs format 6.
- **The machine:** other sessions run long python jobs and a llama-server. At 05:38 the CPU read 100 %, and both final
  benches were worthless. `Tools/night/bench.sh` now skips itself above 30 % CPU.
- **Helpers:** `Tools/night/` (README): smoke, bench, reel, wait-build, wait-tests, integrate, close, rehearse,
  final-judges, decode, gifs, resolve-adds, `persona-triage.md`, `reviewer-veto.md`.
- **Memory:** `docs/memory/` (CORE imported by CLAUDE.md; LESSONS, OWNER, THREADS, FEATURES, IDEAS, ledger/).
</facts_already_measured>

<scope>
- **In:** the five features and the moonshot, each behind `design.n21.<row>`; their views, HUD, reel scenarios,
  harness probes, tests and events; the beyond rows in order; T-11 inside THE STAMP.
- **Out:** audio (rule 7: every new moment emits a named event); new enemy designs; the web repo; the map night
  (`QUEUE-map-night-2-v5.md`); balance retunes (S2 / S4 / S6, MARATHON's heal: parked on DECIDE21). A new idea goes to
  `docs/memory/IDEAS.md`, never into code.
</scope>

<lean_rules>
- **Testing:** one focused test per new rule, seen red once. The full suite once, before the freeze (U3-35.11). The
  Player smoke on every build. Tests verify the work: never edit a test to make it pass.
- **Wired, not just defined:** a feature passes only when:
  - its contract's check printed its line on tonight's Player build or harness;
  - `-skygrindOff21` / `--off21` gives NIGHT20's game (a test);
  - every new config key is read by code, every event is in PORT-STATUS, and every UI list, save and settings row is
    updated.

  A stand-in or a display-only feature is SHORT.
- **Switches:** a row is `enabled: false` in the shared commit and turns on in both config copies with its first
  playable slice. The off side is `--off21`, and the fingerprint keeps every N21 block off until the freeze.
- **Numbers:** a ported feature reads the web's config (`PortExport/config.json`: `arcline`, `endless`). Only
  Unity-only numbers go in `design.n21.<row>` (both copies, each with its `_why`, and the commit saying why the web had
  none). Visual numbers go in `UnityTuning.*.cs` with a comment.
- **Looks:** no new shader or material unless it ships in the Player build (Resources or Always Included). Check the
  Player's frames, not the Editor's: NIGHT20's magenta letter passed in the Editor.
- **Caps** (a cap reached = SHORT with the count):
  - Player builds: 7 (build 1, four rounds, the final, one spare).
  - Reels on Players: 14.
  - Farm batches: 4, where a batch is 2+ processes or 20+ runs, always alone. A single-process probe isn't a batch but
    never runs beside a bench, a smoke or the suite.
  - Bench runs: 4, CPU-gated.
  - Agents: 3 builders alive (5 in total with fresh restarts), 4 veto reviewers, 5 persona triages.
- **Commits:** every finished slice (explicit paths + metas; `git commit -- <paths>`), at least every 45 minutes per
  builder.
- **Time boxes:** `date` at every phase boundary into `NIGHT21-LOG.md`. A phase 20 minutes over its box is cut, and
  its rest goes to the leftovers.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (it imports `docs/memory/CORE.md`), this file, `Research/2026-10-07-wave21/RESEARCH.md`.
Then `docs/memory/THREADS.md` and `Research/2026-10-07-wave21/DECIDE21.md`. **GG's answers come first:** act on any
filled DECIDE21 Answer, or any word from GG about NIGHT20, before U3-35.3, and log how.
**Key files:**
- `Game/SurvivalSim*.cs` (the loop, in the web's `simulate()` order)
- `Core/Mover.cs` (the rope; changed only for the ARCLINE's zip / pull, a task naming it)
- `Core/GrappleAim.cs`, `Core/Guns/Shockwave.cs` (the gun-module pattern), `Core/CoreGrid.cs`, `Core/Payoff.cs`,
  `Core/Enemies/SpecialRounds.cs`, `Core/Style*.cs`
- `Game/Route/Ghost*.cs`, `Game/FeelReel*.cs`, `Tools/feel-metrics.py`, `Tools/SimHarness/`, `Game/Tape/*`,
  `Game/PlayerChecks.cs`

**Git:** stay on `port/stage3`; never push, merge or commit to master; tag `u3-n21` once on the close commit.
**The rules that bite:** CLAUDE.md 1 (explicit paths; never `UnityConnectSettings.asset`; restore the TMP fallback and
Mobile_RPAsset after Play or builds), 3 (golden exact), 4 (the C# collider, no PhysX), 5 (numbers), 6 (Mulberry32),
7 (no audio).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, and move on. What only GG decides goes in
  `DECIDE21.md`, never into code.
- If one approach fails twice, switch to the next-best one and log both.
- The stops you want: the close table, or the hard cap. Stops you don't want: a summary announcing the next step with
  no tool call; an offer to continue; a list of decisions that block nothing; "a good place to report". (The kickoff
  appends `PROMPT-STOPS.md`.)
- On a usage or spend limit: commit, log the gap with `date`, keep building alone, and retry agents at most every 30
  minutes. A row the limit stops is SHORT with that `date`.
</autonomy>

<builders>
Three builders in git worktrees, split by file ownership, each with a queue. The lead builds the shared commit and the
loop lines, integrates (`Tools/night/integrate.sh`), and runs every build, reel, probe, review and gate. When free, the
lead builds beyond rows.

Each brief holds:
- its contracts from `NIGHT21-FEATURES.json`, verbatim;
- its files;
- "commit every slice; tests first, seen red; the off-test first";
- a report under 300 words: commits, what a player now does, the contract's printed line, what's not done.

| Builder | Owns | Queue |
| --- | --- | --- |
| **M** (moves) | `Core/GrappleAim.cs`, `Core/Arcline21*.cs`, the rope lines in `Core/Mover.cs` (zip / pull only), `Core/SlideBite21*.cs`, the ARCLINE views + gadget terminal, `Harness.N21.Arcline.cs`, `Harness.N21.SlideBite.cs`, `FeelReel.N21.Arcline.cs`, `FeelReel.N21.SlideBite.cs`, the `design.n21.arcline` / `.slideBites` blocks, their tests | THE ARCLINE, SLIDE BITES, then beyond 4 |
| **S** (style and rounds) | `Core/Style*.cs`, `Core/Payoff.cs`, `Core/Grades21.cs`, `Game/UiStyleMeter.cs`, `RankWorld.cs`, `StyleTile.cs`, `Game/UiPayoff.cs`, `Game/UiResults*.cs`, `Core/Enemies/SpecialRounds.cs` (the DEADLINE twist only), `Game/UiDeadline21.cs`, `Harness.N21.Stamp.cs`, `Harness.N21.Deadline.cs`, `FeelReel.N21.Stamp.cs` / `.Deadline.cs`, the `design.n21.stamp` / `.deadline` blocks, their tests | the magenta letter, THE STAMP (+ T-11), DEADLINE, then beyond 2, 3 |
| **W** (world and replay) | `Core/Endless21.cs` (endless.js), `Core/Records21.cs` (records.js' best waves), the GRIND mode lines in TITLE and the HUD (`Game/UiGrind21.cs`), `Game/Replay21*.cs`, `Game/Route/Ghost*.cs` (for the replay), `Harness.N21.Grind.cs`, `FeelReel.N21.Grind.cs` / `.Replay.cs`, the `design.n21.grind` / `.replay` blocks, their tests | THE GRIND, the moonshot, then beyond 1, 5 |

**The lead alone** touches:
- `SurvivalSim.cs` / `SurvivalSim.Run.cs` / `SurvivalGame.cs` / `SurvivalGame.Init.cs` (a builder leaves
  `partial void N21Xxx()` and asks for the one call in its report);
- `Tools/SimHarness/Program.cs` and `Harness.N21.cs` (the dispatch);
- `RunLog.cs` / `Farm.cs`;
- `FeelReel.N21.cs` (the phase list);
- the config merge (`Tools/night/resolve-adds.cjs`, adapted to `design.n21`).
</builders>

<tasks>
## U3-35.1 Start (0:00-0:20)
1. `date` (the night's zero). Read the context files. Run `bash Tools/unity-loop.sh status` (stop Play mode;
   `set_autotick`) and `git status`.
2. Bench baseline x2 on NIGHT20's final build: `BDIR=Builds/n20-final bash Tools/night/bench.sh n21-base-1`, then
   `n21-base-2`. It skips itself above 30 % CPU; log the reading.
3. `NIGHT21-FEATURES.json`: one entry per feature (U3-35.3-.7 and the moonshot) with:
   - `id`, `switch`, `player_does`, `player_sees`;
   - `check`: the exact command or reel scenario and the line it must print;
   - `bot_hook`: how a harness or bot exercises it;
   - `out_of_scope`, `owner`, `passes: false`, `evidence: null`.

   These are the sprint contracts. Builders never edit them; the lead flips `passes` only on tonight's evidence.
4. tasks.json: U3-35 and U3-35.1 .. U3-35.11, before any code.

## U3-35.2 The shared commit (0:20-0:55)
1. **The switches:**
   - `Core/N21Blocks.cs`: six bits (arcline, slideBites, stamp, deadline, grind, replay) from
     `design.n21.<row>.enabled`, plus `ForceOff`, `-skygrindOff21` / `--off21` (NIGHT20's game).
   - The rows in both config copies, all `enabled: false`.
   - RunFingerprint sets every N21 block off unless `FingerprintBaseline.N21OnPath` (false until the freeze).
2. **Tape format 6:** the N21 bits after the NIGHT20 byte, formats 1-5 still read (TapeTests).
3. **Pre-placed partials:**
   - `Tools/SimHarness/Harness.N21.cs` dispatching `arcline`, `slidebite`, `stamp`, `deadline`, `grind` to empty
     partial methods, one file per builder (the table above).
   - `Game/FeelReel.N21.cs` (phase E: `arcline`, `slidebite`, `stamp`, `deadline`, `grind`, `replay`) with one partial
     file per scenario.
   - The empty test classes.
   - The `partial void N21*` hooks in the lead's files.
4. **The meters:**
   - feel-metrics' **M-MAGENTA** (pixels within 40 of (255, 0, 255), target 0), with a self-test on a synthetic magenta
     frame.
   - The run log's per-run counts (`slideBites`, `roundGrades`, `deadlineBurns`, `waves`).
5. **Gates:** `fingerprint` EQUAL (N20), `--off19` EQUAL, golden 0 / 22 exact, `impact PASS`, the Editor compile.
   Commit. Launch M, S and W, with a `date` (target 0:55).

## U3-35.3 THE ARCLINE in Survival (M; ~2 h 15)
- **Player:** buys the ARCLINE at the gadget terminal (`gadgets.js`'s cabinet, at the web's spot or the nearest
  SURVIVAL buy spot; logged) for 5500, then presses its button.
  - A small enemy near the crosshair is yanked to them.
  - A big enemy zips them in.
  - Else a grapple node (17 on SURVIVAL) or any surface swings them.
- **Build:**
  - `ropeEnabled` follows ownership (saved in the run state).
  - GrappleAim's enemy branch ported from `arcline.js`, reading the web's `arcline` config.
  - The events `arclineOwned`, `arclinePull`, `slingshot`, and the mover's `ropeFire` / `ropeAttach` / `ropeRelease`.
  - The rope line, node glows and the hook reticle (cyan; red with nothing in range); a HUD gadget chip.
- **The yank is crowd control:** `design.n21.arcline.yankCooldown` 15 s, because GG's rule is a long cooldown on such
  verbs, 15 s or more (CORE "Never again"). The swing keeps the web's 0.4 s.
- **The shared button:** port the web's rule for SPECIAL vs ARCLINE on one button, and log it (DECIDE21 D-2).
- **Check:**
  - SimHarness `arcline` (scripted, the bot_hook): on SURVIVAL, from 10 seeded spots, it attaches in at least 9; peak
    speed after a swing is at least 20 m/s; a yanked husk moves at least 3 m toward the player.
  - Golden 0 / 22 exact; the off-test equals NIGHT20.
  - Reel `arcline` (buy, swing over the horde, yank); S12 0 on the pad pass.

## U3-35.4 SLIDE BITES (M; ~45 min)
- **Player:** slides through a husk and it takes damage, staggers or gibs; a SLIDE BITE style event; a slide kill
  counts as a close kill.
- **Build:** a contact test along the slide's path with the C# collider. Damage, radius and cooldown go in
  `design.n21.slideBites`; it's Unity-only, because the web's slide never hit (Turbo Overkill's slide is the evidence).
- **Check:** `SlideBite21Tests`; SimHarness `slidebite` (a scripted slide through 3 husks damages at least 2); reel
  `slidebite`; the off-test equals NIGHT20.

## U3-35.5 THE STAMP (S; ~2 h)
- **First: the magenta square.** Find what's missing on the rank letter and grade badge in the Player. Check:
  M-MAGENTA 0 on every frame of both reel passes.
- **Player:**
  - After every round clear an inked grade stamps in (D-S). It's **P** with the run P grade's rule applied to the
    round (`design.grade.p`'s thresholds, `Core/Payoff.cs`).
  - RESULTS shows the round strip, and its grade card no longer covers POINTS (T-11).
  - The style meter's rank letter is an inked stamp per rank.
- **Build:** `Core/Grades21.cs` (pure: a round's damage, peak rank, time; it reuses Payoff's thresholds; Unity-only
  numbers in `design.n21.stamp`); the stamp's entrance (M9 measured, no pop-in); a `roundGrade` event.
- **Check:** `Stamp21Tests` (the round P only under the run P rule's conditions); reel `stamp` (M9 PASS, M-MAGENTA 0);
  the run log's grade per round, and its distribution printed from the round-2 farm (no target).

## U3-35.6 DEADLINE (S; ~1 h)
- **Player:** a twist round with a fuse on the HUD (DANGER role). Kills refill it, slide and slam kills most. Burning
  out costs a heavy hit, not the run, and the round's points double.
- **Evidence:** Mullet MadJack (97 % of 5k+ reviews; its fatigue warning keeps DEADLINE one twist among five).
- **Build:** a twist id `DEADLINE` in SpecialRounds (`design.n21.deadline`: fuse, refills, the hit, its weight); the
  HUD fuse; `deadlineTick` / `deadlineBurn` events; `chase --twist deadline` (the bot_hook, in `Harness.N21.Deadline.cs`).
- **Check:** `Deadline21Tests`; THE CHASE's KITE at round 10 under DEADLINE survives at most 50 % of its normal time;
  reel `deadline`.

## U3-35.7 THE GRIND (W; ~2 h 15)
- **Player:** TITLE's MODE row offers THE GRIND. Endless waves on SURVIVAL's core:
  - a 4 s break with a full heal;
  - the WAVE banner;
  - the core reshapes (`reshapeTime` 1.5 s, telegraphed by rising edges in DANGER);
  - death ends the run, and the score is style + 100 x waves.

  RESULTS shows the best wave per HEAT (records.js).
- **Evidence:** the web's own mode (`endless.js`; PORT-STATUS "Missing") and ULTRAKILL's Cyber Grind (96 % of 207k
  reviews).
- **Build:** `Core/Endless21.cs` ported line for line from `endless.js` with its comments, reading the web's `endless`
  config. The web's events `waveBreak`, `waveStart`, `waveClear`, `runEnd`. The reshape picks the core's next heights
  with `CoreGrid.PresetHeights` and the run's Mulberry32. Add the smoke's GRIND step.
- **Check:**
  - `Endless21Tests` (the web's endless tests ported if they exist).
  - SimHarness `grind`, 10 seeds (one process; the bot_hook is the autopilot in GRIND mode): the median wave reached,
    and the core's heights hash differs on every wave.
  - The smoke step PASS; reel `grind`; PORT-STATUS's endless row updated.

## U3-35.8 The moonshot: INSTANT REPLAY (W; 90-minute hard cap, after THE GRIND)
- **Player:** on DOWN and at a round clear, VIEW replays the last 10 s at half speed, from the camera's recorded path,
  with the enemies' recorded poses.
- **Build:** on NIGHT14's ghost recorder (`Game/Route/GhostTrack.cs`) widened to a ring of camera + enemy-view poses
  (the views bind to the live sim, so this is render-side, not a re-sim).
- **Check:** reel `replay`; the replayed camera path within 0.05 m of the recorded one. Over the cap: SHORT, with the
  slice committed behind its switch, off.

## U3-35.9 The beyond list (in order; each gets a contract added to the json before it starts, a switch, a check)
1. **THE GHOST on the DAILY** (W, about 20 min, on NIGHT14's ghost): it unlocks after a first DAILY clear (Neon
   White's gating), with RESULTS' "AHEAD / BEHIND YOUR GHOST".
2. **THE DUSK CLOCK** (S): the sky sinks per round to night by round 15, the colour roles held (a luminance curve on the
   reel).
3. **Directed steps** for THE HUNT and THE BEACON (S; BO6 Directed: quest completion 4 -> 8.23 %).
4. **Haptics** on kills, the shockwave and the ARCLINE's attach (M).
5. **Challenge codes** on TITLE and RESULTS (W: seed + mutators + target; round-trips to the same tape hash on format 6).

## U3-35.10 The loop: rounds at ~1:50, ~2:40, ~3:25, ~4:05
Each round:
1. **Integrate:** cherry-pick, metas, the lead's hook lines.
2. **Gates:** fingerprint x2, golden, impact.
3. **Compile, build:** build, then the smoke alone.
4. **Reels:** on + pad (two of the 14), then feel-metrics: M8 v2, S12, S14, M9, M-MAGENTA.
5. **Probes:** the contracts' checks, one at a time; flip `passes` only on evidence.
6. **Persona triage** (`Tools/night/persona-triage.md`; one fresh subagent; 10 min, about 40 tool calls): five
   personas grounded in `docs/memory/OWNER.md` and `research/genre-features.md` (the Zombies veteran, the ULTRAKILL
   speedrunner, the Steam Deck couch player, the streamer, GG). The output is a ranked fix list. **Triage only:
   personas never pass a feature.**
7. **The fix lists** go to the builders, with keep / iterate / cut per feature, logged.

Additions:
- **Round 2 (~2:40):** two veto reviewers (`Tools/night/reviewer-veto.md`, sides swapped, 25 min / about 80 tool calls
  each); THE MARATHON 20 seeds + the free farm 40 runs as one batch, alone.
- **Round 4 (~4:05) is the last integrated round:** its fix list must land by 4:20.

## U3-35.11 Close (last picks 4:20; freeze 4:30; table at 5:00 or later, before 6:00)
**At 4:30, freeze. Then, in this order:**
1. **The fingerprint, before the final build:** set `FingerprintBaseline.N21OnPath` true. If the on path differs from
   FingerprintBaselineN20, record `FingerprintBaselineN21` once, with the reason. Check that `--off21` still prints
   FingerprintBaselineN20 and `--off19` FingerprintBaselineN18. Gates; commit.
2. **The final build;** then, alone, `bash Tools/night/close.sh Builds/<final>`: the smoke, the tape copied aside,
   golden, fingerprint (both lines), replay, bench x2 (CPU-gated). That ends about 4:50.
3. **The suite** async on the frozen code (4:50 to about 5:30). Beside it: the final reels (on, pad), then two final
   veto reviewers and one persona triage on them. No farm runs beside the suite.
4. **After the suite:** the contracts' probes one at a time, and THE MARATHON 20 seeds (one batch, alone), done by
   about 5:45.
5. **The docs:**
   - NIGHT21-LOG.md;
   - NIGHT21-REPORT.md (GG's page: per feature, what he can do, its switch, its number, the persona line);
   - FEEL-PLAYTEST.md (10 minutes, pad first);
   - PORT-STATUS (every new event; endless no longer "Missing");
   - THIRD-PARTY.md;
   - DECIDE21.md defaults filled;
   - HANDOFF (the `handoff` skill, anchor `u3-n21`);
   - tasks.json;
   - `wave-memory harvest 21`, which scores the forge experiment.
6. **Tag `u3-n21`;** run `date`; print the close table, then `NIGHT21 CLOSED`.
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-35.1 | the json holds 6 contracts with bot hooks; tasks.json parses; the bench baseline ran or its CPU skip is logged |
| U3-35.2 | the N21 commit with its gates printed (fingerprint EQUAL N20, `--off19` EQUAL, golden 0 / 22 exact, `impact PASS`), tape format 6 with TapeTests green, the partials in place, builders launched with a `date` |
| U3-35.3 to U3-35.7 | `passes: true` with its evidence line from tonight, the off-test green, golden exact, its reel scenario run, its events in PORT-STATUS |
| U3-35.8 | the replay's check printed, or SHORT at its cap with the slice committed, off |
| U3-35.9 | each beyond row taken has a contract, a commit and its check's line |
| U3-35.10 | four rounds logged, each with its persona triage and fix list; the round-2 veto reviews and batch logged |
| U3-35.11 | the close's checks above printed, the suite's totals, the docs, `u3-n21` tagged |

A row is SHORT with its measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit (with
its `date`), or the time limit.
</definition_of_done>

<final_report>
`NIGHT21-REPORT.md`, one page for GG. Outcome first. Then each feature: what he can do, its switch, the contract's
number, the persona triage's line. Then what isn't verified, what felt worse, and the DECIDE21 questions. Every number
comes from a run this session.
</final_report>

<closing_reminder>
- **Wired, not just defined:** a feature passes on evidence from tonight's Player build or harness, never on a test
  alone or a builder's word. Check the Player's frames, not the Editor's.
- **Heavy jobs one at a time:** no bench, smoke or farm beside the suite, a build or another farm.
- **Three builders,** commits every slice, the lead-alone fallback. Every message ends with
  `N21-STATUS: <d> done, <s> short, <l> left (of 11) · round <r> · builders <alive>/3 · passes <n>/6 · <h:mm> / 6:00`.
</closing_reminder>

## The kickoff

Launch `claude --model claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, run `/effort high`, then paste:

```text
/goal Met when NIGHT21.md's close table is printed with every row U3-35.1 .. U3-35.11 DONE (its done-when evidence
from this session) or SHORT (a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit
with its date, or the time limit once a `date` reads 6:00 after the first), followed by "NIGHT21 CLOSED"; SHORT rows
count as met. Run NIGHT21.md in C:\Users\gaige\dev\SKYGRIND-unity: `date` first, read CLAUDE.md and NIGHT21.md, then
work it in order with at most 3 builders split by file ownership, committing every slice. Freeze at 4:30; print the
table at 5:00 or later from the first `date`; hard cap 6:00. Never ask me questions; never push, merge or commit to
master; the web repo is read only. On a usage limit keep building alone and retry agents at most every 30 minutes. End
every message with the N21-STATUS line.
```
