# NIGHT12-LOG: the look, the feel and the meta core (a building night)

Run started 2026-10-03 7:06 PM (first `date`). GG's goal message: a 12-hour hard stop (7:06 AM); caps: 3 Play checks,
12 captures, 1 Player build. The close starts by about 5:30 AM. Times below are from `date`.

## RECEIVED (U3-25.1, 7:06-7:09 PM)

| Row | Command | Result |
|---|---|---|
| 1 | `git rev-parse --short HEAD`, `git log --oneline -3`, tags | HEAD `e3d8b27` (the research docs commit), one commit above `u3-n11` = `1128c2e`, as HANDOFF's update note says; no tag on HEAD |
| 2 | `git status --short` | ` M ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT7.md` .. `?? NIGHT13.md`: as expected |
| 3 | `bash Tools/unity-loop.sh status` | `"status": "ready"`, `"playMode": "stopped"`, Unity 6000.3.7f1 |
| 4 | `bash Tools/unity-loop.sh tests` (7:08 PM) | `tests: 437 / 437 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 90 / 90` |
| 5 | `dotnet run --project Tools/CoreHarness -c Release` (7:08 PM) | `golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| 8 | `powercfg -getactivescheme`, AC | Balanced (381b4222-...), on AC (PowerOnline True) |
| 9 | idle seconds (GetLastInputInfo) | 80.8 s at 7:07 PM: GG just started the run and left |

Rows 6-7 (the Player) not run: rows 1-5 matched.

**GG's answers (HANDOFF section 5, the goal message):** D-29 no gibs (the web's dissolve); D-30 no new packages;
D-31 HEAT rungs 1-3 are the web's mutators only; D-32 mechanical contract texts; D-33 a local DAILY ships; D-13 the
theme fonts yes. D-18 stays a LAB toggle, off (no new answer). D-25..D-28 unchanged (no new answer): as shipped.

## Shared types (7:10-7:35 PM)

- `1677ae3`: Core/Profile.cs (the JSON profile, NIGHT12 rule 6) with ProfileTests (5); the meta API stubs (Contracts,
  Mastery + Finishes, Payoff, Heat + MutatorMath, Daily + Seeds, Tips + Onboarding) returning safe empties; MetaEvents
  (event names and payloads; RunSetup); RunStats' meta fields; SurvivalGame: the profile (records migrated once from
  PlayerPrefs and saved there), FrameDt / WorldDt, the run setup, Recording; Ui: the meta hooks (partial methods) and the
  title's PLAY / CONTRACTS / WEAPONS; UnityTuning partial; the design placeholder blocks. Gate: `tests: 442 / 442
  passed, 0 failed`, `replays: 22 / 22`, `golden tests: 90 / 90` (7:27 PM).
- `20d0407`: SurvivalGame's typed fields for the meta systems.
- **Found:** the Unity-generated csproj files build outside the Editor (`dotnet build Skygrind.Game.csproj`: 3.8 s).
  A scratch script (scratchpad `wt-csproj.cjs`) makes worktree copies (absolute Library paths, one `dir\*.cs` glob per
  source folder); tested on a throwaway worktree: Game, Core.Tests and Editor projects build. So tonight every agent,
  Game ones included, works in its own git worktree and compiles there; nothing lands in the main tree until the lead
  cherry-picks. (Rejected: Game agents writing into the main Assets/ while the Editor is open, as NIGHT11 did: an
  Editor refresh mid-edit compiles half-written files.)
- Slip: a `cat > ... <<EOF` scratch file was started for a code patch (CLAUDE.md rule 9); it stayed empty and unused;
  the patches went through the Edit tool.

## Agents (launched 7:36-7:40 PM; each in its own worktree from `20d0407`)

| Agent | Items | Files owned |
|---|---|---|
| A | U3-25.2 light and lens | Stone.shader, Look.cs, LightAndLens.cs (+ new), Motes / BlobShadow shaders, UnityTuning.Light.cs |
| B | U3-25.3 enemy shader, U3-25.4 kills | EnemyViews.cs (views, Particles instanced), Enemy.shader, Particle.shader, KillConfirm.cs, UnityTuning.Enemy.cs |
| C | U3-25.5 viewmodel, U3-25.4 movement / weapon FX, U3-25.10 finishes on guns | Viewmodel.cs, CameraFeel.cs, BlockyKit.cs, WallBuys.cs, WeaponFx.cs, ImpactFx.cs, MovementFx.cs, UnityTuning.Feel.cs |
| D | U3-25.6 ceremonies and type | Ui.cs + the HUD partials, UiCeremony / UiResults / UiHudMotion / TitleView, MysteryBoxView.cs, Editor/ThemeFonts.cs, Assets/Fonts, UnityTuning.Ui.cs |
| E1 | U3-25.7 / .8 / .10 Core | Contracts.cs, Mastery.cs, Payoff.cs + tests; design blocks contracts, grade, mastery |
| E2 | U3-25.9 / .11 Core | Heat.cs, Daily.cs, Tips.cs, HuskBrain.speedMult + tests; design blocks heat, onboarding |
| F | U3-25.7-.11 Game | UiMeta / UiContracts / UiWeapons / UiPayoff / UiOnboarding, GameSettings.cs, UnityTuning.Meta.cs |

The lead alone: SurvivalGame wiring, the Editor, captures, SliceSmoke's two steps, commits.

## The lead while the agents build (7:40-8:55 PM)

- **The "before" capture (7:39-7:41 PM), no Play check spent:** the u3-n11 Player's bench (`-skygrindBench 15`, seed 99,
  the spawn view), its window grabbed at 8 s by a scratch PowerShell script. The first try copied the screen and got
  another window on top (a background process can't take the foreground): an empty dark frame, **capture 1 of 12
  (failed)**. The retry uses `PrintWindow(PW_CLIENTONLY | PW_RENDERFULLCONTENT)`: `Logs/shots/n12-map-before.png`,
  **capture 2 of 12**. The rebuilt Player's bench gives the "after" from the same spot at the close. That 15 s bench
  printed `worst 1% 6.20 ms` (u3-n11's 60 s close bench: 5.55 ms): the 6.9 ms bar has less room than the docs say.
- `f42cb7d` the lead's meta wiring against the stub signatures (the meta systems built at Init; StartRun reads the run
  setup and stamps the stats after `run.Start`, which re-enables regen and restarts the economy; EndRun: P grade, the
  score line's mutators and seed code (main.js endRun passes both), contracts / HEAT / DAILY / best grade / first run,
  one save; the locked MODS read unlocks.js flags; `profileReadOnly`; Profile.CopyFrom). 443 / 443, 22 / 22, 90 / 90.
- `1d59b96` `SurvivalGame.forcedMove` (automation: the slide and wall-ride captures).
- `6dc2cac` Mods.Give (mods.js give) and the DAILY's twist: main.js treats any run on today's seed as the DAILY
  (`seed === dailySeed(new Date())`), so the port does too (rejected: the setup's mode alone); tips.js moments as main.js
  calls them (roundBreak: break / ready; powerUpSpawn; the buy prompt's kind). 444 / 444, 22 / 22, 90 / 90.
- `171db51` agent F's meta screens (its `e736a05`) cherry-picked. Changed: F hid the HEAT rung while DAILY is picked ("a
  DAILY is the same run for everyone"); NIGHT12 section 5's smoke step runs "a DAILY ... with HEAT 1 stamped on its
  RESULTS", so a DAILY may carry a rung (rejected: F's rule, which would make that step impossible). Esc closes the meta
  screens. Agent D told (SendMessage) to keep the Ui helpers and fields F uses, not to add a second static constructor
  to Ui, and to expose the RESULTS HEAT badge for the smoke.
- The main tree's Unity csproj files were globbed by the same script (new files compile with `dotnet build` there too).
  Unity regenerates them (explicit lists) at each Editor compile, so the script runs again before a `dotnet build`.

## Integrations (9:15-9:36 PM)

- `fad5a8d` agent C (40fd984, bc882b2, 1ce570c): the viewmodel's life, MovementFx, CameraFeel.Wire, WeaponFx's tracers /
  flare / shells, finishes on guns. Wired: feel.Wire, MovementFx, SetFinishSource(mastery.FinishFor), viewmodel.Respawn.
  Taken from C's report: effects.js bursts `fx.sparkCount - 1` sparks beside the flare and none for a projectile hit
  (rayCount 0); the port burst `sparkCount` for every world hit. Port-only (C's calls, logged): shells (the web has
  none), wall-ride sparks, the THUNDERCLAP's extra pellets drawn cosmetically (shotResult carries the first pellet),
  the pump timed from "fire" + weapons.shotgun.pumpDelay (Core never emits shotgunPump), a finished gun hangs solid on
  the wall. Editor compile clean.
- `1ea530e` agent B (28a36bb, 210cdc4): SKYGRIND/Enemy (rim, flash, dissolve, x-ray), instanced Particles, KillConfirm.
  Shaders checked in Unity with ShaderUtil (Enemy, Particle: 0 messages) and added to Always Included by an editor eval
  (GraphicsSettings.asset: 2 lines). B's calls: the web has no enemy death dissolve (dissolve.js is the viewmodel's
  swap), so its numbers (0.22 s) are used; flinch / vibration numbers port-only from Research/polish B3; KillConfirm on
  feel time (the web's update(feelDt)).
- `77edbfb` agent E2 (90e65b7, 733d20e): Mutators, Heat, Seeds, Daily, Tips, Onboarding, HuskBrain.speedMult. Gate:
  `tests: 469 / 469 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 90 / 90` (9:35 PM); harness `34 vector sets
  (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches`. The 25 new tests were not seen red (they compile
  only against the implementations; E2 ran them under NUnit outside Unity). E2 flags for GG: a HEAT rung's points
  multiplier also raises the points a player can spend. The mutator picks of the web's title (toggles) have no picker
  in the port: runs play the rung's mutators and the DAILY's twist only (LOW GRAVITY unavailable).
- `c21a6cc` agent A (2e7c3c0) and `6fda847` agent E1 (1006db1, 3476726, acc85f5), gate together: `tests: 500 / 500
  passed, 0 failed`, `replays: 22 / 22`, `golden tests: 90 / 90` (9:50 PM); harness 34 sets, 0 mismatches. Motes and
  BlobShadow compile in Unity and are Always Included. A's wiring rides E1's commit (SurvivalGame's hunks for both); the
  first-run guide light resolves Ui.GuideTarget in SurvivalGame.UpdateGuide (a loop instead of List.Find: no closure per
  frame). E1's 31 tests were seen red outside Unity by two mutations (E1's report).
- `01b21e6` agent D (4d48180) + the fonts made in the Editor (SKYGRIND/Make Theme Fonts, 9:55-9:58 PM). Fixed in D's
  editor script: TryGetGlyphWithUnicodeValue returns true with glyph 0 for a lacking character, read as "atlas full",
  so every face went to 2048 x 2048 (14 MB of YAML each); glyph 0 now means lacking, and the sampling is 56 / 6 so all
  ~200 glyphs fit 1024 x 1024 (7.8-8.6 MB each, about 0.3 MB gzipped: the kerning tables, 10,957 pair records). Lacking
  in the theme faces: U+00B5, U+2190-2193, U+25A1 (the lock glyph), U+2713; they fall back to TMP's default static
  atlas (no fallback-asset rewrite seen after Play check 1).
- `b923d91` smoke steps 11-12. Slip: the steps were inserted with `node -e` (CLAUDE.md rule 9 wants a written .cjs).
- All seven worktrees removed by 10:00 PM.

## Play check 1 of 3: the look pass (10:04-10:14 PM)

`editor_focus`, Play; boot clean (`Survival loaded`, no exception); one eval coroutine (scratchpad look-pass.cs):
`[look] ... done` in 18 s. Captures (Logs/shots, 7 this check; **9 of 12 used**):
- `n12-title.png`: **wrong**: it shows CONTRACTS' first frame (CaptureScreenshot writes at the frame's end and the eval
  opened CONTRACTS in the same frame: my sequencing), with the new rows drawn as solid cyan bars in that frame.
- `n12-contracts.png` (0.8 s later): CONTRACTS · 0 / 39 DONE, every row with name, desc, bar and value; the living
  title's camera over the map behind. Good.
- `n12-kill.png`: a THUNDERCLAP kill at 9.5 m: the dissolve (white, breaking up), the kill ring, a light pool, the
  blob shadow, the lesson card (FIRST RUN · ROUND 1 · MOVE AND SHOOT), the theme fonts on the HUD, the pump-action
  viewmodel. The stripes over the view are THE GRINDER gate's barrier field (the spawn stands beside it; the "before"
  bench had every gate open).
- `n12-round.png`: **wrong moment**: round 1's clear, not round 2's burn-in (the wait loop exited before the round
  cleared). No ceremony visible.
- `n12-slide.png`: the viewmodel mid-slide (mover state Slide), the KESTREL with GRAPHITE chosen; but it slid into a
  wall: the frame is a flat wall.
- `n12-wallride.png`: mid-wall-ride (state WallRide) at (-9, 1.2, 30): the wall tilt, the ride streaks, a light pool at
  the exfil radio. Good.
- `n12-results.png`: RESULTS mid-tally: RUN OVER, ROUND 2, three counting cards (808 points, 7 kills, style 16 fading
  in), the PAYOFF strip (ROUND 2 · +1 ON YOUR BEST, the closest contracts, NEXT UNLOCK □ MOD: STYLE TAX). Good.
- Not taken: the dissolve shot (the dead husk's pooled object respawned alive, so the eval's loop shot it again) and
  the far husk (no clear floor at 26-34 m in sight from the spawn).
- Inspected after (evals, no capture): CONTRACTS' live rows are correct (white 4% backs, Barlow / Chakra, zero-width
  fills): the cyan was the opening frame only. Fixed in `f4f731e` (rows built with the screen; live contracts first).
- Plan for the 3 captures left: TITLE; one shot from the "before" spot (the spawn, its yaw, every gate open) during
  round 2's burn-in with a far husk (the rim) and a near one dissolving (the round ceremony + rim + dissolve + the lit
  map after); one spare (the slide on open floor, or a retake).

## Between the checks (10:14-10:31 PM)

- `f4f731e` CONTRACTS: rows built with the screen; live contracts first, the dormant web badges last.
- `c75a6f5` RETRY SAME SEED on RESULTS (NIGHT12 item 8; no builder had it).
- A read-only review agent over tonight's per-frame paths: two real per-frame allocations from tonight
  (`Finishes.T`'s ConditionalWeakTable callback, a delegate per call under C# 9, every frame through
  `Mastery.FinishFor` once a finish is chosen; a cfg lookup in the guide) and older ones (cfg lookups every step in
  EnemyNear and the EXPOSED drain, per hit / kill in the burst, trauma and hitstop handlers). `f69e370` fixes all of
  them (read once). Gate: `tests: 500 / 500`, `22 / 22`, `90 / 90`, harness 0 mismatches (10:30 PM).

## Play check 2 of 3: the retakes, the allocation profile, the Editor smoke (10:31-10:39 PM)

- **Slip: 3 captures over the cap.** The retake eval's answer timed out on the client ("Main thread operation timed
  out after 5000ms"); `unity-loop.sh eval-file` retried 10 s later, after the first run had finished (55.6-63.1 s) and
  destroyed its guard marker, so a second run (71.5-78.9 s) re-took the same three files before I stopped it (a
  StopAllCoroutines eval at 111 s, too late). **Captures: 15 of 12** (n12-title / n12-round-after / n12-slide each
  written twice; the files hold the second run's frames). Rule learned twice (NIGHT11's double smoke, tonight): the
  guard must outlive the run, and start-something evals go through `unity command eval_file` directly. Memory
  `play-check-eval-recipe` updated. No capture was taken after this.
- `n12-title.png`: TITLE in the theme fonts: the SKYGRIND wordmark (Chakra Petch; its red-orange back layer is the
  web's own `.logo .back`), the run setup row (SURVIVAL, □ DAILY, HEAT 0 with □1-3 and their locks: "□ HEAT 1: EXFIL
  ONCE", "□ DAILY: FIRST FIFTEEN"), the six buttons, the controls line, the living title's camera over the map.
- `n12-round-after.png`: from the "before" spot (seed 99, every gate open, the spawn's yaw): round 2's burn-in (the
  two tally strokes at the centre), the near husk dissolving (10.4 m) with the kill ring, the far husk alive (22.5 m)
  under the lock brackets, the round 2 lesson card (DASH, SLIDE, STYLE), light pools; the lit map after, same view as
  `n12-map-before.png`.
- `n12-slide.png`: the viewmodel in the Slide state again against a wall (speed 0: the plaza's central cover block a
  few metres from the spawn). **Reported short: no clean mid-slide capture** (the cap is spent).
- **Allocation profile** (the bench's setup in Play mode, 320 frames, 24 husks; NIGHT11's profplay method): GC.Alloc
  1322 KB = 4231 B / frame: SKYGRIND.Brain 3310 B / frame in 30 allocations (the spawn's warm-up), SKYGRIND.R.Ui 663 B
  / frame (15 / frame: NIGHT11's known debt), SKYGRIND.Buyables 125 B / frame (1.7 / frame: not in NIGHT11's profile,
  which ran before its run-shape buyables were wired), SKYGRIND.R.Input 68 B / frame (known); nothing from tonight's
  render systems (no R.Light / R.Fx / R.Enemies line).
- **Editor smoke** (10:36-10:39 PM): every earlier step PASS (round 1 in 8 s, ARMORY, VIGOR, the box, the three wall
  guns, the recap, R from RESULTS / DOWN, the spitter round, the power, the SHOCK PLATE kill, the HOLD 3600 / 3600,
  THE PROTOCOL, EXFIL at round 11 with the line now ending `seed 0000UV, 2 forged, PROTOCOL`); step 12 PASS (`DAILY
  run: seed QRSTJY (today's QRSTJY), daily True 2026-10-04, HEAT 1 x1.25, mutators [glassCannon, noRegen]`; `DAILY
  RESULTS: Results, heat 1, badge 'HEAT 1', counted True`); step 11 **FAIL** (`completed in round 27839`): my step
  called DebugSkipTo every frame of a break, which restarts the break, so no fight began. `[smoke] 209.8s DONE: 1
  step(s) failed`. Fixed in `9a025fb`.
- Seen in the captures and fixed: letter-spaced labels drew uneven gaps ("R O UND", "F IRST RUN", the tagline): the
  faces' imported kerning; `5335c0b` drops it (assets 2.9-4.0 MB, from 7.8-8.6).

## After check 2 (10:45-11:05 PM)

- `97c093a` DesignTests: the golden / replay config has every NIGHT12 design flag absent (off), the game's has them on.
- `2a335e7` the x-ray toggle's hint (main.js input:xray).
- `0d597dc` damage numbers (damageNumbers.js, off by default) and SETTINGS' rows for the systems that now exist.
- PORT-STATUS.md and PORT-PLAN.md section 2 updated.
- `f95b707` COPY SCORE LINE on RESULTS (menus.js copyLine).
- A second read-only agent named two of the profile's older allocations: `SurvivalMap.ZoneIndex`'s FindIndex lambda (a
  closure + delegate per call, every step through the power switch's buy check: Buyables' 125 B) and CombatReader's
  `Key[9]` per Poll (R.Input's 68 B); R.Ui's 663 B / 15 allocs it could not find in code (likely the Editor's TMP
  SetText: `#if UNITY_EDITOR` strings). `ea50685` fixes the two. Gate: `tests: 501 / 501 passed, 0 failed`, `replays:
  22 / 22`, `golden tests: 91 / 91` (11:22 PM); harness 0 mismatches.

## Play check 3 of 3: marker costs and the Editor smoke (11:23-11:28 PM)

- The guard held this time: the first `eval_file` answered with a client error but had started; the second call was
  refused ("already started (a retry)").
- **Marker costs** (the bench's setup in Play mode, 300 frames after 240, ProfilerRecorder; Editor frame 4.39 ms):
  SimStep 0.146 ms / frame; R.Feel 0.007, R.Viewmodel 0.040, R.World 0.007, R.Enemies 0.078, R.Projectiles 0.139,
  R.Props 0.050, R.Ui 0.062, R.Signs 0.017, R.Fx 0.010, R.Light 0.037, R.Input 0.008 ms / frame. Tonight's systems
  cost little script time.
- **Editor smoke** (11:25-11:27 PM): `[smoke] 103.6s PASS contract FIRST FIFTEEN (firstFifteen, target 15) completed
  in round 2: contractDone True, profile done True, unlock 'daily' in the profile True (THE DAILY · MOD: INTEREST)`;
  the DAILY steps as in check 2; `[smoke] 107.1s DONE: all steps passed`.

## U3-25.12 close (11:28 PM -)

- **The Windows Player** (23:30:12-23:30:51, the night's only build): `build --target StandaloneWindows64 --outputPath
  Builds/Stage1/SKYGRIND.exe --scenes ["Assets/Scenes/Survival.unity","Assets/Scenes/Arena.unity"] --confirm true`
  (the CLI wants a JSON string array now): `Succeeded`, 0 errors, 520 warnings, 160,215,231 B. `git diff --stat
  ProjectSettings`: only UnityConnectSettings.asset (never staged); Mobile_RPAsset.asset not rewritten; the TMP
  LiberationSans fallback not rewritten. TMP wrote its runtime `_ScaleRatioA / C` into the two SemiBold faces'
  materials: committed once (`git log` after `ea50685`) so later runs leave git clean.
- **INV-smoke** (23:32:28-23:34:27): `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke.log`: every
  step PASS, including `[smoke] 103.6s PASS contract FIRST FIFTEEN ... unlock 'daily' in the profile True`, `[smoke]
  103.6s PASS DAILY run: seed QRSTJY (today's QRSTJY), daily True 2026-10-04, HEAT 1 x1.25, mutators [glassCannon,
  noRegen]`, `[smoke] 107.1s PASS DAILY RESULTS: Results, heat 1, badge 'HEAT 1', counted True ...`; `[smoke] 107.1s
  DONE: all steps passed`; 0 exception lines.
- **-skygrindGolden** (23:34:37-23:34:48): `[golden] golden total: 34 vector sets (31481 records), 22 replays (22
  exact); 0 failing, 0 mismatches`, `DONE: 0 mismatches in 4.2 s`.
- **Bench** (`bash Tools/survival-bench.sh 60`), the plan polled every 2 s:
  - 23:35:35-23:36:47, power seen `13 Silent AC=True`: `median 2.74 ms, p95 4.59 ms, worst 1% 7.62 ms, mean fps 338`;
    `collections 3 in 20263 frames; GC Used Memory rises 57200.0 KB`; FrameTimingManager cpu median 2.74 ms, worst
    1% 7.64 ms; gpu median 1.10 ms, worst 1% 1.42 ms.
  - 23:38:07-23:39:21 (`Logs/player/bench2.log`), `12 Silent AC=True`: `median 2.79 ms, p95 4.32 ms, worst 1% 7.38
    ms`; 3 collections, +57,336 KB; cpu worst 1% 7.40 ms, gpu worst 1% 1.43 ms.
  - **Over the 6.9 ms bar on the Silent plan** (u3-n11's 5.55 ms was on Balanced; the plan was Balanced at 7:07 PM and
    Silent from at least 11:28 PM; the u3-n11 Player's own 15 s bench at 7:40 PM printed 6.20 ms, plan not polled).
    The GPU has room (worst 1% 1.4 ms): the spikes are CPU-side. The plan is not changed by me (a system setting; the
    memory note says GG wants benches on Silent); a monitor waits for a flip to Balanced to rerun the same bench.
  - Read after (01:00 AM, `Assets/Settings/PC_Renderer.asset`): the SSAO feature (Downsample 0, AfterOpaque 0, Source
    1 = DepthNormals, Forward+ rendering mode 2) was already enabled at u3-n11, so its pass and the DepthNormals
    prepass were in u3-n11's bench too; NIGHT12 only made Stone sample it. The new per-frame CPU work is elsewhere
    (PostFx's Volume overrides, LightPools' light moves, EnemyLook's property blocks, animated canvases): HANDOFF D-34.
  - The plan stayed Silent at every check from 23:28 to 01:00 (polled every 20 s); Balanced from 01:02.
  - 01:03:13-01:04:41 (`Logs/player/bench3.log`), `16 Balanced AC=True`: `median 4.49 ms, p95 7.09 ms, worst 1% 10.15
    ms`; **invalid**: GG's other project's scheduled jobs had started at 01:00 (`patric2` `scripts/nightly.py`,
    `refresh.py --batch`, `run_batch.py`: two python processes at about 0.9 cores between them in a 6 s sample at
    01:05, Unity Hub at 0.35). Not touched (GG's processes). The Silent runs at 23:35 / 23:38 had no such load (a 5 s
    sample at 23:41: no python above 0.2 s). Rerun when the batch ends, on whatever plan is active, polled.
  - The batch never ended in time: six of its processes at every check from 01:15 to 03:59 (every ~9 min), and an
    8 s sample at 02:16 showed `llama-server` at 71.7 CPU-s (about 9 cores; 68.7 at 04:00). **Decision (04:00): the
    valid bench is the two Silent runs (7.62 / 7.38 ms); the bar is reported short**; rejected: switching the plan
    myself (a system setting, and the memory note's Silent requirement), a rebuild to tier an effect (the 1-build cap),
    stopping GG's jobs (not mine), and benching under load.
- **Status at the first close commit (`242bef3`, 04:02):** U3-25.2 and U3-25.12 `in_progress` on the bench alone.
- **GG's jobs went idle at 04:16** (a per-process sample: python + llama-server 0 CPU-s in 5 s at 04:17:17; the batch
  processes still exist, idle). The plan was Balanced (u3-n11's condition). **Bench 04:17:35-04:18:48**
  (`Logs/player/bench4.log`), power seen `16 Balanced AC=True`: `median 2.43 ms, p95 4.19 ms, worst 1% 6.90 ms, mean
  fps 368, median fps 411`; `collections 3 in 22033 frames; GC Used Memory rises 58484.0 KB`; FrameTimingManager cpu
  median 2.43 ms, worst 1% 6.93 ms; gpu median 1.19 ms, worst 1% 1.54 ms. Quiet again at 04:19:12 (0 CPU-s). **At the
  6.9 ms bar** at the printed precision (two decimals), on the plan and load u3-n11's 5.55 ms was measured with; on
  Silent the same build measures 7.38-7.62 ms (over). Both are reported (NIGHT12-REPORT, HANDOFF D-34).
- **A pre-registered series (04:27-04:32)**, because 6.90 ms is the bar only to two decimals and the stop hook asked for
  an unambiguous number: three back-to-back 60 s runs, each after the quiet check (all `quiet=True jobsCpuIn5s=0
  plan=Balanced`), each `power seen: 16 Balanced AC=True`, all three reported whatever they print, decided by their
  median. `Logs/player/bench5.log` 04:28:02-04:29:12: `median 2.55 ms, p95 4.19 ms, worst 1% 6.84 ms, mean fps 362`
  (FTM cpu worst 1% 6.87 ms); `bench6.log` 04:29:30-04:30:40: `median 2.47 ms, p95 4.23 ms, worst 1% 7.00 ms, mean fps
  362` (FTM cpu 7.05 ms); `bench7.log` 04:30:58-04:32:08: `median 2.46 ms, p95 4.00 ms, worst 1% 4.64 ms, mean fps 376`
  (FTM cpu 4.65 ms). Each: GC collections 3, GPU worst 1% 1.51-1.57 ms. **Median of the series: 6.84 ms (under the
  bar).** All four clean Balanced runs: 6.90 / 6.84 / 7.00 / 4.64 ms (three at or under the bar, one over; median 6.87
  ms). Silent (23:36 / 23:39): 7.62 / 7.38 ms, over. The worst 1% swings by 2.4 ms between identical runs: a single
  run doesn't decide a 6.9 ms bar (HANDOFF D-34; memory note updated).
- **A pre-registered Silent series (04:36-04:40)**, because the stop hook read the bar as plan-free and the memory note
  says GG benches on Silent (the two 23:3x Silent numbers were single runs): the lead set the plan to Silent
  (`powercfg -setactive`, 04:36:02, a 20 s settle), three 60 s runs each after the quiet check (all `quiet=True
  jobsCpuIn5s=0 plan=Silent`), each `power seen: 16 Silent AC=True`, all reported, decided by the median, then restored
  Balanced (04:40:36, the state found). `bench8.log` 04:36:34-04:37:44: `median 2.49 ms, p95 4.20 ms, worst 1% 7.13 ms,
  mean fps 361` (FTM cpu 7.15 ms); `bench9.log` 04:37:56-04:39:07: `median 2.58 ms, p95 4.24 ms, worst 1% 7.25 ms, mean
  fps 355` (FTM cpu 7.31 ms); `bench10.log` 04:39:18-04:40:28: `median 2.56 ms, p95 4.17 ms, worst 1% 7.19 ms, mean fps
  358` (FTM cpu 7.22 ms). GPU worst 1% 1.51-1.54 ms. **Median 7.19 ms: over the 6.9 ms bar on Silent.** All five clean
  Silent runs: 7.62 / 7.38 / 7.13 / 7.25 / 7.19 ms.
- **Status at the close:** U3-25.1, U3-25.3 .. U3-25.11 `passing`; **U3-25.2, U3-25.12 and U3-25 `in_progress` on the
  bench alone**: at the bar on Balanced (median 6.84 ms), over it on Silent (median 7.19 ms), GG's bench plan. Tiering
  the costliest effect needs a second build, over the night's cap of one: reported short. (They were set `passing` at
  04:25 on the Balanced numbers and set back at 04:45.) Captures 15 of 12 (slip); Play checks 3 of 3; Player builds 1 of 1. The tag `u3-n12`, set
  at 04:02 on `242bef3` before this bench, is moved to the final close commit (deleted and set again: one `u3-n12`
  tag, on the commit that closes the night; logged as a deviation from "created once"). It was moved to `ad9509b` at
  04:25, to `97e1ea6` at 04:35 (the Balanced series) and to the commit recording the Silent series at 04:45 (the same
  deviation, three times).
