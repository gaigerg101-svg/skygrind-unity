# NIGHT13.md: SKYGRIND in Unity, stage 3 night 5 (run better, play anywhere, and the architecture under it), no human in the loop, at most 12 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP 17.3).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **this file:** tonight's job, its caps and the close table
- **`NIGHT13-RESEARCH.md`:** why tonight looks like this; it cites the six reports in `Research/2026-10-04-*.md` (read
  the sections an item names, not all 2,300 lines; the two audits first: `audit-perf-architecture.md`, `audit-input.md`)
- **`HANDOFF.md`:** where things stand (anchor `u3-n12`; its update note first)
- **`PORT-RULES.md`** (section 6: no config lookups, strings or capturing lambdas per step or frame) and **`tasks.json`**
  (tonight: U3-26 and U3-26.1 .. U3-26.13)
- **`NIGHT14-RESEARCH.md`** sections 2 (the hotbar and the radial) and 4 (save-and-quit: the state list, the ten streams)

**Run length: 12 hours from your first `date`. The close starts by hour 10:30 whatever is left.** Stop earlier only when
every item is DONE.

GG, verbatim (2026-10-04): "Again we're building this on Unity only and I needed to run better ... controller support
and justice all the requirements team requires for the game to get improved ... let's continue to do umm less actual
testing in the game and justice more building out the architecture improving and building more obviously based off like
what we think is necessary not just building to build ... I want a lot done for this". Standing: "want this game to be
like cod zombies meets ultrakill as the main, but other games should be inspired" (2026-10-03).

**Why tonight looks like this (NIGHT13-RESEARCH.md section 0):**
- The bench missed its bar (Silent median 7.19 ms against 6.9) and nobody knows why: the tail is engine-side, GC or the
  bench's own noise, and nothing per-frame was ever logged. Players review performance; measure first, then fix.
- Controller support is a port: the web plays fully on a pad, menus included, and `config.json` already carries every
  number. Up to 15% of Steam sessions use a pad; every movement shooter checked is Steam Deck Verified.
- The Game side is one 1,506-line `SurvivalGame` and no test pins the composed `simulate()` order. A composed
  fingerprint makes every refactor provable without playing: GG's "less testing in the game, more architecture".
- Save-and-quit needs a run snapshot, which needs that composition. Players punish long runs with no save.
- The POWER guns and THE FORGE tiers move to NIGHT14 (`NIGHT14.md`): tonight builds the base they land on.

**Goal (the morning):** GG plugs in a pad and plays SKYGRIND from TITLE to RESULTS without touching the keyboard, with
prompts in his pad's glyphs; he rebinds a key in CONTROLS and it sticks; he quits at a round break and RESUME puts him
back; the game runs at or under the bar, or the report names exactly which frame costs keep it over. Underneath:
`SurvivalGame` is split by concern, the sim composition runs headless in a test, and a fingerprint proves no refactor
changed a run. With every new flag off, one player plays exactly the web's game.

## Facts already measured (2026-10-04, read at `u3-n12` = `07f7885`; re-check them, don't trust them blindly)

- **Baseline:** `tests: 501 / 501 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 91 / 91`; harness 34 sets, 22
  replays, 0 mismatches; the Player smoke `[smoke] 107.1s DONE: all steps passed`; `-skygrindGolden` 0 mismatches.
- **The bench's "worst 1%" is the p99 of the uncapped interval** (`PlayerChecks.cs:157-169`: sorted, index
  `floor(0.99 n)`), vsync 0, cap -1, ~21,600 frames a run. FrameTiming's CPU p99 equals it (7.15 vs 7.13 ms in
  `bench8.log`); GPU p99 1.5 ms. Medians ~2.5 ms, p95 ~4.2 ms at both u3-n11 and u3-n12; only the p99 grew (5.55 ->
  6.84 Balanced / 7.19 Silent, medians of three). The sim step costs ~0.27 ms (`bench-dev.log`: 1887.3 ms of
  `SKYGRIND.SimStep` in 60 s) and runs in ~1 frame of 3 [audit-perf 0-1].
- **The bench measures a UI-hurt loop:** the god-mode player never moves or shoots and is hurt every frame
  (`PlayerChecks.cs:90-154`); `Tools/survival-bench.sh:14-16` runs a PowerShell + WMI poll 16 times per run.
- **Perf suspects with file:line** (audit-perf section 2): x-ray second material per husk body (`EnemyViews.cs:167`) and
  per-part property blocks (`:175-182`); seven Volume overrides per frame (`PostFx.cs:105, :125`), the URP asset's
  Volume update mode every frame (`PC_RPAsset.asset:94`); LightPools disabling and re-enabling every steady light every
  frame (`LightPools.cs:292-316`); the overlay camera's defaults (`Viewmodel.cs:187-194`); Opaque Texture on with no
  shader sampling it (`PC_RPAsset.asset:23`); incremental GC (`ProjectSettings.asset:848`) with fixed 3 ms slices while
  uncapped; Mono (`scriptingBackend` lists Android only).
- **Architecture** (audit-perf section 4): `SurvivalGame.cs` concerns by line range (Init 324-605, flow 704-967, loop
  971-1077, SimStep 1163-1251); 18 handlers registered by SurvivalGame itself; **EventBus runs handlers in registration
  order** (`EventBus.cs:27-31`); the brain's watchdog reads the camera through `OnScreen` (`SurvivalGame.cs:272-276,
  :378`); `forcedMove` / `forcedCombat` (`:103-105`) already script input; dead: `BeforeAi` / `AfterPlayerSystems`,
  `IdleSpawner`, `bool click`; the weapon family colour lookup written 6 times.
- **Input** (audit-input sections 0, 4-6): keys hard-coded (`InputReader.cs:50-54`, `CombatReader.cs:26-37`); input polled
  only in Playing / Down (`SurvivalGame.cs:977`); `KeyOf("ShiftLeft")` returns `Key.None` (`:672-676`); the Input System's
  stick processor applies 0.125-0.925 (read `ReadUnprocessedValue()`, negate y); a pad button is down at the web's 0.35;
  `config.json` already has `gamepad`, the 41 settings, the 18 bindings, the CONTROLS rows and the quality presets; the
  web's module constants (pad layout, glyphs, `TRIGGER_DOWN`, `UNBINDABLE`, key names) are not exported; the replays and
  golden checks never pass through the readers (`PortData.cs:249, :295`); the one Core change is the pad's breakaway cap
  (`AimAssist.cs:567`, default +infinity); 47 web test cases cover the pure modules. `framePace.js` is superseded by
  `targetFrameRate` (`SurvivalGame.cs:662-666`).
- **Save-and-quit's state list and the ten Mulberry32 streams:** NIGHT14-RESEARCH.md section 4; Mulberry32's state is a
  private `uint a` (`CoreGrid.cs:12-30`); 29 `new Mulberry32(` sites in 17 Core files outside Golden [audit-perf 5.7].
- **The previous session's first-impression findings S1-S13** (re-check each at HEAD before its fix; some may be stale):
  S1 the lock mark U+25A1 (`UiMeta.cs:223,232,259,264`; `UiPayoff.cs:141`; `UiContracts.cs:115`) is missing from the
  theme fonts and is also TMP's missing-glyph mark; S2 `UiPayoff.cs:162` tests `size == tLabel` while tSmall == tLabel
  == 16; S3 the locked-gate field (`Field.shader:57-59`; material `SurvivalGame.cs:644-650`) paints an orange striped wall
  16 m ahead of spawn; S4 the round tally strokes in danger red-orange (`UiCeremony.cs:38-50, 144-176`); S5 points as an
  unlabelled gold number under the weapon name (`Ui.cs:289-290`); S6 FRESH / STALE never explained (`UiFeel.cs:110-127`);
  S7 CONTRACTS shows dormant rows and 28 of 39 rows no reward (`Contracts.cs:62-75`, `UiContracts.cs:108-117`); S8 zone
  signs render behind RESULTS and menus (`SurvivalGame.cs:1065`); S9 the lesson card 115-192 characters for 9 s at
  roundBegin (`UiOnboarding.cs:72-83`); S10 HEAT / DAILY unexplained, AIM ASSIST a main-menu button (`Ui.cs:344`); S12
  no RESTART in pause (`Ui.cs:349-354`); S13 F3 / F8 / F9 live in Release (`SurvivalGame.cs:1386-1395`).
- **The machine:** GG's scheduled jobs start at 1:00 AM (patric2 `nightly.py`, `refresh.py`, `run_batch.py`, a local
  `llama-server` at ~9 cores) and ran until ~04:16 last night: bench before 00:45 or after their CPU reads 0. The power
  plan flips by itself: poll it with every series.

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch `port/stage3`.** Tag `u3-n13` once, last, on the close commit.
2. **Measure, then build; build what the evidence names.** Every item below cites its reason. A new idea goes to
   HANDOFF's pending decisions or NIGHT14.md, never into code tonight.
3. **Less testing in the game, more proof by machine.** The gates are the compile, the suite (tests, golden, replays),
   the composed fingerprint and the Player smoke; Play checks and captures are few (section 4). Don't re-verify what a
   gate already showed, and don't add "double-check" passes: one focused test per new piece of logic, seen red once.
4. **Port where the web has it** (gamepad.js, holdToggle.js, bindings.js, menus.js' focus helpers, quality.js /
   qualityMath.js: their web tests are the oracle; a vector set only if a test can't pin a double). **Design where it
   doesn't** (the fingerprint, SurvivalSim, the run snapshot, save-and-quit, glyph sets, the perf switches) behind a
   flag or a UnityTuning constant with a comment. Unity-only render / UI numbers in `UnityTuning*.cs`; new gameplay
   numbers in `Design/unity-config.json` with a `_why`; the web's module constants as cited C# constants.
5. **Refactors don't change behaviour.** Each refactor commit prints the same fingerprint hashes as the baseline, the
   suite stays green, and EventBus registration order stays exactly as it is (it is behaviour). Core commits: every
   golden test and 22 / 22 replays (CLAUDE.md rule 3). Mover.cs is not touched.
6. **No new packages** (D-30): the Input System 1.18 already in the manifest covers the pad; Steamworks is N17.
7. **The bench:** a series is three back-to-back 60 s runs, decided by the median, the plan polled, every run reported,
   on an idle machine. The bar is D-34's (6.9 ms p99 on Silent). Fixes that change nothing visible are kept when they
   beat the noise; anything that changes the look or a player-facing default (a frame cap, SSAO off on HIGH) is
   reported for GG. The scripting backend is the one exception, decided by U3-26.4's gates (D-40, reversible).
8. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. A status line in
   `NIGHT13-LOG.md` at the end of every item, with times from `date`.
9. **End every message with one status line** for the `/goal` checker:
   `N13-STATUS: <d> done, <s> short, <l> left (of 13) · tests <passed>/<total> · <time from date>`.
10. **Agents:** background builders split by file ownership (section 3; memory `night-parallel-agents`), each in its own
    git worktree from the shared-types commit, compiled with `dotnet build` on the csproj copies; you alone drive the
    Editor, run the Players, cherry-pick and commit. Evals idempotent (memory `play-check-eval-recipe`).

## 1. Start (U3-26.1, 15 min)

1. Read the files at the top, CLAUDE.md first. Load `unity:unity-cli` before driving the Editor, `unity:ui` before UI.
2. HANDOFF section 1, rows 1-5 and 8-9; `RECEIVED` in `NIGHT13-LOG.md`. HEAD is `u3-n12`; the working tree holds this
   session's research (`NIGHT13-RESEARCH.md`, `NIGHT14-RESEARCH.md` renamed from NIGHT13-RESEARCH.md, six
   `Research/2026-10-04-*.md`, `HANDOFF.md`'s update note, `tasks.json`'s U3-26): commit them first, as one docs commit
   with explicit paths.
3. HANDOFF section 5: GG's answers (D-06, D-34, D-38 .. D-47); log them and apply any that change tonight.

## 2. The items

Each item: what, the build, the check that proves it, done. Owners in section 3; order and boxes in section 5.

**U3-26.2 Measure first (lead, 60 min).** The bar can't be fixed until the spike frames are named.
- `Tools/survival-bench.sh`: poll the plan and AC once before and once after the run, print the poll times, no poll
  during it.
- `-skygrindBench`: per frame, the interval, FrameTiming main / render / present-wait / GPU, the steps run that frame and
  `GC.CollectionCount(0)`; print p99, p99.9, max, the count over 6.94 ms, and each column's mean over the worst-1%
  frames against the median band; write a CSV to `Logs/player/`. Measurement only.
- `-skygrindBench 60 play`: a playing scenario (strafe and fire through `forcedMove` / `forcedCombat`) beside the
  UI-hurt one.
- **The composed fingerprint:** `SurvivalGame.Fingerprint.cs`, a read-only `StateHash()` over the Core state (mover,
  health, economy, run, director, husks, brain tokens, style, loadout, the streams' draw counts) and `-skygrindFingerprint`:
  a seeded run on scripted inputs with `OnScreen` pinned, many steps per frame (seconds, not minutes), a hash every 600
  steps for 7,200 steps. Seen red: another seed prints other hashes.
- Build 1 (Release, Mono): the Player smoke DONE; `-skygrindFingerprint` twice, identical (commit the hashes as the
  baseline the later test reads); a Silent series of the UI-hurt bench and one `play` run. Log the breakdown table.
- **Done:** the breakdown names where the worst-1% frames spend their time (main, render, present, GPU, GC, step).

**U3-26.3 Shared types (lead, 35 min).** `Tools/wt-csproj.cjs` (the worktree csproj script, NIGHT12-LOG "Shared
types"); `IPlayerInput` + the device / radial state types; `IRunState` + StateWriter / StateReader (exact double
round-trip, as JsJson); Ui's partial hooks for CONTROLS, the hotbar, the radial, navigation and COPY REPORT; one
EventSystem with `InputSystemUIInputModule`; empty `UnityTuning.*.cs` partials per agent; the `design.save` block with
its `_why`. Gate: compile + suite. Then launch the agents (section 3).

**U3-26.4 The perf gate (agent P + lead, 75 min).** NIGHT13-RESEARCH section 4.
- Each fix behind a runtime switch (`-skygrindPerfOff <names>`, logged at boot) so one build A/Bs every arm: LightPools
  applies each slot's final state once; Volume update Via Scripting with writes on change; property blocks only on
  change and the x-ray pass attached only while X-RAY is on; the overlay camera without shadows; Opaque Texture off
  (an `Instantiate`d runtime copy of `PC_RPAsset`, never the tracked file); HUD texts on change only; the award feed's
  string cached; a 1 ms incremental-GC slice (`UnityTuning`).
- The rest of quality.js on the runtime asset copy: render scale, per-preset shadows, AUTO (pinned to HIGH and off
  under `-skygrindBench`, the preset logged); `quality.test.js` ported. Shader warm-up at boot (Unity 6
  `GraphicsStateCollection` on DX12 or a ShaderVariantCollection), its time logged.
- Build 2: a Silent series all-on, one all-off, then the pacing arms through the existing FPS CAP (144, 240) and a
  Windows HighQoS arm, as runs allow (bench cap).
- **IL2CPP trial (last, cut first):** one build; it counts only at `-skygrindGolden` 0 mismatches, an identical
  fingerprint and the smoke DONE; then a Silent series. Switch the default backend only on a gain above the noise
  (D-40, logged with the reverse); otherwise revert the setting.
- **Done:** the switches exist; the series are in the log; the bar met, or reported short with the breakdown's top
  cost and the options for GG (D-34).

**U3-26.5 SurvivalGame split by concern (lead, 35 min).** Partial files `SurvivalGame.Init.cs`, `.Flow.cs`,
`.Meta.cs`, `.Render.cs`, `.Debug.cs`; the HUD line's text (`RunShapeKey` / `AppendRunShape`) into Ui; one
`WeaponLook.FamilyColor` for the six sites; one binding label; dead code out. Same type, same statement order, Init
still one method. **Done:** `SurvivalGame.cs` under ~550 lines; fingerprint identical; suite green.

**U3-26.6 The sim composition (lead, 90 min).** `SurvivalSim`, a plain C# class with no UnityEngine use: it owns the
Core systems, the Func wiring and SimStep in simulate() order, with every sim-affecting EventBus registration in today's
sequence and `OnScreen` injected. SurvivalGame keeps boot, the accumulator, hitstop, interpolation and the views, with
forwarding properties so no view changes. A `FingerprintTests` EditMode test builds a `SurvivalSim` headless and equals
the committed baseline hashes. **Done:** the test green inside the suite; the Player fingerprint identical; the smoke
DONE.

**U3-26.7 The input layer and the gamepad (agents I-Core, I-Game; lead wiring 80 min).** audit-input section 4 rows 1-3,
6-11, 16; NIGHT13-RESEARCH section 3 items 1-5.
- Core: `Gamepad.cs` (PadState, the pad maths, the web's layout as an enum + bitmask, MergePad, menu nav), `HoldToggle.cs`,
  `Bindings.cs`, `MenusMath.cs`; the breakaway cap in `AimAssist.cs` (golden + 22 replays at that commit); the web tests
  ported (gamepad 20, holdToggle 6, bindings 6, menuFocus 3), seen red.
- Game: `KeyCodes.cs` (web code <-> Unity Key, both ways; a test that every default binding maps to a Key that isn't
  None) first; then `KeyboardMouseInput` on live bindings, `PadReader` (unprocessed sticks, y negated, the 0.35 trigger,
  edges latched per frame and consumed by the first step), `ScriptedInput` replacing `forcedMove` / `forcedCombat`;
  mouse and stick look summed in one frame; the last device switches prompts and the glyph set (neutral, Xbox,
  PlayStation in words: the atlases lack the shapes); rumble off by default as the web, stopped on pause, focus loss and
  quit; input polled in every flow state.
- **Done:** fingerprint identical (scripted input through the new layer); a smoke step on a virtual pad
  (`InputSystem.AddDevice<Gamepad>()` + `InputState.Change`): TITLE -> PLAY with A, move, look, fire and a kill, START
  pauses, B resumes; the keyboard steps unchanged.

**U3-26.8 Settings, CONTROLS and pad menus (agent S; lead 45 min).** audit-input rows 12-15.
- A typed `SettingsView` rebuilt on the settings version (no string reads per frame or hurt); settings in a versioned
  JSON file beside the profile, migrated once from PlayerPrefs, written atomically.
- SETTINGS shows the web's rows that now work (slideMode, arclineMode, the 8 pad keys as the GAMEPAD tab, UI scale,
  render scale); REDUCED MOTION covers shake, bob and chroma pulses as the web's rows do; Unity-only display rows
  (resolution, window mode) in UnityTuning.
- CONTROLS (menus.js :505-558): key capture, the swap and "CAN NOT BE BOUND" notes, the GAMEPAD table with RESET PAD,
  on TITLE and PAUSED. `UiNav.cs`: one Selectable per row, explicit vertical navigation, a cyan selected state, the
  primary control selected when a screen opens, B back, START resume, WASD out of UI navigation.
- **Done:** settings and bindings tests seen red then green; a smoke step rebinds jump and reads it back after reloading
  the store; a virtual-pad smoke step opens SETTINGS, moves a slider, backs out.

**U3-26.9 The hotbar and the radial (agent H; lead 30 min).** NIGHT14-RESEARCH section 2: keyboardMouse.js's Digit0,
`weapons.wheelCooldown`, quickSwap, the Tab radial (look paused while open, pointer clamped to `radial.maxOffset`,
`ArsenalMath.RadialSector` on release); the pad keeps the web's Y / d-pad cycling. The HUD hotbar (hud.js :620-645: 12
shown, a scrolling window, key labels, FRESH bars, the family colour), the weapon bar, the special hint, TITLE's
controls line. **Done:** the selection tests seen red; a smoke step opens the radial by scripted input and picks a slot.

**U3-26.10 Save-and-quit on a run snapshot (agent E; lead 60 min).** NIGHT14-RESEARCH section 4; BO6's rules.
- Core: a `State` get / set on Mulberry32 (golden + 22 replays at that commit); `IRunState` on every stateful run system
  in the list plus NIGHT12's run state (RunStats' meta fields, contracts' per-run counters, mastery's run XP, the HEAT
  rung and its mutators, the DAILY day, the onboarding lesson); a `RunSnapshot` with `saveVersion` 1.
- Proof (headless, through SurvivalSim): snapshot -> restore -> snapshot gives identical bytes; a seeded scripted run
  restored at step N equals the uninterrupted run at step M for three seeds (a round-2 break, a HOLD round's break, the
  break before an exfil); a corrupted field fails (seen red).
- Game (`design.save`, ON): solo, one slot, after round 1, at a break only, never downed or in an exfil; SAVE & QUIT in
  PAUSED at a break; RESUME on TITLE; the file beside the profile; consumed on load; another `saveVersion` is discarded
  with a log line.
- **Done:** the Core tests green; a smoke step saves at a break, goes to TITLE, resumes, and finds the round, points and
  owned guns equal.

**U3-26.11 First impressions (agent F; lead 35 min).** Re-check S1-S13 at HEAD, then fix: S1 (a lock drawn as an
Image), S2 (an explicit label flag), S3 (the field fades with distance and view angle; stripes in the doorway only), S4
(white / gold strokes, the ROUND label during the burn-in), S5 (a labelled POINTS plate apart from the weapon), S6 (a
tip on the first STALE), S7 (dormant rows hidden behind a count, each row's reward shown, in-progress first), S8 (signs
only while Playing or Paused), S9 (one line, shown at the break, dismissed when the player does the thing), S10 (one
line per mode; AIM ASSIST into SETTINGS), S12 (RESTART and a controls card in PAUSED), PLAY focused first on TITLE.
**Done:** each fixed finding has a commit; the visual ones are in the close captures.

**U3-26.12 Production basics (agent G; lead 20 min).** NIGHT13-RESEARCH section 6, "now".
- A version stamp (`Application.version` + the commit's short hash, written at build time into a gitignored file) on
  TITLE and as the Player log's first line.
- A local run log: one JSONL file per run under `persistentDataPath/runs/` with the run's existing gameplay events (the
  web's names), the last 20 kept, a setting to turn it off, automation runs excluded.
- COPY REPORT in PAUSED: the version, the settings summary, the last run log's path and the log's last 50 lines to the
  clipboard (as COPY SCORE LINE).
- F3 / F8 / F9 only with `-skygrindDev` or in the Editor (S13, D-15).
- **Done:** `git status` clean after a build; the smoke finds the run log with its lines; a test on the dev-key gate.

**U3-26.13 Close (lead, 60 min).** Section 6.

## 3. Agents and file ownership (launched after U3-26.3; each in its own worktree from the shared-types commit)

Each agent's brief starts with: `git log -1 --format=%h` must print the shared-types commit; if not, `git switch -C
n13-<name> <sha>` before any edit (worktrees can branch from `main`). Each brief names its files, the item's done line,
the PORT-RULES section 6 rule and "compile with dotnet, commit in the worktree, never touch files outside your list".

| Agent | Item | Files owned |
|---|---|---|
| P | U3-26.4 | LightPools.cs, PostFx.cs, Look.cs, EnemyViews.cs (EnemyLook), Viewmodel.cs (the overlay camera), Game/Quality.cs (new), Game/Warmup.cs (new), Core/QualityMath.cs + Quality.cs (new) + QualityTests, UnityTuning.Perf.cs |
| I-Core | U3-26.7 | Core/Gamepad.cs, HoldToggle.cs, Bindings.cs, MenusMath.cs (new), AimAssist.cs (the cap only) + their tests |
| I-Game | U3-26.7 | Game/Input/* (new: KeyCodes, KeyboardMouseInput, PadReader, ScriptedInput, Glyphs, Rumble), InputReader.cs, CombatReader.cs, UnityTuning.Input.cs |
| S | U3-26.8 | Core/Settings.cs, GameSettings.cs, Game/SettingsView.cs, UiSettings.cs, UiControls.cs, UiNav.cs (new), UnityTuning.Controls.cs + tests |
| H | U3-26.9 | UiHotbar.cs, UiRadial.cs (new), UiFeel.cs (the weapon line), UnityTuning.Hotbar.cs |
| E | U3-26.10 | Core/CoreGrid.cs (Mulberry32), Core/RunSnapshot.cs (new), `IRunState` members on the Core run systems + RunSnapshotTests |
| F | U3-26.11 | UiMeta.cs, UiPayoff.cs, UiContracts.cs, UiCeremony.cs, UiOnboarding.cs, TitleView.cs, Ui.cs (points plate, pause), Field.shader, Editor/ThemeFonts.cs if needed |
| G | U3-26.12 | Editor/BuildInfo.cs (new), Game/RunLog.cs (new), UiReport.cs (new), .gitignore (one line) |

**The lead alone:** `SurvivalGame*.cs`, `SurvivalSim.cs`, `PlayerChecks.cs`, `SliceSmoke.cs`, `Tools/*`, the URP and
project assets (by eval), the Editor, the Players, the benches, captures, cherry-picks and commits. Integrate each agent
as it lands (compile, suite, fingerprint), not all at the end.

## 4. Testing budget and caps

- Per item: one compile, the item's tests (seen red once), the fingerprint for any refactor; the full suite after each
  Core commit and at the close. No mid-night soaks.
- **Caps:** Play checks (Editor Play-mode evals) at most 2; captures at most 8 (TITLE, the spawn view, CONTRACTS, PAUSED
  with RESTART, CONTROLS mid-rebind, the radial open, the GAMEPAD tab, RESUME on TITLE); Player builds at most 5;
  bench runs at most 24 (every one reported); builders at most 8, read-only reviewers at most 2, no research agents.
  Fingerprint runs and Player smokes are gates, not Play checks.
- **Caps are caps:** if a retake or a rerun would cross one, report it short instead. Never rerun a bench to fish for a
  number.
- One heavy job at a time (a build, a bench series, the suite); never a bench beside a build or an agent's dotnet build.

## 5. Order and budgets (lead minutes)

| Order | Item | Budget |
|---|---|---|
| 1 | U3-26.1 start | 15 |
| 2 | U3-26.2 measure first (build 1, baseline series, fingerprint baseline) | 60 |
| 3 | U3-26.3 shared types, agents launched | 35 |
| 4 | U3-26.5 SurvivalGame split (while the agents build) | 35 |
| 5 | U3-26.6 the sim composition | 90 |
| 6 | U3-26.4 the perf gate: agent P lands, build 2, the series (before 00:45 if the night runs past midnight) | 75 |
| 7 | U3-26.7 the input layer and the gamepad | 80 |
| 8 | U3-26.8 settings, CONTROLS, pad menus | 45 |
| 9 | U3-26.10 save-and-quit | 60 |
| 10 | U3-26.9 the hotbar and the radial | 30 |
| 11 | U3-26.11 first impressions | 35 |
| 12 | U3-26.12 production basics | 20 |
| 13 | U3-26.13 close (starts by hour 10:30) | 60 |

- **An item 25% over its box:** ship what works behind its flag, mark it `in_progress` with what remains, move on, come
  back after the others.
- **Cut order** (only if the night runs short): the IL2CPP trial, then the radial (keep the hotbar), then save-and-quit's
  TITLE UI (keep the Core snapshot and its tests), then the run log (keep the version stamp and COPY REPORT), then S6-S10.
- **Never cut:** U3-26.1, .2, .3, .4 (all but its IL2CPP trial), .5, .6, .7, .13; the rule that the flags off give the
  web's game.

## 6. Close (U3-26.13)

1. **The final Release build**, then `git diff --stat` and the restores (TMP fallback, `Mobile_RPAsset.asset`); the smoke
   DONE with tonight's steps; `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` equal to the baseline; the final
   Silent series on an idle machine.
2. **`tasks.json`:** U3-26.* with status and evidence (a commit hash, an output line, a time); never delete an item.
3. **Docs:** PORT-STATUS.md (input: gamepad, bindings, holdToggle ported; framePace superseded; quality), PORT-PLAN.md
   ticks, PORT-RULES.md (a rule learned twice), `NIGHT13-REPORT.md` for GG (one page: what changed, the numbers, what is
   short and why, a 10-minute checklist on a pad and on keyboard, one question per new screen), a `DESIGN-PILLARS.md`
   draft marked DRAFT (D-47: one page from CLAUDE.md, ZOMBIES-ULTRAKILL-RESEARCH.md, NEXT-WAVE-RESEARCH.md), `HANDOFF.md`
   rewritten with the `handoff` skill (anchor `u3-n13`), `NIGHT14.md` refreshed (anchor, facts, items, numbering).
4. **Memory:** a lesson seen twice becomes a memory note; CLAUDE.md changes go to HANDOFF's pending decisions.
5. **Tag `u3-n13` once** on the close commit; `git status --short` shows only `UnityConnectSettings.asset` (if modified)
   and the untracked NIGHT7.md .. NIGHT14.md.
6. **Print the close table, then the line `NIGHT13 CLOSED`:**

| Item | Status | Evidence |
|---|---|---|
| U3-26.1 .. U3-26.13, one row each | DONE or SHORT | DONE: a commit hash or an output line from tonight. SHORT: the measured reason (a cap, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` reads 10.5 hours or more after the first) |

## 7. Mistakes not to repeat (NIGHT7-12)

1. A retried eval re-takes captures (NIGHT12: 15 of 12): a start-something eval keeps its guard marker after its run and
   is called with `unity command eval_file` directly. Never change screens in a capture's frame; look at every capture.
2. The tag moved three times (NIGHT12): tag last, once.
3. Benching at the end with no build left (NIGHT12): bench mid-night with a build in hand; one run decides nothing.
4. Patch scripts through the shell: write `.cjs` / `.cs` patch files with the Write tool, then run them; no heredocs or
   `node -e`.
5. A config path, string or capturing lambda per step or frame (NIGHT10-12, PORT-RULES section 6).
6. A prompt's fact taken on trust: read the code (NIGHT10's dash refill, NIGHT11's inventory).
7. Builds rewrite files: `git diff --stat` after each build; restore `Mobile_RPAsset.asset`; Play mode rewrites the TMP
   fallback asset.
8. A file written into `Assets/` before its types exist: the Editor compiles it at once; stage it outside.
9. `SurvivalGame.Respawn` or `run.DebugSkipTo` every frame in a check (NIGHT11, NIGHT12).
10. A night's goal criteria that can become impossible (NIGHT12's 9-block `/goal` loop): caps and bars are rules here,
    and an item that can't be met is SHORT with its reason, never a reason to keep going in circles.

## GG's standing instruction (verbatim, from NIGHT8)

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.

## Closing reminder (the rules this kind of night has dropped before)

- **Measure before fixing, and prove refactors by the fingerprint,** not by playing.
- **EventBus order is behaviour;** the flags off give the web's game; golden + 22 replays at every Core commit.
- **Write only what was measured:** every number in the log, report and tasks.json comes from an output line tonight.
- **Caps are caps; SHORT with a reason is a finished row.** End with the close table and `NIGHT13 CLOSED`.
