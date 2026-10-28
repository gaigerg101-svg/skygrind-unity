# NIGHT16.md: SKYGRIND in Unity (the POWER guns and THE FORGE tiers), no human in the loop, at most 12 h

> **QUEUED (2026-10-05 11:10 AM):** behind the new map and the first-fifteen-minutes night (`QUEUE-first-fifteen.md`).
> When scheduled, rename it to its night number and update its anchor, tag and farm baseline.

> **Moved from NIGHT15.md on 2026-10-05 (~9:45 AM), on the NIGHT14 session's recommendation; GG may swap the two files
> back.** NIGHT15 now pays NIGHT14's debts (the smoke residual, the captures) and fixes the AUTOPILOT so the farm can
> judge new guns: on u3-n14 the bot never stood inside a HOLD ring (its own test used half the ring's radius), never fired
> a rocket or a rail it bought, and never called an exfil, so a farm diff of these guns against `Reports/farm-n14/` would
> measure the bot. Run this after NIGHT15: anchor `u3-n15`; the farm baseline is NIGHT15's (`Reports/farm-n15/`); add
> each new gun to `design.autopilot.guns` (or the damage-at-distance choice NIGHT15 builds) so the bot fires it.

> **Moved from NIGHT14.md on 2026-10-04 (9:50 PM)**, when GG gave the next night to a creative prompt ("THE GAME PLAYS
> ITSELF", now NIGHT14.md). Run this after NIGHT14: its anchor becomes `u3-n14`, and the facts below were measured at
> `u3-n13` (re-check them). NIGHT14 adds two tools this night should use: the AUTOPILOT farm (`-skygrindFarm`: run it
> with the POWER guns owned and compare against NIGHT14's farm report) and the run tape (`-skygrindReplay`).

> **Refreshed at the NIGHT14 close (2026-10-05).** The facts below were measured at `u3-n13`; NIGHT14 changed SurvivalSim
> (the hooks, CleanSlate), Ui (the RESULTS buttons) and PlayerChecks, and NIGHT15 changes the bot and the HUD (re-check).

> **Refreshed at the NIGHT13 close (2026-10-04 evening, anchor `u3-n13`).** NIGHT13 built the base these guns land on:
> the composed fingerprint and `SurvivalSim` (the sim without Unity, headless in a test), the input layer and the
> gamepad, the hotbar and the radial (U3-14.7 is done as U3-26.9), save-and-quit on a run snapshot (U3-14.9 is done as
> U3-26.10), the perf switches (the bench at 4.08 ms against the 6.9 ms bar). Two close reviews' open findings are in
> HANDOFF section 6 (RESUME's round-1 messages first; fix them when a gun's work touches those files). What is left here: the POWER guns
> (U3-14.2 .. U3-14.6), THE FORGE tiers (U3-14.8) and the close (U3-14.10), plus each new gun's place in the run snapshot,
> the hotbar and the bench. Research: `NIGHT14-RESEARCH.md` (sections 1 and 3).

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **this file:** tonight's job, caps, order and close
- **`HANDOFF.md`:** where things stand (anchor `u3-n15`)
- **`PORT-RULES.md`:** section 6 (no config lookups, strings or capturing lambdas per step or frame) and section 11 (NEW:
  parallel work: one owner per file, topic-named members in shared partial classes, the integration compile first)
- **`tasks.json`:** U3-14.1 .. U3-14.6, U3-14.8, U3-14.10 (U3-14.7 / U3-14.9 point to U3-26.9 / U3-26.10)
- **`NIGHT14-RESEARCH.md`** sections 1 (the eight POWER guns: config, guns.js lines, projectiles, presets) and 3 (THE
  FORGE tiers); **`NIGHT13-LOG.md`** for how last night ran (agents, gates, slips)

**Run length: up to 12 hours from your first `date`; the close starts by hour 10:30.**

GG, verbatim: "want this game to be like cod zombies meets ultrakill as the main, but other games should be inspired"
(2026-10-03). `DESIGN-PILLARS.md` (DRAFT) is the scope filter until GG edits it: pillar 2 (Zombies rituals, ULTRAKILL
rewards) and pillar 5 (grow by content on proven ground) are tonight's.

**Goal (the morning):** GG buys each of the eight POWER guns (RATTLER, TORCH, BOUNCER, ZAPPER, DISCUS, VORTEX, CHOIR,
PRISM) on the ARMORY wall (CHOIR on the back wall) and each plays as on the web, its special and momentum variant
included; they show in the hotbar and the radial with their names; the box rolls them; THE FORGE can go to tier 3 (a
flag, off by default); SAVE & QUIT at a break with a POWER gun in hand resumes with it. With the new flags off, one
player plays exactly the web's game.

## Facts already measured (2026-10-04, NIGHT13 close at `u3-n13`; re-check them, don't trust them blindly)

- **The baseline:** `tests: 594 / 594 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 91 / 91`; harness 34 sets,
  22 replays, 0 mismatches; the Player (`Builds/Stage1`, build 5, code `aaba74c`, stamp `aaba74c-dirty`: docs were
  uncommitted): smoke `[smoke] 125.2s DONE: all steps passed` (NIGHT13's steps 13-17: the run log, save and resume, the
  radial, a rebind, a virtual pad); `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` `EQUAL to the baseline`.
- **The bench (D-34: 6.9 ms p99 on Silent; the lead sets Silent for a series and restores the plan):** build 5, three
  60 s runs: 4.08 / 3.98 / 4.45 ms, median 4.08 ms (build 4: median 3.98 ms); play 3.87 ms (build 4). survival-bench.sh polls the plan only before and after
  a run now (the old 2 s poller was most of last week's 7.19 ms). The breakdown (`-skygrindBench` prints it, plus CSVs):
  a worst-1 % frame is ~7 ms of engine main-thread work outside our scripts; our scripts 0.5-0.6 ms; one sim step 0.3 ms.
  **Eight POWER guns add projectiles, arcs and heat to the play scenario: run `-skygrindBench 60 play` with them** and
  compare (CHOIR and TORCH on 30 enemies were never measured).
- **The composed fingerprint (`SimFingerprint`, `FingerprintBaseline.cs`, `FingerprintTests`):** a seeded run on a fixed
  input script, a hash every 600 steps, run headless and in the Player. **Adding guns to `Loadout.Fires` changes the box's
  pool and so the run: re-record the baseline in its own commit** (one Player run + the headless test, both equal),
  with the reason in the log; every refactor commit before or after must print the same hashes.
- **Where things live now (NIGHT13):**
  - `SurvivalSim` (`SurvivalSim.cs`, `.Run.cs`, `.Save.cs`; no UnityEngine): every Core system, built in Init's old
    order; `Step()` is simulate(); the views stay in `SurvivalGame*` (partials Init / Flow / Meta / Render / Debug /
    Save / Fingerprint), which forwards its old fields. A new Core system (Guns) is built and stepped in SurvivalSim, at
    weapons.js :229-235's place inside `Loadout.Step` (Core).
  - The run snapshot: `IRunState` on 34 systems, `RunSnapshot`, `SurvivalSim.SnapshotSections()`; `RunRestoreTests` (3
    seeds) proves a restore at a break equals the uninterrupted run. **Guns' state (heat, cooldowns, CHOIR's spin, the
    burst) and the new projectile kinds (grenades, discs, vortexes, darts) must join the snapshot** (or be cleared at a
    break and logged), and RunRestoreTests must stay green with a POWER gun owned.
  - The hotbar / radial (`UiHotbar.cs`, `UiRadial.cs`) take any owned gun: the glyph is the name's first word, 5 letters
    (hud.js's fallback); the web's SVG icons are not ported.
  - Input: `CombatReader` / `WeaponSelect` (1-0, the wheel cooldown, quick swap, the Tab radial), the pad's Y / d-pad cycle;
    the ADS / SPECIAL routing in `Core/Combat.cs` is unchanged since u3-n12 (`git diff u3-n12..u3-n13`: only the
    IRunState Save / Load added), so the ADS gap below still holds.
  - Agents worked in worktrees with `node Tools/wt-csproj.cjs <worktree>` and `dotnet build`; the lead integrated by
    cherry-pick with tests seen red first (NIGHT13-LOG "Agents landing").
- **The guns (measured at u3-n11, re-checked at the NIGHT12 close):**
  - `Loadout.Fires` is revolver, shotgun, gauntlet, nailgun, rocket, rail; `Projectiles.Launch`, `Volley`, `Airburst`,
    `Recall`, `VortexAlt` are stubs.
  - The web: `combat/guns.js` (636 lines: `createGuns` :39-636, `step` :441-581, `meter` :584-594; weapons.js calls it at
    :229-235); Math.random draws rattlerRound 2 / round (:86), choirRound 2 / round (:222), projectiles.js `arm` 1 per
    projectile (:222), `volley` 3 per dart (:283-285): the port splits the web's stream into Loadout.rng and
    Projectiles.rng, so a golden session drawing from both needs one shared stream injected (a test-only hook).
  - No vitest covers guns.js or projectiles.js; the oracles are the browser presets `newGuns` (powerScripts.js :245-252,
    pass :888-894), `gunSpecials` (nightScripts.js :127, pass :466), `momentumVariants` (:227, pass :472).
  - Wall slots (wallBuys.js :167-182): shotgun 46.6, rattler 44.6, torch 42.6, nailgun 40.6, bouncer 38.6, zapper 36.6,
    rocket 34.6, discus 32.6, vortex 30.6, rail 28.6, prism 26.6; CHOIR on `armoryBack` (z 48, x 17.7): SurvivalMap
    doesn't parse armoryBack; `SurvivalSim.AddWallBuys` and `WallBuys` loop armoryWallZs only.
  - Also missing: `bounced` (RICOSHOT) through `Loadout.Damage` and the payloads; `StepAds` zooms the KESTREL only while
    routeSpecial skips every `ads.weapons` gun [U: not played]; `Loadout.Meter` covers nailgun / rail / rocket (the hotbar's
    weapon bar reads it); the KESTREL / THUNDERCLAP specials and variants (weapons.js :557-570, :585-656, :210-212).
- **THE FORGE:** the web has one tier (forge.js, 5000, x1.8). Tiers (5k / 15k / 30k, Cold War / BO6) are new design with
  no numbers in our data: a short research pass into `design.forge.tiers` with a `_why`.
- **The machine:** GG's scheduled jobs start at 1:00 AM; other projects' LLM evals can run by day (NIGHT13: 84-91 % GPU
  at 3:30 PM): check CPU / GPU before every bench series.

## 0. Tonight's rules (on top of CLAUDE.md and PORT-RULES.md, which are in force)

1. **Branch `port/stage3`** from `u3-n15`. Tag `u3-n16` once, last, on the close commit.
2. **The web repo is read only** except a new untracked `skygrind/port-export-p21/` (a copy of p20 plus your recorders).
3. **Port, don't improve:** guns.js and projectiles.js's kinds line for line, comments kept; the oracle (p21 sessions)
   before the translation; the presets' pass criteria as tests seen red. Port the web's bugs as they are (the airburst
   payload `{owner: n}`, projectiles.js :320; the dart life `stepsFor(3)`, :569) and log them.
4. **New design tonight only:** THE FORGE tiers (`design.forge.tiers`, OFF by default: D-28). With the flags off, one
   player plays exactly the web's game.
5. **Numbers:** every web number from `PortExport/config.json`; a missing key is re-exported in p21.
6. **Movement stays bit-exact** (Mover.cs untouched). After every Core commit: every golden test, 22 / 22 replays.
7. **Proof by machine first:** the fingerprint for refactors (a deliberate re-baseline only when guns join Fires), the
   suite, RunRestoreTests, the smoke; Play checks and captures are few.
8. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue; a status line in
   `NIGHT16-LOG.md` per item with times from `date`.
9. **End every message with one status line** for the `/goal` checker: `N15-STATUS: <d> done, <s> short, <l> left (of 8)
   · tests <passed>/<total> · <time from date>`.
10. **Agents:** builders split by file ownership (PORT-RULES section 11; memory `night-parallel-agents`), each in its own
    worktree from a shared-types commit, checking its base commit first; you alone drive the Editor, the Players, git.

## 1. Start (U3-14.1)

1. Read the files at the top, CLAUDE.md first. Load `unity:unity-cli` before driving the Editor.
2. HANDOFF section 1, every row; `RECEIVED` in `NIGHT16-LOG.md`.
3. HANDOFF section 5: GG's answers (D-28, D-34, D-38 .. D-47); apply any that change tonight.

## 2. The items (done = measured)

1. **U3-14.2, the p21 oracle:** projectile sessions (grenade, disc, vortex, darts) and real-loadout sessions for the eight
   guns with their specials and variants (Math.random seeded after the scene is built; one shared stream); `verify.mjs`
   0 mismatches; every new file consumed by a checker; a one-ulp mutation fails.
2. **U3-14.3, Projectiles' POWER kinds:** projectiles.js :182-197, :253-265, :275-291, :311-357, :374-376, :566-767 line
   for line; the p21 projectile sessions bit-exact; a ZeroAllocTests line; IRunState for the new kinds (or cleared at a
   break, logged).
3. **U3-14.4, Core/Guns.cs:** guns.js line for line at weapons.js :229-235's place; `bounced` through Damage; StepAds over
   `ads.weapons`; the meters; `Loadout.Fires` + 8; the p21 loadout sessions bit-exact; the presets' pass criteria as
   tests; no allocation per step; IRunState for Guns; **the fingerprint re-baselined in its own commit**; RunRestoreTests
   green with a POWER gun owned.
4. **U3-14.5, weapons.js's remainder:** the KESTREL's pierce variant, the THUNDERCLAP's cone variant + coneWave, compress +
   double blast, the pump; sessions or tests.
5. **U3-14.6, the guns' Game half:** the eight models (weaponModels.js :215-415, each taking `BlockyKit.Finish`),
   viewmodel rigs (:449-510), projectile views, gunFx + fx/arcs.js, SurvivalMap `armoryBack` + the back wall (in
   SurvivalSim.AddWallBuys and WallBuys); **a smoke step buys and kills with each of the eight**; the bench's play
   scenario with a POWER gun.
6. **U3-14.8, THE FORGE tiers** (`design.forge.tiers`, off = the web's single tier): numbers from a short research pass
   with URLs in the `_why`; the "+" names per tier; tests on and off; golden unchanged.
7. **U3-14.10, the close:** section 6.
8. (U3-14.7 and U3-14.9 were built in NIGHT13 as U3-26.9 / U3-26.10: re-check them with the new guns, nothing more.)

## 3. Caps

- Play checks at most 3; captures at most 4 (the wall with every gun, a ZAPPER chain or a VORTEX pull, the hotbar with
  10+ guns, the FORGE at tier 3); Player builds at most 3; bench runs at most 12 (series of three, decided by the median,
  every run reported); builders at most 6, reviewers at most 2. Caps are caps: a retake that would cross one is short.

## 4. Order and budgets (lead minutes)

| Order | Item | Budget |
|---|---|---|
| 1 | U3-14.1 start | 15 |
| 2 | shared types (Guns API stubs, projectile kinds, the IRunState hooks), agents launched | 30 |
| 3 | U3-14.2 the p21 oracle (a recorder agent; you: checkers) | 35 |
| 4 | U3-14.3 Projectiles' POWER kinds (a Core agent) | 20 |
| 5 | U3-14.4 Core/Guns.cs (+ the fingerprint re-baseline) | 110 |
| 6 | U3-14.5 weapons.js's remainder | 35 |
| 7 | U3-14.6 the guns' Game half (a Game agent; you: wiring, the smoke, the bench) | 50 |
| 8 | U3-14.8 THE FORGE tiers | 35 |
| 9 | U3-14.10 close | 60 |

- **Cut order:** THE FORGE tiers, then the arcs drawn simply, then U3-14.5. **Never cut:** U3-14.1, .2, .3, .4, .6, .10,
  and the rule that the flags off give the web's game.

## 5. Mistakes not to repeat (NIGHT7-13)

1. Patch scripts through the shell: write `.cjs` / `.cs` files with the Write tool, then run them; no heredocs or
   `node -e` (NIGHT13 slipped once).
2. A start-something eval without a guard that outlives it; unqualified names or `using` lines in an eval.
3. A smoke check that waits a number of frames: a Player runs ~400 fps (NIGHT13: 40 frames were 0.1 s): wait in real time.
4. A claim in a commit message or doc that wasn't checked (NIGHT13: "the unused click read removed" while the split had
   copied it): grep before you write it.
5. Two agents' generic names in one partial class (NIGHT13's KeyRow): PORT-RULES section 11.
6. A bench on a busy machine, or one run deciding anything: check CPU / GPU first; three runs, the median.
7. Builds rewrite files: `git diff --stat` after each build; restore `Mobile_RPAsset.asset`; Play mode rewrites the TMP
   fallback asset.
8. The tag before the last docs fix: tag once, last.

## 6. Close (U3-14.10)

1. **The final Release build**, then `git diff --stat` and the restores; the smoke DONE with tonight's steps;
   `-skygrindGolden` 0 mismatches; `-skygrindFingerprint` equal to the (re-recorded) baseline; a Silent series of the
   UI-hurt bench and one `play` run with a POWER gun.
2. **`tasks.json`:** U3-14.* with status and evidence; never delete an item.
3. **Docs:** PORT-STATUS.md (combat, projectiles, the guns' events with the web's names), PORT-PLAN.md ticks,
   ZOMBIES-ULTRAKILL-RESEARCH.md section 3 (the FORGE tiers), PORT-RULES.md (a rule learned twice), `NIGHT16-REPORT.md`
   for GG (one page; a question per gun; the farm diff against `Reports/farm-n15/`), `HANDOFF.md` rewritten with the
   `handoff` skill (anchor `u3-n16`), `NIGHT17.md` (the enemy roster: charger, drone, lancer, warden, affixes, the round
   budget; HEAT rungs 4-6) re-checked against the code.
4. **Memory:** a lesson seen twice becomes a note; CLAUDE.md changes go to HANDOFF's pending decisions.
5. **Tag `u3-n16` once** on the close commit; `git status --short` shows only `UnityConnectSettings.asset` (if modified)
   and the untracked NIGHT*.md; agents' worktree folders removed.
6. **Print the close table** (one row per item: DONE with a commit hash or an output line, or SHORT with its measured
   reason), then the line `NIGHT16 CLOSED`.

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

- **The oracle before the guns;** guns.js and projectiles.js line for line against it.
- **The flags off give the web's game;** golden + 22 replays at every Core commit; the fingerprint re-baselined once,
  in its own commit, when the guns join Fires.
- **Write only what was measured.** Caps are caps; SHORT with a reason is a finished row.
