# FLOW AUDIT, levels 1-5: where the player stops or loses flow (NIGHT46 forge, read-only, 2026-10-17)

Question: "where does the player STOP or lose flow?" Every fight room of levels 1-5 (n45-foundry, n45-iceline, n45-kiln, n40-span,
n41-spire) and the links between them, from data, plus what the campaign code does that halts the player.

## How this was measured (read this before trusting a number)
- **Geometry**: a scratch script (scratchpad `flow-audit.cjs`, not in the repo) rasterised each room's rect at 0.25 m from the built
  level JSON's `collision.boxes` + `collision.wedges` + `lava` (Assets/StreamingAssets/Design/levels/<id>.json). A cell is walkable on a box
  top with 1.8 m headroom (capsuleHeight); lava cells and anything under -6 m (the voids) are not; the capsule margin is a one-cell erosion.
  - **Tiers**: distinct walkable box-top heights (0.5 m bins, >= 4 m2). "(unreached)" = not reachable from the room's spawn with a
    2.8 m double-jump climb.
  - **Loop**: a hole in the walk graph (adjacent cells joined when the height step is <= 0.55 m; also checked at 1.5 m, a hop: the same
    answer in every room). A hole means you can run a closed circuit round it. "Central loop" = round the arena's main feature; "edge /
    local" = round a ride panel, a pillar or an aisle wall only.
  - **Pocket**: >= 2 m2 of cells where 3 of the 4 axis rays hit a wall, lava or a climb within 3 m.
  - **Run / sight**: the longest straight run on the walk graph (8 directions), and the longest clear eye-height (1.6 m) sightline.
  - **Spawn view**: each wave node's bearing from the room's spawn point (facing -z, every room), "out of view" past 53 deg (74 deg vertical
    FOV at 16:9: PortExport/config.json camera.fovDeg). "No LOS" = the segment from the eye (1.6 m) to the body's centre (1.0 m) hits a box.
  - **Doors**: rays from each door's far face at 0.6 m and 1.4 m above the floor, at 0 / +-15 / +-30 deg, to the first box; and the first
    lava / void / drop on the centreline within 12 m.
- **Movement lines**: `ride:true` boxes in `campaign.boxes` (the designed ride walls with the cyan strip), `grappleNodes`, `pads`. Note:
  every wall is ridable except the `noride` boxes (Core/Mover.cs:1601, Core/Ride38.cs:105-114), so the perimeter walls are always
  rideable lines. Levels 1-5 have **no pads and no grind rails** (`pads` [] and `data.strips` [] in all five files).
- **Code halts**: read by a sub-agent from Assets/Scripts and Assets/StreamingAssets/Design/unity-config.json (every `design.*` row lives
  there). I spot-checked Campaign24.cs:965-971 and :672-676 and every number in the code table against unity-config.json.
- **Harness** (run tonight, read-only, ~3 min): `dotnet run --project Tools/SimHarness -c Release -- n43rooms clear --n43 --seed 1` and
  `... n43rooms idle --n43 --wait 1 --seed 1`. These run the **n43** level files for levels 1-3 (the harness has no n45 switch). n45 changes
  only b1, g7 and the k5 boss arena, so b1 / g7 numbers from them are n43's rooms. They do not cover levels 4-5. Outputs are quoted below.
- "Dead time" here means seconds when nothing can be fought or done: wave gates, gaps, reveals, rest rooms, retries. Platforming links
  are counted apart as "non-fight traversal" (their `par`).

## The ranked table: the 12 worst flow breaks in levels 1-5

| # | room | the break | the number | where (data / code) | fix with data alone | fix that needs code |
|---|---|---|---|---|---|---|
| 1 | every fight room; worst in b1 THE SPILL | **A death restarts the whole room from wave 1.** The fight already won is fought again, after the retry pause and the start grace. | 1.2 s retry, then up to 4 s or 6 m of start grace, then every wave again. b1's bot (10 seeds): 10 deaths and median 63.7 s, max 263 s; n43's b1 had 0 deaths and 51.2 s. | Campaign24.cs:693-705; unity-config `design.n24.campaign.retryDelay` 1.2, `design.n40.start40.moveMetres` 6 / `holdSeconds` 4; Research/2026-10-17-wave45/A1.md:39-41 | b1: soften wave 2 (2 spitters + 2 husks on the rim, `campaign.rooms[b1].waves[1]`), or move the rim health pickup to the near bank | Restart at the wave you died in (a per-wave checkpoint), not the whole room |
| 2 | k5 THE RING (level 3 boss) | **The only boss that teleports.** The first step past door d8 puts the player in the worm's own arena, so the speed you carried through the door is lost. The arena is not the room you saw through the door. GG (w35): "you just teleport into these random rooms". | Velocity goes to 0 (A-8). | Campaign24.cs:672-676 (`world.Respawn` when `!world.BossWalkIn`); Mover.cs:283; docs/ROOMS.md:158-159 | None: the k5 rect is 26x26 m, the wyrm45 arena is 60x60 m (half 30) | Walk-in like the other four bosses (build wyrm45 at k5's position), or keep velocity through `Respawn` |
| 3 | level 4 links p2, p4, o1, o2 | **A fall into the void sends you back to the link's start and costs 25 % of max health.** Five of the seven doors open onto the void 6-8 m ahead, so you stop at the door to aim the pull. | Respawn below y -3 (p2, p4) or -5 (o1, o2); 25 % of max HP; speed 0. p4 is 92 m long with two rising gates. Doors d1, d3, d5, d6, d7 have void at 8.1 / 8.1 / 6.1 / 6.1 / 8.1 m. | n40-span.json `campaign.rooms[p2,p4,o1,o2].respawnY`; unity-config `design.n29.roomRules.fallCostFraction` 0.25 | Per-gap lip returns, as `fallReturn43.by` does for b2 / g7 (lip + 10 HP). Data only if Campaign24's reader (:999-1029) also serves rooms with `respawnY`: not checked. Also start each void 10-12 m past the door | Make `respawnY` return to the last ledge, as o3's loom37 catch already does |
| 4 | a2 KESTREL, a8 PIERCE | **Teaching holds where nothing can be fought.** a2: wave 0 waits until the three stand-ins die. a8: the husks wait until you stand on the perch, and the door waits a further 8 s if a lesson is skipped. | Up to 20 s each. Harness doorway pilot (doesn't shoot or ride): first wave at 20.19 s in both rooms. Door opens at 32.0 s (a2) and 28.9 s (a8). | unity-config `design.n43.rooms43.standInHoldS` 20, `afterLessonMaxS` 20, `lessonCapS` 8; n45-foundry.json `rooms[a8].afterLesson43` 0, lesson 0 rect [-1,-357.5,4,-353]; Campaign24.cs:851, :885-896 | Caps 20 -> ~8 s; move a8's perch rect onto the first ride wall's end, which is 4.3 m from the spawn | None needed |
| 5 | every multi-wave room (29 gaps in levels 1-5) | **The arena empties between waves.** The next wave needs every body dead, then a gap, then a queue. New bodies appear at least 12 m away, so you must close the distance again. | About 1.0 s from the last kill to the first new body (`waveGap` 0.8 + 0.2 queue), +0.1 s per extra body. Summed over levels 1-5, about 30-40 s per run. Harness instant-kill doors: 1.5-5.8 s per room (g5 5.8, g7 5.8, b1 5.6). | Campaign24.cs:857-865, :910-928; SurvivalSim.Level36.cs:71-78; unity-config `design.n24.campaign.waveGap` 0.8, `minSpawnDist` 12, `design.n36.level36.nextWaveS` 1, `staggerS` 0.1 | Lower `waveGap` / `nextWaveS`; put later waves' `waveNodes43` where the player will be heading (12-16 m ahead) | Release wave N+1 when 1-2 bodies of wave N remain (overlap the waves) |
| 6 | p1, p3, p5 (level 4); v2, v6 (level 5); kb2c (level 3) | **Big open floors with no movement line and no circuit.** Zero designed ride walls. The only loops go round a 1.2 m block or pillars. Grapple nodes are 14-25 m from the entry. You walk at 8 m/s across 840-1600 m2. | Floors 1152 / 1600 / 1296 / 1131 / 1412 / 840 m2. Largest hole 6.9 / 21.4 / 22.6 / 11.1 / 12.6 / 0 m2. Sightlines 46.8-55.2 m. Nearest grapple 25.1 / 14.1 / 17.7 / 20 / 21.6 m. | n40-span.json, n41-spire.json, n45-kiln.json `campaign.boxes` (no `ride` in these zones); Research/2026-10-17-wave45/AUDIT.md rows p1-p5, v2, v6 | Build from the identity cards: p3's TIE-OFF card already exists (docs/ROOMS.md:201-222). Add a ride wall with a strip on the entry -> first-wave line, and one central obstacle to circle | None |
| 7 | links kb1b, kb2b, kb3b (level 3) | **Doors open onto a 12 m cliff.** The run ends at a wall and becomes a stand-and-boost climb (shoot down). kb2b alone is 24 m of climb between kb2 and kb2c. | The cliff face is 11.6 m (d1, d3) and 13.1 m (d6) past the door. 42 s of `par` of climbing between level 3's fights (kb1 25, kb1b 4, kb2b 8, kb3b 5). | n45-kiln.json collision ledges [-8,-1,-65]-[8,12,-57], [-8,-1,-147]-[8,12,-135], [-8,-4.9,-268.5]-[8,12,-260.5]; DESIGN-RULES A-8 | A side ride wall or a ramp run-up into each cliff, so the climb starts from a ride (kb3b's lesson already says "CLIMB IT ON THE RUN"). Cut kb2b to one 12 m step | None |
| 8 | the five bosses | **Reveal and phase windows where the boss can't be hurt and doesn't attack.** The player keeps control. | Reveal: a9 2 s (+ a 2.5 s camera ease the first time), x9 1.5 s, o3 2 s, v11 2 s, k5 the emerge (not checked). Phase / leg changes: 1.5 / 1.5 / 1 / 0.8 / 1.2 s. Death: 2.5 / 1.2 / 2.5 / 1.5 / 1.2 s. Boss kill slows the world to 30 % for 0.6 s. The WICK's `legLock` wastes damage below 0.6 / 0.3 of its bar until you climb. The wyrm's phase beat also pushes the player back (12). | Boss37.cs:268; unity-config `design.n37.wick37.boss.revealSeconds`, `design.n40.brawl40.revealSeconds` / `phaseSeconds`, `design.n37.loom37.revealS`; `bossKit.beatTime` 1.2, `beatPush` 12, `killSlowMo`; Brawl40.cs:429, Crown37.cs:62 (both hard-coded) | Shorten the reveal / phase seconds in config, about 1 s each | The two hard-coded die times; the WICK's legLock (it's a rule: GG's call) |
| 9 | L1 b1 -> a8; L5 v2 -> v6 and v6 -> v11 | **Long stretches with no fight.** L1's b2 is 104 m over lava with lip teleports (speed 0, 0.5 s fade, -10 HP), then REST, then the chimney, whose foot is a dead end. L5's belfries are four identical pistol-only gates, each with a 17-18 m pit 10.1 m past the door. | L1 b1 -> a8: 162 m, `par` 42 s; a7's foot has 3 pockets of 8-9 m2 at z -329. L5 v2 -> v6: 104 m, `par` 20 s. L5 v6 -> v11: 124 m + doors, `par` 34 s. g7 bot (n43 THE MIX): 23 lip returns in 10 runs. | n45-foundry.json rooms b2 / a6 / a7 (`fallReturn43`); n41-spire.json rooms v3-v5, v7 (`limit` ["pistolOnly"], pits floorY -5); UnityTuning.Ui43.cs:61-63 (fade); A2.md:23 | Merge v4 / v5 into one gate; give b2 or v9 a small fight (a few husks on the gantry lip); cut a6 REST (see the per-level notes) | None |
| 10 | a3 RIDE, g4 THE REACH (also a4, g6, g8) | **Linear rooms: the hazard runs wall to wall, so there is one crossing and no circuit.** Enemies wait on the far side. | a3: lava x -8..6 of an -8..8 room leaves a 2 m strip; 0 loops; 2 husks 22.9 m away. g4: a 24 m canal wall to wall; 0 loops; 0 ride walls; drones 22-25 m across. a4 is 8 m wide; g6 and g8 have no ride wall. | n45-foundry.json `lava[a3]`; n45-iceline.json `pits[g4]`, `surfaces[g4]` | a3: stop the lava 2 m short of the west wall so there is a walk round both sides; g4: a second bridge or a side ledge round the canal | None |
| 11 | kb2c THE ASH BED; every level-2 room | **Doors that stay shut (harness).** If the burrower is moved under or out of the room, kb2c's door never opens and the rooms behind it stay shut. Bodies that end without a kill (pit, out of room) delay level-2 doors 15-35 s. Lava kills cost 2-5 s more than gun kills. | `n43rooms clear` pit / stuck / lava paths: kb2c HELD ("wave 1/2, own alive 5"), RED 6 of 68; kill path 3.3 s. Level 2 pit path vs kill path: g1 17.1 vs 1.5 s, g6 36.5 vs 1.8 s. Lava path: b1 10.2 vs 5.6 s, a5 7.4 vs 5.0 s. | n43rooms clear output (below); Campaign24.cs:907 (lessons hold the clear); unity-config `design.n43.rooms43.endS` 2, `maxTopUps` 2, `lessonCapS` 8 | None | Count a removed body as dead for `husksDead` lessons; end a burrower that leaves its room. Whether either happens in real play: not checked |
| 12 | b1, v6, kb2, g8 | **Wave 1 out of sight from the door (A-2: spawn at nodes the player can see).** You commit into an empty-looking room. | b1 wave 1: all 6 far-bank nodes have no line of sight (hidden behind the 7 m crane rails), 30.9-35.9 m away. v6: all 5 ground nodes have no line of sight (behind the 1.5 m dais). kb2: 5 of 10 ground nodes have no line of sight, 2 high nodes out of view (53 deg). g8: the charger's 2 nodes have no line of sight (pillars). | `campaign.rooms[b1].waveNodes43[0]`, `rooms[v6].nodes`, `rooms[kb2].nodes / highNodes`, `rooms[g8].waveNodes43[0]` | Move wave 1's nodes into the entry's view cone: b1 to the bridges' far ends; v6 onto the dais | None |

## Per-level summary (code-side dead time for an active player with no deaths; estimate = the config values summed)
Per room: the 3 m commit, about 0.4 s at 8 m/s, then a 0.2 s queue. The first fight room of a level also has start40's 6 m. Each wave gap
is about 1.0-1.4 s. Each arena room's last kill runs 0.15 s at 25 %. Bosses as in row 8. "Not counted": the a2 / a8 teach holds (up
to 20 s each when the verb isn't done), debut cards (35 % for 1.5 s, once per new enemy kind per save: UnityTuning.Debut20.cs:8-12), and
deaths (row 1).

| level | dead time (est.) | of which | non-fight traversal (`par` of links) | loops per fight room (y = central circuit; edge = only round panels / pillars / aisle walls; n = none) |
|---|---|---|---|---|
| 1 n45-foundry | **~22 s** | starts ~4 s, 5 gaps ~6 s, slow 0.9 s, REST a6 3 s, a9 WICK ~8.6 s | 54 s (a1 12, b2 25, a6 3, a7 14) | a2 n, a3 n, a4 n, a5 **y** (lava), b1 **y** (river via two bridges), a8 n |
| 2 n45-iceline | **~28 s** | starts ~5 s, 10 gaps ~13 s, slow ~1 s, REST gr 4 s, x9 BRAWL ~5 s+ (phase count not checked) | 4 s (gr) | g1 edge, g2 edge, g3 edge, g4 n, g5 edge, g6 n, g7 **y** (grate via two bridges), g8 edge (4 pillars) |
| 3 n45-kiln | **~16 s + the wyrm's emerge** (not checked) | starts ~2 s, 5 gaps ~6.5 s, slow 0.5 s, REST kb4 3 s, k5 ~4 s + teleport (speed 0) | 42 s (kb1 25, kb1b 4, kb2b 8, kb3b 5: climbs) | kb2 **y** (both pours + crucible), kb2c n, kb3 **y** (the pour) |
| 4 n40-span | **~17 s** | starts ~2 s, 5 gaps ~7 s, slow 0.5 s, no rest room, o3 LOOM ~7 s | 48 s (p2 3, p4 5, o1 20, o2 20); each void fall = back to the link's start, -25 % HP | p1 n, p3 n, p5 n (local holes <= 22.6 m2 only) |
| 5 n41-spire | **~16 s** | starts ~1.6 s, 4 gaps ~5.6 s, slow 0.3 s, VESTRY v8 4 s, v11 CROWN ~4 s+ (phase count not checked) | 62 s (v1 12, v3 8, v4 6, v5 6, v7 8, v9 12, v10 10) | v2 edge (aisle walls, pillars), v6 edge (aisle walls, pillars) |

Fight-room `par` sums for scale: L1 170 s, L2 215 s, L3 155 s, L4 91 s, L5 105 s (campaign.rooms[].par). Code dead time is therefore about 10-15 %
of the fight par. The larger losses are deaths (row 1), falls (row 3), the teach holds (row 4) and the non-fight traversal.

## Per-room data (fight rooms; links in the next section)
"ride / grapple" = designed ride walls (`ride:true`) and the nearest one's distance from the spawn; side walls are always rideable (half
the room's width from the spawn). Wave nodes: distance @ bearing from the spawn. All rooms 0 pads, 0 rails.

### Level 1 (n45-foundry)
| room | size (m) | floor m2 (lava) | tiers | loop | pockets | run / sight (m) | ride / grapple | waves (bodies) | wave nodes from the entry |
|---|---|---|---|---|---|---|---|---|---|
| a2 KESTREL | 14x20x7 | 280 | 0, 2.0 deck | n | 0 | 19.8 / 23.1 | 0 / 0 (side 7 m) | 2 + 3 stand-ins | high deck 16.8 m @ -10..-17 deg |
| a3 RIDE | 16x26x9 | 297 (119) | 0 (6.0 top unreached) | n (lava to the west wall) | 0 | 25.8 / 29.2 | 2 @ 4.4 m | 2 | 22.9 m @ +-10 deg |
| a4 PARRY | 8x19x6 | 152 | 0, 1.5 | n (1.5 m2 only) | 0 | 15.8 / 16.6 | 0 (side 4 m) | 1 / 2 spitters | deck 12.5-12.9 m @ 0..+-13.5 deg |
| a5 CRUCIBLE | 20x24x10 | 359 (122) | 0, 1.0 | **y** 147 m2 round the lava | 0 | 20.8 / 24.4 | 2 @ 5 m | 2 / 3 / 3 | ground 8.1 m @ +-83 deg (out of view; skipped while < 12 m), 12 m @ +-42 deg no LOS; high 17.5-18 m in view |
| b1 THE SPILL | 36x40x12 | 1206 (234) | 0, 2.5 rim (rails 7.0, bowl 6.5 unreached) | **y** 175 m2: near bank -> bridge -> far bank -> bridge | 0 | 39.8 / 40.5 | 2 rails @ 12.6 m | 3 / 4 / 4 | w1 30.9-35.9 m @ +-19..29 deg, all no LOS; w2 rim 16.5-22.6 m in view; w3 near bank 16.3-17.5 m, 4 of 6 @ 52-79 deg |
| a8 PIERCE | 14x22x9 | 308 (a dry pit of 172 m2 at -4 m) | 0 (pit -4, perch 3.0) | n | 0 | 13.8 / 13.2 | 2 @ 4.3 m | 3 (after the perch) | 17.8-20.1 m @ 7.5-9.7 deg |
| a9 THE STACK (boss) | 28x28x50 | geometry built by the boss code | not checked | not checked | | | | boss | |

### Level 2 (n45-iceline)
| room | size | floor m2 (lava) | tiers | loop | pockets | run / sight | ride / grapple | waves | wave nodes from the entry |
|---|---|---|---|---|---|---|---|---|---|
| g1 TANK ALONE | 22x26x9 | 572 | 0 | edge: behind the 2 ride panels (15 m2 each) | 0 | 30.8 / 32.6 | 2 @ 8.5 m | 1 tank | 12 m @ 0 deg |
| g2 THE SHIELD | 24x28x9 | 672 | 0, 1.5 | edge: 2 panels + 2 pillars | 0 | 28.3 / 34.2 | 2 @ 9.8 m | 1 / 3 | 14-18 m @ 0 deg; high 24.8 m no LOS |
| g3 THE SWARM | 26x28x9 | 728 | 0 | edge: 2 panels | 0 | 29.7 / 34.8 | 2 @ 11.2 m | 3 / 8 | 13.3-24.3 m @ 0..+-16 deg (2 no LOS) |
| g4 THE REACH | 24x30x10 | 720 | -1 canal, 0, 1.5 | n (the canal wall to wall) | 0 | 29.8 / 37.0 | 0 / 0 | 1 / 2 drones | 22-25.2 m across the canal |
| g5 THE COOLDOWN | 26x30x9 | 780 | 0, 1.5 | edge: 2 panels | 0 | 32.9 / 37.7 | 2 @ 9.2 m | 5 / 8 / 3 | 12-15 m ahead; high 25-28 m @ +-22..25 deg |
| g6 THE ARMOURY | 14x18x6 | 252 | 0 | n (plinth 2.6 m2) | 0 | 19.4 / 21.4 | 0 / 0 | 2 / 1 (after the 2 kills) | 13.3-14.7 m @ +-13..18 deg |
| g7 THE THAW LOCK | 30x32x10 | 792 (159) | 0, 2.0 gantries | **y** 119 m2 round the grate | 0 | 29.8 / 40.5 | 0 / 0 | 3 / 7 / 6 | w1-2 21.4-29.5 m in view (2 no LOS); w3 11.2-14.8 m @ 62-75 deg (door side, by design) |
| g8 THE REHEARSAL | 24x30x9 | 720 | 0 | edge: 4 pillars (9.8 m2 each) | 0 | 33.6 / 35.2 | 0 / 0 | 1 / 3 / 2 | w1 charger 17.2 m @ +-35.5 deg, no LOS; then 20-23.8 m |
| x9 THE BRAWL (boss) | 38x38x14 | 1444 flat in JSON (the floes are Brawl40's code) | not checked | | | | | boss | |

### Level 3 (n45-kiln)
| room | size | floor m2 (lava) | tiers | loop | pockets | run / sight | ride / grapple | waves | wave nodes from the entry |
|---|---|---|---|---|---|---|---|---|---|
| kb2 POUR HALL | 36x44x16 | 1408 (176) | 0, 3, 6 crucible | **y** round both pours (105 m2 each) + crucible | 1 (3.4 m2 @ 15.6, -107.1) | 43.8 / 47.0 | 1 @ 19.2 m | 6 / 5 / 3 | ground 13-38 m (5 of 10 no LOS); high 20 m @ +-53 deg, 30.5 m |
| kb2c THE ASH BED | 28x30x12 | 840 | 0, 0.5 plates | n | 0 | 39.2 / 39.6 | 0 / 0 | 1 / 4 | 14.1-25.3 m @ 0..+-42 deg |
| kb3 CASTING FLOOR | 40x46x18 | 1694 (146) | 0, 4, 8 ladle | **y** 177 m2 round the pour | 0 | 45.8 / 53.5 | 2 @ 16.3 m | 5 / 3 / 4 | 13.1-30.7 m @ 0..+-33 deg |
| k5 THE RING (boss) | 26x26 door room; the fight is in Design/maps/wyrm45-arena.json (it declares `loop43`) | not checked | | | | | | boss | the teleport (row 2) |

### Level 4 (n40-span)
| room | size | floor m2 | tiers | loop | pockets | run / sight | ride / grapple | waves | wave nodes from the entry |
|---|---|---|---|---|---|---|---|---|---|
| p1 HOOK YARD | 32x36x12 | 1152 | 0, 4 terrace (160 m2) | n (2 holes of 6.9 m2) | 0 | 44.9 / 46.8 | 0 / 1 @ 25.1 m | 3 / 6 | 14-32.3 m @ -23..+27 deg (3 no LOS) |
| p3 KNOT YARD | 40x40x14 | 1600 | 0, 5 balconies (256 m2) | n (21.4 m2 round the 1.2 m block) | 0 | 49.9 / 55.2 | 0 / 4 @ 14.1 m | 4 / 5 / 6 | 13-32.6 m @ 0..+-36 deg (4 no LOS) |
| p5 SPOOL HALL | 36x36x14 | 1296 | 0, 4 gallery (144 m2) | n (22.6 m2) | 0 | 50.6 / 49.5 | 0 / 2 @ 17.7 m | 6 / 5 / 6 | ground 12.8-36.9 m all @ -23..-45 deg (left); high 24.5-25 m |
| o3 THE CANYON (boss) | 44x140x30 | 1212 of 6160 (the rest is the canyon) | 0, 8 (unreached) | not checked (threads) | | | 6 @ 29.4 m / 13 @ 19.8 m | boss | |

### Level 5 (n41-spire)
| room | size | floor m2 | tiers | loop | pockets | run / sight | ride / grapple | waves | wave nodes from the entry |
|---|---|---|---|---|---|---|---|---|---|
| v2 THE NAVE | 30x38x14 | 1131 | 0, 2.5 aisles (312 m2) | edge: 2 aisle walls (11 m2), 4 pillars | 0 | 37.8 / 43.0 | 0 / 3 @ 20 m | 5 / 4 / 4 | 12-28.3 m @ 0..+-14 deg; high 18.4 m @ +-41 deg, 26.8 m no LOS |
| v6 THE CHOIR | 34x42x16 | 1412 | 0, 1.5 dais, 2.5 aisles | edge: 8 (aisle walls, pillars) | 0 | 41.8 / 50.2 | 0 / 3 @ 21.6 m | 6 / 4 / 6 | ground 19.7-35 m, **all 5 no LOS**; high 18-29.5 m |
| v11 THE SPIRE (boss) | 68x68 over a 74 m chasm | platforms built by the boss code | not checked | | | | 4 | boss | |

## Links and doors (A-8: does a door carry your speed?)
- **Code**: no door delays you. The exit door opens on the same sim step as the last kill (Campaign24.cs:1077-1078). Zones.Open drops the
  collider at once (Zones.cs:185-194). The 0.28 s curtain shimmer is view only. `checkpoint:true` only emits an event (Campaign24.cs:658).
  Nothing in roomEnter, locks, opening or checkpoints touches velocity. **Speed is zeroed only by teleports**: death, `fallReturn43`,
  `respawnY` and entering k5.
- **Geometry**: every door is 4x4 m, 4.5 m tall, with its floor flush with both rooms. The first solid straight ahead is never closer than
  11.6 m. The closest wall at +-30 deg is 3.5 m: a6 -> a7, the chimney's two walls (intended).

| door | first solid straight ahead / at +-30 deg | first hazard on the centreline (12 m) | note |
|---|---|---|---|
| L1 d3 a3 -> a4 | 13.1 m (1.5 m deck) / 8.1 m side walls | none | a4 is 8 m wide (GG G10's "brick wall" was the a8 -> a9 boss door: its frame is code, not checked) |
| L1 d4 a4 -> a5 | 18 m / 8.1 m (ride walls) | **lava at 5.1 m** | land, then ride or turn at once |
| L1 d6 b1 -> b2 | >25 m / 6.1 m (ride wall) | lava at 8.1 m | the gantry's gate 1 |
| L1 d8 a6 -> a7 | >25 m / **3.5 m** | none | the chimney (a climb, by design) |
| L1 d9 a7 -> a8 | >25 m / 8.1 m | **drop to -4 at 3.1 m** | a8's dry pit |
| L2 d6 g6 -> g7 | 13.6 m (THE BOILER) / >25 m | lava at 11.6 m | landmark on the axis (the card) |
| L3 d1 / d3 / d6 | **11.6 / 11.6 / 13.1 m (12 m cliffs)** | d6: drop at 9.1 m | row 7 |
| L3 d2 kb1b -> kb2 | 16.1 m (6 m crucible) / >25 m | none | |
| L4 d1, d3, d5, d6, d7 | >25 m (d5: 19.7 m ride tower) | **void at 8.1 / 8.1 / 6.1 / 6.1 / 8.1 m** | row 3 |
| L4 d2 p2 -> p3 | 18.1 m (1.2 m block) | none | |
| L5 d2-d4, d6 (belfries) | >25 m / 10.4 m | drop to -2.5 at 10.1 m (the 17-18 m pits) | four identical gates (row 9) |
| L5 d8 / d9 / d10 | >25 m | drop at 3.1 / 6.1 / 10.1 m (d10: the 74 m chasm) | |
| all others | >= 14 m ahead | none | |

Distance between fight rooms (door to door, through the links): L1 a2-b1 adjacent (4 m doors), **b1 -> a8 162 m**. L2 g1-g8 all
adjacent (4 m doors). L3 kb2 -> kb2c 36 m + 8 m (24 m of it vertical), kb2c -> kb3 adjacent. L4 p1 -> p3 50 m + 8 m, **p3 -> p5 92 m** + 8 m,
p5 -> o3 96 m + 12 m. L5 v2 -> v6 104 m + 16 m, v6 -> v11 124 m + 20 m.

## What the code does that halts or slows the player (all rows enabled:true unless marked)
| what | duration | where | applies to |
|---|---|---|---|
| Commit gate: wave 1 waits for the feet 3 m in, or 3 s | ~0.4 s at a run (harness doorway pilot: first wave at 1.20 s after a 1 s wait) | `design.n43.rooms43.commitM` 3 / `commitMaxS` 3; Campaign24.cs:965-971 | every fight room |
| Start grace: 6 m or 4 s, then 1.6 s with no attacks | ~0.75 s at a run, up to 4 s (harness: g1 and kb2 first wave at 4.20 s) | `design.n40.start40`; SurvivalSim.N40.Start.cs:95-123 | a2, g1, kb2, p1, v2, and every room after a death |
| Stand-in hold | until the 3 targets die, cap 20 s | `design.n43.rooms43.standInHoldS` 20; Campaign24.cs:851 | a2 |
| afterLesson43 | until the lesson, cap 20 s | `afterLessonMaxS` 20 | a8 (the perch); g6 (2 kills: always given) |
| Wave gap + queue | 0.8 s + 0.2 s, +0.1 s per extra body | `design.n24.campaign.waveGap`; `design.n36.level36.nextWaveS` / `staggerS`; Campaign24.cs:857-865 | every multi-wave room |
| A skipped "reached" lesson holds the door | 8 s after the last kill | `design.n43.rooms43.lessonCapS` 8; Campaign24.cs:885-896 | a3 (rect at the exit: always met), a8, kb2 (rect 6-10 m in: met on entry) |
| Last kill slow-mo | 0.15 s at 25 % | `design.n36.level36.lastKillSlowS` / `lastKillSlowScale` | every arena room |
| Debut card slow-mo | 1.5 s at 35 %, once per enemy kind per save | UnityTuning.Debut20.cs:8-12 | first sight of each kind (L1 2, L2 5, L3 1) |
| Death | 1.2 s, then the room from wave 1 | `design.n24.campaign.retryDelay`; Campaign24.cs:693-705 | all |
| fallReturn43 | speed 0, 0.5 s fade (the sim runs on), -10 HP, back to the lip | Campaign24.cs:999-1029; UnityTuning.Ui43.cs:61-63 | b2, g7 |
| respawnY | speed 0, back to the room's start, -25 % HP | `design.n29.roomRules.fallCostFraction` 0.25 | p2, p4, o1, o2 (o3: the loom37 ledge catch, -20 HP) |
| Boss reveal / phase / death | row 8 | Boss37.cs:268 and the boss rows | the five bosses |
| k5 teleport | speed 0 | Campaign24.cs:676; Mover.cs:283 | k5 |
| Player-chosen slow-mo (SKYGRIND TIME, ride + zoom, ride aim, bend, parry) | 25-75 % for 0.3-5 s | `adds.skygrindTime`, `design.n39.slow39`, `design.n27.slowAim27`, `design.n34.bend34` | the player's choice: not a flow break |

The sub-agent found no full freeze, no `Time.timeScale` change, no input lock and no mid-level results screen in campaign play. Lessons
never freeze time. The big pick at level end is OFF (`design.n24.bigPick.campaign` false). Hit freezes are budget-capped at 0.5 s
(`design.impact.budget`).

## Harness output quoted (n43 level files, levels 1-3, seed 1)
`n43rooms clear --n43`: seconds from entry to door open; the pilot stands at the exit side, never performs a verb, and every enemy is
removed 0.5 s after it appears.
- kill path: a2 32.0, a3 1.6, a4 3.1, a5 5.0, b1 5.6, a8 28.9; g1 1.5, g2 3.2, g3 3.9, g4 3.1, g5 5.8, g6 1.8, g7 5.8, g8 4.8; kb2 13.6
  (its lesson rect was never crossed: + lessonCapS), kb2c 3.3, kb3 5.4.
- pit path: level 2 rooms 17.1-36.5. kb2c HELD, and kb3 and every room after it shut. Summary: 62/68 opened, RED (6 held).

`n43rooms idle --n43 --wait 1`: the first wave came 1.20 s after entry in every room except a2 (20.19 s: stand-ins), a8 (20.19 s: the
afterLesson cap), g1 and kb2 (4.20 s: start40), and g6 (1.00 s). No enemy was ever in view before the fight started (0.0 enemy-seconds).

## Not checked
- Every boss arena's own geometry (a9's tower, x9's floes, the wyrm45 arena, o3's threads, v11's platforms), and the boss phase counts
  for x9 and v11.
- The wyrm's emerge duration, and whether its die time gates k5's clear.
- Whether Campaign24's `fallReturn43` reader works in rooms that also carry `respawnY` (decides whether row 3's fix is data only).
- Whether a burrower can actually leave its room or fall under the floor in play (row 11), and whether lava kills count toward
  `husksDead` lessons on n45-iceline's g7. The harness ran on n43's g7, which has no lava there.
- How a person plays any of this: every human-time number above is a config sum or bot data, not seen on a frame. The a8 -> a9 boss door
  frame that GG called "a brick wall" (G10) is code and view, and was not looked at.
- Levels 4-5 have no harness timing (the n43rooms commands cover levels 1-3 only).
