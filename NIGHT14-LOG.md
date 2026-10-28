# NIGHT14-LOG: THE GAME PLAYS ITSELF

Run started 2026-10-04 9:58:52 PM (first `date`). The close starts by hour 10:30 (8:28 AM on Oct 5); 12 hours ends
9:58 AM. GG's goal message: work U3-27.1 .. U3-27.13 in NIGHT14.md section 5's order with background agents split by
file ownership; never ask, never stop early; the close table with every row DONE or SHORT, then `NIGHT14 CLOSED`. Caps
(NIGHT14.md section 3): Play checks 3, captures 10, Player builds 5, bench runs 18, farm 4 processes / 3 h, builders 7,
reviewers 2. Times below are from `date`.

**The conflict, logged once:** HANDOFF section 4 says "NIGHT14 = the POWER guns"; NIGHT14.md (written 9:50 PM at GG's
request) moved that night to `NIGHT15.md` as it was. NIGHT14.md wins.

## RECEIVED (U3-27.1, 9:58-10:20 PM)

| Row | Command | Result |
|---|---|---|
| 1 | `git rev-parse --short HEAD`, tags | HEAD `18fd426` on `port/stage3`, tag `u3-n13` on it: as expected |
| 2 | `git status --short` | ` M ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT7.md` .. `?? NIGHT15.md`: as expected (NIGHT15.md is the moved POWER guns night) |
| 3 | `bash Tools/unity-loop.sh status` | `"status": "ready"`, `"playMode": "stopped"` |
| 4 | `bash Tools/unity-loop.sh tests` (10:05 PM) | `tests: 594 / 594 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 91 / 91` |
| 5 | `dotnet run --project Tools/CoreHarness -c Release` | `golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| 6 | `SKYGRIND.exe -skygrindSmoke` (build 5) | `[smoke] 125.3s DONE: all steps passed`, 0 exception lines (`Logs/player/smoke-n14-arrival.log`) |
| 7 | `-skygrindGolden`, `-skygrindFingerprint -batchmode -nographics` | `[golden] golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches`; `[fingerprint] seed 1: 12 hashes over 7200 steps in 0.39 s: EQUAL to the baseline; at the end: run Fight round 4, kills 36, shots 80, earned 4310, alive 0, owned 2, flow Playing`; seed 2 (red) 0 of 12 equal |
| 8 | `powercfg /getactivescheme`, AC | Balanced (381b4222-...), on AC (PowerOnline True) |
| 9 | idle seconds, CPU, GPU (10:18 PM) | idle 6 s (GG had just typed the goal), CPU 21-26 %, GPU 0 % |

- **GG's answers (HANDOFF section 5):** none new. D-06, D-34, D-38 .. D-52 keep their defaults; nothing in them changes
  tonight's plan. D-34 / D-39 set the bench bar for the final series (6.9 ms p99 on Silent, UI-hurt).

## Measured before building (U3-27.1, 10:05-10:25 PM)

**The web bot's real API** (`src/bot/botBrain.js`, read in full):
- `createBotBrain(config, self {mover, health, loadout}, {getPlayer, getCollider, getGraph, getOrbs, rng = Math.random})`
  (:23). `getPlayer()` returns `{position, velocity, health {dead}, crouched}`; `getCollider().rayIntersect(ray)` returns a
  hit with `distance`; `getGraph()` the nav graph or null; `getOrbs()` `[{position, active}]`.
- The brain object: `difficulty`, `state` ('chase' | 'engage' | 'evade' | 'recover'), `look {yaw, pitch}`, `visible`,
  `stats`, `plan`, `reset(difficulty)` (:71), `onPlayerFire(origin, dir)` (:97), `step(frame, dt)` (:176), `override`,
  `follower`, `pickWeapon`, `directChase`, `setOverride(goal)`, `clearPath()`, `navStats {plans, stuck}` (:339-361, the nav
  override `navOnly`), `face(frame)` (:379).
- `step` writes the web's one InputFrame: `moveX / moveZ / yaw / pitch`, the `jump / dash / slide` buttons, `fire`
  (held + pressed) and `weapon` (a slot). The port splits that frame into `InputFrame` (movement, Mover.cs:25) and
  `CombatInput` (Combat.cs:160): the bot writes both.
- It reads `self.loadout.weapon` as a weapon id: `b.preferredRange[weapon] ?? b.preferredRange.revolver` and
  `weapon === 'shotgun' ? shotgunRange + 2 : config.weapons[weapon]?.range ?? config.weapons.revolver.range` (:272, :391):
  the duel's two guns. In SURVIVAL every other gun falls to the revolver's numbers (the web's own fallback).
- `rng()` draws, all conditional (short-circuit order matters): 2 per aim-error reroll (every `errorReroll` 0.35 s), 2-3 per
  strafe switch in ENGAGE, 1 per step for the slide-jump roll (only when grounded, moving in, past the gap, fast), 1 per
  step for hard's wall jump (only airborne at a wall). Its own Mulberry32 stream from the run seed (rule 8).
- `config.bot` (exported): reaction easy 0.4 / normal 0.25 / hard 0.15 s, `aimErrorMult` 2 / 1 / 0.5, `turnRate` 540,
  `losInterval` 0.1, ... `config.nav` (exported: 40 keys) and `config.kiosk` (`idleReset 60, orbitSpeed 0.12,
  orbitRadius 16, orbitHeight 9, lookHeight 2`).
- The attract loop (main.js :701-740): a fresh neutral frame each step, `frame.weapon = loadout.targetSlot`,
  `attractBrain.step(frame, 1 / simHz)`, RIVAL's HP reset under 30 % ("the show goes on"), `player.setGodMode(true)`,
  difficulty 'normal', the 'spire' map with core preset `duelPreset`. The flythrough (main.js :2337-2343): `angle +=
  frameDt * orbitSpeed`, target the two fighters' midpoint at `lookHeight`, camera at `orbitRadius` / `orbitHeight`.

**Where navGraph.js needs the arena** (`buildNavGraph(map, collider, config, core, grid)`, :40):
- `map.boxes` (min / max / kind; node tops on the kinds in `config.nav.walkWalkers`), `map.floorSize`, `map.pads` (x, y,
  z, target), `map.core` (x0, z0, cells, cellSize); `collider.rayIntersect`, `capsuleIntersect`, **`sphereIntersect`**;
  `core.heights`, `core.topAt`; `grid.snapshot` (count, linkStart, linkKind, linkTo, y) with `grid.nodeX / nodeZ`.
- **The web builds it for the duel 'spire' only** (navWorker.js; main.js :301 posts `map: 'spire', core:
  config.core.duelPreset`). `PortExport/maps/spire.json` already holds that graph (`duelNavGraph`: 1013 nodes, stats
  `flights 38554, gridLinks tried 307 / added 266`, preset `towers`): an oracle for the graph exists before p22.
  `survival.json`'s `duelNavGraph` is null.
- **The port's map data holds what it reads:** `SurvivalMap` has `boxes` (MapBox min / max / kind: 96 for survival),
  `floorSize` 96, `pads` 8, the core spec, the raw `navGrid` (11228 nodes, links by kind); spire.json has 90 boxes.
  **Missing in the port:** a `sphereIntersect` on the collider (three's `Octree.sphereIntersect`): agent N adds it to
  Core beside the octree's capsule test. SURVIVAL's collider also carries the live gate layers (box layers): the graph
  is built on what the collider holds at build time.
- Its config reads go through movementMath.js (gravityFor, jumpVelocity, dashSpeed, gravityScale, verticalStep,
  stepsFor), all ported in `MovementMath`.

**How `onScreen` is called per step:** one call site, `HuskBrain.cs:962`, in the per-enemy stuck watchdog, only when
`wants && r.watch.CanRelocate(relocateAt)`: 0 .. N calls a step, in the brain's enemy order. The tape records each answer
in call order (a bit stream), or pins it.

**The sim's other non-input influences** (read in `SurvivalSim*.cs`):
- per step: `AssistMode` (`settingStr("aimAssist")` through `assistModeFor`, which follows the device in use: it can
  change between steps), `settingNum("snapStrength")`, `BreakawayCap()` (the pad's);
- per hurt: `settingBool("damageDirection")` (only the hit arcs: view state);
- at a run's start: `lab.Read(settingBool)`, `SettingOr("onboarding")`, `DateTime.UtcNow` (the DAILY's seed, day and
  twist), `DateTime.Now.Ticks` (a seed when none is given), and the profile (`records.Best` for the round-10 mod unlock,
  `contracts.Unlocked`, `heat.Unlocked`, the onboarding state);
- `keyLabel` (the tips' text only: no state), `DateTimeOffset.UtcNow` in EndRun (the record's date: meta only).
- So a tape header holds: seed, setup (mode, HEAT), the LAB toggles, onboarding, the DAILY day, the unlock flags it
  read; and per step the input, the look, the assist mode and cap when they change, and the onScreen bits.

**U3-27.1 DONE (10:25 PM):** `71cf9aa` (RECEIVED, the facts, tasks.json U3-27.*).

## The shared types (10:25-10:36 PM)

- `e828821`: Core `Bot/BotTypes.cs` (BotFrame: the web's one InputFrame split as the port splits it; BotTarget, BotOrb,
  BotLook, BotRay, BotSelf, BotDeps with a Mulberry32 rng; IBotBrain), `Nav/NavTypes.cs`, stubs BotBrain / Astar; Game
  `SurvivalSim.Hooks.cs` (StepEnded / RunStarted, UseInput, metaSilent honoured by Recording, the records' judging and
  the run log; the utcNow clock StartRun reads; `SurvivalSim.Headless`), stubs AutopilotInput / Farm / BotNav / RunTape /
  FlightRecorder / RouteTrack, `SurvivalGame.Night14.cs` (attractOn, UiFlow, the partial hooks called from the frame
  loop), Ui's per-agent hooks and RESULTS' SAVE CARD / COPY SEED (hidden), the RECAP settings row, PlayerChecks
  `-skygrindFarm` / `-skygrindReplay`, UnityTuning partials, `design.autopilot / attract / farm / tape / recap / route /
  ghost` with their `_why` (the Player copy byte-identical). Gates: compile clean; `tests: 594 / 594 passed`, `replays:
  22 / 22`, `golden tests: 91 / 91` (10:33 PM).
- **New tool, `Tools/SimHarness`** (not in NIGHT14.md; a decision): SurvivalSim + Core compiled outside Unity with dotnet
  (CoreCLR), so headless runs take seconds without the Editor (the agents can't use the Editor). `fingerprint seed 1: 12
  hashes in 0.52 s: EQUAL to the baseline` (the Mono baseline holds on CoreCLR); `twice: run 1 vs baseline EQUAL; run 2
  vs run 1 first difference at checkpoint 1` (HANDOFF section 3's finding reproduced in 6 s). Rejected: agents running
  everything through the lead's Editor (one Editor, serial, 5 s eval timeouts).
- `4aab8a5`: AutopilotInput's stub gains HasTarget / TargetPosition / Enabled / TakeDraft (agent K compiles against them).
- **Agents launched at 10:40 PM** (builders 7 of 7): R (U3-27.3, the p22 oracle, web repo `port-export-p22/`), and in
  worktrees from `4aab8a5`: N (U3-27.4 the bot in Core + SphereIntersect + BotNav), A (U3-27.5 / .6 the autopilot, a
  stand-in brain until N lands, the farm, farm-report.cjs), K (U3-27.7 kiosk + attract), T (U3-27.8 the tape), V
  (U3-27.9 WHAT GOT YOU), V2 (U3-27.10 / .11 the route, the card, the ghost). Each brief: its files, its hooks, tests
  first against the stubs, no Editor, no .meta files (the lead's Editor makes them).
- **Decision: A starts now, not after N** (NIGHT14.md: "A after N's stubs"; the stubs are in the shared commit). A
  builds the adapter / shopper / farm on its own stand-in brain behind IBotBrain and switches to N's BotBrain when it
  lands. Rejected: A idle for ~2 h.

## U3-27.2 a clean slate per run (the lead, 10:42-11:12 PM; systematic-debugging)

- **Reproduced:** `twice: run 1 vs baseline EQUAL; run 2 vs run 1 first difference at checkpoint 1` (SimHarness, 6 s).
- **Evidence by system** (`Tools/SimHarness` `slate`: each system and each Mulberry32 stream hashed on its own right
  after StartRun and after every step of both runs): `first difference right after StartRun: enemyIds, loadout,
  simStep, rng.director, rng.directorPace`. So the leak is at the run's start, before any step:
  1. **The enemy pools and ids:** `Husks.Spawn` reuses the first dead enemy of its kind's pool and the registry keeps the
     last run's dead in creation order plus its id counter; a second run reused them in another order, and the
     registry's order is the order every system walks the enemies in (the crowd pass, the brain, nearest-enemy ties).
     The web has the same carry-over in one page (its pools and ids persist).
  2. **The loadout's shot ids:** weapons.js `shotSeq` (a closure counter the web never resets either).
  3. **The director's two streams** held the last run's state until round 1's `StartWave` reseeds them.
  4. `simStep` (sim time since boot) differs by design: only time differences are read from it (not compared).
- **Fix** (`ec47c50`, port-only, logged in each file): `SurvivalSim.CleanSlate()` right after ResetWorld in StartRun:
  `Husks.ClearPools()` (the pools emptied, `EnemyRegistry.Reset()`), `loadout.shotSeq = 0`; `Director.NewRun` puts its
  streams back to the constructor's seeds. Each is already in that state on a fresh composition, so **the first run is
  unchanged and the fingerprint is not re-recorded**. Rejected: a fresh composition per run in the Game (it would rebuild
  every view's wiring and the EventBus registrations: an overhaul).
- **Gates:** `slate: no section differs over 7201 points`; `twice: ... run 2 vs run 1 EQUAL (12 of 12)`; `fingerprint seed
  1: ... EQUAL to the baseline`; CoreHarness `0 failing, 0 mismatches`; the test
  `FingerprintTests.TwoRunsOnOneComposition_PrintTheSameHashes` (`a78cd7b`) **seen red** in the Editor with the fix
  stashed (`tests: 2 / 3 passed, 1 failed`), then the full suite `tests: 595 / 595 passed, 0 failed`, `replays: 22 / 22`,
  `golden tests: 91 / 91` (11:00 PM).
- Smoke step 18 (`efd94e8`): on the Player's live composition after every earlier smoke run, seed 1 twice: the same 12
  hashes and the baseline's. Waits for the next build.

**U3-27.2 in_progress (11:12 PM):** everything but the Player smoke step (the next build).

## The machine (11:25 PM - midnight)

- Simple shell commands took 60-120 s from 11:25 PM: **the commit charge read 142.6 GB of 145.6 GB** (free RAM 1.2 GB). The
  holders: GG's llama-servers (22 GB private; a new one started 9:50 PM), the Editor (11.3 GB), **two Unity asset import
  workers alive since Oct 2 with 14.4 GB and 5.7 GB private** (mostly paged out), Chrome, CapCut (GG started it at
  11:02 PM: GG is at the machine), seven agents' dotnet builds.
- **Decision:** the Editor's own API shut its idle import workers down (`EditorUserSettings.desiredImportWorkerCount = 0;
  AssetDatabase.ForceToDesiredWorkerCount()`, the setting restored to 6 after): `commit 126.0 GB of 145.6 GB`, four fresh
  workers at ~530 MB each. No process was killed (CLAUDE.md rule 10; GG's jobs untouched). Rejected: killing the worker
  processes by PID (the Editor's children: too close to rule 10), or waiting for the limit (an allocation failure could
  take the Editor or GG's jobs down). An earlier `UnloadUnusedAssetsImmediate` + GC eval timed out on the main thread
  (5 s) but the Editor's private bytes read 11.3 GB after it.
- GG is using the machine: Play checks are batched into as few runs as possible (CLAUDE.md workflow).

## U3-27.3 the p22 oracle (agent R, 10:40 PM - 12:06 AM)

- `C:\Users\gaige\dev\skygrind\port-export-p22\` (untracked; the web repo's status shows only it new): p20's tooling plus
  `scenes.mjs`, `navSets.mjs`, `navGraphSet.mjs`, `botBrainSet.mjs`, `p22.mjs`, `compareDuel.mjs`, `mutate.mjs`, an
  extended `verify.mjs` and a README section H with the exact formats and step order. `config.json` byte-identical to
  the Unity repo's `PortExport/config.json` (`cmp`); p22's regeneration of p20's 61 files byte-identical to p20's.
- The sets: `astar.json` (9 graphs, 526 path records (32 null), 198 nearest; ties, one-way and parallel edges, an
  unreachable goal, the spire graph), `reach.json` (473 records, 116 return 0; each kind's peak dy and one ulp above),
  `navGraph.json` (spire 1013 nodes / 11229 edges; **survival 848 nodes / 9652 edges**), `botBrain.json` (6 sessions x
  1200 steps: normal x3, hard x3; states reached: every session chase / engage / evade, normal-3 RECOVER without an orb,
  hard-6 with one; fires, jumps, dashes, slides; GROUND / AIR / DASH / SLIDE / WALLRIDE; rng mulberry32(seed) after the
  scene, Math.random made to throw), `follower.json` (12 sessions, 4482 steps: one path per edge kind + 4 random; 11
  arrived, 1 stuck).
- `node port-export-p22/scripts/verify.mjs`: everything p20 checks plus astar `724 calls reproduced exactly, 0
  mismatched`, reach `473 ...`, botBrain replayed from the file `1200 of 1200 steps reproduced exactly` x6, all five
  regenerated and compared value by value: **`total: 0 mismatches`**. **Mutation test** (`mutate.mjs`, one ulp in one
  value per file, then restored byte-identical): astar 2, reach 2, navGraph 1, botBrain 2, follower 1 mismatches: each
  run exited 1.
- **The spire graph rebuilt by the web code equals `PortExport/maps/spire.json`'s `duelNavGraph`** (0 nodes, 0 edge
  lists differ). **navGraph.js builds on the Survival map unchanged: 3.1 - 7.7 s in Node** (core flat, every zone open:
  the graph reads no zone tags). So U3-27.4's fallback isn't needed; the build is over 1 s: cached by content hash.
- R's format additions relayed to N by message (12:12 AM). The C# checkers are N's (U3-27.4).

## U3-27.8 THE TAPE (agent T, 10:40 PM - 12:20 AM; integrated 12:20-12:33 AM)

- Cherry-picked: `1cec97f` (T's tests first: TapeTests against throwing stubs) **seen red** in the Editor (4 failing with
  `NotImplementedException`, 00:22), then `04342b0` (RunTape, TapeBuffer, TapeRecorder, TapeReplay / TapePlayer,
  TapeScript, TapeFiles; Harness.Tape.cs) and `4f487db` (SurvivalGame.Tape.cs BootTape, UnityTuning.Tape.cs, COPY
  REPORT's tape line); `fb7c9a6` the .meta files. TapeTests `tests: 6 / 7 passed, 0 failed, 1 skipped` (the AUTOPILOT
  tape waits for U3-27.5).
- **The format (version 1, `.sktape`):** a header (the SKTAPE mark, the version, the build stamp, the baseline id, the
  seed, the mode, the HEAT rung, the UTC clock StartRun read, noRecords / noLab / metaSilent / onboarding / damageDirection,
  the 6 LAB bits, the health at the first step, the profile's JSON at the run's start, the step count, how it ended, the
  first exception) and ops: STEP (a flag byte of what changed, then only those fields: moveX / moveZ raw doubles, the
  look, the 10 movement + 10 combat button bits, the weapon, the grapple target, the onScreen answers as packed bits, then
  a 32-bit light hash of the step), STEP_ABORTED, ASSIST (mode / snapStrength / cap when changed), TOGGLE (a lock between
  steps), DRAFT_OPEN / PICK / REROLL, MOD_GIVE, END_RUN, CHECK (SimFingerprint.Hash every 600 steps and at the end).
- **Measured** (SimHarness, CoreCLR): `tape-scripted`: the taped run's 12 checkpoints are the baseline's (the recorder only
  observes); `162535 B for 7200 steps (1 min of play): 162535 B per minute (158.7 KB/min)`; `replay on a fresh composition
  (0.40 s): EQUAL (12 of 12 checkpoints, 7200 steps)`; the recorder allocates 0 B per step (13,232 B over 7,140 steps,
  all from the 12 checkpoint hashes). T's runs: `tape-old` `REFUSED: tape format 0, this build reads format 1 only`;
  `tape-red` (seed 2 in a seed-1 header) `DIFFERS at step 480 (checkpoint 1 of 3)`; `tape-busy` (a DAILY on a pinned
  clock, HEAT 1, STYLE RAMP, onboarding, 5 assist changes, 6 lock toggles, a draft) EQUAL 12 of 12; `tape-onscreen`
  (every onScreen answer inverted: the run differs from checkpoint 9) both EQUAL on replay.
- **Recorded, or proved not to matter:** onScreen (a tap on brain.onScreen), the assist mode / snapStrength / cap (read at
  each SampleMove), the clock (a tap on utcNow), the seed, the LAB toggles, onboarding, damageDirection (view state only),
  the profile, noRecords / noLab / metaSilent, a health set by automation before the first step; hitstop (nothing reads
  it inside a step), RideCore (the core stays flat in SURVIVAL), keyLabel (text) and EndRun's clock (the record's date)
  proved not to influence the run.
- **T's decisions** (rejected alternatives in its report): a 32-bit light hash every step so a replay names the first
  differing step; between-step operations observed on the EventBus; bot runs (metaSilent, the attract) get no live tape;
  automation tapes to `temporaryCachePath/tapes`; a player's tape beside its run log while RUN LOG is on (the newest 20);
  a resumed run gets no tape (it starts from a snapshot); no compression (WebGL has no zlib; 159 KB/min).
- **Observation (not tonight's scope):** on CoreCLR the sim allocates ~1.29 MB over 7,140 steps (~180 B / step) in the
  fingerprint's run; the Editor's ZeroAllocTests (Mono, 30 enemies, the tested systems) still pass. HANDOFF risk.
- Smoke step 19 (`66713cb`): a short run taped and replayed in-process; its path logged for `-skygrindReplay`.

**U3-27.8 in_progress (12:33 AM):** the headless proof is in; the Player smoke tape + `-skygrindReplay` EQUAL wait for
the next build.

## U3-27.10 / U3-27.11 THE ROUTE, the SHARE CARD, the GHOST and SPLITS (agent V2, 10:40 PM - 12:38 AM; integrated 12:40-12:46 AM)

- Cherry-picked `6750a2e` (V2's tests first) **seen red** (`RouteTrackTests 0 / 8`, `GhostStoreTests 0 / 10`, 00:42),
  then `cf9a732` (U3-27.10), `61a134a` (U3-27.11), `0e0f1f7` (automation hooks, the ghost hidden in DOWN): `RouteTrackTests
  tests: 8 / 8 passed`, `GhostStoreTests tests: 11 / 11 passed` (00:45).
- **RESULTS:** a plate right of the tally's buttons and under the PAYOFF strip (x +40..+900, y -262..-530 at 1080p): the
  arena from above (box footprints shaded by height, the zones' borders, the core, the outline in barrier.boundaryColor),
  the path on a cyan ramp anchored on hud.speedMid / speedHigh, round ticks, kills as gold dots, the start ring, a cyan
  exfil ring, the DOWN as a red-orange X; a legend; SAVE CARD and COPY SEED inside the plate. Drawn once per RESULTS (base
  layer 45 ms, draw 4 ms in V2's .NET check of a 12,000-step run).
- **The card:** 1200 x 630 PNG to `<My Pictures>\SKYGRIND\SKYGRIND-<seed code>-<local time>.png` (persistentDataPath when
  there is no Pictures folder): the route at 570 px, the wordmark, the outcome (DAILY / HEAT when they apply), ROUND and
  grade, SCORE, BEST RANK, TIME, TOP GUN, kills / headshots / accuracy, SEED, the build stamp; an off-screen canvas and
  camera through URP's SingleCameraRequest. COPY SEED: the seed code on the clipboard and a hint.
- **The ghost:** best route per seed in `<persistentDataPath>/ghosts/<seed code>.bin` (at most 50, the oldest dropped;
  written through a .tmp), "best" = the higher round begun, then sooner; nothing stored for bot / LAB / resumed /
  noRecords / profileReadOnly runs unless `GhostStore.FolderOverride` is set (automation). The runner: the player's
  capsule in cyan at alpha 0.3 with a 1.5 s trail sampled from the best's path, PLAYING / PAUSED only, on the run's clock.
  **Splits:** "ROUND n · -x.x s" in cyan when ahead, "+x.x s" in the muted text otherwise, 4 s, under the round line
  (ASCII hyphen: U+2212 is not in the static TMP atlas and would rewrite the fallback asset).
- V2's decisions (rejected alternatives in its report): gold kill dots (not red-orange: a map of danger; not cyan: the
  path), the run's clock for the track, the ghost needs design.route on, heat stored but not compared.
- Not checked by V2 (the lead's Editor / Player checks): the plate's layout and overlaps, the card's render, the
  ghost's look, the split line's position.

## U3-27.4 the bot in Core (agent N, 10:40 PM - 12:47 AM)

- N's commits (base `4aab8a5`): `9ffa788` tests first (BotNavTests: tests/nav.test.js ported; BotZeroAllocTests; throwing
  stubs; `PortExport/maps/spire-duel-navgrid.json` + `PortExport/scripts/navgrid-duel.mjs`), `ef43b47` Reach, `9d608f7`
  Astar, `a717ce0` Follower, `4fbfaa0` SphereIntersect (ThreeOctree / CoreGrid / BoxLayer / GameWorld: new methods only),
  `c094563` NavGraph + NavMap, `3b5f74a` the p22 vectors, `dd90df4` BotBrain, `5d007f7` the p22 checkers (GoldenBot.cs,
  `AddBot` in Golden.cs), `7a07a31` BotNav + harness commands, `ac47357` the harness's `follow`.
- **N's harness (CoreCLR):** `golden PASS astar: 724 records, 0 mismatches`, `botBrain: 7200 records`, `follower: 4494
  records`, `navGraph: 3722 records` (spire and survival both built), `reach: 473 records`; `golden total: 39 vector sets
  (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`; the fingerprint EQUAL at every Core commit.
- **The spire graph:** `1013 nodes, 11229 edges, stats flights 38554, gridLinks 307 / 266, built in 2940 ms; PASS spire
  duelNavGraph: 2026 records, 0 mismatches`. Found: spire.json's own navGrid is the core-'flat' grid while navWorker.js
  builds the duel graph on the core-'towers' grid, so N exported the towers grid with V8 (the web's own modules from a
  scratch folder) as `maps/spire-duel-navgrid.json`.
- **The Survival graph:** navGraph.js builds there unchanged: 848 nodes, 9652 edges, flights 34097, gridLinks 196 / 156,
  equal to R's (`1696 records, 0 mismatches`); **2951 - 3672 ms on CoreCLR** (Mono not measured): over 1 s, so
  `BotNav.Survival` caches it by content hash (in memory ~2 ms; on disk with `BotNav.CacheDir`: 55 ms). It is built from
  the map data alone (flat core, the nav grid, no live gate layers): **it crosses locked zones**; `BotNav.Masked(full,
  open)` gives the open-zones view.
- **Zero allocation:** `SimHarness bot-alloc`: `BotBrain.Step: 0 B` (through chase, engage, evade, recover),
  `Follower.Step: 0 B`; the Unity tests BotZeroAllocTests (2) to run in the Editor.
- **The web's quirks ported as they are** (N's list): reset() keeps errTimer / errYaw / strafe / evadeSide / look /
  lastSeen across rounds; line of sight and hideSpot use the target's current chest, the aim the delayed one; planGoal set
  even when A* fails, and a failed CHASE plan keeps the old path (two NavPath buffers keep that); the shotgun's range is
  shotgunRange + 2, every other gun the revolver's 200 m and preferred 12 m; look.pitch x0.9 per step while chasing
  unengaged; A* ties by heap order; pad nodes keep only walk / drop + the pad edge; link nodes get no airborne candidates;
  three's triangleSphereIntersect tests containsPoint on the sphere's centre; nearestOrb uses distanceTo, elsewhere hypot.

## Integration of V, N, K, A (12:46 - 1:41 AM) and the Editor hang

- **V (U3-27.9):** `056b8aa` (V's tests first) **seen red** (`FlightRecorderTests 0 / 8`, 00:45), then `f9ea104` (the
  flight recorder), `758c78c` (WHAT GOT YOU on DOWN: SurvivalGame.Recap.cs, RecapStage.cs, UiRecap.cs), `1694f88` (flag
  off: no UiRecap). The green run is still owed (the hang below).
- **N (U3-27.4):** `474825c` (N's tests first, with `PortExport/maps/spire-duel-navgrid.json`), then N's ten
  (`f510d39` checkers ... `d472726`).
- **K (U3-27.7):** `42dd2d7` (K's tests first: KioskTests 16 against a throwing stub; red in K's scratch NUnit runner, 0 / 16),
  `ff35911` Kiosk.cs, `25c0c42` the kiosk's TITLE / PAUSED, `07bfdd3` the attract loop and the flythrough.
- **A (U3-27.5 / .6):** `2599feb` (A's tests first), `52f894f` the AUTOPILOT + THE FARM on the stand-in, `20108ca`
  farm-report.cjs, `e2c116d` AutopilotNavView, `6a65430` the default brain = N's BotBrain (+ two navigation fixes found by
  tracing). A's own cherry-picks of N's commits were skipped (already on main).
- `8a2813a` the .meta files the Editor made at 12:50 AM; `eb71610` the wiring the agents asked for (AttractYield first in
  StartRun / ResumeSave; Farm.BuildStamp; BotNav.CacheDir + the graph pre-built on a worker at boot; Sync Port Data's
  spire-duel-navgrid.json; Tools/farm-batch.sh).
- **Gates without the Editor (1:32-1:41 AM):** CoreHarness `golden PASS astar: 724 records, 0 mismatches`, `botBrain: 7200
  records`, `follower: 4494 records`, `navGraph: 3722 records`, `reach: 473 records`; **`golden total: 39 vector sets
  (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`**; SimHarness `fingerprint seed 1: ... EQUAL to the
  baseline`; `twice: ... run 2 vs run 1 EQUAL (12 of 12)`; `tape-scripted ... EQUAL (12 of 12 checkpoints, 7200 steps)`;
  **`autopilot-twice seed 1 normal (botBrain): EQUAL (152 of 152 hashes, the JSONL lines equal); run 1 round 14 762.1 s
  ACID, run 2 round 14 762.1 s ACID`**; `dotnet build` of Skygrind.Game.Tests / Skygrind.Core.Tests /
  Assembly-CSharp-Editor (the Unity csproj files turned to folder globs by Tools/wt-csproj.cjs): `Build succeeded`.
- **THE EDITOR HANG (from 12:50:30 AM):** after the compile that took V's implementation and N's tests first (the DLLs
  written 12:50:26), the Editor's main thread never came back: every Pipeline call `Main thread operation timed out
  after 30000ms`; the window `IsHungAppWindow True`; a busy dialog "Hold on (busy for ...)" reading **"Waiting for Unity's
  code to finish executing. MonoCompiler.Tick"**; the main thread in `Wait / UserRequest` at ~6 % of a core, no disk
  reads; the import workers' logs end at their "Prepare" (12:50:18); no compiler process alive. The Editor log ends with
  the compile's warnings and the Pipeline's timeouts: the domain reload never began. Our code has no reload hooks; the
  pipeline package stops its HTTP server in `beforeAssemblyReload` (HttpListener.Close on the main thread) while the
  server kept taking my CLI calls (hypothesis, not proven). Tried: the window brought to the foreground (it revived the
  Editor once at 1:13 AM: an unfocused Editor stalls, CLAUDE.md; not the second time), the busy dialog's Cancel. Not done:
  killing the Editor (CLAUDE.md rule 10) or my own stuck CLI call (the permission classifier refused it). A terminal
  notification went to GG at 1:35 AM. **Without the Editor there are no EditMode runs, Player builds, Play checks or
  captures; the dotnet harnesses carry the gates meanwhile.** A candidate contributor, not proven: at 11:54 PM the lead
  forced the import workers to 0 and back to 6 (to free 16 GB); after the 12:50 compile the Editor sent its workers
  "Prepare" (their logs end there) and may wait on that exchange. Logged in memory (unity-editor-play-mode-recompile).
  The Editor was the foreground window from 2:00 AM and stayed hung (2:26 AM: "busy for 1:36:03").
- **Decision (3:06 AM): no second Unity instance** (a batchmode build from a separate checkout with its own Library) to
  get a Player past the hung Editor. Measured: commit 139.3 GB of 145.6 GB, 950 MB free (GG's batch 13.9 GB, a new
  llama-server 14.3 GB at 3 AM); a fresh import and a build would risk the commit limit under GG's jobs and the hung
  Editor, and CLAUDE.md's workflow forbids starting a second Editor. Rejected for those reasons; the Player-side checks
  stay blocked while the Editor is hung.
- `3f2dbcd` review A-3 fixed too (the tape's buffer starts at 4 MB: no doubling inside a step before ~25 minutes).
- **Slip (the lead's):** agent A's brief did not carry rule 12, and A ran its 20 x 3 table (213 s wall) and its 30-run trial
  farm around 12:45-1:20 AM, partly after 1:00 AM while GG's jobs ran (single harness processes, ~1 core each).
- More review fixes while waiting: `d54b8b0` (SAVE CARD falls back to the profile's folder when Pictures refuses the
  write), `27ff28b` (A-4: RunFingerprint ends a live tape and isn't taped itself).
- GG's jobs, re-measured (2:55 AM): the 1:01 AM `run_batch.py` (pid 62660) at 0.95 of a core, a `--serve-only` from 2:31
  AM, the resident Oct 2 server; the vitest run ended; the GPU 7-16 %. The farm waits for the 1:01 AM batch (rule 12).
- GG's 1:00 AM jobs are running (`python scripts/run_batch.py` from 12:58 / 1:01 AM, a vitest run from 1:07 AM; the GPU at
  78-87 %, llama-server ~0.7 of a core): rule 12 holds the farm until they finish.
- While waiting (1:41-1:52 AM): `4b6402b` smoke steps 20-22 (the autopilot twice in the Player, the attract then a clean
  run, WHAT GOT YOU + THE ROUTE + the card + the ghost and a split), compiled with dotnet; `7fe836f` PORT-STATUS.md;
  `a24364e` PORT-PLAN.md ticks. **Slip:** the PORT-PLAN patch script was written through a bash heredoc (CLAUDE.md rule
  9: patch scripts with the Write tool); it checked both rows existed before writing and applied as intended.

## Reviews (reviewers 2 of 2, read-only, launched 1:37 AM with every build unspent; reported 2:36 / 2:42 AM)

A on allocation, cost, determinism and the rules; B on the new flows as a player hits them (`u3-n13..HEAD`).
- **Integration seam found by the lead first (1:52 AM, `0858a75`):** the attract never called AutopilotInput.Detach (K
  disposed an IDisposable; A wrote Detach), so after an attract the aim assist stayed off for the session (B-2 confirms).
- **A-1 / B-1 HIGH (both):** the attract saved the player's metaSilent / noLab after the first pilot had set them: every
  later run of the session recorded nothing and ignored the LAB. **B-3:** D-pad down during the attract flipped the saved
  X-RAY (RoutePad before FrameKiosk). **B-4:** RESUME hid under the attract (Ui read game.flow); tips / lessons could show
  over TITLE. **B-5:** a kiosk left on SETTINGS / CONTROLS never reset. **A-2 / B-6:** the boot graph build raced
  GameConfig's cache and ran with every flag off. **A-5:** the RECAP row existed with design.recap off. **B-7:** a dropped
  shopping goal stayed dropped for the run (one farm bot bought no gun in 12 rounds; kills by gun then describe a bot
  that rarely shops).
- **Fixed:** `f835ee4` (B-3, B-4's RESUME and the price signs: UiFlow; HotKeys / RoutePad skip the attract), `6f18348`
  (A-1 / B-1 save order, B-4 tips / lessons / momentum arc, B-5, A-2 / B-6 a worker GameConfig + only when the attract can
  run, A-5, B-7 `design.autopilot.shopRetryRounds` 3). Gates: dotnet builds; `autopilot-twice ... EQUAL (156 of 156
  hashes)`; the fingerprint EQUAL.
- **Open (HANDOFF):** A-3 the tape buffer doubles inside a step at ~3 / 6.5 / 13 / 26 / 52 min (16 MB on the LOH at the
  last) and RESULTS copies it twice (~10 MB at 60 min); A-4 (automation only) a fingerprint run during a taped smoke run
  leaves brain.onScreen on OffScreen for the session; B LOW: SAVE CARD has no fallback when the Pictures write is denied;
  the built graph keeps its Builder (a second Octree) alive; the BotNav Mono build time not measured.
- Checked fine by both: no per-step / per-frame allocations in the bot, nav, recorder, route, tape, kiosk, ghost; no
  System.Math transcendentals in Core/Bot or Core/Nav; the bot's stream seeded per run; rule 6 (real HP, no assist, the
  brain's own line of sight); rule 7 for bot runs; the tape's coverage; WHAT GOT YOU's gate, skip, R and flag-off paths;
  the ghost store's rules; the clean slate with RESUME.

## U3-27.6 THE FARM (7:40 AM on)

- **Waiting (1:01-7:40 AM):** GG's 1:00 AM `run_batch.py` (pid 62660) ran until ~7:40 AM; through it a llama-server held
  9-10 cores and the GPU 40-87 %, and the commit charge stood at 139-150 GB of ~150 GB. Rule 12 held the farm. The
  Editor stayed hung throughout ("busy for 6:49:45" at 7:40 AM): no Player, so no `-skygrindFarm` in a Release Player.
- **Decision (7:40 AM): the farm through `Tools/SimHarness`** (the same `Farm.Run`, fresh headless compositions, CoreCLR)
  **at BelowNormal priority**, 3 processes (easy, normal, hard), 100 runs each from seed 1, each waited for by PID.
  Measured first: the 1:00 AM batch ended; a new `run_batch.py` from 6:35 AM ran light (GG's LLM and python processes
  0.83 of a core in total, the GPU 29 %); the commit charge 118.5 of 148.1 GB (11.9 GB free). Rule 12 says to judge GG's
  jobs by their CPU; three farm processes are 3 of 24 cores, and a farm's results don't depend on load (deterministic).
  Rejected: no farm (U3-27.6 is never cut).
- Before it, the report pipeline was checked on agent A's 30-run trial (`farm-report: 30 JSONL lines -> 30 runs`), the
  HTML rendered in a headless browser and looked at (`Logs/shots/n14-farm-trial-report.png`: every chart drawn, the dusk
  palette).
- The harness farm stopped at 117 lines (easy 53, normal 41, hard 23) when the session ended (~7:43 AM); the PC rebooted
  at 7:55 AM. Those lines are kept in `Logs/player/farm-n14-harness/` (gitignored) as the CoreCLR side of the comparison
  below; the Player farm's own logs are in `Logs/player/farm-n14/`.

## After the reboot (7:55 AM on): the Editor back, the Player checks, two smoke findings

- **The Editor:** GG asked whether the night had finished and to open the project. `unity open` (8:02 AM) started Editor
  pid 5908 on `C:\Users\gaige\dev\SKYGRIND-unity` (6000.3.7f1): `ready`, `compilationFailed false`, `consoleErrors 0`; it
  imported and compiled every NIGHT14 commit. The reboot ended the hang (no agent killed the Editor).
- **The full suite, the night's tests' first Unity run (8:10 AM):** `tests: 676 / 677 passed, 0 failed, 1 skipped`,
  `replays: 22 / 22`, `golden tests: 96 / 96` (FlightRecorderTests, BotNavTests, BotZeroAllocTests, KioskTests,
  AutopilotTests and FarmTests included; the skip is TapeTests' ignored case). `472eb36` commits the .meta files Unity
  wrote for A's and K's files.
- **Build 1 (8:12-8:13 AM, Release, Survival + Arena; Sync Port Data by its pre-build hook):** `Succeeded`, 0 errors;
  `git diff --stat ProjectSettings` only UnityConnectSettings.asset (not staged); `Mobile_RPAsset.asset` not rewritten.
- **Build 1 smoke (`Logs/smoke-n14-b1.log`, 181 s): `DONE: 2 step(s) failed`.** Tonight's steps 19-22 passed: `tape: 491
  steps taped (4148 B), replayed in this process: EQUAL`; `autopilot: seed 4242 normal (botBrain) twice for 150 s: round 5
  / 5, 66 kills, cause timeout; 30 of 30 hashes equal`; `attract: started True, 20 s behind TITLE True (2369 steps, 7
  kills, metaSilent True); stopped: released True; the next run EQUAL to the baseline (12 of 12)`; `WHAT GOT YOU + THE
  ROUTE: recap played True (killer HUSK), RESULTS True, the route drawn True (17 points), the ghost stored True, the card
  84641 B`; `ghost: a second run on seed 515151: loaded True, the runner shown True, a split at round 1: 'ROUND 1 · +0.0
  s'`. Failed:
  - `save and resume: ... saved to TITLE False` (NIGHT13's step, passing at u3-n13). The log: `[save] not saved:
    snapshot: saving 'husks': snapshot: a reference to an enemy (spitter 8) outside the registry`. **Root cause:** the
    clean slate (U3-27.2) empties the registry, but `SpitterAcid.Clear` only marks the glob and puddle slots dead; a dead
    glob still named the last run's spitter, and the snapshot looks every slot's shooter up. Tests first `6afe21a` (seen
    red: `the glob's shooter ... But was: <Skygrind.Core.Spitter>`), fix `ac97a1a` (`Husks.ClearPools` calls
    `SpitterAcid.DropEnemies`); CoreHarness after it: `golden total: 39 vector sets (48094 records), 22 replays (22
    exact); 0 failing, 0 mismatches`.
  - `clean slate: seed 1 twice on the played composition: 12 of 12 checkpoints equal, the first run differs from the
    baseline at checkpoint 6` (step 18, its first run anywhere). **One cause found:** the fingerprint run's aim assist
    read the last device (`PadReader.AimMode`, `BreakawayCap`), and the virtual pad step runs just before step 18. Test
    first in `6afe21a` (seen red: `first difference at checkpoint 2` with the pad's hooks), fix `4e718c6`
    (`SurvivalSim.RunFingerprint` sets both hooks aside for its run). A wrong claim of mine, corrected: the acid's stream
    IS reseeded per run (`Husks.NewRun`); only its slots' enemy references carried over.
- **The suite after the fixes (8:31 AM):** `tests: 678 / 679 passed, 0 failed, 1 skipped`, `replays: 22 / 22`, `golden
  tests: 96 / 96`.
- **Build 2 (8:31-8:32 AM):** `Succeeded`, 0 errors, the same ProjectSettings diff. Its first smoke stopped after 8 s with
  a clean input-system shutdown and no exception (`Logs/smoke-n14-b2.log`; GG was at the machine: the window was most
  likely closed); the rerun (`Logs/smoke-n14-b2b.log`, 180 s): `save and resume: allowed at round 2's break True, saved
  to TITLE True, resumed round 2 / points 1283 / guns revolver,gauntlet (equal True), file consumed True`; steps 19, 20
  and 22 passed as in build 1 (`tape: 487 steps ... EQUAL`, `autopilot: ... 30 of 30 hashes equal`, `WHAT GOT YOU + THE
  ROUTE: ... the card 84319 B`, `ghost: ... 'ROUND 1 · +0.0 s'`); **`DONE: 2 step(s) failed`**: step 18 still `the first
  run differs from the baseline at checkpoint 6` (12 of 12 equal to each other), and step 21 now `the next run differs
  at checkpoint 6`.
- **Build 2's Player checks (8:36-8:38 AM, batchmode, `Logs/player/*-n14-b2.log`):** `-skygrindFingerprint`: `seed 1: 12
  hashes over 7200 steps in 1.16 s: EQUAL to the baseline`; `-skygrindGolden`: `golden total: 39 vector sets (48094
  records), 22 replays (22 exact); 0 failing, 0 mismatches`; `-skygrindReplay` on the smoke's tape
  (`run-20261005-153524-737373.sktape`): `[replay] EQUAL (1 of 1 checkpoints, 487 steps)`.
- **The residual (open, measured):** only a composition that has played the smoke's earlier steps differs, from
  checkpoint 6 (steps 3001-3600), and two seed-1 runs on it are equal to each other (RETRY SAME SEED holds in the
  Player). Build 1's step 21 passed while the save failed; build 2's step 21 fails now that SAVE & QUIT and RESUME work.
  Hypothesis tested: a resumed run leaves state the next StartRun doesn't reset. `853416f`
  (`RunRestoreTests.AfterAResume_TheNextRunIsAFreshCompositions`: resume, leave, then seed 1; every snapshot section
  compared with a fresh composition's after StartRun) **passes**, so the carry-over is not in the sim's sections: it is
  Game-side (a SurvivalGame hook or view that touches the sim between steps 3001 and 3600, or state outside
  SnapshotSections). Two approaches spent; stopped there at 8:40 AM (10.7 h after the first `date`).
- **The share card, looked at** (`Logs/shots/n14-share-card-b2.png`, build 2's smoke): the arena outline in the
  boundary's pale violet, the DOWN mark in red-orange, ROUND 1 D, SCORE 0, BEST RANK DULL, TIME 0:04, TOP GUN -, 0 KILLS,
  SEED 00B1HR, the build stamp `SKYGRIND 0.1.0 (4e718c6-dirty)`. The route itself is a few points (a 4 s run).
- **`0ddcbc3`:** TapeTests' AUTOPILOT case on (it waited for U3-27.5): `tests: 7 / 7 passed, 0 failed, 0 skipped`.
- **THE FARM in the Player (8:38-8:57 AM, build 2, batchmode; farm wall time 1115 s for the batch, 800 s for the 4th
  process; at most 4 processes):** `Tools/farm-batch.sh Reports/farm-n14 100 1 easy normal hard`, plus a 4th process for
  hard seeds 51..100 (the hard runs were the slowest); the first hard process was stopped by its PID (45716) at 50 lines.
  `[farm] farm 100 runs, easy, seeds 1..100, brain botBrain: mean round 6.70, max 14, timeouts 9`; `normal: mean round
  9.52, max 26, timeouts 13`; `farm 50 runs, hard, seeds 51..100: mean round 13.16, max 26, timeouts 7`. 300 lines, 300
  unique (difficulty, seed). **The Player (Mono) and the harness (CoreCLR) agree:** `farm lines: A 117, B 300, in both
  117, equal 117, different 0` (every line but the build stamp). `node Tools/farm-report.cjs --name n14 --out Reports`:
  `farm-report: 300 JSONL lines -> 300 runs (easy 100, normal 100, hard 100)`; the HTML rendered in a headless browser and
  looked at (`Logs/shots/n14-farm-report.png`: the observations, the per-difficulty table, rounds reached, the survival
  curve, points per round x3, kills by gun, downs by enemy kind, buy timing; the only console error a missing favicon).
  The top observations are in NIGHT14-REPORT (round 6, the first HOLD round, ends 42-44 % of runs; 37 of the 41
  timeouts stuck there).
- **The autopilot bench series (8:57-9:01 AM, build 2, 60 s, 24 husks; bench runs 3 of 18):** median
  4.24 / 4.28 / 4.37 ms (p95 5.49 / 5.56 / 5.78, worst 1 % 7.26 / 8.43 / 7.78) -> **the series median 4.28 ms**; Balanced
  before and after every run, **AC=False (on battery)**, GG's other sessions ~2.5 cores busy. A first reading of the
  scenario, not the bar's Silent series (not run: battery, a busy machine, the time).
- **The close:** the agents' six worktree folders removed (`git worktree remove`, each clean; the branches stay).

## Close table (9:30 AM)

| Item | Status | Evidence |
|---|---|---|
| U3-27.1 start | DONE | `71cf9aa`; `tests: 594 / 594 passed, 0 failed, 0 skipped` (10:05 PM) |
| U3-27.2 clean slate | SHORT | `ec47c50`, `ac97a1a`, `4e718c6`; the Player smoke `seed 1 twice on the played composition: 12 of 12 checkpoints equal`; short: that run still differs from the baseline at checkpoint 6 (Game-side; `853416f` proves the sim clean), two approaches and the time limit |
| U3-27.3 p22 oracle | DONE | `verify.mjs` `total: 0 mismatches`; CoreHarness `golden PASS astar: 724 records` ... `follower: 4494`, 0 mismatches |
| U3-27.4 bot in Core | DONE | `474825c` .. `d472726`; Player `-skygrindGolden` `39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| U3-27.5 AUTOPILOT | DONE | `6a65430`; Player smoke `autopilot: seed 4242 normal (botBrain) twice for 150 s: round 5 / 5 ... 30 of 30 hashes equal` |
| U3-27.6 THE FARM | DONE | 300 Player runs; `farm-report: 300 JSONL lines -> 300 runs (easy 100, normal 100, hard 100)`; the HTML looked at; Player = harness `equal 117, different 0` |
| U3-27.7 kiosk + attract | SHORT | `07bfdd3`; build 1 smoke `attract: ... the next run EQUAL to the baseline (12 of 12)`; short: build 2's step 21 (the U3-27.2 residual) and the capture (the time limit) |
| U3-27.8 THE TAPE | DONE | `0ddcbc3` TapeTests `7 / 7 passed`; Player `-skygrindReplay`: `[replay] EQUAL (1 of 1 checkpoints, 487 steps)`; 162,535 B a minute; `REFUSED: tape format 0` |
| U3-27.9 WHAT GOT YOU | SHORT | `758c78c`; Player smoke `recap played True (killer HUSK)`; short: the 2 captures and the bench delta (the time limit; the machine on battery and busy) |
| U3-27.10 ROUTE + card | SHORT | `cf9a732`, `d54b8b0`; smoke `the card 84319 B`, the PNG looked at (`Logs/shots/n14-share-card-b2.png`); short: the RESULTS capture (the time limit) |
| U3-27.11 GHOST + splits | SHORT | `61a134a`; smoke `ghost: ... the runner shown True, a split at round 1: 'ROUND 1 · +0.0 s'`; short: the capture (the time limit) |
| U3-27.12 the bot as a tool | DONE | `bf15341`, `4b6402b`, `534de56`; the autopilot series median 4.28 ms (3 runs, logged); smoke step 20 PASS |
| U3-27.13 close | SHORT | build 2's golden / fingerprint `EQUAL to the baseline` / replay `EQUAL`; docs, tasks.json, tag `u3-n14`; short: the smoke `DONE: 2 step(s) failed` (the residual) and the final Silent series (on battery, busy machine, the time limit) |

**U3-27.3 DONE (1:32 AM):** `verify.mjs` `total: 0 mismatches`, the one-ulp mutation fails in all five files, and each
file's C# checker (N's GoldenBot.cs) passes in the CoreHarness on main: `golden PASS astar: 724 records`, `reach: 473`,
`navGraph: 3722`, `botBrain: 7200`, `follower: 4494`, 0 mismatches each.

## U3-27.5 / U3-27.6 the AUTOPILOT and THE FARM (agent A, 10:40 PM - 1:20 AM)

- **The autopilot** (Game/Bot: AutopilotInput, AutopilotTarget, AutopilotShopper, AutopilotGuns, AutopilotParams,
  AutopilotNavView, AutopilotStandInBrain): BotBrain on the player's Mover and loadout; the rules while it drives
  (Detach restores them): metaSilent + noLab (rule 7), **no aim assist** (`assistModeFor` answers "off": rule 6, rejected
  the human's default assist), the brain's rng `Mulberry32(runSeed ^ design.autopilot.rngSalt)` re-seeded at every run
  start (rule 8). **The target adapter:** the nearest live enemy in sight (checked every bot.losInterval), else the
  cheapest by A* among the 3 nearest (at most every bot.replanInterval), kept unless another is targetSwitchMeters
  nearer; perceived at its aim point. **Guns** through BotBrain's `pickWeapon` hook by distance (design.autopilot.guns
  near / far; rejected the web's slot 0 / 1 rule: in SURVIVAL slot 1 is the GAUNTLET). **The shopper:** the first wanted,
  affordable entry of design.autopilot.shopping (THE FORGE appended) counting the gate prices on the cheapest way to its
  zone; the nav override there, the last metres straight; hold interact while it is the buy candidate; READY (hold
  interact with nothing in reach) when nothing is affordable; a goal not bought in shopGiveUpSeconds dropped. **HOLD
  rounds:** walk to the ring, fight in sight within holdFightMeters on the way, hold inside holdInsideShare of the
  radius. **The graph:** BotNav's Survival graph seen through AutopilotNavView (locked zones' nodes lifted to +infinity,
  refilled in place when the zones change). Two fixes found by tracing (`6a65430`): a shopper in reach no longer steps
  the brain (1198 "stuck" replans by round 3), and the nav override's stop short of a gate spot / wall gun finished on foot.
- **New design.autopilot keys** (both copies byte-identical, the `_why` updated): shopGiveUpSeconds 20, guns.near /
  guns.far, gunHysteresisMeters 1 (botBrain.js's literal), graphCandidates 3, rngSalt 1640531527, holdInsideShare 0.5,
  holdHeightMeters 2.5 (objectives.js's literal), holdFightMeters 8.
- **Measured (A's harness, CoreCLR, the REAL BotBrain, real HP, 40 min cap; 213.4 s wall for the 60 runs):**

| seed | easy | normal | hard |
|---|---|---|---|
| 1 | 6, 332.0 s, SWARMER | 14, 762.1 s, ACID | 6, 146.7 s, SWARMER |
| 2 | 9, 529.9 s, SPITTER GLOB | 6, 213.7 s, ACID | 6, 202.3 s, SPITTER GLOB |
| 3 | 14, 1051.7 s, HUSK | 6, 142.4 s, ACID | 6, 213.3 s, ACID |
| 4 | 7, 470.1 s, SPITTER GLOB | 14, 793.7 s, HUSK | 14, 538.4 s, ACID |
| 5 | 6, 2400.0 s, timeout | 26, 1902.4 s, HUSK | 6, 2400.0 s, timeout |
| 6 | 6, 343.5 s, SPITTER GLOB | 6, 182.5 s, ACID | 14, 507.8 s, SPITTER GLOB |
| 7 | 6, 404.7 s, SPITTER GLOB | 6, 208.3 s, ACID | 6, 2400.0 s, timeout |
| 8 | 6, 2400.0 s, timeout | 12, 612.3 s, SPITTER GLOB | 17, 823.1 s, SPITTER GLOB |
| 9 | 4, 169.3 s, HUSK | 4, 103.6 s, SWARMER | 14, 681.9 s, HUSK |
| 10 | 2, 57.6 s, HUSK | 6, 181.7 s, HUSK | 11, 437.7 s, HUSK |
| 11 | 6, 357.4 s, HUSK | 6, 175.9 s, SPITTER GLOB | 6, 151.0 s, ACID |
| 12 | 14, 1098.5 s, ACID | 6, 231.0 s, ACID | 14, 614.8 s, HUSK |
| 13 | 6, 319.8 s, HUSK | 14, 746.4 s, SPITTER GLOB | 6, 2400.0 s, timeout |
| 14 | 2, 60.4 s, HUSK | 5, 140.5 s, HUSK | 7, 2400.0 s, timeout |
| 15 | 14, 1020.1 s, ACID | 3, 77.4 s, HUSK | 26, 1525.8 s, ACID |
| 16 | 6, 364.1 s, ACID | 6, 253.9 s, ACID | 14, 537.6 s, SWARMER |
| 17 | 6, 330.2 s, SPITTER GLOB | 6, 186.9 s, SWARMER | 14, 568.7 s, HUSK |
| 18 | 9, 489.3 s, HUSK | 14, 754.7 s, HUSK | 6, 2400.0 s, timeout |
| 19 | 11, 858.2 s, SPITTER GLOB | 9, 356.8 s, HUSK | 14, 583.7 s, ACID |
| 20 | 2, 70.6 s, HUSK | 19, 1178.2 s, SPITTER GLOB | 26, 1524.2 s, ACID |

| difficulty | mean round | max | mean time | causes |
|---|---|---|---|---|
| easy | 7.10 | 14 | 656.4 s | ACID 3, HUSK 8, SPITTER GLOB 6, SWARMER 1, timeout 2 |
| normal | 9.40 | 26 | 460.2 s | ACID 7, HUSK 7, SPITTER GLOB 4, SWARMER 2 |
| hard | 11.65 | 26 | 1052.8 s | ACID 6, HUSK 4, SPITTER GLOB 3, SWARMER 2, timeout 5 |

  Normal passes round 2 (mean 9.4), so the "two improvements" clause did not apply. Hard outlasting easy is the
  difficulty tables' reaction / aim (hard aims better and kills faster); the timeouts are HOLD rings the bot can't reach
  (2 of 3 in A's trial farm). Observations for GG only.
- **THE FARM** (`Farm.Run`: a fresh `SurvivalSim.Headless` per run, metaSilent, real HP, drafts taken, a run ended as a
  quit at design.farm.maxMinutes ("timeout")), one JSONL line per run (seed, difficulty, build, brain, round, cleared,
  simTime, steps, cause, killer, kills, earned / spent and per round, guns (round, slot), killsByGun, perks / mods / forge
  / gates with rounds, the style rank histogram, downs, exfil, grade, shots, hits, dropped goals, the last round's type
  and HOLD state). **Tools/farm-report.cjs** (Node, no packages): `Reports/FARM-n14.md` + a self-contained
  `Reports/farm-n14.html` (8 inline SVG charts, no script, no external URL) and a `diff` mode. A's trial (real brain, seeds
  101-110 x 3): `farm-report: 30 JSONL lines -> 30 runs (easy 10, normal 10, hard 10)`.
- Determinism: `autopilot-twice seed 1 normal (botBrain): EQUAL (152 of 152 hashes, the JSONL lines equal)`;
  `autopilot-selftest` (A's mirror of AutopilotTests / FarmTests on CoreCLR): 11 PASS, including `PerStep_AllocatesNothing:
  0 bytes ... over 12169 fight + 2751 break steps` (the adapter's own path; the brain's growth is N's test).

## U3-27.7 the KIOSK and the attract (agent K, 10:40 PM - 1:00 AM)

- `Kiosk.cs` (kiosk.js's logic, Unity-free): `-skygrindKiosk` (exact, any case); the idle clock on frame time (60 s:
  config.kiosk.idleReset, any input anywhere resets it) sends a run back to TITLE (`kioskReset`); the attract starts on
  the kiosk's TITLE and, on a normal TITLE, after design.attract.idleSeconds (30) with no input (automation flags suppress
  it). `UiKiosk.cs`: the hints panel (the web's rows with the live bindings' labels, refreshed on a rebind; ARCLINE
  dropped: SURVIVAL's mover has the rope off; "CLICK PLAY TO START"), TITLE's QUIT and PAUSED's QUIT TO TITLE / SAVE &
  QUIT hidden in the kiosk. `SurvivalGame.Kiosk.cs`: AttractStart (from TITLE: a new AutopilotInput swapped in, metaSilent
  / noLab, a SURVIVAL HEAT 0 run on design.attract.seed, the player's TITLE choices put back, god mode = 1e6 HP pinned as
  the fingerprint does (the port's Health has no god flag), the cursor free and hidden, TITLE's buttons locked so the
  stopping click presses nothing, the gun camera off); AttractStop on any input (the player's input back first, ToTitle:
  the run ended unjudged, the flags restored, TITLE unlocks the next frame); a fresh attract run after DOWN / RESULTS /
  PAUSED; drafts taken by AutopilotInput.TakeDraft; the flythrough = main.js :2337's orbit with config.kiosk's numbers.
- K's notes for the lead (open): code that reads `game.flow` rather than `UiFlow` may show during the attract (the
  world price signs, the momentum arc, lessons, damage numbers); HotKeys / RoutePad run before FrameKiosk, so the press
  that stops the attract also does its own job (X-RAY, LOCK ON); with VIBRATION on the bot's hurts rumble the pad.
