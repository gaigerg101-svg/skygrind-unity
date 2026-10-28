# R4: Menu / mode / weapon switch lag in Unity 6 (uGUI + TMP), 2026-10-08

Web-only research. Every fact has a URL; "not found" = I did not find a source. Items marked (inference) are my reasoning, not a sourced claim.
Fetched with a small summarizing model, so check quotes against the page before you rely on them.

## 0. Short answer
- Gameplay at 100-240 FPS with "laggy" switches usually means rare long frames, not low average FPS. Look at the worst frame, not the average (inference).
- Likely causes, most to least probable (inference, from the sources below):
  1. Canvas re-enable via SetActive: full mesh rebuild.
  2. First-use shader / PSO compile.
  3. TMP dynamic atlas glyph population.
  4. Synchronous loads / Instantiate on first use.
  5. GC.
  6. Editor-only overhead.

## 1. uGUI + TMP hitches on first show / Canvas enable

### 1.1 SetActive vs Canvas.enabled
- `GameObject.SetActive(false)` on a canvas discards its VBO data and forces a full Canvas rebuild when re-enabled. https://learn.unity.com/tutorial/optimizing-unity-ui
- `Canvas.enabled = false` stops drawing but keeps meshes resident and keeps the batching. MonoBehaviours on it still get lifecycle callbacks. https://learn.unity.com/tutorial/optimizing-unity-ui
- Test: replace SetActive on large screens with `Canvas.enabled`, or a CanvasGroup (alpha 0, interactable false, blocksRaycasts false). Record the "show screen" frame ms before and after.
  - CanvasGroup behaviour: not found in a fetched source, so treat it as unverified.
- Caveat: with `Canvas.enabled=false`, scripts on the canvas still run Update. Disable heavy behaviours by hand (inference from the source above).

### 1.2 Canvas splitting
- The Canvas system does not combine batches across canvases. Split any non-trivial canvas into at least a static part (backgrounds, labels) and a dynamic part (frequently changing elements). https://learn.unity.com/tutorial/optimizing-unity-ui
- Test: put the per-frame HUD (timers, ammo) on its own canvas, separate from the menu and the static frame. One changing label should then not dirty the whole menu mesh.

### 1.3 Layout groups
- Layout components recalculate child sizes/positions whenever they are marked dirty. Replacing simple layouts with anchored RectTransforms moves the work to native code and is generally faster. https://learn.unity.com/tutorial/optimizing-unity-ui
- Test: count the Horizontal/Vertical/Grid LayoutGroups and ContentSizeFitters under each screen. Inside the Profiler, look for `Canvas.BuildBatch` and layout rebuild samples.
  - The exact marker names are not on the Unity markers page I fetched ("not found" there). Search the Profiler by the substring "Canvas" and "Layout".
- Four hotspots Unity names to profile:
  - GPU fill-rate
  - CPU time rebuilding a canvas batch
  - over-dirtying (frequent rebuilds)
  - vertex generation, especially from text
  - https://learn.unity.com/tutorial/optimizing-unity-ui

### 1.4 Pre-warm a canvas
- `Canvas.ForceUpdateCanvases()` forces all canvases to update. A canvas normally lays out and builds content at the end of a frame, just before rendering, so in Start and the first Update the layout may be stale. https://docs.unity3d.com/ScriptReference/Canvas.ForceUpdateCanvases.html
- Pattern (inference): at boot or during a loading beat, enable each screen's canvas for one frame offscreen or at alpha 0, call `Canvas.ForceUpdateCanvases()`, then switch it off with `Canvas.enabled=false`. The first real show then reuses the built mesh.
- Whether a canvas disabled via `Canvas.enabled=false` keeps its mesh across a Unity 6 domain reload or scene change: not found.

### 1.5 TMP dynamic font atlas
- Dynamic font assets start with an empty atlas and add characters as they are used. They need more compute than static fonts. https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsDynamicFonts.html
- Dynamic atlas population adds glyphs at runtime up to the atlas size. When it is full, oldest (LRU) glyphs are replaced. Multi Atlas Textures can be enabled per font asset in Generation Settings to add atlas textures instead of evicting. (Search-result summary; the dynamic fonts page I fetched did not elaborate.) https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsDynamicFonts.html
- `TMP_FontAsset.TryAddCharacters()` adds characters from script, for pre-population. https://discussions.unity.com/t/tryaddcharacters-stuck-in-infinite-loop-with-tiny-multi-atlas-textures/858836
  - Known issue: it can loop forever with tiny multi-atlas textures when characters don't fit. Guard against no progress. Same link.
- A forum report says even one new character in a dynamic atlas took about 5 s. Single report, treat as an outlier. https://forum.unity.com/threads/dynamic-atlas-population-taking-a-long-time-even-for-a-single-character.1430263/
- Using dynamic population to generate everything at runtime gets expensive for large character sets. One suggested approach: pre-generate the atlas for known characters. Search-result summary. https://forum.unity.com/threads/saving-loading-font-atlas-for-textmeshpro-font-assets-during-runtime.1325754/
- Checklist:
  - For ASCII-only UI, use a Static atlas that covers it (inference).
  - If you keep Dynamic, call `TryAddCharacters` with every string the UI can show during boot (inference).
  - Check the Profiler on first show for TMP / font sample names. I did not find an official list of TMP marker names ("not found").
  - A first-use glyph shows as a one-time spike. If it repeats every show, something else (rebuild) is the cause (inference).
- Unity ships a skill/doc topic on TMP optimization (`unity:optimize-text-mesh-pro` skill is listed in this environment); no web URL found.

### 1.6 Shader / PSO first-use compile
- First use of a shader variant makes the driver compile it and create a pipeline state object (PSO). Prewarm so the driver caches to disk. https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html
- Unity 6 API: `GraphicsStateCollection.WarmUp` (all PSOs) and `WarmUpProgressively` (a set number per call, avoids blocking). Prewarm during app or scene loading. It is marked experimental. https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html
- Profiler markers to search for: `Shader.CreateGPUProgram` and `CreateGraphicsGraphicsPipelineImpl`. https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html
- `GraphicsStateCollection` with `traceCacheMisses=true` records permutations not in the collection. Use it to find out why prewarming didn't help. https://docs.unity3d.com/ScriptReference/Experimental.Rendering.GraphicsStateCollection.WarmUp.html
- `ShaderVariantCollection.WarmUp()` prewarms recorded variants. On DX12/Vulkan/Metal the driver may still do work if vertex layout or render-target setup differs, so a stall can remain. Unity points to `ShaderWarmup` as the all-API option. https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ShaderVariantCollection.WarmUp.html
- `Shader.WarmupAllShaders()` warms every variant of every shader in memory. It can mean long load and high memory. Prefer a collection. https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Shader.WarmupAllShaders.html
- Which graphics API your Windows build uses (DX11 vs DX12 vs Vulkan) matters for how well warmup holds. Force one with `-force-d3d11` to compare. https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html
- URP-specific prewarm setting in the pipeline asset: not found in what I fetched.
- Checklist:
  - Play every screen and weapon once in a Development Player with Profiler. Search `Shader.CreateGPUProgram`. Any hit during a switch is a first-use compile.
  - Record a GraphicsStateCollection from a full play-through, ship it, and WarmUp (progressively) behind the title screen.

### 1.7 Synchronous loads
- `Resources.Load` is synchronous. `Resources.LoadAsync` returns a ResourceRequest and does not block the main thread. https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Resources.LoadAsync.html
- Test: grep your code for `Resources.Load(` in anything triggered by a button click. Replace with preload at boot or LoadAsync (inference).

### 1.8 Vsync / target frame rate toggles
- On desktop, Unity ignores `Application.targetFrameRate` if `vSyncCount` is set. vSync is hardware sync; `targetFrameRate` is software timing and subject to microstutter. vSyncCount=0 plus targetFrameRate does not give stutter-free output. https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
- Test: do not change vSync, targetFrameRate or QualitySettings level at runtime on a mode switch. Quality-level switching cost: not found.
- For a 240 Hz claim: with vSyncCount=0 and no cap, frame times are small, so one 30 ms hitch is a visible 7x drop (inference).

### 1.9 GC
- Managed allocations trigger the collector when the heap is full. 1 KB per frame at 60 fps is 3.6 MB per minute and causes frequent collections and hitches. Aim for 0 B per frame. https://docs.unity3d.com/6000.0/Documentation/Manual/performance-garbage-collector.html
- Profiler marker: `GC.Alloc`; `GC.Collect` stops your code until done. https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-markers.html
- Test: on a mode switch, any `GC.Alloc` bigger than a few KB, or a `GC.Collect` sample, is a suspect. Typical sources (inference): string building for TMP, LINQ, new List per show, lambdas in button listeners.

### 1.10 Not found
- Time.timeScale changes causing hitches: not found. (inference: timeScale=0 only freezes scaled time; UI using unscaled time is unaffected.)
- URP renderer feature or Volume changes on mode switch cost: not found.
- Material instance creation at first draw: not found as a source. Note that touching `renderer.material` clones a material (inference); use `sharedMaterial` or MaterialPropertyBlock.

## 2. Weapon switch hitches
- Sourced items: shader/PSO prewarm (1.6), synchronous loads (1.7), GC (1.9).
- Instantiate on first equip, mesh upload, animator init cost: no authoritative URL found for exact costs ("not found").
- Prewarm pattern (inference, mark as untested):
  1. At run start, instantiate every weapon view model once, inactive, parented under the camera rig.
  2. Activate each for one frame offscreen (or behind the title) so mesh upload, material/shader PSO and animator init occur then.
  3. Switch by toggling Renderer.enabled (or SetActive of an already-warmed object), never Instantiate/Destroy during play.
  4. Pool muzzle flashes/decals/projectiles the same way.
- Test: Profiler on the switch frame. Search for `Instantiate`, `Shader.CreateGPUProgram`, `Animator.Initialize` (name unverified), `GC.Alloc`, and any `Resources.Load`.
- Textures: a texture is uploaded to the GPU on first use unless it is already resident. Exact setting for forced upload (e.g. `Texture2D.Apply`, "Preload"): not found in my fetches.

## 3. How to measure

### 3.1 Profiler
- CPU Usage module has a Timeline view (all threads along a time axis, the only view that shows all threads at once) and Hierarchy / Raw Hierarchy / Inverted Hierarchy views. https://docs.unity3d.com/6000.0/Documentation/Manual/ProfilerCPU.html
- Use Timeline on the spike frame; click the longest sample; check Hierarchy sorted by time. Inverted Hierarchy shows markers with high cumulative time. Same link.
- Markers that mean "waiting", not working: `Gfx.WaitForPresentOnGfxThread` (render thread still waiting on GPU present) and `Semaphore.WaitForSignal` (a sync point; find the thread it waits for by what ended just before). https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-markers.html
- Unity advises profiling a Development Player, not the Editor, because the Editor's own GUI skews data. Search-result summary of https://learn.unity.com/tutorial/diagnosing-performance-problems-2019-3
- Profile Analyzer and Deep Profile: not found in the page I fetched.

### 3.2 ProfilerRecorder (works in the Editor and in Players, including release Players)
- `ProfilerRecorder.StartNew()` begins recording; `LastValue` / `CurrentValue` read it; it must be `Dispose()`d. Create in OnEnable, read in Update, dispose in OnDisable. https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- `ProfilerRecorderHandle.GetAvailable` lists the supported metrics. Same link.
- Use it to log per-frame: main thread ms, GC allocated in frame, SetPass calls. Exact stat names ("Main Thread", "GC Allocated In Frame", "SetPass Calls Count") come from my memory of the docs, not from the fetched page ("not found" there). Verify with GetAvailable.

### 3.3 FrameTimingManager
- Captures high-level timing per frame. Results come with a delay of four frames. https://docs.unity3d.com/6000.0/Documentation/Manual/frame-timing-manager.html
- Supported fully on Windows (all graphics APIs); partial on Linux/WebGL/XR (no GPU frame time). Same link.
- It does not replace the Profiler; use it for high-level metrics, then profile. Same link.

### 3.4 Game view Stats overlay
- Game view toolbar has a Statistics overlay for rendering statistics (audio and graphics). https://docs.unity3d.com/6000.0/Documentation/Manual/GameView.html
- Per-frame max/worst-frame stat there: not found. Use your own recorder for worst frame.

### 3.5 Player log timestamps
- `-timestamps` prefixes every Player log message with the timestamp and thread ID. `-logFile <path>` sets the log path. https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html
- Log "SCREEN_SHOW start/end" with `Time.realtimeSinceStartupAsDouble` around each switch; compare to frame deltas (inference).

### 3.6 Finding a one-frame ~100 ms hitch
- Method (inference, standard practice):
  1. Record a script-side `Time.unscaledDeltaTime` ring buffer; when a delta exceeds 3x the median, log the frame number plus the last UI/mode event name.
  2. In the Profiler, enable "Highlight Filter / frame time" chart and jump to the red frame, or use the Profiler's frame search (exact UI path not verified).
  3. In Timeline, check whether the long sample is on Main Thread (script/Canvas/Instantiate), Render Thread (shader compile, `Shader.CreateGPUProgram`), or GPU wait (`Gfx.WaitForPresentOnGfxThread`).
- Disable "Incremental GC" and Deep Profile while hunting: not verified; deep profile slows the whole game (inference).

### 3.7 Headless approach
- `-batchmode` runs without display or input. With `-nographics` no graphics device is created, so there is no rendering, no shader/PSO cost, and no real canvas rebuild or render cost. https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html
- Consequence (inference): headless is good for logic/GC/Resources.Load/Instantiate timing, and blind to shader compile and GPU waits. Drive screens with a script (e.g. a smoke test flag), log timestamps with `-timestamps`, then diff against a windowed run.
- CI-style Player run with Profiler data capture flags: documented on a separate Profiler command-line page; I did not fetch it ("not found" here).

## 4. Editor vs Player
- Editor performance is worse than a build: the Editor runs its own GUI and extra windows (e.g. Scene view) on the same threads as your game, and does background tasks. Search-result summary. https://discussions.unity.com/t/low-performance-in-editor-but-working-fine-in-build/674725 and https://learn.unity.com/tutorial/diagnosing-performance-problems-2019-3
- Profiling through the Editor adds CPU and memory cost; a Development Player gives more accurate data. Same sources.
- Game view toolbar: Scale slider (zoom the view), Play Focused / Play Maximized / Play Unfocused, Stats overlay, Gizmos menu, Mute audio. https://docs.unity3d.com/6000.0/Documentation/Manual/GameView.html
  - A reduced Scale or low-resolution aspect setting changes how many pixels are drawn vs a native-res Player (inference). Test at Scale 1x and at the Player resolution.
  - Gizmos on adds draw cost; turn off for tests (inference).
  - Whether Maximize On Play changes the frame time: not found. (The three Play options are listed in the doc above.)
- Enter Play Mode Options (Edit > Project Settings > Editor): can disable Domain Reload, Scene Reload, or both for faster entry. The trade-off is less accurate simulation of what a build does; with them off, static state persists between runs. https://docs.unity3d.com/6000.0/Documentation/Manual/configurable-enter-play-mode.html
  - Static-field leaks across plays can produce state that looks like lag or first-show/second-show differences only in the Editor (inference).
- Editor focus stalls: no official source found for "unfocused Editor throttles ticking" ("not found"). Your own memory notes say an unfocused Editor stalls evals and stops ticking; treat that as local evidence, not a sourced fact.
- Editor-only costs not found as a list. Known sample categories (inference): `EditorLoop`, Inspector repaint, Scene view render, Profiler window itself. Look for these in the Timeline when profiling in the Editor.
- "Laggy in Editor but not in a build" checklist:
  1. Close Scene view, Inspector, Profiler window; keep only the Game view; set Play Maximized.
  2. Turn Gizmos and Stats off. Scale 1x.
  3. Compare the same sequence in a Development Player (no Profiler, then with).
  4. If Editor-only, treat it as noise and gate on Player numbers.
  5. Shader compile in the Editor can come from variants being compiled on demand rather than from a built cache; a Player build ships precompiled variants (inference; not sourced).

## 5. Title screen with a mode picker that doesn't hitch
- Sourced building blocks:
  - Hide/show screens with `Canvas.enabled`, not SetActive (1.1).
  - Pre-build and force-update canvases before the first show (1.4).
  - Prewarm shaders/PSOs behind the title (1.6).
  - Async scene load: `LoadSceneAsync` loads in the background; progress runs 0 to 0.9 for loading and 1 marks activation. With `allowSceneActivation=false` the scene waits; an un-released op blocks other async ops in the queue. Search-result summary. https://discussions.unity.com/t/loading-scenes-with-async-issues/876997 and https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be
  - Loading screens: show feedback with a coroutine so the game doesn't freeze, hide it when the scene is ready. https://leyaa.ai/codefly/learn/unity/part-3/unity-loading-screens-with-coroutines
- Mode-picker patterns (all inference; no authoritative UX source found, "not found"):
  1. One Title scene or one persistent canvas set. The picker is a canvas swap among pre-warmed canvases, not a scene load.
  2. Start the heavy work (map/scene load, weapon warm, shader warm) the moment the title appears, in progressive chunks (`WarmUpProgressively`, `LoadSceneAsync`), so by the time the player picks a mode it is already hot.
  3. On selection: preselect a default mode (last played) and begin loading it speculatively while the player is still browsing; cancel/discard if they pick another. Costly for memory; measure.
  4. The mode's data (description, best score, preview) comes from preloaded assets; no `Resources.Load` on hover/click.
  5. Keep hover/focus animation on a tiny dynamic canvas so a hover never dirties the picker's mesh (canvas splitting, 1.2).
  6. Gate the first input until warm-up is done, or show a thin non-blocking progress bar, rather than letting the player click into a stall.
  7. Pad-first: focus/selection highlight must not rebuild layout (use color tint or a separate highlight object, not a LayoutElement change).
- Transition between screens: use unscaled time for UI animation if timeScale is ever changed (inference).

## 6. A 90-minute plan to find and fix menu/mode/weapon switch hitches in a Unity 6 uGUI/TMP project

Preconditions: Development Player build, nothing else heavy running, CPU/GPU idle (your own notes: bench beside other jobs lies). Fixed resolution. Record numbers in a table.

### Minute 0-10: baseline instrument
1. Add a small MonoBehaviour that stores `Time.unscaledDeltaTime` each frame, with a ring buffer of 600. When a frame is more than 3x the median (and more than 8 ms), log: frame index, ms, last event tag (e.g. "show:Title", "mode:Survival", "weapon:2"). Write to the Player log. Launch with `-logFile` and `-timestamps`. (-timestamps/-logFile: https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html)
2. Add `ProfilerRecorder`s for main-thread time and GC allocation per frame and print them with the hitch log. Get exact names from `ProfilerRecorderHandle.GetAvailable`. (https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html)
3. Number to record: median frame ms, p99 frame ms, worst frame ms, over a 60 s scripted tour (each screen, each mode, each weapon, 3 times).

### Minute 10-30: reproduce and classify
4. Run the scripted tour in the Development Player with the Profiler attached (CPU Usage, Timeline). Do the tour twice: first pass (cold) and second pass (warm).
5. For each hitch above 8 ms: note cold vs warm.
   - Cold-only spike: first-use cost (shader/PSO, TMP glyph, asset load, Instantiate). Search `Shader.CreateGPUProgram` and `CreateGraphicsGraphicsPipelineImpl` (https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html).
   - Cold and warm: rebuild/GC/layout. Search "Canvas", "Layout", `GC.Alloc`, `GC.Collect`.
   - Mostly `Gfx.WaitForPresentOnGfxThread` / `Semaphore.WaitForSignal`: GPU or sync wait, not CPU work (https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-markers.html).
6. Numbers to record per hitch: main thread ms, render thread ms, GC bytes allocated in frame, dominant marker name and its self ms.

### Minute 30-50: fix the cheap, common causes
7. Canvas show/hide: switch each large screen from SetActive to `Canvas.enabled` toggling (https://learn.unity.com/tutorial/optimizing-unity-ui). Target: the "show screen" frame under 4 ms main thread above the steady-state frame (target is my suggestion, not sourced).
8. Split canvases: static art / dynamic text / per-frame HUD on separate canvases.
9. Boot prewarm pass (hidden): for each screen, enable canvas, `Canvas.ForceUpdateCanvases()`, then disable canvas (https://docs.unity3d.com/ScriptReference/Canvas.ForceUpdateCanvases.html).
10. TMP: for every dynamic font asset, `TryAddCharacters` with all UI strings, plus 0-9 and punctuation, at boot, with a no-progress guard (https://discussions.unity.com/t/tryaddcharacters-stuck-in-infinite-loop-with-tiny-multi-atlas-textures/858836). Number: count of `TryAddCharacters` misses (characters not added) must be 0; first-show glyph spike gone.
11. Remove allocations from the switch path (string building, new List, LINQ). Target: 0 B in the switch frame (https://docs.unity3d.com/6000.0/Documentation/Manual/performance-garbage-collector.html).

### Minute 50-70: weapons and assets
12. Warm all weapon view models at run start (inactive instantiate, one hidden render frame each, then toggle). Switch frame should show no `Instantiate` sample.
13. Replace `Resources.Load` in click paths with boot preload or `LoadAsync` (https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Resources.LoadAsync.html).
14. Shaders: run the tour once with `GraphicsStateCollection` tracing/recording, ship the collection, and call `WarmUpProgressively` during the title (https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html). Compare against `ShaderVariantCollection.WarmUp` on your graphics API, since DX12/Vulkan/Metal may still stall (https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ShaderVariantCollection.WarmUp.html). Number: count of `Shader.CreateGPUProgram` samples in the cold pass; goal 0 during gameplay.
15. Check you do not change `vSyncCount`, `targetFrameRate` or quality level on mode switch (https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html).

### Minute 70-85: re-measure
16. Re-run the scripted tour cold and warm in the Player: p99, worst frame, hitch count above 8 ms, hitch count above 33 ms. Target (mine, not sourced): 0 hitches above 33 ms, worst cold frame under 20 ms, warm tour under 8 ms.
17. Repeat the same tour in the Editor (Play Maximized, Gizmos off, Scale 1x, only Game view open). If the Editor is much worse, note the delta as Editor noise (https://discussions.unity.com/t/low-performance-in-editor-but-working-fine-in-build/674725).
18. Optional headless: `-batchmode -nographics` run of the same tour to isolate logic/GC/load costs (no render costs there) (https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html).

### Minute 85-90: lock in
19. Add the hitch logger's summary (hitches above 8 ms and above 33 ms per tour) as a pass/fail check in your smoke run so the regression is caught. Commit the recorded GraphicsStateCollection and tour script.

## 7. Sources used
- https://learn.unity.com/tutorial/optimizing-unity-ui
- https://docs.unity3d.com/ScriptReference/Canvas.ForceUpdateCanvases.html
- https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsDynamicFonts.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/shader-prewarm.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ShaderVariantCollection.WarmUp.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Shader.WarmupAllShaders.html
- https://docs.unity3d.com/ScriptReference/Experimental.Rendering.GraphicsStateCollection.WarmUp.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/frame-timing-manager.html
- https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/ProfilerCPU.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-markers.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/configurable-enter-play-mode.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/GameView.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/performance-garbage-collector.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Resources.LoadAsync.html
- https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
- https://discussions.unity.com/t/low-performance-in-editor-but-working-fine-in-build/674725
- https://learn.unity.com/tutorial/diagnosing-performance-problems-2019-3
- https://forum.unity.com/threads/dynamic-atlas-population-taking-a-long-time-even-for-a-single-character.1430263/
- https://forum.unity.com/threads/saving-loading-font-atlas-for-textmeshpro-font-assets-during-runtime.1325754/
- https://discussions.unity.com/t/tryaddcharacters-stuck-in-infinite-loop-with-tiny-multi-atlas-textures/858836
- https://discussions.unity.com/t/loading-scenes-with-async-issues/876997
- https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be
- https://leyaa.ai/codefly/learn/unity/part-3/unity-loading-screens-with-coroutines
