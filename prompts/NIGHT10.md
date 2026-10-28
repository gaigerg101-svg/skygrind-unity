# NIGHT10.md: SKYGRIND in Unity, stage 3 night 2 (the ARMORY guns and the Mystery Box), no human in the loop, at most 12 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **this file:** tonight's job
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **`ZOMBIES-ULTRAKILL-RESEARCH.md`:** GG's new direction and the evidence behind tonight (sections 1, 4, 6, 7)
- **`PORT-RULES.md`:** how to translate a web module
- **`PORT-PLAN.md`:** the whole port (its update note: the night order follows the research's section 6)
- **`HANDOFF.md`:** where things stand (anchor `u3-n9`)
- **`tasks.json`:** tonight's items are U3-13.1, .3, .4, .5, .8, .9, .6, .7 (in that order); U3-13 is the umbrella and
  its acceptance holds
- **`NIGHT9-LOG.md`:** how last night ran, its slips

**Run length: up to 12 hours from your first `date`.** Stop earlier only when the definition of done (section 2) is met.

GG, verbatim (2026-10-03): "want this game to be like cod zombies meets ultrakill as the main, but other games should be
inspired". Before that (2026-10-02): everything the browser version has, rebuilt in Unity.

**Why tonight looks like this:**
- The research found SKYGRIND already has most of Zombies x ULTRAKILL under the hood:
  - CoD's exact points
  - doors, perks and Pack-a-Punch (THE FORGE)
  - ULTRAKILL's exact dash and freshness constants
- What it lacks is what players see:
  - Zombies' rituals, starting with the **Mystery Box**, which the web never had
  - enough guns to make a box worth rolling: only KESTREL, THUNDERCLAP and the GAUNTLET are ported
  - the ULTRAKILL rules players feel
- **So tonight ports the three ARMORY guns and builds the box on top of them:**
  - STITCHER, COMET and LONGWAVE, with projectiles and homing
  - the box's pool is every ported gun
  - the ULTRAKILL alignment goes behind design flags
- **Moved out of tonight:**
  - the rest of `Player[]` (director, wallets, buys): to the co-op night (D-02), since a second player only exists in
    tests
  - nothing else; `weaponFx.js` and decals stay, but are cut first

**Goal (the morning):** GG opens the ARMORY, buys STITCHER, COMET and LONGWAVE on the wall, and rolls the **Mystery Box**:
- 950 points
- a gun rises out of it
- after a few rolls the teddy bear sends it to another spot

ULTRAKILL's rules, per GG's answer to D-17 (ON if none):
- freshness feeds style and points, not damage
- style no longer drains between rounds
- a parry refills the dash and freezes 0.25 s

With every flag off, the game is still exactly the web's.

`NIGHT11.md` is written for tomorrow, following the research's section 6 (N11: the run's shape).

## Facts already measured (2026-10-03; re-check them, don't trust them blindly)

- **The baseline (`u3-n9`):**
  - `tests: 339 / 339 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 84 / 84` (01:49).
  - The Player (built 01:39): smoke `[smoke] 25.1s DONE: all steps passed`; `-skygrindGolden` 0 mismatches (28
    vector sets, 30,523 records, 22 replays).
  - Bench: median 2.49 ms, worst 1% 4.49 ms, on the Silent plan on AC.
  - The harness: `dotnet run --project Tools/CoreHarness -c Release` gives the same golden total in about a second.
- **Names:** STITCHER = `nailgun`, COMET = `rocket`, LONGWAVE = `rail` (web config.js:478, 488-490).
- **Their web code:**
  - **`src/combat/weapons.js`** (750 lines):
    - `fireNail` :659-668 (calls `projectiles.fire('nail', ..., owner.assist?.homingTarget())`)
    - `fireRocket` :670-682
    - `fireRail` :688-747 (hitscan: `octree.rayIntersect` plus an `enemies.raycast` loop; emits `railFire {owner, from,
      to, hits}` and `shotResult`)
    - dispatch in `step()` :276-299
    - specials: nailgun detonate :249-255, rocket steer :256-262, rail overcharge :263-275, zoomed release :218-228
    - events: `overheat` :217, `railCharge` :266 / :283, `railCancel` :226
    - helpers: `meter()` :120-127, `cooldownFraction` :139-143, `variantNow` :484-486
  - **Momentum variants:**
    - nailgun `pierce` (weapons.js:667; projectiles.js:400-406)
    - rocket `radiusMult` plus `fragments` above `momentum.variantAt` (weapons.js:676-681; projectiles.js:149, 166-172,
      450-462)
    - rail through `variantWalls` walls (weapons.js:699-720)
  - **Config** (web config.js; all already in `PortExport/config.json`):

    | Gun | Base block | Key numbers |
    |---|---|---|
    | nailgun | :357-369 | rate 12, pool 160, stuckPool 140, speed 60, damage 9, heat |
    | rocket | :370-382 | fireInterval 0.9, pool 16, speed 30, damage 70, splashRadius 4, knockback 16 |
    | rail | :383-390 | charge 0.6, damage 150, cooldown 3, range 250, pierce 24 |

    Specials :449 / :452 / :455; prices 1250 / 2000 / 3000.
- **`src/combat/projectiles.js`** (775 lines):
  - One export, `createProjectiles(scene, config, events, {getOctree, enemies, orbs, particles})` :43; it imports
    `three`.
  - Methods you port tonight: `fire(kind 'nail'|'rocket', ...)` :245, `detonate` :293, `steerRockets` :324, `step`
    :378-463, `render` :465-563.
  - Internals: `take` :92, `stick` :96, `sweep` :113-126 (world, orbs and enemies; the nearest wins), `steer` :129-140,
    `blast` :146-175, `explode` :177, `arm` :200-233.
  - Events: `rocketBlast` :165, `rocketJump` :160, `nailDetonate` :308, `rocketFragment` :460.
  - A web bug: `airburst` puts the count in `owner` (:320). Port it as is and log it.
  - **No web test imports it.** The nearest tests are `tests/weapons.test.js` (72 lines) and `assistMath.test.js`
    :21-45.
- **Homing:**
  - `aimAssist.js` `homingTarget()` :483-491 (`aim.homingTurn` 120, `aim.lockHomingMult` 2, `aim.homingAssistAngle`
    0.0087).
  - `AssistMath.HomeVelocity` is already ported and golden-checked (AssistMath.cs:230).
- **Wall buys:**
  - `wallBuys.js` (214 lines) hangs the guns in `Object.keys(prices)` order on `map.armoryWall` = `{x 24, y 1.7, zs
    [46.6 .. 26.6], facing -1}`.
  - So the nailgun is slot 3 (z 40.6), the rocket slot 6 (z 34.6), the rail slot 9 (z 28.6).
  - The buy point is x 22.8. The ARMORY must be open.
- **Models:** `src/assets/weaponModels.js`: nailgun :134-158, rocket :159-182, rail :183-212.
- **Projectile vectors need three:** record them the way `trialRoutes.mjs` :10 builds a Scene. The session pattern is
  at `vectors.mjs`:615-619.
- **The Mystery Box (new design; the web has none):**
  - CoD's numbers [research 2.1]:
    - 950 points.
    - Teddy bear by pull number: 0% on pulls 1-3, 15% on 4-7, 30% on 8-12, 50% after.
    - A teddy refunds the 950 and moves the box to another spot.
    - Fire Sale makes it 10 (not tonight).
  - SKYGRIND has no ammo (`PortExport/config.json` has no ammo key), so there is no Max Ammo analogue.
  - The run's rolls come from a seeded `Mulberry32` (CLAUDE.md rule 6).
- **The ULTRAKILL alignment, today's values in SKYGRIND [V]:**
  - The arsenal's FRESH scales **damage and points** x lerp(0.65, 1.25) (`weapons.arsenal`; Arsenal.cs, the loadout's
    Damage).
  - The style meter's freshness multiplies style (`style.freshness.freshMult` 1.1, `staleMult` 0.5).
  - Style drains `style.drainBase` 17 + `drainPerRank` 4.8 per rank, every step, rounds and breaks alike.
  - The punch parry's hitstop is `punch.parryHitstop` 0.06 s; the perfect parry adds 0.15 s
    (`Design/unity-config.json` `design.parry`).
  - A parry doesn't touch the dash pips.
- **The ULTRAKILL alignment, ULTRAKILL's values [research 2.2]:**
  - freshness changes style only, never damage
  - a parry gives a 0.25 s hitstop and refills stamina
- **`Design/unity-config.json`:** top keys `_about`, `design` (`parry`, `director`, `lab`). Every Unity-only number
  lives there with a `_why`.

## 0. Tonight's rules (on top of CLAUDE.md and PORT-RULES.md, which are in force)

1. **Branch:** keep working on `port/stage3` (from `u3-n9`). Tag `u3-n10` at the end. `u2a-demo` stays as it is.
2. **The web repo is read only** (CLAUDE.md rule 2). The one thing allowed is a new untracked folder
   `skygrind/port-export-p19/`: a copy of `port-export-p18/` plus your recorders, run with `node` from the web repo
   root. No commits there, no branch switch, no edits to tracked files.
3. **Port, don't improve, for every web module.**
   - Port line for line, comments kept: the sim half in Core, three.js drawing in Game.
   - Port its web tests and see them red.
   - Consume its vectors (a checker in `Core/Golden/`).
   - A difference from the web is a bug until a vector proves otherwise.
4. **New design is allowed tonight only for the Mystery Box and the ULTRAKILL alignment** (GG's Oct 3 direction;
   HANDOFF D-17, D-19, D-20).
   - Their numbers go in `Design/unity-config.json`, each with a `_why` that names its reference (CoD's box, the
     ULTRAKILL wiki).
   - Each is a design flag.
   - **The golden tests and the replays run with every flag off**, so one player with the flags off is exactly the
     web's game.
   - Set the flags' defaults to GG's answer to D-17 / D-20. With no answer: ON, as the research recommends.
5. **Numbers:** every web number comes from `PortExport/config.json`, and none is typed in code (CLAUDE.md rule 5). A
   key missing from config.json is re-exported in p19, never hand-added.
6. **Movement stays bit-exact.** Mover.cs changes only if an item names it; none does. A rocket jump uses the mover's
   existing `Impulse` (Mover.cs:282). After every Core commit: every golden test, 22 / 22 replays.
7. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. A status line in
   `NIGHT10-LOG.md` at the end of every item, with times from `date`.
8. **GG owns sound and enemy looks** (CLAUDE.md rule 7).
   - The box's model is a plain blocky crate in the dusk palette; a teddy is a simple blocky shape.
   - Weapon models are the web's blocky models, rebuilt as they are.
   - No audio code. New events get names (`boxOpen`, `boxRoll`, `boxTeddy`, `boxMove`) and a line in PORT-STATUS's
     event list marked port-only.
9. **What only GG decides goes in HANDOFF's pending decisions, never into code:** D-02, D-16, D-18 (hard damage and the
   blood heal: not tonight), D-21, D-22, D-09, D-11..D-15.
10. **Not tonight:** the POWER guns, the power switch, special rounds, exfil, hard damage (N11-N12 per the research);
    the rest of `Player[]` (the co-op night); new enemies (N13).

## 1. Start (U3-13.1)

1. Read the files listed at the top, CLAUDE.md first.
2. Run HANDOFF section 1, every row. Write `RECEIVED` in `NIGHT10-LOG.md` with each result. If tests, the Player smoke
   or the Player's golden fail, fix that first (it is the baseline) and log it.
3. Check HANDOFF section 5 for GG's answers to D-17 and D-20, and log what you found.
4. Load `unity:unity-cli` before driving the Editor.

## 2. Definition of done (all measurable)

1. **U3-13.3, projectiles:**
   - `port-export-p19/` holds projectile sessions recorded on a test arena:
     - nails: fire, stick, pierce, detonate
     - rockets: fire, blast, splash, knockback, rocket jump, fragments, steer
     - the rail, from weapons.js `fireRail`: hits, pierce, walls
   - `verify.mjs` reports 0 mismatches.
   - `Core/Projectiles.cs` covers the nail, rocket and fragment paths plus the shared core, line for line. N11's kinds
     are stubbed and listed.
   - Every new vector file is consumed by a checker, bit-exact.
2. **U3-13.4, the three guns:**
   - In the loadout as weapons.js fires them:
     - STITCHER: heat, nails, detonate, the pierce variant
     - COMET: rockets, steer, the fragments variant
     - LONGWAVE: charge, pierce, overcharge, the walls variant
   - Their wall buys sit at the ARMORY wall slots, with their models and viewmodel rigs.
   - Each gun kills in the smoke (new smoke steps).
3. **U3-13.5, homing:** `AimAssist.HomingTarget` as aimAssist.js. Nails and rockets steer with it, assisted past
   `aim.homingAssistAngle`. The web's homing tests are ported if they aren't already.
4. **U3-13.8, the Mystery Box:**
   - **Spots:** 3 box spots on the Survival map in different zones, each on open floor and reachable, chosen by casting
     the collider (memory `play-check-eval-recipe`). The first spot is in the starting zone. Spots and numbers go in
     `Design/unity-config.json` (`design.box`).
   - **A roll:**
     - Hold to buy: 950 points.
     - A seeded pick from every ported gun you don't own.
     - A short rise-and-cycle reveal, then the gun waits to be taken: E swaps it into the slot in hand, as wall buys
       do.
     - If it isn't taken it sinks after a set time.
   - **The teddy:** on CoD's ladder (pull 1-3 0%, 4-7 15%, 8-12 30%, then 50%). It refunds the 950 and moves the box to
     another spot.
   - **Events:** `boxOpen`, `boxRoll`, `boxTeddy`, `boxMove` (port-only).
   - **A HUD prompt:** "MYSTERY BOX 950", TMP at least 16 px at 1080p.
   - **Tests (`BoxTests`):** the seeded sequence, the teddy ladder at each pull band, the refund, the move, no
     duplicate of an owned gun.
   - A smoke step rolls the box.
   - Off by flag = no box (the web's game).
5. **U3-13.9, the ULTRAKILL alignment** (design flags in `design.ultra`, each with a `_why`):
   - **`freshDamage`:** when false, the arsenal's FRESH multiplies points and style but not damage. PRIMED, CROSS and
     the statuses are unchanged.
   - **`styleBreakPause`:** no style drain during a break, or while no live enemy is within `style.trickEnemyRange`.
   - **`parryRefillsDash`:** a parry refills the dash pips.
   - **`parryHitstop`:** 0.25 s on a parry (the perfect parry keeps its own).
   - **Tests:** each flag on and off; the golden suite unchanged with the flags off.
6. **U3-13.6, FX** (cut first): `weaponFx.js` (the rail's charge laser, dot and beam flash; the ricochet line) and
   effects.js's decals on hitscan `shotResult` only.
7. **U3-13.7, the close:** section 7, every line.
8. **Invariants at the end:**
   - `tests: N / N passed, 0 failed` with N > 339
   - every golden test passes (more than 84), replays 22 / 22
   - the Editor and Player smokes DONE, with the three guns' kills and a box roll
   - the Player's `-skygrindGolden` 0 mismatches
   - `git status --short` shows only `UnityConnectSettings.asset` (if modified) and untracked NIGHT7..NIGHT11.md

## 3. Notes for tonight's items (decided; don't re-open them)

- **U3-13.3:**
  - Copy p18 to p19. Add a `projectiles.mjs` recorder that builds a Scene as `trialRoutes.mjs` does: an arena with a
    wall and a floor, and fake enemies with known capsules (the web's `enemies` facade: `raycast`, `within`,
    `hitEnemy`, `aimPoint`, `knockback`).
  - It records `{op, args, after}` sessions.
  - Wire it into `all.mjs`; run all.mjs, then verify.mjs.
  - Collision goes through the C# web collider and the enemy registry (CLAUDE.md rule 4).
- **U3-13.4:** the loadout dispatches as weapons.js `step()` :276-299. Wall buys follow wallBuys.js (`weapon:<name>`,
  ARMORY open).
- **U3-13.8:**
  - The box is a Buyables entry like a wall gun: same hold-to-buy and same prompt style.
  - Its sim lives in Core (`MysteryBox.cs`); its view lives in Game.
  - Its pool reads the ported weapon catalog (`WeaponCatalog`), so N11's guns join with no change to the box.
  - The reveal and sink times are Unity-side numbers (`design.box`).
  - A taken gun counts as "fresh" for the arsenal, as a wall buy does.
- **U3-13.9:** a flag reads `GameConfig` through the `design.` prefix like the existing ones; ON / OFF per D-17, logged
  at boot.

## 4. Testing budget (build first)

- **Per item:** one `bash Tools/unity-loop.sh compile`, the item's own tests, at most one Play check (one eval file).
- **The harness first:** `dotnet run --project Tools/CoreHarness -c Release -- PortExport <set>` checks a new vector
  set in about a second. The Editor run stays the gate.
- **The full suite:** after each Core commit, and once at the close.
- **Play checks:** at most 5 tonight, each in one eval file, never timed across two evals. **Captures:** at most 4.
  Capture the box mid-roll for the report.
- **One heavy job at a time:** one Player build tonight, at the close (`build ... --confirm true`).
- **Caps are caps:** if a retake would cross a cap, report it short instead.

## 5. Mistakes not to repeat (NIGHT7-9)

1. **Patch scripts created through the shell** (NIGHT9: a `printf` and a heredoc). Create every `.cjs` / `.cs` patch
   with the Write tool, then run it in a second call.
2. **Unqualified names in an eval file:** no `using` lines; fully qualify (`Skygrind.Game.SliceSmoke.Begin()`). Read
   the eval's whole result (`success`, `errors`) before polling for its log lines.
3. **Play-check actors placed by guessing:** the spawn (0, 0, 40) is enclosed and THE GRINDER's gate blocks. Cast the
   collider. The ARMORY wall is at x 24, z 26.6-46.6.
4. **Numbers from TestContext in an Editor run:** run_tests keeps none. Print them through the harness with a scratch
   `.cs` (`-p:Extra=...` and a `[ModuleInitializer]`).
5. **Claims beyond the measurement:** write what the output line says, or "not measured".
6. **Builds rewrite files:** after the build, run `git diff --stat`.
   - Restore `Assets/Settings/Mobile_RPAsset.asset` (D-09) and any WebGL batching entry.
   - Clear a stale `M` with no diff by `git add` of the same bytes (check the hash first).
7. **Unfocused stalls:** `editor_focus` before every Play check.
8. **The tag after the close:** put every docs fix in the close commit, then tag once.

## 6. Order of work and budgets (budgets include each item's checks)

| Order | Item | Budget |
|---|---|---|
| 1 | U3-13.1 verify on arrival (and D-17 / D-20 answers) | 15 min |
| 2 | U3-13.3 projectiles (p19 sessions, Projectiles.cs) | 180 min |
| 3 | U3-13.4 STITCHER, COMET, LONGWAVE (+ wall buys, models, smoke steps) | 150 min |
| 4 | U3-13.5 homing | 45 min |
| 5 | U3-13.8 the Mystery Box | 90 min |
| 6 | U3-13.9 the ULTRAKILL alignment flags | 45 min |
| 7 | U3-13.6 weaponFx + decals | 45 min |
| 8 | U3-13.7 close | 60 min |
| if time | the first POWER gun (RATTLER, guns.js) on its own commit, so the box's pool grows | |

- **An item 25% over its budget** stops:
  - ship what works behind a design flag if it is unfinished
  - mark the item `in_progress` with what remains in its notes
  - move on, and come back to it after the others
- **The close starts by hour 10:30** whatever is left.
- **Cut order:** U3-13.6 first, then LONGWAVE's overcharge / walls variant, then homing.
- **Never cut:** U3-13.1, U3-13.3, U3-13.8, U3-13.7, and the rule that the flags off give the web's game.

## 7. Close (U3-13.7): the handoff loop

1. **The invariants in tasks.json, results in the log.** Rebuild the Player (scenes Survival then Arena), then run:
   - `git diff --stat` and the restores (section 5.6)
   - INV-smoke (with the three guns' kills and a box roll)
   - `-skygrindGolden`
   - one bench (`bash Tools/survival-bench.sh 60`, the plan polled)
2. **`tasks.json`:**
   - Every item touched tonight has a status and evidence: a commit **hash**, an output line, a time.
   - Never delete an item.
   - U3-13 passes when its acceptance is met.
3. **Docs:**
   - `PORT-STATUS.md`:
     - the guns, projectiles, aim assist, box and alignment rows
     - new event names checked against the web (railFire, railCharge, railCancel, overheat, rocketBlast, rocketJump,
       nailDetonate, rocketFragment are web names; the box's are port-only)
   - `PORT-PLAN.md` section 2: tick the rows done.
   - `ZOMBIES-ULTRAKILL-RESEARCH.md` section 3: update the box and alignment rows.
   - `PORT-RULES.md`: any rule learned twice, in its change table.
4. **Rewrite `HANDOFF.md`** with the `handoff` skill, in HANDOFF's own structure (rewrite, don't append):
   - a new anchor
   - the verify block with tonight's values
   - next actions = N11 (the run's shape)
   - decisions, risks, dead ends, invisible state
   - a changelog line
5. **`NIGHT10-REPORT.md` for GG (one page):**
   - what shipped, with commits
   - tonight's numbers
   - what is partial and why
   - a 10-minute play checklist, one question each:
     - STITCHER's nails and heat
     - COMET's blast and rocket jump
     - LONGWAVE's charge
     - homing on a V lock
     - the box's roll and teddy
     - does freshness-on-style feel better than on damage
     - does the break pause help
     - does the parry's freeze and dash refill feel right
6. **Write `NIGHT11.md`** (untracked) from the research's section 6 N11 row, in this file's shape:
   - re-read the code first and fill its facts block with measured numbers
   - split the next umbrella item into numbered items in tasks.json; existing acceptance and check stay as written
7. **Promote any lesson seen twice to memory** (one file each, an index line). Propose CLAUDE.md changes in HANDOFF's
   pending decisions; don't edit CLAUDE.md.
8. **Finish:**
   - Restore the TMP fallback asset if a Play run changed it.
   - Tag `u3-n10` once, on the close commit.
   - `git status --short` as in section 2.8.
   - Nothing pushed, nothing merged.

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

- **The oracle decides** for every web module: port line for line and let the vectors judge; never edit a test or a
  vector to pass.
- **New design stays behind flags.** With the flags off, one player plays exactly the web's game.
- **Write only what was measured.** Every number in the log, report and tasks.json comes from an output line tonight.
- **The web repo is read only** except the new untracked `port-export-p19/`. Movement stays bit-exact: golden and
  replays after every Core commit.
