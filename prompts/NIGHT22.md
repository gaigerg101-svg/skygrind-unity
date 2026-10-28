# NIGHT22.md: SKYGRIND in Unity: FINISH, POLISH, PROVE (a 4 h night, hard cap 4.5 h: nine proofs, the polish they need, the accuracy debts)

<library_header>
id: skygrind-n22 | version: 1 | date: 2026-10-07 | model / effort: claude-opus-5-5 / high
raw request: GG, 2026-10-07: "Prioritize building an iteration polishing accuracy all of." (4 h box, 4.5 h hard cap)
planning set: Research/2026-10-07-wave22/ (RESEARCH.md, IDEAS-POOL.md, DECIDE22.md, NIGHT22-FEATURES.seed.json) | forge: wave-forge
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C#
sim with its own collider and an input tape that replays runs bit for bit), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch
`port/stage3`, base tag `u3-n21` (`b3f6194`).

**Run length: 4 hours from your first `date`. Hard stop at 4:30.** The close table prints at 4:00 or later. There is no early stop: the table is
printed at 4:00 or later, and a SHORT never cites the time limit before 4:30.

GG asked for four things at once: building, iteration, polishing, accuracy. NIGHT21 built five features and a moonshot but proved
1 of 7: no reel scenario existed for six of them, the Editor sat on a dialog for 3 of 5 hours, and several of its own numbers were
true only by construction. So this night is the first one that is judged on proof:

1. **Proof-first.** Every feature row starts with its Player reel scenario and contract check written and seen RED, then the
   feature is built, then polished. A scenario can lie (NIGHT21's slidebite reel read 0 bites because its spawn spot sat beside a
   jump pad), so every scenario prints its preconditions.
2. **Build what is missing** in the nine rows of `NIGHT22-FEATURES.json`: the HUD bars, chips, entrances and screens NIGHT21's
   Core left unbuilt, plus four new rows (THE GHOST on the DAILY, THE DUSK CLOCK, haptics, challenge-code UI).
3. **Polish with numbers:** zero M9 pop-ins on every element built this night, M-MAGENTA 0, M10 / M11 / S12 clean.
4. **Accuracy debts:** the seven A-items (U3-36.9), each a number from a run, never "by construction".

Order: start, the shared commit, builders, four compile rounds, the freeze at 3:10, the close.
</role_and_goal>

<facts_already_measured>
Read 2026-10-07 14:30-14:35 at `b3f6194` (= tag `u3-n21`). Re-check them; don't trust them blindly.
- **NIGHT21's close:** 4 DONE, 7 SHORT, 1 of 7 contract passes (slidebites only); suite 1625 / 1626 (the red,
  `AutopilotTests.PerStep_AllocatesNothing`, passes 8 / 8 alone), replays 22 / 22, golden 97 / 97; the final Player prints three
  fingerprint lines EQUAL; the smoke passes alone (200.1 s) and once failed 3 pad steps beside nothing known; the bench never ran
  (CPU 100 % from other sessions' python jobs). `NIGHT21-LOG.md` (close table + two addenda), `NIGHT21-REPORT.md`, `HANDOFF.md`.
- **Switches** (`design.n21.<row>`, both config copies byte-identical, `-skygrindOff21` = NIGHT20's game): arcline, slideBites,
  stamp, deadline, grind, challenge are **on**; replay is **off**. `FingerprintBaseline` = FingerprintBaselineN21 (round 3, 35 kills,
  4808 earned, 1 alive); `FingerprintBaselineN20` and `N18` are the off paths. Tape format 6.
- **What exists, per row** (grepped): ARCLINE `Core/Arcline21.cs`, `Game/Arcline21View.cs` (never looked at; no chip, no reticle
  states); stamp `Core/Grades21.cs` (no entrance; the RESULTS card still covers POINTS, T-11; a round P never observed);
  DEADLINE `Core/Deadline21.cs` (fuse 18 s, burn fuse 5 s; no bar); GRIND `Core/Endless21.cs`, `Records21.cs`, TITLE MODE row, WAVE banner,
  RESULTS line (no smoke step, no persistence, `SurvivalGame.cs:115` unapplied); replay `Core/Replay21Ring.cs` (no view; its "0.0000 m" is
  one pose per step); challenge `Core/Challenge21.cs` (no UI; no reader of `setup.mutators`); haptics `Game/ImpactFrames.cs` already
  calls `Rumble.Play` for impact tiers; the ghost `Game/Route/Ghost*.cs` (NIGHT14).
- **Reels:** `Game/FeelReel.N21.cs` has `slidebite` (spawns at (0, -8)) and `arcline`; run one with
  `bash Tools/night/reel.sh <buildDir> <tag> <on|pad> 300 [-skygrindReelPad] -skygrindReelOnly <ids>`; the virtual-pad pattern is the
  `rbcycle` scenario in `Game/FeelReel.N20.cs` and `Game/SliceSmoke.Night13.cs`. Latest Player with the scenarios: `Builds/n21-6`.
- **The pad:** v3 binds Y to SPECIAL + ARCLINE and RB to weapon scrolling (`Core/Gamepad.cs`, line 286 is the older v2).
- **The Editor:** it comes up on "Recovering Scene Backups" after a crash and blocks the Pipeline until someone answers (L-025).
  The build command that worked: `unity command build --target StandaloneWindows64 --outputPath Builds/<dir>/SKYGRIND.exe --scenes
  '["Assets/Scenes/Survival.unity","Assets/Scenes/Arena.unity"]' --confirm true --caller plugin --skill unity-cli`; without `--confirm true` it
  answers "Refused" and `build_status` stays `idle` (NIGHT21 polled that for 10 minutes: poll only after the reply says `queued`).
- **The machine:** other sessions run python jobs; the CPU read 100 % at 08:30, 14:00 and the bench never ran. `Tools/night/bench.sh`
  skips above 30 % CPU.
- **Measured research** (RESEARCH.md; snippets only, secondary): UI large elements enter 400 ms / exit 200 ms, small 250 / 150 ms;
  Steam Deck min text 9 px (12 recommended) at 1280x800; a kill-refilled fuse is one number that doubles as the bar, red + pulse for the
  last seconds. Not found: stamp reveal timing, grapple cooldown vs out-of-range cues. Those are our tuning with a DECIDE22 row.
</facts_already_measured>

<scope>
- **In:** the nine rows; the polish and accuracy lists below; the seeded leftovers in order (U3-36.10).
- **Out:** audio (rule 7; every new moment emits a named event); new enemy designs; the web repo; the map night; balance retunes (S2 / S4 /
  S6, BLOOD PRICE, the BULWARK bot plan); wall-run and rail grind. A new idea goes to `docs/memory/IDEAS.md`, never into code.
</scope>

<lean_rules>
- **Building share:** at least 65 % of agent-minutes go to committed code, scenarios and fixes; measuring and reviewing the rest (tests
  at most 20 %, reviewers at most 10 %). Log the split in the close.
- **Testing:** one focused test per rule, seen red once; the full suite twice (at compile round 2 and at the close; re-run only failing
  files; a red that passes alone is re-run twice alone before it is called a flake). Tests verify the work: never edit one to make it
  pass; if a premise changed, say so in the commit (NIGHT21's four).
- **Wired, not just defined:** a row passes only when its contract's check printed its line on a Player build or harness THIS night,
  `--off22` / `-skygrindOff21` gives the older game (a test), every new config key is read by code, every event is in PORT-STATUS.
- **RED means** the same scenario on the same Player build with the row's switch off (a Core check on the harness before the Core exists counts for
  Core checks only). The first Player is compile round C1, so the lead records every RED line in `NIGHT22-PROOF.md` at C1, one reel invocation per build
  (`-skygrindReelOnly a,b,c` runs several rows).
- **Scenarios print their preconditions** (spawn spot, distance to the nearest jump pad and buy station, what was owned) in the reel's
  result line, and a scenario is not trusted until it has been seen RED.
- **Numbers:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n21.<row>` / `design.n22.<row>` (both copies,
  each with `_why`) or `UnityTuning.*.cs` with a comment. Visual numbers from RESEARCH.md are cited by row (R1 .. R8).
- **Looks:** no new shader or material that is not in the Player build (Resources or Always Included). Read frames from the Player
  reel, never the Editor's. The cyan / red-orange / magenta / pale-violet roles hold: **no red for "out of range"** (rule 8).
- **Caps** (a cap reached = SHORT with the count): Player builds 8; reel invocations on Players 24 (one may run several rows; the keyboard and the pad
  pass count separately); farm batches 3 (2+ processes or 20+ runs, alone; A-5's 30-seed chase is batch 1); bench runs 3 in total (`close.sh` runs 2 and they
  are A-6's pair; alone, CPU-gated, the median); suite runs 2 (C2 and the close; nothing beside them; budget 10-15 minutes each); smokes alone (200 s each);
  agents: 3 builders alive (4 in total with W2), 4 veto reviewers, 3 persona triages. `export NIGHT=n22` at 0:00 so the helper scripts log under n22.
- **Commits:** every finished slice, explicit paths plus `.meta` files; builders at least every 40 minutes. Rule 1 holds.
- **Time boxes:** `date` at every phase boundary into `NIGHT22-LOG.md`. A phase 20 minutes over its box is cut; the rest goes to the leftovers.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, the NIGHT21 block of `HANDOFF.md`, `Research/2026-10-07-wave22/RESEARCH.md`,
`DECIDE22.md` and `Research/2026-10-07-wave21/DECIDE21.md`'s state table. **GG's answers come first:** act on any filled Answer or any word from GG before U3-36.3 and log how.
**Key files:** `Game/FeelReel*.cs` (scenarios), `Tools/feel-metrics.py` (M9, M10, M11, S12, S14, M-MAGENTA), `Tools/SimHarness/`, `Game/SurvivalSim*.cs`,
`Game/Ui*.cs`, `Core/Arcline21.cs`, `Grades21.cs`, `Deadline21.cs`, `Endless21.cs`, `Replay21Ring.cs`, `Challenge21.cs`, `Game/Route/Ghost*.cs`.
**Git:** stay on `port/stage3`; never push, merge or commit to master; tag `u3-n22` once on the close commit.
**The rules that bite:** CLAUDE.md 1 (explicit paths; never `UnityConnectSettings.asset`; restore the TMP fallback and Mobile_RPAsset after
Play or builds), 3 (golden exact), 4 (the C# collider, no PhysX), 5 (numbers), 6 (Mulberry32), 7 (no audio), 8 (roles, 16 px), 9 (docs by Write / Edit).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on. What only GG decides goes in `DECIDE22.md`.
- If one approach fails twice, switch to the next-best one and log both.
- **The Editor gate (L-025):** as soon as U3-36.1 has written the two scripts (by 0:08), run `bash Tools/night/editor-ready.sh`. If the Pipeline is unreachable for 5 minutes
  because of a startup dialog, `powershell -File Tools/night/dismiss-dialog.ps1` may choose **Skip** on a window titled exactly "Recovering Scene Backups" and
  nothing else (it logs the title it matched; no title match = no keystroke). DECIDE22 D-1's default is yes; if GG's Answer is blank, wait 10 minutes first (builders work
  headless meanwhile), then use it once and log it.
- If no Unity process exists at all (a crash), start it once with `Start-Process` on `C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe`
  and `-projectPath C:\Users\gaige\dev\SKYGRIND-unity`; never kill a running Editor (rule 10).
- The stops you want: the close table, or the hard cap. Stops you don't want: a summary announcing the next step with no tool call; an offer to
  continue; a list of decisions that block nothing; "a good place to report". (The kickoff appends `PROMPT-STOPS.md`.)
- On a usage or spend limit: commit, log the gap with `date`, keep building alone, retry agents at most every 30 minutes; a row it stops is SHORT with that `date`.
</autonomy>

<builders>
Three builders in git worktrees by file ownership, a fourth (W2) launched when the first one reports. **Only the lead touches the Editor.**
Builders write Core, harness AND Unity-side files (views, scenarios) in their worktree; the lead integrates by cherry-pick at each compile
round and compiles once, then returns compile errors to the owner by SendMessage (L-026, L-028). Each brief holds: its contracts from
`NIGHT22-FEATURES.json` verbatim, its files, "scenario first, seen red; commit every slice; the off-test first", RESEARCH.md rows for its look,
and a report under 300 words: commits, what a player now does, the contract's printed line, what is not done or not compiled.

| Builder | Owns | Queue |
| --- | --- | --- |
| **M** (moves) | `Core/Arcline21*.cs`, `Game/Arcline21View.cs`, the ARCLINE chip + reticle files, `Game/Haptics22*.cs`, `FeelReel.N22.Arcline.cs`, `.Haptics.cs`, `Harness.N22.Arcline.cs`, their tests | arcline, haptics, then the A-7 bisect |
| **S** (style and rounds) | `Core/Grades21.cs`, `Deadline21.cs`, `Game/UiStyleMeter.cs`, `RankWorld.cs`, `StyleTile.cs`, `UiPayoff.cs`, `UiResults*.cs`, the DEADLINE bar `Game/UiDeadline22.cs`, `FeelReel.N22.Stamp.cs`, `.Deadline.cs`, their harness and tests | stamp (+ T-11), deadline, then leftovers 2-3 |
| **W** (world) | `Core/Endless21.cs`, `Records21.cs`, `Game/UiMeta.cs` (MODE row), `SurvivalSim.Grind21.cs`, `FeelReel.N22.Grind.cs`, `Harness.N22.Grind.cs`, their tests | grind, then replay (`Core/Replay21Ring.cs`, `Game/Replay22*.cs`, `FeelReel.N22.Replay.cs`; **the first cut**: it starts only after the other eight rows are green, 60-minute cap) |
| **W2** (launched at the first report) | `Core/Challenge21.cs`, `Game/Code22*.cs` (TITLE field, RESULTS line), `Game/UiGhost.cs` and `Game/Route/Ghost*.cs` for the DAILY, `Game/DuskClock22*.cs`, `FeelReel.N22.Code.cs`, `.Ghost.cs`, `.Dusk.cs`, their tests | challenge (+ the mutators reader), ghost-daily, dusk clock |

**The lead alone** touches: `SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Tools/SimHarness/Program.cs`, `Harness.N22.cs`,
`FeelReel.N22.cs` (the phase list and `SafeSpot`), `RunLog.cs`, the config merge, and every build, reel, probe, review and gate.
A builder who needs a line there leaves a `partial void` hook and names the one call in its report.

## Contracts at a glance (`NIGHT22-FEATURES.json`, copied from the seed at U3-36.1; the seed has the full check text)
arcline (M) | stamp (S) | deadline (S) | grind (W) | replay (W, the first cut) | challenge (W2) | ghost-daily (W2) | dusk-clock (W2) | haptics (M)
</builders>

<tasks>
## U3-36.1 Start (0:00-0:15)
1. `date` (the night's zero); `export NIGHT=n22`. **Write the two scripts first** (0:00-0:08, Write tool): `Tools/night/editor-ready.sh` polls `bash
   Tools/unity-loop.sh status` for `"status": "ready"` every 15 s for up to 5 minutes and prints the Unity window title; `Tools/night/dismiss-dialog.ps1` (see
   Autonomy). Then run the Editor gate, `git status`, a CPU reading (`powershell (Get-CimInstance Win32_Processor).LoadPercentage`; log it; the bench waits for
   under 30 %), `unity command set_autotick --enable true --interval_ms 16 --caller plugin --skill unity-cli`, `editor_focus`.
2. Copy `Research/2026-10-07-wave22/NIGHT22-FEATURES.seed.json` to `NIGHT22-FEATURES.json`. Add U3-36 and U3-36.1 .. U3-36.11 to `tasks.json`.
3. Read the context files, `docs/memory/THREADS.md` (T-21 .. T-26), and act on GG's answers.

## U3-36.2 The shared commit (0:15-0:45)
1. **Switches:** `Core/N22Blocks.cs` (six bits: ghostDaily, duskClock, haptics, directed, trickChain, overdrive; `ForceOff`, `-skygrindOff22` /
   `--off22`), `SurvivalSim.N22.cs` with partial hooks, rows in both config copies (all `enabled: false`, each with `_why`), tape format 7 (formats
   1-6 still read), `FingerprintBaseline.N22OnPath` false. Copy the NIGHT21 pattern (`N21Blocks.cs`, `SurvivalSim.N21.cs`, `N21SharedTests.cs`).
2. **Scenario scaffolding:** `FeelReel.N22.cs` with phase F (`arcline2`, `stamp`, `deadline`, `grind`, `replay`, `code`, `ghostdaily`, `dusk`) and
   `SafeSpot(g)`: an open spot at least 12 m from every jump pad (`pads` in `PortExport/maps/survival.json`) and every buy station, whose distances go in the result
   line. One empty partial file per scenario (names in the table above).
3. **Harness:** `Harness.N22.cs` dispatching one command per row; the proof ledger `NIGHT22-PROOF.md` (row, scenario, RED line, GREEN line, date).
4. **Lead hooks, placed once so builders never wait on the lead's files:** one `partial void` call per row in `Game/Ui.cs` (HUD build; builders keep their widget in
   their own file), `SurvivalGame.Init.cs` / `SurvivalGame.Render.cs` (views) and `SurvivalGame.Route.cs` (the DAILY ghost gate). **Tape format 7** stores the N22 byte AND the
   run mode (W checks a GRIND tape round-trips its mode; one line in `RunTape.cs` is allowed); TapeTests green at C1. `UiGhost.cs` belongs to W2.
5. **Meters** in `Tools/feel-metrics.py` (the lead's; builders ask for changes), each with a synthetic self-test as M-MAGENTA had: **T11** (the RESULTS grade
   card's `ui[]` rect must not intersect the POINTS rect on any frame of `ui-results` / `stamp`), **LUM** (mean frame luminance per scenario and the
   monotonic check across the `dusk` rounds), **STAMP-T** (the stamp element's entrance time from the existing M9 run machinery).
6. **Gates:** fingerprint EQUAL (N21), `--off21` EQUAL (N20), `--off19` EQUAL (N18), golden 0 / 22 exact, `impact PASS`, `n21shared PASS`, the
   Editor compile, `N22SharedTests`. Commit. Launch M, S and W with a `date` (target 0:45).

## U3-36.3 ARCLINE: prove, then polish (M; about 2 h)
- **Red first:** `arcline2` on the Player with the chip and the virtual pad absent: it must FAIL its checks (no chip, no pad-fired yank).
- **Build:** the terminal buy played in the scenario; 3 or more husks in a line; a virtual pad press of the button the layout binds to GRAPPLE fires the yank (spike, forge 14:40: v3 is `SwapRbY(DefaultLayoutV2)`, `Core/Gamepad.cs:389`, so Y carries SPECIAL + GRAPPLE; copy `rbcycle`, which reads `g.padReader.NextWeaponInputButton`, and add the GRAPPLE analogue if the reader lacks one); the HUD chip
  with three looks: READY (cyan), COOLDOWN (a dim arc and the seconds), NO TARGET (the reticle dims to 40 % alpha, never red: rule 8, DECIDE22 D-3);
  cooldown and out-of-range are different looks (RESEARCH R7). Rope and node glows looked at on 3 frames per pass.
- **Check:** the contract `arcline`; `SimHarness arcline` still PASS; golden exact.

## U3-36.4 THE STAMP and T-11 (S; about 1 h 40)
- **Red first:** `stamp` on the Player shows a pop-in or no entrance, and the grade card rect intersecting POINTS.
- **Build:** the inked letter enters in 200 to 300 ms (the M9 entrance target is 90-300 ms in `feel-metrics.py`; RESEARCH R1's 400 ms is for a large dialog, so
  this is our tuning) and leaves in 150 to 200 ms; the grade card moves off
  POINTS; a scripted run that earns grade S, peak SS and no revive prints `roundGrade p=true` once (A-4). The rank letter's lost shine (DECIDE22 D-7):
  one Player-safe replacement, or leave it flat and log why.
- **Check:** the contract `stamp`; M9 0 pop-ins on the stamp; M-MAGENTA 0.

## U3-36.5 DEADLINE: the bar (S; about 1 h)
- **Red first:** `deadline` shows a round with no bar.
- **Build:** `UiDeadline22.cs`: the fuse number is the bar (18 px or more at 1080p: M10's measured floor in `feel-metrics.py` is 18, above CLAUDE.md's 16; new text uses 18; DANGER role), a pulse in the last 5 s (our tuning from R5), a
  separate "+N" pop on each kill, a burn flash. The chase check over 30 seeds at rounds 10 and 15 (A-5): at most 50 % of normal at BOTH; if round 15
  fails, retune and log both numbers.
- **Check:** the contract `deadline`.

## U3-36.6 THE GRIND: wired (W; about 1 h 40)
- **Red first:** the smoke has no GRIND step.
- **Build:** the smoke step (pad: A on the MODE row, a wave reached), the ROUND banner off, `SurvivalGame.cs:115` PB source (the lead applies W's line),
  `Records21` persisted across a restart, a GRIND tape that replays EQUAL (check how the tape records the mode; format 7 may carry it), the rising-edge
  telegraph in DANGER.
- **Check:** the contract `grind`.

## U3-36.7 INSTANT REPLAY: a view and a real error (W; 60-minute cap, after U3-36.6)
- Ring every 2nd step, interpolated on playback; measure the camera error against the live path (A-1). VIEW on DOWN and at a round clear, half speed,
  back to RESULTS. Turn `design.n21.replay` on only if the contract passes; over the cap: SHORT, committed, off.

## U3-36.8 Building: the four new rows (parallel, by owner)
- **challenge (W2):** the TITLE CODE field (pad-fillable), the RESULTS line, and a reader for `setup.mutators` so a code's mutator is active in the run (A-3).
- **ghost-daily (W2):** the DAILY ghost unlocks after a first DAILY clear; RESULTS says AHEAD or BEHIND YOUR GHOST with the seconds (Neon White's gate).
- **dusk-clock (W2):** the sky sinks per round toward violet and cyan by round 15, never magenta or red-orange (rule 8), using shipped materials only; the LUM meter's
  mean luminance falls monotonically at rounds 1, 8, 15 (the median of a window, not one frame).
- **haptics (M):** distinct patterns for the shockwave, ARCLINE attach, kills and DOWN on the existing `Rumble`, plus a per-scenario haptics log; VIBRATION off silences all.

## U3-36.9 The accuracy sweep (the lead, whenever a build or a suite runs; builders when idle)
- **A-1** the replay error (above). **A-2** the pad press of the GRAPPLE button (U3-36.3). **A-3** mutators (above); "the same tape" means identical bytes with the header's
  clock stamp zeroed. **A-4** the round P: the bot never reaches SS, so a harness input script that feeds the style meter to SS is allowed and is labelled "scripted" in
  the result; 20 minutes, then SHORT. **A-5** DEADLINE at both rounds (above).
- **A-6** the bench: three runs, alone, CPU under 30 % before each, the median; if the CPU never clears, log the readings and leave it SHORT.
- **A-7** `AutopilotTests.PerStep_AllocatesNothing`: builder M bisects the step in a SimHarness loop (a GC allocation counter around each system's step; no profiler, no Editor)
  to name the one allocation, or documents that it cannot be reproduced; do not weaken the test.

## U3-36.10 The leftovers, in order (a builder who is free takes the next; each gets a contract line before it starts; 4 and 5 only before C3)
A run does not end at its list: after leftover 7 take the top row of `docs/memory/IDEAS.md` that is not parked.
1. Directed steps for THE HUNT / THE BEACON. 2. T-12: M9 pop-ins on the older screens through the shared motion. 3. T-10: words where glyphs belong.
4. TRICK CHAIN (`design.n22.trickChain`; a sim change: one more fingerprint re-record). 5. OVERDRIVE (after 4). 6. THE GRIND's spawn beam. 7. The shine replacement.

## U3-36.11 The loop and the close
**Compile rounds** (cherry-pick, metas, one compile, errors back to owners, build, smoke alone, the rows' scenarios, feel-metrics M-MAGENTA / M9 / M10 / M11 / S12):
C1 1:15, C2 1:50 (plus the first suite, async; no reel beside it), C3 2:30, C4 3:05.
**Review:** a persona triage at C2, C3 and C4 (`Tools/night/persona-triage.md`; triage only: personas never pass a feature). Two veto reviewers at C3
(`reviewer-veto.md`, sides swapped, 25 minutes / about 80 tool calls each) and two at the final reels. They see real scenarios now.
**Freeze 3:10.** In this order: (1) `N22OnPath` is true if and only if a sim-changing default is on (the mutators reader, directed steps, TRICK CHAIN or OVERDRIVE); then record
`FingerprintBaselineN22` once, in its own commit, with the reason. Otherwise leave it false, say so in the log, and the as-configured run must still be EQUAL to the N21
baseline. Either way `--off22` is EQUAL to the N21 baseline, `--off21` to N20 and `--off19` to N18; (2) the final build; (3) alone: `bash Tools/night/close.sh
Builds/<final>`; (4) the suite on the frozen code, with nothing heavy beside it; (5) the rows' final scenarios and the bench x3 if the CPU allows; (6) the docs:
NIGHT22-LOG.md (close table + the building-share split), NIGHT22-REPORT.md, FEEL-PLAYTEST.md (10 minutes, pad first, per row what to look at), PORT-STATUS (events),
DECIDE22.md state table, HANDOFF.md (a NIGHT22 block, the `handoff` skill), tasks.json, NIGHT22-PROOF.md, `wave-memory harvest 22`; (7) tag `u3-n22`; `date`; the close
table; `NIGHT22 CLOSED`. Last picks 2:50; the table at 4:00 or later, before 4:30.
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-36.1 | the Editor gate logged, contracts copied, tasks.json parses, the CPU reading logged |
| U3-36.2 | the shared commit with its gates printed, tape format 7, scenarios scaffolded, builders launched with a `date` |
| U3-36.3 .. 36.8 | each row: its scenario seen RED, then GREEN, `passes: true` with the evidence line, the off-test green, golden exact, events in PORT-STATUS; or SHORT with the measured reason |
| U3-36.9 | A-1 .. A-7 each printed a number (A-6 may be SHORT with the CPU readings) |
| U3-36.10 | each leftover taken has a contract line, a commit and its check line |
| U3-36.11 | four compile rounds logged with triage; the close checks printed; the suite totals; the docs; `u3-n22` tagged |
A row is SHORT with its measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT22-REPORT.md`, one page for GG. Outcome first: the passes count against the forge's hypothesis (6 of 9). Then each row: what he can do, its switch, the
contract's number, the persona line, and what felt worse. Then what is not verified and the DECIDE22 questions. Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Proof before polish:** a row passes only on a Player reel or harness line from tonight; a scenario that was never seen RED proves nothing.
- **One Editor, one driver:** builders never touch it; compile rounds are on the clock; the Editor gate comes first.
- **Heavy jobs one at a time:** no bench, smoke, reel or farm beside the suite or a build. Commit every slice.
- Every message ends with `N22-STATUS: <d> done, <s> short, <l> left (of 11) · compile round <r> · builders <alive>/3 · passes <n>/9 · <h:mm> / 4:30`.
</closing_reminder>

## The kickoff

Launch `claude --model claude-opus-5-5 --append-system-prompt-file PROMPT-STOPS.md`, run `/effort high`, then paste:

```text
/goal Met only when a `date` reads 4:00 or later after the first `date` AND NIGHT22.md's close table is printed with every row U3-36.1 .. U3-36.11 DONE (its done-when evidence from this session) or SHORT (a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its date, or the time limit once a `date` reads 4:30), followed by "NIGHT22 CLOSED"; SHORT rows count as met; at 4:30 the run stops whatever the table says. Run NIGHT22.md in C:\Users\gaige\dev\SKYGRIND-unity: `date` first, then the Editor gate, read CLAUDE.md and NIGHT22.md, and work it in order with at most 3 builders (4 in total) split by file ownership, committing every slice, proof-first: each row's reel scenario is written and seen RED before the feature is built. Freeze at 3:10; print the table at 4:00 or later from the first `date`; hard cap 4:30. Never ask me questions; never push, merge or commit to master; the web repo is read only. On a usage limit keep building alone and retry agents at most every 30 minutes. End every message with the N22-STATUS line.
```
