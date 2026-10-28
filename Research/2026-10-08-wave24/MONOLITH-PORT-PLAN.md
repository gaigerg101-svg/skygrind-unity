# MONOLITH port plan (NIGHT24 leftover 8, prep by builder B; for NIGHT25, layer 2's boss)

Source (read only): `skygrind/src/enemies/boss.js` (695 lines, `createBoss`). Numbers: `PortExport/config.json` `boss` (all of them below). Template in the port: `Core/Enemies/Bulwark.cs` (847 lines) and, for the newest conventions, `Core/Enemies/Wyrm.cs` (`StartFight`, `Step`, the `Filter` + `Apply` split, `WyrmBody`, the `EnemyHits.Filter` line). Nothing is built yet; every number below was read from the config, none measured in Unity.

## State
A 12 m construct of five stacked stone boxes (`boss.body`, each [w, h, d, x, y, z, yaw]) and two arms, a core sphere (radius 1.1, 7 m up, 1.3 m in front), 6000 HP (the kit scales it: `bossKit.hpPerRound`, `hpPerCycle`). It rises out of THE GRINDER over `introTime` 2 s (no damage), then phases `p1 -> mid (66 %) -> p2 -> p3 (33 %) -> dying -> dead`. Its body is a box layer (`rays: false`: solid to the player, shots go to the core and body shapes); the layer is one box that grows with `rise`.

## Phases and attacks (web numbers)
- **Slam** (p1, p2; p3 as a triple): a circle of radius 9 fills for `slamTell` 1 s (0.55 s in the combo), then 2 shock rings (`ringGap` 0.8 s apart) run out at 11 m/s (15 fast), band 0.9, height 0.7, 25 damage, to 34 m; a jump clears them. Every 3rd slam it kneels 3 s (core drops 3.5 m, x3, melee on it x1.5 with momentum).
- **Orb volley** (p1-p3): tell 0.7 s, 8 orbs in a 1.4 rad fan from the core at 7 m/s. A punch-parried orb into the core deals 200 (`parryDamage`).
- **Surge** (p2): 3 circles (radius 2) led by the player's velocity (0.45 s apart), tell 0.8 s, then a pillar erupts: 30 damage and a 13 m/s launch.
- **Sweep** (p2): a painted line, tell 1.2 s, then a charge at 18 m/s over up to 34 m (overshoot 12): 30 damage, push 14 (+7 up).
- **Laser** (p3): tell 1 s, a horizontal beam at 1.2 m sweeping 6.5 rad over 3.2 s, range 26, 40 damage, 0.3 s hit cooldown; a 4 s kneel window after the p3 combo.
- **Midpoint** (2.5 s): roar, a 16 m/s knockback pulse, the Grinder reshapes to its ring (`roundReshape` 0.6), 2 Husk squads (3 each). **p2/p3 adds** every 20 s (swarmer x5, spitter x2; max 10). **p3**: the Grinder floor sinks except platforms (`sinkTime` 1, `platformCells` 3, `fallDamage` 20, `abyssHalf` 24).
- Reward (`bossKit.rewards.monolith`): 2000 points, a token, a power-up. Events: bossRise, bossPhase, bossTell, bossHit, bossRoar, bossSlam, bossVolley, bossSurge, bossKneel, bossSink, bossBeat, bossDying, bossDown (the web's names; the port keeps them).

## Through BossKit
`kit.bosses["monolith"] = monolith; kit.Start("monolith", round, cycle, mutator)` calls `StartFight(ctx)` (the web's `startFight` -> `start(center, round, 1, ctx.scale)`), the kit shows the intro card (`bossKit.names/subtitles` already hold MONOLITH / THE GRINDER WAKES), `kit.Beat` at 66 % and 33 % (the pulse is the kit's: `beatPush` 12 / `beatPushUp` 5; the web's own `pulsePush` 16 is used only without a kit), `kit.Dying` and `kit.Down` for the finisher and reward. Hooks the port still lacks for it: **(1)** `deps.onMidpoint` (reshape the Grinder: `CoreGrid.Reshape` exists), **(2)** `deps.onAdds` (`director.SpawnGroupAt(kinds, p)`, as Bulwark's hatches), **(3)** the Grinder's floor sink and `surfaceY` (`CoreGrid.topAt`), **(4)** orbs: `DroneOrbs` exist in `Core/Enemies/Drone.cs` (a boss-owned fire with the parry route `SpitterAcid.TryReflect`), **(5)** a punch target while kneeling (`Combat.cs` excludes `BulwarkBody` from punch; MONOLITH needs the opposite for the kneeling core), **(6)** `SurvivalSim.Boss20.cs` registers one boss: the schedule round 10 slot needs a second registration beside the Bulwark, and `design.adds.monolith` (new) as its switch.

## Phases of work
1. **Core sim** (`Core/Enemies/Monolith.cs`, `MonolithBody : Enemy`): params, the state machine, slam / rings / volley / surge / sweep / laser, the damage rule (`Filter` + `Apply`: core x1.6, kneeling x3, body x0.5, parry 200, blocked in intro / mid / dying), the layer. Seeded action stream (the web draws `Math.random`: use `Mulberry32`, `runSeed ^ salt`, as Bulwark does).
2. **Tests + harness** (`monolith` command: a bot kills it, a tape replays EQUAL, tells >= 0.5 s, fingerprint EQUAL with the row off): copy `Harness.N24.Wyrm.cs` and `BulwarkTests.cs`.
3. **Sim wiring** (round 10, mid reshape, adds, sink): `SurvivalSim.Boss20.cs` extension.
4. **View** (`EnemyViews.Monolith.cs`): the web's look as it is (stone 0x6b625c with the rim, core 0xff7a20); shock rings and the laser through `FxDraw.Additive`; telegraphs through Telegraph20.
5. **Reel** scenario (intro, slam ring, kneel, laser, finisher) and the Player check.

## Risks
- **The floor sink (phase 3)** touches the Grinder's `CoreGrid` heights and the player's fall damage: the largest unknown; plan a standalone flat arena first (`bossKit.arenas.monolith: flat`).
- **Orb parry** needs the punch / reflect path that currently serves the SPITTER; a boss-owned orb pool may need a small `DroneOrbs` change (Core, golden-gated).
- **`Math.random` in `nextAction`**: the port's stream changes which attack comes when; the web numbers still hold, the sequence does not (log it, as Bulwark's port notes do).
- **Balance**: 6000 HP and x0.5 body were tuned on the web with the snap and momentum; measure with a `--human`-style lag bot before trusting them (the WYRM's first guess was 2x off).
- The kit's `hpPerRound` scales HP by round: the pool is tuned at the first appearance only.

## Estimate (builder-hours, one builder, headless gates only)
Core sim 5-6 h, tests and harness 2-3 h, sim wiring 2 h, view 3 h, reel + Player check + dn-check fixes 2 h, floor sink and adds 3-4 h: **about 18-20 h**, or 9-10 h for a first slice without the sink and the adds (phases p1 and the midpoint, a flat arena).
