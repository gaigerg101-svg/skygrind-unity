# Audit: the bench's worst 1% and the code's organisation (2026-10-04, read only)

Scope: `port/stage3` at `07f7885` (tag u3-n12), the Player bench logs in `Logs/player/`, NIGHT11 / NIGHT12 logs,
`HANDOFF.md` section 4, `PORT-RULES.md` section 6, the code, and the project / URP settings. No Unity command, build,
test or Player was run. Every number below comes from a named log or file. "Not checked" means not checked.

## 0. Bottom line

- **The worst 1% is not the sim.** About 1 frame in 3 runs one 120 Hz step (120 / 368 mean fps). A step costs about
  0.27 ms: 1887.3 ms of `SKYGRIND.SimStep` in 60 s, so 7200 steps (`bench-dev.log:116`), and 959.2 ms in 30 s
  (`bench-prof.log:140`). The p99 sits 4.4-4.8 ms above the median. A frame runs two steps only when it lasts more
  than 8.33 ms, so frames at the p99 hold one step at most. The script markers sum to about 0.6 ms of a 4.39 ms Editor
  frame (NIGHT12-LOG.md:202-205).
- **What changed n11 → n12 is the tail only.** The median held (2.47 ms → 2.43-2.56 ms) and p95 barely moved (4.11 →
  4.19-4.23 ms). The p99 rose by 1.3 ms (5.55 → 6.84 ms on Balanced). The GPU p99 stays at 1.4-1.6 ms. Most of
  NIGHT12's per-frame work is render-side, and its scripts measured small. So the delta is most likely engine-side:
  the URP render thread, Present, or canvases. Second most likely is GC or the bench's environment.
- **The bench is noisy enough to hide the 1.3 ms signal.** The same build on the same plan measured 4.64 ms and
  7.00 ms (`bench7.log:40`, `bench6.log:40`). The bench script starts a PowerShell + WMI query every poll
  (`Tools/survival-bench.sh:14-16`), 16 times in a 70 s run.
- **Incremental GC uses fixed 3 ms slices here.** `gcIncremental: 1` (ProjectSettings.asset:848), with vsync 0 and no
  frame cap (the bench prints `vsync 0, cap -1`; `UnityTuning.cs:60` VsyncDefault false). In that case Unity's
  documented behaviour is a fixed `incrementalTimeSliceNanoseconds` (default 3 ms) per frame while a collection runs.
  The release Player allocates about 1 MB/s.
- **The one measurement to run next:** a per-frame table from the bench. Columns: frame time; FrameTiming main /
  render / present-wait; sim steps run; GC count; then the worst 1% frames compared with the median band. It works in
  the release Player. A Development run with the existing `SKYGRIND.*` recorders names the script share.
- **Architecture:** Core is clean and well tested: 391 test attributes, golden and replays. The Game side has a
  1,506-line god object (`SurvivalGame`, 75 public members) and a 4,130-line partial `Ui`, and almost no tests. No
  test pins the composed `simulate()` order; only the two smokes cover it. First move: a read-only "composed
  fingerprint". After it: split files, then extract the sim composition, then the run snapshot for save-and-quit.

## 1. The perf evidence

Release Player, 60 s, 24 husks spawned + director (31 alive at end), 1920x1080 windowed (`survival-bench.sh:19`):

| Log | Build / plan | median | p95 | worst 1% | FTM cpu p99 | GPU p99 | GC (collections, Used rise) |
|---|---|---|---|---|---|---|---|
| NIGHT11-LOG.md:287-289 | u3-n11 / Balanced (12 polls) | 2.47 | 4.11 | 5.55 | - | - | 5, +54,476 KB |
| bench.log:40-42 | u3-n12 / Silent | 2.74 | 4.59 | 7.62 | 7.64 | 1.42 | 3, +57,200 KB |
| bench2.log:40-42 | u3-n12 / Silent | 2.79 | 4.32 | 7.38 | 7.40 | 1.43 | 3, +57,336 KB |
| bench3.log:69-71 | u3-n12 / Balanced, GG's jobs running (void) | 4.49 | 7.09 | 10.15 | 10.51 | 1.42 | 3, +50,328 KB |
| bench4.log:40-42 | u3-n12 / Balanced | 2.43 | 4.19 | 6.90 | 6.93 | 1.54 | 3, +58,484 KB |
| bench5/6/7.log:40 | u3-n12 / Balanced | 2.55 / 2.47 / 2.46 | 4.19 / 4.23 / 4.00 | 6.84 / 7.00 / **4.64** | 6.87 / 7.05 / 4.65 | 1.51-1.57 | 3 each, ~+58 MB |
| bench8/9/10.log:40 | u3-n12 / Silent | 2.49 / 2.58 / 2.56 | 4.20 / 4.24 / 4.17 | 7.13 / 7.25 / 7.19 | 7.15 / 7.31 / 7.22 | 1.51-1.54 | 3 each, ~+58 MB |
| bench-dev.log:92-116 | u3-n11 Development / Balanced (8 polls) | 2.50 | 3.84 | 5.08 | 5.16 | 1.56 | 18, +120,152 KB, 3864 B/frame, sim 1887.3 ms |
| bench-prof.log:116-140 | same, 30 s with a 300-frame profile | 2.48 | 3.77 | 4.76 | 4.81 | 1.53 | 9, 4011 B/frame, sim 959.2 ms |
| n12-before-bench.log:40 | u3-n11, 15 s, plan not polled | 2.17 | 3.56 | 4.25 | 4.34 | 1.09 | 2, +40,744 KB |

- Each `.power` file shows one plan for the whole run: 12-16 polls of `Balanced AC=True` or `Silent AC=True`.
- **A discrepancy.** NIGHT12-LOG.md:63 says the u3-n11 15 s run printed `worst 1% 6.20 ms`. The file on disk shows
  4.25 ms. Two runs were made for that capture (NIGHT12-LOG.md:58-62), so the file probably holds the other run.
  Either way, the same build spread by about 2 ms.
- **Allocation profiles:**
  - n11 Development: 3716 B/frame. 3660 B of it fell outside every `SKYGRIND.*` marker. Fixed: WallBuys, Loadout,
    IMGUI (NIGHT11-LOG.md:83-108).
  - n12, Editor Play mode, 4231 B/frame. `SKYGRIND.Brain` 3310 B (read as "the spawn's warm-up"), `R.Ui` 663 B,
    `Buyables` 125 B, `R.Input` 68 B (NIGHT12-LOG.md:170-174). The last two were fixed in `ea50685`.
  - The release Player still rises about 58 MB per 60 s. At ~360 fps that is about 2.7 KB/frame, not attributed.
- **Marker costs** (Editor, 24 husks, 300 frames, NIGHT12-LOG.md:202-205): SimStep 0.146 ms/frame, plus the render
  markers. R.Projectiles 0.139, R.Enemies 0.078, R.Ui 0.062, R.Props 0.050, R.Viewmodel 0.040, R.Light 0.037,
  R.Signs 0.017, R.Fx 0.010, R.Input 0.008, R.Feel 0.007, R.World 0.007. Total ≈ 0.60 ms of a 4.39 ms frame.
- **Machine** (every bench log, lines 8-15 and 38): RTX 5070 Laptop GPU, AMD Ryzen AI 9 HX 370 "w/ Radeon 890M",
  24 logical processors, D3D12, `kGfxThreadingModeSplitJobs` (graphics jobs on), Mono (`Mono path[0]`, lines 1-2).
  The bench Player is a Release build: no PlayerConnection lines, and it prints `GC Allocated In Frame n/a (release
  Player)`.

**What the bench exercises** (`Game/PlayerChecks.cs:90-154`):
- seed 99, every gate open, god mode, director.maxAlive husks.
- The player never moves or shoots (`:136`), so the nav fields seldom restart (`Core/NavField.cs:519-520`), and
  weapons, kills, hitstop and particles are idle.
- The player is hurt continuously and HP is reset each frame (`:153`). That keeps the HUD's HP text, the hurt
  vignette and the hit arcs changing (`Ui.cs:890`, `:1116-1123`, `:1052-1063`). It is a UI-heavy scenario, not a
  combat one.

## 2. Ranked suspects (estimated shares of the p99's ~4.5 ms over the median: estimates, not measured)

1. **The render thread / Present path. Estimate ~35% (the likely home of the n11 → n12 +1.3 ms).**
   - Evidence. The median hasn't changed while the tail grew, and the GPU is idle-ish. Script costs are small, so most
     of each frame is engine work. NIGHT12 added engine work with little script time:
     - an x-ray second material on each husk body (`EnemyViews.cs:167`), with per-part MaterialPropertyBlocks
       (`:175-182`, `:508-509`). An MPB makes a renderer SRP-Batcher-incompatible; MPBs existed at u3-n11 too
       (`git show u3-n11:...EnemyViews.cs`, 6 SetPropertyBlock calls).
     - corpse `Graphics.RenderMesh` per part (`:358-372`), the motes, the blob shadows, kill rings and movement-FX
       instanced draws.
     - 3 pool lights plus a flash and a guide light in Forward+ (`LightPools.cs:85-90`, `:129-131`).
     - seven Volume overrides written each frame (`PostFx.cs:105`, `:125`).
     - the animated HUD (UiHudMotion, UiCeremony).
   - The pipeline also runs:
     - two cameras: the viewmodel overlay (`Viewmodel.cs:187-194`) keeps URP defaults (renderShadows not set).
     - Forward+ (`PC_Renderer.asset:56`) and SSAO at full res from a DepthNormals prepass (`:73-78`).
     - 4 soft cascades at 2048 over 60 m (`PC_RPAsset.asset:45-66`).
     - the depth and opaque textures (`:22-23`), HDR and SMAA.
   - On this laptop the panel may hang off the Radeon 890M while rendering is on the RTX. In that case each Present
     crosses adapters (not checked).
   - **Settle:** FrameTiming per frame: `cpuMainThreadFrameTime`, `cpuRenderThreadFrameTime`,
     `cpuMainThreadPresentWaitTime`, `gpuFrameTime`. `enableFrameTimingStats: 1` (ProjectSettings.asset:157), so this
     works in Release. Compare the worst-1% frames with the median band. If the spike frames are render-thread or
     present-wait time, this is the culprit. Then read one of those frames in the Profiler binary log (the NIGHT11
     method), with Unity's `Gfx.*` / `Canvas.*` / culling markers (exact names in 6000.3 not checked).

2. **The bench environment. Estimate ~25%, and all of the run-to-run spread.**
   - Evidence. Identical runs gave 4.64 / 6.84 / 7.00 ms on Balanced (`bench5-7.log`). The 15 s u3-n11 runs gave 4.25
     vs 6.20 ms. Silent's median of 3 is 0.35 ms over Balanced's. A loaded machine gave 10.15 ms (`bench3.log`).
   - The poller runs `powershell -NoProfile ... Get-CimInstance` (`survival-bench.sh:14`) every 2 s plus its own run
     time. That is 16 polls in a 70 s run, so a PowerShell / WMI process is alive for much of the bench.
   - The HX 370 mixes Zen 5 and Zen 5c cores (AMD's spec; not checked on this machine). The main thread or the render
     thread landing on a compact core, or a power-limited plan, stretches frames.
   - **Settle:** an A/B of three runs each. Poller as-is, versus one poll before and one after (or `powercfg` only,
     every 10 s). Optionally a third arm with the Player's affinity limited to the full cores (the mask is not
     checked). Log poll timestamps so spikes can be lined up with them.

3. **Incremental GC at a fixed 3 ms slice. Estimate ~20%.**
   - Evidence. The release Player allocates about 1 MB/s and collects 3 times a minute (n11: 5 times). Uncapped, Unity
     gives each frame up to `incrementalTimeSliceNanoseconds` (default 3 ms; Unity's scripting reference) while a
     collection marks. A slice frame is about median + 3 ms ≈ 5.5 ms, which is near the p99 plateau.
   - Each collection must span about 70 frames to fill the whole 1% (216 of ~21,600 frames). Short collections would
     only touch the top 0.1-0.3%. The heap size is not logged.
   - The bimodal series fits a GC knee (4.64 vs ~7 ms) but proves nothing.
   - Remaining allocators (per event, not per frame):
     - `new HurtEvent` per hurt (`Core/Health.cs:62`).
     - `new HurtSource` per hurt through `DeathRecap.SourceName` (`Core/DeathRecap.cs:57-58`).
     - the style feed's interpolated string per award (`Ui.cs:320`).
     - payloads per shot / hit / kill (`Core/Combat.cs:648`, `:777`, `:838`, `:1039`, `:1046`).
     - the rest of the ~2.7 KB/frame is not attributed.
   - **Settle:** log `GC.CollectionCount(0)` and `GC Used Memory` per frame in the same table, and test whether spike
     frames cluster before each count change. Or run once with `GarbageCollector.incrementalTimeSliceNanoseconds = 1 ms`:
     the p99 should drop by about 2 ms if GC sets it. In a Development run, add a recorder on the GC markers (names
     not checked).

4. **NIGHT12's render-side scripts and the HUD. Estimate ≤10%.**
   - Evidence. Script markers are tiny: R.Light 0.037, R.Fx 0.010, R.Enemies 0.078, R.Ui 0.062 ms/frame.
   - `LightPools.UpdatePool` turns every steady light off (`Set(…, 0)` → `light.enabled = false`, `LightPools.cs:292-296`)
     and back on (`:312-316`) every frame. That is a Behaviour disable/enable per slot per frame, a cost outside the
     script marker.
   - Canvases: one root plus 4 nested (`Ui.cs:182-196`, `:498-508`) plus the dash tint (`MovementFx.cs:219`). Texts are
     set only on change (`Ui.cs:20-22`), but the bench's constant hurt dirties "HUD values" and "HUD sight" every frame.
   - **Settle:** per-frame recorders on the existing `SKYGRIND.R.*` markers (`SurvivalGame.cs:1150-1153`,
     `LightAndLens.cs:40`). Report their mean over the worst-1% frames.

5. **The fixed loop's step frames. Estimate ≤5-10%.**
   - Steps per frame are 0 or 1 below 8.33 ms (`SurvivalGame.cs:999-1010`). A step costs ~0.27 ms.
   - AI thinking is staggered (`HuskBrain.cs:532`, `:626-633`), LOS runs on a budget (`:639`), and the flow fields
     advance on a pop budget (`NavField.cs:514-528`).
   - Only sums are logged, so a rare heavy step (a director spawn, a sync recompute) is not excluded.
   - **Settle:** the existing `SKYGRIND.SimStep` recorder (`PlayerChecks.cs:113`), read per frame instead of summed,
     with its max and its mean over the worst 1%. In Release, a Stopwatch around the accumulator loop exposed as a
     field gives the same column.

6. **Mono. Estimate ≤5-10% of the tail.**
   - `scriptingBackend` lists only Android (ProjectSettings.asset:836-837), so Standalone is Mono, as the logs confirm.
   - IL2CPP usually speeds script work 1.5-3x, but script is ~0.6 ms of the frame.
   - **Settle:** one IL2CPP Player run of the bench, gated by `-skygrindGolden` (0 mismatches over 34 sets / 22
     replays) and `-skygrindSmoke` before any number counts. Double arithmetic under IL2CPP's C++ is not checked.

Also noted:
- `QualitySettings.asset:86` vSyncCount 0. `targetFrameRate` -1 (`SurvivalGame.cs:662-666`, fpsCap default 0).
  `maxQueuedFrames` is not set in the project (default, not checked).
- Graphics jobs: project default (ProjectSettings.asset:533-534 empty); the log shows split jobs.
- SRP Batcher on (`PC_RPAsset.asset:72`). GPU Resident Drawer off (`:86`). Volume update every frame (`:94`).
- Quality: Standalone uses level 1 "PC" → `PC_RPAsset` (`QualitySettings.asset:104`, `:129`).
- First-use PSO / shader hitches are a likely p99.9 source. `m_PreloadedShaders: []`, no warm-up: they hit real play,
  not the p99 of a 60 s bench.

## 3. Cheap, safe wins (no gameplay change; none touches Skygrind.Core except where marked)

1. **Bench hygiene** (`Tools/survival-bench.sh:13-16`): poll the plan and AC once before and once after, or
   `powercfg` only every 10 s. Print the poll times. Run three runs and report the median, as now.
2. **Per-frame breakdown in the bench** (`PlayerChecks.cs:107-171`): add the FrameTiming main / render / present-wait
   columns, a steps-this-frame counter and the GC count. Print the median-band and worst-1% means per column, and
   optionally a CSV. Measurement only.
3. **A second bench scenario** that plays: the existing automation (`SurvivalGame.forcedMove` / `forcedCombat`,
   `:103-105`) strafes and fires, so weapons, kills, hitstop, particles, pops and nav restarts are measured. Today's
   scenario measures a UI-hurt loop.
4. **GC slice.** Set `GarbageCollector.incrementalTimeSliceNanoseconds` from a commented `UnityTuning` constant (for
   example 1 ms) while uncapped. Render / timing only. Measure the heap and collection count after, since
   collections run longer.
5. **LightPools:** pick each slot's final intensity, then call `Set` once per slot per frame (`LightPools.cs:280-318`).
   No light is disabled and re-enabled within a frame.
6. **URP asset:**
   - Opaque Texture off (`PC_RPAsset.asset:23`): no project shader samples `_CameraOpaqueTexture` (grep of
     `Assets/Shaders`).
   - Check Depth Texture (`:22`) against SSAO's own DepthNormals source in the Frame Debugger first.
   - Set the overlay camera's `renderShadows = false` and keep its post off (`Viewmodel.cs:192-193`). Whether URP
     renders a shadow pass for it is not checked.
7. **Allocation leftovers on the Game side:** the feed string per award (`Ui.cs:320`; cache per award name / points
   or build it in the existing StringBuilder). The Core ones (`Health.cs:62`, `DeathRecap.cs:57-58`) go through the
   golden / replay gate. A reused payload is safe only if no handler keeps it: read `HuskAttackEvent`'s note in
   `EventBus.cs:7-8`.
8. **An IL2CPP trial** behind the Player golden gate (section 2, item 6). Not "safe" until the gate is green.

## 4. The architecture as it is (measured)

**Size.**
- 160 `.cs` files, 46,816 lines in `Assets/Scripts`:
  - Core: 95 files, 30,094 lines (Golden: 13 / 4,534).
  - Game: 62 files, 15,725 lines.
  - Bench: 3 files, 997 lines.
- Editor: 4 files, 370 lines. Tests: 65 files, 10,665 lines.
- Top 15 files: JsMath.Fdlibm 2,160 · **SurvivalGame 1,506** · Mover 1,302 · **Ui 1,169** · Combat 1,110 ·
  Viewmodel 948 · EnemyViews 933 · HuskBrain 926 · NavField 733 · Director 725 · SliceSmoke 638 · AimAssist 636 ·
  MovementPresets 616 · GoldenPorted 607 · Projectiles 601.

**Assemblies.**
- `Skygrind.Core`: `noEngineReferences: true`, no references.
- `Skygrind.Game`: Core, InputSystem, URP / RP Core runtime, TMP, UGUI.
- `Skygrind.Bench`: Core, Game, Burst, Collections, Mathematics, InputSystem. All platforms and autoReferenced, so it
  compiles into the Player; whether a built scene uses it is not checked.
- `Skygrind.Core.Tests` and `Skygrind.Game.Tests`: Editor only.
- `Assets/Editor` has no asmdef (Assembly-CSharp-Editor).
- `Tools/CoreHarness` compiles `Assets/Scripts/Core/**` on net10.0 to run the oracle outside Unity.

**SurvivalGame.cs by concern** (one MonoBehaviour, 75 public field / property lines, `Instance` static at `:249`):

| Lines | Concern |
|---|---|
| 23-150, 226-285 | Fields: systems, views, meta, automation flags (`forcedCombat` 103, `forcedMove` 105, `noRecords` 126, `profileReadOnly` 147, `noLab` 229); the hook events 251-255 |
| 153-225 | Meta / tips glue: SaveProfile, SettingOr, Tip, TipForCandidate, XrayHint, KeyLabel |
| 288-322 | Boot: Awake, BootWeb (WebGL) |
| 324-605 | **Init, the composition root:** world / look / camera 324-353; player systems, AI and Func wiring 354-431; views, box, run shape, perks, mods, forge 432-480; FX, trauma, hitstop and recap handlers 481-567; profile + meta 568-605 |
| 607-625 | StartGame: the run, Ui, title, signs, `PlayerChecks.Attach` |
| 627-702 | MakeMaterials; helpers (ApplyFrameCap, KeyOf, SurvivalTarget, Hitstop, Respawn) |
| 704-967 | Flow: ToTitle, ResetWorld, StartRun (seed, resets in order, meta, HEAT, DAILY), EndRun (stats, grade, records, contracts, save), Restart, RetrySameSeed, PickMod, Pause / Resume |
| 971-1077 | **The loop:** input 974-991; accumulator + hitstop 992-1012; the render half 1013-1077 |
| 1079-1148 | UpdateGuide (the first-run guide light) |
| 1150-1160, 1163-1251 | Markers; **SimStep in main.js simulate() order** |
| 1253-1340 | BuildBox, BuildRunShape, PlaceBox |
| 1342-1380 | EnemyNear, StepMode (DOWN timer, run.Step, exfil outcome) |
| 1382-1414 | HotKeys (F3 / F8 / F9 debug, R, V, Esc, draft keys) |
| 1416-1474 | Run hooks; **the HUD line's text** (RunShapeKey, AppendRunShape: UI glue) |
| 1476-1505 | DebugLine, DebugOverlay, IdleSpawner |

**Ui.**
- One `partial class Ui` across Ui.cs plus 16 partial files: 4,130 lines. Largest parts: UiMeta 543, UiResults 372,
  UiOnboarding 289, UiHudMotion 237. Plus the TitleView partial.
- It shares about 200 fields, with `partial void` hooks (`Ui.cs:441-444`).
- It builds one root canvas and 4 nested layers (frame, values, sight, menus).
- `RunShapeKey` / `AppendRunShape`, which build the HUD line, live in SurvivalGame.

**How systems find each other.**
- Core systems get dependencies through constructors and about 40 `Func` / `Action` closures wired in `Init`. Some
  examples: `economy.campMult` 389, `regen.camping` 390, `style.getFresh` 417, `mods.isUnlocked` 459,
  `loadout.bonus` 465.
- 25 Game files take `SurvivalGame` in a constructor and read its public fields. Statics: `SurvivalGame.Instance`
  (used by PlayerChecks, SliceSmoke, DebugOverlay), `EnemyLook.shared` (`EnemyViews.cs:91`), `Look.Post`, Ui's static
  fonts and name caches (`Ui.cs:114-115`, `:717-734`).
- The EventBus (`Core/EventBus.cs`) is string-keyed, synchronous, and **runs handlers in registration order**
  (`:27-31`, `:51`). SurvivalGame registers 18 handlers itself. Handler order is part of the behaviour.
- **A render → sim dependency, kept from the web:**
  - The brain's watchdog asks `OnScreen`, which reads the camera's viewport (`SurvivalGame.cs:272-276`, `:378`). The
    camera is placed by the render half, so a composed run is not a pure function of its InputFrames.
  - `zones.Track` runs in the render half (`:1033`). Only the HUD listens to `zoneEnter` (grep).

**Config.**
- `GameConfig`: dotted paths, cached `Get`; `Has` splits the path on every call (`GameConfig.cs:66-74`).
  `MovementConfig` is typed.
- `Design/unity-config.json`: 31,863 bytes, 22 `_why` entries, merged under `design.` (`GameConfig.cs:28-35`).
- `UnityTuning`: 6 partial files, 327 constants (Ui 143, Meta 87, base 40, Enemy 20, Feel 20, Light 17).
- Settings: `GameSettings` over Core's `SettingsStore` (a `Dictionary<string, object>`), read by string at call
  sites: per frame `SurvivalGame.cs:979`, `Ui.cs:1160`; per hurt `:566`; through a closure `:362`.

**Input.**
- `InputReader` / `CombatReader` latch per frame and sample per step into Core's `InputFrame` / `CombatInput`. That is
  already the right seam.
- The keys are hard-coded (`InputReader.cs:50-71`, `CombatReader.cs:26-59`), though their headers cite
  `config.input.bindings`. Only restart, lockOn and xray read the bindings (`SurvivalGame.cs:601-602`,
  `EnemyViews.cs:124`). There is no gamepad.

**Tests.**
- 65 files, 391 `[Test]` / `[TestCase…]` attributes (HANDOFF counts 501 cases):
  - run shape 103, meta 85, weapons 76, movement 59, AI / director 37, oracle / infra 17, feel 10, Game 4.
- `Skygrind.Game.Tests` (`Skygrind.Game.Tests.csproj` compiles `Assets\Tests\EditMode\Game\*.cs`) holds
  CollisionParityTests (3) and UnityReplayTests (1): the spike's PhysX-parity checks. **Nothing tests SurvivalGame,
  Ui or the views.**
- `ZeroAllocTests` has 5 lines.
- The golden sets cover Core modules one by one. `GoldenRunShape` replays the run-shape systems with fakes. **The
  composed SimStep order is guarded only by the Editor and Player smokes.**

**Tools.**
- `unity-loop.sh` (57 lines), `survival-bench.sh` (28), `serve-web.cjs` (34).
- `CoreHarness` (37 lines plus a csproj), `EnemyBench/` (run.sh, summarize.mjs, results).

**Duplication and dead code.**
- The "weapon family colour" lookup is written 6 times, with different fallbacks: `KillConfirm.cs:77`,
  `LightPools.cs:162-171`, `SurvivalGame.cs:504-508`, `Viewmodel.cs:347`, `WallBuys.cs:95`, `WeaponFx.cs:178-181`.
- The "KeyX → X" label is parsed 3 times: `SurvivalGame.cs:219-225`, `:672-676`, `Ui.cs:362-365`.
- Husk / Swarmer / Spitter views repeat one pool-and-render pattern (`EnemyViews.cs:390-828`).
- `Mulberry32` lives in `CoreGrid.cs:12`. Its state `a` is private with no accessor.
- Dead or vestigial:
  - `BeforeAi` / `AfterPlayerSystems` have no subscriber, yet are invoked every step (`SurvivalGame.cs:252-253`,
    `:1190`, `:1241`).
  - `IdleSpawner` is replaced at `:381`.
  - `bool click; _ = click;` (`:1399-1401`).
- Markers: TODO / FIXME 2 (both in JsMath.Fdlibm.cs); "not ported"-type phrases 36 in 26 files; "port-only" 79 in
  32 files.

## 5. Proposed reorganisation (ranked by value / risk; Core untouched unless marked)

Gates referred to below:
- G1 = compile, plus `Tools/unity-loop.sh tests` at 501 / 501, 22 / 22 replays, 91 / 91 golden.
- G2 = the Editor slice smoke and `SKYGRIND.exe -skygrindSmoke` (all steps passed).
- G3 = `-skygrindGolden` (0 mismatches) when the backend or Core changes.
- G4 = the composed fingerprint from move 1, which must be equal.

1. **A composed fingerprint (value high, risk ~0).**
   - Files: `Game/SurvivalGame.Fingerprint.cs` (a read-only `StateHash()` over Core state: mover, health, economy, run,
     director, husks, brain tokens, style, loadout, RNG draws counted). A `-skygrindFingerprint` check in
     `PlayerChecks.cs` and a smoke step. About 150 lines.
   - It runs a seeded run on scripted inputs (`forcedMove` / `forcedCombat`) with `brain.onScreen` pinned (the camera
     dependency above), and prints a hash every 600 steps.
   - Record it on today's code. Every later move must print the same hashes.
   - Proof: G1 and G2. Core untouched, so golden and replays cannot move.
2. **Split SurvivalGame.cs into partial files (value medium, risk ~0).**
   - Files: `SurvivalGame.cs` (fields, Awake / Start, Update, SimStep, StepMode: about 520), `.Init.cs` (Init,
     MakeMaterials, BuildBox / RunShape: about 420), `.Flow.cs` (about 270), `.Meta.cs` (about 100), `.Render.cs` (the
     render half as `RenderFrame`, UpdateGuide: about 170), `.Debug.cs` (HotKeys, DebugLine, overlay: about 70).
   - Move RunShapeKey / AppendRunShape into Ui.
   - Same type and same statement order. Handler registration order is unchanged because Init stays one method.
   - Proof: G1, G2, G4.
3. **De-duplicate and remove dead code (value medium, risk low).**
   - `WeaponLook.FamilyColor(cfg, weapon, fallback)` for the 6 sites; one `Bindings.Label / Key`.
   - Drop `click` and `IdleSpawner`. Drop `BeforeAi` / `AfterPlayerSystems` only if GG doesn't want them as hooks.
   - About -150 lines. Proof: G1, G2, G4, plus captures for colours.
4. **A typed settings view (value medium, risk low).**
   - A `SettingsView` struct rebuilt on `GameSettings.version`. Call sites read fields, not string keys: the per-frame,
     per-hurt and per-step reads in section 4, and `LightAndLens.cs:82-87`, `EnemyViews.cs:326-331`. About 120 lines.
   - Proof: SettingsTests, G2, G4.
5. **An input layer (value high for gamepad, risk low for the sim).**
   - `IPlayerInput { Poll(); InputFrame SampleMove(); CombatInput SampleCombat(); }` with three implementations:
     - `KeyboardMouseInput`: today's two readers, reading `config.input.bindings`.
     - `ScriptedInput`: replaces `forcedMove` / `forcedCombat`.
     - a `GamepadInput` later; look acceleration and deadzones are GG's design numbers.
   - About 250 lines. Replays feed InputFrame directly, so G1 is unaffected by construction.
   - Proof: G4 run through ScriptedInput equals the forced path. Optional: an `InputTestFixture` test (it ships with
     the installed Input System; the manifest's testables are not checked).
6. **Extract the sim composition (value high, risk medium).**
   - A plain C# `SurvivalSim` (Game, or Core if it needs no Unity) owns the Core systems, the Func wiring and SimStep
     in simulate() order. SurvivalGame keeps boot, the accumulator / hitstop / interpolation and the views.
   - **Keep every sim-affecting EventBus registration in today's sequence** (kill → particles, trauma, hitstop at
     `SurvivalGame.cs:490-547`; views register later). Make `OnScreen` an injected interface.
   - About 600 lines moved. It enables a headless Game.Tests fingerprint.
   - Proof: G4 identical before and after; then the headless test pins it.
7. **A run snapshot for save-and-quit (value high for NIGHT13, risk medium; touches Core).**
   - `IRunState { void Save(StateWriter w); void Load(StateReader r); }` on each stateful Core system. RNG state
     accessors for 35 `new Mulberry32(` sites in 21 Core files (29 sites in 17 files outside Golden). HANDOFF section 4 item 3 lists the
     meta parts the snapshot must include.
   - Additive: called only on save / load.
   - Proof: a Core test runs to step N, snapshots, loads into a fresh composition (move 6) and runs to step M, then
     checks the hash equals the uninterrupted run. Use several seeds: break, fight, exfil. G1 and G3 at each Core
     commit (CLAUDE.md rule 3).
8. **Split Ui by screen (value medium, risk medium: visual).**
   - `HudView`, `TitleScreen`, `ResultsScreen`, `SettingsScreen`, `MetaScreens` over a static `UiKit` (Text / Img /
     Button / Layer / Twin).
   - Proof: G2 (SliceSmoke reads `UiView`) plus captures. No perf or determinism gain, so last.

## 6. Not checked

- What the spike frames contain. No per-frame data exists. `profile.raw` (40 MB, 300 n11 frames) was not opened; it
  needs the Editor's ProfilerDriver.
- Heap size and per-collection duration. Unity 6000.3's exact GC and `Gfx.*` / `Canvas.*` marker names.
- The ~2.7 KB/frame of Release allocations: which code makes them.
- Whether the display is wired to the iGPU. The HX 370's core layout on this machine. The poller's CPU cost.
- URP's real use of the depth texture. Whether the overlay camera renders a shadow pass. `maxQueuedFrames`. Managed
  stripping level.
- IL2CPP determinism. Whether a built scene uses `Skygrind.Bench`. The DAMAGE DIRECTION default. How often
  AimAssist calls the `snapStrength` closure.
- Why NIGHT12-LOG.md quotes 6.20 ms for a run whose file shows 4.25 ms.
