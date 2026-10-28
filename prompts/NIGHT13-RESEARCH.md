# NIGHT13-RESEARCH.md: run better, play anywhere, and the architecture under it

GG, 2026-10-04, after NIGHT12 (verbatim): "What should next goal prompt focus on, Again we're building this on Unity only
and I needed to run better. also i keep running into " A hook blocked the turn from ending 9 consecutive times ..."
Also again let's do another detailed layer of research related to you know game development and similar games to what
I've made to look at the data what is worked wire users coming back to it what appeals to the gamers How can we
organize it better also controller support and justice all the requirements team requires for the game to get improved
let's also focus on that and just other iterations and then yeah let's continue to do umm less actual testing in the
game and justice more building out the architecture improving and building more obviously based off like what we think
is necessary not just building to build and follow best methods to prompt Opus 5.5 and yeah create the next detailed
goal prompt I want a lot done for this".

This file is the synthesis. The evidence is in six reports written the same afternoon (about 2,300 lines with their
sources and URLs), two read-only audits of our own code first and four web studies after:
- `Research/2026-10-04-audit-perf-architecture.md`: the bench logs and the code: why the worst 1% grew, how the code is
  organised, a ranked reorganisation with its proofs
- `Research/2026-10-04-audit-input.md`: what the web game's input stack has (gamepad, rebinding, hold / toggle, frame
  pacing, quality) and what Unity has
- `Research/2026-10-04-comparables.md`: about 30 games like SKYGRIND with review, player and price data
- `Research/2026-10-04-controller.md`: controller share on Steam, Steam Deck Verified, Steam Input vs the Input System,
  aim assist, stick settings, accessibility (a 33-line requirements checklist)
- `Research/2026-10-04-unity-perf-architecture.md`: diagnosing CPU tail latency in Unity 6, frame pacing, IL2CPP,
  quality tiers, code architecture for a code-built game with a deterministic core
- `Research/2026-10-04-production-and-prompting.md`: what a small team needs in place (playtests, crash reports, CI,
  Steam, planning docs; a 24-line checklist) and how to prompt Opus 5.5 and `/goal`

**Tags:** [V] verified in our code or logs; [W] web source fetched; [S] web snippet only; [U] inference. The URLs are in
the reports.

## 0. Bottom line

1. **"Run better" is a measurement problem first.** The worst 1% is the p99 of the uncapped frame interval
   (`PlayerChecks.cs:159-169`) [V]. It rose 5.55 -> 6.84 ms (Balanced) / 7.19 ms (Silent) from u3-n11 to u3-n12 while
   the median (~2.5 ms) and p95 (~4.2 ms) held, the GPU p99 stayed 1.5 ms, the sim step costs about 0.27 ms and runs in
   about 1 frame of 3 [V audit section 1]. So the tail is engine-side (render thread, Present, canvases), GC slices or
   the bench's own noise: the bench script starts a PowerShell + WMI poll 16 times a run, and identical runs spread 4.64
   to 7.00 ms [V]. Nothing per-frame was ever logged: the next build must log it before any fix is chosen.
2. **Performance is a review line in this genre.** KF3's 10-30 minute shader compiles and Deck "Unsupported", Rogue
   Core's "stutter city", Anger Foot's "brought my 5090 to its knees"; none of ULTRAKILL's 100 recent negatives mention
   performance [W comparables 0.7]. Shader warm-up and the bar are product work.
3. **Controller support is mostly a port.** The web plays fully on a pad, menus included: `gamepad.js` (dead zones,
   curve, button table, menu navigation), `bindings.js` (remapping), `holdToggle.js`, a GAMEPAD settings tab, pad aim
   assist, glyph hot-swap, `framePace.js`, `quality.js` (`docs/GAMEPAD.md`) [V]. Unity has keyboard and mouse only,
   with hard-coded keys (`InputReader.cs:50-71`, `CombatReader.cs:26-59`) [V].
4. **Controllers matter on Steam:** up to 15% of Steam sessions use one (2024); by pad type Xbox 59%, PlayStation 26%,
   Deck 10% [W controller 1]. Every movement shooter checked is Deck Verified; Sker Ritual, Roboquest and Risk of Rain 2
   lost Verified on unreadable text [W comparables 0.8].
5. **Feel and readability decide this genre's reviews.** The movement shooters sit at 89-98% positive; the Zombies-likes
   that failed, failed on basics: Sker Ritual's most-voted negative is forced mouse acceleration, then "no minimap";
   KF3 (44.0% of 23,861) shipped a battle pass before text chat [W comparables 0.1]. SKYGRIND's exact movement is the
   moat; raw mouse input and readable screens protect it.
6. **Long runs need a save.** "No save system... Lose hours of playtime" (Sker Ritual), "30 minute runs would be much
   better" (Roboquest) [W comparables 0.6]; BO6's save-and-quit is "a feature players have been asking for since 2008"
   [W NIGHT14-RESEARCH 4]. SKYGRIND's run to exfil is 20-30 minutes.
7. **The architecture is the next bottleneck.** Core is clean (391 test attributes, golden, replays); the Game side is a
   1,506-line `SurvivalGame` with 75 public members and a 4,130-line partial `Ui`, almost no Game tests, and **no test
   pins the composed `simulate()` order**: only the two smokes cover it [V audit 4]. A composed fingerprint (a seeded,
   scripted run hashed every 600 steps) makes every refactor provable without playing, which is GG's "less testing in
   the game, more architecture".
8. **Only content cadence grows a game** (ULTRAKILL's peaks rose with each layer; Gunfire Reborn holds on a season every
   ~4 months; one-release games faded to 20-200 players) [W comparables 0.2]. So the architecture work must make the
   next content nights cheaper (an input layer, a typed settings view, a sim composition, a run snapshot), not add
   features for their own sake.
9. **The `/goal` loop came from the goal's wording,** not a broken hook: NIGHT12's condition required "all of these",
   a bench bar and caps among them; when they slipped, the checker answered "not met" on every turn and Claude Code's
   cap ended each one; the goal stayed active for 18 hours [V transcript]. The fix is in section 7.

## 1. Our data: the two audits

### 1.1 Performance (Research/2026-10-04-audit-perf-architecture.md sections 1-3)
- **The bench** (`Tools/survival-bench.sh`, `-skygrindBench 60`): seed 99, gates open, god mode, director.maxAlive
  husks; the player never moves or shoots and is hurt every frame, so it measures a UI-hurt loop, not combat [V].
- **Ranked suspects (estimates, not measured):** the render thread / Present ~35% (NIGHT12's x-ray second material per
  husk body `EnemyViews.cs:167`, per-part property blocks `:175-182` that make renderers SRP-Batcher-incompatible,
  corpse / motes / blob / ring draws, 3-5 Forward+ lights, seven Volume overrides written per frame `PostFx.cs:105,125`,
  two cameras, full-res SSAO, 4 soft cascades, depth + opaque textures; Present may cross from the RTX to the Radeon
  iGPU); the bench environment ~25%; incremental GC at a fixed 3 ms slice ~20% (uncapped and vsync 0 means Unity gives
  each frame up to `incrementalTimeSliceNanoseconds`; the release Player allocates about 1 MB/s, 3 collections a
  minute); NIGHT12's scripts and the HUD <=10% (`LightPools.cs:292-316` disables and re-enables every steady light every
  frame); the fixed loop's step frames <=10%; Mono <=10% [V file:line in the audit].
- **Cheap safe wins:** bench hygiene (poll once before and after); a per-frame breakdown (FrameTiming main / render /
  present-wait, steps run, GC count; median band vs worst-1% means; `enableFrameTimingStats` is on, so it works in
  Release); a playing bench scenario (`forcedMove` / `forcedCombat`); a 1 ms GC slice from UnityTuning; LightPools'
  final state applied once; Opaque Texture off (no shader samples it); the overlay camera without shadows; the award
  feed's string cached; an IL2CPP trial behind the Player golden gate [V audit 3].
- **The measured-first order** (Research/unity-perf section 7): define the metric -> per-frame CSV -> Dev profile ->
  view A/Bs -> pacing A/Bs (uncapped / 240 / 144; incremental GC on / off once at 0 B/frame; maxQueuedFrames) ->
  Windows QoS A/B -> IL2CPP gated -> AUTO tiers. Each run a median of three, plan polled [W/U].
- **IL2CPP:** usually 1.5-3x on script work, but script is ~0.6 ms of the frame [V]; the floating-point risk (MSVC
  contraction, CRT maths) is unverified, so it ships only at `-skygrindGolden` 0 mismatches, 22 / 22 replays and an
  identical fingerprint [W/U].
- **Frame pacing changes the metric:** capped, the interval always reads the cap, so the bar becomes the p99 CPU busy
  time (FrameTiming `cpuMainThreadFrameTime`) [W unity-perf 0.4]. Which policy ships is D-44.

### 1.2 Architecture (audit sections 4-5)
- 160 .cs files, 46,816 lines: Core 30,094 (95 files), Game 15,725 (62), Bench 997; tests 10,665 [V].
- `SurvivalGame.cs` by concern: fields 23-285, boot 288-322, **Init (the composition root) 324-605**, flow 704-967,
  **the loop 971-1077**, the guide 1079-1148, **SimStep in simulate() order 1163-1251**, run shape, hot keys, the HUD
  line's text (UI glue) 1416-1474 [V].
- Systems find each other through constructors and ~40 Func / Action closures wired in Init; 25 Game files take
  SurvivalGame and read its public fields; statics `SurvivalGame.Instance`, `EnemyLook.shared`, `Look.Post` [V].
- **The EventBus runs handlers in registration order** (`EventBus.cs:27-31, :51`): handler order is behaviour [V].
- **A render -> sim dependency kept from the web:** the brain's watchdog asks `OnScreen`, which reads the camera's
  viewport (`SurvivalGame.cs:272-276, :378`), so a composed run is not a pure function of its inputs [V].
- Settings are a `Dictionary<string, object>` read by string at call sites, per frame and per hurt [V].
- Duplication: the weapon family colour lookup 6 times, the "KeyX -> X" label 3 times; dead: `BeforeAi` /
  `AfterPlayerSystems` (no subscriber, invoked every step), `IdleSpawner`, `bool click` [V].
- **The proposed order** (each with its proof): (1) a composed fingerprint (risk ~0); (2) SurvivalGame split into
  partial files (same type, same statement order); (3) de-duplication and dead code; (4) a typed settings view; (5) an
  input layer (`IPlayerInput`: keyboard-mouse, scripted, gamepad); (6) the sim composition extracted into a plain C#
  `SurvivalSim` (keep every sim-affecting EventBus registration in today's order; `OnScreen` injected); (7) a run
  snapshot (`IRunState` on each stateful Core system, a `State` accessor on Mulberry32: 29 creation sites in 17 files
  outside Golden); (8) Ui split by screen, last [V audit 5].

### 1.3 Input (Research/2026-10-04-audit-input.md)
- **Unity is keyboard and mouse only, every key hard-coded** (`InputReader.cs:50-54`, `CombatReader.cs:26-37`); the
  only pad read is RESULTS' tally skip (`UiResults.cs:368`); no rebinding, hold / toggle, CONTROLS or pad navigation [V].
- **The data is already exported:** `config.json` has the `gamepad` block, all 41 web settings (the 8 pad keys,
  slideMode / arclineMode, fpsCap, quality), the 18 bindings with labels, the CONTROLS rows and the quality presets; the
  settings store already saves the pad keys without showing or applying them. Missing: only the web's module constants
  (the pad layout, glyphs, the 0.35 trigger threshold, the unbindable keys, key names) [V].
- **The pure modules port to Core cleanly:** `gamepad.js` (~320 lines C#), `holdToggle.js` (~70), the pure half of
  `bindings.js` (~170), `qualityMath.js` + `quality.js` (~220), `menus.js`' focus helpers (~60): 47 web test cases to
  port. About 2,300 lines in all (Core ~650, tests ~600, Game ~550, UI ~550): PORT-PLAN's N19 estimate [V].
- **"No pad = the keyboard's frame, bit for bit" holds by defaults:** the golden checks and replays never pass through
  the readers (`PortData.cs:249, :295`); MergePad returns early without a pad, HOLD is a no-op, and the pad's breakaway
  cap defaults to +infinity. The one change to existing Core is that cap (`AimAssist.cs:567`): golden + 22 replays at
  that commit [V].
- **`framePace.js` is superseded:** Unity's `targetFrameRate` already does its job, wired to the FPS CAP row
  (`SurvivalGame.cs:662-666`, `UiSettings.cs:19`). **Quality is half done:** bloom, SMAA, grain, lights and motes follow
  the preset; AUTO, dynamic resolution, render scale and per-preset shadows are not ported [V].
- **Gotchas:** (1) the Input System's default stick processor already dead-zones 0.125-0.925: read
  `ReadUnprocessedValue()` and negate y; (2) `SurvivalGame.KeyOf("ShiftLeft")` is `Key.None` (Unity says `LeftShift`):
  a web-code <-> Unity Key table comes before any rebinding, or dash silently unbinds; (3) input is polled only in
  Playing / Down (`SurvivalGame.cs:977`), so pad menus get nothing; uGUI buttons have default navigation and no
  visible selected state; (4) a pad button is "down" at the web's 0.35, not the Input System's 0.5 press point; (5)
  `SetMotorSpeeds` persists until reset (stop on pause, focus loss, quit); (6) the Switch Pro pad swaps confirm / back
  under uGUI's defaults; the PlayStation shapes are not in the static TMP atlases (use words or sprites); (7) runtime
  quality changes go on an `Instantiate`d copy of `PC_RPAsset`, or Play mode rewrites the tracked asset; (8) AUTO and
  dynamic resolution change what the bench measures: pin HIGH under `-skygrindBench` and log the preset [V].
- **Automation:** a virtual pad (`InputSystem.AddDevice<Gamepad>()` + `InputState.Change`, no new package) drives the
  pad path in the smoke; a full run on a real pad needs GG [V].

### 1.4 NIGHT12's first-impression findings (the earlier session's three read-only audits, 2026-10-04)
Seed findings S1-S18, re-checked at HEAD by the night before each fix (NIGHT13.md section 1 lists them): the lock mark is
U+25A1, which the theme fonts lack and which is also TMP's missing-glyph mark (S1); label tracking on small payoff lines
(S2); the locked gate's field paints an orange striped wall 16 m ahead of spawn (S3); the round tally's strokes in
danger red-orange (S4); points that read as ammo (S5); FRESH / STALE never explained (S6); CONTRACTS shows dormant rows
and no rewards (S7); zone signs behind RESULTS (S8); a long lesson card arriving with the first enemies (S9); TITLE's
HEAT / DAILY unexplained and AIM ASSIST on the main menu (S10); an empty RESULTS half (S11); no RESTART in pause (S12);
debug keys live in Release (S13); 22 of the web's 41 settings shown (S14); per-frame UI work (S15).

## 2. What the market says (Research/2026-10-04-comparables.md, ranked)

| # | Finding | Evidence | For SKYGRIND |
|---|---|---|---|
| 1 | Protect feel and readability: raw mouse input, honest hits, guidance, readable screens | Sker's top negatives, KF3's, ULTRAKILL's praise [W] | raw input in the input layer; first-impression fixes |
| 2 | No hitches: shader warm-up, the bar, a low-spec check | KF3, Anger Foot, Rogue Core [W] | the perf gate; warm-up at boot |
| 3 | One-press PLAY, a one-screen run setup, at most 3 meta screens, plain words | RoR2's lobby vs KF3's terminal hub, CoD's "shopping mall" [W] | TITLE and the run setup (NIGHT12 built them; tidy, don't add) |
| 4 | Plan the Next Fest backwards: one fest per game; demos up a month early earned 2.5x wishlists; momentum in the last two weeks predicts best (rho 0.81) | Steam, Zukowski [W] | D-06: June 2027 unless a public page and demo are up by mid-December 2026 |
| 5 | Demo: one map, uncapped runs, HEAT 1 and the DAILY, other modes shown locked, wishlist prompts | Sker's demo, ULTRAKILL's held-back Cyber Grind [W] | D-38 |
| 6 | Steam Deck Verified: glyphs, defaults, legible text | all movement shooters checked are Verified [W] | the input night; D-41 text floor |
| 7 | EA at $14.99-19.99, no MTX; raise at 1.0 | the median 2025-26 hit $16.99; $40 horde shooters faded [W] | D-42 |
| 8 | A dated public EA cadence, a map / mode / rungs every 8-12 weeks; never announce a sequel in EA | ULTRAKILL, Gunfire, Deadzone's backlash [W] | the plan, section 8 |
| 9 | Every laster has a difficulty ladder | ULTRAKILL, Gunfire, Roboquest, DRG: Survivor, Witchfire [W] | HEAT 1-3 now; 4-6 with the roster |
| 10 | Short sessions; save-and-quit | Roboquest, KF3, Sker [W] | U3-26 save-and-quit |
| 11 | Variety, not stat sticks | Rogue Core, Deadzone, DRG: Survivor complaints [W] | CONTRACTS / MASTERY already so |
| 12 | Co-op later, for 4, solo tuned on its own; co-op demos convert worst | Roboquest's 2-player cap, Mycopunk [W] | D-02 after the demo |
| 13 | Free modes revive shooters (Boltgun's Horde +57%, Echo Point Nova's Rogue) | [W] | Survival is the core; endless / boss rush later |
| 14 | No AI art; a visible roadmap | Gunfire, Sker, Deadzone, BO7 reviews [W] | GG's art direction |

## 3. Controller, Steam Deck and accessibility (Research/2026-10-04-controller.md)

**For NIGHT13 (the port plus what Unity needs):**
1. Mouse and stick look summed in the same frame; prompts swap by the last device, look never gated on it [W Deck].
2. Input System for gameplay; Steamworks later only for pad identity and glyphs (Steam Input shows the game an
   emulated Xbox pad) [W]. No new package tonight (D-30).
3. Latch button edges per frame and consume them in the first 120 Hz step; never read a press per step [W/U].
4. One dead-zone stage: read the stick unprocessed (Unity's default stick processor applies 0.125 / 0.925) so the web's
   scaled radial 0.15 / 0.95 stays exact [W/U].
5. Glyph sets by position: neutral (the web's), Xbox, PlayStation (26% of pad sessions) [W].
6. Per-axis sensitivity, invert X / Y per stick, inner / outer dead zone, LINEAR / PRECISE, vibration on / off and
   strength, paused in menus [W XAG 107, the web's GAMEPAD tab].
7. Every action remappable on both devices; the web swaps two pad buttons so nothing is unbound; XAG asks for
   assigning, not only swapping (D-45) [W].
8. Hold / toggle for every held action (the web: slide, the ARCLINE) [V web].
9. Pad weapon selection without a wheel (Y next, d-pad previous / last: the web's layout), plus the radial [V web].
10. Shake, bob, chroma pulses each with their own setting under REDUCED MOTION [W XAG 117].

**Later (N17 ship-shape or before the demo):** Steam Input glyphs and `GetInputTypeForHandle`; Deck defaults through
`GetSteamHardwareDefaultConfig` (SDK 1.65 deprecated `IsSteamRunningOnSteamDeck`); gyro through Steam Input; the
on-screen keyboard; a 30 / 40 / 60 cap set for Deck; the 18 px text floor (D-41); colour-independent cues (XAG 103);
assist modes reshaped the BO6 / BO7 way (weaker at point blank, full pull only while the stick moves toward the target:
D-43, GG's feel numbers).

## 4. Performance plan for NIGHT13 (measured first)

1. Metric: keep the p99 of the uncapped interval as the bar's number (it is what D-34 measured), and add p99.9, max,
   the count over 6.94 ms, and the FrameTiming main / render / present-wait p99s.
2. One Release build logs per frame: interval, steps run, FrameTiming columns, GC count; prints the worst-1% means per
   column against the median band (and a CSV). Bench hygiene first (one plan poll before and after).
3. A playing scenario beside the UI-hurt one (`-skygrindBench 60 play`).
4. Fixes behind runtime switches (`-skygrindPerfOff <names>`), so one build A/Bs every arm without a rebuild:
   LightPools' final state once, Volume Via Scripting + writes on change, property blocks only on change and the x-ray
   pass only while X-RAY is on, the overlay camera without shadows, Opaque Texture off, HUD texts on change, a 1 ms GC
   slice, the award feed's string.
5. The pacing arms (uncapped / 240 / 144 through the existing FPS CAP: `targetFrameRate` already replaces
   `framePace.js`), the Windows HighQoS arm, then the rest of quality.js (AUTO, render scale, per-preset shadows, on a
   runtime copy of the URP asset), with AUTO and dynamic resolution pinned off under the bench.
6. Shader warm-up at boot (Unity 6: `GraphicsStateCollection` on DX12, experimental; a ShaderVariantCollection
   fallback): it moves the max and the 0.1%, which is what players see in the first minute.
7. IL2CPP: one trial build gated by golden, replays and the fingerprint; it ships only on a gain above the noise
   (D-40, data-decided, reversible).

## 5. Architecture plan for NIGHT13 (audit section 5, in order)

1. Composed fingerprint (Game-side; seeded, scripted inputs, `OnScreen` pinned, a hash every 600 steps), recorded on
   the unchanged u3-n12 code. Every later move prints the same hashes.
2. `SurvivalGame` split into partial files (Init, Flow, Meta, Render, Debug); the HUD line's text into Ui.
3. De-duplication (one `WeaponLook.FamilyColor`, one binding label) and dead code out.
4. The input layer (`IPlayerInput`), the typed `SettingsView`, settings in a JSON file beside the profile (Steam
   Auto-Cloud syncs files; PlayerPrefs lives in the registry on Windows).
5. `SurvivalSim` extracted (owns the Core systems, the wiring and SimStep in simulate() order; SurvivalGame keeps boot,
   the accumulator, hitstop, interpolation and the views, with forwarding properties so no view changes); then the
   fingerprint runs headless in EditMode, inside the suite.
6. The run snapshot (`IRunState`, Mulberry32 `State`), proven by a Core test: run to step N, snapshot, load into a fresh
   composition, run to M, equal hashes to the uninterrupted run; several seeds (a break, a fight, an exfil). Then
   save-and-quit with BO6's rules on top.
Skipped on purpose: DI containers and ScriptableObject event channels (they hide order, which is behaviour here),
Burst for now (Strict mode only, never `FloatMode.Deterministic`: it can't match V8), the Ui split (no perf or
determinism gain) [W unity-perf 6].

## 6. What a small team needs in place (Research/2026-10-04-production-and-prompting.md)

| When | What | Why |
|---|---|---|
| **now (NIGHT13)** | a version stamp (version + commit) on TITLE and in the log; a local per-run log; a COPY REPORT button; debug keys only with `-skygrindDev` | testers attach evidence; no privacy surface |
| now (GG) | D-06 the fest; Steam Direct ($100) and onboarding; a one-page PILLARS / GDD; a decision log with status (HANDOFF's D-xx) and a risk list | the store page is the critical path; a scope filter for night prompts |
| before outside saves | `saveVersion` and a migration test (save-and-quit ships with version 1) | outside saves must survive updates |
| before the first outside tester | Sentry (free: 5k errors / month, Windows native crashes via crashpad, PII off) or BugSplat; a privacy policy page | crashes on other PCs; telemetry is personal data under GDPR |
| before the demo | playtests in rounds of 5 (Nielsen: 5 users find ~85% of usability problems); Steam Playtest; CI v1 (EditMode tests on push, GameCI); a beta branch; the public store page (capsules, 5+ shots, a trailer, tags); the content survey (AI section: content players see, not AI-written code) | wishlists, momentum, safe uploads |
| before EA | Steamworks.NET achievements and Auto-Cloud (or Unity's Platform Toolkit Steam package 1.0.2: coverage not verified); controller + Deck review; patch-note events; CI v2 (a Windows build + golden) | the platform bar |

## 7. The `/goal` fix and prompting Opus 5.5 (production-and-prompting Part 2)

- `/goal` is a session-scoped prompt-based Stop hook: after each turn a small fast model reads the condition and the
  transcript (it runs nothing, reads no files) and says not met / met / impossible. Claude Code overrides a Stop hook
  that blocks about 8 times in a row without progress (`CLAUDE_CODE_STOP_HOOK_BLOCK_CAP` raises it; raising it only
  hides a bad condition) [W].
- **The rule now:** the condition ends on what the transcript shows: the night's close table printed with every item
  DONE (a commit hash or an output line) or SHORT (a cap, a decision reserved for GG, a blocker after two approaches,
  or the time limit), then "NIGHT13 CLOSED"; or a `date` 12 hours after the first. Caps, bars and one-shot rules stay
  in NIGHT13.md as rules. A fixed status line ends every message so the evaluator can read progress. `/goal clear`
  after the night (or a fresh session) before asking for anything else.
- **Opus 5.5:** name the early stops you don't want (the NIGHT8 paragraph; Anthropic suggests it in the system prompt
  from the first turn); no "double-check" or "verify with a subagent" lines (over-verification); delegate only
  independent work; calm wording with reasons; elapsed time from `date` [W].
- **Worktrees:** subagent worktrees may branch from the default branch (`main`) unless `worktree.baseRef` is `"head"`
  [W, not checked against NIGHT12, whose agents started from the right commit]: NIGHT13 checks each agent's base
  before it builds.

## 8. The plan: the next nights (amended)

| Night | Theme | Why now |
|---|---|---|
| **N13** | **Run better, play anywhere:** the perf gate (measured first, switches, pacing, warm-up, quality AUTO, an IL2CPP trial), the composed fingerprint and the sim composition, the input layer with the gamepad, rebinding and hold / toggle, the settings reviewers check (typed, in a JSON file), the hotbar and the radial, save-and-quit on a run snapshot, first-impression fixes, a version stamp and a local log | GG's request; the bar is missed; controller support is a port; every later night is cheaper on this base |
| N14 | The POWER guns and THE FORGE tiers (`NIGHT14.md`, `NIGHT14-RESEARCH.md`) | variety for the box and the wall |
| N15 | The enemy roster + the round budget; HEAT rungs 4-6 by composition | content; the ladder |
| N16 | A boss + the secrets ladder (+ gibs if GG says yes) | the run's spine |
| N17 | Ship-shape: Steamworks (achievements = contracts, Auto-Cloud, leaderboards for the DAILY, Steam Input glyphs), the demo shell (D-38), Deck settings, Sentry before outside testers | the demo's platform bar |
| EA | co-op (2-4), map 2, a free update every 8-12 weeks | the cadence that grows a game |

**Dates (D-06):** February 2027 Next Fest runs Feb 22 - Mar 1 (register by Jan 10, demo and page by Jan 25); June 2027
registers by Apr 25. A public store page this autumn is the gate for February [W]. Recommendation: June 2027, unless the
page and a demo can be public by mid-December 2026.

## 9. Decisions for GG (proposals; nothing here changes code without him)

| ID | Question | Recommendation | Default if no answer |
|---|---|---|---|
| D-34 | The bench bar: plan and metric | keep 6.9 ms p99 uncapped on Silent (the memory note); report the FrameTiming breakdown beside it | Silent |
| D-06 | Next Fest Feb or June 2027 (due Nov 1) | June, unless a public page + demo by mid-December 2026 | none |
| D-38 | The demo's content cap | one map, uncapped runs, HEAT 1 + the DAILY, other modes shown locked, wishlist prompts | none |
| D-39 | Which bench scenario the bar applies to (today's UI-hurt loop never moves or shoots) | report both from N13; the bar on the UI-hurt one until GG says | UI-hurt |
| D-40 | IL2CPP as the Windows backend | data-decided in N13: ship only at 0 mismatches and a gain above the noise; reversible | Mono |
| D-41 | UI text floor 16 -> 18 px at 1080p (Xbox XAG; Deck's 12 px recommendation) | yes, with a UI SCALE setting | 16 px |
| D-42 | EA price | $14.99-19.99 | none |
| D-43 | Aim assist reshaped the BO6 / BO7 way (weaker at point blank; pull only while the stick moves toward the target) | after a pad playtest; the web's numbers ship first | the web's |
| D-44 | Frame pacing default (uncapped / cap at the display rate / 240) | data-decided in N13 from the pacing arms; the bar's metric follows | uncapped |
| D-45 | Remap: assign any action (XAG) vs the web's swap | port the web's swap tonight; assign later | swap |
| D-46 | Sentry before the first outside tester (free tier, PII off) | yes, at N17 | none |
| D-47 | A one-page PILLARS / GDD drafted from the docs for GG to edit | yes, drafted at the N13 close, marked DRAFT | none |
