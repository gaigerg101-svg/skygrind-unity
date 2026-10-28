# NIGHT16.md: SKYGRIND in Unity: THE FEEL NIGHT (movement that rewards skill over spam, impact freezes and slow-mo, the hands, the small things), hard cap 6 h

> **Written 2026-10-05 ~2:15 PM, after the NIGHT15 close** (anchor `u3-n15`), from GG's message: "I want next prompt to
> be purely focused on quality of life as well as effects ... I want movement to be a little more balanced like you
> can't you know spam slide over and over and other movement and UI stuff, be critical, think hard and follow a good
> process ... that cool effects where if you hit a punch or a certain attack or gun hit or head shot its like ultrakill
> where the game will almost pause or go slomo ... there should be an animation where pulling out the fist weapon or
> other weapons ... full polish ... 5 hours 6 hour hard cap so I don't want it to be rushed or lazy and then anything
> that we didn't get finished let's wrap it up." No new guns, enemies or maps tonight. Map night 2 waits in
> `QUEUE-map-night-2.md`.

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves; the Editor is open on this project. Nobody answers questions during the night.

## Why this night (read this first; it decides every trade-off below)

Fifteen nights made a faithful, deterministic port. Its feel was ported line by line too, so it carries the web's gaps,
and nobody has judged it as a game you hold in your hands. Read from the code on 2026-10-05 (not measured yet; the
audit measures them):

- **The slide is free speed.**
  - A slide press from a standstill sets the speed to `slide.floorSpeed` 11.6 m/s at once, 1.45 x `ground.walkSpeed` 8
    (`Mover.StartSlide`, `Mover.cs:812`).
  - A held slide on flat ground never drops below 11.6 (`StepSlide`, `Mover.cs:896-903`: friction only above the
    floor, then `speed = max(floor, speed)`), so sliding beats walking for as long as the key is held.
  - Releasing and pressing again pays the burst again. Every slide-jump multiplies the horizontal speed by
    `slide.jumpHorizontalMult` 1.2 up to `jumpBoostMaxSpeed` 24 (`MovementMath.SlideJumpSpeed`, `MovementMath.cs:151`;
    `Mover.cs:748-754`). The slide friction (1.5 above the floor) bleeds some of that between hops; the real gain per
    hop is not measured.
  - The best play is to hold or mash slide everywhere. That is what GG felt.
- **The impact layer is thin.**
  - Freezes today: 0.04 s on every player kill (`feel.hitstopTime`, `SurvivalSim.cs:273-277`), 0.045 s on a melee hit
    above momentum 0.6, and the parry.
  - A headshot, a punch that connects, a rocket's direct hit and the round's last kill have no freeze of their own, and
    nothing ever goes slow.
  - **The parries are inverted:** a PERFECT PARRY freezes 0.15 s (`design.parry.perfectHitstop`), a plain parry 0.25 s
    (`design.ultra.parryHitstop`, `Ultra.cs:40-41`). The better parry gets the shorter freeze.
  - In a horde, 0.04 s on every kill may add up to a stutter. Nobody has measured the frozen time per minute.
- **The hands are generic.**
  - Every swap is one drop-and-raise curve (`Viewmodel.cs:701-717`): a 0.25 s swap (`weapons.swapTime`), the gun down
    for the first half (`swapLowerFraction` 0.5) and able to fire from 0.7 (`swapFireFraction`; check `Combat.cs:899`
    on). That leaves about 0.05 s of visible raise before the gun can fire.
  - There is no per-gun draw. **The left arm's punch is not ported** (the web's `viewmodel.arm`, `src/player/
    viewmodel.js:157-161, 682-686, 925`; PORT-STATUS's Player row). A quick punch shows no fist.
  - Also not ported: the cooldown-ready glint and its `weaponReady` event (`viewmodel.js:850`), and the multikill FOV
    punch.
- **The UI has open defects.** The pad wheel's disc shows world labels through it, TITLE's MAP group has no sub-line,
  and nothing teaches hold-Y for the wheel (HANDOFF section 6). Nobody has looked for the rest with a critic's eye.

Tonight's job:
1. **Audit** the feel critically, with instruments and research.
2. **Make the slide a skill**, not a stance, behind `design.move` flags, ON by default (GG asked for it).
3. **Build an impact layer** of tiered freezes and a little slow-mo, with a budget so a horde never stutters.
4. **Give every weapon a draw** and the punch a fist.
5. **Fix the UI defects** a critic finds.
6. **Spend every remaining minute** to hour 5:00 on the ranked polish queue.
7. **Hand GG an A/B kit**: NIGHT15's build beside tonight's, and a 10-minute playtest script.

**Process, the way the good ones do it (the web's NIGHT5 did this: `docs/FEEL-TARGETS.md` in the web repo):**
1. Instrument first.
2. Write targets as numbers.
3. Change one thing.
4. Measure again.
5. Look at it, frame by frame.
6. Say what is still wrong.

A still capture can't show a freeze or a draw. Tonight's proof is the **strip** (consecutive frames on one contact
sheet, each stamped with real time and the sim step) and the **ledgers** (every freeze and slow-mo with its cause and
its real milliseconds; the viewmodel pose per frame).

## Read these files

- **`CLAUDE.md`** first, then this file. Then:
  - **`DESIGN-PILLARS.md`**: pillar 1 ("Nothing may make it mushier: raw mouse input, the 120 Hz step, no acceleration,
    no animation lock") and "Not SKYGRIND" (D-29 the dissolve, D-30 no new packages).
  - **`DESIGN-RESEARCH.md`** rows 4-5 and the Sakurai hitstop source it lists.
  - **`ZOMBIES-ULTRAKILL-RESEARCH.md`** section 2.2, item 6 "Juice numbers".
  - **`PORT-STATUS.md`**: the Player + input row and "Events the port emits".
- **`HANDOFF.md`** sections 0, 1, 6, 7 and 8; **`PORT-RULES.md`** sections 6, 11 and 12; **`tasks.json`** (write U3-29
  and U3-29.1 .. U3-29.10 below before any code).
- **The web repo, read only:**
  - `docs/FEEL-TARGETS.md` (the model for tonight's targets table), `docs/FEEL-LOG.md`, `docs/MOVEMENT-CHANGES.md`.
  - `scripts/feelTrace.mjs` and `scripts/moveTrace.mjs` (the instruments NIGHT5 used).
  - `src/player/viewmodel.js` (the arm, the glint: the port source).
- **The code named above:**
  - `Mover.cs:804-820, 879-905, 740-760` and `MovementMath.cs:151-155`.
  - `SurvivalGame.cs:253-306` (the loop: hitstop, the accumulator, `WorldDt`).
  - `SurvivalSim.cs:272-292, 395-401` (the freeze requests) and `Ultra.cs`.
  - `Viewmodel.cs` (its header and :701-717) and `Combat.cs:357, 398-400, 899` (the swap timer).
- **The memory notes:**
  - `unity-editor-unfocused-autotick`: send `set_autotick` at the start; the Editor stalls unfocused.
  - `unity-eval-latency-timing`: never time a freeze across evals; measure in the Player.
  - `play-check-eval-recipe`, `night-parallel-agents`, `bench-power-plan-flips`.

**Run length: a hard cap of 6 hours from your first `date`. The close starts at 5:00, not before, unless every item is
DONE and the polish queue is empty. Rushing to an early close is the failure GG named; so is running past 6:00.**

## 0. Tonight's rules (on top of CLAUDE.md, which is in force)

1. **Branch `port/stage3`** from its head (`u3-n15` plus docs commits). Tag `u3-n16` once, last, on the close commit.
   The web repo stays read only. No push, no merge, nothing on master.
2. **Pillar 1 is the law.** These rules hold for every feature tonight:
   - **No animation lock.** Every visual follows the sim's timers and never gates them: a shot during a draw fires when
     the sim says it can, and the draw cancels on the spot.
   - **No eaten input.** A press during a freeze or a slow-mo reaches the sim on the next step, through the input
     latches. A test proves it: a jump pressed and released inside a freeze still jumps.
   - **No added latency.** The mouse look stays live during a freeze and a slow-mo, unless the audit shows that's wrong,
     with a reason.
3. **Effects are time and pictures only.**
   - The new freezes, the slow-mo, the hands and the UI live in the Game layer: the loop driver, the views, the UI,
     `UnityTuning*`.
   - The composed fingerprint stays EQUAL across every effects commit. An effect that changes it is in the wrong layer.
   - New freeze requests go through `SurvivalGame`'s loop, not into Core state.
   - `sim.hitstopLeft` is saved in the run snapshot (`SurvivalSim.Save.cs:44`). Say in the log whether a new request
     touches it.
4. **The slide change is tonight's one gameplay change.** GG asked for it.
   - **Where it lives:** in Core (`Core/MoveBalance.cs`, pure and tested) behind `design.move.*` in
     `Design/unity-config.json`, with `_why`, and its byte-identical Player copy. Each rule has its own flag, ON by
     default, and a LAB row so GG can switch each one off.
   - **`Mover.cs` is named by U3-29.4:** new fields only, neutral by default (like `slideSpeedMult`, `Mover.cs:137`).
     The web's lines and comments stay as they are.
   - **Gates at every Core commit:** golden 0 mismatches and 22 / 22 replays (the Core tests run the web's config with
     the rules neutral).
   - **The fingerprint is re-recorded once, in its own commit,** as `FingerprintBaseline.cs`'s header says: the
     before / after lines and the reason in the log. Old tapes carry the old baseline id (`RunTape.cs:4`); check that
     one says so instead of drifting silently.
   - **Every other gameplay number stays:** config.json, the guns' damage and timers (`weapons.swapTime` included), the
     enemies, the rounds.
5. **Numbers:**
   - New freeze, slow-mo and movement numbers go in `design.impact` / `design.move`, each with `_why` and its source.
   - Render-only numbers go in `UnityTuning.Impact.cs` / `UnityTuning.Hands.cs` (new), each with a comment: flash alpha,
     FOV punch-in, strip layout, arm poses that config.json lacks.
   - Starting values are below; the audit may change any of them, with the reason in `FEEL-DESIGN.md`.
6. **No new packages (D-30)** and no Animator controllers: the viewmodel stays code-driven springs and curves
   (`Viewmodel.cs`), so it stays deterministic and allocation-free.
7. **GG owns sound, music and enemy design** (CLAUDE.md rule 7).
   - No audio.
   - Enemy models and behaviour stay as they are; the effects on an enemy are its existing hit flash and the dissolve
     (D-29: no gore that stays).
   - Each new moment emits an event named for GG's audio map: the web's name where one exists (`weaponReady`), else a
     **port-only** name (`impactFreeze`, `slowMo`, `slideCharged`, `weaponDraw`, ...), listed in PORT-STATUS's events.
8. **Accessibility:**
   - Every new flash follows `flashIntensity`, with **at most 3 flashes in any second** (WCAG 2.3.1's three-flash rule).
   - REDUCED MOTION turns the slow-mo off.
   - New settings rows: **HIT FREEZE** (OFF / LIGHT / FULL) and **SLOW-MO** (ON / OFF), defaults from `design.impact`.
   - The automation runs at FULL, so the gates exercise it: the smoke, the AUTOPILOT, the farm, the attract and the tour.
9. **Verify by measuring and by looking.**
   - A DONE row for something on screen needs a strip or capture after the change, plus a two-line review: what a
     first-time player feels in the first second, and what is still wrong.
   - Movement rows need the spam table. Freeze rows need the ledger.
   - Captures go to `Logs/shots/n16-*.png` (gitignored).
10. **Performance (pillar 4):**
    - No allocation per frame in new code (PORT-RULES 6).
    - At most +4 world and +4 viewmodel draw calls at rest (the web's NIGHT5 budget).
    - The worst 1 % stays under 6.9 ms: the close's Silent series, if on AC and idle.
11. **Builds:** build 1 by hour 2:30; every build runs the smoke; at most 4. Before build 1, copy NIGHT15's build 3
    (`Builds/Stage1/`, 165 MB, stamp `cb25832`) to `Builds/n15-before/` for GG's A/B. `Builds/` is gitignored.
12. **One driver for the Editor:** builders (at most 4) in worktrees, dotnet only (`Tools/wt-csproj.cjs`; a refresh
    regenerates the main tree's csproj: re-glob before a dotnet build there). Long suites run async (`run_tests
    --async_tests true`, then poll `test_status`).
13. **No questions, no waiting:**
    - One status line per item in `NIGHT16-LOG.md`.
    - End every message with `N16-STATUS: <d> done, <s> short, <l> left (of 10) · tests <passed>/<total> · <h:mm from
      the first date>`.

## 1. Who owns what (the builders, from the shared commit)

| Builder | Owns (creates or edits only these) | Proves with |
| --- | --- | --- |
| **M (movement)** | `Core/MoveBalance.cs`, `Mover.cs` (new fields + their call sites only), `Game/SurvivalSim.MoveBalance.cs` (the partial that sets the fields from `design.move`), `Tests/EditMode/MoveBalanceTests.cs`, `Tools/SimHarness/Harness.Spam.cs` | CoreHarness golden + replays; `SimHarness spam` before / after; `SimHarness verbs` on both maps |
| **I (impact)** | `Core/ImpactRules.cs` (pure: event to request, tiers, budget, cooldowns), `Game/ImpactFrames.cs` (the director: listens to the bus, asks the loop), `Game/SurvivalGame.Impact.cs`, `Game/UnityTuning.Impact.cs`, `Tests/EditMode/ImpactRulesTests.cs`, `Tools/SimHarness/Harness.Impact.cs` (the headless freeze ledger) | the tests; the ledger over a 300 s AUTOPILOT run on each map |
| **V (hands)** | `Game/Viewmodel.cs`, new `Game/Viewmodel.Draw.cs` / `Viewmodel.Arm.cs`, `BlockyKit.cs` (the arm's parts only), `Game/UnityTuning.Hands.cs`, `Tests/EditMode/Game/HandsTests.cs` | the fire-gate test per gun; the clear-box check; strips |
| **U (UI and the strip)** | new `Game/UiFeelHud.cs`, the named Ui files for the open defects (`UiRadial.cs` the wheel's disc, `UiMeta.cs` the MAP sub-line, `UiHotbar.cs` the hold-Y hint), `Game/FeelStrip.cs` (the strip tool), `Tests/EditMode/Game/FeelHudTests.cs` | captures; the strip tool's own sheets |
| **Lead** | the shared commit; `SurvivalGame.cs`'s loop lines; `GameSettings.cs` rows; `PlayerChecks.cs` hooks; `ShotTour.cs`; every doc; the Editor; git; the polish queue | everything else |

Same rules as NIGHT15 (memory `night-parallel-agents`):
- Topic-named members in shared partial classes (PORT-RULES 11).
- Tests first, seen red at cherry-pick.
- An agent's cherry-pick of a dependency may be refused: give each agent its dependencies in the shared commit.
- Before `git worktree remove --force`, compare each leftover file with the integrated commit.
- Agents never run `unity`.

## 2. The items (U3-29.1 .. U3-29.10)

### U3-29.1 Start (lead, 15 min)
- `date`; HANDOFF section 1, row by row; `set_autotick` (GG is often at the machine by day); RECEIVED in
  `NIGHT16-LOG.md`; the machine (power plan, AC, CPU: `bench-power-plan-flips`); U3-29.* in tasks.json; copy
  `Builds/Stage1/` to `Builds/n15-before/` and launch it once with `-skygrindSmoke` to confirm it still runs (close it
  by its PID).
- **Done when:** the rows are logged, tasks.json parses, and `Builds/n15-before/SKYGRIND.exe` ran.

### U3-29.2 The feel audit (lead + one read-only research agent, 30 min research cap; the audit 60 min, beside the builders)
- **Research** (an agent, `general-purpose`, read only, 30 min, every fact tagged **VERIFIED** with the source read or
  **UNVERIFIED**):
  - ULTRAKILL's freezes and slow-downs: which actions, how long; does the camera look stay live during them? The parry
    is 0.25 s per our notes.
  - Sakurai on hitstop (DESIGN-RESEARCH's source); Vlambeer's "Art of Screenshake" (the "sleep" frames); Eiserloh's
    trauma.
  - How the movement shooters keep the slide a skill: Titanfall 2 and Apex's slide boost and its cooldown, the Call of
    Duty slide-cancel changes, ULTRAKILL's slide (constant speed? check), Neon White.
  - DOOM Eternal's weapon switch: speed, and why it has no lock.
  - Impact frames in Hi-Fi Rush and Sifu.
  - First-person hands: Mirror's Edge, Titanfall 2.
- **Instruments first** (the builders build them; the lead runs them):
  - `SimHarness spam` (M): scripted abuse per verb on flat ground (the arena's floor and DRYDOCK's deck), each against
    walking and against the skilled line.
    - The verbs: slide tapped every 0.15 / 0.3 / 0.5 s; slide held 10 s; a slide-hop chain from walk speed; dash
      mashed; a wall-jump chain; slam-boost chains.
    - It prints metres in 10 s, the peak speed, the speed at 10 s, and the inputs per second.
  - `SimHarness impact` (I): the freeze ledger, using what `ImpactRules` would request. Over a 300 s AUTOPILOT run per
    map it prints freezes by cause, frozen ms per minute, the worst rolling second, and slow-mos.
  - The viewmodel trace (V): the pose per frame through each gun's swap, as a CSV, like the web's `feelTrace.mjs`.
- **The critique:** `FEEL-DESIGN.md` (tracked, new).
  - **Section 1, what is wrong now**, ranked by what a player feels in the first minute. Each row carries evidence: a
    measured number, a capture from `Builds/n15-before/`, or a code line. Judge at least:
    - each item in "Why this night";
    - dash spam (3 pips, 0.7 / s regen);
    - the wall-jump cap (3 per airtime);
    - the slam-boost;
    - the camera's landing and slide;
    - the HUD at a glance, the buy prompts, the menus' transitions;
    - the three worst UI defects from the n15 tour.
  - **Section 2, the research** with tags.
  - **Section 3, the design:** the movement rules, the impact tiers with the budget, the hands list, the UI list. Each
    has a target number and the instrument that checks it, as a table like the web's FEEL-TARGETS.md.
  - **Section 4, what we will not do and why** (pillar 1, D-29, D-30, GG's areas).
  - **Section 5, the ranked polish queue:** at least 15 entries, each with impact x confidence / cost (S / M / L),
    seeded from U3-29.8's list.
- **Done when:** FEEL-DESIGN.md sections 1-5 are written; every number in them is measured or marked "target"; the
  research is tagged; the targets have been sent to the builders (SendMessage).

### U3-29.3 The shared commit (lead, 20 min; before the builders launch)
- `design.move` and `design.impact` blocks with `_why` and the starting values below, and the Player copy.
- Stubs that throw: `MoveBalance`, `ImpactRules`, `ImpactFrames`, `FeelStrip`, `UiFeelHud`.
- In `SurvivalGame.cs`, the loop's time scale: `accumulator += frameDt * impactScale` and `WorldDt` x `impactScale`.
  `impactScale` is 1 until I fills `SurvivalGame.Impact.cs`.
- The settings rows HIT FREEZE / SLOW-MO; the LAB rows for the three move rules; the `-skygrindStrip <dir> [scenario]`
  hook in `PlayerChecks.cs`.
- The SimHarness dispatch (`Harness.Night16.cs`: `spam`, `impact`); the event names declared; the empty test files.
- **Done when:** the commit compiles in the Editor, the suite is still all green (724 + the new stubs' tests, which are
  marked to fail until implemented), the fingerprint is EQUAL, and four builders are launched on that commit.

### U3-29.4 The slide becomes a skill (builder M; lead 40 min)
- **Starting rules** (each its own flag in `design.move`, ON; the audit may retune them):
  1. **Slide boost charge:** the snap up to `slide.floorSpeed` needs a charge. There is one charge; it recharges
     `rechargeTime` 1.0 s after the slide ends. An uncharged slide keeps its entry speed, so no burst but never a loss.
     The press always slides: no lockout.
  2. **Flat-ground decay:** after `sustainTime` 0.6 s the slide's floor eases from `floorSpeed` toward
     `ground.walkSpeed` over `decayTime` 0.5 s. A downhill slide is untouched (the slope boost still pays, and the
     DRYDOCK slipway keeps its 28.8 m/s); uphill as now.
  3. **Slide-hop falloff:** each slide-jump within `chainWindow` 1.0 s of the last landing gets a multiplier 0.1 lower
     (1.2, 1.1, 1.0, then 1.0), and the cap stays 24. A well-timed first hop still pays in full.
- **Targets:**
  - (a) Slide tapped at any rate on flat ground for 10 s: at most 1.1 x a walk's distance.
  - (b) Slide held 10 s on flat ground: at most 1.1 x.
  - (c) A slide-hop chain on flat ground from walk speed peaks at 15 m/s at most.
  - (d) The skilled lines stay within 5 %: the dash-jump 21.2 m, the wall-ride climb 7.6 m, the first slide-jump 8.9 m
    (MAP-DESIGN.md's measured table), DRYDOCK's slipway 28.8 m/s, and LAUNCH RAIL's 3 of 6 take-offs (`SimHarness
    verbs`).
  - (e) Dash, wall-jump and slam change only if the spam table shows mashing beats skill. Otherwise write "no change"
    with the number.
- **Feedback** (a limit you can't see feels like a bug): the slide charge on the HUD (U3-29.7), the `slideCharged`
  event, and a short cyan flick at the feet or on the viewmodel when the charge returns (V or the polish queue).
- **Done when:**
  - the spam table before / after is in FEEL-DESIGN.md with every target met or explained;
  - golden 0 mismatches and 22 / 22 replays at the Core commit;
  - `SimHarness verbs` passes on both maps;
  - the smoke's step 24 (the AUTOPILOT on DRYDOCK clears 3 or more rounds, twice, hashes equal) still passes;
  - the fingerprint is re-recorded in its own commit, with the before / after lines;
  - a farm diff (PORT-RULES 12): 1 x 100 arena runs before and after if the time allows, else SHORT on that half only,
    with the reason.

### U3-29.5 Impact frames: tiered freezes and a little slow-mo (builder I; lead 60 min)
- **Starting tiers** (freeze seconds; the longest request wins, as the web's `Hitstop` does):

  | Moment | Freeze (s) |
  | --- | --- |
  | Kill (the web's, unchanged) | 0.04 |
  | Heavy body hit (rocket direct, rail, shotgun with 6+ pellets in one body) | 0.03 |
  | Headshot | 0.05 |
  | Headshot kill | 0.08 |
  | Punch or gauntlet hit | 0.06 (the momentum hit's 0.045 stays as a floor) |
  | Punch kill | 0.09 |
  | Parry | 0.25 (design.ultra) |
  | PERFECT PARRY | 0.30, plus a slow-mo tail (fixes the inversion; log the change to `design.parry.perfectHitstop`) |

- **Slow-mo** (new; the sim steps at 120 Hz as always, just fewer per real second):
  - The round's last kill: 0.3 x for 0.5 s real, easing back over 0.3 s.
  - The perfect parry's tail: 0.35 x for 0.25 s.
  - A multi-kill of 3 or more from one shot or blast: 0.5 x for 0.25 s.
  - None while DOWN, paused or in a menu. Slow-mos never stack (the strongest wins), and there is at most one in any
    3 s.
- **The budget** (a horde must never stutter): frozen time at most 0.12 s in any rolling second and at most 6 % of real
  time over a minute. Each tier has a cooldown (a kill freeze at most every 0.1 s). A request over budget is skipped
  and logged. The audit's ledger sets the real numbers.
- **What a freeze shows:**
  - the world stopped (as now);
  - the struck enemy's hit flash held through the freeze;
  - the hitmarker / kill marker on the freeze's first frame;
  - a render-only FOV punch-in of about 1-2 degrees;
  - a low-alpha screen flash on parry, perfect parry and headshot kill only (x `flashIntensity`, at most 3 per second);
  - a pad rumble pulse (x RUMBLE).
  - Say in FEEL-DESIGN.md whether the viewmodel freezes with the world; ULTRAKILL's choice is a research question.
- **Done when:**
  - `ImpactRulesTests`: the tier choice, longest wins, the budget, the cooldowns, no slow-mo while DOWN, and the latch
    test (a jump pressed and released inside a freeze still jumps);
  - the ledger on both maps meets the budget;
  - strips of a headshot kill, a punch kill, a parry and the round's last kill, each frame stamped with real ms and the
    sim step (equal steps across frames = frozen), each reviewed in two lines;
  - the fingerprint is EQUAL with every impact feature at FULL.

### U3-29.6 The hands: a draw for every gun, the fist, the glint (builder V; lead 60 min)
- **Per-gun draw within the sim's swap timer.** The raise has about 0.05 s before fire opens, so a draw's signature
  lives mostly in the settle after the gun is up, and a shot cancels it at once. Signatures for at least five:
  - the revolver / KESTREL: a cylinder spin and a flick up;
  - the shotgun / THUNDERCLAP: a pump rack on arrival;
  - the rocket: a heavy lift and a shoulder settle on its class spring;
  - the rail: a charge glow sweeping along it;
  - the gauntlet: the fist rising as it clenches, the gem flaring.
  - The rest by weight class.
  - The holster tips toward the hip. The run start and the respawn raise use the draw.
- **The left arm (port, `viewmodel.arm`, `armRest` / `armPunch`; the web's `viewmodel.js` line for line where it
  exists):** the quick punch's fist from the lower left, sharing the sway and the bob. On a parry the knuckles flash;
  on a perfect parry they flash white.
- **The glint (port, `viewmodel.js:850`):** a short glint when a slow gun's cycle ends, plus the `weaponReady` event.
- **Movement on the hands** (candidates; the audit picks): a lift on take-off, a landing weighted by fall speed (beyond
  `landDipScale`), the left hand pushing off on a wall-jump, a slam wind-up.
- **Done when:**
  - `HandsTests`: every gun reaches its fire-ready pose (within 0.5 degrees and 2 mm of hold) by the step the sim opens
    fire, and a shot mid-draw cancels the flourish the same frame;
  - no viewmodel pixel lands inside the centred 10 % x 10 % box on any frame of any draw (the web's NIGHT5 rule);
  - zero allocation per frame;
  - strips of the five signature draws, the left-arm punch and a parry, each reviewed in two lines.

### U3-29.7 The UI and quality of life, and the strip tool (builder U; lead 45 min)
- **The strip tool first** (`FeelStrip.cs`, `-skygrindStrip <dir> [scenario]`):
  - An event-triggered contact sheet: during an AUTOPILOT or scripted run it waits for the event (a headshot kill, a
    punch kill, a parry, the last kill, a swap), then grabs N consecutive frames (default 10). Each frame is stamped
    with real ms and the sim step, all on one PNG.
  - A grab costs frame time, so the step stamp is the truth.
  - Scenarios: `swap` (every owned gun in turn, by `ScriptedInput`), `punch`, `freeze`, `slowmo`, `slide`.
- **A movement HUD at the crosshair:** the 3 dash pips and the slide charge as small cyan ticks under the crosshair.
  They show while something is spent and fade 1 s after everything is full.
- **The hitmarker:** a headshot gets a distinct marker by shape, not only by colour.
- **The open defects:** the pad wheel's disc showing world labels through (a dim backdrop behind it), TITLE's MAP
  sub-line, the hold-Y wheel hint for pad players.
- **Transitions:** TITLE to the run, DOWN to RESULTS, MODS open: 0.15-0.25 s fades or slides that never delay input. A
  press during a transition acts at once.
- **The three worst defects** from the audit's n15 tour.
- **Done when:** a before / after capture for each, each reviewed in two lines; the strip tool's own sheets read
  correctly (the stamps legible, the frames consecutive).

### U3-29.8 The polish queue (lead, from the first free moment to hour 5:00)
- Work down FEEL-DESIGN.md section 5 in rank order, one commit each, each with its strip, capture or measured line and
  its two-line review. Seeds (the audit adds, re-ranks and cuts):
  - the multikill FOV punch (web, not ported);
  - slide sparks / dust from the heels scaled by speed;
  - wall-ride sparks at the contact point;
  - a landing dust ring scaled by fall speed;
  - the dash pip's refill blink;
  - a dash-start smear frame (x effects intensity);
  - shotgun shells ejected on the pump (the Particles pools, no PhysX);
  - an idle flourish (8 s without input in a break: the revolver spins, the shotgun's pump is checked; never in a
    round);
  - a quick-swap flick (two swaps in 0.4 s skip the second holster visually);
  - the struck enemy held white through the freeze, then the dissolve;
  - a perfect parry's one-frame "impact frame" in an inverted dusk palette (x `flashIntensity`, at most 3 per second;
    judge it against pillar 3's readability first);
  - the round-clear beat (the slow-mo, the banner, a soft vignette);
  - a hold-progress ring on BUY;
  - menu focus movement and button press scale (UiFeel).
- **Not rushed, not lazy:** keep going until 5:00. A change you wouldn't defend to an ULTRAKILL player gets reverted
  and logged.
- **Done when:** at 5:00, the log lists each entry taken (commit, evidence, review) and each left, with the reason.

### U3-29.9 Builds, the reviewer and GG's kit (lead, 40 min, spread)
- **Build 1 by 2:30** (the smoke, `-skygrindFingerprint`, the strips, the tour); build 2 after the reviewer's fixes;
  at most 4.
- **One read-only reviewer at about 3:30** (before a build is spent) on the night's diff, with the brief "where would
  this feel bad?":
  - eaten inputs, timing that drifts from the sim, stutter, nausea, flashes;
  - settings and reduced motion ignored;
  - allocation in a hot path;
  - the automation broken.
  - Each finding cites file:line and the symptom a player would see.
- **`FEEL-PLAYTEST.md`** (tracked) for GG: a 10-minute script on `Builds/n15-before/` then `Builds/Stage1/`. It
  covers:
  - slide spam against a timed slide-hop;
  - a headshot kill, a punch kill, a parry and a perfect parry;
  - the round's last kill;
  - swapping through every gun; the left-arm punch.
  - What to feel for in each; how to turn each rule off (the LAB rows, HIT FREEZE, SLOW-MO).
  - The questions for GG: the slide limits, the freeze lengths, slow-mo on by default.
- **Done when:** the builds' results are logged, each reviewer finding is fixed or answered, and FEEL-PLAYTEST.md is
  written.

### U3-29.10 Close (lead, 60 min, from hour 5:00; hard stop 6:00)
- **The final Release build:**
  - the smoke: all steps. Record the NIGHT14 residual (steps 18 / 21) per build; it isn't tonight's job.
  - `-skygrindGolden`: 0 mismatches.
  - `-skygrindFingerprint`: EQUAL to tonight's baseline.
  - `-skygrindReplay` on its smoke's tape: EQUAL.
  - the suite; the UI-hurt Silent series, if on AC and idle.
- **The docs:**
  - tasks.json, `NIGHT16-LOG.md`;
  - `NIGHT16-REPORT.md` for GG: one page with the spam table, the freeze ledger, three strips and the questions;
  - PORT-STATUS: the Player row (the arm, the glint, the multikill FOV punch) and the events list;
  - HANDOFF rewritten (the `handoff` skill, anchor `u3-n16`);
  - `NIGHT17.md` = map night 2 from `QUEUE-map-night-2.md`, renumbered, unless GG's message says otherwise.
- Tag `u3-n16`; the close table; then `NIGHT16 CLOSED`.

## 3. The clock (from the first `date`)

| When | Lead | Builders |
| --- | --- | --- |
| 0:00-0:15 | U3-29.1 | none |
| 0:15-0:35 | U3-29.3 the shared commit; launch M, I, V, U, then the research agent | none yet |
| 0:35-1:35 | U3-29.2 the audit (the n15 tour and captures, the research, FEEL-DESIGN.md 1-5); send the targets | instruments first, then the mechanisms |
| 1:35-2:30 | integrate as each lands (tests seen red, then green), tune from the instruments | landing |
| by 2:30 | build 1: smoke, fingerprint, strips | follow-ups by SendMessage |
| 2:30-3:30 | the reviews of build 1's strips and ledger; fixes | the last follow-ups |
| ~3:30 | the reviewer; build 2 | none |
| 3:30-5:00 | U3-29.8 the polish queue; U3-29.9's kit | none |
| 5:00-6:00 | U3-29.10 the close | none |

## 4. Caps and the cut order

**Caps:** Play checks 2 · captures and strips 40 · Player builds 4 · builders 4 · research agent 1 (30 min) ·
reviewer 1 · bench runs 3 (the close's series only). A cap reached makes its item SHORT with the count.

**Cut order if short:**
1. the polish queue's depth (it's elastic by design);
2. U3-29.7's transitions;
3. U3-29.6's signatures beyond the five named;
4. U3-29.5's multi-kill slow-mo;
5. U3-29.4's farm diff.

**Never cut** U3-29.1, .2, .4's rules, .5's tiers and budget, .6's draws and the fist, or .10.

## 5. The close table (print it, then `NIGHT16 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-29.1 Start | | |
| U3-29.2 The feel audit | | |
| U3-29.3 The shared commit | | |
| U3-29.4 The slide becomes a skill | | |
| U3-29.5 Impact frames | | |
| U3-29.6 The hands | | |
| U3-29.7 The UI and the strip tool | | |
| U3-29.8 The polish queue | | |
| U3-29.9 Builds, reviewer, GG's kit | | |
| U3-29.10 Close | | |

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

## Suggested `/goal` for GG

> Run NIGHT16.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT16.md and the files it
> lists, and work U3-29.1 .. U3-29.10 in NIGHT16.md's order with at most four background builders split by file
> ownership. Never ask me questions and don't stop early; end every message with the N16-STATUS line. Never push, merge
> or commit to master; the web repo is read only. Hard cap: 6 hours from the first `date`; don't start the close before
> 5 hours unless every item is DONE and the polish queue is empty. This goal is met when your latest message prints
> NIGHT16.md's close table with every item U3-29.1 .. U3-29.10 marked DONE (with its "done when" evidence from this
> session) or SHORT (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches,
> or the time limit once a `date` output reads 5 hours or more after the first `date` output), followed by the line
> "NIGHT16 CLOSED". SHORT rows count as met.
