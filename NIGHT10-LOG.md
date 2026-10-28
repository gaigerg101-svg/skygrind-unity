# NIGHT10 log (Unity stage 3, night 2: the ARMORY guns, the Mystery Box, the ULTRAKILL flags)

Times from `date`. Brief: `NIGHT10.md`. Branch `port/stage3` from `u3-n9` (plus the research commit `a93d443`). First
`date`: Sat Oct 3 2026, 10:02:57 AM.

**Run length (GG, 10:03 AM, mid-turn): "make hard stop 6 hours not 12".** Hard stop: 4:03 PM, not 10:03 PM. This
overrides NIGHT10 section 6's budgets, which add up to 10.5 h. GG's goal message also says nothing is cut tonight and
overrides the cut order: an item over its budget is parked, the others finish, then it is resumed until it passes. The
budgets are scaled to the 6 h (x0.55) and the close starts by 3:15 PM whatever is left.

| Item | NIGHT10 budget | Tonight (6 h) |
|---|---|---|
| U3-13.1 verify | 15 min | 10 min |
| U3-13.3 projectiles | 180 min | 95 min |
| U3-13.4 the three guns | 150 min | 80 min |
| U3-13.5 homing | 45 min | 20 min |
| U3-13.8 the Mystery Box | 90 min | 50 min |
| U3-13.9 the ULTRAKILL flags | 45 min | 25 min |
| U3-13.6 FX | 45 min | 25 min |
| U3-13.7 close | 60 min | 45 min |

Decisions for tonight (GG's goal message, 2026-10-03 morning; HANDOFF section 5 had no other answers):
- **D-17 ON:** freshness on style and points only, no style drain in breaks, a parry refills the dash, parry hitstop
  0.25 s. The `design.ultra` flags default ON; the golden tests and replays run with them off.
- **D-20 ON:** the Mystery Box ships behind its flag, ON.
- **D-18 off:** no hard damage or blood heal tonight.

Org-rule note: the material for this work is this repo and the web reference repo (read tonight). As on NIGHT9, no
Drive / Gmail / Calendar / Linear content is relevant to a game port, and none was read.

## U3-13.1 verify on arrival (10:03-10:08 AM)

RECEIVED (HANDOFF section 1, every row):
1. HEAD `a93d443` (the research commit "docs: CoD Zombies x ULTRAKILL research ..."), on `port/stage3`; `git tag
   --points-at HEAD` prints nothing. `git log u3-n9..HEAD` is that one commit, which HANDOFF's update note announces.
   As expected.
2. `git status --short`: ` M ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT10.md`, `?? NIGHT7.md`,
   `?? NIGHT8.md`, `?? NIGHT9.md`. As expected.
3. `unity-loop.sh status` at 10:04:05: drift. `"status": "playing"`, `"playMode": "playing"`. Idle 67 s (GG had just
   sent the 6-hour message). Stopped as HANDOFF row 3 says ("Exited play mode"); at 10:04:47 `"status": "ready"`,
   `"playMode": "stopped"`.
4. `unity-loop.sh tests` (10:05:09-10:05:44): `tests: 339 / 339 passed, 0 failed, 0 skipped`, `replays: 22 / 22`,
   `golden tests: 84 / 84`. As expected.
5. `dotnet run --project Tools/CoreHarness -c Release` (10:05:17): `golden total: 28 vector sets (30523 records), 22
   replays (22 exact); 0 failing, 0 mismatches`. As expected.
6. `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke-arrival.log` (10:06:14-10:06:52): the Player
   quit by itself; `[smoke] 25.1s DONE: all steps passed`; 0 lines matching "exception". As expected.
7. `Builds/Stage1/SKYGRIND.exe -skygrindGolden -logFile Logs/player/golden-arrival.log` (10:07:22): `[golden] golden
   total: 28 vector sets (30523 records), 22 replays (22 exact); 0 failing, 0 mismatches`, `[golden] DONE: 0
   mismatches in 4.3 s`. As expected.
8. Power plan `Turbo` (6fecc5ae-...), on AC (Win32_Battery BatteryStatus 2, 100%). Drift from NIGHT9's Silent: recorded.
9. Idle 67 s at 10:04 AM: daytime, and GG was at the machine a minute earlier. Play checks get batched into as few runs
   as possible, `editor_focus` before each.

Status U3-13.1: passing (drift: Play mode was on at arrival; stopped. The run is 6 h, not 12).

## How the night was run (10:15 AM)

Three background agents for the separable parts, none of them driving the Editor or committing on `port/stage3`:
- **the p19 recorder** (web repo, the new untracked `port-export-p19/` only), to a record format I gave it, so the C#
  checker could be written in parallel;
- **the Mystery Box core** (`MysteryBox.cs`, `BoxTests`, `design.box`, the spots by collider casts) in its own git
  worktree, cherry-picked by me;
- **the weapon models and viewmodel rigs** (`BlockyKit.cs`, `Viewmodel.cs` only).
I ported `projectiles.js` and the loadout's three guns, wrote the checkers, the game wiring and the smoke steps, and ran
every Unity gate.

## U3-13.3 projectiles, U3-13.4 the three guns, U3-13.5 homing (10:15-11:16 AM)

- **p19** (agent, 10:15-10:42): `port-export-p19/` copied from p18 plus `scripts/projectiles.mjs`, wired into `all.mjs`
  (`F  vectors/projectiles.json: 4 records (NIGHT10)`, `F  vectors/weapons.json: 2 records (NIGHT10)`). The 55 older
  output files are byte-identical to p18's (`cmp`). `verify.mjs` regenerates the two new sets: `sessions
  vectors/projectiles.json (regenerated, NIGHT10): 4 of 4 records (1458 ops) reproduced exactly, 0 mismatched`, `sessions
  vectors/weapons.json (regenerated, NIGHT10): 2 of 2 records (1307 ops) reproduced exactly, 0 mismatched`, `total: 0
  mismatches`. Sessions: nails (fire, stick, pierce, homing, detonate per loadout, release by death and by stickTime),
  rockets (splash falloff, radiusMult 1.4, 5 fragments, rocket jump, steer, homing, direct hit), lifetimes, clear; rail
  and railSpecial on the real `createLoadout` (pierce through 3, a wall stops it, cooldown, cancel, overcharge x2 with
  the kick back, the walls variant through both walls at momentum 1).
- **The agent's departures (accepted):** Math.random is seeded after the scene is built (three's `generateUUID` draws
  4 per object, which the C# never makes), so only gameplay draws count; a 4th `lifetimes` session; weapons.json
  records events only. It also confirmed the web's `stick()` aliasing bug from the data (every stuck nail sits at the
  aim point; detonations come `from` the aim point).
- **Projectiles.cs** (projectiles.js line for line; N11's kinds stubbed; no orbs until N13), the **loadout's** fireNail /
  fireRocket / fireRail, heat, specials, variants, newShot / groupShot, routeSpecial, the QUICK charge multiplier, and
  **AimAssist.HomingTarget**. Harness: `PASS projectiles: 4 records, 0 mismatches` (after one checker fix: a boxed int
  passed to `Same`), `PASS weapons: 2 records, 0 mismatches` on the first run. A one-ulp mutation of each file
  (scratch copy) FAILs: `calls[0].amount: expected 9.5 got 9`, `railCharge: expected 0.0138888888888889 got
  0.01388888888888889`.
- **Tests:** ArmoryTests 5, HomingTests 4 (the web's two homeVelocity tests from assistMath.test.js :21-45 were not
  ported before tonight), ZeroAllocTests + `ProjectilesStep_InFlight_AllocatesNothing`. The first full run (10:49-10:50)
  was `349 / 350`: `ZeroAllocTests.SimSteps_With30Enemies_AllocateNothing` failed, `Loadout.Step 3000` (my
  `$"ads.weapons.{Weapon}"` lookup every step); cached per weapon. 10:52-10:53: `tests: 351 / 351 passed, 0 failed`,
  `replays: 22 / 22`, `golden tests: 86 / 86`. Commit `748ae8a` (the Core half of U3-13.3 / .4 / .5 in one commit: the
  loadout's guns, the projectiles' loadout interface and the homing call are one change in Combat.cs).
- **The weapon models** (agent, 10:15-10:33): Stitcher / Comet / Longwave in BlockyKit from weaponModels.js :134-212,
  `ForWeapon(id)`, the viewmodel rigs and viewmodel.js animateParts (heat strip, drum, charge glow, rings). Measured
  lengths 0.483 / 0.770 / 1.030 m. Not drawn: the extrude bevels (2-8 mm smaller outlines).
- **The game:** WallBuys hangs every ported gun at its armoryWall slot; ProjectileViews draws nails, stuck nails and
  rockets; SurvivalGame steps Projectiles after the loadout, the buyables and READY, before the enemies (main.js
  simulate); the SPECIAL key (B). Commit `58cc73f` (with the box's game half: both live in SurvivalGame.cs and
  SliceSmoke.cs). 11:03-11:04: `tests: 363 / 363 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 86 / 86`.
- **Decision (logged, no GG input needed):** the SPECIAL key is B (config `input.bindings.special: 'KeyB'`); right
  mouse is the special on a gun that does not zoom (ads.js routeSpecial).

Status U3-13.3: passing. U3-13.4: passing (smoke below). U3-13.5: passing.

## U3-13.8 the Mystery Box (10:15-11:16 AM)

- **Core** (agent, worktree, commit `d0a0c1a`, cherry-picked as `da23e7c` at 10:48): `MysteryBox.cs` (Idle,
  Revealing, Offering, Teddy, Moving; its own Mulberry32 from the run seed xor `design.box.seedSalt`; the CoD ladder;
  a teddy refunds what was spent and moves the box to another open spot), `Economy.Refund` (not earned, no BOUNTY),
  `design.box` with a `_why`, 3 spots by collider casts (LANDING (-8.4, 0, 30), THE GRINDER (19, 0, 22.9), SLAB GARDENS
  (0, 0, -46.9): floor y 0, a 1 m x 2.2 m capsule touches nothing, inside the zone, reachable on foot from the spawn),
  BoxTests 12 (passed in Unity in the 11:03 run). Events: boxOpen, boxRoll, boxTeddy, boxMove, boxTake, boxSink
  (port-only).
- **Game:** MysteryBoxView (a dark crate with the cyan trim, a lid, the cycling gun hologram, a blocky grey teddy, the
  sink-and-rise move), two Buyables entries (MYSTERY BOX for its price; TAKE <gun> while one is offered), the HUD prompt
  `HOLD E · MYSTERY BOX 950`, a teddy only moves to a spot whose zone is open.
- **Decision:** a taken box gun joins the loadout as a wall buy does (wallBuys.js :4, :158: the next slot, in hand).
  NIGHT10's "swaps it into the slot in hand" read literally would replace the gun in hand (CoD's two-gun limit); the
  port has no slot cap, so that was rejected.
- **Play check 1 of 5** (11:08-11:10): the smoke's new steps FAILed: `FAIL Mystery Box rolled ... taken: owned
  [revolver, gauntlet]`, `FAIL STITCHER: no open floor in sight for a target` (and COMET, LONGWAVE), `FAIL death:
  Results`. Causes (mine, in the smoke script): VIGOR's apply reset health.max, so the round-2 husks killed the player
  during the 40 s of new steps (the run was over, nothing could be bought or taken); and the target search ran
  through the wall (`-facing`). The captures worked (`Logs/shots/n10-box-roll.png`, `n10-armory-wall.png`), but the
  crate drew tan: `mats.lit` ignores MeshKit's vertex colours.
- **Play check 2 of 5** (11:14-11:15), after commit `5465598` (god mode and frozen husks in the new steps, the search
  into the hall, the crate on the kit material): `[smoke] 22.4s PASS Mystery Box rolled at spot 0 (landing): paid 950,
  offered shotgun, taken: owned [revolver, gauntlet, shotgun]; events [boxOpen, boxRoll shotgun, boxTake]`, `PASS
  STITCHER (bought 1350 -> 100): a husk 8.0 m away killed in 0.6 s; kill weapons [nailgun]`, `PASS COMET (bought 2100
  -> 100): a husk 8.0 m away killed in 0.5 s; kill weapons [rocket, rocket, rocket]`, `PASS LONGWAVE (bought 3100 ->
  100): a husk 8.0 m away killed in 0.8 s; kill weapons [rail]`, `[smoke] 32.1s DONE: all steps passed`. Capture 3 of
  4: `Logs/shots/n10-box-roll-2.png` (mid-reveal, `reveal 0.64`).

Status U3-13.8: passing.

## U3-13.9 the ULTRAKILL alignment flags (11:17-11:28 AM)

- `design.ultra` (Design/unity-config.json and the Player's copy) with a `_why`: `freshDamage` false, `styleBreakPause`
  true, `parryRefillsDash` true, `parryHitstop` 0.25 (D-17 ON, GG). `Core/Ultra.cs` reads them; Core objects keep the
  web's rules by default and SurvivalGame applies the flags at boot, logging `[ultra] ...`. So the golden sets and the
  replays (config.json without the design config) and the existing tests (ArsenalTests' FRESH x1.25 damage) are the
  web's.
- **freshDamage off:** the loadout's damage uses `ArsenalHit.multNoFresh` (primed x cross / combo); the hit keeps
  `freshMult`, so points and style still pay FRESH.
- **styleBreakPause:** the meter's drainMult is 0 in a break or with no live enemy within style.trickEnemyRange.
- **parryRefillsDash: measured, and NIGHT10's facts block was wrong.** It says "a parry doesn't touch the dash pips",
  but the web's parries already refill them: punch.js :36-39 ("A parry: heal, full stamina") and melee.js :105 call
  `owner.mover.restoreStamina()`, which sets `this.pips = this.maxPips` (mover.js :244-246); the port's Parried does
  the same. So ON is the web's rule; an explicit false skips the refill (an A/B). Logged, flag kept.
- **parryHitstop:** 0.25 s on a plain parry; the PERFECT PARRY keeps design.parry.perfectHitstop (0.15).
- UltraTests 6 (each flag on and off). 11:26-11:27: `tests: 368 / 368 passed, 0 failed`, `replays: 22 / 22`, `golden
  tests: 86 / 86`. Commit `7844541`.

Status U3-13.9: passing.

## U3-13.6 weaponFx + decals (11:29-11:46 AM)

- A background agent wrote `Game/WeaponFx.cs` (weaponFx.js: the rail's laser and dot while charging, the beam flash on
  `railFire`, the ricochet bounce line; not the Prism preview) and `Game/Decals.cs` (effects.js decals on hitscan world
  hits, `rayCount > 0`, none on a force field). I moved its three Unity-only numbers into `UnityTuning.cs` (CLAUDE.md
  rule 5) and wired both into SurvivalGame.
- **Play check 3 of 5** (11:41-11:42): `[n10c] decals: 0 -> 4 after 5 KESTREL shots at the wall; rail: charge 0.40
  laser True dot True; fired True, beam on 2 frames later True, beam after 0.6 s True, laser after True; decals now 5`.
  **Slip:** the laser / dot / beam values read `activeSelf`, which WeaponFx never changes (it toggles the renderers), so
  they are not evidence. The decal counts are (Decals toggles its quads' active state). Capture 4 of 4:
  `Logs/shots/n10-rail-beam.png` shows the beam flash from the muzzle to the wall and the decals by the crosshair.
- 11:44-11:45: `tests: 368 / 368 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 86 / 86`. Commit `1266a39`.

Status U3-13.6: passing.

## U3-13.7 close (11:47 AM -)

- **The Windows Player** (11:47:55-11:48:21, the night's one build): `build --target StandaloneWindows64 --outputPath
  Builds/Stage1/SKYGRIND.exe --scenes [Survival, Arena] --confirm true`: `Succeeded`, 0 errors, 8 warnings;
  `Builds/Stage1` 154,799,071 B; StreamingAssets/PortExport/vectors holds 30 files (projectiles.json and weapons.json
  included).
- **git diff --stat after it:** `Assets/Settings/Mobile_RPAsset.asset` rewritten (Unity's URP upgrade, D-09): restored
  with `git checkout --`. Nothing else from the build. `UnityConnectSettings.asset` never staged.
- **INV-smoke** (11:49:41-11:50:23): `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke.log`:
  `[smoke] 22.4s PASS Mystery Box rolled at spot 0 (landing): paid 950, offered shotgun, taken: owned [revolver,
  gauntlet, shotgun]; events [boxOpen, boxRoll shotgun, boxTake]`, `[smoke] 23.6s PASS STITCHER (bought 1350 -> 100): a
  husk 8.0 m away killed in 0.7 s; kill weapons [nailgun]`, `[smoke] 24.5s PASS COMET (bought 2100 -> 100): a husk 8.0 m
  away killed in 0.5 s; kill weapons [rocket, rocket, rocket]`, `[smoke] 25.9s PASS LONGWAVE (bought 3100 -> 100): a
  husk 8.0 m away killed in 0.8 s; kill weapons [rail]`, `[smoke] 32.1s DONE: all steps passed`; 0 lines matching
  "exception"; the boot line `[ultra] freshDamage off: FRESH pays points and style only, styleBreakPause on,
  parryRefillsDash on, parryHitstop 0.25 s`.
- **-skygrindGolden** (11:50:55-11:51:10): `[golden] golden PASS projectiles: 4 records, 0 mismatches`, `[golden] golden
  PASS weapons: 2 records, 0 mismatches`, `[golden] golden total: 30 vector sets (30529 records), 22 replays (22 exact);
  0 failing, 0 mismatches`, `[golden] DONE: 0 mismatches in 4.6 s`.
- **Bench** (11:51:31-11:52:41, `bash Tools/survival-bench.sh 60`): `power seen: 11 Balanced AC=True`; `1920x1080,
  NVIDIA GeForce RTX 5070 Laptop GPU`; `bench result: 18496 frames, alive at end 31, median 2.91 ms, p95 6.24 ms, worst
  1% 8.69 ms, mean fps 309, median fps 344`; `bench gc: collections 16 in 18526 frames; GC Used Memory rises 103916.0
  KB`; `FrameTimingManager cpu median 2.91 ms, worst 1% 8.72 ms; gpu median 0.97 ms, worst 1% 1.06 ms`. Against NIGHT9's
  (Silent plan: median 2.49 ms, worst 1% 4.49 ms, 75,672 KB, 10 collections) the plan differs (Balanced now; Turbo at
  arrival: it flipped during the day), so frame times don't compare. The managed growth per frame rose (about 5.6 KB
  against 3.3 KB). Reading the new step and render paths (Projectiles, the box, PlaceBox, EnemyNear, the views,
  WeaponFx) found no allocation; the cause is not measured (it needs a Development Player profile, which is a second
  build). Logged as a risk in HANDOFF.
- **Harness** (11:58): `golden total: 30 vector sets (30529 records), 22 replays (22 exact); 0 failing, 0 mismatches`.
- **INV-tests / INV-golden / INV-replays** (12:00:34-12:01:16): `tests: 368 / 368 passed, 0 failed, 0 skipped`,
  `replays: 22 / 22`, `golden tests: 86 / 86`.
- **Docs:** `tasks.json` (U3-01, U3-13, U3-13.1 .. U3-13.9 statuses and evidence; U3-24 + U3-24.1 .. U3-24.9 proposed
  for NIGHT11, from the research's N11 row; GG owns their acceptance), `PORT-STATUS.md` (the oracle, combat, Survival,
  style, FX and debug rows; the 8 new web event names checked in weapons.js / projectiles.js; the box's 6 events
  port-only), `PORT-PLAN.md` section 2 (STITCHER / COMET / LONGWAVE, wall buys 4, homing, the kit models, a new-design
  row; the co-op row moved to the co-op night), `ZOMBIES-ULTRAKILL-RESEARCH.md` section 3 (wall buys, the box,
  freshness), `PORT-RULES.md` (section 6: no strings built per step or per frame; the change table),
  `HANDOFF.md` rewritten (anchor `u3-n10`; D-23, D-24 new; D-10 extended with (h), (i)), `NIGHT10-REPORT.md`,
  `NIGHT11.md` (untracked). Memory: `night-parallel-agents` (new), `play-check-eval-recipe` (extended).
- **CLAUDE.md:** not edited; proposals are D-10 (h) and (i) in HANDOFF.
- **The "if time" item** (the first POWER gun, RATTLER) was not started: it needs guns.js vectors (a p20 recorder) and
  the dart path in Projectiles; NIGHT11's U3-24.7 does it with TORCH.
- The TMP fallback asset was not modified by tonight's Play runs (`git status` clean of it after each).
- The web repo (12:02): branch `night6` at `p17-night6`; `git status --short` shows the pre-existing ` M vite.config.js`
  and untracked files plus the new `?? port-export-p19/` only: no tracked file changed.
- HANDOFF.md also copied to `%TEMP%\SKYGRIND-HANDOFF-u3-n10.md` (the handoff skill's default location; the repo copy
  is the one that counts, NIGHT10 section 7.4).

Status U3-13.7: passing (the tag `u3-n10` and INV-clean are on the close commit, checked right after it).
Status U3-13: passing.

