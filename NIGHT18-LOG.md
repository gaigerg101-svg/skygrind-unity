# NIGHT18-LOG: feel in motion (U3-31.1 .. U3-31.10)

First `date`: **Tue Oct 6 08:22:08 USMST 2026**. The close starts no earlier than 13:12:08 (4:50); the table prints no
earlier than 13:52:08 (5:30); the hard cap is 14:22:08 (6:00).
GG's switches as written in NIGHT18.md (GG didn't edit them): **AUDIO OFF, KILL MARKER ON, CHUNKS LINGER OFF, CAMERA FX
100 %**.

## Status lines (one per item)

| Item | Status | Line |
| --- | --- | --- |
| U3-31.1 Start | DONE 08:30 | RECEIVED rows logged (below); tasks.json parses (159 items, 11 U3-31.*); `Builds/n17-before/` exists (173 MB, NIGHT17's build 6) |
| U3-31.2 The harness and the baseline | DONE 09:40 | see the close table |
| U3-31.3 The kill at every range | SHORT (the time limit) | M1-M4 PASS, 8 of 8 picks; fa's "too much" on horde5 (both sides) after the freeze |
| U3-31.4 The hit that lands | DONE 13:59 | M5 12.6 px PASS; 2 of 2 picks |
| U3-31.5 Movement you can see | DONE 13:59 | M6-M8 PASS; 7 of 10 picks |
| U3-31.6 HUD motion and the kill marker | SHORT (the time limit) | M9 / M10 / M11 / M2 marker PASS; 5 of 8 picks (target 6) |
| U3-31.7 Menus motion and words | SHORT (the time limit) | 8 of 10 picks, M10 / M11 0, the smoke's menus green; M9 2 menu pop-ins |
| U3-31.8 The rounds | SHORT (a cap) | three rounds (builds 7 / 7, Editor reels 6 / 6) |
| U3-31.9 The backlog | DONE 11:57 | six items taken, the backlog table |
| U3-31.10 Close | DONE 14:0x | the close table |

## The verification budget (75 min of lead time: suites and the headless gates only)

| Check | When | Lead minutes | Running total |
| --- | --- | --- | --- |
| Arrival: CoreHarness, SimHarness fingerprint + impact, the suite (async) and its read | 08:23-08:33 | 3 | 3 |
| The shared commit's fingerprint | 08:37 | 1 | 4 |
| FeelProbeTests | 08:58 | 1 | 5 |
| Integration 1: seen-red / green filters (HitSpring, CameraMove, HudMotion, KillLadder, ViewmodelN18) | 09:03-09:31 | 4 | 9 |
| Integration 1's gates (CoreHarness, fingerprint, impact; X touched Core/ImpactRules.cs) | 09:24 | 2 | 11 |
| Integration 2: filters (KillLadder, MenuMotion18) + the whole suite (async) and its read | 09:47-10:00 | 3 | 14 |
| Integrations 3-5: filters (HudMotion, KillLadder, MenuMotion18, Crunch, CameraMove, ViewmodelN18, HitSpark, HitSpring) + fingerprint | 10:02-10:31 | 5 | 19 |
| Integrations 6-7: filters (CameraMove, KillLadder, MenuMotion18, Menus, Crunch) + fingerprint | 10:50-11:21 | 3 | 22 |
| The whole suite (async) on `3bdd99d`, its one failure re-run alone | 11:21-11:30 | 3 | 25 |
| Integrations 8-10: filters (MenuMotion18, Menus, HudMotion, CameraMove, ViewmodelN18, KillLadder, Crunch) + fingerprints | 11:35-12:31 | 5 | 30 |
| The final code: the whole suite (async), CoreHarness, fingerprint, impact | 12:39-12:48 | 3 | 33 |

## RECEIVED (U3-31.1, 08:22-)

**GG's notes:** no `GG-NOTES.md` in the repo root; GG's start message is the /goal line only, with no playtest notes.
`FEEL-PLAYTEST.md` is NIGHT17's guide (last changed `ceacb7b`, 02:54), not notes from GG. So the plan's priorities stand
as written.

HANDOFF.md section 1, row by row:

| # | Row | Result |
| --- | --- | --- |
| 1 | HEAD, tag | `154a7a8` on `port/stage3`, `u3-n17-2-g154a7a8` (two docs commits after the tag: the retro + research, the FeelAB note) |
| 2 | `git status --short` | as expected: ` M ProjectSettings/UnityConnectSettings.asset`, `?? Assets/_Recovery*`, `?? NIGHT7.md` .. `NIGHT19.md`, `?? QUEUE-*.md` |
| 3 | Editor status, auto-tick | `ready`, `playMode: stopped`, Unity 6000.3.7f1; `set_autotick`: `Auto-tick already enabled (interval updated to 16ms)` |
| 4 | The suite (async) | started 08:24 (`run_tests ... async`; `test_status`: `running`); result below when polled |
| 5 | CoreHarness | `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| 6 | SimHarness | `fingerprint seed 1: 12 hashes in 0.83 s: EQUAL to the baseline`; `impact PASS: the budget holds on 2 map(s)` |
| 7 | The Player smoke | not run on arrival (`Builds/Stage1` is NIGHT17's build 6, unchanged; the first smoke is build 2's) |
| 8 | Power plan, AC, CPU / GPU (08:23) | **Balanced**, AC (battery status 2, 100 %); **CPU 100 %** (two samples 100 / 99.97 %): GG's `patric2` pytest-xdist workers and `run_batch.py --serve-only` (about 60 python processes, `C:\Users\gaige\Desktop\patric2`), plus a Godot 4.7.2 process; not ours, left alone. GPU 20 % / 644 MiB. Disk C: 348 GB free |

**What the machine means for tonight:** with the CPU pinned by another project, builds, reels and the smoke run slower
than NIGHT17's, and a bench's frame times carry that load. M12 compares two benches taken under the same conditions
(both logged with the CPU load beside them).

- `Builds/n17-before/` = a copy of `Builds/Stage1/` (NIGHT17's build 6, stamped `a105aaf`), for GG's A/B. 173 MB.
- tasks.json: U3-31 and U3-31.1 .. U3-31.10 inserted after U3-30.11 (a text insertion, the layout kept); it parses
  (159 items, 11 of them U3-31.*).
- The suite on arrival (started 08:24, read 08:32): `tests: 969 / 969 passed, 0 failed, 0 skipped (485.92 s)`,
  `replays: 22 / 22`, `golden tests: 96 / 96`.

## The shared commit (U3-31.2, 08:30-08:40)

- `35bf441`: EnemyLook split as a pure move. EnemyViews.cs keeps the rest and the per-type views (X's); the hit members
  went to EnemyViews.Hit.cs (R's), the death members to EnemyViews.Death.cs (X's). The hit timers' loop and Clear's hit
  lines were extracted unchanged as `FrameHits` / `ClearHits`. The proof: the sorted non-blank lines of the old file
  against the three new files differ only by the added wrappers, headers and the two calls.
- `e9fe051`: FeelAB (on by default, `-skygrindFeelAB off`, F6 and the LAB row FEEL N18 in a developer build, the corner
  chip, never saved), `UnityTuning.Pick`, FeelClock (the reel's fixed 1/60 frame clock, routed through the loop's
  frameDt, FlashGate.RealNow, UiPress, the quit-confirm window and the wheel's clock; AUTO quality held; no attract
  inside a scenario), the FeelProbe API, the FOV KICK / CAMERA SHAKE keys, and the fixtures.
- `7094cd9`: Tools/feel-schema.md + a hit12 sample; tasks.json; this log.
- `e330914`: the CAMERA FX composition point (`CameraFeel.FovKickScale` / `ShakeScale`; the impact layer's FOV kick
  goes through it).
- Gates: Editor compile clean (08:33, 08:39); SimHarness `fingerprint seed 1: 12 hashes in 0.82 s: EQUAL to the
  baseline`.

## Builders (launched 08:42:28, `date`; 0:20)

| Builder | Item | Owns (the brief: scratchpad `builders-n18.md` + its own) | First report due |
| --- | --- | --- | --- |
| X | U3-31.3 the kill (M1, M2 effects, M3, M4) | Chunks, KillConfirm, ImpactFx, PostFx, EnemyViews.cs + .Death.cs, UnityTuning.Crunch, ImpactFrames, design.impact (+ n17), Core/ImpactRules (M4 only) | ~09:55 |
| R | U3-31.4 the hit (M5) | EnemyViews.Hit.cs, HitSpring.cs, UnityTuning.HitReact.cs, HitSpringTests | ~09:40 |
| M | U3-31.5 movement (M6-M8) | CameraFeel, MovementFx, UiFeel, Viewmodel*, the stamina bar files, UnityTuning.CameraMove.cs | ~09:55 |
| H | U3-31.6 HUD motion + the kill marker (M9-M11 HUD, M2 marker) | Ui.cs (HUD), UiHudMotion, UiPointsPops, UiStyleMeter, UiCeremony, UiHotbar, UiFeelHud, UiPowerUps, UiPerks, UiMoveRow, UiMarkers, UiTween (additive), UnityTuning.Hud17 / FeelHud | ~09:55 |
| N | U3-31.2 feel-metrics.py first, then U3-31.7 menus | Tools/feel-metrics.py, feel-roles.json; then UiMenus, UiKit, UiMeta, UiResults, UiSettings, GameSettings, UiMods, UiOnboarding, UiNav, UnityTuning.Menus17 | ~09:25 (the tool) |

## The harness (U3-31.2, 08:42-09:00)

- **Editor reel 1** (Play check 1 of 6, Editor reels 1 of 6; 08:54, `Logs/feel/e1/on`, 2560 x 1440: the Editor's game
  view): hit12 55 frames, kill12 91, boost 72, ui-settings 114; `done: 332 frames written, 0 failed, 21 s wall`.
  - **The clock proof (Editor):** kill12 `steps per non-held frame: {2: 87, 1: 1}`, held frames 3, heldSteps 7 (the
    1 is the kill's frame, cut by the freeze); boost `{2: 72}`; hit12 `{2: 55}`.
  - What it showed: the gun's recoil kick moves the whole view, so the victim's raw screen rect moved about 25 px (at
    1080 p) on a hit, mostly from the camera. The probe now logs each husk's **world anchor** (its sim position + 0.9 m,
    projected), and M5 is the rect's motion against it (schema updated, N told).
- `0a505ab` the recorder + FeelReel (phase A and B). FeelProbeTests `3/3 passed`.

## Build 1, the baseline (09:00-09:04)

- **Build 1** (08:59:23 queued; `Succeeded`, 51 s, 0 errors; code `0a505ab`): ProjectSettings: only
  UnityConnectSettings; the four theme SDF font assets rewritten by the build, restored with `git checkout --`.
- **Its reel** (`Logs/feel/b1/on`, 1920 x 1080, FeelAB on = NIGHT17's look, 09:01:58-09:04:01, 108 s):
  `done: 2329 frames written, 0 failed, 90 s wall`; 22 of 23 scenarios. Two setup misses, fixed in FeelReel for build 2:
  - kill20: `SKIPPED: no open floor at 20 m` from the ARENA spawn (now: the player moves to open floor nearby, off the
    record);
  - horde5: `1 of 5 killed in one blast` (the THUNDERCLAP's 8 deg cone; the front bodies block the pellets). Now the
    COMET (4 m splash) on a pack within 1 m of its middle, 7 m out.
- **The bench baseline, run 1** (`-skygrindBench 60`, 24 husks; 09:05:53-09:07:26): `median 5.92 ms, p95 13.18 ms,
  worst 1% 17.66 ms`, `p99 17.66 ms, p99.9 27.85 ms`; plan Balanced, AC; CPU 100 % before and after (another
  project's pytest workers).

## First passes in (09:00-09:07)

| Builder | Commits | Its claim (not yet measured) |
| --- | --- | --- |
| R | `1c9b268` (tests first: seen red in the Editor, `0/13 passed, 13 failed`), `e6a0f09`, `a16bf03` (tests first), `d269773` | a sideways kick + lean + squash on a damped spring (k 1500, c 36), sized by distance: ~12-15 px at 1080 p from 3 to 20 m, back by 200 ms; ReactN17 untouched |
| M | `53041f7` (tests first), `3e4fbfb`, `551da9b`, `1f0b719`, `f527289` | boost +10 deg back by ~0.31 s; sprint 5.5 deg; slide +7 deg, roll ~4 deg; strafe roll 2.4 deg; land dip 0.137 m; speed lines <= 0.28 alpha under 70 % of the boost's speed; the stamina bar 240 x 14 px |
| H | `21a0cf2` (tests first), `8a38c8d`, `2fd56a7`, `b8723f2` | every HUD entrance from 0.6 x with ~10 % overshoot + 60 ms fade; the kill marker (white wedges, 52 px, a 1.22 x pop, gone at 360 ms); ROUND N CLEAR never static |

H was named five more HUD-side files (UiFeelEdges, UiDamageNumbers, UiMomentum, UiPayoff, UiWeapons). M moved to
backlog 1, 2 and 6. R is parked until build 2's measured M5.

- **The bench baseline, run 2** (09:07:35-09:09): `median 7.59 ms, p95 23.12 ms, worst 1% 34.32 ms`, `p99 34.32 ms`;
  Balanced, AC, CPU 100 % before. The two baseline runs differ 2x in p99 (17.66 vs 34.32 ms) with the CPU pinned by
  another project: M12 compares like with like only if the final's runs see the same load (logged with them).

## Build 1's metrics and the three eye checks (U3-31.2, 09:24-09:36)

`Logs/feel/b1/FEEL-METRICS.md` (`35 PASS, 62 FAIL, 20 n/a, 4 info (of 121 rows)`). The baseline rows that matter:

| Metric | Build 1 (NIGHT17's look) | Target |
| --- | --- | --- |
| M1 kill12 / hit12 | 1.97 (31302 / 15881 px) | >= 3.0 |
| M2 / M3 | n/a (no effect reported the probe in NIGHT17's code) | |
| M4 hit / kill / headshot / punch / horde | 0 / 7 / 8 / 6 / n/a (1 of 5 died) | 0-2 / 6-8 / 10-13 / 11-14 / >= 18 |
| M5 hit12 | centre 2.7 px, top 1.9 px (against the anchor); flash 6 frames | >= 8 px within 50 ms |
| M6 boost | +12.6 deg by frame 2, peak +16.8 at f31; back 250 ms | +10 +/- 2, back 300-400 ms |
| M6 sprint / slide FOV / drop / roll | +16.8 deg / -6.5 deg / 0.80 m / 4.5 deg | +5-6 / +6-8 / >= 0.4 / 3-5 |
| M7 strafe roll / land dip / back | 1.4 deg / 0.133 m / 167 ms | 1.5-3 / 0.05-0.15 / 150-200 ms |
| M8 | n/a (the lines weren't reported) | |
| M9 pop-ins | e.g. ui-settings 12, roundend, hud-fight 23 of 29, kill12 / hit12 HUD elements | 0 |
| M10 | 9 texts at 16 px on ui-results | 0 under 18 |
| M11 | every role user unlisted (feel-roles.json empty until filled) | |

**The eye checks (each: the telemetry value beside the value read from the frames):**
1. **The boost's FOV** (`fovcheck.py`: the scale that best maps f17 onto f18 about the centre, the far band): telemetry
   74.00 -> 85.00 deg (picture scale 0.822); from the frames 0.840, i.e. **83.8 deg**. Agree within 1.2 deg; the
   residual is the 0.4 m the dash moved the camera forward in those two steps. (f17 -> f30 read 84.9 against 90.6: by
   then the camera has moved 4.8 m, so that pair can't be read this way.)
2. **The kill's M1 window** (`m1check.py`: the pre-frame, frame 1 and the delta > 40 mask, HUD rects masked): kill12
   **31481 px** by hand against the tool's 31302; hit12 **15881** against 15881. Agree. It also showed a reel bug: in
   the Player the KESTREL fired about every 0.4 s in every scenario from a real device (the GAMEPAD tab is shown, so the
   Player sees a pad), and kill12's target died on f10, before the input frame. Fixed in `8d8f29e` (the reel drives idle
   input); build 3 is the first clean reel.
3. **One UI element's first frame** (`uicheck.py`, ui-settings): `Settings/Head` telemetry f18 scale 1.00, alpha 1.00:
   the crop shows f17 = TITLE, f18 = SETTINGS whole (a pop-in); `Row hudScale` telemetry f18 alpha 0.43, x 377 (rest
   410): the crop shows the row see-through and about 33 px left. Agree.

**Measurement decisions (logged, builder N's tool):** a pop-in is a first frame at 95-105 % of rest scale (a slam from
above is motion, not a pop-in; `POP_SCALE_MAX = inf` restores the literal rule); M4 counts a frame cut by a freeze
(heldSteps > 0, held false); M9's entrance time ends when an element stays at rest for 6 frames; M8 reads the lines'
clear box (FeelFx.SpeedLinesClear), since the lines' union covers the frame by construction.

## Builds 2 and 3 (09:23-09:36)

- **Build 2** (09:23:54, `Succeeded`, 58 s; R, M pass 1, H pass 1 + 2 part, X pass 1, N's tool). Its gates
  (09:24): CoreHarness `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`
  (X changed Core/ImpactRules.cs: the freeze table as a prefix, no rule changed); SimHarness `EQUAL to the baseline`;
  `impact PASS: the budget holds on 2 map(s)` (worst rolling second 192.0 ms of 200).
  - Its reels: off `2705 frames written, 0 failed`; on `2554 frames written, 0 failed`. Both carry the device's stray
    shots, so they are not round 1's (the spare build was spent on the harness fix, as the plan allows).
  - **Its smoke** (09:30:13-09:33:59; builders working): `[smoke] 193.6s DONE: all steps passed`.
- Tests seen red then green in the Editor at integration: HitSpring 0/13 -> 15/15; CameraMove 0/12 -> 12/12 ->
  (12/14 red) -> 14/14; HudMotion 0/10 -> 10/10 -> (10/13) -> 13/13; KillLadder 0/8 -> 8/8; ViewmodelN18 0/5 -> 6/6.
- **Build 3** (09:34:30, `Succeeded`, 53 s): + M pass 2 (backlog 1 the flourish, 2 the QUICKDRAW glint, 6 recoil, the
  dash tint off DANGER, the clear box, H's ATTACHED hook), the idle-input fix. Round 1's build.
- **Slip:** a two-line code patch by `node -e` (09:31, FeelFx.Gun; CLAUDE.md rule 9 wants a written .cjs).

## Round 1 (build 3, 09:35-)

- **The reels:** off (09:35:59-09:37:35) `2399 frames written, 0 failed`; on (09:37:35-09:39:30) `2396 frames written,
  0 failed`. kill20 at 20 m (the player moved off the record); horde5 `5 of 5 killed in one blast` on both sides.
  The input frames line up: every kill / hit scenario's `shot` is on its input frame (f18).
- **The measurement rules, fixed once at round 1** (NIGHT18.md allows one retune of the inferred targets, logged;
  `f8c9bcf`): M9's rest is an element's brightest 10-frame window (a fading points pop's exit read as
  its rest: "first frame 0.85 / 0.62, rest 0.85 / 0.62"); M6 sprint is the held widen 500-1000 ms after the press (the
  sprint is the boost key held: its first 300-400 ms are the boost's kick, which M6 boost measures); M8's region reads
  the lines' reported clear box. M9 judges the HUD scenarios (rankup, roundend, pickup, hud-fight) and the ui-* ones,
  as NIGHT18.md's M9 row says; the kill / hit / camera scenarios' M9 rows are information.
- **Metrics** (`Logs/feel/b3/FEEL-METRICS.md`, FeelAB on vs build 1 and vs the same build's off side):
  **on 97 PASS, 23 FAIL** (of 125 rows); **off 60 PASS, 58 FAIL**.

  | Metric | On | Off (NIGHT17's look, same build) | Target | |
  | --- | --- | --- | --- | --- |
  | M1 kill12 / hit12 (b1) | 2.60 (41275 px) | 1.77 | >= 3.0 | FAIL |
  | M2 effects kill6 / 12 / 20 | 39.5 / 23.9 / 15.1 % of h | 37.5 / 20.2 / 20.3 % | >= 8 % at 20 m | PASS |
  | M2 marker | frame 1, 61 px, 367 ms | frame 1, 89 px, 317 ms (NIGHT17's marker reports too) | f1-2, >= 48 px, 300-400 ms | PASS |
  | M3 kill12 centroid / outside | 2.03 body h / 100 % | 0.73 / 78 % | >= 1.5 / >= 60 % | PASS |
  | M3 horde5 centroid / outside | 1.22 / 78 % | 1.05 / 78 % | >= 1.5 / >= 60 % | FAIL / PASS |
  | M4 hit / kill / headshot / punch / horde5 | 0 / 7 / 11 / 13 / 18 | 0 / 7 / 9 / 11 / 18 | 0-2 / 6-8 / 10-13 / 11-14 / >= 18 | PASS |
  | M5 hit12 | centre 12.6 px, top 5.4 px; back 67 ms; flash 3 frames | 2.7 px | >= 8 px in 50 ms, back in 200 | PASS |
  | M6 boost | +10.0 by frame 2, peak +10.0; back 300 ms | +12.6, peak +16.8; 250 ms | +10 +/- 2; 300-400 ms | PASS |
  | M6 sprint (held) | +5.5 | +6.4 | +5-6 | PASS |
  | M6 slide FOV / drop / roll | +7.3 / 0.80 m / 4.0 deg | +4.4 / 0.80 / 4.5 | +6-8 / >= 0.4 / 3-5 | PASS |
  | M7 strafe / land dip / back | 2.3 deg / 0.136 m / 150 ms | 1.4 / 0.133 / 167 | 1.5-3 / 0.05-0.15 / 150-200 | PASS |
  | M8 alpha / region (sprint, boost) | 0 frames / 0 frames | 2 frames / 18 of 18 | | PASS |
  | M9 HUD pop-ins rankup / roundend / pickup / hud-fight | 0 / 3 / 0 / 0 | 1 / 1 / 1 / 15 | 0 | FAIL (roundend) |
  | M9 menus ui-title / settings / paused / results / mods | 0 / 12 / 3 / 1 / 0 | 0 / 12 / 3 / 1 / 0 | 0 | FAIL (N's pass is in build 4) |
  | M10 | 0 under 18 px on every scenario | ui-results 9 at 16 px | 0 | PASS |
  | M11 | 0 unlisted | the dash tint, NIGHT17's kill marker | 0 | PASS |

- **Fix lists sent (09:46-09:47):** X: M1 (frame 1's change is a soft white bloom over a pale band: make it hard-edged
  and high-contrast in the window), M3 horde5 (the shared cloud's lateral / up throw), and keep kill12's chunks in view
  ~400 ms (they leave the 480 px crop by +250 ms). H: roundend's `Marker 0-2` whole on f17; entrances under 90 ms
  (`Round clear band` 33, `Rank-up streak` 33, the kill marker 83); horde5's `Tag`; the power-up edge; the low-HP
  breath on X's pulse (`PostFx.LowHpPulseNow`, behind FeelAB).
- **Integrated for build 4 (09:47-09:48):** X `7a4c5c6` (tests first: KillLadder `8/10`, 2 red) `ce32e9d` (backlog 3:
  the spawn ring under the bloom threshold, red-orange held) `efa0653` (backlog 4: the low-HP vignette pulse); N
  `81f86c4` (tests first: MenuMotion18 `2/11`, 9 red) `dfe7977` `2406838` `0795493` (menus enter, focus slides, key
  chips, FOV KICK / CAMERA SHAKE up front), `2246bca` (the tool). Then KillLadder `10/10`, MenuMotion18 `11/11`.
- The whole suite started async at 09:48 on the integrated code.

- **The suite on the integrated code** (09:48-10:00, before H's / X's round-1 fixes): `tests: 1041 / 1041 passed,
  0 failed, 0 skipped (592.5 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`.

### Round 1's blind reviewers (r1a 09:44-10:05; r1b, every side swapped, 09:44-10:06; reviewers 2 of 10)

Decoded with `Logs/feel/b3/key-r1a.json` / `key-r1b.json` (N18 = FeelAB on, N17 = off):

| Scenario | r1a pick | r1b pick | "Too much?" |
| --- | --- | --- | --- |
| hit12 | N18 ("the body tilts back and recovers") | N17, slight ("a small orange chip lands at the hit point") | neither |
| kill12 | N17 | N17 | N18 (both: "the puff / bloom hides the body, 1-2") |
| kill20 | N17 | N17 | N18 (both) |
| headshot12 | N17 | N17 | N18 (both) |
| horde5 | N18 | N17 | both (r1a) / N18 (r1b) |
| kill6 | N17 | N17 | N18 (both) |
| punchkill | Same | N18 | both (r1a) |
| boost | N17 ("dense lines held to 250 ms; N18's thin, gone by +250") | N17 ("full-frame lines, a 5-15 % wider FOV") | N17 (r1b: lines over the centre, the red edge) |
| sprint | N17 | N17 | N17 (both) |
| boostside | N17 | N17 | N17 (r1b) |
| slide | N18 ("the gun drops low, with streaks") | Unknown (the two runs started in different places) | neither |
| land | Same | Same ("the dip identical") | neither |
| strafe | N18 | N18, slight | neither |
| swapall | N18 | N18 | neither |
| rankup | N18 ("a tiny C that grows to size") | N18 | neither |
| roundend | N18 | N18, slight ("ghosted, then solid: it enters") | neither |
| pickup | Unknown (the banner only on tile 8) | Unknown | neither |
| hud-fight | N18 ("the D panel fades in small") | N18 | neither |
| ui-title / settings / paused / results / mods | Same x 5 | Same x 5 (N's pass wasn't in build 3) | neither |

Per item: **U3-31.3** 1 of 8 (target 6); **U3-31.4** 1 of 2 (target 2); **U3-31.5** 4 of 10 (target 7; boost and
sprint 0 of 4, strafe 2, slide 1); **U3-31.6** 6 of 8 (target 6; pickup Unknown x 2 from the framing); **U3-31.7** 0 of
10 (the pass wasn't in the build). The rubric for N18 (r1a / r1b): kill crunch 3 / 3, hit 3 / 2, weapon 3 / 3,
movement 3 / 3, score 3 / 3, style 3 / 3, HUD 3 / 3, menus 4 / 4, identity 4 / 4, motion 3 / 3: **32 / 31 of 50**
(NIGHT17's four fresh reviewers: 25, 23, 27, 27).

**Fix lists (10:06-10:08), each quoting the reviewers:** X: no soft bloom (a crisp silhouette, hard streaks outside
it), the chunks burst around the target and stay in the crop; M: denser / brighter edge lines through the dash, the
kick at the top of +10 +/- 2, strong sprint lines allowed (M8's max speed back to the slide's floor 11.6 m/s), a landing
that reads; R: a crisp impact at the hit point; H: the kill marker readable over a white burst (an INK stroke), a bolder
hit marker, roundend's "+96" over "+100 ROUND". The reel's framing: pickup's input at the take, land's at the landing
looking 20 deg down (`e1dd865`); the setup on the fixed clock (`0f6a527`: the slide's sides had started apart).

**Editor reel 2** (Play check 2 of 6, Editor reels 2 of 6; 10:10-10:11; `Logs/feel/e2/on`, 2560 x 1440; X's round-1
disc and N's menus integrated): kill12 M1 **3.67** (58319 px; PASS), M2 37.9 %, M3 1.88 body h; hit12 M5 12.6 px; M9
pop-ins **0** on all five ui-* scenarios (build 3: settings 12, paused 3, results 1); M10 0 on ui-results (was 9).

**An API spend limit (10:12):** builders M, R, H and N stopped mid-task ("You've hit your monthly spend limit", HTTP 429).
X had finished (`db149e0`), H had committed `89efaa6`, N `d665c6c`: integrated (KillLadder 13/13, HudMotion 15/15,
MenuMotion18 11/11, Crunch 15/15). A fresh probe agent then ran (10:14), so M and R were resumed to finish their
in-progress fixes (M at its tests-first stubs, R mid-HitSpark).

## Builds 4 and 5: two reels spoiled by the input path (10:31-10:58)

- **Build 4** (10:31, `Succeeded`, 87 s; + M `261d48a` / `e735a0d`, R `3acaa45` / `2c9bc1a`, X `db149e0`, H `89efaa6`,
  N `d665c6c`; tests seen red then green: CameraMove 10/14 -> 14/14, ViewmodelN18 6/7 -> 7/7, HitSpark 0/5 -> 5/5;
  fingerprint EQUAL). Its smoke (10:46-10:49): `[smoke] 186.8s DONE: all steps passed`.
  - Its on reel: kill20 `NO HIT in 60 frames`, headshot12 `NO HIT`, rankup `NO RANK-UP in 10 s`; the off side clean.
    Every on-side scenario's camera sat +1.77 deg in pitch (and ~1 deg in yaw) off the off side's at f5, and the view
    crept further in each post-roll (no SetLook there). Re-runs of kill20 (10:38) and headshot12 + rankup (10:41) came
    back clean: the offset was transient.
- **The cause, after two approaches** (CLAUDE.md's "blocker after two approaches" was not reached: the second worked):
  `InputReader.Poll` adds the mouse's delta (and the pad's sticks) after the reel's `SetLook`; on the fixed clock a
  stale, constant delta is a constant offset. Approach 1 (`cbf36ca`, build 5, 10:50): disable the gamepads and the mouse
  for the reel. Build 5's reels (10:52-10:56) were worse, and identical on both sides: disabling the mouse froze its
  last delta (the cursor lock's warp). Approach 2 (`ReaderInput.lookHeld`, the commit after `cbf36ca`): only SetLook
  moves the look while the reel runs; the mouse stays enabled, the pads disabled. **Editor reel 3** (Play check 3 of 6,
  10:59-11:01): kill12 / headshot12 / kill20 centred at pitch -2.62 / -0.24 / -1.44 (build 3's values), a steady
  post-roll, every kill landing. The Player's devices, logged by the reel: `Keyboard, Mouse, Xencelabs Hid Device,
  Wireless Gamepad`.
- M's landing return (build 4 read 133 ms, target 150-200): `409e083` (0.20 s return).
- Builds 2, 4 and 5 were spent on the harness's input path (the spare included): builds 7 / 7 at the close.

## Round 2 (build 6, 11:01-)

- **Build 6** (11:02, `Succeeded`, 61 s). Its reels: off (11:05:32-11:06:56) `2354 frames written, 0 failed`; on
  (11:06:56-11:08:41) `2357 frames written, 0 failed`; every kill landed, horde5 5 of 5, rankup 0 -> 1 on both. The
  camera matches on both sides at f5 (one scenario 0.03 deg: roundend). Its smoke (11:12:27-11:15:59, idle 0 s, two
  reviewers reading): `[smoke] 186.5s DONE: all steps passed`.
- **Metrics** (`Logs/feel/b6/FEEL-METRICS.md`): **on 105 PASS, 17 FAIL; off 58 PASS, 64 FAIL** (of 125).

  | Metric | On | Off | Target | |
  | --- | --- | --- | --- | --- |
  | M1 kill12 | 2.82 (44755 px) | 1.51 | >= 3.0 | FAIL |
  | M2 effects kill6 / 12 / 20 | 41.9 / 30.2 / 22.9 % | 37.5 / 20.2 / 20.3 % | >= 8 % at 20 m | PASS |
  | M2 marker | frame 1, 76 px, 367 ms | frame 1, 89 px, 317 ms | | PASS |
  | M3 kill12 / horde5 centroid | 1.76 / 1.06 body h | 0.73 / 1.05 | >= 1.5 | PASS / FAIL |
  | M4 hit / kill / head / punch / horde | 0 / 7 / 11 / 13 / 18 | 0 / 7 / 9 / 11 / 18 | | PASS |
  | M5 hit12 | 12.6 px, back 67 ms, flash 3 | 2.7 px | | PASS |
  | M9 HUD pop-ins rankup / roundend / pickup / hud-fight | 0 / 0 / 0 / 0 | | 0 | PASS |
  | M9 menu pop-ins title / settings / paused / results / mods | 1 / 1 / 0 / 0 / 0 (the run setup's Lock) | 1 / 15 / 3 / 1 / 0 | 0 | FAIL |
  | M10 / M11 | 0 / 0 | | 0 | PASS |

- **Reviewers 3 and 4 of 10** (r2a, r2b swapped) launched 11:12. Fix lists sent 11:13: X (M1 +2900 px, M3 horde5),
  N (the Lock pop-in, the onboarding row's 367 ms, the footer's re-entry).

### Round 2's blind reviewers (r2a 11:12-11:42; r2b, sides swapped, 11:12-11:30; reviewers 4 of 10)

| Scenario | r2a | r2b | "Too much?" (decoded) |
| --- | --- | --- | --- |
| hit12 | N18 ("a bold X grows on the body and an orange chip sits at its edge") | N18 ("the body tilts and a chip comes off its side") | neither |
| kill12 | N18 ("a black-outlined X, rays and a dark halo say kill while frozen") | N18 ("bold outlined marker and starburst; bigger chunks thrown wider") | neither |
| kill20 | N18 | N18 | N18 (r2b: "the marker hides the whole target in 2-4") |
| headshot12 | N18 | N18 | neither |
| horde5 | N18 | N18 | neither |
| kill6 | N18 | N17 ("[N17] the body bursts into big chunks on the spot") | neither |
| punchkill | Same | N18 | both (point blank: white, then orange) |
| boost | N17 ("FOV opens ~15 % more") | N17 ("[N17] long lines still full at 250 ms") | N17 (r2a: thin lines over most of the frame) |
| boostside | N17 | N17 | N17 (r2a) |
| sprint | N18 | N18 | N18 (r2b: "dense edge band over the HP and ammo panels") |
| slide | N18 | N18 | N18 (r2b, mild) |
| land | N18 ("the view nods, the gun starts ~50 px lower") | N18 | neither |
| strafe | Same | N18 | neither |
| swapall | N18 | N18 | neither |
| rankup | N18 | N18 | neither |
| roundend | N18 ("the title and band fade and grow in") | N17 ("[N17] slams from ~1.5x") | neither |
| pickup | N17 ("[N17] DOUBLE POINTS shrinks from oversized") | N17 ("slams down from ~1.4x") | neither |
| hud-fight | Same | N18 | neither |
| ui-title | N18 | N18 | neither |
| ui-settings | N18 | N18 | N18 (r2b: "still building at 250 ms") |
| ui-paused | N18 | N18 | neither |
| ui-results | N18 ("the grade stamps in oversized and tilted") | Same | N17 (both: "t8 whole-screen grey wash") |
| ui-mods | N18 | N18 | neither |

Per item (round 2): **U3-31.3 8 of 8** (target 6); **U3-31.4 2 of 2** (2); **U3-31.5 7 of 10** (7; boost 0 of 2);
**U3-31.6 4 of 8** (6; pickup 0 of 2, roundend 1 of 2, hud-fight 1 of 2); **U3-31.7 9 of 10** (7). The rubric for N18:
r2a 3/2/3/3/3/4/3/4/4/4 = **33**; r2b 4/3/3/3/3/3/3/4/4/3 = **33** (round 1: 32 / 31).

- **The suite on `3bdd99d`** (11:21-11:29): `tests: 1051 / 1052 passed, 1 failed`: `AutopilotTests.PerStep_AllocatesNothing`
  "Expected: 0 But was: 1" over 12169 fight steps; re-run alone (11:30) `1/1 passed`: logged as a flaky allocation count.
- **Round 2's fixes, integrated 11:35-11:41, red first** (MenuMotion18 11/12 -> 12/12; CameraMove 11/15 -> 15/15;
  HudMotion 15/18 -> 18/18): H `63ab2c4` / `7df2fdc` (the big banners slam from 1.4x at 0.6 alpha; the kill marker
  hollow, 40 px clear; the points pops' lanes); M `fc5ee07` / `9542cf4` (a boost layer to the 60 % box's edge at 0.8
  through the dash; the sprint / slide band out of the HUD's corners, 0.4); N `c8c9d97` / `f32e156` (every screen whole
  by ~200 ms; RESULTS' slab first); X `a32a54d` / `1a26185` (the disc x 0.7, 3.2 bodies; the COMET's blocks erupt up).
- `8a0e3fe` the probe sees a TMP text by its mesh's characters (builder H: SetText leaves `.text` empty, so ROUND CLEAR
  and late pops showed on one frame).

## Round 3 (the Editor, 11:43-; Play check 4 of 6, Editor reels 4 and 5 of 6)

- **The reels** (one guarded eval, FeelAB off then on, 2560 x 1440): off `2354 frames, 0 failed`; on `2357 frames, 0
  failed`; every kill landed, horde5 5 of 5, rankup 0 -> 1.
- **Metrics** (`Logs/feel/e4/FEEL-METRICS.md`): **on 104 PASS, 16 FAIL; off 58 PASS, 64 FAIL.** Now passing: **M1 3.25**
  (51613 px), **M3 horde5 1.88 body h** (100 % outside), M2 marker 96 px; M9 menu pop-ins 0 on all five ui-*; HUD 0.
  M8 region boost failed (10 of 24 frames) on a probe bug: the clear box was in 1080p canvas units, so at 2560 x 1440 it
  read small and off-centre (build 6's Player at 1080p passed); M's `087a202` reports it in screen pixels.
- **Reviewers 5 and 6 of 10** (r3a, r3b swapped) launched 11:51.

## U3-31.9 the backlog (each taken: commit, metric or capture, a two-line review)

| Item | Commits (builder) | Metric / capture | Review (two lines) |
| --- | --- | --- | --- |
| 1 The kill flourish captured and readable | `2505a73` (tests first) `3fd4133` (M) | ViewmodelN18Tests 4/4; the reel's kill post-roll (1.2 s) and the `flourish` probe marker | Any kill arms it (NIGHT17: multikill / rank-up only), 0.2 s after the freeze with the trigger let go: the gun lifts 3 cm and cants 12 deg in. The reviewers never named it, so its read is not proven by a judge. |
| 2 The QUICKDRAW glint on the gun's own surface | `38d0901` (tests first) `b689132` (M) | ViewmodelN18Tests 5/5; swapall flipbook (round 2: r2b "6 and 8 the gun flashes cyan") | The band runs over the gun's own meshes, so nothing floats off it (NIGHT17's strip did). Reviewers in rounds 2 picked tonight's swapall 2 of 2. |
| 3 The spawn ring in DANGER | `7a4c5c6` (tests first) `ce32e9d` (X) | CrunchMath.AdditivePeak in KillLadderTests (the ring's peak under `post.bloomThreshold`, so the red-orange isn't bloomed white); capture `Logs/shots/n18-backlog/sheet-spawn.png` (Play check 5) | The capture fired on `enemySpawn`, after the telegraph, so it shows the same pale floor disc on both sides: the ring itself isn't in a frame tonight. The rule is tested; the picture is not checked. |
| 4 The low-HP vignette pulse | `ce32e9d` (rules) `efa0653` (X); H's breath on it `6ac6134` | `Logs/shots/n18-backlog/sheet-lowhp.png` (4 captures 0.25 s apart per side, HP 12 of 100) | Tonight's edge beats (strong, weaker, strong) where NIGHT17's breathes; under 3 bumps a second, REDUCED MOTION steady. In stills the difference is mild. |
| 5 HUD STYLE ATTACHED | `6775c11` (tests first) `dab6fb8` (H), hook `91ed2d4` (M) | HudMotionTests 3 attach tests; the Player's saved setting is `hudStyle: attached`, so every reel since build 3 ran with it | The weapon block rides the gun's sway and bob (up to ~18 px, ~1.1 deg). Not called out by a reviewer. |
| 6 Per-gun recoil measured as distinct | `5ec11cf` (M) | ViewmodelN18Tests.EachGunsKickIsDistinct: every pair of the five guns at least one step apart (the closest, COMET vs LONGWAVE, 3.2 steps) | A measurement of NIGHT17's kicks: they already differ; nothing was retuned (no FeelAB). The reel's swapall doesn't fire, so no frame shows it. |

### Round 3's blind reviewers (r3b, sides swapped, 11:51-12:15; r3a still reading at 12:21)

r3b decoded: hit12 N18 ("the X keeps a dark outline on the white flash; an orange chip at the impact"); kill12 N18,
kill20 N18 ("the wedges frame the small, distant body"), headshot12 N18, horde5 N17 ("[N17] big slabs keep flying;
[N18] small-cube confetti"), kill6 N17 ("[N18] only sparks in B5-B6"), punchkill N17 (too much N18: "a pale-orange wash
fills the crop"); boost N17 (too much N18: "a solid white edge band and haze"), sprint N18, slide N18, land N18, strafe
N18, boostside N17; rankup N18, roundend N17 ("[N18] A1 nearly invisible, A2 a faint 1.24x"), pickup N17, hud-fight N17
("[N17] the D ~1.7x and white"); ui-title N18, ui-settings N17 ("[N17] rows cascade; [N18] the whole list at once,
faint"), ui-paused / results / mods Same. Rubric for N18: 3 / 4 / 3 / 3 / 3 / 3 / 3 / 4 / 4 / 4 = **34**.

Per item (r3b alone): U3-31.3 3 of 4; U3-31.4 1 of 1; U3-31.5 4 of 5; U3-31.6 1 of 4; U3-31.7 1 of 5.

**The lead's mistake, logged:** round 2's note to H asked for the slam's first frame at 0.6 alpha "so M9 sees an
entrance", but under the tool's rule a first frame above 1.05x its rest scale is already an entrance: the 0.6 alpha read
as faint and lost roundend / pickup / hud-fight.

**The last fix lists (12:21-12:23; last code round before the 13:12 freeze):** H: the slams at full alpha from ~1.6x,
white-hot then the colour; N: the rows' cascade back but quick (in by ~250 ms), PAUSED / RESULTS / MODS visibly
different in their first 170 ms; X: horde5's chunks as slabs, close kills that stay in the crop, no pale wash at point
blank; M: the boost as fewer, thinner, longer radial streaks (no band, no haze), still out of the central 60 %.

r3a (11:51-12:31) decoded: hit12 N18, kill12 N18, kill20 N18 ("wedges frame the body at range with the centre clear"),
headshot12 N18, horde5 N17 (too much N18: "a swarm of tiny embers"), kill6 N17, punchkill N17 (too much N18); boost N18,
boostside N18 (too much N18: "a dense frame with a hard rectangular inner edge; it runs under the HP and points boxes"),
sprint N18, slide N18, land N18, strafe N18; rankup N18, roundend N18, pickup N17, hud-fight N18; ui-title Same,
ui-settings N17 ("[N17] rows cascade in one per frame; [N18] the whole panel fades in within about 2 frames"), ui-paused
N18, results / mods Same. Rubric for N18: 3 / 2 / 3 / 3 / 3 / 3 / 3 / 4 / 4 / 3 = **31**.

**Round 3 per item (r3a + r3b): U3-31.3 6 of 8** (target 6); **U3-31.4 2 of 2** (2); **U3-31.5 9 of 10** (7);
**U3-31.6 4 of 8** (6); **U3-31.7 2 of 10** (7: ui-settings to NIGHT17 with both, results / mods "Same").

**The last fixes, integrated 12:28-12:31, red first** (MenuMotion18 12/14 -> 14/14, HudMotion 16/18 -> 18/18, CameraMove
16/17 -> 17/17; KillLadder 14/14, Crunch 15/15, Menus 20/20, ViewmodelN18 7/7; fingerprint EQUAL): N `ba8cbc6` /
`a0c4dce` (rows cascade 20 ms apart, each a 60 px slide, in by ~260 ms; PAUSED / RESULTS' slab sweeps 360 px first;
MODS' cards rise 35 ms apart); H `2f8bab4` / `4d0c5e9` (ROUND CLEAR, the power-up banner and the style block's first
showing slam at full alpha from 1.6x, white-hot 50 ms); M `a1c0f19` / `e651a9d` (the boost as 50 thin radial streaks to
the 60 % box's edge, no haze); X `cd408fb` (horde5's bodies break into slabs; kills under 12 m throw slower; no wash at
point blank).

**Play check 6 of 6 / Editor reel 6 of 6** (12:33-12:35, FeelAB on, the 13 touched scenarios, `Logs/feel/e6/on`,
1414 frames, 0 failed): M1 **3.55** (56454 px), M3 kill12 1.76 / horde5 **1.58** body h, M2 marker 96 px, M8 boost
`0 of 24 frames` in the centre (the clear box in screen pixels now), M9 pop-ins **0** on roundend, pickup, hud-fight and
all four ui-* in it. Flipbooks `Logs/feel/e6/flip-on/`: horde5 throws slabs (tiles 3-5, 8) with the generic death
embers under them (6-7); the boost is long thin radial streaks clear of the middle.

**The code is final at 12:37** (no Play check or Editor reel left to try another change; builds 6 / 7).

**The final code's gates, before the close** (12:39-12:48, on `06110b6`): the suite `tests: 1062 / 1062 passed,
0 failed, 0 skipped (413.75 s)`, `replays: 22 / 22`, `golden tests: 96 / 96`; CoreHarness `golden total: 39 vector sets
(48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; SimHarness `fingerprint seed 1: 12 hashes in 1.40 s:
EQUAL to the baseline`, `impact PASS: the budget holds on 2 map(s)` (worst rolling second 192.0 ms of 200).
Docs drafted: HANDOFF.md (`06110b6`), NIGHT18-REPORT.md's tables and questions, FEEL-PLAYTEST.md, PORT-STATUS.md.

## U3-31.10 the close (13:12:16-; `date` 13:12:16 = 4:50 after the first)

- **The freeze and build 7** (13:12:33, `Succeeded`, 55 s, 0 errors; code `d5dd749`, stamped `SKYGRIND 0.1.0
  (d5dd749)`): ProjectSettings: only UnityConnectSettings; the theme SDF fonts restored.
- **The smoke, alone** (13:14:09-13:17:42; every builder and reviewer had reported, nothing of the night's running;
  idle 0 s: GG's other project's pytest workers keep the CPU at 100 %): `[smoke] 184.0s DONE: all steps passed`; round 1
  `cleared in 8 s: 9 kills, 9 headshots, points 1576` (the clean run's numbers).
- **The Player gates** (13:17-13:19): `-skygrindGolden` `golden total: 39 vector sets (48094 records), 22 replays (22
  exact); 0 failing, 0 mismatches`; `-skygrindFingerprint` `seed 1: 12 hashes over 7200 steps in 1.03 s: EQUAL to the
  baseline`; `-skygrindReplay` on the smoke's tape `EQUAL (1 of 1 checkpoints, 490 steps); seed 737373, Stopped, stamp
  SKYGRIND 0.1.0 (d5dd749)`.
- **The final reels** (Player reel runs 13-14 of 14: build 1 1, build 2 2, build 3 2, build 4 2 + two partial re-runs,
  build 5 2, build 6 2, final 2 = 15 runs, one over the cap of 14; the over-run was build 4's partial re-runs, logged):
  off (13:19:47-13:21:02) `2354 frames written, 0 failed`; on (13:21:02-13:22:25) `2357 frames written, 0 failed`;
  every kill landed, horde5 5 of 5, rankup 0 -> 1, the devices logged and pads disabled.
- **The final metrics** (`Logs/feel/final/FEEL-METRICS.md`): **on 104 PASS, 18 FAIL; off 56 PASS, 66 FAIL** (of 125).
  M1 **3.05** (48423 / 15881 px); M2 kill6 / 12 / 20 41.9 / 32.1 / 22.9 % of h, marker frame 1, 96 px, 367 ms; M3 kill12
  1.76 / horde5 1.59 body h, 100 % outside; M4 0 / 7 / 11 / 13 / 18 steps; M5 12.6 px in 50 ms, back 67 ms, flash 3
  frames; M6 boost +11.8 by frame 2, back 300 ms, sprint +5.9 held, slide +6.9 / 0.80 m / 4.0 deg; M7 strafe 2.3 deg,
  land 0.141 m back 183 ms; M8 0 bad frames, 0 in the centre (sprint, boost); M9 HUD pop-ins 0 (rankup, roundend,
  pickup, hud-fight), menus 0 on ui-title / paused / results / mods and **2 on ui-settings** (TITLE's run-setup MAP row's
  `Nav focus/Fill` and `/Bar` whole at f13, before SETTINGS opens); M10 0 under 18 px; M11 0 unlisted. The remaining
  FAIL rows are M9 entrance times (pulsing accents that "never settle", camera-scenario lines) and the camera
  scenarios' line pop-ins (an instant onset by design).
- **M12, the bench** (`-skygrindBench 60`, 24 husks; Balanced, AC, the CPU at 100 % before and after each, as at the
  baseline): b7-1 (13:24:13-13:25:44) `median 4.77 ms, p95 8.38 ms, worst 1% 15.62 ms`, `p99 15.62 ms`; b7-2
  (13:25:50-13:27:12) `median 3.82 ms, p95 7.67 ms, worst 1% 13.42 ms`, `p99 13.42 ms`. Baseline p99 17.66 / 34.32 ms:
  the final is under baseline + 1 ms on both runs (PASS; the load from the other project is the noise).
- **GIFs** (13:27-13:28): `Logs/feel/final/ab-kill20.gif` (46 frames, 1.53 s), `ab-hit12.gif` (0.93 s), `ab-boost.gif`
  (1.20 s), `ab-ui-settings.gif` (1.90 s), NIGHT17 left / tonight right; flipbooks `Logs/feel/final/flip-on`, `flip-off`.
- **The final pair of blind reviewers** (9 and 10 of 10; fa, fb with every side swapped) launched 13:23.
- **The suite on the frozen code** (13:28-13:37, async): `tests: 1062 / 1062 passed, 0 failed, 0 skipped (509.69 s)`,
  `replays: 22 / 22`, `golden tests: 96 / 96`.
- The builders' worktree folders removed (13:28; every commit integrated: `git cherry` 0 not applied on all five; the
  branches stay). Docs: NIGHT18-REPORT.md's final metrics, HANDOFF.md section 0, FEEL-PLAYTEST.md, PORT-STATUS.md.
- **Slip:** a one-line doc edit by `sed` (FEEL-PLAYTEST.md's status line, 12:49; rule 9 wants the Edit tool).

### The final pair of blind reviewers (fa 13:23-13:59; fb, every side swapped, 13:23-13:46; reviewers 8 of 10 used)

Decoded with `Logs/feel/final/key-fa.json` / `key-fb.json` (N18 = FeelAB on, build 7):

| Scenario | fa | fb | "Too much?" (decoded) |
| --- | --- | --- | --- |
| hit12 | N18 ("an X hit-marker on the body and an orange spark at its edge") | N18 ("a grey X on the head and a spark at the body edge") | neither |
| kill12 | N18 ("a dark disc, radial streaks and 4 kill arrows") | N18 ("dark disc, rays and 4 corner kill-markers, then big slabs") | neither |
| kill20 | N18 | N18 | neither |
| headshot12 | N18 ("the kill arrows hold through tile 6") | N18 | neither |
| horde5 | N18 | N18 | fa: both (N17 "3-5 slab mass", N18 "6-7 spark confetti"); fb: N17 |
| kill6 | N18 | N18 | N18 (fa and fb: "slabs near the camera cover the right third") |
| punchkill | N18 | N18 | N17 (both) |
| boost | N18 ("FOV ~14 % wider, streaks on the rim and a clear centre") | N17 ("denser streaks nearer the centre") | N17 (fa) |
| boostside | N18 | N18 | N17 (fa) |
| sprint | N18 | Same | N17 (fa) |
| slide | N17 (weak) | N18 | neither |
| land | N18 ("the camera drops ~25 px and the gun dips") | N18 | neither |
| strafe | N18 ("rolls 1.5-3.25 deg vs 1.0-2.25") | N18 | neither |
| swapall | Same | N18 | neither |
| rankup | N18 | N18 | neither |
| roundend | N18 (weak) | N18 | fa: both ("banner text collides with the +75 / +71 pops") |
| pickup | N18 ("the text flashes white, then gold, as it shrinks") | N17 | neither |
| hud-fight | Same | Same | neither |
| ui-title | Same | Same | neither |
| ui-settings | N18 ("the panel and tabs sweep in") | N18 | neither |
| ui-paused | N18 | N18 | neither |
| ui-results | N18 | N18 | fa: both (N17 "the grey wash"; N18 "the card covers the POINTS value") |
| ui-mods | N18 ("cards cascade one per tile") | N18 | neither |

**Final per item (fa + fb):** U3-31.3 **8 of 8** (target 6); U3-31.4 **2 of 2** (2); U3-31.5 **7 of 10** (7);
U3-31.6 **5 of 8** (6); U3-31.7 **8 of 10** (7). The rubric for N18: fa 4 / 3 / 3 / 3 / 3 / 4 / 3 / 4 / 4 / 3 = **34**;
fb 4 / 2 / 3 / 3 / 3 / 4 / 4 / 4 / 4 / 4 = **35** (NIGHT17's fresh reviewers: 25, 23, 27, 27).

## The close table

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-31.1 Start | DONE | `date` 08:22:08 first; GG's notes: none (no GG-NOTES.md, no notes in the start message); HANDOFF section 1 rows logged (RECEIVED); `set_autotick` on; the machine (Balanced, AC, CPU 100 % from another project, 348 GB free); tasks.json parses with U3-31.* (159 items); `Builds/n17-before/` exists |
| U3-31.2 The harness and the baseline | DONE | FeelProbe, FeelClock, FeelAB, FeelReel (phase A + B) and feel-metrics.py committed (`e9fe051`, `0a505ab`, `20fd4bf`), FeelProbeTests 3/3; `Logs/feel/b1/FEEL-METRICS.md` with every phase-A row valued or n/a with its reason; the clock proof on the Player (b1 kill12 `{2: 86, 1: 1}` steps per non-held frame, boost `{2: 72}`); three eye checks agree (FOV 85.0 vs 83.8 from the frames; M1 31481 by hand vs 31302; the SETTINGS head's first frame); the bench baseline 2 runs (p99 17.66 / 34.32 ms, Balanced); builders launched 08:42:28 (0:20); phase B and its FeelAB-off reels in from build 2 |
| U3-31.3 The kill at every range | SHORT (the time limit) | M1 **3.05**, M2 **22.9 %** at 20 m, M3 **1.76 / 1.59** body h, M4 7 / 11 / 13 / 18 steps: all PASS; the final pair picked tonight **8 of 8** (target 6); but reviewer fa marked horde5 "too much" on both sides (tonight's "6-7 spark confetti"), which holds the item until fixed, and the code had frozen at 4:50 with builds 7 / 7 |
| U3-31.4 The hit that lands | DONE | M5 **12.6 px** within 50 ms (against the world anchor), back at 67 ms, flash 3 frames: PASS; hit12 picked tonight by both final reviewers: **2 of 2**; no "too much" |
| U3-31.5 Movement you can see | DONE | M6 (boost +11.8 by frame 2, back 300 ms; sprint +5.9 held; slide +6.9 deg / 0.80 m / 4.0 deg), M7 (strafe 2.3 deg; land 0.141 m, back 183 ms), M8 (0 frames strong under 70 %, 0 in the central 60 %): PASS; the final pair **7 of 10** over boost, sprint, slide, land, strafe (target 7); no "too much" on tonight's side |
| U3-31.6 HUD motion and the kill marker | SHORT (the time limit) | M9 HUD pop-ins **0** (rankup, roundend, pickup, hud-fight), M10 0, M11 0, M2 marker frame 1, 96 px, 367 ms: PASS; the final pair **5 of 8** (target 6: pickup 1 of 2, hud-fight Same x 2) and fa's "too much: both" on roundend (the pops on the banner); the code froze at 4:50, builds 7 / 7 |
| U3-31.7 Menus motion and words | SHORT (the time limit) | the final pair **8 of 10** (target 7); M10 0, M11 0; the smoke's menu and pad steps passed (`DONE: all steps passed`, build 7); but M9 shows **2 menu pop-ins** (TITLE's run-setup MAP row `Nav focus/Fill` and `/Bar` whole at f13 of ui-settings, before SETTINGS opens) and fa's "too much: both" on ui-results (the grade card over POINTS); the code froze at 4:50, builds 7 / 7 |
| U3-31.8 The rounds | SHORT (a cap) | three rounds logged with metrics tables, both reviewers' verdicts and 10-row scores, fix lists and what changed (round 1 build 3, round 2 build 6, round 3 the Editor); a fourth didn't fit: builds 7 / 7 (builds 2, 4, 5 spent on the reel's input path: a pad's fire, a stale mouse delta, two approaches) and Editor reels 6 / 6 |
| U3-31.9 The backlog | DONE | all six taken, each with commits, a metric or capture and a two-line review (the backlog table): the flourish `3fd4133`, the glint `b689132`, the spawn ring `ce32e9d` (its rule tested; the capture inconclusive, said so), the low-HP pulse `efa0653` (+ `sheet-lowhp.png`), HUD STYLE ATTACHED `dab6fb8`, per-gun recoil `5ec11cf` |
| U3-31.10 Close | DONE | freeze and build 7 at 13:12 (`Succeeded`); the smoke alone `DONE: all steps passed`; `-skygrindGolden` 0 mismatches (22 exact); `-skygrindFingerprint` EQUAL; `-skygrindReplay` EQUAL; the bench 2 runs (p99 15.62 / 13.42 ms); the suite on the frozen code 1062 / 1062; the final reels, metrics (on 104 / off 56 of 125), GIFs and the final blind pair decoded above; NIGHT18-REPORT.md, FEEL-PLAYTEST.md, PORT-STATUS.md, HANDOFF.md (`u3-n18`), tasks.json; tag `u3-n18` |
