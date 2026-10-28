# NIGHT9.md: SKYGRIND in Unity, stage 3 night 1 (the oracle and the arsenal), no human in the loop, at most 12 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **this file:** tonight's job
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **`PORT-RULES.md`:** how to translate a web module
- **`PORT-PLAN.md`:** the whole port, night by night
- **`PORT-RESEARCH.md`:** why it is done this way
- **`HANDOFF.md`:** where things stand
- **`tasks.json`:** items U3-xx

**Run length: up to 12 hours from your first `date`. Hard stop at 12:00.** Stop earlier only when the definition of done
(section 2) is met.

GG asked, verbatim: "great we need this next prompt to basically add everything that was built on the browser version
(WHICH SHOULD HAVE BEEN UNITY VERSION) and switch over to unity, more weapons, better movement, bosses, other gamemodes,
eveerything that was built, style meter, better UI. we need to resrecah theorughly and find best way to tarnsfer all of
that and rebuilt ON THE UNITY VERSION. start with resrecah create plan and next prompt, look and see if people have
done this before, browser game to unity".

**Why tonight looks like this:**
- The research (PORT-RESEARCH.md) found one pattern in every port that finished. It held for browser → Unity
  (Vampire Survivors), compilers (TS → Go), and AI-driven migrations at Google, Airbnb and Anthropic:
  - **keep the old build as the oracle**
  - **move in small units in dependency order**
  - **check every unit by machine**
- Ports that rewrote without an oracle slipped for years or shipped missing features.
- About 22,700 web LOC remain, over 11 more nights (PORT-PLAN.md section 3).
- Tonight builds the base the other ten stand on:
  - **the oracle**: every exported golden vector consumed, new vectors, the 6 trial routes
  - **determinism**: V8's maths in JsMath, the golden vectors inside the built Player
  - **the two combat systems everything after depends on**: the arsenal and momentum
  - **quick wins**: the V-lock and lunge, and four perks whose systems already exist

**Goal (the morning):** GG plays the Player and the run feels like the web's:
- FRESH weapons hit harder, a swap primes, mixing families CROSSes.
- Speed is damage: a fast gauntlet hit sends an impact wave.
- V locks on.
- Nine perk machines stand in THE GRINDER and around the map.

`NIGHT10.md` is written for tomorrow.

## Facts already measured (2026-10-02; re-check them, don't trust them blindly)

- **The baseline:**
  - At `u2a-demo` (`db0ec85`): `tests: 237 / 237 passed`, `replays: 16 / 16`, `golden tests: 55 / 55` (20:01).
  - The Player smoke `DONE: all steps passed` (19:54).
  - The bench median 3.35 ms, worst 1% 8.37 ms, Balanced on AC.
  - The plan commit `d3c9249` ("docs: the full-parity plan ...") sits directly on top of `u2a-demo`, on `port/stage2`.
- **Vectors exported but not consumed by any test:** `weaponMath`, `arsenalMath`, `momentumMath`, `styleCore`,
  `styleSim`, most of `gunMath` (only `sweepTargets` is used), and most of `spawnMath` (only mulberry32). They are in
  `PortExport/vectors/`.
- **The export scripts:**
  - Where: `C:\Users\gaige\dev\skygrind\port-export-p17\scripts\` (lib, export-config, vectors, trajectories, maps,
    verify, all). They import the real `src/` and run headless in Node; `EXPORT_DIR` is the script folder's parent.
  - The recipe for a new set: PORT-RESEARCH.md §0.3. Stateful systems record as styleCore-style sessions
    `{op, args, result, events, after}`.
  - The trial routes (scripted Mover runs on the real maps, with jump pads): `skygrind/tests/support/trialRoutes.js` and
    `trialRunner.js`.
- **The Player's data:** `Assets/StreamingAssets/PortExport/` holds only config.json and maps/survival.json
  (`SKYGRIND/Sync Port Data`, Assets/Editor/SurvivalSetup.cs:78). Sizes: vectors 0.95 MB, trajectories 2.3 MB, maps 4.9 MB.
- **System.Math in Core outside JsMath:** Atan2 13, Exp 7, Tan 6, Acos 6, Pow 3, Asin 2, Atan 1, Tanh 1 (CombatMath:262);
  Log10 1 is UI formatting (Settings.cs:111). V8's versions are in `src/base/ieee754.cc` (fdlibm); check how V8's
  `Math.pow` is implemented before porting it (it is not the same path as the others).
- **The arsenal on the web:**
  - `src/combat/arsenal.js` (236) + `arsenalMath.js` (105); config `weapons.arsenal`: primedWindow 1, primedBonus 0.35,
    crossWindow 3, crossMult 1.5, statusTime burn 3 / shock 2.5 / pull 1.5 / stuck 6 / stagger 1.5 (config.js:513-518).
  - `weapons.js:79` creates it; damage applies its tags at `weapons.js:348-410`.
  - main.js:
    - `createLoadout(..., { arsenal: true, momentum: true })` at line 362
    - enabled in every mode but the sandbox, at lines 936-939
    - the style meter's `getFresh` from the arsenal, at lines 495-496 (`styleMeter.js:44-46` passes `fresh` into
      styleCore)
- **FLIP:** `weaponMath.js:97-113`: swap time and `arsenal: { recover, primedBonus }`. Unity's `Forge.status` hook is
  null and waits for the arsenal.
- **Momentum:**
  - The modules: `combat/momentum.js` (87), `momentumMath.js` (68), `impact.js` (160), `ui/momentumArc.js` (49).
  - main.js: the momentum hitstop at lines 1546-1547; the arc at lines 2371-2372.
  - In damage: melee x lerp(1, 3, M), guns x lerp(1, 1.35, M); explosions grow their radius instead (weapons.js:348-356).
- **Aim assist:** `aimAssist.js`: the V lock (lockOn / lockOff, lines 30 and 201), `homingTarget` (486: waits for
  projectiles, N10), `lungeTarget` (497).
- **Perks:** all 14 perks' numbers are in `Core/Perks.cs`. BOUNTY, DEADEYE, FLOW and SLIPSTREAM need no new system;
  their machines were never placed (`Perks.Ported`, PerkMachines.cs).

## 0. Tonight's rules (on top of CLAUDE.md and PORT-RULES.md, which are in force)

1. **Branch:** create `port/stage3` from the plan commit `d3c9249`. Every commit tonight goes there. Tag `u3-n9` at
   the end. `u2a-demo` stays as it is: Demo Day (Oct 7) plays it.
2. **The web repo is read only** (CLAUDE.md rule 2). The one thing allowed is a new untracked folder
   `skygrind/port-export-p18/` (a copy of `port-export-p17/` plus your recorders), run with `node` from the web repo root.
   No commits there, no branch switch, no edits to tracked files.
3. **Port, don't improve.**
   - Each module line for line, comments kept.
   - Its web tests ported and seen red.
   - Its vectors consumed.
   - A difference from the web is a bug until a vector proves otherwise.
   - Ideas go to `IDEAS.md`, never into the port.
4. **Numbers:**
   - Every web number from `PortExport/config.json`.
   - A Unity-only one in `Design/unity-config.json` with a `_why`.
   - None typed in code (CLAUDE.md rule 5).
   - If tonight's `config.json` lacks a key a module needs, re-export it in p18 and sync; never hand-add it.
5. **Movement stays bit-exact.** Mover.cs changes only if an item names it (none does tonight). After every Core
   commit: every golden test, 16 / 16 replays (+ the 6 trial routes once U3-07 adds them).
6. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. A status line in
   `NIGHT9-LOG.md` at the end of every item, with times from `date`.
7. **GG owns sound and enemy looks** (CLAUDE.md rule 7). Statuses get the web's visuals as they are; no audio code.
8. **What only GG decides goes in HANDOFF's pending decisions, never into code:**
   - D-02: when local split-screen co-op runs
   - D-11: whether "better movement" means anything beyond the web's movement
   - D-12: the UI system
   - D-13: the theme fonts
   - D-14: the tone-mapping / fog parity
   - D-15: the debug keys
   - D-09: the URP asset
9. **Not tonight (later nights in PORT-PLAN.md):**
   - projectiles, the other guns, melee weapons and their wall buys
   - new enemy kinds, bosses, modes
   - rebinding / gamepad / menus
   - look parity
   - the `Player[]` refactor, except as the "if time" item
10. **NIGHT8's debt:** nothing blocks tonight.
    - The U2-02 allocation closer (about 3.9 KB a frame) stays in HANDOFF's next actions.
    - A mismatch that U3-07 finds in an already-ported module is tonight's debt, and is fixed first (section 3).

## 1. Start (U3-06)

1. Read the files listed at the top, CLAUDE.md first.
2. Run HANDOFF section 1, every row. Write `RECEIVED` in `NIGHT9-LOG.md` with each result. If tests or the Player smoke
   fail, fix that first (it is the baseline) and log it.
3. `git switch -c port/stage3` at `d3c9249`.
4. Load `unity:unity-cli` before driving the Editor.

## 2. Definition of done (all measurable)

1. **U3-07, the oracle:**
   - `skygrind/port-export-p18/` exists, untracked; its `verify.mjs` reports 0 mismatches.
   - Every file in `PortExport/vectors/` has a consuming EditMode test (an orphan test fails on any file without one).
   - The new vector sets and the 6 trial-route trajectories pass bit-exact.
2. **U3-04, determinism:**
   - JsMath has V8's atan2, exp, tan, acos, asin, atan, pow (and tanh, if CombatMath keeps it), each matched against a
     `jsMath` vector set from Node (thousands of inputs, edge cases included).
   - A test fails if Core calls those from System.Math.
   - `SKYGRIND.exe -skygrindGolden -logFile Logs/player/golden.log` prints every vector set and every replay with 0
     mismatches.
3. **U3-08, the arsenal:**
   - arsenalMath vectors bit-exact.
   - ArsenalTests (from the web's tests, if any, plus a session over hits and swaps) pass.
   - Style awards use the arsenal's FRESH; the styleCore vectors pass.
   - A forged THUNDERCLAP's STAGGER lands.
   - FLIP is buyable.
   - One Play check shows a CROSS hit paying CROSS.
4. **U3-09, momentum:**
   - momentumMath vectors bit-exact.
   - A fast gauntlet hit sends an impact wave and the momentum hitstop (one Play check or a test).
   - The crosshair arc tracks momentum.
5. **U3-10, aim assist:** V locks, cycles and releases as the web (lockOn / lockOff); the melee lunge moves toward its
   target; assistMath vectors still bit-exact.
6. **U3-11, the perks:** BOUNTY, DEADEYE, FLOW and SLIPSTREAM machines placed and buyable; PerkTests extended.
7. **U3-12, the close:** section 7, every line.
8. **Invariants at the end:**
   - `tests: N / N passed, 0 failed` with N > 237
   - every golden test passes
   - replays 16 + 6
   - the Editor and Player smokes DONE
   - `git status --short` shows only `UnityConnectSettings.asset` (if modified) and untracked NIGHT7 / 8 / 9 / 10.md

## 3. Notes for tonight's items (decided; don't re-open them)

- **U3-07 order:**
  1. Copy p17 to p18.
  2. Add the recorders: the unconsumed sets need none (they exist); the new pure sets (regenMath, pointsMath, modMath,
     powerUpMath, deathRecap, hitDirection, kicks, springs, tokens, slots, aim, crowd, flowField, watchdog) and the
     `jsMath` set (U3-04).
  3. Add a trajectory exporter for the trial routes (reuse `trialRunner.js` as the web tests do).
  4. Run all.mjs, then verify.mjs.
  5. Copy into `PortExport/`; extend `Sync Port Data` so the Player gets vectors/ and trajectories/ (and the maps they
     use).
  6. Write the consuming tests in `CombatGoldenTests`' `Cases(module, fn)` style.

  **If a newly consumed set shows a mismatch in an already-ported module** (styleCore is the likely one), that is debt:
  fix the port until it is bit-exact, log what was wrong, and add the rule to PORT-RULES.md if it is a translation
  pattern.
- **U3-04:**
  - Port V8's fdlibm functions into `JsMath` line for line (keep their comments and constants). Replace the System.Math
    calls in Core.
  - The golden vectors and replays must stay bit-exact after the swap; if one changes, the old System.Math result was
    the one off.
  - `-skygrindGolden`: a runner in `Skygrind.Game` (PlayerChecks), not the NUnit tests. Move the case evaluation the
    golden tests use into a small Core class both can call, so the Player checks what the Editor checks.
- **U3-08:**
  - Port `arsenal.js` and `arsenalMath.js` into Core. The enemy records live in the arsenal, keyed by enemy id as the web
    keys them.
  - The loadout's `Damage` applies `arsenal.hit(enemy, weapon)` and calls `arsenal.after(...)` exactly where weapons.js
    does.
  - The style meter takes a `getFresh` (styleMeter.js) and passes `fresh` into the core.
  - `Forge.status = arsenal.Status`.
  - FLIP's machine and its perkEffects `arsenal` part.
  - The HUD's FRESH tag reads the arsenal's FRESH (hud.js).
  - Statuses' visuals: the web's (fx files for burn / shock tints if they exist), otherwise a one-line log of what is
    missing.
- **U3-09:**
  - Port `momentum.js` + `momentumMath.js` (Core) and `impact.js` (Core for the waves; the view draws them).
  - Then `ui/momentumArc.js` (uGUI, UnityTuning for layout) and the momentum hitstop from main.js.
  - The impact FX (`fx/impactFx.js`) can be a simple ring tonight; its full look is N12.
- **U3-10:** V uses `input.bindings.lockOn`; the events keep the web's names.
- **U3-11:** add the four ids to `Perks.Ported`; their machines come from `data.perkSpots` like the others.

## 4. Testing budget (build first)

- **Per item (about 3 minutes of checks):** one `bash Tools/unity-loop.sh compile`, the item's own tests, and at most one
  Play check (one eval file).
- **The full suite:** after each Core commit (it gates movement), and once at the close.
- **Play checks:** at most 5 tonight, each in one eval file, never timed across two evals (the memory note on eval
  latency).
- **Captures:** at most 4, each moved from `Assets/` to `Logs/shots/` at once.
- **One heavy job at a time:** never two Play sessions or two builds; one Player build tonight, at the close.
- **Caps are caps:** if a retake would cross a cap, report it short instead.

## 5. Mistakes not to repeat (NIGHT7-8)

1. **Patches through a heredoc or `node -e`.** Write `.cjs` / `.cs` patch files with the Write tool (NIGHT8 slipped
   once).
2. **Hidden output.** Never send compile or test output to `/dev/null`; trust `compilationFailed` / `consoleErrors`.
3. **Timing across evals:** each eval costs 10-17 s of game time. Do a timing check inside one eval or a coroutine.
4. **Clock-seeded checks:** a check that depends on a random draw seeds its run (the smoke's spitter step).
5. **Claims beyond the measurement:** NIGHT8 logged a 960x600 canvas as "1920x1080". Write what the output line says.
6. **Builds rewrite files:** after any build or target switch, run `git diff --stat`.
   - Restore `Assets/Settings/Mobile_RPAsset.asset` (the URP upgrade, D-09).
   - Restore a WebGL batching entry in ProjectSettings.
   - Clear a stale `M` with no diff by `git add` of the same bytes.
7. **Unfocused stalls:** `editor_focus` before every Play check; if two commands in a row time out, `editor_stop`, then
   `editor_focus`.

## 6. Order of work and budgets (budgets include each item's checks)

| Order | Item | Budget |
|---|---|---|
| 1 | U3-06 verify on arrival, branch | 15 min |
| 2 | U3-07 the oracle (p18, consume every set, new vectors, trial routes) | 150 min |
| 3 | U3-04 determinism (JsMath, the source-scan test, `-skygrindGolden`) | 120 min |
| 4 | U3-08 the arsenal (+ FLIP, forge statuses, style FRESH) | 150 min |
| 5 | U3-09 momentum, impact waves, the arc | 90 min |
| 6 | U3-10 the V lock and the lunge | 45 min |
| 7 | U3-11 four perk machines | 30 min |
| 8 | U3-12 close | 60 min |
| if time | the first item of PORT-PLAN.md's N10 (`Player[]` in Core), on its own commit, golden + replays unchanged | |

- **An item 25% over its budget** stops: ship what works behind a design flag if it is unfinished, mark the item
  `in_progress` with what remains in its notes, and move on.
- **The close starts by hour 10:30** whatever is left.
- **Cut order:** U3-11, then U3-10, then U3-09's arc and impact FX (keep the damage).
- **Never cut:** U3-06, U3-07, U3-04, U3-08, U3-12.

## 7. Close (U3-12): the handoff loop

1. The invariants in tasks.json, results in the log. The Player rebuilt (scenes Survival then Arena), then:
   - `git diff --stat` and the restores (section 5.6)
   - INV-smoke
   - `-skygrindGolden`
   - one bench (`bash Tools/survival-bench.sh 60`, the plan polled)
2. **`tasks.json`:** every U3 item touched tonight has a status and evidence (commit, output line, time). Never delete an
   item.
3. **Docs:**
   - `PORT-STATUS.md`: the arsenal, momentum, aim assist, perks rows, and new event names marked port-only.
   - `PORT-PLAN.md` section 2: tick the rows done.
   - `PORT-RULES.md`: any rule learned twice, in its change table.
4. **Rewrite `HANDOFF.md`** with the `handoff` skill, in HANDOFF's own structure (rewrite, don't append):
   - a new anchor
   - the verify block with tonight's values
   - next actions = N10
   - decisions
   - risks, dead ends, invisible state
   - a changelog line
5. **`NIGHT9-REPORT.md` for GG (one page):**
   - what shipped, with commits
   - the numbers from tonight's runs
   - what is partial and why
   - a 10-minute play checklist with one question per new feel point (FRESH, PRIMED, CROSS, momentum, the V lock,
     the perks)
6. **Write `NIGHT10.md`** (untracked) from PORT-PLAN.md's N10 row, in this file's shape:
   - re-read the code first, and fill its facts block with measured numbers
   - split U3-13 into numbered items in tasks.json (it may add items; existing acceptance and check stay as written)
7. Promote any lesson seen twice to memory (one file each, an index line). Propose CLAUDE.md changes in HANDOFF's
   pending decisions; don't edit CLAUDE.md.
8. Restore the TMP fallback asset if a Play run changed it. Tag `u3-n9`. `git status --short` as in section 2.8.
   Nothing pushed, nothing merged.

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

- **The oracle decides.** Port the web line for line and let the vectors judge; never edit a test or a vector to pass.
- **Write only what was measured.** Every number in the log, report and tasks.json comes from an output line tonight.
- **The web repo is read only** except the new untracked `port-export-p18/`. Movement stays bit-exact: golden and
  replays after every Core commit.
