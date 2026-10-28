# NIGHT21-LOG: THE MOVES AND THE MOMENTS

Times are from `date` on 2026-10-07. Night zero (first `date`) = **08:20:33**. Close table no earlier than 13:20, hard stop 14:20.

## U3-35.1 Start

- 08:20 `date` read; NIGHT21.md read in full. Base: `port/stage3` at `700ec05` (`u3-n20` + 2 docs commits).
- 08:2x The Editor was **not running** (the terminal crash took it down; `Temp/UnityLockfile` was stale; no Unity process).
  `unity-loop.sh status` said "No Unity Editor instances found". Launched `Unity.exe -projectPath` (PID 50736) at ~08:30.
- 09:09 The Editor is open on the modal **"Recovering Scene Backups"** (Unity's post-crash prompt). The Pipeline server is not
  reachable until it is answered. Asked GG; GG answered "I'll click it myself". Nothing was clicked by the agent.
  About 45 minutes of the night went to waiting on the Editor (my 500 s poll loop outlived the dialog).
- CPU read 100 % at 08:3x and 09:09 (many `python` processes from other sessions). `Tools/night/bench.sh` skips above 30 %
  CPU, so the bench baseline x2 (`n21-base-1/2`) is **not run yet**; no bench reading is claimed.
- 09:16 `NIGHT21-FEATURES.json` written (6 contracts: arcline, slidebites, stamp, deadline, grind, replay; all
  `passes: false`, `evidence: null`); parses.
- 09:16 `tasks.json`: U3-35 and U3-35.1 .. U3-35.11 added (todo); parses.
- Not checked yet: HANDOFF vs NIGHT21 first-fix agreement beyond the first 30 lines; `Research/2026-10-07-wave21/RESEARCH.md`
  (not read); `docs/memory/THREADS.md`; DECIDE21 has no filled Answer column (all D-1..D-13 blank).

## U3-35.2 The shared commit (Core half), 09:33

- `42bdd3b`: N21Blocks (six bits), tape format 6, `--off21` / `-skygrindOff21`, `design.n21.<row>` in both config copies
  (all `enabled: false`), N21SharedTests (+ the AsFormat helpers of N19 / N20 / Stamina tests), SimHarness `n21shared` and the
  dispatch for arcline / slidebite / stamp / deadline / grind, feel-metrics M-MAGENTA with a synthetic self-test.
- Measured (headless, this session): `fingerprint` EQUAL (round 4, 4775 earned, 8 alive); `--off19` EQUAL to N18; golden 39 sets,
  0 mismatches, 22 exact; `impact PASS`; `n21shared PASS` (format 6 carries the byte, a format-5 tape replays EQUAL); feel-metrics
  `selftest: PASS`.
- **Not done in the shared commit (needs the Editor):** FeelReel phase-E partials, the empty test classes, the run log's counts;
  the EditMode compile and tests of the new files (the .meta files are hand-made).

## Builders launched, 09:36

- M (ARCLINE, SLIDE BITES), S (STAMP, DEADLINE), W (GRIND, then REPLAY) started in git worktrees from `ed6c3a4`, Core + SimHarness
  only (no Editor). Target was 0:55 from night zero (09:15); actual 1:15 (the Editor wait cost ~45 min).
- The lead's shared-commit leftovers (need the Editor): FeelReel phase E partials, empty test classes, run-log counts.

## Builder S integrated, 10:05

- S's report (30 min): `2f2ab42` magenta fix (UNCOMPILED: UIEffect's TMP-variant shader is found by `Shader.Find`, absent from the Player;
  UIEffect now installs in the Editor only, so the Player loses the shine / edge glow), `fa6b466` Grades21 + Deadline21 Core with
  `roundGrade`, `deadlineTick`, `deadlineBurn`, its tests NOT RUN, `e165b6b` RESULTS ROUNDS strip (UNCOMPILED). Cherry-picked as `71814d9` (tip).
- After integrating (headless): fingerprint EQUAL (round 4, 4775 earned, 8 alive); `--off19` EQUAL; `n21shared PASS`; golden 22 exact,
  0 mismatches; both config copies byte-identical.
- S's own probes: `stamp --seeds 2 --rounds 3` 6 grades (A 3, B 1, D 1, S 1), event once per round; `deadline` fuse 25 s, burn never kills.
  A round P was never observed (the autopilot does not reach SS in short runs).
- `passes` stays false for `stamp` and `deadline`: their checks need the Player build (M-MAGENTA on a reel, M9) and `chase --twist deadline`.
  The magenta cause is read from source, not confirmed in a Player.
- Still S's, not done: stamp entrance, T-11 card move, DEADLINE fuse bar (need hooks in Ui.cs and a visual pass: Editor).

## Builder W integrated, 10:11

- W (35 min): cherry-picked as `4e6a2dc` (Endless21, Records21, SurvivalSim.Grind21, presets, Endless21Tests, `grind`), `504ceb3` (`grind --kill`),
  `2a0a6ad` (TITLE MODE row, WAVE banner, RESULTS line: UNCOMPILED), `2531416` (INSTANT REPLAY Core ring + recorder, switch off). Tip `2531416`.
- Gates after integrating (headless): fingerprint EQUAL (round 4, 4775 earned, 8 alive); `--off19` EQUAL; `n21shared PASS`; golden 22 exact, 0 mismatches;
  config copies byte-identical; `grind --selftest` Endless21Tests 11/11 and Replay21Tests 7/7 via `Tools/SimHarness/NunitMini.cs` (a stand-in, not Unity's runner).
- W's probes: `grind --seeds 10 --seconds 300`: median wave 8 (min 5, max 9), core heights hash differs from the wave before on every wave (75 fights);
  the bot never died in 300 s, so death was checked with `--kill 90` (runEnd once, survival record untouched). `grind --replay`: replayed camera
  within 0.0000 m of the recorded path (limit 0.05) PASS, **0 by construction** (a pose per step; every 2nd step missed at 0.064 m), run hash on vs off EQUAL.
- W outside its files: MetaEvents.cs (`RunSetup.Grind`), UiMeta.cs, UiResults.cs. Lead lines still to apply (Editor needed): `SurvivalGame.cs:115` (RESULTS PB source for a GRIND run),
  `Ui.cs:476` (suppress the ROUND ceremony in the GRIND).
- Not done: the smoke's GRIND step, reel scenarios, PORT-STATUS endless row, the replay's Unity view, Records21 persistence, the DANGER rising-edge telegraph,
  tape replay of a GRIND run (the tape does not record the mode). Both rows still `enabled: false`; `passes` stays false (Player evidence).

## Builder M integrated, 10:36 (2:15 into the night)

- M (60 min): `0a1d3c5` ARCLINE, `0282e5a` SLIDE BITES, `4eafb40` ARCLINE look (UNCOMPILED, UNSEEN) cherry-picked on top of W's; tip `24aca6f`.
- Gates after integrating all three builders (headless): fingerprint EQUAL (round 4, 4775 earned, 8 alive), `--off19` and `--off21` EQUAL,
  `n21shared PASS`, golden 22 exact / 0 mismatches, config copies byte-identical.
- M's probes, re-run by the lead: `arcline PASS: attaches 10/10 (want >= 9), peak speed after a swing 30.7 m/s median (min 29.9; want >= 20), a yanked husk moved 8.6 m toward
  the player (want >= 3)`; `slidebite PASS: ... damaged 3 (want >= 2), 3 bites, CLOSE CALL x1`. The swing probe is a scripted dash into the hook (a plain hook topped out near 19 m/s).
- M set `design.n21.arcline` and `.slideBites` `enabled: true` in both copies (first playable slice). The fingerprint still keeps them off (N21OnPath false).
- M edited Mover.cs (`aimHook`, `zipAnchor`, zip lines, `GrappleTarget.zip`, `Rope.zip`; `MoverState.Skipped` lists the two fields) and Combat.cs ("slide" a Secondary source);
  golden is exact. `RunSnapshotTests` unrun after the Mover edit.
- All three builders are done. **Everything else needs the Editor:** one compile of ~20 uncompiled Game files, the EditMode suite, a Player build, the reels. The Editor has
  been on "Recovering Scene Backups" since 09:09 (1h30 so far).

## 10:47 The lead's headless work while the Editor is blocked

- `d710b81` `chase --twist deadline` (SandboxChase.Twist + Harness.Sandbox) and DEADLINE's retune. THE CHASE's KITE at round 10, one process each:
  normal median 71.5 s (5 seeds); DEADLINE with S's 25 s / 10 s fuse 43.2 s (60 %, over the contract's 50 %); 20 / 6: 37.7 s (53 %);
  **18 / 5, 10 seeds: 30.3 s of 63.2 s normal = 48 %** (PASS the survival half of the check). Rejected: S's numbers (over). The reel half of the
  contract is not run, so `passes` stays false. These were hand-run, one process at a time, on a CPU the log reads as 100 % from other sessions.
- PORT-STATUS: NIGHT21's events listed (payloads read from the code); endless no longer "Missing".
- Beyond 5 (challenge codes): contract added (`challengecodes`), builder W2 launched (Core + harness only).

## 10:56 W2 (challenge codes, beyond 5) integrated

- `a633dec` (cherry-picked): `Challenge21.cs` (12-char code: seed, HEAT, mutators, mode, target round; checksum; forgiving input), `design.n21.challenge` in both copies
  (byte-identical, enabled true), `Challenge21Tests` (10/10 through the NunitMini stand-in), `challenge` in the harness, one Compile line in SimHarness.csproj.
- Gates after: fingerprint EQUAL (round 4, 4775 earned, 8 alive), `n21shared PASS`, golden 22 exact / 0 mismatches, `challenge PASS` (200 round trips,
  corrupted codes rejected, decoded-code tape EQUAL to the raw-seed tape, format 6).
- Caveat from W2: the tape check zeroes `utcTicks`; `StartRun` has no visible reader of `setup.mutators`, so the check proves the seed + HEAT path, **not mutator effects**.
  The TITLE CODE field and the RESULTS code are not written. `passes` stays false.

## 11:00 Stamp distribution, and the push

- `stamp --seeds 8 --rounds 4` (one process, 8.8 s wall, autopilot): 32 round grades, **A 12, B 11, C 3, D 3, S 3** (no target; a round P needs grade S + peak SS + no revive,
  and the autopilot never reached SS in short runs, so no round P was seen). The event fires once per round PASS; the row off emits none PASS.
- Pushed a terminal notification asking GG to answer the Editor's "Recovering Scene Backups" dialog (Remote Control inactive, so no phone push).
  Rejected: dismissing the dialog by keyboard automation (GG answered "I'll click it myself").

## 13:20 The loop, honestly, and the close

**The Editor came back at 11:30** (GG answered the dialog; Unity recovered the scene backups into untracked `Assets/_Recovery/`).
Everything from the first Unity compile on happened between 11:30 and 13:20.

- **Compile:** one clean compile of ~20 never-compiled files (0 errors); the hand-made `.meta` files were accepted unchanged.
- **Suite 1 (11:43, 542 s): 1620 / 1626**, replays 22 / 22, golden 97 / 97. Reds and their causes (`fc1b27b`): Deadline21Tests (a fixed 20 s
  burn written for the 25 s fuse), Arcline21Tests (the husk 12 m toward -z can stand behind a wall; now an open line as the harness does),
  SandboxHarnessTests.TheChase_IsDeterministic (pins `--off19`, which now implies `--off21`), TapeTests.OnScreenAnswers (the fingerprint
  script's watchdog asks onScreen 12 times with N21 off and **0 with N21 on**: `tape-onscreen 1 8`), and two MapPipelineTests (Editor
  `/api/exec` 30 s timeouts; 19 / 19 alone). No test was weakened; the premises changed.
- **Build 1 (12:24):** `Succeeded`, 42.8 s, 0 errors; smoke `199.5s DONE: all steps passed`; reels on (4726 frames, 8 failed items) and pad
  (4845 frames, 9 failed items; the failed items were not triaged). **M-MAGENTA 0 px on every frame, both passes**; the same
  frame in NIGHT20's reel (`Logs/feel/final20/on/hud-fight/f0060.jpg`) reads 9487 px, tonight's 0. M9 pop-in FAIL rows remain (T-12,
  not counted); no S12 FAIL on the pad pass.
- **Wiring (lead):** the ARCLINE view constructed and rendered in SurvivalGame; no ROUND banner in THE GRIND. Not applied:
  `SurvivalGame.cs:115` (the RESULTS PB source for a GRIND run).
- **Freeze 12:47:** `N21OnPath` true; `FingerprintBaselineN21` recorded once (`1d2500a`): round 3, 35 kills, 80 shots, 4808 earned, 1 alive;
  two processes identical (md5), `twice` EQUAL; `--off21` EQUAL to `FingerprintBaselineN20` (kept for good), `--off19` EQUAL to N18.
- **Final build `Builds/n21-final` (12:48):** `Succeeded`, 26.6 s. `close.sh`: smoke FAILED 3 pad steps (226 s; `virtual pad`, `virtual pad menus`,
  `pad only`); golden PASS (0 mismatches on every set printed); fingerprint **three lines EQUAL**; replay EQUAL (1 of 1 checkpoints, 488 steps);
  **bench x2 SKIPPED (CPU 100 % > 30 %)**. Smoke run again alone at 12:55: `200.1s DONE: all steps passed`.
- **Suite 2 (12:59, 876 s), frozen code: 1625 / 1626**, replays 22 / 22, golden 97 / 97. The red: `AutopilotTests.PerStep_AllocatesNothing` (1
  allocation, under load); 8 / 8 passed twice alone.
- **Persona triage (one fresh agent, files only, 13:0x):** no human play and no N21 reel scenario existed, so it judged the code. Ranked asks:
  (1) prove the magenta fix in a Player [done above: M-MAGENTA 0]; (2) the ARCLINE's button and tutorial text, and whether the pad press collides
  with RB [unchecked]; (3) a cooldown arc on the chip and a distinct reticle state (a red reticle means danger only); (4) the DEADLINE fuse bar
  (16 px+, DANGER, last-3-seconds pulse); (5) the GRIND's two lead lines, persistence, the smoke step; (6) the challenge-code UI;
  (7) the stamp entrance + T-11; (8) a SLIDE BITE tag and a crunch beat. Keep / iterate / cut: all kept; REPLAY stays off. It also asks GG whether
  the swing's 0.4 s cooldown is a "movement verb" under his long-cooldown rule.
- **Not run:** the round-2 veto reviewers and the blind passes (no N21 reel scenario to show them), THE MARATHON, the free farm, the bench,
  the reel scenarios arcline / slidebite / stamp / deadline / grind / replay, the FeelReel phase-E partials, a smoke GRIND step.
- **Caps used:** Player builds 2 of 7; reels 2 of 14; farm batches 0 of 4; bench runs 0 of 4; agents: 4 builders (M, S, W, W2) and 1 persona triage.

## Close table (U3-35.1 .. U3-35.11)

| Row | State | Measured reason |
| --- | --- | --- |
| U3-35.1 start | DONE | 7 contracts with bot hooks, tasks.json parses; the bench baseline did not run: CPU 100 % > 30 %, logged |
| U3-35.2 shared commit | SHORT | gates, N21Blocks, tape format 6 (TapeTests 13 / 13), harness partials and builders done; **the FeelReel phase-E partials and the empty test classes were never placed** (no Editor until 11:30, then the time went to the suite) |
| U3-35.3 ARCLINE | SHORT | harness PASS, 5 / 5 tests, view compiled and wired, S12 clean; **no reel scenario, never seen**; no red reticle / chip |
| U3-35.4 SLIDE BITES | DONE (late, 13:58) | harness PASS, 3 / 3 tests, Player reel `slide bites 3`, off-test; see the second addendum (it was SHORT at the first close) |
| U3-35.5 THE STAMP | SHORT | **magenta fixed in the Player (M-MAGENTA 0)**, 7 / 7 tests, round grades; the stamp entrance, T-11, the `stamp` reel (M9) not done |
| U3-35.6 DEADLINE | SHORT | 7 / 7 tests, KITE 48 % (contract met); **no HUD fuse bar, no reel** |
| U3-35.7 THE GRIND | SHORT | 11 / 11 tests, median wave 8, TITLE / banner / RESULTS compile; **no smoke step, no reel**, not persisted |
| U3-35.8 INSTANT REPLAY | SHORT | Core ring + recorder, 7 / 7 tests; **no view**, off; the path check is 0 by construction |
| U3-35.9 beyond | DONE | one row taken (challenge codes, beyond 5): contract, commit `a633dec`, `challenge PASS`; the other four not taken |
| U3-35.10 loop | SHORT | one triage and no veto reviewers, no blind rounds, no MARATHON / farm: nothing N21-specific was on a reel to review |
| U3-35.11 close | DONE | the close checks printed (bench skipped, logged), the suites, the docs, the tag `u3-n21` |

**0 of 7 contract `passes` flipped at 13:26; 1 of 7 after the 13:58 addendum (slidebites).** 3 DONE, 8 SHORT at the first close; 4 DONE, 7 SHORT now.

## 13:50 Late addendum: phase E of the reel (after the tag), 2 scenarios

Written after the first close because the stop hook asked for the reel half (`Game/FeelReel.N21.cs`, `-skygrindReelOnly slidebite,arcline`; builds 3-5, 3 of 7).
- **arcline, on the Player (build 5, keyboard; build 3, pad):** `239 frames (arclinePull 1, ropeAttach 1)`: the yank fired on a husk 12 m ahead, then a second
  press inside the 15 s cooldown attached the rope to the floor. M-MAGENTA 0 px on 239 frames both passes; **S12 0 on the pad pass**; M10 / M11 PASS; M9 one
  pop-in entrance (the T-12 class). **Not met literally:** the buy is scripted (`Own(true)`; the buy is Arcline21Tests'), there is no horde, no human saw the frames.
  `passes` stays false.
- **slidebite, on the Player:** `slide bites 0` in three tries (husks 7-11 m, then 3.5-6.5 m; sprint + slide, then the harness's exact input). Telemetry shows the
  scripted player bouncing in place at the spawn spot (camera z -28 -> -27.5, `vel.up` ~ +17 m/s), so this is the scenario's placement, not proof about the feature
  (the headless `slidebite PASS` stands). Not fixed; no pass.
- DEADLINE, THE GRIND, INSTANT REPLAY still have no scenario. The close table above is unchanged (3 DONE, 8 SHORT, 0 / 7 passes); U3-35.3 is nearer: its reel half now exists.
- Tag `u3-n21` moved (locally, not pushed) to include this commit.

## 13:58 Second late addendum: SLIDE BITES passes

The slidebite reel read 0 bites because the open spot sits beside the jump pad at (0, -26.5) (`pads[0]` in survival.json): running forward steps on it and
bounces. Starting from (0, -8) instead (`FeelReel.N21.cs`): **`slide bites 3`** on the Player (build 6, both passes). Contract `slidebites` now met: SlideBite21Tests
3/3, `slidebite PASS`, the reel, the off-test. **`passes` true for `slidebites`: 1 of 7.** U3-35.4 DONE. Close table now **4 DONE, 7 SHORT**.
Not part of the contract: M11 flags a red `Buy prompt/Hold` at the new spot (a buy station), the same row-class as the old ui-results / vitals fails.

## Blocked (resolved 11:30)

- Everything that needs the Editor (compile, tests, builds, Play checks, harness builds that touch the Unity project) waits
  for the scene-backup dialog to be answered.
