# NIGHT13-LOG: run better, play anywhere, and the architecture under it

Run started 2026-10-04 2:55:29 PM (first `date`). The close starts by hour 10:30 (01:25 AM); 12 hours ends 02:55 AM.
GG's goal message: work U3-26.1 .. U3-26.13 in NIGHT13.md section 5's order with background agents split by file
ownership; never ask, never stop early; the close table with every row DONE or SHORT, then `NIGHT13 CLOSED`. Caps
(NIGHT13.md section 4): Play checks 2, captures 8, Player builds 5, bench runs 24, builders 8, reviewers 2. Times below
are from `date`.

## RECEIVED (U3-26.1, 2:55-3:03 PM)

| Row | Command | Result |
|---|---|---|
| 1 | `git rev-parse --short HEAD`, tags | HEAD `07f7885` on `port/stage3`, tag `u3-n12` on it: as expected |
| 2 | `git status --short` | ` M HANDOFF.md`, `RM NIGHT13-RESEARCH.md -> NIGHT14-RESEARCH.md`, ` M ProjectSettings/UnityConnectSettings.asset`, ` M tasks.json`, `?? NIGHT7.md` .. `?? NIGHT14.md`, `?? NIGHT13-RESEARCH.md`, six `?? Research/2026-10-04-*.md`: the research session's docs, as HANDOFF's update note says |
| 3 | `bash Tools/unity-loop.sh status` | `"status": "playing"`: the Editor was in Play mode at arrival; idle 35 s (the goal message had just been typed). Stopped with `unity-loop.sh stop` (HANDOFF row 3: stop first); then `"status": "ready"`, `"playMode": "stopped"`; the TMP fallback asset restored with `git checkout --` (no diff left) |
| 4 | `bash Tools/unity-loop.sh tests` (3:00 PM) | `tests: 501 / 501 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 91 / 91` |
| 5 | `dotnet run --project Tools/CoreHarness -c Release` | `golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches` |
| 6 | `SKYGRIND.exe -skygrindSmoke` (the u3-n12 build, 23:30 Oct 3) | `[smoke] 107.2s DONE: all steps passed`, 0 exceptions (`Logs/player/smoke-n13-arrival.log`) |
| 7 | `SKYGRIND.exe -skygrindGolden` | `[golden] golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches` (`Logs/player/golden-n13-arrival.log`, 3:03 PM) |
| 8 | `powercfg -getactivescheme`, AC | Balanced (381b4222-...), on AC (PowerOnline True) |
| 9 | idle seconds (GetLastInputInfo) | 35 s at 2:57 PM (the goal message) |

- **The docs commit:** `d5c30d7` (NIGHT13-RESEARCH.md, NIGHT14-RESEARCH.md, six Research/2026-10-04-*.md, HANDOFF.md's
  update note, tasks.json U3-26*), explicit paths.
- **GG's answers (HANDOFF section 5):** none new. D-06, D-34, D-38 .. D-47 are still pending; their defaults apply
  tonight: D-34 the 6.9 ms p99 bar on Silent; D-39 the bar on the UI-hurt scenario, both reported; D-40 Mono unless
  the IL2CPP trial's gates and a gain above the noise say otherwise; D-44 uncapped; D-45 the web's swap; D-47 a DRAFT
  pillars page at the close. Nothing changes tonight's plan.
- **The power plan:** Balanced at arrival. The bar is on Silent (D-34's default, the memory note). As NIGHT12 did once,
  the lead sets Silent for each bench series and restores the plan found (Balanced) after it, polling before and after.

**U3-26.1 DONE (3:05 PM):** every row as expected; `d5c30d7`.

## U3-26.2 measure first (3:05-3:35 PM; the series deferred)

- **The composed fingerprint** (`SimFingerprint.cs`, `SurvivalGame.Fingerprint.cs`, `-skygrindFingerprint`): FNV-1a 64
  over the run's Core state (mover + look, health, economy, run + stats, director state, the live enemies, style,
  loadout, zones, perks, mods, and the 13 Mulberry32 streams' states read by reflection until U3-26.10's `State`) on a
  fixed input script (strafe, jump, dash, slide, aim at the nearest enemy, fire, punch, the special, BUY held, gun
  switches), seed 1, a hash every 600 steps to 7200, all steps in one frame. Pinned: OnScreen false, settings at their
  defaults (the store only), a fresh profile (`records` re-pointed too: `Profile.CopyFrom` replaces the records
  object the MODS unlock reads), no LAB / records / profile writes, a plain Survival run at HEAT 0, the player at 1e6
  HP. Gates: compile; `tests: 501 / 501 passed`, `replays: 22 / 22`, `golden tests: 91 / 91` (3:20 PM).
- **The Editor (one Play-mode run, a gate):** 7200 steps in 0.48 s; seed 1 `step 600 hash 42c74be7ac21a5b3` ...
  `step 7200 hash 7b02ca5c9fa6bb0e`; seed 2: 0 of 12 checkpoints equal (seen red).
- **Found (not tonight's fix; HANDOFF pending):** a second seed-1 run in the same session differs from the first at
  step 600 (0 of 12 equal): state carries across runs (candidates: the enemy registry's order / ids, the nav fields'
  progress, the brain's timers). So RETRY SAME SEED within one session is not the same run, though seeds.js promises
  "same seed, same run". The fingerprint's gate is therefore two fresh Player processes, as NIGHT13.md asks.
- **Build 1** (3:26:26-3:26:52, Release, Mono, `Succeeded`, 0 errors, 9 warnings, 160,230,591 B; builds 1 of 5).
  `git status`: nothing new beyond the night's edits (no ProjectSettings / Mobile_RPAsset / TMP rewrite).
  - `-skygrindFingerprint` twice (`Logs/player/fp-b1-1.log`, `fp-b1-2.log`): identical, and equal to the Editor's
    twelve: `seed 1: 12 hashes over 7200 steps in 0.38 s ... at the end: run Fight round 4, kills 36, shots 80, earned
    4310, alive 0, owned 2, flow Playing`; seed 2 (red): `0 of 12 checkpoints equal to seed 1's`. The twelve are the
    baseline (`FingerprintBaseline.cs`).
  - Smoke: `[smoke] 107.1s DONE: all steps passed`, 0 exceptions (`Logs/player/smoke-b1.log`).
  - A copy kept as `Builds/N13-b1/` (gitignored) so the baseline bench can run on the u3-n12 composition later.
- **The bench series is deferred: the machine is not idle.** At 3:31 PM GG's other project was running an LLM eval
  (patric2 `scripts/ask_eval_246.py --models granite4.2:8b` against a local `ollama` / `llama-server`, plus an
  `ollama pull qwen3.5:4b`): GPU utilization 84-91 %, total CPU 58-60 % in its bursts (11.5 % between). A bench now
  would be void, as NIGHT12's 01:03 run was. Never GG's jobs to stop. The series runs on `Builds/N13-b1` when the load
  reads idle (checked before each heavy step), and before 00:45.
- `192bff9`: the fingerprint, the baseline, the per-frame breakdown in `-skygrindBench` (p99.9, max, frames over
  6.94 ms; per frame the interval, steps, step ms, Update ms, GC count, heap; FrameTiming main / render / present-wait
  / GPU; worst-1% vs median-band means; two CSVs), `-skygrindBench 60 play`, and `survival-bench.sh` polling the plan
  once before and once after.

**U3-26.2 in_progress (3:35 PM):** everything but the series and its breakdown table (waits for an idle machine).

## U3-26.3 shared types (3:35-3:57 PM)

- `2a163b1`: `Core/RunState.cs` (IRunState, StateWriter / StateReader); `Game/Input/` IPlayerInput (the sim's input
  seam with the look), InputDevice, GlyphSet, RadialState, ScriptedInput, ReaderInput (today's readers + the forced
  overrides, as SimStep applies them); `CombatReader.radial`; Ui's per-agent hooks and the TITLE / PAUSED buttons
  (RESUME hidden, CONTROLS, RESTART, SAVE & QUIT hidden, COPY REPORT); `SurvivalGame.Save.cs` (save stubs,
  RestartRun); UnityTuning partials Perf / Input / Controls / Hotbar / Impressions / Report; `design.save` (+ the
  Player copy, byte-identical); `Tools/wt-csproj.cjs`. Gate: `tests: 501 / 501 passed`, `replays: 22 / 22`,
  `golden tests: 91 / 91` (3:46 PM).
- `11ec1b2`: PlayerDevices + MenuInput (the devices' state the UI reads) and `WeaponLook.FamilyHex / FamilyColor`.
  Compile only.
- The EventSystem with `InputSystemUIInputModule` already existed (`Ui.cs:191-192`): kept as the one.
- **Decision: one input agent (I) instead of I-Core + I-Game.** The Game side consumes the Core API (PadState, the
  bindings store, the pad layout); one owner avoids two agents guessing each other's signatures. Rejected: stubs of
  the Core API in the shared types (they would be guesses before reading gamepad.js). Builders: 7 of 8.
- **Decision: the hotbar / radial INPUT (Digit0, the wheel cooldown, quickSwap, the Tab radial) goes to agent I**
  (it lives in CombatReader / InputReader, I's files); agent H draws the hotbar and the radial from
  `CombatReader.radial`.
- **Agents launched at 3:53 PM, every one in its own worktree from `11ec1b2`:** P (U3-26.4 perf switches, quality,
  warm-up), I (U3-26.7 input + gamepad, the radial input), S (U3-26.8 settings view + file, rows, UiNav, CONTROLS
  after I's commits), H (U3-26.9 hotbar + radial UI), E (U3-26.10 Mulberry32 State, IRunState, RunSnapshot; phase 2
  the composed proof once SurvivalSim lands), F (U3-26.11 S1-S12), G (U3-26.12 version stamp, run log, COPY REPORT,
  dev keys). Each brief checks its base commit first.

**U3-26.3 DONE (3:57 PM):** `2a163b1`, `11ec1b2`; the agents launched.

## U3-26.5 SurvivalGame split (3:57-4:48 PM)

- `d882659`: partial files `.Init` / `.Flow` / `.Meta` / `.Render` (RenderFrame: the render half of Update, moved
  whole, called at the same place) / `.Debug`, by a script over checked line ranges; `SurvivalGame.cs` 1521 -> 443
  lines. The HUD line's text into Ui (`UiRunShape.cs`). WeaponLook at four of six family-colour sites (LightPools and
  Viewmodel are agent P's files tonight: after P lands). One binding label: SurvivalGame.KeyLabel now goes through
  `Ui.KeyLabel` (bindings.js keyName): tip texts print ShiftLeft as the web's label, not SHIFTLEFT. Dead code out:
  BeforeAi / AfterPlayerSystems, IdleSpawner, the unused click read.
- Gates: the Editor fingerprint (a gate, not a Play check) `[fpc] EQUAL to the baseline (12 of 12); run Fight round 4,
  kills 36, shots 80, earned 4310, alive 0, owned 2, flow Playing`; `tests: 501 / 501 passed`, `replays: 22 / 22`,
  `golden tests: 91 / 91` (4:40 PM).
- `0ead5b9`: S8 (the zone price signs only while Playing or Paused; ZoneSigns lives in Ui.cs, a one-line guard far
  from agent F's regions).
- Slips: one multi-line patch through `node -e` (KillConfirm's three edits; CLAUDE.md rule 9 says a written .cjs);
  the result was checked by grep and the compile. The first Editor fingerprint attempt found no `[fpc]` line within its
  60 s poll (idle read 21 s at 4:31 PM: someone may be using the machine, which stalls the Editor); the second
  attempt's console read showed it.

**U3-26.5 DONE (4:48 PM):** `d882659`; fingerprint equal; suite green.

## U3-26.6 the sim composition (4:48-5:28 PM)

- `e1645cf`: `SurvivalSim` (`SurvivalSim.cs` + `.Run.cs`, no UnityEngine): every Core system built in Init's old order
  (every sim-affecting EventBus registration in today's sequence), Step in simulate() order, the flow's sim part
  (ResetWorld, StartRun with callbacks at the two points where views reset and OnNewRun fired, EndRun, ToTitle, the
  death / hitstop handlers, stepMode), RunFingerprint. Injected: IPlayerInput, the three settings readers, OnScreen,
  the key labels, the profile and its writer, runEnded (the cursor), profiler marks by id (the Game maps them to the
  old `SKYGRIND.*` ProfilerMarkers). The sim halves of WallBuys (the guns' buyables) and PerkMachines (the buyables,
  perks.apply, MEND's buy point riding its pillar: `RideCore`, still called from the render each frame as before)
  moved in. SurvivalGame (289 lines) keeps the boot, the frame loop, the devices (`ReaderInput`) and every view, built
  after the sim in their old relative order, and forwards its old fields (no view changed).
- **Decision: the views' EventBus handlers now follow all of the sim's** (before, some interleaved: particles, trauma,
  the viewmodel, CameraFeel). Checked: none of them changes sim state, and the one handler that calls back (Hitstop)
  has no listener on its `hitstop` event; the sim's handlers keep their exact order. Rejected: staged construction
  interleaving views between sim stages (more API for no behaviour difference).
- **The gate:** `FingerprintTests` (Game.Tests): a headless SurvivalSim from PortExport (settings at their defaults, a
  fresh profile, OnScreen false, taken to TITLE with the machines' buy points on the flat core as TITLE's frames put
  them) plays the fingerprint run: `tests: 2 / 2 passed` (17:23): equal to the Player's baseline at 12 of 12, and
  seed 2 differs. Then `tests: 503 / 503 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 91 / 91` (5:27 PM).
  The Player fingerprint and smoke on this code wait for build 2.
- Agent E sent its phase-2 go (the composed restore proof on SurvivalSim) with `e1645cf`.

**U3-26.6 DONE (5:28 PM):** `e1645cf`; the headless test equals the baseline inside the suite (the Player's run of it
comes with build 2).

## Agents landing (from 5:30 PM)

- **G (U3-26.12)** finished 5:29 PM: cherry-picked `0e91fd5` (tests on stubs), then the implementation `43b3a9d`
  (DevKeys), `ba1cb92` (version stamp: BuildInfo writes `build-stamp.txt` into the built Player's data folder after a
  build, nothing in the repo), `132d67f` (RunLog), `fc7d46d` (COPY REPORT). Seen red: `DevKeysTests` 0 / 3 and
  `RunLogTests` 0 / 11 (NotImplementedException) at `0e91fd5`; green 3 / 3 and 11 / 11 after (5:35 PM). Wiring
  `04babc0`: `RunLog.Attach` in StartGame, F3 / F8 / F9 behind `DevKeys.Enabled`. The RUN LOG settings row
  waits for agent S's GameSettings.
- **H (U3-26.9 HUD half)** finished 5:33 PM: cherry-picked `80786c5` (the weapon block: plate, hotbar of up to 12 with
  key labels, FRESH bars, stale dimming, family colours, "+N", the weapon meter, the special hint; pad labels from
  PlayerDevices) and `b65e5c8` (the radial: up to 24 options at radius 170, the highlight, the middle prompt, a
  pointer dot). Compile clean (5:42 PM). The radial's input is agent I's.
- `891148d`: the smoke's night-13 steps file (step 13: the run log) and the smoke's profile restore re-pointing the
  sim's records (found while wiring: `Profile.CopyFrom` replaces the records object, so after a smoke the old
  SurvivalGame.records field held a detached copy).
- **The bench at 5:45 PM: still not idle** (CPU 10-45 %: GG's patric2 pytest and two loaded llama-servers, plus five
  agents compiling). Deferred again.
- **P (U3-26.4)** finished 5:50 PM: QualityTests `df52368` seen red 0 / 7 on the stubs, then 10 commits (QualityMath /
  Quality, PerfSwitches, LightPools, the overlay camera, PostFx Volume via scripting + writes on change, EnemyLook
  property blocks on change + the x-ray material only while X-RAY is on, the opaque texture off per camera,
  QualityRuntime (GC slice, AUTO / render scale / shadows, the boot perf line), Warmup (GraphicsStateCollection:
  none without a recorded file)). Wiring `2421dfb` (QualityRuntime.Boot, Warmup.Run, LightAndLens on the effective
  preset). Not done by P, reported: the gun's bob under REDUCED MOTION (the web keeps the bob: viewmodel.js
  :745-752; `config.js:2282`'s REDUCED MOTION lists roll, shake, fringing, speed lines), so the web's behaviour stays.
  Suite `524 / 524`, 22 / 22, 91 / 91 (6:05 PM).
- **S (U3-26.8)** finished 6:02 PM: tests `e45ef6c` seen red (SettingsTests 4 of 10 failing, SettingsViewTests 0 / 3),
  then the typed SettingsView + the settings JSON file (migrated once, atomic writes), the rows (SLIDE, ARCLINE SWING,
  the GAMEPAD tab's eight, UI SCALE, RENDER SCALE, WINDOW MODE, RESOLUTION), UiNav, CONTROLS on an interim adapter.
  Suite `531 / 531` (6:12 PM). Wiring `c4d4ff1` (CONTROLS capture / Esc in HotKeys, the ADS read through the view,
  the RUN LOG row). After I landed, S was resumed with I's hashes: CONTROLS on the real bindings (`23eec62` test seen
  red 0 / 2, `da7b43a` the swap). Suite `593 / 593` (6:58 PM).
- **F (U3-26.11)** finished 6:08 PM: 12 commits, one per finding (S1 padlock Image, S2 label flag, S3 the doorway
  field, S4 white / gold tally, S5 POINTS label, S6 the first-STALE tip, S7 CONTRACTS order / rewards / dormant line,
  S9 one-line lesson at the break, S10 mode lines + AIM ASSIST off TITLE, PLAY focused, TITLE's controls line, S12
  YOUR KEYS card). Integration `d367005`: F's `KeyRow` class renamed `PauseKeyRow` (S's UiControls defines a `KeyRow`
  method in the same partial Ui: CS0102); the boundary keeps the old sheet (`_Doorway 0`). Suite `531 / 531`.
- **I (U3-26.7)** finished 6:25 PM: 49 tests seen red on the stubs (Gamepad 0 / 21, HoldToggle 0 / 6, Bindings 0 / 8,
  MenuFocus 0 / 4, PadBreakawayCap 0 / 1, KeyCodes 0 / 4, RadialInput 0 / 5), then the ports, the AimAssist cap
  (harness green per I at each Core commit), KeyCodes, WeaponSelect (Digit0, the wheel cooldown, quickSwap, the Tab
  radial), the readers on live bindings, PadReader / Glyphs / Rumble / InputPrefs. One full run showed
  `ZeroAllocTests.SimSteps_With30Enemies_AllocateNothing` failing on 1 allocation in Director.Step (579 / 580); it
  passed alone twice right after and in every later full run: logged as a flake (its own comment notes first-call JIT
  noise). Wiring `287c192` (adapted: only the pad's lockOn / xray / pause are routed; the keyboard's keep their own
  reads, so nothing toggles twice). Suite `580 / 580`, 22 / 22, 91 / 91 (6:47 PM).
- **E (U3-26.10)** finished 6:46 PM: `4fd8fc5` State test seen red (0 / 1), `82ea9b7` Mulberry32.State, `d60aadb`
  RunSnapshotTests seen red (1 / 8 passing), `51b2733` RunSnapshot + IRunState on 34 systems, `c1a31c9` the composed
  proof `RunRestoreTests` (3 seeds: round 2's break, round 6's break into a HOLD round, round 5's break; the break
  before an exfil is not reachable by the script: reported). E's own cherry-pick of SurvivalSim into its worktree was
  refused by the permission system; it compiled against an exported copy. Suite `591 / 591`, 22 / 22, 91 / 91 (6:53 PM).

## The bench (U3-26.2's series and U3-26.4's), 7:01-7:21 PM: the machine idle

Before: CPU 3.6-9.5 %, GPU 0 %, no user input for 4914 s, no builder running, GG's jobs quiet. The plan found Balanced;
set to Silent by the lead for each series (`powercfg -setactive`, as NIGHT12 did) and restored to Balanced after it.
Every run below polled `Silent AC=True` before and after (survival-bench.sh: no poller during the run). Bench runs 10
of 24.

| Series | Build | Runs: worst 1% (p99) | Median | p99.9 / max (run 2) | Frames over 6.94 ms |
|---|---|---|---|---|---|
| b1-ui (U3-26.2) | build 1 (`Builds/N13-b1`, u3-n12 composition + instrumentation) | 4.43 / 6.06 / 4.81 | **4.81 ms** | 11.16 / 14.87 ms | 0.29 / 0.69 / 0.49 % |
| b1-play | build 1, `play` | 6.79 | (one run) | 9.34 / 16.54 ms | 0.91 % |
| b2-on (U3-26.4) | build 2 (`da7b43a`), every fix on | 5.30 / 5.42 / 7.22 | **5.42 ms** | 9.82 / 22.39 ms | 0.57 / 0.56 / 1.20 % |
| b2-off | build 2, `-skygrindPerfOff all` (u3-n12 behaviour) | 7.78 / 6.99 / 7.10 | **7.10 ms** | 9.01 / 20.28 ms | 1.80 / 1.02 / 1.14 % |

- **The breakdown (U3-26.2's done line)**, b1-ui run 2, mean over the worst 1 % | the median band (45-55 %):
  interval 8.110 | 2.531 ms; sim steps 0.964 | 0.310; sim step time 0.304 | 0.078 ms; Update (all our scripts)
  0.628 | 0.312 ms; **the main thread outside our scripts 7.482 | 2.218 ms**; GC count moved 0.004 | 0.000; a GC
  completing within 60 frames 0.031 | 0.005. FrameTiming (by its own cpuFrameTime): main thread 7.594 | 2.520 ms,
  present wait 0.523 | 0.001 ms, render thread 2.023 | 0.933 ms, GPU 0.173 | 0.751 ms. So a worst-1 % frame is ~5 ms
  of engine main-thread work outside SurvivalGame.Update (URP's render loop on the main thread, canvases, TMP, culling:
  not split further by a Release Player), plus one sim step (0.3 ms), plus 0.5 ms waiting on Present; GC is near 3 % of
  the worst frames; the GPU is idle. The play run reads the same (engine 7.347 | 2.222 ms). Build 2 all-on, run 2:
  engine 7.081 | 2.109 ms, scripts 0.542 | 0.303 ms, present wait 0.442, render thread 1.773 | 0.913 ms.
- **Readings.** (1) NIGHT12's Silent median 7.19 ms on this composition came with the old poller (a PowerShell + WMI
  query every 2 s through each run); with it gone, build 1 reads 4.81 ms on the same plan: the bench's own noise was
  the largest single cost (audit-perf's suspect 2). (2) Tonight's new UI (the hotbar, the radial, the nav, the cards)
  costs: build 2 with the fixes off reads 7.10 ms. (3) The fixes recover it: 5.42 ms with them on, under the 6.9 ms
  bar. Runs of one build still spread 1.9 ms (5.30-7.22): medians of three decide.
- Build 2 (7:08:38-7:09:30, `Succeeded`, 0 errors, 519 warnings, 160,453,322 B; builds 2 of 5). `git status` after:
  only UnityConnectSettings.asset. `build-stamp.txt`: `da7b43a`; the Player log's stamp `SKYGRIND 0.1.0 (da7b43a)`;
  `[perf] perf: lights on, volume on, mpb on, xray on, overlayShadows on, opaqueTexture on, gcSlice on, warmup on,
  quality on`, `[perf] incremental GC slice 3 ms -> 1 ms`.
- Build 2's gates: `-skygrindFingerprint` twice: `seed 1: 12 hashes over 7200 steps ... EQUAL to the baseline; at the
  end: run Fight round 4, kills 36, shots 80, earned 4310` both times (U3-26.6's Player gate); the smoke: every
  earlier step PASS, my new run-log step FAIL on its own threshold (`3 lines (header first: True, footer last: True)`:
  I had assumed 4: header, roundBreak, roundBegin, footer; the first break emits no roundBreak), 0 exceptions. The
  check is corrected to header + at least one event + footer.

## The lead's items after the agents (7:22-7:52 PM)

- `be2b609` U3-26.10's Game side: SurvivalSim.SnapshotSections (E's list moved into the game, its own section without
  reflection), CanSaveAtBreak; SAVE & QUIT (PAUSED, shown only when allowed) writes `{saveVersion, seed, heat}` + the
  snapshot to `skygrind-run.json` (temp file swapped in) and goes to TITLE unjudged; RESUME reads and deletes it, starts
  the saved seed and rung, loads the snapshot (another saveVersion or a StateException: discarded with a `[save]` log
  line, TITLE). RunRestoreTests now proves the game's own list: 3 / 3 (7:27 PM).
- `beffd20` the smoke's steps 14-17 (save / resume through a temp file; the radial by scripted input; a jump rebind
  read back after reloading the store; a virtual pad) and SampleCombat(Active) in the sim (agent I's contract: the
  reader gates the keyboard on the captured cursor, a playing pad counts without it; ScriptedInput's Captured is true,
  so the fingerprint is unchanged). Suite `593 / 593`.
- `dfb105e` leftovers: WeaponLook at LightPools and Viewmodel (the six family-colour sites are one lookup); the award
  feed's line cached per (award, points).
- **Build 3** (7:39:24-7:39:46, `Succeeded`, 0 errors; builds 3 of 5): fingerprint `EQUAL to the baseline`; smoke:
  run log, save and resume (`resumed round 2 / points 1283 / guns revolver,gauntlet (equal True), file consumed True`),
  radial (`shown on the HUD True, highlighted 1, the loadout's slot 1 (cursor captured True)`), rebind (`jump -> KeyJ
  after reloading the store; restored to Space`) PASS; the virtual pad FAIL on two checks of mine (the stick held 40
  frames = 0.1 s at ~400 fps: 0.2 m under my 0.5 m check; in SETTINGS right acted on the selected tab, not a row).
  `ca95a5d` corrects both checks (0.6 s of stick; one d-pad down onto the first row first).
- **The IL2CPP trial is SHORT (blocked):** this Editor's Windows player has only Mono variations
  (`PlaybackEngines/windowsstandalonesupport/Variations`: win32/win64/win_arm64 `_mono` only, no IL2CPP), and the machine
  has no Visual Studio / C++ toolchain (no `Microsoft Visual Studio` folder, no vswhere). Installing a Unity module and
  the C++ build tools is GG's call (a machine change). D-40 stays Mono; NIGHT13.md lists the trial first in the cut
  order anyway.
- `ffafa2b` the bench's pacing arms (`-skygrindFpsCap N`) and a `bench cpu` line (FrameTiming p99s); the fingerprint
  reads the streams through `Mulberry32.State` (FingerprintTests still equal). Suite `593 / 593`, 22 / 22, 91 / 91.
- **Build 4, the final build** (7:51:50-7:52:29, `Succeeded`, 0 errors, 8 warnings, 160,468,170 B; builds 4 of 5;
  `build-stamp.txt` `ffafa2b`): `git status` only UnityConnectSettings.asset. `-skygrindGolden`: `golden total: 34 vector
  sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches`. `-skygrindFingerprint` twice: `seed 1: 12 hashes
  over 7200 steps in 0.39 s: EQUAL to the baseline` (and 0.40 s); seed 2: 0 of 12 equal. Smoke: `[smoke] 125.3s DONE: all
  steps passed`, 0 exceptions: `virtual pad: TITLE selected 'PLAY', A played True, the stick moved 2.4 m and turned True,
  the trigger killed True, START paused True, B resumed True`; `virtual pad menus: the d-pad reached 'SETTINGS', A opened
  SETTINGS True, down reached 'Row sensitivity', right moved a value True (settings version 1 -> 2), B backed out True`.

## The final series on build 4 (7:57-8:06 PM, Silent, idle: CPU 2.7-8.9 %, GPU 0 %)

| Series | Runs: worst 1% (p99) | Median | FrameTiming main p99 | Frames over 6.94 ms |
|---|---|---|---|---|
| b4-ui (the bar's scenario, D-39) | 3.91 / 4.64 / 3.98 | **3.98 ms** | 3.92 / 4.55 / 3.98 ms | 0.22 / 0.41 / 0.34 % |
| b4-play | 3.87 | (one run) | 3.88 ms | 0.24 % |
| b4-cap144 (pacing arm) | 7.23 (interval median 6.94 = the cap) | (one run) | **2.69 ms** | 82.56 % (the cap itself) |
| b4-cap240 (pacing arm) | 4.56 (median 4.17) | (one run) | **2.39 ms** | 0.20 % |

- Every run polled `Silent AC=True` before and after; the plan was restored to Balanced after each series. Bench runs
  16 of 24. A CPU spike (40-58 %) right after the smoke Player exited was waited out (it read 2.7-8.9 % before the
  series).
- **The bar (D-34: 6.9 ms p99 on Silent) is met:** median 3.98 ms on the final build. Under a cap the interval reads the
  cap; the CPU's own p99 busy time is 2.4-2.7 ms ("main minus present wait" is meaningless capped: the throttle's wait
  is counted as present wait).
- The Windows HighQoS arm is not run (the bench has no process-QoS hook; adding one is a new idea for NIGHT14).

## Captures (Play check 1 of 2; 8 of 8 captures), 8:09-8:12 PM

One guarded eval coroutine in the Editor (`n13-caps-guard` left in the scene; eval_file called directly; the profile
read-only, no records, no LAB; a temp save file). The console: `contracts click True`, `bind click True`, `gamepad tab
click True, pad connected True`, `radial shown True highlight 1 locked True`, `paused, save allowed True, round 2,
cleared 1`, `title, has save True`, `[caps] done`. Each capture looked at:
- `Logs/shots/n13-1-title.png`: PLAY selected (the cyan frame), the three mode lines (SURVIVE THE ROUNDS / ONE SEED A
  DAY / HARDER RULES, BIGGER SCORE), padlocks on DAILY and HEAT 1-3, no AIM ASSIST, the new controls line (1-0 WEAPONS ·
  TAB WEAPON WHEEL · Q LAST WEAPON), the stamp `SKYGRIND 0.1.0 (editor)` bottom right.
- `n13-2-contracts.png`: `CONTRACTS · 0 / 28 DONE`, in-progress rows first with bars, a reward on every row (`REWARD:
  [padlock] MOD: SHRAPNEL`, `REWARD: THE BADGE`).
- `n13-3-controls-rebind.png`: the four columns; BACK's cell reads PRESS A KEY in cyan; the web's rows the port lacks
  are listed as they are (PHOTO MODE F2: agent I's note).
- `n13-4-gamepad-tab.png`: the GAMEPAD page (17 actions with the neutral labels), the `GAMEPAD CONNECTED · SETTINGS >
  GAMEPAD` hint; the selection frame sits on the KEYBOARD + MOUSE tab while the GAMEPAD label is the active page.
- `n13-5-spawn-view.png`: no orange striped wall 16 m ahead (S3), THE GRINDER's price sign, the hotbar (1 KESTR, 2
  PULSE), KEY 1 / KESTREL FRESH / RMB ZOOM x1.5, the POINTS plate, the one-line FIRST RUN lesson.
- `n13-6-radial.png`: 1 · KESTREL up, 2 · PULSE GAUNTLET down and highlighted (cyan), the pointer dot.
- `n13-7-paused.png`: RESUME selected, RESTART, SETTINGS, CONTROLS, AIM ASSIST: SNAP, SAVE & QUIT (shown at the break),
  COPY REPORT, QUIT TO TITLE, the YOUR KEYS card.
- `n13-8-resume-title.png`: RESUME under PLAY after SAVE & QUIT; PLAY keeps the focus (a question for GG: should RESUME
  be the primary control when a save exists?).

## Reviews (reviewers 2 of 2, read-only; B ran 28 min and reported by 8:38 PM, A ran 41 min; fixes 8:38-9:01 PM)

Two read-only agents on `d5c30d7..HEAD`: A on per-frame / per-step allocations and PORT-RULES section 6, B on the new
flows' correctness (save / resume, input, menus, the sim split). Neither edited a file or ran Unity.

**A (allocations):** no per-step and no per-frame allocation in tonight's code; every input, UI and quality path it
traced is clean. Its other findings:
- LOW: the sim reads settings by string key on the step path. AimAssist's mode / strength go through
  `settingStr("aimAssist")` and `settingNum("snapStrength")`. These are dictionary hits, not allocations, against
  SettingsView's own header.
- LOW: `UiFeel.RenderFeel` calls `cfg.Num` six times a frame (the paths are cached, so nothing allocates; still against
  NIGHT11's "no config lookups on the frame path").
- MED (perf): PostFx's bloom / chroma `.Equals` checks fail almost every frame of play, so `UpdateVolumeStack` runs
  anyway.
- MED: `SettingsView.Write` fsyncs on the main thread, and the X-ray key saves settings in play: a possible hitch, not
  measured.
- MED: `PerkMachines.Render` calls `sim.RideCore` from the render path, which moves a buyable. Older behaviour, moved
  tonight; the fingerprint is still equal.
- LOW: Step's profiler marker pairs have no try / finally.
- Gameplay A-A .. A-D, as B's 1, 3 and 9 plus one more (A-C).

**B (flows), ten findings:**
1. HIGH: rebinding RESTART / LOCK ON / X-RAY doesn't move them, and the old key fires both actions. HotKeys and EnemyLook
   read the config's keys once.
2. MED: a DAILY saved and resumed on a later UTC day records that day's run late and resets the streak.
3. MED: the shown key labels ignore rebinds (`Ui.KeyLabel` cached the config).
4. MED: the MODS draft selects its first card as it opens, so a jump's A (the pad's submit) can take it unseen.
5. LOW-MED: RESUME's StartRun sends round 1's break first: round 1's lesson / break tip, and a same-day DAILY's banner.
6. LOW: the HUD perk row compares only the count, so it can show another run's icons after RESUME.
7. LOW: RESUME leaves TITLE's RUN SETUP on SURVIVAL and the saved HEAT rung.
8. LOW: Q after RESUME goes to slot 0 (the snapshot doesn't hold the last weapon).
9. LOW: the pad's weapon cycle is lost without a locked cursor (or without a mouse device).
10. LOW: a failed delete after the read is logged as "could not read".

Also checked by B and fine: what the snapshot holds, file handling, `design.save` off, no-pad parity, the double-press
guards, and the sim split's order against u3-n12.

**Fixed in `aaba74c`** (Game side only, no Core):
- B-1 / A-A: HotKeys and EnemyLook re-read `KeyCodes.Bound` whenever `bindings.Version` moves; the config's keys are
  dropped.
- B-3 / A-B: `Ui.KeyLabel` reads `Ui.LiveBindings` and clears its cache on a rebind.
- A-C: AIM ASSIST's button writes `padAimAssist` while the pad is the device (the mode `AssistMode` reads); before, it
  wrote the mouse's and changed nothing on a pad.
- B-9 / A-D: the pad's selection reaches the loadout without the cursor, never while inactive.
- B-2: `CanSaveAtBreak` refuses a DAILY. `RunRestoreTests.SaveRule_RefusesADailyRun` was **seen red** with the rule
  removed (`a DAILY run may not: Expected False, But was True`), then green.
- B-7: RESUME restores TITLE's mode and HEAT after StartRun.
- B-4: the draft selects nothing until a menu move. The keys 1 / 2 / 3 and clicks need no selection. The web focuses
  card 0, so this differs from the web: D-52 in HANDOFF.

Gates: compile clean, then tests `594 / 594 passed`, replays 22 / 22, golden 91 / 91 (9:00 PM).

**Not fixed** (HANDOFF section 6): B-5, B-6, B-8, B-10, A's per-frame / per-step lookups, the fsync, PostFx's dirty
compare, RideCore on the render path, the markers. Also: three key labels built once at boot (the hotbar's radial slot,
the radial prompt, DOWN's R RETRY NOW) stay on the old key until the next launch after a rebind.

## Build 5, the final build (9:03-9:13 PM)

- **Build 5** (9:03:43-9:04:03, `Succeeded`, 0 errors, 8 warnings, 160,468,682 B; builds 5 of 5). The stamp is
  `aaba74c-dirty`: HANDOFF.md and NIGHT13-LOG.md were uncommitted, so TITLE shows `(aaba74c-dirty)`; the code is
  `aaba74c`. `git diff --stat ProjectSettings` showed only UnityConnectSettings.asset; no Mobile_RPAsset rewrite.
- `-skygrindGolden`: `golden total: 34 vector sets (31481 records), 22 replays (22 exact); 0 failing, 0 mismatches`.
- `-skygrindFingerprint`, twice: `seed 1: 12 hashes over 7200 steps in 0.41 s: EQUAL to the baseline; at the end: run
  Fight round 4, kills 36, shots 80, earned 4310` (0.40 s the second time); seed 2: 0 of 12 equal.
- Smoke: `[smoke] 125.2s DONE: all steps passed`, 0 exceptions, steps 13-17 as on build 4 (`save and resume: allowed at
  round 2's break True ... equal True, file consumed True`; `virtual pad: ... A played True`; `virtual pad menus: ... B
  backed out True`).
- Logs: `Logs/player/golden-b5.log`, `fp-b5-1.log`, `fp-b5-2.log`, `smoke-b5.log`.
- **The series on build 5** (9:09-9:13 PM). Before it: idle 488 s, CPU 6.6-12.8 %, GPU 0 %; the resident llama-server /
  ollama / python processes each used under 0.2 % over 5 s. The plan was found Balanced, set to Silent for the series and
  restored to Balanced after; every run read `Silent AC=True` before and after.

| Series | Runs: worst 1% (p99) | Median | FrameTiming cpu worst 1% | Frames over 6.94 ms |
|---|---|---|---|---|
| b5 (UI-hurt, the bar's scenario) | 4.08 / 3.98 / 4.45 | **4.08 ms** | 4.10 / 3.99 / 4.48 ms | 0.22 / 0.26 / 0.37 % |

- The bar (D-34: 6.9 ms) is met on the final build too. Build 4's series read a median of 3.98 ms; one run's spread is
  larger than the 0.10 ms between the medians. Bench runs 19 of 24.

## Item status lines (times from `date`)

- **U3-26.2 DONE (7:08 PM):** the series on build 1 (median 4.81 ms) and its breakdown table (above); `192bff9`.
- **U3-26.4 DONE (8:05 PM):** the switches exist and A/B'd (all on median 5.42 ms vs all off 7.10 ms on build 2), the
  bar met on the final build (median 3.98 ms), the pacing arms run; SHORT sub-parts: the IL2CPP trial (blocked: no
  module / C++ toolchain), the HighQoS arm (no hook), the REDUCED MOTION bob (the web keeps it). `2421dfb`, `ffafa2b`.
- **U3-26.7 DONE (7:55 PM):** build 4 smoke `virtual pad: ... A played True, the stick moved 2.4 m and turned True, the
  trigger killed True, START paused True, B resumed True`; fingerprint unchanged. `287c192`.
- **U3-26.8 DONE (7:55 PM):** build 4 smoke `rebind: jump -> KeyJ after reloading the store` and `virtual pad menus: ...
  right moved a value True ... B backed out True`; captures 3 and 4. `da7b43a`, `c4d4ff1`.
- **U3-26.9 DONE (7:55 PM):** build 4 smoke `radial: 2 guns, shown on the HUD True, highlighted 1, the loadout's slot 1`;
  capture 6. `80786c5`, `b65e5c8`.
- **U3-26.10 DONE (7:55 PM):** RunRestoreTests 3 / 3 on the game's list; build 4 smoke `save and resume: ... equal True,
  file consumed True`; captures 7 and 8. `be2b609`.
- **U3-26.11 DONE (8:12 PM):** F's 12 commits + `0ead5b9` (S8); captures 1, 2, 5, 7.
- **U3-26.12 DONE (7:55 PM):** build 4 smoke `run log: ..., 3 lines (header first: True, footer last: True)`; the stamp
  `SKYGRIND 0.1.0 (ffafa2b)`; DevKeysTests 3 / 3; `git status` clean after builds 2-4. `04babc0`.
- **After the reviews (9:08 PM):** build 5 (`aaba74c`) repeats every smoke step above, with `[smoke] 125.2s DONE: all
  steps passed`; the U3-26.7 / .8 / .10 lines hold on it, and the fixes touch only those items' input and save paths.
- **U3-26.13 DONE (9:30 PM):** the final Release build (build 5) with smoke, golden and fingerprint; the final Silent
  series (median 4.08 ms); the reviews and their fixes (`aaba74c`, tests 594 / 594). Also tasks.json, PORT-STATUS /
  PORT-PLAN / PORT-RULES, NIGHT13-REPORT.md, DESIGN-PILLARS.md (DRAFT), HANDOFF.md, NIGHT14.md (refreshed, untracked),
  the memory notes, and the tag `u3-n13` on the close commit.

## Close table

| Item | Status | Evidence |
|---|---|---|
| U3-26.1 received | DONE | `d5c30d7`; RECEIVED row 4 `tests: 501 / 501 passed, 0 failed, 0 skipped`, replays 22 / 22, golden 91 / 91 (3:00 PM) |
| U3-26.2 measure first | DONE | `192bff9`; build 1 series median 4.81 ms and the breakdown (worst 1 %: engine main thread 7.48 vs 2.22 ms) |
| U3-26.3 shared types | DONE | `2a163b1`, `11ec1b2` |
| U3-26.4 the frame budget | DONE | `2421dfb`, `ffafa2b`; build 4 median 3.98 ms, build 5 median 4.08 ms on Silent (bar 6.9). Short sub-parts: IL2CPP (no module / toolchain: GG's call), HighQoS (no hook), the REDUCED MOTION bob (the web keeps it) |
| U3-26.5 SurvivalGame split | DONE | `d882659`; the fingerprint equal, suite green (4:48 PM) |
| U3-26.6 the sim composition | DONE | `e1645cf`; FingerprintTests headless equal to the baseline; build 5 `-skygrindFingerprint` `EQUAL to the baseline` twice |
| U3-26.7 gamepad | DONE | `287c192`, `aaba74c`; build 5 smoke `virtual pad: ... A played True ... B resumed True` |
| U3-26.8 settings, CONTROLS, pad menus | DONE | `da7b43a`, `c4d4ff1`, `aaba74c`; build 5 smoke `rebind: jump -> KeyJ after reloading the store` |
| U3-26.9 hotbar and radial | DONE | `80786c5`, `b65e5c8`; build 5 smoke `radial: 2 guns, shown on the HUD True, highlighted 1, the loadout's slot 1` |
| U3-26.10 save-and-quit | DONE | `be2b609`, `aaba74c`; RunRestoreTests 4 / 4; build 5 smoke `save and resume: ... equal True, file consumed True` |
| U3-26.11 first impressions | DONE | F's 12 commits + `0ead5b9`; captures 1, 2, 5, 7 |
| U3-26.12 production basics | DONE | `04babc0`; build 5 smoke `run log: ..., 3 lines (header first: True, footer last: True)` |
| U3-26.13 close | DONE | build 5 smoke `[smoke] 125.2s DONE: all steps passed`; golden `0 failing, 0 mismatches`; docs; tag `u3-n13` |
