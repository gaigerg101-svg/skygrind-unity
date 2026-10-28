# A4: lava AI, trigger spawns, the enemy whip, the piercing charge shot, boss pools (wave 33 audit)

Read-only audit of `port/stage3` at `4c79a5ce` (u3-n32). Line numbers are from that tree. "Measured" means a script read the shipped level JSON; everything else was read in code. Nothing was run in the Editor or the Player.

## (a) Enemy navigation and lava

**Who paths how.**
- `HuskBrain` drives husk, swarmer, spitter, charger and tank (`Core/HuskBrain.cs:1-20`). Its field choice is at `:979`.
- `Navigate` (`:933-1039`) has three modes:
  - **path**: a watchdog repath (`:948`)
  - **direct**: a straight line if `LineWalkable` passes (`:982`)
  - **field**: the flow field's next link (`:985`). Near the goal in the attack or approach phase it steers straight at the player (`:1010-1014`).
- The drone flies straight (`:895-901`).
- The charger's charge is a 22 m/s straight dash (`Enemies/Charger.cs` header). It ignores the nav.
- The field is a Dijkstra (`NavField.cs:289-345`), followed with `BestLink` (`:352-362`).

**How lava is represented.**
- A plan `pits` row with `lava: true` becomes the JSON's `lava: [{rect, y, room}]` (`Assets/Editor/Maps/LevelPlan29.cs:133-147`).
- Only `Lava30.Load` reads it (`Core/Lava30.cs:68-84`), apart from the view.
- **The nav grid knows nothing about lava.** The grid is baked into each level JSON by `NavGridBuilder` (`Core/Nav/NavGridBuilder.cs:68-89`) and loaded at `SurvivalSim.cs:174`. A grep finds 0 "lava" hits in `Core/Nav/`, `NavField.cs`, `HuskBrain.cs` and `Husks.cs`.

**Measured** (n32 levels and n30 span):
- Every lava column has a walkable node at lava height: kiln k2 42/42, k4 183, spire v6 310, span s8 240+240+144+144.
- The pits are 0.6-1.0 m deep. `LineWalkable` accepts a 0.7 m step down (`NavField.cs:866-879`). So foundry's f2 and f3 pits (y -0.6) pass the **direct** straight-line test.
- Deeper pits are a drop link in and a jump link out. Dijkstra takes them whenever they are shorter.
- That is GG's "runs into the lava to pass on me".

**Where an enemy dies.**
- `Lava30.Step` `:134-141`: feet inside a rect at or below `y + 0.05` start the burn (`lavaEnemyBurn`).
- `:145-164`: after 0.8 s it is killed (`lavaEnemyDeath`).
- It is called from `Game/SurvivalSim.Systems30.cs:101-105`, via `N30SystemsStep` (`SurvivalSim.Run.cs:370`). That is after `husks.Step` (`:348`) and before `brain.Separate` (`:387`).

**Minimal fix:**
1. **Grid forbid (Core, small).**
   - Add a `byte[] hazard` to `NavGrid` and OR it into `SetBlocked` (`NavField.cs:155-158`). Today `SetBlocked` overwrites `blocked` from the zone tags on every gate or zone event (`SurvivalSim.cs:175-177`, `SurvivalSim.Campaign24.cs:105`), so a mask written once would be erased.
   - Fill it in `Lava30StartRun` (`Systems30.cs:94-96`) after `lava30.Load`. A node is hazard when its x/z is in a zone and `y <= zone.y + ~0.3`. Then call `nav.SetBlocked(zones.LockedTag)` again so the fields are marked dirty (`:614-619`).
   - The y filter matters: kiln k2 has 84 nodes in its rect and only 42 at lava height. The rest are the lane above it.
   - Every query already skips blocked nodes, so the one mask covers them all: `NodeAt` (`:119`; and through it `LineWalkable`/`WalkableAt`), seeds `:298`, Dijkstra `:331`, `BestLink` `:362`, `GridPath` `:435`, `Relocate` `:1009`.
   - Use a block, not a cost: a cost would touch `Flow`'s float32 costs, which the `flowField` golden set pins.
2. **Edge guard (Game only).**
   - After `husks.Step`: a husk whose step carried its feet into a lava rect, with `knockSteps == 0` and not launched, goes back to its previous x/z and loses its horizontal velocity.
   - This catches the crowd push, the near-goal straight steer and the charger's dash. For the charger, set `hitWall` so it stuns.
   - Knocked enemies still fall in.
   - Exempt feet more than 1.5 m above `zone.y` (walkways over the lava).

Apply the same mask in `Game/MapAuthoring/LevelValidate24.cs:113-117`, or its reachability check passes rooms where walkers can no longer reach the player. Check k4 and s8 first.

## (b) Room encounters

**How a room spawns today** (`Core/Campaign24.cs`):
- `Enter` (`:543-574`): locks the door behind, seeds the room's Mulberry32 (`:540`), emits `roomEnter`.
- `StepFight` (`:754` on) spawns the next wave only when `alive == 0` and `waveGap` (0.8 s) has run out.
- `SpawnWave` (`:786-803`) puts each enemy on a random node of the room's pooled `nodes` and `highNodes` (`:248-249`), at least 12 m from the player.
- **No positional trigger exists.** Every wave can appear anywhere in the room.

**Telegraph.**
- None in the sim. `ICampaignWorld.Spawn` (`Game/SurvivalSim.Campaign24.cs:77-83`) calls `Husks.Spawn` (`Core/Husks.cs:276-297`), which places the body at once (`Husk.cs:141`) and emits `enemySpawn`.
- `Game/Chunks.cs:1478` queues a view effect at that spot (not read further).

**Hook:**
- **Plan format.**
  - Rooms are copied verbatim into the JSON's `campaign` block (`Assets/Editor/Maps/LevelPlan24.cs:224-231`), so no editor code is needed.
  - Proposal: `"triggers33": [{"wave": 1, "rect": [x0, z0, x1, z1], "nodes": [[x, y, z]], "early": true}]`, parallel to `waves`.
  - Don't change `waves`' element type: `:254` casts every entry to string.
- **Parse** beside `waves31` (`:262-266`).
- **StepFight:**
  - A triggered wave waits for the player's feet in its rect.
  - With `early: true` it may fire while earlier enemies still live (ULTRAKILL); otherwise the rect plus `alive == 0`.
  - `SpawnWave` uses the trigger's own `nodes`.
- **Telegraph.** Keep a pending list and emit `enemySpawnTell33 {kind, x, y, z}`. Call `world.Spawn` after `tellSeconds` (whole steps, a `design.n33` number) and draw a red-orange column in the view.
- **Retry and rank.** `Enter` resets per-room state. `Campaign24State` (`Core/N24Blocks.cs:89`) holds only room, checkpoint and limits, so a retry re-arms the triggers. The KILLS denominator (`:163`) is unchanged.

## (c) The grapple and enemies

**What a press on an enemy does now** (`Core/Arcline21.cs:112-153`):
- The enemy pick is `GrappleAim.ChooseEnemy`: a 15° cone halved, `range` 40, a clear line of sight (`Core/GrappleAim.cs:43-66`).
- `arcline.pullKinds` (husk, swarmer, drone) are **yanked to you** (`:122-135`) and start a 15 s cooldown.
  - During it, small kinds are skipped (`:66`) and the hook falls to a node or surface. `tapNodesOnly` then refuses a surface tap (`GrappleTap31.cs:145-149`), so tapping a husk does nothing.
- Bigger kinds are a **zip** (`:137-140`). The Mover reels at `zip.reelSpeed`, which `SurvivalSim.Grapple31.cs:51-52` sets to 44 m/s, and lets go at `zip.arrive` 2.6 m (`Mover.cs:606-608`).
  - `ReleaseRope` keeps the velocity x1.08 (`Mover.cs:497`).
  - The arrival clamp and pop run only for the latch's own zips (`OwnZip`, `GrappleTap31.cs:150`; gate at `SurvivalSim.Grapple31.cs:105-109`). An enemy zip skips them.
  - Result: **the player leaves at ~47 m/s through the body and deals no damage.**

**Stop-short and the hit (no Mover edit):**
- **Stop.** In `G31OnMover` (`SurvivalSim.Grapple31.cs:105-109`), add an enemy branch: a `"zip"` release while `Pulling && !OwnZip`. In `N31GrappleStep` (`:112-126`), clamp the horizontal speed to `whip.stopKeep` (~3-6 m/s) and add a small hop.
  - For a longer stop distance, write `cfg.movement.arcline.zip.arrive = enemy.Radius + gap` at fire and restore it at release. This is the same outside-config pattern as the reel speed. `G31Hang` reads it at `:96`, so restore it per pull.
- **Hit.** Call `loadout.Damage(enemy, whip.damage, false, id, eye)` (`Combat.cs:1390`), which gives kill credit, style and `kill`.
  - `Arcline21.zipEnemy` is private (`:49`) and needs an accessor, a Core edit.
  - Check `TuneDamage` and the arsenal tags with a new weapon id.
- **Whiplash toward all enemies.** Add `design.n33.whip.pullKinds` (e.g. `[]`), read in `Arcline21Rules.From` (`:38-57`) instead of the web list.
- **Risk: the kill refill.** `G32OnKill` (`SurvivalSim.Grapple32.cs:163-167`) refunds a charge on a non-boss kill. A whip that kills husks (hp 40) chains forever. Exclude whip kills, or keep the damage below 40.
- **Bosses.** Whether boss bodies are zip targets was not checked.

**Views:**
- **The rope** (`Game/Arcline21View.cs:42-46, 98-113`):
  - a **2-point straight LineRenderer** from the chest (1.2 m, below the camera) to the hook or anchor, 0.07 m wide (`UnityTuning.Arcline21.cs:9`)
  - the web's `ropeSegments` 14 and `sag` 0.35 are unused
  - no hook head, no flight wobble
- **`Game/Grapple31View.cs`**: the cyan mark, 5 pull beads (`:89-97, 167-196`; `UnityTuning.Grapple31.cs:24-28`) and the FOV kick.
- **View-only fixes:**
  - start the rope at the gauntlet or hand
  - a segmented sag and whip curve in flight, taut on attach
  - a hook-head mesh
  - a red-orange whip-target mark

## (d) The charge shot

**Numbers** (`design.n31.charge`, `Core/Guns/ChargeShots27.cs:36-50`, wired at `SurvivalSim.Weapons31.cs:40-44`):

| Setting | Value |
|---|---|
| Hold before a charge starts | 0.4 s |
| KESTREL charge time | 0.5 s |
| Full charge fires itself after | 0.5 s more |
| Damage | 4 x 25 = **100**, headshot x2 (`Combat.cs:445-448`) |
| Pierce | **3** |
| Cooldown | **4 s** |
| Falloff | none |

**The shot** (`FireRevolverCharged27Body`, `:308-357`):
- A wall stops it (`:315, :326`). The loop is capped at `pierce` (`:321`). It steps 0.9 m past each body (`:342`).
- The tracer ends at the **last body** (`:340`), so a pierce looks like a short normal tracer.
- The view is `WeaponFx`'s plain tracer (`WeaponFx.cs:21-24`). There is no beam.

**Knobs:**
- `revolver.pierce` 3 -> 24 (`weapons.rail.pierce`), or loop to the wall or range.
- `damageMult` 4 -> 8-10.
- `cooldownSeconds`.
- In code: `shot.end` = the wall or range end, plus a new event `chargeBeam33 {from, to, hits}`. Subscribe `WeaponFx`'s existing rail beam to it: 0.28 s, `#d6fbff`, additive (`WeaponFx.cs:44, 91, 105, 149`). Don't reuse the `railFire` name.
- Side effect on bosses: THE CROWN multiplies charged hits (`chargeCoreMult` 2, `chargePlateMult` 5, `chargeMin` 60: `Crown32.cs:58, 404`), and THE LANTERN's break is a charged shot. A stronger charge shortens those fights by itself, so re-sweep them.

## (e) Boss pools and levers

- Pools are multiplied by `hpMult` in `Boss32Pool` (`Game/SurvivalSim.Boss32.cs:156-161`). THE CROWN is `boss5.pool x ctx.scale.hp` (`Crown32.cs:259`).
- Player n32-r5 pilot times: 30.1 / 26.6 / 29.2 / 28.0 s, and THE CROWN 39.3 s (`NIGHT32-PROOF.md:9-13`).

| Boss | One number | Now | Pool |
|---|---|---|---|
| THE LANTERN | `design.n32.bosses32.lantern.hpMult` | 2.4 | 1231 (`retune_why`) |
| THE STAG | `...stag.hpMult` | 0.52 | 936 |
| THE KILN WYRM | `...wyrm.hpMult` (+ `gunScale` 0.8) | 0.51 | 3063; kill times come in ~6 s steps |
| THE LOOM | `...loom.hpMult` | 0.46 | `retune_why` still says 0.38 / 912; ~1104 if the base is 2400 (not checked) |
| THE CROWN | `design.n32.boss5.pool` | 10000 | x `ctx.scale.hp` |

**Sweeps** (from the repo root):
- `dotnet run --project Tools/SimHarness -c Release -- boss32 all --seeds 20` (`--cap`, `--pistolfist`).
  - It has **no `--set`**: it clears `sets30` without parsing it (`Harness.N32.Boss.cs:33`). Each probe needs a config edit, or a 1-line patch that copies `Harness.N30.Boss.cs:25`.
- `... camp32 all`: the campers must still lose after a cut.
- `... crown32 --seeds 20 --set pool=7000`: this one takes `--set`.
- `... hurt32`.
- Then the Player (L-029).

## (f) Risks

| Change | Core? | Golden / trajectories | Survival fingerprint | Other |
|---|---|---|---|---|
| Lava nav mask | yes (`NavField.cs`) | `flowField` and `astar` read their own grids, with no hazard: expected 0 mismatches. Run golden + 16 replays. | `survival.json` has no lava: expected EQUAL (not run) | Lava-level tapes and walks (`tape30`, `walk30`/`31`, `lavawalk30`) change. Re-baseline; `LevelValidate24` reachability. |
| Edge guard | no | none | gated on zones present: EQUAL | as above |
| Trigger spawns | yes (`Campaign24.cs`) | no golden set | Survival skips `Campaign24`: EQUAL | Campaign / Lesson25 EditMode tests, LevelSelect29, DoorFree29. Gate on the room key plus a `design.n33` row. |
| Whip stop and hit | Game + a Core accessor | no Arcline set; `Mover.cs` untouched | the script never grapples (not checked): EQUAL expected | `grapple31`/`32`, `pick31`, the refill loop |
| Piercing charge | yes (Core/Guns) | charged path not covered (not checked) | **Unknown**: if the script holds the KESTREL past 0.4 s, a charge fires. Run `fingerprint` and `--off32`/`--off33`. | `charge31`, `boss31w`, the Crown and Lantern sweeps |
| Boss cuts | config only | none | no boss within 60 s: EQUAL expected | `boss32`, `camp32`, `crown32`, the Player reel |

Every new number goes under `design.n33.*` with a `_why`; timers use `StepsFor`; the new event names go into PORT-STATUS.
