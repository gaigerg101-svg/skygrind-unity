# NIGHT9 log (Unity stage 3, night 1: the oracle and the arsenal)

Times from `date`. Brief: `NIGHT9.md`. Branch `port/stage3` cut from the plan commit `d3c9249`. First `date`:
Fri Oct 2 2026, 9:30:28 PM. Hard stop: Sat Oct 3, 9:30 AM. The close starts by 8:00 AM (hour 10:30) whatever is left.

## U3-06 verify on arrival (9:30-9:34 PM)

RECEIVED (HANDOFF section 1, every row):
1. HEAD `d3c9249` ("docs: the full-parity plan ..."), on `port/stage2`, parent `db0ec85` (= `u2a-demo`). As expected.
2. `git status --short`: `?? NIGHT7.md`, `?? NIGHT8.md`, `?? NIGHT9.md`. As expected (UnityConnectSettings.asset not
   modified at this moment).
3. `unity-loop.sh status`: drift. `"status": "playing"`, `"playMode": "playing"` at 9:31 PM, idle 103 s (GG had just
   started the run and left). Stopped it as HANDOFF row 3 says ("Exited play mode"); then `"status": "ready"`,
   `"playMode": "stopped"`. `git status --short` unchanged after the stop.
4. `unity-loop.sh tests` (9:32:32-9:33:00): `tests: 237 / 237 passed, 0 failed, 0 skipped`, `replays: 16 / 16`,
   `golden tests: 55 / 55`. As expected.
5. `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke-arrival.log` (9:33 PM): the Player quit by
   itself (exit 0); `[smoke] 25.1s DONE: all steps passed`; 0 lines matching "exception". As expected.
6. Power plan `Balanced` (381b4222...), on AC (PowerOnline True).
7. Idle 103 s at 9:31 PM (GG started the run). It is night: Play checks are fine after GG leaves; `editor_focus` before
   each one.

Org-rule note: the material for this work is the repo and the web reference repo (both read tonight); no Drive / Gmail /
Linear content is relevant to a game port, and none was read.

Branch `port/stage3` created from `d3c9249` at 9:34 PM.
Status U3-06: passing (drift: Play mode was on at arrival; stopped).

## U3-07 the oracle + U3-04 determinism (9:34 PM - 11:40 PM)

How it was run: four background agents for the separable parts (each with its own copy of a dotnet harness that
compiles Skygrind.Core outside Unity; none touched the Editor or git), the integration and the Unity gates by me.
Two agents (fdlibm, new sets) stopped at 10:32 PM on an API spend limit with their work half done; I finished both.
- **The oracle in Core** (`Assets/Scripts/Core/Golden/`): GoldenData (a PortExport folder), GoldenResult (bitwise
  comparisons), the set registry (partial methods, one per checker file), the trajectory replays (`Golden.Replay`, with
  the trial routes' pads and core). The EditMode tests (`OracleGoldenTests.Golden_Set(set)`, the legacy golden tests'
  bodies), the Player (`-skygrindGolden`) and the harness run the same code. The orphan test fails on a vector file
  with no checker.
- **The legacy golden tests moved into Core** (agent): movementMath, swingMath, combatMath, styleMath, directorMath,
  assistMath; the 55 legacy test methods keep their names; records the old tests skipped are now checked
  (styleMath FRESH_WEAPONS, directorMath createDirectorState / stepDirector / stepCamp; `DirectorMath.Phases` ported for
  the PHASES constant).
- **Unconsumed sets consumed:** gunMath, spawnMath, weaponMath, arsenalMath, momentumMath (agent: GunMath.cs,
  SpawnMath.cs, WeaponMath.cs, ArsenalMath.cs, MomentumMath.cs, line for line, plus 42 ported web tests); styleCore and
  styleSim (me: the style meter split as the web has it, `StyleCore.cs` = styleCore.js line for line, `StyleMeter.cs` =
  styleMeter.js; `StyleSim.cs` = styleSim.js). The styleCore session (107 ops) and styleSim (paceReport's 19 runs + 2)
  passed on the first run.
- **New sets** (agent recorders in port-export-p18 vectors.mjs; checkers by the agent and me): regenMath, pointsMath,
  modMath, powerUpMath, deathRecap, hitDirection, kicks, springs, tokens, slots, aim, crowd, watchdog, flowField. Port
  fixes the new vectors forced (agent, logged in the files): token holders in Map order (a Dictionary reused slots after
  a steal: a tie took the wrong loser), slots.js flankPoint's grouping (`yaw + (PI + side PI/3)`), deathRecap's source
  names, the spring helpers' Math.sign, aim's `|| 1` for NaN, NavField's heap keys compared unrounded and its path
  scratch as float32; `Slots.cs` new (slots.js); crowd's separateBodies as a pure function.
- **The 6 trial routes as trajectories** (me: port-export-p18 `trialRoutes.mjs` mirrors trialRunner.js runTrial and
  records every step; each route finishes on runTrial's own step count and its par): trial_slideLine 3343 steps,
  gapRunner 3356, wallRider 3398, slamDeck 3770, targetRun 3785, skybridge 4082 (spire, core `flat`). Replayed with the
  map's pads after every step: bit-exact in the harness and in Unity on the first try.
- **U3-04, V8's maths:** `JsMath.Fdlibm.cs` (agent: V8 13.6 ieee754.cc atan2, atan, exp, tan + rem_pio2, acos, asin,
  tanh + expm1; fdlibm pow), `JsMath.Log.cs` (me: e_log.c, springs.js uses Math.log), and 44 System.Math calls in 15
  Core files swapped (`swap-jsmath.cjs`: Atan2, Exp, Tan, Tanh, Acos, Asin, Atan, Pow, Log, and System.Math Sin / Cos in
  HuskBrain and GrappleAim). After the swap hitDirection and springs went from 34 / 46 last-bit mismatches to 0: the
  System.Math results were the ones off, as NIGHT9 predicted. Every golden and the 22 replays stayed bit-exact.
- **Math.pow:** V8 13.6's Math.pow is not fdlibm: past its own cases (NaN exponent, (±1) ** ±∞, y == 2, y == 0.5) it
  calls the host's std::pow (the UCRT on Windows). Measured on 3,592 recorded pow inputs: System.Math.Pow (CoreCLR) 1
  mismatch, Mono's (Unity) 25 (subnormal results), fdlibm 245, an exact correctly rounded integer pow 3 of 1,509 (the
  UCRT's own non-correctly-rounded results: 10 ** 23 is an exact tie it rounds away from even; 0.9999999999999999 ** -1).
  Decision: JsMath.Pow = V8's own cases, then deterministic (integer exponents |y| <= 2048 correctly rounded from the
  exact value, cached; the rest fdlibm). The sim calls pow only with integer exponents (style.diminish.repeat ** n, the
  LAB's 0.96 ** n) or ** 2 (V8's x * x shortcut). Rejected: the host pow (not deterministic: Mono 25 mismatches) and
  porting the UCRT (closed). The jsMath set records pow only where V8's value doesn't depend on the host: 1,875 kept,
  1,714 dropped (non-integer exponents on ordinary bases), 3 dropped (the UCRT ties above); the counts and the 3 are in
  the file's header. GG may want the non-integer pow checked some other way later (HANDOFF risk).
- **jsMath set:** 25,248 records (atan2, exp, tan, acos, asin, atan, tanh, pow, round, sin, cos, hypot 2/3, log, min,
  max); 0 mismatches in the harness (CoreCLR) and in Unity (Mono). JsJson now reads subnormals (they were "outside the
  normal double range"). JsMath.Round fixed: x in [-0.5, 0) gave +0, JS gives -0 (agent find).
- **DeterminismTests:** fails on any System.Math transcendental in Core outside JsMath*.cs (Settings.cs Log10, UI text,
  allowed by name).
- **The Player's oracle:** `-skygrindGolden` (PlayerChecks) runs `Golden.RunAll` on StreamingAssets/PortExport;
  `SKYGRIND/Sync Port Data` copies vectors/, trajectories/ and the spire / training maps there (gitignored copies;
  `PortDataBuildSync` runs the sync before every build; a Web build gets no oracle). Its run is at the close (U3-12).
- **p18:** `node port-export-p18/scripts/all.mjs` (11:27 PM) regenerated everything; the 13 old vector files, 16
  trajectories, config and maps are byte-identical to p17's; `verify.mjs`: `vectors: 30183 calls reproduced exactly, 0
  mismatched, 36 skipped (injected callbacks)` (after the pow scope; 31900 before it), every trajectory "all N steps
  reproduced exactly" (the trial routes after fixing verify's Mover owner to runTrial's 'trial'). Copied into PortExport.
- **Checks:** harness (11:36 PM) `golden total: 28 vector sets (30523 records), 22 replays (22 exact); 0 failing, 0
  mismatches`. Unity (11:37-11:38 PM): `tests: 315 / 315 passed, 0 failed, 0 skipped`, `replays: 22 / 22`,
  `golden tests: 84 / 84`.
- Decision: U3-07 and U3-04 in one commit: the new sets pass only with V8's maths and the jsMath set needs the oracle
  (two commits would leave the first failing golden). Arsenal.cs (U3-08, started) stays out of it.
- Rules learned (PORT-RULES change table): transcendentals JsMath only + the pow rule; float grouping; Map order for
  deleting / re-inserting maps.

Status U3-07: passing. U3-04: in_progress until the rebuilt Player's `-skygrindGolden` (U3-12).
Commit `ff72de9` (11:43 PM).

## U3-08 the arsenal (11:43 PM - 12:04 AM)

- `Arsenal.cs`: combat/arsenal.js line for line on ArsenalMath (FRESH per weapon, the primed swap, cross combos with the
  five statuses, COLLAPSE / OVERLOAD, burn ticks, OVERCLOCK, FLIP's mods, the SWAP kill). The web keys its records by
  the enemy object in a Map: here an ordered list + an index, compacted after the step's walk (a record added during
  the walk is visited, a deleted one is not, as a Map does).
- The loadout (Combat.cs) feeds it where weapons.js / melee.js / punch.js do: `step` before the swap, `swapIn` at the
  raise, `use` after each shot / swing (the punch wears the Gauntlet). `Damage()` now follows weapons.js's order: the
  arsenal's tags, the multipliers (x tags.mult, DEADEYE's headshotMult ready for U3-11), the HP, `hit` / `kill`, the
  arsenal's `after`, then the bonus hook's after. The hit carries freshMult / primed / cross / combo, so points pay
  x FRESH as on the web (Economy already multiplied by freshMult).
- The style meter's `getFresh` from the arsenal (main.js), `crossHit` / `swapKill` pay CROSS / COLLAPSE / OVERLOAD /
  SWAP; OVERCLOCK goes to the arsenal (powerUps.js) and the meter's stand-in is gone; `Forge.status` = the arsenal's
  (a forged gun's status lands); FLIP's machine placed (`Perks.Ported`) and its effects applied (`arsenal.SetMods`);
  the HUD's FRESH tag reads the arsenal (0..1 shown on the meter's tiers, the scale style pays it on).
- Statuses' visuals: nothing on the web listens to 'status' (no burn / shock tint exists to port); statuses are tags
  (cross families, combos) and burn ticks.
- Tests: `ArsenalTests` 7 (the web's two arsenal-system tests from arsenal.test.js, a loadout session where a swap
  primes and a family change crosses and the meter pays CROSS, a forged THUNDERCLAP's STAGGER landing, FLIP, OVERCLOCK,
  burn ticks). Three test rigs that measure base damage (CombatTests, ForgeTests, SpitterTests) now turn the arsenal
  off, as the web's sandbox presets do (main.js: "the presets measure exact damage and points"); with it on they saw
  x1.25 FRESH (6 failures, expected). One test of mine was wrong (FLIP's recovery measured into the cap), fixed.
- 11:55 PM `tests: 322 / 322 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 84 / 84`.
- **Play check 1 of 5** (12:00-12:02 AM, one eval starting a coroutine, the smoke's input path): `[check9] hit revolver
  dmg 31.25 fresh x1.25 primed False cross False`, `[check9] hit shotgun dmg 81 fresh x1.25 primed True cross True`,
  `[check9] arsenal events: primed shotgun, crossHit CROSS shotgun`, `[check9] style awards: HIT 6, HIT 4, CROSS 8`,
  `[check9] CROSS paid: True`; `[check9] FLIP at (20, 0, 47.3) zone 1: owned True, points 1100 -> 100 (price 1000)`.
  The eval body takes no `using` lines (it is wrapped in a method): fully qualified names.
- Slip: one patch script (`arsenal-off-rigs.cjs`) was generated with printf in bash instead of the Write tool (rule 9's
  intent); the rest of tonight's patch files go through Write.

Status U3-08: passing. Commit `7b9104c` (12:04 AM).

## U3-09 momentum, impact waves, the arc (12:04 AM - 12:25 AM)

- `Momentum.cs`: combat/momentum.js line for line on MomentumMath (the 60 ms smoothed velocity, the 0.35 s gain ring,
  `At` / `Along`, `force` for debug, reset with the run).
- `Impact.cs`: combat/impact.js: the trigger (impactWave's fires / share / knockback, the cooldown), the wave hit (a
  share of the blow through the loadout's damage with `shock`, a push, a stun, the arsenal's STAGGER) and the ring wave
  (the PULSE GAUNTLET and the punch, the port's only melee). The crescent / echo / quake / crack come with their weapons
  (N12): trigger refuses them before spending the cooldown (logged in the file).
- `Enemy.Stun` (enemyKind.js) and `Husk.Stun` (husk.js): the wave's stagger.
- The loadout: momentum and the impact cooldown step before the arsenal (weapons.js order); `Damage` scales by
  MomentumDamage (melee x lerp(1, 3, M), guns x lerp(1, 1.35, M); not a wave's share, not secondary damage, only catalog
  weapons), the hit carries `momentum` and `shock`; a melee hit pays IMPACT above momentum.impactBonusAt and triggers its
  wave; `AimMomentum()` for the arc.
- The momentum hitstop (main.js OVERNIGHT2 N8): a player melee hit, not a wave's, with M > feel.momentumHitstopAt freezes
  feel.momentumHitstop (the longest request wins).
- The arc (`UiMomentum.cs`, ui/momentumArc.js): a ring of hud.momentumArc radius / width round the crosshair, radial
  fill clockwise from the top with M along the aim (the web's 2-decimal step), the rank colour above momentum.arcAt,
  else idleColor (CSS rgba parsed); a faint ring behind it.
- The impact FX, simple tonight (`ImpactFx.cs`): an expanding flat ring (0.2 + 0.8 (1 - (1 - t)^2) of the radius over
  momentum.shock.ring.time, held with REDUCED MOTION) in the kinetic family's colour x fx.impact.glow fading out, and the
  web's particle burst (18 x k). The full impactFx.js look is N12.
- Tests: `MomentumTests` 3 (the web's skipped "a dash toward a target from a walk reaches M above the impact threshold";
  a Gauntlet hit at M 0.9: x lerp(1, 3, M), IMPACT, the ring hits the neighbour with a shock share and staggers it, the
  hit carries the momentum the hitstop reads, the cooldown, the punch's ring; a slow hit: no wave, a gun x gunMax at M 1,
  burn keeps its number). 12:24 AM `tests: 325 / 325 passed, 0 failed, 0 skipped`, `replays: 22 / 22`,
  `golden tests: 84 / 84`.

Status U3-09: in_progress until the combined Play check (the arc tracking M in the running game). Commit `3bba47f`
(12:25 AM).

## U3-10 the V lock and the lunge (12:25 AM - 12:38 AM)

- AimAssist (aimAssist.js): the manual lock: `ToggleLock()` (gather within aim.lockRange and aim.lockCone, the candidates
  in sight, `nextLock` from the manual target: the nearest, then the next, wrapping; with no other it releases, blocked
  for reacquireDelay); `StartLock(..., manual)` emits lockOn (and lockOff 'switch' when an automatic lock replaces a
  manual one), `EndLock` emits lockOff with the reason; the scan keeps a manual lock to max(cone, aim.lockCone) x
  releaseCone and manualLosLost of blindness and never lets a better candidate take it; `Info().manual`;
  `ManualTarget`.
- `LungeTarget` (lungeTarget): the body nearest the aim within range + aim.lunge and the cone (OFF: none). The punch
  (punch.js) and the Gauntlet swing (melee.js assistLunge) lunge (an impulse closing the gap over aim.lungeTime, the
  'lunge' event) and land; a lunged hit counts as assisted, as on the web.
- V (input.bindings.lockOn) toggles the lock at input time while playing (main.js input:lockOn).
- Tests: `LockOnTests` 4 (the web has no tests for these: they check the module's documented rules). One test of mine
  assumed the Gauntlet had to be bought (weapons.survivalStart already holds it), fixed. 12:37 AM `tests: 329 / 329
  passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 84 / 84` (assistMath 133 records unchanged).

Status U3-10: passing (a V press in the running game rides the combined Play check). Commit `250930e` (12:38 AM).

## U3-11 four perk machines (12:38 AM - 12:56 AM)

- `Perks.Ported` += bounty, deadeye, flow, slipstream (with FLIP from U3-08: ten of 14 machines placed at
  data.perkSpots; STEADY, ANVIL, STATIC, PHOENIX wait for their systems, N12).
- perks.js apply (PerkMachines.Apply): SLIPSTREAM the mover's slide mods (the Mover already had slideMods), DEADEYE
  the loadout's headshotMult and the economy's headshot points, BOUNTY the economy's points, FLOW the style meter's
  perk mods.
- Economy (economy.js perk): `SetPerks(points, headshot)`; earn() rounds amount x BOUNTY; a headshot hit adds DEADEYE's
  points (reason 'deadeye').
- PerkTests +5: the ten machines' spots, BOUNTY / DEADEYE points, DEADEYE damage, FLOW gain / drain, SLIPSTREAM's
  slide. 12:55 AM `tests: 334 / 334 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 84 / 84`.

## The Play checks for U3-09 / U3-10 / U3-11 (12:48 AM - 12:55 AM)

- **Play check 2** (12:50 AM): the ring follows M during a dash: `[check9b] dash t0: dist 14.0 M aim 0.3 ring 0.30
  colour -1`, `t12: ... M aim 0.557 ring 0.56 colour 0`, `t60: ... M aim 0.896 ring 0.90 colour 0`, then the dash stopped
  (`dist 8.4`) and the ring fell back (`t84: ... M aim 0 ring 0.00`). The four perks bought at their machines (hold E):
  `bounty at (15.4, 0, 47.3): owned True, points 2350 -> 100 (price 2250)`, `deadeye at (22, 0, -22): owned True, points
  2030 -> 100 (price 1930)`, `flow at (-40, 29.2, 40): owned True, points 3700 -> 100 (price 3600)`, `slipstream at
  (-25.5, 0, 32.5): owned True, points 1730 -> 100 (price 1630)`; `[check9b] effects: headshotMult 1.5, slide x1.25
  friction x0.75 jump cap x1.25, style drain x0.6 gain x1.2`. Capture 1 of 4: Logs/shots/n9-momentum-arc.png (the ring
  round the crosshair at a forced M 0.9: thin, hud.momentumArc radius 20 px width 2, in the DULL rank's colour). The
  husks stood behind THE GRINDER's locked gate: the dash stopped at 8.4 m and the lock saw nothing (no punch, no V).
- **Play check 3** (12:52 AM): turned round, but the husks were behind a wall again (the dash stopped at 5.35 m; V found
  none). My placement was a guess.
- **Play check 4** (12:53-12:54 AM): the line picked by casting the game's collider on 16 headings from the spawn
  (`spawn (0, 0, 40); ... using 338 deg (17.3 m)`): `[check9d] V: 29 -> 30 -> 29 (husks 29, 30); manual True; events
  lockOn 29, lockOn 30, lockOn 29`, `after 0.5 s: target 29, manual True`, `with one husk, V twice: ... lockOff toggle,
  lockOn 29`; `[check9d] punch at 2.35 m (reach 2): punch M 0.881 shock False dmg 33.7 | punch M 0 shock True dmg
  21.95`, `[check9d] impact waves: ring M 0.881 radius 4.34 hits 1; hitstops: momentum`.
- Play checks used: 4 of 5; captures: 1 of 4. Lesson (memory): place a Play check's actors by casting the collider,
  not by guessing open floor: the spawn is enclosed.

Status U3-09: passing. U3-10: passing. U3-11: passing.

## Tools: the Core harness committed (12:57 AM)

- `Tools/CoreHarness` (926ebb8): Skygrind.Core compiled with dotnet outside Unity, running the same golden oracle on
  `PortExport` or a `port-export-pNN` folder (`dotnet run --project Tools/CoreHarness -c Release -- [dir] [set |
  replay:name ...]`), output under `Temp/CoreHarness`. A fast loop (about a second), not a gate: the gates stay the
  Editor's EditMode run (Mono) and the Player's `-skygrindGolden`.

## The "if time" item: `Player[]` in Core, first slice (1:02 AM - 1:25 AM)

The survey (an Explore agent): the web has one fixed target (main.js `getTarget = () => target`), no `nearestPlayer`;
RIVAL is a target system the player's weapons hit, never a second player the enemies hunt. COOP-RESEARCH.md plans
`getTarget(from)` (nearest player), a seeded flow field, owner ids on events. So this is new design on both sides and
has no vectors; the rule to keep is "one player: exactly as before". Time box: 4:00 AM; done at 1:25 AM.

- `Core/Players.cs`: `Player` (id = the events' owner, Mover, Health, and the Regen / Loadout / AimAssist / StyleMeter
  slots, the step's yaw) and `Players` (join order; `Nearest(from)`: the nearest living by squared 3D distance, a tie
  keeps the earlier one, nobody alive gives the first, so a solo run keeps its one target).
- Enemies: `Husks.Step(Players, dt)`: each husk / swarmer / spitter ticks against its nearest living player (the old
  one-target `Step(pos, health, dt)` stays for the tests and the bench). The spitter's volley leads its own target's
  velocity. `SpitterAcid.Step(Players, ...)`: a glob hits the first player it touches; the puddles age once and each
  player stands in the acid (and ticks) on their own.
- `HuskBrain.Step(dt, Players, round, camping)`: the brain keeps the feet / yaw / alive it saw at its step; every think,
  line-of-sight check and swarmer flank test reads the enemy's nearest living player; the ranged hit roll reads that
  player's speed. `NavField.Step(dt, at, alive, count)`: one goal per living player seeding one field (COOP-RESEARCH:
  "the flow field takes seeds"); `IsGoal(node)` for the brain's "route ends on a player".
- Owner: `Loadout(..., owner = "player")`; its hits, kills, parries, shots, swings, IMPACT bonuses, impact waves and
  arsenal events carry it (HitEvent, KillEvent, StyleBonusEvent, ImpactWaveEvent, ArsenalEvent, and owner fields on
  ShotEvent / MeleeEvent). The events that still carry a bare string (fire, weaponSwap, ...) become {owner, weapon}
  with the guns (N10).
- `SurvivalGame`: `players` with `p1` (the systems it already had); the brain and the enemies step on `players`.
- Still single (listed in Players.cs for N10 / U4-02): the Director (spawn picks, camp state, PEAK FADE), the wallet
  (COOP-RESEARCH: one each), the buyables, power-up grabs, zones, the mods' hooks, the token pools' player-count term.
- `PlayersTests` 5: the Nearest rules; two players 86.3 m apart on the Survival map (every zone open): each husk
  hunts the nearest and hits it, then P2 dies and husk A crosses to P1; the flow field seeds both and drops the dead;
  the glob and acid per player; a loadout's owner on its hits and the first wallet paying only "player".
- **Checks:** harness (1:16 AM) `golden total: 28 vector sets (30523 records), 22 replays (22 exact); 0 failing, 0
  mismatches`. The 2-player scenario through the harness (a scratch probe, same Core code; the EditMode run keeps no
  TestContext lines): `P1 (0.0, 0.0, 40.0), P2 (-20.0, 0.0, -44.0): 86.3 m apart`, `A from 4.5 m of P2 (83.0 m of P1):
  closest to P2 0.95 m; B from 20.4 m of P1: closest 2.16 m; both hit after 3.88 s (hurt 1 / 1)`, `P2 dead: A 85.6 m
  from P1 -> 2.99 m after 15.37 s; P2 hurt after death 0`. Unity 1:20 AM `tests: 5 / 5 passed` (PlayersTests), then
  1:23 AM `tests: 339 / 339 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 84 / 84`. ZeroAllocTests
  pass on the one-target path, which now runs the shared code; the Players overloads themselves are not measured.
- Slip: the HuskBrain patch script (`brain-players.cjs`) was written through a Bash heredoc instead of the Write tool
  (CLAUDE.md rule 9); the next one (`loadout-owner.cjs`) used the Write tool.

Status: the "if time" item done (commit c818416); N10 keeps the rest of the refactor.

## U3-12 the close (1:30 AM -)

- **Editor smoke** (Play check 5 of 5, 1:33-1:37 AM): `editor_focus`, Play, `SliceSmoke.Begin()` by eval (the first eval
  failed to compile: `The name 'SliceSmoke' does not exist`, it lives in `Skygrind.Game`; rerun fully qualified in the
  same Play session): `[smoke] 25.2s DONE: all steps passed` (round 1 cleared in 8 s with 9 kills, round 2, ARMORY,
  VIGOR, the recap, RESULTS, the personal-best line, R from RESULTS and from DOWN in 0 ms, round 6 "1 spitter(s) alive
  at once, 1 volley(s) in 5 s"); no exception line in the console. Play stopped; the TMP fallback asset was not
  modified.
- **The Windows Player** (1:38:54-1:39:43 AM, the night's one build): `build --target StandaloneWindows64 --outputPath
  Builds/Stage1/SKYGRIND.exe --scenes [Survival, Arena]`: `Succeeded`, 0 errors, 8 warnings, 150,867,799 B, 25.1 s.
  `Builds/Stage1/SKYGRIND_Data/StreamingAssets/PortExport` holds config.json, maps, 28 vector files and 22
  trajectories (PortDataBuildSync).
- **`git diff --stat` after it:** `Assets/Settings/Mobile_RPAsset.asset` rewritten again (the URP upgrade, D-09):
  restored with `git checkout`. `ProjectSettings/ProjectSettings.asset` showed `M` with no diff: its bytes hash to the
  index entry (`93c2448`), a stale stat; `git add` of the same bytes cleared it. `UnityConnectSettings.asset`:
  `m_Enabled: 0 -> 1` again; never staged (D-05). No WebGL batching entry changed.
- **INV-smoke** (1:41:03-1:41:43 AM): `SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke.log` (pid 146844, quit
  by itself): `[smoke] 25.1s DONE: all steps passed`, 0 lines with "exception" in the log. The steps as the Editor's,
  with 'FIRST RUN ON RECORD' (the Player's own PlayerPrefs) and R in 2-3 ms.
- **`-skygrindGolden`** (1:42:00-1:42:17 AM, pid 117536, quit by itself): every set `golden PASS <set>: N records, 0
  mismatches` (aim 292, arsenalMath 59, ..., jsMath 25248, ..., weaponMath 93) and every replay `replay PASS <name>: N
  steps identical` (16 + the 6 trial routes); `[golden] golden total: 28 vector sets (30523 records), 22 replays (22
  exact); 0 failing, 0 mismatches`; `[golden] DONE: 0 mismatches in 4.4 s (...StreamingAssets\PortExport;
  WindowsPlayer, AMD Ryzen AI 9 HX 370 w/ Radeon 890M )`. U3-04 now passing.
- **The bench** (1:42:49-1:43:58 AM): `bash Tools/survival-bench.sh 60`: `power seen: 15 Silent AC=True` (every 2 s
  poll: the Silent plan, on AC; `powercfg -getactivescheme` before it said Silent too); `1920x1080, NVIDIA GeForce RTX
  5070 Laptop GPU`; `bench result: 22783 frames, alive at end 31, median 2.49 ms, p95 3.84 ms, worst 1% 4.49 ms, mean
  fps 381, median fps 402, vsync 0, cap -1`; `bench gc: collections 10 in 22813 frames; GC Used Memory rises 75672.0 KB
  (valid)`; `bench timing: FrameTimingManager cpu median 2.49 ms, worst 1% 4.52 ms; gpu median 1.29 ms, worst 1% 1.42
  ms`. NIGHT8's (Balanced, on AC): median 3.35 ms, worst 1% 8.37 ms, 8 collections in 16919 frames, +65412 KB. The plan
  differs (Silent tonight), so the two are not a like-for-like comparison; the allocation rate per frame is about the
  same (about 3.3 KB a frame both nights).
- **INV-tests / INV-golden / INV-replays** (1:48:41-1:49:28 AM, after the last code commit `c818416`): `tests: 339 /
  339 passed, 0 failed, 0 skipped`, `replays: 22 / 22`, `golden tests: 84 / 84`. (tasks.json's invariant rows still
  say "16 / 16" and ">= 237"; their expectations were left as written; HANDOFF section 1 carries tonight's values.)
- **Docs** (1:50-1:58 AM): PORT-STATUS.md (the arsenal, momentum, aim assist, perks, style, FX, UI rows; a Players row;
  the event list, every new name found as `emit('<name>'` in the web source: primed, crossHit, swapKill, status,
  overload, impactWave, styleBonus, lunge, lockOn, lockOff); PORT-PLAN.md section 2 (the Oracle, arsenal / momentum,
  V lock, perks, style rows ticked; a "Co-op readiness (code)" row marked part); PORT-RULES.md unchanged since the
  U3-07 commit (no other rule was learned twice). HANDOFF.md rewritten with the `handoff` skill in its own structure
  (anchor `u3-n9`; the skill's default of a file in the OS temp folder was overridden by NIGHT9 section 7: the repo's
  HANDOFF.md, with a copy in temp). NIGHT9-REPORT.md for GG. NIGHT10.md written (untracked) from PORT-PLAN's N10 row,
  its facts block from tonight's runs and a read-only survey agent's measured line numbers. tasks.json: U3-04 and
  U3-12 passing with evidence; U3-13 split into U3-13.1..U3-13.7.
- **Memory:** `play-check-eval-recipe` (fully qualified eval names incl. Game types, reading an eval's whole result,
  the console's JSON) and `patch-scripts-not-node-e` (create the patch file with the Write tool) updated; both lessons
  were seen twice tonight. CLAUDE.md proposals in HANDOFF D-10 (e)-(g).

- **Commits and the tag** (2:00-2:03 AM): the close `ba75edd`, tagged `u3-n9`; then a check of tasks.json found
  three evidence strings (U3-07, U3-08, U3-11) naming their commit by subject, not hash: a docs follow-up `45b727f`
  put the hashes in and fixed HANDOFF's anchor line, and this last commit records it. The local tag moved to the last
  NIGHT9 commit each time (`git tag -f`; never pushed). `git status --short` after it: ` M
  ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT10.md`, `?? NIGHT7.md`, `?? NIGHT8.md`, `?? NIGHT9.md`.

Status U3-12: passing (the tag and the final git status are checked right after the last commit).
