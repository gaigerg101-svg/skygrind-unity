# PORT-RULES: the JS → C# rulebook for porting SKYGRIND's web modules

Every night that ports a web module follows these rules. CLAUDE.md's hard rules still come first; this file is the
"how to translate" layer under them. It is a **rulebook** in the sense of the large AI-assisted migrations
(PORT-RESEARCH.md B.3): when a review, a test or a replay catches the same translation mistake twice, add one sentence
here and re-check every file that has the pattern. Don't patch files one by one and leave the rule unwritten.

The web source is `C:\Users\gaige\dev\skygrind` at `p17-night6` (read only). Port from it, not from memory.

## 1. The unit of work

1. **One web module per commit, leaves first.** Port in dependency order (PORT-PLAN.md): a module whose imports are
   not ported yet waits. A module that is too big for one commit goes class by class, each commit compiling and green.
2. **The oracle comes before the translation.** Before porting a pure module:
   - port its web tests (vitest) as EditMode tests and see them fail
   - if it has golden vectors in `PortExport/vectors/`, load them
   - if it has none and it is pure (no three.js, no DOM), export them first (section 9)
3. **Line for line, comments kept.** The C# keeps the web's structure, names (PascalCase methods are fine) and comments,
   with the web file named in the header. A reviewer must be able to put the two side by side.
4. **Scope:** port what the module does on the web. Improvements go in `Design/unity-config.json` behind a flag that
   is off, or in an ideas list. Never in the port itself.

## 2. Numbers and math

- **`double` everywhere in Core.** JS numbers are doubles. `float` only in views (`WebSpace.ToUnity` converts).
- **Rounding:** `Math.round` → `JsMath.Round` (half toward +∞); never `Math.Round` (half to even) for a web value.
- **Min / max:** `Math.min` / `Math.max` → `JsMath.Min` / `JsMath.Max` (NaN and −0 like JS).
- **Transcendentals (NIGHT9):** every `Math.sin / cos / tan / atan / atan2 / asin / acos / exp / tanh / log / pow /
  hypot` → the `JsMath` one (V8 13.6's fdlibm code, checked by `vectors/jsMath.json`). Never `System.Math` for them in
  Core: it goes to the C runtime, which differs by OS and by .NET runtime (Mono's `Math.Pow` misrounded 25 recorded
  inputs, CoreCLR's 1). `DeterminismTests` fails on any such call. `Math.Sqrt`, `Floor`, `Ceiling` and `Abs` are exact
  and stay. A function JsMath lacks (`Math.log2`, `expm1`, ...) gets ported from V8's `src/base/ieee754.cc` first, with
  its jsMath records.
- **`Math.pow` is the host libm in V8** (`--use-std-math-pow`): Node on Windows returns the UCRT's last bit, which is not
  always correctly rounded. `JsMath.Pow` keeps V8's own cases (NaN exponent, (±1) ** ±∞, `y == 2` → `x * x`, `y == 0.5`
  → `sqrt`) and is deterministic past them: integer exponents correctly rounded (exact), the rest fdlibm. jsMath.json
  records pow only where V8's value doesn't depend on the host.
- **Grouping (NIGHT9):** keep the web's parentheses and order in floating-point sums and products: `yaw + (PI + side *
  PI / 3)` is not `(yaw + PI) + side * PI / 3` in the last bit (slots.js flankPoint was regrouped).
- **`Float32Array` storage** (the nav field, the core grid): compute in double and cast to `float` once at the store, as
  a JS typed-array write does. Never do the arithmetic in `float` (Mono and IL2CPP differ there).
- **Integers:** `x | 0` → `(int)` of a value already in range, or `unchecked((int)(long)x)`; `x >>> 0` → `uint`;
  `Math.imul(a, b)` → `unchecked(a * b)` on ints. C# `int / int` truncates: JS has no int division, so divide doubles.
- **`%`:** JS `%` on doubles keeps the dividend's sign; so does C#'s. Same result; keep the operands double.
- **Number formatting:** `toFixed(n)` → `ToString("F" + n, CultureInfo.InvariantCulture)`, and check a half-way case
  when the text matters; `String(x)` of a double → `ToString("R", InvariantCulture)`. Never the current culture.

## 3. Truthiness and defaults

- **`a || b`** treats `0`, `""`, `NaN`, `null`, `undefined` and `false` as missing. Translate it to the exact test the
  web relies on (`a != 0 ? a : b`, `string.IsNullOrEmpty(a) ? b : a`); `??` only covers null.
- **`a ?? b`** → `??` (null) or a `Has()` check on config paths.
- **`!x` on a number** is `x == 0 || double.IsNaN(x)`.
- **Optional fields** (`o?.blast`) → nullable or a default; keep the web's default.

## 4. Collections and order

- **Iteration order is behaviour.** JS `Map`, `Set` and object keys iterate in insertion order; C# `Dictionary` and
  `HashSet` do not promise any. When the web iterates a map and the order matters (a draw, a tie, a list shown), keep a
  `List` for the order beside the dictionary.
- **Sorting:** JS `Array.sort` is stable; `List<T>.Sort` is not. Use a stable sort (LINQ `OrderBy` off the step path, or
  an index tiebreak in the comparer) whenever equal keys can occur.
- **`splice`, `indexOf`, `filter`, `map`:** in per-step code, indexed loops over reused lists (section 6).
- **`for (const k in obj)`** over config objects: `cfg.Obj(...)` is a `Dictionary` that JsJson fills in the JSON's
  order. .NET enumerates it in that order while nothing is removed, but that is an implementation detail, not a promise.
  Where the order decides behaviour (a weighted pick, a draft, a tie), copy the keys into a `List` at load and iterate
  that.

## 5. Randomness and time

- **Randomness:** the web's `mulberry32` is `Mulberry32` (Core/CoreGrid.cs), seeded per run with the web's seed
  offsets and one stream per system as on the web. Never `UnityEngine.Random`, `System.Random` or a clock seed in sim
  code. A check that depends on a draw seeds its run (the smoke's spitter step).
- **Time:** the sim steps at 120 Hz; timers count whole steps (`MovementMath.StepsFor(seconds, dt)`); the web's `dt`
  arithmetic stays as it is. Frame time is for cosmetics only (camera feel, UI tweens, particles).
- **Hitstop and pause** freeze the sim as the web's main.js does; nothing in Core reads a wall clock.

## 6. Events, allocation and lifetime

- **Events keep the web's names** (PORT-STATUS.md's list and the web's docs/AUDIO-HOOKS.md). New names are marked
  port-only there.
- **EventBus semantics:** synchronous, in registration order, a handler removed during an emit still finishes that
  emit (Node's EventEmitter). Payload classes use the web's field names.
- **No strings built per step or per frame (NIGHT10):** an interpolated config key (`$"ads.weapons.{w}"`), a label or a
  concatenation on the step / render path allocates every call. Cache per key at load or on change (a `Dictionary`
  filled on first use), and set texts only when the value changed.
- **No config lookups on the step or frame path (NIGHT11):** `GameConfig.Has` splits its path every call (an array
  and a string per segment), and `Get` / `Num` / `Str` on an interpolated path build the string first. Read the config
  once at construction (a params struct, a field, a per-key cache) and keep only cached reads in Step / Render.
- **No `List.Find` / `FindIndex` / `Exists` / `FindAll` with a capturing lambda on the step or frame path (NIGHT12):**
  each call allocates a closure and a delegate. Write the indexed loop (or a dictionary built once). A lookup helper
  that looks harmless (`SurvivalMap.ZoneIndex`) may sit under a per-step delegate (a buyable's `available()`).
- **Zero allocation per step:** no closures, LINQ, `foreach` over interfaces or boxing on the step path. Use
  `Boxes.Int`, reused payloads (read them inside the handler), scratch lists, cached delegates (a method group passed
  each step allocates). A system with a per-step path gets a line in `ZeroAllocTests` (warm the delegate first; the JIT
  counts).
- **Pools:** projectiles, globs, puddles, drops and effects are pooled Core objects with views that follow them.

## 7. Config and data

- **Every gameplay number comes from `PortExport/config.json` through `GameConfig`** (`cfg.Num("weapons.catalog.x.y")`),
  read once into a params struct (`From(GameConfig)`), never as a literal.
- **Unity-only gameplay numbers** live in `Design/unity-config.json` with a `_why`; render and UI numbers in
  `UnityTuning.cs` with a comment.
- **A web module that reads `config.x` at call time** still reads it once at construction in C#, unless the web mutates
  it at run time (mutators, settings), in which case keep a reference to the live value.

## 8. three.js and the DOM

- **Coordinates:** web (x, y, z) → Unity (x, y, −z) through `WebSpace.ToUnity` / `FromUnity`; a web yaw / pitch →
  `WebSpace.LookRotation`. Never convert inside Core.
- **Meshes:** the web's procedural models (assets/*Models.js, the blocky kit) are rebuilt in code (`BlockyKit.cs`,
  `MeshKit.cs`) with the same parts, sizes and colours. GG owns enemy looks: rebuild them as they are.
- **Materials and shaders:** colours through `Look.C(cfg, ...)`; a three.js shader becomes HLSL in a ShaderLab file under
  `Assets/Shaders` (never raw GLSL); post-processing on the URP Volume.
- **No PhysX:** every ray, overlap and move uses the C# web collider (`ThreeOctree`, `TriangleWorld`, `CoreGrid`).
- **DOM / CSS → uGUI:**
  - Pixel sizes at 1080p; UI text at least 16 px.
  - Canvases split by change rate; `SetText` only when a value changed.
  - Layout numbers in `UnityTuning.cs`, colours from config.
  - A screen's structure follows the web's HTML (the same order of rows, the same labels).

## 9. The oracle: golden vectors and replays

- **Golden vectors** are recorded web calls (`PortExport/vectors/<module>.json`), exported by the web repo's export
  scripts into a new untracked `port-export-pNN/` folder (never into the web repo's tracked files), copied into
  `PortExport/`, and replayed bit for bit by an EditMode test. Generate inputs broadly (random seeds, edge values), not
  only the web tests' cases.
- **Replays** are recorded runs (seed + per-step input) with their expected state. The 16 movement trajectories are the
  model; run-scale replays (a scripted round, a boss fight) extend them when a plan item asks for it.
- **Only V8 (Node) makes vectors.** Never generate expected values from the C# side.

## 10. Verification per unit (the gate before each commit)

1. `bash Tools/unity-loop.sh compile`: `compilationFailed: false`, `consoleErrors: 0`.
2. The unit's own tests (its ported web tests and golden vectors), seen red before they turn green.
3. `bash Tools/unity-loop.sh tests` when the unit touches `Skygrind.Core`: 0 failed, every golden test, 16 / 16 replays.
4. The Editor slice smoke when the unit touches the run's flow.
5. **Tests verify the port; they never get edited to pass.** A mismatch is a translation bug until proven otherwise.

## 11. Parallel work (agents in worktrees)

- **One owner per file; a shared partial class gets one partial file and its own hooks per agent** (`Ui`: `BuildControls`,
  `RenderHotbar`, ...; `SurvivalGame`: the lead alone). The shared-types commit places every agent's hooks, buttons and
  fields first, so no two agents edit the same lines.
- **A new private member or nested type in a shared partial class or namespace carries its file's topic in its name**
  (`PauseKeyRow` in Ui.cs, `CtlRow` in UiControls.cs), never a generic one (`KeyRow`, `Row`, `Helper`): two agents'
  generic names compile alone in their worktrees and clash only at integration (CS0101 / CS0102).
- **The integration compile is the first gate after each cherry-pick batch**, before any test: a clash there is a
  rename, never a revert.

## 12. After a gameplay change: a farm diff (NIGHT14)

- **A change that can move how a run plays** (a gun, an enemy, a perk, a price, a director or HEAT number, a new
  system in `SurvivalSim.Step`) runs THE FARM on the new build with the same seeds and difficulties as the last report
  (`SKYGRIND.exe -skygrindFarm <runs> <difficulty> <seedStart> -batchmode -nographics -logFile ...`, at most 4
  processes, every farm process closed by PID before any bench, none within 10 minutes of a bench), then
  `node Tools/farm-report.cjs diff <last.jsonl> <new.jsonl>`. The diff goes in the night's log beside the gates.
- **The diff is a report, not a gate:** it says what moved (round reached, the survival curve, the economy, kills by gun,
  what downs you). GG owns enemy design and the numbers; a surprise is an observation for GG, never a silent re-tune.
- **The farm is the autopilot's** (BotBrain on the player's body, real HP, the meta silent): a run is a pure function of
  (seed, difficulty, build), so two farms on one build must match line for line; if they don't, determinism broke first
  (the fingerprint and `Tools/SimHarness twice` find where).

## Rule changes

| Date | Rule | Why (the mistake seen twice) |
|---|---|---|
| 2026-10-02 | first version | built from CLAUDE.md, NIGHT7-8's slips and PORT-RESEARCH.md B.4 |
| 2026-10-02 (NIGHT9) | §2 transcendentals: JsMath only (DeterminismTests), and the Math.pow rule | the System.Math gap seen in three newly consumed sets (hitDirection's atan2, springs' exp / atan, momentumMath's exp), each off by an ulp until swapped |
| 2026-10-02 (NIGHT9) | §2 grouping | flankPoint regrouped, and Math.sign / `\|\| 1` translated by C# idioms that differ on ±0 / NaN (springs, aim): the new vectors caught both |
| 2026-10-02 (NIGHT9) | §4 applies to a Map that deletes or re-inserts (TokenHolders, the arsenal's records and statuses: ordered lists) | token holders kept in a Dictionary lost the Map order after a steal (a tie picked the wrong loser), and the arsenal walks statuses in key order |
| 2026-10-03 (NIGHT10) | §6 no strings built per step or per frame | NIGHT8 U2-03: the HUD built its strings every frame; NIGHT10: `Loadout.Step` built `$"ads.weapons.{Weapon}"` every step (ZeroAllocTests: 3000 B over 600 steps), cached per weapon |
| 2026-10-03 (NIGHT11) | §6 no config lookups on the step or frame path | the Development Player's profile: `Loadout.StepAds` called `cfg.Has("ads.weapons.revolver")` every step (4 allocations a step with the KESTREL in hand; ZeroAllocTests `revolver 1200`), and `WallBuys.Render` built four `$"weapons.catalog.{id}..."` keys per gun per frame (36 allocations a frame); after NIGHT10's interpolated key, the second time |
| 2026-10-04 (NIGHT13) | §11 parallel work: topic-named members in shared partial classes; the integration compile first | NIGHT11: two agents defined the same helper class name in one namespace (a compile clash at integration); NIGHT13: agents F (Ui.cs) and S (UiControls.cs) both added `KeyRow` to the partial `Ui` (CS0102 at integration; renamed `PauseKeyRow`) |
| 2026-10-04 (NIGHT14) | §12 after a gameplay change: a farm diff against the last report | NIGHT14.md U3-27.12 (the farm is new tonight: a tool rule, not a mistake seen twice) |
| 2026-10-04 (NIGHT12) | §6 no List.Find / FindIndex with a capturing lambda on the step or frame path | `SurvivalMap.ZoneIndex`'s FindIndex ran every step through the power switch's buy check (Play check 2's profile: SKYGRIND.Buyables 125 B / 1.7 allocs a frame), and the first-run guide's gate lookup used List.Find every frame (caught in review before its commit) |
