# PORT-RESEARCH: moving everything from the browser build to Unity (2026-10-02)

GG asked (verbatim): "great we need this next prompt to basically add everything that was built on the browser version
(WHICH SHOULD HAVE BEEN UNITY VERSION) and switch over to unity, more weapons, better movement, bosses, other gamemodes,
eveerything that was built, style meter, better UI. we need to resrecah theorughly and find best way to tarnsfer all of
that and rebuilt ON THE UNITY VERSION. start with resrecah create plan and next prompt, look and see if people have
done this before, browser game to unity".

**How this was made:**
- Three research agents ran on 2026-10-02:
  - an inventory of both repos (read only)
  - prior art for browser-to-Unity ports (web)
  - Unity patterns for what is left (web + this repo)
- Drive, Gmail and Calendar had nothing on SKYGRIND. Linear wasn't searched (it needs a login).
- The plan built on this is `PORT-PLAN.md`; the translation rules are `PORT-RULES.md`; the first night is `NIGHT9.md`.

**Tags:**
- (V): the claim is on the page the agent opened.
- (S): a search summary only.
- (I): inference.
- (L): read in our repos on 2026-10-02.

## 0. Our own data (the inventory)

### 0.1 Sizes

| | Web (`skygrind`, `p17-night6`) | Unity (`SKYGRIND-unity`, `u2a-demo`) |
|---|---|---|
| Code | `src/`: 190 files, **50,411 LOC**; without debug 41,780; without debug and `config.js` 39,141 | Core 47 files, 14,242 LOC (pure C#); Game 31 files, 5,783; Bench 997 |
| Tests | 51 vitest files, about 432 tests | 35 EditMode files, 237 tests, 16 trajectory replays, 55 golden tests |
| History | 121 commits, Sep 28 - Oct 1 | 56 commits since Sep 30 |
| The oracle | `port-export-p17/scripts` (export-config, vectors, trajectories, maps, verify) | `PortExport/`: config.json, 13 vector files (1,267 calls), 16 trajectories, 3 maps |

**Ported vs remaining:**
- About 16,400 web LOC are ported.
- **About 22,700 remain** (estimated file by file, ±15%).
- The biggest remaining parts:
  - bosses (about 3,170)
  - RIVAL / Duel (about 1,950)
  - FX (about 1,270)
  - UI (about 1,840 + the menus)
  - the guns (about 2,700 with projectiles)
  - the enemy roster (about 1,290)

**Throughput:** NIGHT8 ported about 1,900 web LOC plus UI in 6 h 19 min of a 12 h cap (L, NIGHT8-LOG).

**`PORT-INVENTORY.md` is out of date** (taken at `a001407`, 44,869 lines). It and PORT-STATUS.md lack rows for:
- trials, ghosts, finishes
- daily, seeds, unlocks
- objectives, tips
- gamepad, holdToggle, framePace
- killConfirm, momentumArc

### 0.2 Everything the web game has, against Unity today

| Content | Web | Unity |
|---|---|---|
| Weapons (`weapons.catalog`) | **18**: 13 guns + 5 melee in 8 families (KESTREL, THUNDERCLAP, STITCHER, COMET, LONGWAVE, RATTLER, TORCH, BOUNCER, ZAPPER, DISCUS, VORTEX, CHOIR, PRISM; PULSE GAUNTLET, SUNDER BLADE, FLICKER DAGGERS, THUMPER HAMMER, TETHER WHIP); 12 specials; a momentum variant per gun; 16 wall buys; 7 finishes | 3 (KESTREL, THUNDERCLAP, PULSE GAUNTLET), 1 wall buy |
| Combat systems | the arsenal (FRESH, PRIMED, CROSS / COLLAPSE / OVERLOAD, 5 statuses), momentum (speed as damage) + impact waves, projectiles, aim assist's V-lock / homing / lunge, static aura, health orbs | none of these |
| Movement | the mover (bit-exact in Unity), swing math, the ARCLINE gadget (a 5500 buy, enemy pull), jump pads, the momentum arc | the mover, swing math and pads exact; no ARCLINE buy, no enemy pull, no momentum |
| Enemies | **7 kinds** (husk, swarmer, drone, spitter, charger, lancer, warden), 5 affixes, squads / slots / flankers | 3 kinds, no affixes, no squads |
| Bosses | **5**: BULWARK (r5), MONOLITH (r10), RIVAL PRIME (r15), ORBITER (r20), HIVE (r25); plus RIVAL (the Duel bot) | none |
| Modes | **7 title cards**: SURVIVAL, BOSS RUSH, DUEL (3 levels, first to 3 / 5), TRAINING, TUTORIAL (10 steps), CLASSIC ARENA, TRIALS (6 courses with ghosts); plus DAILY, seeded runs, mutators (4), challenges (24), the kiosk | SURVIVAL |
| Survival | 14 perks, 19 mods, 5 power-ups, 4 traps, 1 gadget, HOLD / GOLD RUSH objectives, the PROTOCOL quest, 7 tips, 7 unlock flags, the forge | 5 perks (4 more need nothing new), 10 mods, 5 power-ups, the forge, READY |
| Style meter | styleCore / styleMath / styleMeter / styleSim | ported (golden); styleCore and styleSim vectors not yet consumed |
| UI | 14 menu screens, the full HUD (crosshair states, vitals, style box, weapon block + hotbar + radial, perk row, power-up chips, boss bar, kill feed, award pops, markers), tooltips, damage numbers, boss card, signs, loading; **41 settings in 6 tabs, 18 rebindable actions, the gamepad** | TITLE, PAUSED, DOWN, RESULTS, SETTINGS (17 settings in 4 tabs + LAB), the MODS draft; most of the HUD |
| Look | 10 ShaderMaterials + 4 shader patches + a grade pass; sky, skyline, AO volume, motes, barrier, x-ray, rim glow, dissolve, blob shadows; 2 GLB guns; 6 woff2 fonts (OFL) | 4 shaders (Stone, Field, Sky, Acid), URP bloom, TMP's default font |

### 0.3 The oracle we already have, and what's missing

- **Vectors exported but not consumed by any Unity test:** `weaponMath`, `arsenalMath`, `momentumMath`, `styleCore`,
  `styleSim`, most of `gunMath`, most of `spawnMath`.
- **Pure modules still to vectorize:**
  - the README's list: regenMath, pointsMath, bossMath, zoneMath, barrierMath, qualityMath, kicks, tokens, slots, aim,
    crowd, flowField, watchdog, reach, astar
  - newer ones: modMath, powerUpMath, trialMath, ghostCodec, seeds, finishes, deathRecap, hitDirection, tooltipText,
    springs, trauma, gamepad, holdToggle, runMoments' gradeOf, bossKit
- **New trajectories to record:**
  - the 6 trial routes (`tests/support/trialRoutes.js`: scripted Mover routes on the real maps, with jump pads)
  - moving core pillars
  - perk-modified movers
  - the lowGravity mutator
  - the ADS walk
- **How to add a set:** copy `port-export-p17/` into a new untracked `port-export-p18/` in the web repo (CLAUDE.md rule
  2 allows only that), add a recorder in `scripts/vectors.mjs`, list the module in `verify.mjs`, and run `all.mjs` then
  `verify.mjs`. Copy the output to `PortExport/vectors/`, and add a C# test in `CombatGoldenTests`' style.
- **Stateful systems** (the arsenal, projectiles, a boss) record as `styleCore`-style sessions:
  `{op, args, result, events, after}`.

## A. Has anyone done this? Browser games moved to Unity

### A.1 Case studies

| Game | From → to | How | How long | What went wrong / the lesson | Sources |
|---|---|---|---|---|---|
| **Vampire Survivors** (poncle) | Phaser (JS) → Unity, v1.6 (Aug 17, 2023) | Method not published (rewrite vs tools unknown). Freelancers hired March 2022; consoles and mobile moved first, PC last | about 17 months, announcement to PC switch (I) | The first Unity build was "10 times more performing", but its physics "behaved in a very realistic way compared to the arcade-y nature of the original". They **"discarded a fully working, first version of the new engine"**, ran both engines "in parallel for a while", and kept the Phaser build on a beta branch until the differences were "very subtle" (V) | [wiki 1.6](https://vampire.survivors.wiki/w/Updates/Version_1.6:_New_engine_&_Local_co-op), [80.lv](https://80.lv/articles/vampire-survivors-will-get-new-engine-dlcs-updates-in-2023), [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors) |
| **CrossCode** (Radical Fish + Deck13) | impact.js (HTML5) → Switch / PS4 / Xbox | Interpreting the JS "turns out to not be fast enough", and consoles allow no JIT, so Deck13 **compiled the JS ahead of time to C++** | R&D from about 2015; shipped July 2020 | "We underestimated the amount of work a bit... performance became a major challenge" (V) | [Radical Fish](https://www.radicalfishgames.com/?p=6892), [Siliconera](https://www.siliconera.com/crosscode-interview-radical-fish-games-on-console-ports-and-whats-next/) |
| **Numolition** (Q42) | HTML5 DOM / CSS → Unity | the game logic ported JS → C# first, then the visuals rebuilt | about 6 months | shaders, tooling, "2D is still 3-D" (V) | [Smashing Magazine](https://www.smashingmagazine.com/2014/04/rebuilding-an-html5-game-in-unity/) |
| **Realm of the Mad God Exalt** | Flash → Unity | a rewrite; Flash development halted | beta March 2020 | stability became the "absolute top priority"; 22% of players hadn't tried the new client (V) | [RotMG](https://remaster.realmofthemadgod.com/?p=577) |
| **Habbo** | Flash → Unity | a rewrite | beta Dec 2020 | Feb 2021: the old client came back because the Unity one "was lacking key features" (V) | [Wikipedia](https://en.wikipedia.org/wiki/Habbo) |
| **AdventureQuest Worlds** | Flash → Unity ("Infinity") | a rewrite by hand: "every detail must be painstakingly recreated by hand" | announced 2015, still slipping in 2023 | years of delay (V) | [Wikipedia](https://en.wikipedia.org/wiki/AdventureQuest_Worlds), [MMOBomb](https://www.mmobomb.com/adventure-quest-worlds-moving-to-unity-engine-adventure-quest-worlds-infinity) |
| **TypeScript compiler** (not a game) | TS → Go (2025) | "a port rather than a rewrite", so behaviour matches "even in edge cases". A generator gave syntax only; the rest was "hand-written (or at least, hand-accepted completions)" | - | not a transpile: a faithful port judged by the old test suite (V) | [devclass](https://devclass.com/2025/03/12/typescript-compiler-ported-to-native-code-c-faithful-ask-why-go-was-used/), [typescript-go #467](https://github.com/microsoft/typescript-go/discussions/467) |

**What the cases say together (I):**
- The ports that kept the old build as the reference and judged the new one by behaviour converged: Vampire Survivors,
  the TS compiler, CrossCode.
- The rewrites without that reference slipped for years or shipped missing features: AQW, Habbo.
- SKYGRIND already does the first kind: the web build is frozen as the reference, and golden vectors and replays
  compare behaviour bit for bit.

### A.2 Tools: is there a shortcut?

- **JS / TS → C# transpilers:**
  - None is maintained for game code: robin-han/typescript-converter has 29 stars and needs .NET 2.1; rollrat's is for
    "limited circumstances"; Haxe 5 removed its C# target (V).
  - Even the TS → Go team used a generator for syntax only (V).
  - In practice, an LLM following a rulebook and checked by tests is today's transpiler (I).
- **Running the JS inside Unity** (Jint, PuerTS, ClearScript, OneJS, ReactUnity):
  - They exist and work: PuerTS runs V8 / QuickJS / Node in Unity; OneJS renders React into UI Toolkit; ReactUnity
    renders a CSS subset over Yoga (V).
  - No measurement exists for a JS game sim at a 120 Hz fixed step.
  - Consoles and iOS forbid a JIT, and JIT-less V8 is 40-80% slower (V, [v8.dev](https://v8.dev/blog/jitless)).
  - CrossCode found interpreting the JS too slow (V).
  - **Verdict:** JS stays a test oracle (Node / V8 produce the golden vectors), never the runtime of the sim (I).
- **Models:**
  - three.js GLTFExporter → .glb → glTFast (Unity's official importer, URP) or UnityGLTF (V).
  - Our models are procedural "blocky kit" code, so they port as code (BlockyKit.cs). With GG's OK they could port as
    exported .glb instead (I).
- **Shaders:**
  - Raw GLSL is "not recommended" in Unity; write HLSL (ShaderLab, or a Shader Graph Custom Function node).
  - glslang + SPIRV-Cross can draft HLSL from GLSL.
  - three.js chunks and uniforms map by hand (V).
- **UI:** UI Toolkit is "inspired by standard web technologies": UXML ≈ HTML, and USS ≈ a subset of CSS (display flex or
  none, position absolute or relative, no grid) (V).

### A.3 How AI-assisted migrations succeed (and fail)

| Who | What | Result | How | Source |
|---|---|---|---|---|
| Google | int32 → int64, JUnit3 → 4 and 39 other migrations | 80% of landed edits fully AI-written; 74% of changes LLM-generated; about 50% time saved | Code search / AST tools find the targets, the LLM edits, then build + tests + a repair loop check it, with several prompt variants per file. "LLMs alone through simple prompting is not sufficient". Human review was the bottleneck, so generation was throttled to it | [arXiv 2501.06972](https://arxiv.org/html/2501.06972), [2504.09691](https://arxiv.org/abs/2504.09691) |
| Airbnb | 3.5k test files, Enzyme → RTL | 75% in 4 h, 97% in 4 days; estimated at 1.5 years, took 6 weeks | A pipeline per file (refactor → jest → lint → tsc), each step retried with its errors; 40-100k-token prompts with related files; the last 3% by hand | [InfoQ](https://www.infoq.com/news/2025/03/airbnb-llm-test-migration/) |
| Slack | the same migration | AST tools alone 45%, AST + LLM 80% | feed in the real rendered output; humans verify every change | [Slack Engineering](https://slack.engineering/balancing-old-tricks-with-new-feats-ai-powered-conversion-from-enzyme-to-react-testing-library-at-slack/) |
| Spotify (Honk) | background coding agents | an LLM judge vetoes about 25% of sessions; the worst failure is a change that "passes CI but is wrong" | describe the end state, say when not to act, give verifiable goals, one change per prompt, short parsed verifier errors, a stop hook | [Spotify part 2](https://engineering.atspotify.com/2025/11/context-engineering-background-coding-agents-part-2), [part 3](https://engineering.atspotify.com/2025/12/feedback-loops-background-coding-agents-part-3) |
| Anthropic / Bun | Zig → Rust, about 1M lines | under 2 weeks; 100% of the suite before merge; 19 regressions after | A **rulebook** ("add one sentence to the rulebook and regenerate the affected batch"), stress-tested on 3 files first; a work queue rebuilt from disk ("resumable by construction"); a 7-scenario parity harness where "any behavior change [is] a bug"; "review loop results, not code" | [claude.com](https://claude.com/blog/ai-code-migration) |
| Research | whole-program translation | 2.1-47.3% correct (Lost in Translation); AlphaTrans 96% compiled but only 25% functionally validated | translate in dependency order, code and tests together, and check equivalence per step (Syzygy) | [2308.03109](https://arxiv.org/abs/2308.03109), [2410.24117](https://arxiv.org/abs/2410.24117), [2412.14234](https://arxiv.org/abs/2412.14234) |

**The pattern (I):**
- The 75-97% results all have four things:
  - small units in dependency order
  - an oracle in place before the translation
  - machine checks with bounded retries
  - a rulebook that gets fixed when the same mistake repeats
- Whole-program, one-shot translation is where the 2-47% numbers come from.

### A.4 Parity testing, and the traps of JS → C#

- **Golden master / characterization tests** record what the old code does
  ([Wikipedia](https://en.wikipedia.org/wiki/Characterization_test)). **Differential testing** treats any difference as a
  bug ([Wikipedia](https://en.wikipedia.org/wiki/Differential_testing)).
- **Deterministic replays at run scale:**
  - Factorio compares a state CRC after every integration test ([FFF-60](https://factorio.com/blog/post/fff-60)).
  - Riot logs hierarchical key-value state per frame, so two playbacks can be diffed and bisected (580,000 cases in 20
    iterations). The causes they found:
    - uninitialised variables
    - leaks from non-deterministic systems
    - compiler float differences

    They replaced systems "in parallel with the legacy tech"
    ([Riot](https://www.riotgames.com/en/news/determinism-league-legends-fixing-divergences)).
  - Floats are deterministic only with the same compiler, CPU and settings
    ([Gaffer On Games](https://gafferongames.com/post/floating_point_determinism/)).
- **Feel:**
  - Frame pacing: Unity fixed a deltaTime jitter by sampling presentation time
    ([Unity blog](https://unity.com/blog/engine-platform/fixing-time-deltatime-in-unity-2020-2-for-smoother-gameplay)).
  - The web's requestAnimationFrame follows the display and pauses in background tabs
    ([MDN](https://developer.mozilla.org/en-US/docs/Web/API/Window/requestAnimationFrame)).
- **The JS → C# traps:** PORT-RULES.md sections 2-6. Each is sourced on MDN or Microsoft Learn: number types, rounding,
  int coercion, Math precision, unseedable `Math.random`, stable sort, Map order, `||` defaults, culture,
  EventEmitter order, allocation.

## B. How to build the rest in Unity (patterns for this architecture)

| Topic | Decision | Why (evidence) |
|---|---|---|
| **UI system** | **Stay on uGUI + TMP, built from code**: the HUD, every in-run screen, results, and anything per player. Revisit UI Toolkit only for single-focus front-end menus, after co-op ships. | **Split-screen co-op (Oct 28) needs per-player UI focus.** The Input System's MultiplayerEventSystem does that for uGUI ([UISupport](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/UISupport.html)). UI Toolkit has no per-player focus, and 6.3's split-screen input is "not fully supported" ([forum 2026](https://discussions.unity.com/t/uitoolkit-eventsystem-changes-in-6-3-and-split-screen-input/1707949)).<br>**Neither system ports CSS mechanically.** USS lacks grid, skew, box-shadow, gradients and @keyframes ([USS properties](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-USS-SupportedProperties.html)), and the web's UI uses about 10 grids, 19 skews, 19 gradients, 12 box-shadows and 8 keyframe animations (L).<br>The measured uGUI HUD already allocates nothing per frame (L). |
| **Content data** | **JSON stays the only source of gameplay numbers** (`PortExport/config.json` + `Design/unity-config.json`). Load each new catalog (guns, enemies, bosses, modes) once into typed, read-only Core records. ScriptableObjects only in `Skygrind.Game`, for presentation (meshes, materials, icons, FX), keyed by the web's ids, with an EditMode test that every id matches. | Core is `noEngineReferences` (L); the JSON already reaches WebGL (L); ScriptableObjects are read-only in builds ([manual](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html)). Hot reload: re-read the JSON at run start in the Editor, never mid-run. |
| **Projectiles and many enemies** | Fixed-capacity pools in Core (capacities from config) that **keep the web's iteration and removal order**. Enemies as pooled GameObjects. Projectiles, cube particles and motes drawn from the sim's arrays with `Graphics.RenderMeshInstanced` (no GameObjects). GPU Resident Drawer later (U3-05), with the per-renderer Shader User Value in place of MaterialPropertyBlocks. No Entities Graphics. No VFX Graph in the shared build (WebGL has no compute shaders). | [RenderMeshInstanced](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html); [GPU Resident Drawer](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/gpu-resident-drawer.html); [Shader User Value](https://docs.unity3d.com/6000.3/Documentation/Manual/renderer-shader-user-value-intro.html); [VFX Graph requirements](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@17.3/manual/System-Requirements.html). Swap-remove would change the order in which two projectiles resolve (I). |
| **Decals** | URP Decal Projector + the Decal renderer feature (technique Automatic); one atlas material; a ring-buffer pool capped from config; Rendering Layers to keep decals off enemies and the viewmodel. | [Decal renderer feature](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-feature-decal-reference.html) |
| **Bosses** | Port each boss module line for line into Core as an explicit enum state machine with whole-step timers. Phases are data (HP thresholds, attack sets, timings). Structure: phase → pattern → windup / active / recover, with a pushdown stack for interrupts (stagger, parry). Not Unity Behavior: its agent ticks in `Update`, outside the 120 Hz loop. | [Game Programming Patterns: State](https://gameprogrammingpatterns.com/state.html); [BehaviorGraphAgent](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/api/Unity.Behavior.BehaviorGraphAgent.html) |
| **Modes** | An `IMode` in Core with hooks at fixed points of `simulate()`; `SurvivalGame` becomes the mode host. One bootstrap scene builds each mode's map from exported data (maps are data, not scenes). The kiosk / attract mode replays recorded inputs or the bot. | The web's modes/*.js share one main.js loop (L). [LoadSceneMode.Additive](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.LoadSceneMode.Additive.html) only for hand-made set dressing. |
| **RIVAL bot** | Port botBrain, rival and rivalPrime to Core. The bot emits the same InputFrame a human does into its own Mover and Loadout, with its own Mulberry32 stream. Duel replays then cover it. | (I) the Mover is already deterministic and replay-tested. |
| **Determinism** | Port the seven remaining System.Math transcendentals (Atan2, Exp, Tan, Acos, Asin, Pow, Atan) from V8's ieee754 into JsMath, plus a test that fails if Core calls them from System.Math. Audit the Float32Array ports (compute in double, store once). Run the golden vectors inside the built Player (`-skygrindGolden`). Choose the Steam scripting backend before U6 (the Windows Player is Mono today; Mono is gone in Unity 6.8). | System.Math "may differ between different operating systems or architectures" ([Microsoft](https://learn.microsoft.com/en-us/dotnet/api/system.math.atan2)). Wine 8.6 moved msvcrt exp / atan2 to musl ([report](https://www.linuxcompatible.org/story/wine-86-released/)). V8 has its own versions ([ieee754.h](https://github.com/v8/v8/blob/main/src/base/ieee754.h)). Mono does float maths in double ([Aras P., 2026](https://aras-p.info/blog/2026/06/11/Unity-vs-floating-point/)). [CoreCLR guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279) |
| **Replays at run scale** | Record seed + config hash + per-step InputFrames (which is what the 16 trajectories are). Add a whole-run replay that hashes the sim state every N steps. InputRecorder only for input-layer bugs. | (I) + A.4 (Factorio, Riot) |
| **Co-op readiness** | **Now:** `Player[]` in Core (per-player Mover, Health, Regen, Loadout, Style, AimAssist); `owner` on every event (the web already does this); enemies target the nearest living player; the flow field takes several seeds; co-op numbers under `design.coop.*`.<br>**Input and views:** PlayerInput / PlayerInputManager (split-screen camera rects); per player a camera, a viewmodel, a HUD canvas and a MultiplayerEventSystem.<br>**Online, later:** a listen-server host runs the sim; clients send InputFrames and predict only their own Mover; Unity Transport / Relay or a Steam transport as the pipe. No lockstep (cross-machine float determinism). | [PlayerInputManager](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/PlayerInputManager.html); [Gaffer: lockstep](https://gafferongames.com/post/deterministic_lockstep/); [choosing netcode](https://unity.com/blog/games/how-to-choose-the-right-netcode-for-your-game); the web's COOP-RESEARCH.md (L) |
| **Render parity** | Keep the hand-written HLSL world shaders (SRP Batcher compatible). **Two real mismatches to fix:**<br>- **Fog:** three.js is `smoothstep(near, far, depth)`; URP's is linear → a shared HLSL fog include.<br>- **Tone mapping:** the web uses three's ACES with a 1/0.6 exposure scale; URP's ACES is an approximation → three's curve as a custom pass or a baked LUT.<br>Compare same-step screenshots from replays. glTFast at edit time for the web's two .glb guns. | [three fog chunk](https://raw.githubusercontent.com/mrdoob/three.js/dev/src/renderers/shaders/ShaderChunk/fog_fragment.glsl.js); [three tonemapping chunk](https://raw.githubusercontent.com/mrdoob/three.js/dev/src/renderers/shaders/ShaderChunk/tonemapping_pars_fragment.glsl.js); [URP tonemapping](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/post-processing-tonemapping.html); [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) |
| **Agent workflow** | Scenes from editor scripts, never YAML by hand. An Editor-only asmdef for `Assets/Editor`. Split `Skygrind.Game` into UI / Views / Boot as it grows (an edit recompiles only its dependents). Audit the Game statics before Fast Enter Play Mode becomes the default (6.6) and then mandatory (6.8). | [asmdefs](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-intro.html); [domain reloading](https://docs.unity3d.com/6000.3/Documentation/Manual/domain-reloading.html) |

## C. What the two halves say together (ranked, for SKYGRIND)

1. **Port, don't rewrite, and never swap in an engine feature for web logic.** No Unity physics, NavMesh, Animator,
   Behavior or VFX Graph in place of a web system.
   - Evidence: Vampire Survivors threw away a working Unity build because its physics changed the feel; the TS → Go
     port kept edge cases by porting, not rewriting.
   - SKYGRIND already works this way (CLAUDE.md rules 3-4). Keep it for every system left.
2. **The oracle first, for everything left (A.3).**
   - Export a `port-export-p18` with vectors for every pure module still missing (§0.3).
   - Consume the seven vector sets nobody reads yet.
   - Add the 6 trial routes as trajectories.
   - Node / V8 is the only source of expected values.
3. **One web module per commit, leaves first, with a rulebook** (PORT-RULES.md; Anthropic, Spotify). When the same
   translation mistake appears twice, the rule changes and the affected files are re-checked.
4. **Dependency order:**
   1. arsenal + momentum
   2. the players list (co-op readiness)
   3. projectiles and guns
   4. melee, perks, mods
   5. the enemy roster
   6. the boss framework and bosses
   7. RIVAL / Duel
   8. modes
   9. Survival depth and meta (trials, challenges, daily)
   10. input and UI
   11. look

   The inventory's 18 work packages fit into **about 11 nights at NIGHT8's pace** (about 2,000-2,500 web LOC per night).
5. **Co-op readiness early:** the `Player[]` refactor goes before projectiles, so every system after it is written per
   player (B). Local split-screen can then land whenever GG wants it for Game 3.
6. **Determinism at the base:** V8's transcendental functions in JsMath, and the golden vectors inside the built Player.
   A Proton / Steam Deck replay depends on both (B).
7. **UI: stay uGUI + TMP**, with a few reusable widgets for what CSS gave the web: a row / column / grid helper, a skew
   mesh, gradient images, shadows. Convert the six OFL theme fonts to TMP SDF. Split-screen decides it (B).
8. **Raise parity from functions to runs once the systems exist:**
   - a seeded whole-run replay with state hashes every N steps (Factorio, Riot)
   - per-night parity reports
   - the web build kept runnable side by side (Vampire Survivors)
9. **Look parity is its own night:** fog, tone mapping, the world material, sky, AO, motes. Compare against web
   screenshots from the same seed and step.
10. **GG reviews results, not code** (Anthropic; Google's finding that review was the bottleneck). Each night ends with a
    one-page parity report and a 10-minute play checklist; owner-only decisions go on a decide list, never into code.
