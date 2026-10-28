# NIGHT15.md: SKYGRIND in Unity: THE FIRST FIFTEEN MINUTES (what a new player meets, made true and looked at), at most 12 h

> **QUEUED (2026-10-05 11:10 AM):** GG made the new map the priority (`NIGHT15.md` is map night 1). When this night is
> scheduled, rename it to its night number, update its IDs, anchor and tag, and re-check every fact: the look tour and the
> teaching should then run on the new map, and the bot fixes (U3-28.3 here) come before any farm on it.

> **Written at the NIGHT14 close (2026-10-05 ~9:45 AM, anchor `u3-n14`), replacing the POWER guns night, which moved to
> `NIGHT16.md` unchanged.** GG may swap the two files back; everything here still holds after the guns.

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves; the Editor is open on this project. Nobody answers questions during the night.

## Why this night (read this first; it decides every trade-off below)

Fourteen nights built a faithful, deterministic port with strong tools: golden vectors, the fingerprint, the tape, an
AUTOPILOT and a farm. Nobody has played a Unity build yet (NIGHT11-REPORT, NIGHT13-REPORT), and the screens a new player
meets have been looked at less and less: NIGHT12 and NIGHT14 cut their captures, and NIGHT14 shipped five new screens
with no captures at all. What we know now:

- **The farm measures the bot, not the game.** On u3-n14 the AUTOPILOT counted itself "in the ring" only within half the
  ring's radius (`AutopilotInput.cs:73`, `design.autopilot.holdInsideShare` 0.5, against the game's full
  `objectives.radius`, `Objectives.cs:244`), its nav stops up to 3.8 m from the round-6 ring's centre, and inside the ring
  it zeroes move, dash and slide (`AutopilotInput.cs:296-298`), so it cannot dodge acid. Result: 54 % of the runs that
  enter a HOLD round end there, 37 of 41 timeouts stand next to the round-6 ring for 35 minutes, the bot never fired the
  rocket or the rail it bought (they're absent from `design.autopilot.guns.near`), never called an exfil, and left 24,818
  of 32,492 points unspent (perk:plate first on its list, unreachable within `shopGiveUpSeconds`). NIGHT14-REPORT told
  GG "round 6 is a wall"; that is mostly the bot. A farm that lies can't judge the POWER guns, so it is fixed first.
- **The smoke is red.** Build 2 of u3-n14 prints `DONE: 2 step(s) failed`: on a composition that played the smoke's earlier
  steps, a seed-1 run differs from the baseline from checkpoint 6 (steps 3001-3600). A fresh Player is EQUAL; two runs
  on the played composition equal each other; `RunRestoreTests.AfterAResume_TheNextRunIsAFreshCompositions` proves the
  sim's snapshot sections clean. The cause is Game-side. A red smoke hides the next regression.
- **The first fifteen minutes teach little and read poorly.** The onboarding never teaches the parry, the box, HOLD,
  MODS or exfil (`design.onboarding.rounds`: cards at the start of rounds 1, 2, 3, 5 and 6 of the first run, read
  mid-fight or not at all); round 6 stacks the first HOLD, the
  spitter's debut and the PROTOCOL; the shader warm-up does nothing (`[warmup] none: no ... .graphicsstate`). The
  captures from NIGHT12-13 show: a HUD label drawn "R O UND" with no number beside its bar, hotbar labels cut ("KESTR"),
  the HOLD ring as a thin floor line with its state inside one dense HUD line, lesson cards stacking top-left during
  fights, diagonal light stripes over the whole view, and a 12-binding controls line on TITLE.

Tonight's job: make what a new player meets in rounds 1-7 true (the smoke green, the farm honest) and good to look at
(every screen captured and reviewed, the worst defects fixed with before / after captures), teach the missing verbs at
the moment they matter, and hand GG a playable build with a 15-minute playtest kit. Gameplay numbers and enemy design
are GG's (CLAUDE.md rule 7, D-55): where the evidence says they should change, write a proposal, not a change.

**Demo Day (Oct 7) plays the tagged `u2a-demo` build** (PORT-PLAN.md:141; `Builds/skygrind-web-u2a.zip`). Nothing
tonight touches that tag or its builds.

## Read these files

- **`CLAUDE.md`** (the rules that always hold; read it first), then this file.
- **`HANDOFF.md`** sections 0, 1, 4, 5 (D-57 especially) and 6; **`NIGHT14-LOG.md`** "After the reboot" and the close
  table; **`Reports/FARM-n14.md`**; **`PORT-RULES.md`** sections 6, 11 and 12.
- **`tasks.json`:** write U3-28 and U3-28.1 .. U3-28.10 (below) as the night's items before any code.
- The memory notes (`MEMORY.md`): `play-check-eval-recipe`, `night-parallel-agents`, `unity-editor-play-mode-recompile`,
  `bench-power-plan-flips`.

**Run length: up to 12 hours from your first `date`; the close starts by hour 10:30.**

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch `port/stage3`** from `u3-n14`. Tag `u3-n15` once, last, on the close commit. The web repo stays read only.
2. **What you may change:** the bot (`Game/Bot/*`, `design.autopilot`), the HUD and screens (Ui*.cs, Unity-side layout
   and readability), the onboarding prompts (Unity-side text and triggers), the smoke and Player checks, the warm-up
   file. **What you only propose:** any `PortExport/config.json` number, any enemy behaviour, round order or economy
   value. A proposal goes in `PROPOSALS-n15.md` (U3-28.9) with its evidence.
3. **Flags:** each new player-facing behaviour sits behind a `design.*` key in `Design/unity-config.json` (and its
   byte-identical StreamingAssets copy) with a `_why`, default on; off gives u3-n14's game. A bug fix needs no flag.
4. **Verify by looking, inside the item:** an item that changes something on screen is DONE only with a capture taken
   after the change and a written two-line review of it (what a first-time player sees in one second; what is still
   wrong). Captures go to `Logs/shots/n15-*.png` (CLAUDE.md: move them out of `Assets/` at once).
5. **A Player build early:** build 1 within two hours of the first `date` (after U3-28.2's first fix or at hour 2,
   whichever comes first), with the smoke run on it. Every later build runs the smoke. New smoke steps run first on a
   Player build, the night they are written.
6. **One driver for the Editor:** you alone drive the Editor, the Players and git. Builders (at most 3) work in worktrees,
   compile with `dotnet build` (the csproj globs from `Tools/wt-csproj.cjs`) and never call `unity` or the Editor. Pause
   their builds before an Editor compile (the commit charge under 85 % of the limit).
7. **If the Editor stops answering** (a call times out): run `editor_focus`, wait 2 minutes, try one `status`. Poll at
   most once a minute after that. After 20 minutes: log it, send a terminal notification, and move to work that needs no
   Editor (the SimHarness, dotnet tests, docs, the farm on the last good build). Restarting the Editor follows GG's answer
   to D-57 in HANDOFF section 5; with no answer, CLAUDE.md rule 10 stands.
8. **The machine:** benches only on AC power, Silent or Balanced the same before and after, no heavy process beside them
   (check CPU and GPU first); run them before 1:00 AM (GG's scheduled jobs). The farm never runs beside a bench.
9. **Builders' briefs** carry CLAUDE.md's hard rules, this section, their file list and their acceptance lines verbatim.
10. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. One status line
    per item in `NIGHT15-LOG.md` with times from `date`.
11. **End every message with one status line** for the `/goal` checker: `N15-STATUS: <d> done, <s> short, <l> left (of
    10) · tests <passed>/<total> · <time from date>`.

## 1. The items (U3-28.1 .. U3-28.10)

Each item says why it matters, what to do, and what proves it done. "Done when" lines are the acceptance: a result you
can quote (a test line, a capture you reviewed, a number), never "implemented".

### U3-28.1 Start (lead, 15 min)
- `date` first; HANDOFF section 1 row by row; `RECEIVED` in `NIGHT15-LOG.md` with each row's output.
- The machine: AC state, power plan, CPU / GPU load, commit charge. Write U3-28.* into `tasks.json`.
- **Done when:** the arrival rows are logged with their outputs and tasks.json parses.

### U3-28.2 The smoke green (lead, 90 min; SHORT after 150 with the bisect's result)
- **Why:** a red smoke hides regressions, and a long session must play the same as a fresh one (RETRY SAME SEED, the
  attract, a tape replayed after other runs).
- **Do:**
  1. Make step 18's check runnable after any prefix of the smoke's steps (e.g. `-skygrindSmoke -smokeUntil <n>` runs
     steps 1..n, then step 18's two fingerprint runs). Bisect on the Player: the first step whose presence makes the
     run differ is the suspect.
  2. Add a component dump beside the hash (`SimFingerprint` parts every 100 steps: mover, health, economy, run, director,
     enemies, style, loadout) and diff a played composition's dump against a fresh one's from step 3000 on.
  3. Suspects, in order: SurvivalGame's event handlers or views that write sim state between steps; state outside
     `SurvivalSim.SnapshotSections` that a played session changes (zones and gates, the core grid, the machines' ride
     points, the profile's unlocks, settings applied to the sim); the save / resume step (build 1's step 21 passed while
     SAVE & QUIT was broken; build 2's fails with it working).
  4. Root cause, then a test that fails on it (EditMode where the cause is reachable headless), then the fix.
- **Done when:** a Player build's smoke prints `DONE: all steps passed`, and the log names the root cause and the red
  test's output.

### U3-28.3 The farm tells the truth (builder A in a worktree; lead 40 min to integrate; builder ~120 min)
- **Why:** the farm is the balance tool for every later night (PORT-RULES 12; the POWER guns next). Its u3-n14 numbers
  are the bot's limits.
- **Do (each fix test-first, seen red):**
  1. **Hold:** "inside" is the game's own test (`objectives` inside, the full radius); when the nav override stops
     short of the ring, finish the approach on a straight line (`Approach()` exists for the shopper); inside the ring keep
     the brain's movement clamped to the ring so it can dodge globs and leave puddles; fight anything visible within the
     gun's range, not only within `holdFightMeters`.
  2. **Shopping:** plan by route time on the masked nav graph the bot uses, skip goals with no route or a route longer
     than the time left, allow several buys per break, buy mid-round when no enemy is within a design distance; keep
     `perk:plate` / `perk:flow` reachable or after the reachable ones.
  3. **Guns:** the owned gun with the best damage at the current distance (read from config), so every bought gun fires.
  4. **Exfil:** call the radio when offered, at a design round (`design.autopilot`, with `_why`), so exfil is measurable.
  5. **Farm records per round:** time, the ring's position, steps inside the ring, points banked and spent.
- Every new bot number goes in `design.autopilot` with its `_why`. The game's rules and numbers are untouched; the
  fingerprint has no bot in it and stays EQUAL; `SimHarness autopilot-twice` stays EQUAL.
- **Done when:**
  - AutopilotTests cover the five fixes (each seen red, then green in the Unity suite).
  - A test on the round-6 ring at (36, 0, 0): after the bot arrives it is inside by the game's own test for at least
    90 % of the HOLD round's steps.
  - A Player farm of 3 x 100 runs from seed 1 lands in `Reports/farm-n15/`, and `node Tools/farm-report.cjs diff` against
    `Reports/farm-n14/` writes `Reports/FARM-n15-diff.md`. The diff is looked at and quoted in the log: the HOLD-round end
    rate, the round-6 timeouts, rocket and rail kills (above 0 when owned), exfils (at least one, or the measured reason),
    unspent points.
  - Tonight's `Reports/farm-n15/` becomes the baseline that NIGHT16's guns diff against.

### U3-28.4 The look tour (lead, 60 min; one batched Play check)
- **Why:** nobody has looked at most of what a new player sees, and five NIGHT14 screens have never been captured.
- **Do:** one Play check (one `eval_file` coroutine, the memory recipe), capturing 12 screens:
  1. TITLE, with the attract running behind it
  2. round 1 with its first lesson
  3. a husk close, mid-fight
  4. the first break's gate prompt
  5. the box or a wall buy prompt
  6. the round-6 ring from outside it
  7. the same ring from inside it
  8. a spitter's glob incoming
  9. DOWN, then WHAT GOT YOU
  10. RESULTS with THE ROUTE
  11. a ghost and a split on a second run
  12. the kiosk's hints
- For each, write two lines in `Reports/LOOK-n15.md`: what a first-time player sees in one second, and what is wrong.
  Check every screen for:
  - enemies readable against the background
  - text at least 16 px at 1080p
  - overlaps and truncation
  - the colour rules (CLAUDE.md 8)
  - clutter competing with the threat
- **Done when:** the 12 captures exist in `Logs/shots/n15-look-*.png`, each one looked at, and `LOOK-n15.md` ranks the
  defects P1 (a new player misreads or misses something that matters), P2 (ugly or confusing), P3 (polish). The known
  ones from "Why" are in the list, confirmed or struck.

### U3-28.5 Fix the P1 defects (builder V in a worktree, Unity-side UI only; lead 30 min; builder ~90 min)
- **Why:** the defects a new player hits in the first minutes cost more than any new feature adds.
- **Do:** start with the known ones while U3-28.4 runs, then take LOOK-n15's P1 list:
  - the HOLD guidance (`design.holdGuide`): a world marker over the ring visible from outside, a progress arc on the
    ring itself, and the HUD line split into the objective and its timer
  - one lesson card on screen at a time, held back while enemies are within a design distance
  - the "R O UND" label and its missing number
  - the hotbar's cut labels
  - P1s found by the tour
- Unity-only layout numbers go in `UnityTuning.cs` with a comment; new behaviour behind a `design.*` flag.
- **Done when:** each fix has a before / after capture pair, looked at and reviewed in LOOK-n15.md; the smoke passes; the
  UI-hurt bench is within 0.2 ms of build 1's (or SHORT with the measured machine state).

### U3-28.6 Teach by doing (builder K in a worktree; lead 20 min; builder ~75 min)
- **Why:** the parry, the box, HOLD, MODS and exfil are core verbs that no lesson teaches; a text card at round start is
  read by few players.
- **Do:** a one-line prompt at the moment of need, at most one on screen, once per profile, off in automation:
  - the parry: the first glob or wind-up aimed at you
  - the box: the first time you stand near it with enough points
  - HOLD: entering the round-6 ring's zone
  - MODS: the first draft
  - exfil: the radio offered
- Texts and triggers live in a new `design.onboarding.moments` list beside the existing round cards
  (`design.onboarding.rounds`), with a `_why`; `moments.enabled` false gives u3-n14's onboarding.
- **Done when:** a test per trigger (seen red); 3 of the prompts captured in context and reviewed; a smoke step shows
  each trigger fires once and never in automation.

### U3-28.7 The shader warm-up (lead, 25 min)
- **Why:** first-use stalls are what a new player feels in the first minute (pillar 4).
- **Do:** record `SKYGRIND-Direct3D12-PC.graphicsstate` with `-skygrindTraceShaders` during a smoke run; ship it under
  `StreamingAssets/Warmup/`. Measure the first 60 s of a Player run before and after, in the same conditions: the max
  frame, and the frames over 33 ms.
- **Done when:** the Player log no longer prints `[warmup] none`, and the before / after numbers are in the log.

### U3-28.8 A playtest kit for GG (lead, 35 min)
- **Why:** the one thing no night can produce is a human's first reaction; make GG's 15 minutes count.
- **Do:**
  - **F9 NOTE in a run:** saves the live tape, a screenshot and the run's state (round, points, seed, time) to
    `runs/notes/<time>/`. Unity-only, `UnityTuning` key; a smoke step proves it.
  - **`PLAYTEST.md`** (one page): how to start the build, a 15-minute script for GG and one peer (play blind for 5
    minutes, then the checklist), five questions, F9 at every "huh?", where the notes land.
- **Done when:** the smoke's F9 step passes and PLAYTEST.md exists.

### U3-28.9 Proposals for GG (lead, 30 min)
- **Why:** the evidence points at numbers and design that only GG may change.
- **Do:** `PROPOSALS-n15.md`, each with the evidence (from `Reports/farm-n15/` after the bot fixes, or a file:line), the
  options, a recommendation and the config keys involved (unchanged):
  - round 6 stacking the first HOLD, the spitter's debut and the PROTOCOL right after the first boss
  - HOLD against the anti-camp hunters (`Director.cs:704-721`)
  - a HOLD round that is free once its enemies are dead
  - GOLD RUSH's chain bonus growing with the square of the chain
  - income against what there is to buy
  - the ARCLINE that can't be had in SURVIVAL
- **Done when:** every proposal cites a farm-n15 number or a file:line.

### U3-28.10 Close (lead, 60 min; section 4)

## 2. Builders and file ownership

- **The shared commit first (lead, 20 min):** every new `design.*` block (autopilot additions, holdGuide,
  onboarding.moments)
  with `_why` and defaults, and the hooks each builder needs, so nobody edits another's lines.
- **A (the bot):** `Game/Bot/*`, `Farm.cs`, `Tests/EditMode/Game/AutopilotTests.cs`, its `design.autopilot` block.
- **V (the HUD):** the HUD and screen views it names in its brief (Ui*.cs minus UiOnboarding), `UnityTuning.cs` entries
  it adds, its `design.holdGuide` block.
- **K (teaching):** `UiOnboarding.cs`, a new `Onboarding*.cs`, its tests, its `design.onboarding.moments` block.
- **Lead:** SliceSmoke*, PlayerChecks, Warmup, SurvivalGame*, integration, every build, Play check and capture.
- Topic-named members in shared partial classes (PORT-RULES 11). Integration by cherry-pick, tests seen red first.

## 3. Caps and order

- **Caps:** Play checks at most 3 · captures at most 40 (the tour's 12, the fix pairs, the prompts) · Player builds at
  most 5 · bench runs at most 12 (series of three, the median, every run reported) · builders at most 3, reviewers at most
  1 · farm at most 4 processes and 2 hours of wall time. A cap reached makes its item SHORT with the count.
- **Lead minutes:** 15 + 20 + 90 + 40 + 60 + 30 + 20 + 25 + 35 + 30 + 60 = 425 (about 7 h of 12): the rest is buffer for
  what goes wrong. **An item 25 % over its box** ships what works behind its flag, is marked `in_progress` with what
  remains, and the night moves on.

| Order | Item | Lead box |
|---|---|---|
| 1 | U3-28.1 start | 15 |
| 2 | the shared commit; builders A, V, K launched | 20 |
| 3 | U3-28.2 the smoke green (build 1 by hour 2) | 90 |
| 4 | U3-28.7 the warm-up (rides build 1 / 2) | 25 |
| 5 | U3-28.4 the look tour (on build 2 or the Editor) | 60 |
| 6 | U3-28.3 integrate A; the farm in the Player | 40 |
| 7 | U3-28.5 / .6 integrate V and K; their captures | 50 |
| 8 | U3-28.8 the playtest kit | 35 |
| 9 | U3-28.9 proposals (after the farm) | 30 |
| 10 | U3-28.10 close (starts by hour 10:30) | 60 |

- **Cut order** (only if the night runs short): the proposals' prose (keep the evidence table), then U3-28.6's MODS and
  exfil prompts, then U3-28.5's P1s beyond the known ones. **Never cut:** U3-28.1, .2, .3, .4, .10.

## 4. Close (U3-28.10)

1. **The final Release build;** `git diff --stat` and the restores (TMP fallback, `Mobile_RPAsset.asset`); the smoke
   `DONE: all steps passed`; `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` EQUAL; `-skygrindReplay` on the
   smoke's tape EQUAL; the UI-hurt Silent series if the machine is on AC and idle (else SHORT with the measured state).
2. **`tasks.json`:** U3-28.* with status and evidence (a commit hash, an output line, a time). Never delete an item.
3. **Docs:**
   - **`NIGHT15-LOG.md`** (tracked).
   - **`NIGHT15-REPORT.md` for GG** (one page): what a new player meets now, before and after (three capture pairs);
     the farm diff's headline, stated as the bot's numbers; the playtest kit; one question per changed screen.
   - **`HANDOFF.md`** rewritten with the `handoff` skill (anchor `u3-n15`).
   - **`NIGHT16.md`** refreshed: the anchor, the farm baseline `Reports/farm-n15/`, the new guns added to the bot's gun
     choice.
   - **PORT-STATUS.md** (the Unity-only rows); **PORT-RULES.md** (a rule learned twice).
4. **Memory:** a lesson seen twice becomes a note; CLAUDE.md changes go to HANDOFF's pending decisions.
5. **Tag `u3-n15` once** on the close commit. `git status --short` shows only `UnityConnectSettings.asset` (if
   modified), `Assets/_Recovery*` (GG's to decide) and the untracked NIGHT*.md.
6. **Print the close table, then the line `NIGHT15 CLOSED`:**

| Item | Status | Evidence |
|---|---|---|
| U3-28.1 .. U3-28.10, one row each | DONE or SHORT | DONE: the item's "done when" evidence from tonight. SHORT: the measured reason (a cap, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` reads 10.5 hours or more after the first) |

## 5. Mistakes not to repeat (NIGHT7-14)

1. Patch scripts through the shell: write `.cjs` / `.cs` files with the Write tool, then run them; no heredocs or
   `node -e` (NIGHT13, NIGHT14 slipped once each).
2. A start-something eval without a guard that outlives it; unqualified names or `using` lines in an eval.
3. A smoke check that waits a number of frames: a Player runs ~400 fps: wait in real time.
4. A claim in a commit message or doc that wasn't checked: grep or run before you write it. NIGHT14 told GG "round 6
   is a wall" from bot data; describe farm numbers as the bot's until the bot's behaviour is verified.
5. Two agents' generic names in one partial class (PORT-RULES 11).
6. A bench on a busy machine or on battery, or one run deciding anything: three runs, the median.
7. Builds rewrite files: `git diff --stat` after each build; restore `Mobile_RPAsset.asset`; Play mode rewrites the TMP
   fallback asset.
8. No Player build until the end: NIGHT14's first build came after 10 hours and found two bugs the night had shipped.
9. CLI poll loops against a busy Editor, and forcing the import workers (NIGHT14's hang, HANDOFF section 7).
10. A builder's brief without the night's rules (NIGHT14's rule 12 slip).

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

## Closing reminder

- **True first, then good to look at:** the smoke green and the farm honest before anything is judged by them.
- **Looked at means a capture reviewed in writing,** inside the item, the same hour.
- **GG owns the numbers and the enemies:** evidence becomes a proposal. Write only what was measured; caps are caps;
  SHORT with a measured reason is a finished row.

## Suggested `/goal` for GG

> Run NIGHT15.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT15.md and the files
> it lists, and work U3-28.1 .. U3-28.10 in NIGHT15.md section 3's order with at most three background builders split by
> file ownership. Never ask me questions and don't stop early; end every message with the N15-STATUS line. Never push,
> merge or commit to master; the web repo is read only. This goal is met when your latest message prints NIGHT15.md's
> close table with every item U3-28.1 .. U3-28.10 marked DONE (with its "done when" evidence from this session) or SHORT
> (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit
> once a `date` output reads 10.5 hours or more after the first `date` output), followed by the line "NIGHT15 CLOSED".
> SHORT rows count as met.
