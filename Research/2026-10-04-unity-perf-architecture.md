# Worst-1% frames, frame pacing, IL2CPP / Burst, quality tiers and code architecture for SKYGRIND

Research for the lead, 2026-10-04. Web search and fetch only: no repo file was read except
`Research/2026-10-03-unity.md` (what URP features we use; not repeated here). Nothing was built; git was not run.

Tags:
- **[W]** read in a fetched page (section 8).
- **[S]** from a search snippet only.
- **[R]** from the 2026-10-03 research or this session's git log (bench numbers); not re-checked.
- **[U]** my inference, not verified. Nothing here was measured.

**Bench facts used throughout [R]:**
- Release Player, 24 husks, 60 s, Balanced: median 2.43 ms, p95 4.19 ms, worst 1% 6.90 ms, mean 368 fps.
- Medians of three: 6.84 ms Balanced vs 7.19 ms Silent, so the plan moves the tail by ~0.35 ms.
- GPU worst 1% ~1.5 ms (brief).
- Mono, incremental GC on, flip model on, Frame Timing Stats on, vsync off by default.

---

## 0. Bottom line (ranked for SKYGRIND)

1. **The 7 ms tail is a recurring cost, not a rare hitch.**
   - 60 s at ~368 fps ≈ 22,000 frames, so the worst 1% ≈ 220 frames, ~3.7 a second [U]. GC collections, Mono JIT or
     a first-use shader can't set a p99 unless they happen that often.
   - Suspects: sim-step frames (at 120 Hz about 1 frame in 3 runs a step), 2-step catch-up frames, periodic
     flow-field rebuilds, HUD rebuilds.
   - First confirm whether "worst 1%" is a p99 boundary or the mean of the slowest 1%: they react very differently
     to rare spikes (section 2).
2. **Instrument before optimising.**
   - One build logs per frame: interval, steps run, step time per system, FrameTimingManager CPU / render / GPU, GC
     count.
   - Add one Development-build Profiler capture compared in Profile Analyzer (top 1% vs median). Order in section 7.
3. **Uncapped + incremental GC can add up to 3 ms to a frame.**
   - With no vsync and no `targetFrameRate`, a GC step is bounded by `incrementalTimeSliceNanoseconds` (default
     3 ms). With either set, Unity uses the frame's idle time instead [W].
   - Keep 0 B/frame. Once there, A/B incremental GC **off**: the PC e-book says its barriers "can add up to ~1 ms per
     frame" [W].
4. **Pick the pacing policy, then fix the metric.**
   - Uncapped: the bar is the p99 interval.
   - Capped (vsync / 144 / 240): the bar is the p99 CPU busy time (`cpuMainThreadFrameTime` or PresentMon
     `MsCPUBusy`), because a capped interval always reads ~6.94 ms.
   - Targets: laptop CPU p99 ≲ 5.5–6 ms [U]; Deck 60 fps at 800p, never under 30 [R].
5. **Cheap view-side wins, if the profiler shows them:** HUD SetText only on change (dirty flag), canvases split by
   update rate, Volume Update Mode = Via Scripting [W]. Rule 3 untouched.
6. **IL2CPP is the likeliest free CPU gain for a managed sim, but unproven here and a rule-3 risk.**
   - Unity calls it faster [W]; public numbers are old and mixed (~2× faster on some workloads, slower on others) [W].
   - One trial build, gated on `-skygrindGolden`, all replays, and a per-step state-CRC soak against Mono. Builds
     take ~2× as long (Android data point [W]).
7. **Burst: one hot numeric kernel at most (flow field), Strict mode** (the default).
   - Never `FloatMode.Deterministic`: it flushes subnormals and swaps in Burst's own math [W], so it can't match V8.
   - Gate with a Burst on/off bit-compare. Jobs likely not worth it at 24–30 enemies [U].
8. **Windows moves the tail.** Plan ~0.35 ms [R]; Windows 11 may infer EcoQoS and drop timer resolution for a hidden
   or occluded window [W]. Bench in the foreground, keep logging the plan, A/B a HighQoS opt-in [U].
9. **GPU tiers won't fix the laptop** (GPU p99 1.5 ms). CPU knobs matter more: particle cap, light pool, decals, HUD
   effects. AUTO = GPU table + 3–5 s timed scene, split CPU-bound vs GPU-bound with FrameTimingManager (section 5).
10. **PSO / shader warm-up hits the max and the 0.1%, not the p99.** Unity 6 path: `GraphicsStateCollection` on DX12 /
    Vulkan, experimental, legacy fallback on DX11 [W]. Our API: not checked.
11. **Architecture:** keep the explicit composition root and one readable `simulate()` order list; split SurvivalGame
    by partial classes, then system classes; a struct event queue drained after the step; config stays JSON; skip SO
    event channels and DI containers (section 6).
12. **Saves:** versioned JSON envelope, migration chain, atomic write [S]. A mid-run save = seed + input log
    (replays), not a heap snapshot. Factorio-style save→load→compare test [W].

---

## 1. Diagnosing worst-1% CPU spikes in a Windows Player

### Tools
- **Development build + Profiler:** most accurate on target hardware, not the Editor [W]. Deep Profiling is "resource
  intensive" and distorts relative costs; prefer Call Stacks [W].
- **Profile Analyzer:** aggregates frames and compares captures, e.g. median vs top 1% [W].
- **FrameTimingManager** (release Player with Frame Timing Stats) [W]:
  - `cpuMainThreadFrameTime` = PlayerLoop minus render-thread, present and target-FPS waits.
  - `cpuMainThreadPresentWaitTime` = `Gfx.WaitForPresentOnGfxThread` + `WaitForTargetFPS`.
  - Also `cpuRenderThreadFrameTime`, `gpuFrameTime`.
- **ProfilerRecorder** works in release Players [W]: draw calls, batches, SetPass, Total Used Memory there; "GC
  Allocated In Frame" is development-only [W]. Custom markers in release: not documented; time sim systems with
  `Stopwatch.GetTimestamp` [U].
- **Allocations:** `GC.Alloc` with Call Stacks in a dev build; the Memory Profiler package [W]. Measure in a build
  (`GetComponent` allocates only in the Editor) [W].
- **Native profilers:** Unity lists PIX (DX12), Superluminal (paid), VTune (Intel), AMD uProf, Nsight, RenderDoc [W].
  Superluminal shows mixed managed / native stacks for Mono Players [S]. ETW / WPA for frequency and scheduling: not
  checked.

### Usual causes, ranked for this project (CPU-bound, ~3.7 slow frames a second)
1. **Sim-step frames and catch-up.**
   - The 120 Hz accumulator runs 0, 1 or 2+ steps per frame; a slow frame makes the next one run 2.
   - Gaffer clamps frame time at 0.25 s against the "spiral of death" [W]; the e-book describes the same spiral for
     physics [W].
   - Split the bench histogram by steps per frame [U]. Capping catch-up changes only wall-clock sync, not results [U].
2. **GC.** Stop-the-world unless incremental [W]. The e-book: no allocations in the gameplay loop (strings, boxing,
   LINQ, closures, per-frame Lists); incremental GC only "temporarily" helps [W]. PORT-RULES' List.Find rule fits.
3. **uGUI rebuilds** [W, PC e-book]:
   - One big Canvas rebuilds whole when one element changes: split by update rate.
   - Disable the Canvas component, not the GameObject; drop GraphicRaycasters from non-interactive canvases; avoid
     Layout Groups.
   - Markers: `Canvas.BuildBatch`, `Canvas.SendWillRenderCanvases` [S].
   - TMP: `SetText(StringBuilder)` and the format overloads write without a string; reading `.text` afterwards
     allocates [S].
4. **Volume / post changed per frame.** URP: Volume Update Mode = Via Scripting [W]. A forum report cut 2–3 ms
   (Switch) to 0.4 ms by excluding unused VolumeComponents and avoiding casts in `Override` / `ReplaceData` [W]
   (2022-era URP; 17.3 not checked). Matters once post.js's pulses land [R].
5. **Lights.** Forward+ bins lights on the CPU every frame with Burst jobs (z-binning, tiling) [W]; cost follows
   visible-light count, not moves [U]. Whether an intensity-0 pooled light enters the cluster pass: not checked; A/B it
   against `enabled = false`.
6. **Batching.** An MPB makes a renderer SRP-Batcher-incompatible; use instancing or `RenderMeshInstanced` [W]. MPBs
   are in 10+ view files [R].
7. **Shader / PSO first use.** Stutter on a variant's first use [W]. `GraphicsStateCollection` tracing and
   `WarmUpProgressively` [W]; collections are per API and per quality level, and tracing must render everything [W].
   Mostly hits the max frame [U].
8. **Mono JIT.** `Mono.JIT` is a method's first-call compile [W]; it's rare and hits the max. IL2CPP is AOT [W].
9. **Windows.**
   - Timer resolution is per process since 10 2004; on Windows 11 it isn't guaranteed for occluded or minimized
     windows [W].
   - Without an opt-in, Windows infers QoS by heuristics. EcoQoS lowers frequency and prefers efficient cores. Opt out
     via `SetProcessInformation(ProcessPowerThrottling)`: ControlMask EXECUTION_SPEED, StateMask 0 (HighQoS) [W].
   - Another DirectX-presenting program caused `WaitForTargetFPS` spikes in one report [S].
10. **Audio / IO.** No audio (rule 7). Strip per-frame `Debug.Log` from builds [W]; check the bench logs only at its
    end [U].

**For SKYGRIND:** add the per-frame recorder (section 7 step 1) to `-skygrindBench`: one build answers causes 1, 2 and
9. A single dev-build capture covers 3–6. Causes 7 and 8 go in a "max frame" column.

---

## 2. Frame pacing

- **Unity** [W]:
  - `vSyncCount > 0` makes Unity ignore `targetFrameRate`. `vSyncCount` defaults to 1; we set it off [R].
  - `targetFrameRate` is software timing, "subject to microstuttering"; use vsync "when smooth frame pacing is needed".
  - Don't assume the startup refresh rate persists.
- **`maxQueuedFrames`:** default 2 on PC; D3D11 / D3D12 / Vulkan only. Lower = less input lag, usually lower fps [W].
- **2020.2 deltaTime fix:** samples time after the previous present; needs vsync, D3D11 / 12 and no dropped frames
  [W], so we don't get it with vsync off. Accumulator dt jitter moves only the interpolation alpha, never the sim [U].
- **Measuring 1% lows** [W]:
  - CapFrameX: P1 is the percentile boundary. "1% low" was the average of the slowest 1% of frames; it's now
    time-based (slowest frame times summed to 1% of run time).
  - PresentMon: `MsBetweenPresents` (app cadence), `MsCPUBusy`, `MsCPUWait`, `MsGPUBusy`, `MsBetweenDisplayChange`
    (what the screen showed), `MsPCLatency`. Flags: `--process_name`, `--output_file`, `--timed`.
  - Unity's e-book: reason in ms, not fps [W].
- **Targets** [U]:
  - **144 Hz laptop:** offer "uncapped" for latency fans but default to a cap; 240 hands idle time to incremental GC
    [W]. Hold CPU p99 ≤ ~6 ms.
  - **Deck:** vsync to the panel, 60 fps at 800p, 30 floor [R]. Deck refresh rates: not checked this session.

**For SKYGRIND:** report p50, p95, p99, p99.9, max, count over 6.94 ms, and ms over budget, for both interval and CPU
busy; median of three [R]. Add the cap and `maxQueuedFrames` (1 / 2) as settings and bench each.

---

## 3. Mono vs IL2CPP for a managed-heavy simulation

**Speed:**
- IL2CPP "can offer … improved performance and shorter startup times"; builds take "significantly longer" [W].
- PC e-book: Mono to iterate, IL2CPP for release candidates [W]. Mono's own page: "lower runtime performance" [W].
- **Measurements:** Dunstan (iOS, 2015) had NBody ~48% and SpectralNorm ~52% of Mono's time, some tests far slower
  [W]. Android on 6000.4: builds 2.1× longer, no runtime numbers [W]. Nothing current covers Windows double-heavy code:
  **measure it ourselves**.

**Build:**
- Windows IL2CPP uses MSVC and the VS C++ workload [S]; on GG's laptop: not checked.
- Ship `GameAssembly.dll` and SymbolMap, never `*_BackUpThisFolder_ButDontShipItWithYourGame` [W].
- Master = MSVC link-time code generation, slower builds [S]. "Optimize for code size and build time" builds faster,
  runs slower [W].

**Determinism risk:**
- Unity staff: no IL2CPP float guarantees across architectures; same compiler → expect none, but test [W].
- **Contraction:** VS2022 `/fp:precise` no longer contracts to FMA by default; `/fp:fast` / `/fp:contract` do [W].
  IL2CPP's `/fp` and `/arch` flags: not verified, and that is the crux.
- **Floats:** in 2015 staff said Mono widened float math to double and IL2CPP didn't [W]. Our sim is doubles [R]; any
  `float` in Core: not checked.
- **CRT math:** `System.Math` Atan2 / Pow / Exp / Log go to native libraries that may differ by backend [U]. JsMath
  covers sin, cos, hypot [R]; check Core calls nothing else.
- **x64:** both backends use SSE2 scalar doubles with correctly rounded + − × ÷ sqrt [U]. Risk sits in contraction,
  libraries and floats.

**Gate (for SKYGRIND):**
1. IL2CPP Player in a separate folder (one build).
2. `-skygrindGolden`: 0 mismatches on all sets and replays [R].
3. CRC soak: a seeded 10-minute bot run, state CRC every N steps in both Players, all equal (Factorio's CRC tests
   [W]).
4. Bench, median of three. Switch only on 0 mismatches and a gain above noise (~0.35 ms [R]). The Editor stays Mono
   [S].

---

## 4. Burst / Jobs inside a bit-exact managed sim

**Modes** [W]:
- `Strict` (default): "No floating point optimizations are performed".
- `Fast`: allows reassociation, reciprocals, "fused instructions".
- `Deterministic` (Burst 1.8.25, 2025-09-16; latest 1.8.30, 2026-07-06): Burst's own math functions, subnormals
  flushed to zero, no identical-NaN promise, Burst code only [W]. It buys Burst-to-Burst consistency, not equality
  with Mono or V8. **Don't use it.** The ECS Galaxy sample uses `FloatPrecision.High` + `Deterministic` [W]: right for
  a Burst-only sim, not ours.

**Constraints** [W]: HPC# has no managed objects or strings, no `catch`, no static stores outside SharedStatic. A
directly called `[BurstCompile]` static needs blittable parameters and no generics. Only an array kernel fits: pinned
`double*` / `int*` through a static entry point, Core keeping the managed copy [U].

**Realistic** [U]:
- Flow field: grid BFS / Dijkstra on one thread, no Jobs. The wavefront is serial, and scheduling overhead at this
  size likely eats the gain.
- Enemy steering (24–30 branchy class objects): keep managed.
- **Gate:** the same static with Burst on and off over golden vectors and replays, equal bit for bit. Strict's FMA
  behaviour isn't documented (only Fast mentions fused), so the test is the proof. EditMode tests may hit the managed
  fallback while Burst compiles asynchronously unless compilation is forced synchronous (not verified).
- Burst 1.8.27 and Collections are already installed [R].

**For SKYGRIND:** Burst only if per-system timing (section 7) shows the flow field above ~1 ms in slow frames;
otherwise the IL2CPP trial is the cheaper experiment.

---

## 5. Quality tiers and auto-detection

**How games pick a preset:**
- **Unreal:** `RunHardwareBenchmark` gives CPU and GPU indices (100 = "average good"); per-group thresholds tuned late
  [W].
- **GPU tables:** Intel GPU Detect maps vendor / device IDs to Low / Medium / High; unknown parts get the lowest [W].
- **The web game** already has presets, AUTO and dynamic resolution (`config.quality`) [R]. Port its logic.

**URP tier knobs** [W]: render scale + upscaling filter (FSR 1.0 / STP), MSAA; main-light shadow resolution, cascades,
max distance, soft-shadow quality; additional lights; opaque / depth textures; grading mode, LUT size; Volume Update
Mode. SSAO: normal quality 1 / 5 / 9 samples, plus blur quality [S].

**Dynamic resolution:** DX12-only on Windows per the 2022.2 docs [S]; Unity 6.1 made DX12 the default for new projects
[W]; our API: not checked. Otherwise `renderScale` [R] with hysteresis, every few seconds at most, since render
targets reallocate [U].

**For SKYGRIND:**
- **AUTO on first launch:** read `SystemInfo.graphicsDeviceName` against a short table (Deck APU, known iGPUs → Low).
  Run 3–5 s of the real arena after warm-up with FrameTimingManager. GPU p95 over budget → lower GPU knobs; CPU over
  → lower CPU knobs. Store it. Never touch the sim.
- **CPU knobs:** particle cap, light-pool size, decal count, HUD effects, Volume blending, the viewmodel camera (every
  camera costs culling and rendering [W]).
- **GPU knobs:** render scale + FSR1, SSAO off / on / High normals, cascades 1 / 2 / 4, shadow distance, bloom filter,
  grain.
- Adaptive Performance has Android and simulator providers only [S]: inert on Windows [U].

---

## 6. Code architecture for a code-first game with a pure C# core

**Layering:**
- Core (`noEngineReferences`), Game, Bench, Editor, Tests [R] is the right shape.
- `noEngineReferences` drops the UnityEngine / UnityEditor references; `autoReferenced: false` hides an assembly from
  the predefined ones [S].
- Optional: `Skygrind.Game.UI` / `.Views` split out, for smaller recompiles and so UI can't reach sim internals [U].

**Composition root vs singleton vs service locator:** VContainer's `LifetimeScope` is a composition root with
zero-allocation resolves, but its entry points run on its own PlayerLoopSystem [W], an ordering axis beside
`simulate()`. The patterns e-book flags singleton pitfalls [W].
- **For SKYGRIND:** pure DI. `SurvivalGame.Awake` news every system with constructor arguments in one grep-able file;
  statics only for immutable config (rule 6).

**Splitting the 1,500-line loop without changing order** [U]:
1. Move code into `partial class SurvivalGame` files by concern; gate on golden and replays.
2. Extract plain classes with `Step(ref StepCtx)`; `Simulate()` stays one visible list of calls in web order.
3. An EditMode test: systems append ids to a trace, compared with the web's `simulate()` list.

**Event bus:** `Delegate.Combine` returns a new delegate, a then b [W]: subscribing allocates and invocation order is
implicit subscription order. Capturing lambdas allocate closures [U].
- **For SKYGRIND:** the sim writes web-named events (rule 7) into a preallocated struct ring per step; views drain it
  after the step loop. No UnityEvent [U].

**Config:**
- The PC e-book prefers ScriptableObjects or binary formats over runtime JSON [W]: a load-time cost, paid once [U].
- SOs are writable in Editor Play mode and read-only in a Player [W]. Runtime leaks in the Editor and inspector wiring
  that grep can't see are a poor fit for agents [U]. The SO e-book itself says event channels need custom debug tools
  [W].
- **Keep `PortExport/config.json` → `GameConfig` (rule 5).**

**From the e-books:**
- **Applies:** pooling, flyweight (shared enemy defs), command (per-step input structs for replays), state (AI, run
  phases), strategy (weapons), MVP + dirty flag for the HUD [W lists the patterns].
- **Doesn't:** SO event channels, runtime sets, SO delegate objects as sim logic.

**Saves:**
- Envelope `{schema, saveVersion, build}`; pure, unit-tested v1→v2→… migrations; write `.tmp` then rename [S].
- Mid-run save = seed + config hash + input log; restore by re-simulating [U]. Measure replay cost first.
- **Test:** save → load → save, byte-compare; run both copies on and compare a per-step CRC (Factorio: save, exit,
  load must not change outcomes [W]).

**Testing pyramid:**
- In place: EditMode units, golden, the 16 trajectories, replays, SliceSmoke, the Player smoke [R].
- **Add:** the order test, IL2CPP golden parity, a bench gate (median of three, budget + tolerance).
- `-testPlatform StandaloneWindows64` runs PlayMode tests inside a Player [W].

**CI (GameCI):**
- Personal: the `.ulf` contents + email + password as secrets; Pro: `UNITY_SERIAL` [W].
- The test runner does EditMode / PlayMode / All; package tests are Linux-only [W]. A Linux EditMode golden run would
  also probe cross-OS determinism ahead of a Deck-native build [U].
- IL2CPP Windows builds need a Windows runner; caching `Library/` cuts build time by over 50% [W].
- Push and CI are GG's call (rule 1).

---

## 7. A measured-first perf plan

Steps 1–2 fit in one Player build (the one-build cap [R]).

| # | Measure | Decides |
|---|---|---|
| 0 | The bench's "worst 1%" definition; add p99 / p99.9, max, count over 6.94 ms, ms over budget | Rare spikes vs a recurring cost |
| 1 | Per-frame CSV in `-skygrindBench`: interval, steps (0/1/2+), Stopwatch µs per system, FTM CPU / render / GPU, `GC.CollectionCount(0)`, plan | Slow 1-step frames → 3. Slow 2-step → the frame before. GC rises → 2. Slow 0-step → 4 |
| 2 | Dev build: GC.Alloc call stacks; Profile Analyzer top 1% vs median | Allocations to remove; Canvas.*, VolumeManager, light culling, Mono.JIT |
| 3 | Per-system p99 from step 1 | Flow field dominant → amortise or Burst. Spread out → IL2CPP trial |
| 4 | View A/Bs: HUD dirty flags, Volume Via Scripting, lights off vs intensity 0, MPB → instancing | Keep what beats noise on CPU p99 |
| 5 | Pacing A/Bs: uncapped / 240 / 144; incremental GC on vs off (at 0 B/frame); `maxQueuedFrames` 1 vs 2 | Default cap, GC mode; the bar becomes CPU-busy p99 if capped |
| 6 | Windows A/B: HighQoS opt-in vs default, foreground, plan logged | Ship the QoS call?; plan tolerance in the bar |
| 7 | IL2CPP: golden + replays + CRC soak, then bench | Switch only at 0 mismatches and a gain above noise |
| 8 | AUTO: a 3–5 s timed scene on the laptop, later the Deck | Tier thresholds, CPU and GPU knobs separately |

Every run is a median of three with plan and AC polls [R]. Core changes need golden and replays at 0 mismatches
(rule 3).

---

## 8. Sources ([W] fetched and read; [S] search snippet only)

**Profiling, GC, timing**
- [W] https://docs.unity3d.com/6000.1/Documentation/Manual/performance-incremental-garbage-collection.html · https://docs.unity3d.com/ScriptReference/Scripting.GarbageCollector-incrementalTimeSliceNanoseconds.html
- [W] https://docs.unity3d.com/6000.1/Documentation/Manual/frame-timing-manager-counter-reference.html · https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- [W] https://docs.unity3d.com/6000.2/Documentation/Manual/profiler-counters-reference.html · https://docs.unity3d.com/6000.3/Documentation/Manual/profiler-markers.html
- [W] https://docs.unity3d.com/6000.3/Documentation/Manual/performance-garbage-collector.html · https://docs.unity3d.com/6000.3/Documentation/Manual/performance-track-garbage-collection.html
- [W] https://unity.com/blog/use-unity-6-profiling-tools-smart-efficient · https://unity.com/how-to/best-practices-for-profiling-game-performance · https://unity.com/how-to/profiling-and-debugging-tools
- [W] https://unity.com/resources/console-pc-game-performance-optimization-unity-6 and its PDF (text extracted; pp. 16–24, 40, 50, 97–99, 110–111): https://cdn.bfldr.com/S5BC9Y64/at/xbhk7z8kvttn35t3nx45mm98/Optimize_your_game_performance_for_consoles_and_PCs_in_Unity_Unity_6_edition_e-book.pdf
- [W] https://unity.com/how-to/performance-optimization-high-end-graphics (GPU-only; little used)
- [S] https://unity.com/resources/ultimate-guide-to-profiling-unity-games-unity-6 (the PDF exceeded the fetch limit)
- [S] https://learn.unity.com/tutorial/profile-cpu-performance-with-superluminal · https://www.superluminal.eu/docs/documentation.html
- [S] https://discussions.unity.com/t/waitfortargetfps-spikes-in-addition-to-dxgi-waitonswapchain-causing-frame-skips-with-vsync/941005 · https://forum.unity.com/threads/profiler-overhead-for-first-time-invocation-of-functions.677158/

**UI, Volumes, lights, batching, PSOs**
- [S] https://discussions.unity.com/t/huge-garbage-generated-1-6mo-per-frame-when-changing-a-lot-of-texts/769813 · https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_Text.html · https://unity.com/how-to/unity-ui-optimization-tips
- [W] https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html · https://discussions.unity.com/t/srp-volume-api-performance/895695
- [S] https://docs.unity3d.com/6000.4/Documentation/Manual/urp/volumes-troubleshooting.html
- [W] https://github.com/Unity-Technologies/Graphics/pull/4500 · https://docs.unity3d.com/6000.3/Documentation/Manual/urp/lighting/light-limits-in-urp.html (Forward limits only)
- [W] https://docs.unity3d.com/6000.3/Documentation/Manual/SRPBatcher-Incompatible.html
- [W] https://docs.unity3d.com/6000.3/Documentation/Manual/shader-pso-trace-warming.html · https://docs.unity3d.com/6000.3/Documentation/Manual/shader-pso-introduction.html
- [W] https://discussions.unity.com/t/graphicsstatecollection-tracing-and-warmup-in-unity-6/951031 · https://unity.com/blog/directx-12-improvements-in-unity-6

**Windows, frame pacing, metrics**
- [W] https://learn.microsoft.com/en-us/windows/win32/api/timeapi/nf-timeapi-timebeginperiod · https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setprocessinformation · https://devblogs.microsoft.com/performance-diagnostics/introducing-ecoqos/
- [W] https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-targetFrameRate.html · https://docs.unity3d.com/6000.3/Documentation/ScriptReference/QualitySettings-vSyncCount.html · https://docs.unity3d.com/6000.3/Documentation/ScriptReference/QualitySettings-maxQueuedFrames.html
- [W] https://unity.com/blog/engine-platform/fixing-time-deltatime-in-unity-2020-2-for-smoother-gameplay · https://gafferongames.com/post/fix_your_timestep/
- [W] https://www.capframex.com/news/detail/New%20version%201.5.3 · https://raw.githubusercontent.com/GameTechDev/PresentMon/main/README-ConsoleApplication.md

**IL2CPP, determinism, Burst**
- [W] https://docs.unity3d.com/Manual/il2cpp-introduction.html · https://docs.unity3d.com/Manual/WindowsPlayerIL2CPPScriptingBackend.html · https://docs.unity3d.com/6000.0/Documentation/Manual/scripting-backends-mono.html
- [S] https://docs.unity3d.com/ScriptReference/Il2CppCompilerConfiguration.html · https://www.guardingpearsoftware.com/blog/il2cpp-debug-release-and-master-builds-which-one-should-54666
- [W] https://www.jacksondunstan.com/articles/3001 · https://dev.to/indiecoredev/unity-il2cpp-vs-mono-on-android-measured-49ke
- [W] https://discussions.unity.com/t/are-floating-point-numbers-deterministic-on-the-same-architecture-il2cpp/892251 · https://discussions.unity.com/threads/il2cpp-floating-point-precision-issue.347108/
- [W] https://devblogs.microsoft.com/cppblog/the-fpcontract-flag-and-changes-to-fp-modes-in-vs2022/
- [S] https://learn.microsoft.com/en-us/cpp/build/reference/fp-specify-floating-point-behavior?view=msvc-170 · https://zenn.dev/mod_poppo/articles/c-fp-contract?locale=en
- [W] https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/compilation-burstcompile.html · https://docs.unity3d.com/Packages/com.unity.burst@1.8/api/Unity.Burst.FloatMode.html · https://docs.unity3d.com/Packages/com.unity.burst@1.8/changelog/CHANGELOG.html
- [W] https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/csharp-hpc-overview.html · https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/csharp-calling-burst-code.html
- [W] https://discussions.unity.com/t/what-is-the-state-of-bursts-floatmode-deterministic/1668491 · https://github.com/Unity-Technologies/ECSGalaxySample/blob/main/_Documentation/determinism.md

**Quality tiers**
- [W] https://docs.unity3d.com/6000.3/Documentation/Manual/urp/universalrp-asset.html · https://tomlooman.com/unreal-engine-optimal-graphics-settings/ · https://github.com/havealex/gpudetect
- [S] https://docs.unity3d.com/6000.0/Documentation/Manual/urp/ssao-renderer-feature-reference.html · https://docs.unity3d.com/2022.2/Documentation/Manual/DynamicResolution.html
- [S] https://dev.epicgames.com/documentation/en-us/unreal-engine/BlueprintAPI/Settings/RunHardwareBenchmark · https://docs.unity.cn/Packages/com.unity.adaptiveperformance@5.0/manual/provider.html

**Architecture, tests, CI, saves**
- [W] https://unity.com/blog/game-programming-patterns-update-ebook · https://unity.com/how-to/scriptableobjects-event-channels-game-code · https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html
- [S] https://unity.com/resources/design-patterns-solid-ebook · https://discussions.unity.com/t/unity-6-edition-ebook-and-sample-project-on-scriptable-objects/1680222
- [S] https://docs.unity3d.com/6000.1/Documentation/Manual/assembly-definition-file-format.html
- [W] https://github.com/hadashiA/VContainer · https://learn.microsoft.com/en-us/dotnet/api/system.delegate.combine
- [W] https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html
- [W] https://game.ci/docs/github/activation/ · https://game.ci/docs/github/test-runner/ · https://game.ci/docs/github/builder/
- [S] https://github.com/game-ci/unity-license-activate
- [W] https://factorio.com/blog/post/fff-60 · https://factorio.com/blog/post/fff-270
- [S] https://www.factorio.com/blog/post/fff-47 · https://arcadeonstudios.co.uk/blog/a-practical-save-system-for-indie-games-versioned-portable-testable

**Not checked:** the Player's graphics API; IL2CPP's MSVC flags; whether Core uses `float` or CRT math beyond JsMath;
the bench's "worst 1%" definition; whether an intensity-0 URP light costs CPU; shipped-game postmortems on Unity frame
tails (the search budget ran out).
