# NIGHT14.md: SKYGRIND in Unity: THE GAME PLAYS ITSELF. No human in the loop, at most 12 h

> **Written 2026-10-04 (9:50 PM) at GG's request:** "go full creative mode ... dont overhaul anything but lets think
> outside the box on how to make this game better ... 8+ hour long". The POWER guns night that sat here moved, as it was,
> to `NIGHT15.md`. HANDOFF section 4 still says "NIGHT14 = the POWER guns". **This file wins; log the conflict once.**

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves. The Editor is open on this project. Nobody answers questions and nobody playtests, and
that is tonight's whole point.

**Read these files:**
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **this file:** tonight's idea, its items, caps, order and close
- **`HANDOFF.md`:** where things stand (anchor `u3-n13`), with sections 1 (verify on arrival), 3 (the RETRY SAME SEED
  finding) and 6 (the reviews' open findings)
- **`PORT-RULES.md`:** section 6 (nothing allocated, no config lookup, string or capturing lambda per step or frame) and
  section 11 (parallel work)
- **`DESIGN-PILLARS.md`** (DRAFT): tonight serves pillars 3 (every run is readable) and 5 (proven ground)
- **`NIGHT13-LOG.md`:** how the last night ran (agents, gates, slips, the reviews)
- **The web's code named below** (read the lines an item names; the web is the reference and is read only)

**Run length: up to 12 hours from your first `date`. The close starts by hour 10:30, whatever is left.**

GG, verbatim: "want this game to be like cod zombies meets ultrakill as the main, but other games should be inspired"
(2026-10-03); "less actual testing in the game and justice more building out the architecture improving and building
more obviously based off like what we think is necessary not just building to build" (2026-10-04).

## The idea

**SKYGRIND already knows how to play itself; the port just hasn't asked it to.** The web game has a bot:
- **`src/bot/botBrain.js`** (429 lines): RIVAL's decisions, "written into an InputFrame for the shared Mover and loadout".
  It has five states: ENGAGE, CHASE (A* to the last sighting), EVADE (a dash), RECOVER and a reaction delay; aim error
  grows with the target's speed. Its numbers are already exported: `config.bot` in `PortExport/config.json` (reaction
  easy 0.4 / normal 0.25 / hard 0.15 s, aim error, strafe, evade, ...).
- **Its nav:** `src/nav/astar.js` (113), `follower.js` (220), `reach.js` (53), `navGraph.js` (396; built off-thread by
  `navWorker.js` from the arena map, its octree, the core and the Survival enemies' nav grid).
- **The web's kiosk** (`src/modes/kiosk.js` 75; `main.js` :701-740 and the flythrough camera at :2337) already plays
  "your own body on an autopilot brain (the same BotBrain + Mover RIVAL uses)" behind the title.
- **PORT-STATUS.md:** "Bot (RIVAL, Duel) | Not started".

Tonight ports that bot exactly, then points it at SURVIVAL. A bot that plays the real game, deterministically, at about
150x real time headless (the fingerprint runs 7200 steps in 0.4 s) turns into five things that make the game better
without touching how it plays:

1. **THE FARM:** hundreds of seeded bot runs a night, headless. The first balance report GG has ever had (where runs end,
   what kills, the economy per round, gun and perk use), and from then on every night's regression check. It is a report
   only: GG owns enemy design and the numbers.
2. **A living TITLE:** the attract loop (the web's kiosk, ported) and a demo-booth kiosk mode for D-38 (the demo) and
   Next Fest.
3. **THE TAPE:** every run records its inputs; any run replays bit for bit, headless. Every future bug report becomes a
   repro, and every replay doubles as a determinism test.
4. **WHAT GOT YOU:** a death recap from a flight recorder: the last seconds before DOWN, from an orbit camera, the killer
   marked. Pillar 3: every run is readable.
5. **THE ROUTE, the SHARE CARD, the GHOST and SPLITS:** RESULTS shows your run as a map, saves it as a PNG, and on a
   seed you've played, your best run races you with ULTRAKILL-style splits.

None of this is claimed as player demand; it is the night's design bet. Log it as such in the report; GG decides.

## Facts already measured (2026-10-04, NIGHT13 close at `u3-n13`; re-check them, don't trust them blindly)

- **The baseline:** `tests: 594 / 594 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 91 / 91`; harness 34 sets,
  22 replays, 0 mismatches. The Player is `Builds/Stage1`, build 5 (code `aaba74c`). Its smoke reads `[smoke] 125.2s
  DONE: all steps passed`; `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` `EQUAL to the baseline`.
- **The bench (D-34: 6.9 ms p99 on Silent):** build 5 4.08 / 3.98 / 4.45 ms, median 4.08 ms (UI-hurt); play 3.87 ms
  (build 4). `Tools/survival-bench.sh` polls the plan only before and after a run (a poller during a run was the noise);
  check idle time, CPU and GPU before each series (other projects run LLM evals by day; GG's scheduled jobs start at
  1:00 AM and spoil benches).
- **Determinism:**
  - `SurvivalSim` (no UnityEngine) is built headless by `FingerprintTests.Headless()`.
  - The composed fingerprint hashes the run every 600 steps.
  - **A second seed-1 run in the same composition differs from the first at step 600** (HANDOFF section 3): state
    carries across runs. Candidates: enemy registry order and ids, nav progress, brain timers. RETRY SAME SEED, the
    attract loop and the farm all need this fixed or avoided (a fresh composition per run).
- **The sim's non-input influences** (`SurvivalSim.cs` :103, :140, :177):
  - `onScreen` (a render-to-sim dependency kept from the web, pinned `OffScreen` in the fingerprint)
  - the settings funcs, `assistModeFor`, `breakawayCap`, `keyLabel`
  - wall-clock time in the DAILY (`nowUtc`)
  A tape must record or pin every one.
- **Input:** `IPlayerInput` (`Game/Input/IPlayerInput.cs`): Yaw, Pitch, SetLook, Captured, `SampleMove(active)`,
  `SampleCombat(active)`. `ScriptedInput` implements it. An autopilot is one more implementation.
- **The port's nav:** `Core/NavField.cs` (the enemies' flow field), `NavJumper.cs`, `CoreGrid`. **No A*, follower or nav
  graph yet.** The web's graph uses the enemies' nav grid "and its jump links" (navWorker.js).
- **Palette rules (CLAUDE.md 8):** cyan is player-usable, red-orange is enemies and danger, **magenta is RIVAL only**,
  pale violet-white is the boundary. The ghost is not RIVAL: its colour is D-53 (below).

## 0. Tonight's rules (on top of CLAUDE.md and PORT-RULES.md, which are in force)

1. **Branch `port/stage3`** from `u3-n13`. Tag `u3-n14` once, last, on the close commit.
2. **The web repo is read only,** except a new untracked `skygrind/port-export-p22/` (a copy of p20's tooling plus your
   bot recorders).
3. **Port first, then extend.** botBrain.js, astar.js, follower.js, reach.js and navGraph.js are ported line for line
   into `Skygrind.Core`, comments kept, oracle before translation (p22 sessions), the web's quirks ported as they are and
   logged. Everything Unity-only (the SURVIVAL target adapter, the shopper, the farm, the tape, the recap, the route, the
   ghost) is built **on top** and never edits the ported files.
4. **Don't overhaul anything.**
   - Every feature is additive, behind a `design.*` flag in `Design/unity-config.json` (and its byte-identical Player
     copy).
   - Existing systems get hooks, not rewrites.
   - With the new flags off, the game is u3-n13's game.
   - Movement stays bit-exact (Mover.cs untouched); golden + 22 / 22 replays after every Core commit.
   - The fingerprint stays equal at every commit. The one exception is the clean-slate fix (U3-27.2), and only if it
     changes a first run: then re-record in its own commit, with the reason.
5. **Numbers:**
   - Web numbers come from `PortExport/config.json` (`config.bot` is already there).
   - The autopilot's shopping order, the farm's settings, the recap's seconds and the ghost's alpha are new numbers:
     they go in `Design/unity-config.json` with a `_why`, or in `UnityTuning.cs` when they are render / UI only.
6. **The bot never cheats in SURVIVAL:**
   - It reads only what BotBrain reads: the target through its reaction-delayed history, the collider, the graph.
   - It never sees through walls beyond the web's line-of-sight rule, and never gets god mode or extra HP in the farm.
   - The attract loop may use the web's own god-mode line (`player.setGodMode(true)`, main.js :729).
7. **The meta never records a bot:** autopilot, farm and attract runs write no profile, records, contracts, mastery,
   DAILY or run log, the way `RunFingerprint` pins them (`noRecords`, `profileReadOnly`). Add a test.
8. **Gameplay randomness stays `Mulberry32`,** seeded per run. BotBrain's `rng` (Math.random in the web) gets its own
   stream, seeded from the run seed, so a bot run is a pure function of (seed, difficulty, build).
9. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. Write a status
   line in `NIGHT14-LOG.md` per item, with times from `date`.
10. **End every message with one status line** for the `/goal` checker: `N14-STATUS: <d> done, <s> short, <l> left (of
    13) · tests <passed>/<total> · <time from date>`.
11. **Agents:**
    - Builders split by file ownership (PORT-RULES section 11; memory `night-parallel-agents`).
    - Each works in its own worktree from a shared-types commit and checks its base commit first.
    - Each commits tests first against throwing stubs.
    - You alone drive the Editor, the Players and git, and integrate by cherry-pick with tests seen red first.
    - **Launch the two reviewers while a Player build is still unspent** (NIGHT13's lesson: their fixes needed a build).
12. **The farm is heavy:**
    - At most 4 farm processes at once.
    - Every farm process is closed by PID before any bench, and no bench starts within 10 minutes of a farm.
    - No farm after 1:00 AM if GG's jobs are running (check their CPU).

## 1. Start (U3-27.1)

1. Run `date`. Read the files at the top, CLAUDE.md first. Load `unity:unity-cli` before driving the Editor and
   `unity:ui` before UI work.
2. Run every row of HANDOFF section 1. Write `RECEIVED` in `NIGHT14-LOG.md`.
3. Read HANDOFF section 5 for GG's answers (D-34 .. D-52) and apply any that change tonight.
4. **Write tonight's items into `tasks.json`:** U3-27 plus U3-27.1 .. U3-27.13, each with the acceptance below. Never
   delete an item. The U3-14 items stay as they are (NIGHT15).
5. **Measure before building:**
   - The web bot's real API (botBrain.js :23, :172 step, :339-361 the nav override and stats).
   - Where navGraph.js needs the arena: `arena.map`, `octree`, `core`, `grid`.
   - Whether the port's map data holds what it reads.
   - How `onScreen` is called per step (count and order).
   Write the facts into the log before the shared-types commit.

## 2. The items (done = measured)

1. **U3-27.1, start:** section 1, the RECEIVED rows, tasks.json written, the measured facts.
2. **U3-27.2, a clean slate per run.**
   - **Test, red first:** in FingerprintTests, two seed-1 runs back to back in one composition print the same 12
     hashes.
   - Find what carries over, with `systematic-debugging`:
     - At the first differing checkpoint, hash each system separately; the first system that differs names the leak.
     - Fix it at the system's run-start reset: Core (golden + replays after) or SurvivalSim.
   - Done when:
     - the test is green
     - the first run's 12 hashes are still the baseline's (or re-recorded in their own commit with the reason)
     - RETRY SAME SEED gives the same run in the Player: a smoke step plays seed 1 twice and compares the hashes
3. **U3-27.3, the p22 oracle (agent R, web repo):**
   - Recorders in `port-export-p22/`, given the record format up front:
     - astar on fixed graphs
     - the follower's outputs along a path
     - reach.js's jump / drop answers
     - navGraph.js's nodes and edges for the Survival map and the duel 'spire'
     - **BotBrain sessions:** a scripted target walks a fixed path, and the bot's InputFrame, look and state are
       recorded every step (normal and hard), with Math.random seeded after the scene is built
   - Done when `verify.mjs` reports 0 mismatches, a one-ulp mutation fails, and each new file has a C# checker.
4. **U3-27.4, the bot in Core (agent N, worktree):**
   - `Core/Nav/Astar.cs`, `Follower.cs`, `Reach.cs`, `NavGraph.cs`, `Core/Bot/BotBrain.cs`, each line for line with
     the web's comments.
   - Uses Vec3, JsMath where a web number must match, Mulberry32 for `rng`, and `config.bot` through GameConfig.
   - Done when:
     - every p22 set is bit-exact
     - nothing allocates per step (a ZeroAllocTests line)
     - the graph build is timed: if over 1 s on the Survival map, build it once and cache it with a content hash
       (the web builds it off-thread), and log the time
     - PORT-STATUS's Bot row reads "Core ported (no Duel yet)"
   - `config.nav` (the graph builder's numbers) and `config.kiosk` (idleReset 60, the orbit's speed / radius / height)
     are already exported.
   - **If navGraph.js can't be built on the Survival map** (the web builds it for the duel 'spire' only): log the
     reason. The fallback for U3-27.5: plan with the port's NavField flow field toward the target and keep the
     follower's steering; the ported graph still serves the kiosk and a future Duel.
5. **U3-27.5, AUTOPILOT in SURVIVAL (agent A, worktree; `design.autopilot`):**
   - `Game/Bot/AutopilotInput.cs : IPlayerInput`: BotBrain driving the player's own Mover and loadout, exactly as the
     web's attract loop drives it (main.js :702-721).
   - Built on top (none of it edits the ported files):
     - **a target adapter:** BotBrain's `getPlayer()` returns the chosen enemy (the nearest one in line of sight, else
       the nearest by graph cost), so the bot fights the horde with RIVAL's brain
     - **a shopper:** during a break, plan to the best affordable wall buy / perk machine on a fixed priority list
       (`design.autopilot.shopping`) and hold interact; take the first MODS card; buy THE FORGE when affordable
     - **difficulty:** easy / normal / hard = `config.bot`'s tables
   - Done when:
     - the same seed and difficulty twice give the same hashes, headless
     - a measured table, logged: 20 seeds × 3 difficulties, at real HP, with the round reached, the time and the cause
       of DOWN (no pre-set bar; if normal never passes round 2, log why, then try at most two improvements to the
       adapter / shopper)
     - a test proves the meta records nothing
6. **U3-27.6, THE FARM (agent A, after 5; `-skygrindFarm <runs> <difficulty> <seedStart>`):**
   - In a Release Player, batchmode: a fresh headless composition per run (no leaks, whatever U3-27.2 finds).
   - One JSONL line per run:
     - seed, difficulty, build stamp, round reached, sim time
     - cause of DOWN (enemy kind or hazard)
     - points earned and spent per round
     - guns bought (round, slot) and kills per gun; perks / mods / THE FORGE and their rounds
     - the style rank histogram, downs, exfil reached
   - **`Tools/farm-report.cjs`** writes `Reports/FARM-n14.md` plus a self-contained `Reports/farm-n14.html` (inline SVG,
     no network) with these charts:
     - round reached per difficulty
     - a survival curve
     - points per round
     - kills by gun
     - downs by enemy kind
     - perk timing
     - **A "farm diff" mode** compares two farms (what NIGHT15's guns will use).
   - Done when ≥ 300 runs across the three difficulties are in, the report renders (look at it), and the numbers are
     consistent (the JSONL count equals the report's). Findings are written as **observations for GG**, never as
     changes.
7. **U3-27.7, the KIOSK and a living TITLE (agent K, worktree):**
   - **Port kiosk.js** behind `-skygrindKiosk`, the web's `?demo=1`: the control hints panel (with the live bindings'
     labels), 60 s without input goes to TITLE, PAUSED has no QUIT. The web's numbers are in `config.kiosk`.
   - **The attract loop** (Unity-side adaptation: no Duel arena yet): the AUTOPILOT plays SURVIVAL behind TITLE on a
     seeded run, with god mode as the web's attract has it and the meta silent (rule 7).
   - **The web's flythrough camera** (main.js :2337): "a slow orbit around the two fighters, a little above them" (here:
     the autopilot and its target).
   - It runs in the kiosk always, and on a normal TITLE after `design.attract.idleSeconds` with no input (default 30;
     0 = off).
   - Done when any input returns to TITLE at once and PLAY then starts a clean run (U3-27.2: a smoke step plays the
     attract for 20 s, then PLAY, and compares the run's first checkpoint with a fresh run's). Capture: TITLE over the
     attract.
8. **U3-27.8, THE TAPE (agent T, worktree; `design.tape`):**
   - Every run records a compact tape:
     - a header: format version, build stamp, fingerprint baseline id, seed, setup, the sim-relevant settings
     - per step: `InputFrame` + `CombatInput` + yaw / pitch, and every non-input answer the sim consumes in call order
       (`onScreen` bits; anything else found in U3-27.1), or proof that it's constant
   - `RunTape.Save / Load`; tapes in `runs/` next to the run logs, keeping the last 20.
   - `-skygrindReplay <tape>` replays it headless and prints `EQUAL` or the first differing step. COPY REPORT gives the
     last tape's path; an unhandled exception writes the tape.
   - Done when:
     - a headless test tapes a scripted run and an autopilot run, replays both into fresh compositions, and they are
       equal at every checkpoint
     - the Player smoke tapes a short run and `-skygrindReplay` on that file prints `EQUAL`
     - bytes per minute is measured and logged
     - an old-format tape is refused with a line, never a crash
9. **U3-27.9, WHAT GOT YOU (agent V, worktree; `design.recap`, a SETTINGS row RECAP on / off):**
   - **The flight recorder:** a preallocated ring of the last `design.recap.seconds` (default 6) of view state:
     - the player's feet and look
     - each live enemy's id, kind, position, yaw and state
     - projectiles, pickups and hits
   - It allocates nothing after boot (a ZeroAllocTests line) and records off the sim's events / state after each step.
   - **On DOWN:** "WHAT GOT YOU" replays the last ~4 s from an orbit camera using the existing views. The killer gets a
     red-orange outline and its name; the last 0.5 s plays at a slow rate; any key or B skips; then DOWN / RESULTS as
     now.
   - Done when 2 captures exist (a husk swarm, a spitter glob), the bench's play scenario is within 0.3 ms of before,
     and the flag off is the old DOWN.
10. **U3-27.10, THE ROUTE and the SHARE CARD (agent V2, worktree; `design.route`):**
    - A whole-run coarse track: positions every 0.25 s plus kills, downs, round starts and exfil, preallocated for 60
      min.
    - **RESULTS shows THE ROUTE:**
      - a top-down map of the arena outline
      - the path coloured by speed on a cyan ramp
      - kills as small dots and the DOWN mark in red-orange
      - round ticks
    - **SAVE CARD** writes a PNG to `Pictures/SKYGRIND/` (seed code, round, score, rank, time, top gun, the route, the
      build stamp); **COPY SEED** copies the seed code.
    - Done when the PNG is written and you have looked at it, plus 1 capture of RESULTS.
11. **U3-27.11, the GHOST and SPLITS (agent V2, after 10; `design.ghost`):**
    - On a seed you've finished before (the DAILY first), your best run's coarse track plays as a faint runner (D-53's
      colour) with a short fading trail.
    - Round splits against that best: a one-line "ROUND 5 · −12.4 s" at each round's start (ULTRAKILL's clock on
      CoD's rounds).
    - Best tracks are stored per seed in the profile folder, capped at 50 seeds.
    - Done when there is 1 capture, the smoke step "a second run on the same seed shows the ghost and a split" passes,
      and the flag off is the old HUD.
12. **U3-27.12, the bot as a tool for every later night:**
    - The bench gets an `autopilot` scenario: real fights instead of the scripted strafe. Run its first series and keep
      the old scenarios.
    - The smoke gets an autopilot step: 2 rounds in the Player, deterministic.
    - `PORT-RULES.md` gets a short section: "after a gameplay change: a farm diff against the last report".
    - Done when the series is logged (3 runs, median) and the smoke step passes.
13. **U3-27.13, the close:** section 6.

## 3. Caps

- Play checks at most 3; captures at most 10 (TITLE over the attract, the kiosk hints, the autopilot mid-fight, WHAT GOT
  YOU ×2, RESULTS with THE ROUTE, the GHOST with a split, and 3 spare). Look at every capture.
- Player builds at most 5. Bench runs at most 18 (series of three, decided by the median, every run reported; the bar's
  UI-hurt series last, on the final build).
- Farm: at most 4 processes, at most 3 hours of farm wall time in total, logged per batch.
- Builders at most 7 (R, N, A, K, T, V, V2); reviewers at most 2.
- **A cap reached makes its item SHORT with the count. Caps are caps.**

## 4. The shared-types commit (before any agent starts)

It pre-places every agent's contract so nobody edits the same lines:
- `IBotBrain` stub (Core): `Step(InputFrame, dt)`, `Look`, `State`, `Reset(difficulty)`.
- `INavGraph` stub.
- `AutopilotInput` and `RunTape` stub signatures, plus `FlightRecorder` record structs.
- `SurvivalSim` partial hooks:
  - `OnStepEnd` (the recorder, the tape, the route track)
  - `InputSource` (swap the IPlayerInput for the autopilot / a tape player)
  - `MetaSilent` (rule 7)
- Ui partial hooks and pre-placed buttons: RESULTS SAVE CARD / COPY SEED, the DOWN recap host, the TITLE attract layer.
- The `design.autopilot / attract / tape / recap / route / ghost` keys with `_why`, all present.
- `PlayerChecks` modes `farm` and `replay` (stubs).
- Topic-named members only (PORT-RULES 11: the `KeyRow` clash was seen twice).

## 5. Order and time boxes (minutes)

| # | Item | Box |
|---|---|---|
| 1 | U3-27.1 start + the measurements | 30 |
| 2 | the shared-types commit; launch R, N, T, V, V2, K (A after N's stubs) | 30 |
| 3 | U3-27.2 clean slate (the lead, while agents build) | 75 |
| 4 | U3-27.3 oracle (R) → U3-27.4 bot port (N) | 75 + 120 |
| 5 | U3-27.8 the tape (T) | 90 |
| 6 | U3-27.9 recap (V) | 75 |
| 7 | U3-27.10 route + card (V2) | 60 |
| 8 | U3-27.5 autopilot (A) | 90 |
| 9 | U3-27.7 kiosk + attract (K; the attract waits for A) | 75 |
| 10 | U3-27.6 the farm (A) + farm batches in the background (not near benches) | 75 + batches |
| 11 | U3-27.11 ghost + splits (V2) | 45 |
| 12 | U3-27.12 tools; the reviewers launched with builds left | 30 |
| 13 | U3-27.13 close (starts by hour 10:30) | 60 |

- **An item 25% over its box:** ship what works behind its flag, mark it `in_progress` with what remains, move on, and
  come back after the others.
- **Cut order** (only if the night runs short): the ghost's splits, then the ghost, then the share card PNG (keep THE
  ROUTE), then the kiosk's hints panel (keep the attract), then the recap's slow last half-second.
- **Never cut:** U3-27.1, .2, .3, .4, .5, .6, .8, .13; the rule that the flags off give u3-n13's game.

## 6. Close (U3-27.13)

1. **The final Release build**, then `git diff --stat` and the restores (TMP fallback, `Mobile_RPAsset.asset`).
   - The smoke DONE with tonight's steps.
   - `-skygrindGolden` 0 mismatches.
   - `-skygrindFingerprint` equal to the baseline.
   - `-skygrindReplay` on a smoke tape: `EQUAL`.
   - The final Silent series on an idle machine (UI-hurt for the bar, plus the new autopilot scenario).
2. **`tasks.json`:** U3-27.* with status and evidence (a commit hash, an output line, a time). Never delete an item.
3. **Docs:**
   - **PORT-STATUS.md:** the Bot row, nav, the kiosk, the new Unity-only systems and their flags.
   - **PORT-PLAN.md** ticks.
   - **PORT-RULES.md:** the farm-diff section, plus any rule learned twice.
   - **`NIGHT14-REPORT.md` for GG**, one page:
     - what changed
     - the farm's top 5 observations, with a link to the HTML
     - the numbers, and what is short and why
     - a 10-minute checklist: watch the attract, die once and watch WHAT GOT YOU, save a card, replay a tape, open
       the farm report
     - one question per new screen
   - **`HANDOFF.md`** rewritten with the `handoff` skill (anchor `u3-n14`; D-53 added).
   - **`NIGHT15.md`** refreshed: anchor, facts, and the farm and tape as tools for the guns.
4. **Memory:** a lesson seen twice becomes a memory note. CLAUDE.md changes go to HANDOFF's pending decisions.
5. **Tag `u3-n14` once** on the close commit. `git status --short` shows only `UnityConnectSettings.asset` (if modified)
   and the untracked NIGHT7.md .. NIGHT15.md.
6. **Print the close table, then the line `NIGHT14 CLOSED`:**

| Item | Status | Evidence |
|---|---|---|
| U3-27.1 .. U3-27.13, one row each | DONE or SHORT | DONE: a commit hash or an output line from tonight. SHORT: the measured reason (a cap, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` reads 10.5 hours or more after the first) |

## 7. Decisions tonight adds (defaults shipped; GG decides later in HANDOFF)

- **D-53, the ghost's colour:**
  - The default is the player's cyan at `design.ghost.alpha` (0.3) with a pale trail.
  - Rejected: magenta (RIVAL only, CLAUDE.md 8), red-orange (danger only).
- **D-54, the attract on a normal TITLE:** on after 30 s idle (`design.attract.idleSeconds`); the web runs it only in
  the kiosk.
- **D-55, the farm as a nightly gate:** report-only tonight. Recommendation: a farm diff after any gameplay change.
- **D-56, tapes kept:** the last 20 runs (the run log's rule); sharing tapes between players is not tonight.

## 8. Mistakes not to repeat (NIGHT7-13)

1. A retried eval re-takes captures: a start-something eval keeps its guard marker and is called with `unity command
   eval_file` directly. Never change screens in a capture's frame; look at every capture.
2. The tag moved (NIGHT12): tag last, once.
3. Benching at the end with no build left, or benching beside a farm: bench mid-night with a build in hand, every farm
   process closed by PID first.
4. Patch scripts through the shell: write `.cjs` / `.cs` patch files with the Write tool, then run them; no heredocs or
   `node -e` for code.
5. A config path, string or capturing lambda per step or frame (PORT-RULES 6). The recorder, the tape and the route
   allocate nothing after boot.
6. A prompt's fact taken on trust: read the code (the web bot's API, the graph's inputs, the onScreen calls).
7. Builds rewrite files: `git diff --stat` after each build; restore `Mobile_RPAsset.asset`; Play mode rewrites the TMP
   fallback asset.
8. A file written into `Assets/` before its types exist: the Editor compiles it at once; stage it outside.
9. `SurvivalGame.Respawn` or `run.DebugSkipTo` every frame in a check.
10. Two agents' generic names in one partial class (`KeyRow`, NIGHT11 and NIGHT13): topic-named members.
11. Reviewers launched after the last build (NIGHT13): launch them while a build is unspent.
12. A night's goal that can become impossible: an item that can't be met is SHORT with its reason, never a reason to
    loop.

**Caps are caps; SHORT with a reason is a finished row.** End with the close table and `NIGHT14 CLOSED`.
