# Enemy routes: why husks can't reach (or leave) upper floors, and how shipped games solve it

2026-10-06. Read-only research for GG's report: "The enemy AI struggles to go above floors and can't really get out of
floors most of the time ... you can kind of just camp out ... there always really needs to be a route that enemies can
climb up or something or just find a way to pass by normally."

Method: the repo's Core nav code and config read at `port/stage3` (d4673c5); the web's `src/ai`, `src/enemies`, `src/nav`,
`POWER.md` and `SURVIVAL.md` read at `night6` (read only); a read-only Node analysis of the two exported nav grids
(`PortExport/maps/survival.json` = ARENA, `Design/maps/drydock.json` = DRYDOCK) in the session scratchpad (not in the
repo); web sources for sections 3-4. Nothing in Unity was run. Every in-game behaviour in section 1 is read from code,
**not measured in Play**. Sources marked [unverified] were not confirmed from a fetched page. Licences marked [lic] were
read from the LICENSE file or the GitHub licence API.

## 0. Summary

- **Husks have no verb that climbs more than 2.2 m or drops more than 5 m.** The nav grid's links are walk (0.6 m step),
  jump (up to 2.2 m), drop (up to 5 m) and gap (4 cells, +1 m / -2.5 m) (`NavGridBuilder.cs:11-15`, `:417-464`;
  `ai.grid` in `PortExport/config.json`). Every upper floor of both maps is 8-30 m up, so the only ways up are a walkable
  ramp or a chain of <=2.2 m steps. Pads are player-only (`Mover.cs`; no enemy code reads pads).
- **ARENA: no grid node above 5 m is reachable from the ground (0 of 3,227), and none can get back down** (0 of 3,227):
  a husk that gets up there by its free leap is stranded. DRYDOCK is better because of the slipway ramps (42 % of the
  8-12 m band) but the hull deck (560 nodes at 11 m), every rib top (8-12 m) and the crow's nest (30 m) are unreached and
  can't be left (section 1.4).
- **The only way up today is the husk's free leap** (`Husk.cs:242-251`): when the player is >= 1.5 m above and <= 10 m
  across it launches at the player's spot, every 3 s, with **no line-of-sight or headroom check**. The flow field parks
  husks at the foot of the wall (its seeds are the floors within 4 m under you, `NavField.cs:555-586`), the worst place
  to leap from: the arc meets the wall and slides up it. Under a floor it leaps into the ceiling.
- **The worst perches switch the whole director off.** On ARENA's four 24 m spire tops and the 29.2 m pinnacle (576
  nodes) the field gets no seed a ground husk can reach, so every husk's distance is Infinity: no spawn passes
  `Director.cs:605`, the camp HUNTERS can't spawn either, and live husks go `mode = "lost"`.
- **The web was the same** (POWER.md P1: "up to 2.2 m is a jump link, drops up to 5 m"); its answer to height was other
  enemies: the DRONE (flies) and the LANCER (a perch sniper that "prioritizes campers"). In the port the DRONE is a stub
  (`Core/Enemies/Drone.cs`) and the LANCER and WARDEN are spawned as husks (`Director.cs:437-444`).
- **Shipped games cap climb-up at about 5 m** (L4D `ClimbUpHeight` 200 units = 5.1 m; Vermintide 2's ledge links
  5.1 m) and let enemies drop much further (L4D `DeathDrop` 400 units = 10.2 m). Higher ground gets authored routes
  (CoD barriers and jump-downs, Days Gone ladders) or a special tier (Dying Light Virals, WWZ pyramids). Stuck or
  unreachable enemies are recycled out of sight (CoD BO3: 30 s; KF2: 5 s; L4D Tank: 20 s). Height is punished by other
  enemies (Smokers "select areas above the Survivor team").
- **Recommended** (GG's call, rule 7):
  - LONG DROP up to 12 m for every walker: on its own it lifts ARENA's "can get down" from 71 % to 91 %.
  - CLIMB 2.2-5 m for husks and spitters, and 2.2-10 m for swarmers, with a tell and a knock-off.
  - LEAP-UP links that replace the free leap.
  - AUTHORED links in the map file for the 9-30 m perches, which no generic verb reaches.
  - The new links go in as a Unity-side extra layer behind a design flag, so the web's grid export and the
    `flowField` golden vectors stay untouched.
  - A headless ROUTE JUDGE: 0 dead perches; every standable node reached within 12 s and able to get down within 6 s.
  - A CAMP JUDGE: damage taken on any perch at least 50 % of the ground baseline.
  - A height-aware camp score and the DRONE / LANCER ports. Details in section 5.

## 1. Today's enemy navigation (the repo, file:line)

### 1.1 The grid and its links

- `Core/Nav/NavGridBuilder.cs` ports the web's `navGrid.js` build. Header `:6-19`: 1 m columns scanned top-down with
  rays; a node is an up-facing surface where the probe capsule fits (`probeRadius` 0.45, `minClear` 1.9 m). Links
  (`LinkNode`, `:398-466`):
  - **walk**: 8 neighbours within `step` 0.6 m (`:418-428`); a riser over `stepSmooth` 0.2 m becomes a small jump
    (`:423-427`); diagonals need both side cells (`:421`).
  - **jump**: an orthogonal neighbour 0.6-2.2 m higher (`jumpUp`), with headroom `dy + bodyHeight` over the source
    (`:430-433`), cost `1 + jumpCost 1.5 + dy`.
  - **drop**: an orthogonal neighbour up to 5 m lower (`drop`) with clearance at the landing (`:434-437`), cost
    `1 + dropCost 0.5`.
  - **gap**: over 2-4 empty cells (`gapCells` 4; diagonals 3) to a floor at most 1 m higher / 2.5 m lower, two ray
    checks at 1.1 m and 0.4 m lift (`:440-464`), cost `k + gapCost 2`.
  - No climb, no ladder, no long drop, no pad, no authored link. Link kinds are `Link.Walk/Jump/Drop/Gap = 0..3`
    (`NavField.cs:13-16`).
- The exported grids: ARENA 11,228 nodes, links walk 79,103 / jump 141 / drop 360 / gap 878; DRYDOCK 10,732 nodes,
  walk 74,486 / jump 48 / drop 161 / gap 131 (each file's `navGrid.linkCounts`). Measured from the files: ARENA's jump
  links rise 0.50-2.01 m (median 1.50), its drops fall 0.90-5.00 m (median 2.50); DRYDOCK's jumps 0.25-2.12 m, drops
  0.62-4.87 m.

### 1.2 The flow field and the brain

- `NavField.cs` (ports `flowField.js` + `navField.js`): two Dijkstra fields toward the player(s) over the reverse links,
  `jumpField` (every link) and `walkField` (walk + drop: `Usable`, `:195`, for kinds with `ai.jumps.<kind>.jumps` false,
  the CHARGER), recomputed every `ai.flow.every` 0.25 s when the player's node changes, 2,500 pops a step (`:614-628`).
- **Seeds** (`SeedGoal`, `:555-586`): your own node at 0, plus every node within `seedRadius` 4 columns that is not
  above you, at `hypot + (yourY - y) x seedHeight 2 + seedBase 12`. The web's comment (`navField.js:46-47`): "with no
  route to your own node (a pillar top, a ledge they can't climb) enemies still gather at the nearest place they can
  reach". So a husk below a perch is routed to the floor under you, at the wall's foot.
- `RouteFrom` (`:719-780`) walks `BestLink` up to 12 links ahead; a jump / gap link at the husk's own node becomes a nav
  jump; `HuskBrain.Navigate` (`HuskBrain.cs:869-942`) asks it (`AskJump`, `:882-890` on a repath, `:906-912` on the
  field), or sets `ai.blocked` for a non-jumper (the CHARGER).
- **Nav jumps** (`HuskNavJumper`, `HuskBrain.cs:312-382`): a crouch of `ai.jumps.<kind>.tell` (husk 0.12 s, swarmer
  0.05, spitter 0.2), then a ballistic arc with apex `ai.jumps.<kind>.apex` (1.1 m) landing `ai.jumpLandIn` 0.7 m past
  the link's end. `NavJumper.cs:1-2` still says "S6 fills this in; until then nothing jumps": a stale comment (the
  subclass exists).
- **The watchdog** (`HuskBrain.cs:944-1001`, `ai.watchdog`): no progress for 0.5 s: a 1.5 m hop (apex 1.2), at 1.0 s a
  side-step, at 1.5 s an A* repath (`nav.Path`); stuck 3 s **and off-screen**: `Relocate` (`NavField.cs:811-831`)
  teleports it to the nearest reachable node >= 12 m away in your 60 deg front cone. On-screen it never relocates.
- `mode = "lost"` (`HuskBrain.cs:925`) when the field has no distance at the husk: it arrives straight at its ring goal
  with no nav, so it walks off edges or presses into walls.

### 1.3 The husk's free leap (the only way up past 2.2 m)

- `Husk.cs:242-251`: standing, not winding up or stunned, `leapCooldownSteps == 0`, `above >= husk.leapMinHeight` 1.5 m
  and `horizontal <= husk.leapMaxHorizontal` 10 m: `LeapVelocity` toward the player's feet minus `stopDistance` 1 m,
  apex `leapApexAbove` 1 m, cooldown `leapCooldown` 3 s, event `huskLeap`.
- `CombatMath.cs:118-124`: the peak is `max(from.y, to.y) + apexAbove`, so the leap reaches **any height** (gravity
  `GravityFor(1.5 m, 0.36 s)` = 23.1 m/s2: an 8 m leap launches at 20.4 m/s and lands in about 1.2 s).
- What the code does not check: line of sight to the landing, headroom over the husk, or whether the arc clears the lip.
  A husk at the wall's foot (where the seeds put it) launches nearly straight up; the wall removes its horizontal speed
  (`Husk.Move`, `:309-313`) and it falls back. Inside a building it leaps into the floor above. This matches "struggles
  to go above floors" but was **not checked in Play**.
- The Swarmer's pounce (`Swarmer.cs`, `pounceRange` 4.5 m, apex 0.9 m) and the Spitter's lobbed globs (`Spitter.cs:2-6`,
  6-24 m, "while you are hunted (camping) it aims at the spot you stand on") are the other ported answers to height.

### 1.4 Measured from the exported grids (read-only Node script; every zone open)

"Reached" = a husk starting on any ground node (y <= 0.3 m) can get there over the grid's links; "can get down" = from
there it can reach a ground node; the cost is the grid's m-equivalent cost / 6 m/s (`husk.speed`).

**ARENA** (`PortExport/maps/survival.json`, 7,538 ground nodes):

| band (m) | nodes | reached from the ground | can get down to the ground | median / max cost from the ground (s) |
|---|---|---|---|---|
| 0.3-2.5 | 298 | 228 (77 %) | 298 (100 %) | 0.7 / 1.4 |
| 2.5-5 | 165 | 32 (19 %) | 165 (100 %) | 1.2 / 1.6 |
| 5-8 | 388 | **0** | **0** | - |
| 8-12 | 959 | **0** | **0** | - |
| 12-20 | 755 | **0** | **0** | - |
| 20+ | 1,125 | **0** | **0** | - |
| all | 11,228 | 7,798 (69 %) | 8,001 (71 %) | |

The largest unreached walk-connected components (zone tags: 4 = HEIGHTS, 5 = ISLANDS):

| nodes | y (m) | where (x, z) | smallest rise from a reached node <= 4 m across |
|---|---|---|---|
| 264 | 8 | 14.5..24.5, 24.5..47.5 (the ARMORY roof; the secret relay at (22.5, 8, 46.5)) | 5.3 m |
| 4 x 240-256 | 24 | the four corner spires | 24 m (no reached floor within 4 m) |
| 3 x 84 | 9 | the pad landings at x or z +-40 | 7.5 m |
| 4 x 64-68 | 9 | the HEIGHTS ledges along the walls | 9 m |
| 64 + 64 | 17.5, 12.5 | ISLANDS | 17.5 m, 12.5 m |

- **Dead perches: 576 nodes** (the four 24 m spire tops away from their edges, and the 29.2 m pinnacle at
  (-40, 29.2, 40)): standing there, `SeedGoal` finds no node a ground husk reaches, so every ground husk's field
  distance is Infinity. Consequences in code: `Director.PickNode` skips every node (`Director.cs:603-605`: "a route to
  you on the grid"), so no squad and no HUNTER squad spawns (`:737-756` goes through the same pick); `RouteFrom`
  returns false and husks go `lost`. These are exactly the validator's player-only spots (`Reports/MAP-survival-validate.md`:
  zone HEIGHTS "the enemies' grid 0 of its 2502 grid nodes", ISLANDS "0 of its 436"; "enemy-only islands: 34
  components (3227 grid nodes) the enemies' grid can't leave").
- The other 2,854 unreached nodes do get seeds under them, so husks come and stand at the foot and the free leap fires
  (3,286 of the 3,430 unreached nodes have a reached node <= 10 m across and >= 1.5 m below them).
- Design/unity-config.json's exfil note already names one: "HEIGHTS (-40, 24, -40) ... node 911 is on no nav route (the
  spire's 2.5 m ledge steps are over ai.grid.jumpUp 2.2 m)".

**DRYDOCK** (`Design/maps/drydock.json`, 7,324 ground nodes):

| band (m) | nodes | reached from the ground | can get down to the ground | median / max cost from the ground (s) |
|---|---|---|---|---|
| 0.3-2.5 | 168 | 168 (100 %) | 168 (100 %) | 0.7 / 1.3 |
| 2.5-5 | 192 | 160 (83 %) | 192 (100 %) | 2.0 / 2.8 |
| 5-8 | 610 | 208 (34 %) | 252 (41 %) | 3.9 / 7.0 |
| 8-12 | 1,434 | 596 (42 %) | 596 (42 %) | 7.0 / 8.5 |
| 12-20 | 896 | 0 | 896 (100 %) | - |
| 20+ | 108 | 0 | 0 | - |
| all | 10,732 | 8,456 (79 %) | 9,428 (88 %) | |

- Reached up high: the slipway deck (10 m) by its two 40 m ramps at 14 deg (MAP-DESIGN.md:228). Unreached and
  inescapable: the hull deck (560 nodes at 11 m, pads only), the eight rib tops (8-12 m, 2 columns wide), the 30 m crow's
  nest (36 nodes). The 896 nodes at 14 m have zone tag -1 (the perimeter wall tops); whether a player can stand there
  was **not checked**.
- No dead perches: every unreached node has floor within 4 m under it, so husks gather below and leap.
- MAP-DESIGN.md:52-53 makes this a rule: "A ledge everyone climbs <= 2.0 m; a ledge that needs a verb (walkers can't
  follow) 2.5 m". Nothing in the design rules requires a husk route to every perch.

### 1.5 What-if: extra links on the same grids

The same script with two approximate link kinds added (orthogonal neighbours only; a CLIMB up to H m with headroom
`dy + 1.9 m` over the source, cost `1 + 1.5 + 2 x dy`; a LONG DROP up to H + 4 m with headroom at the landing, cost
`1 + 0.5 + 0.25 x dy`). It does not run the real `Riser` / `ClearLine` collider checks, so it over-counts a little.

| map | climb <= / drop <= | reached from the ground | can get down | dead perches |
|---|---|---|---|---|
| ARENA | today (2.2 / 5) | 69 % | 71 % | 576 |
| ARENA | 4 / 8 | 70 % | 84 % | 576 |
| ARENA | 6 / 10 | 72 % | 88 % | 576 |
| ARENA | 8 / 12 | 82 % | 91 % | 576 |
| ARENA | 10 / 14 | 86 % | 98 % | 576 |
| ARENA | 12 / 16 | 92 % | 99 % | 288 |
| DRYDOCK | today (2.2 / 5) | 79 % | 88 % | 0 |
| DRYDOCK | 4 / 8 | 89 % | 91 % | 0 |
| DRYDOCK | 8 / 12 | 91 % | 99 % | 0 |
| DRYDOCK | 12 / 16 | 99 % | 100 % | 0 |

Reading: **long drops fix "can't get out of floors" cheaply** (ARENA 71 % -> 98 % at 14 m). **Generic climbing does
not fix the tall perches**: ARENA's 9 m ledges need a 7.5-9 m climb and its 24 m spires can't be climbed at any sane
height; DRYDOCK's 11 m hull deck needs 11 m. Tall perches need authored routes (a ladder, a stair, a chain of <= 6 m
ledges, or a designer link) or a ranged / flying answer.

### 1.6 What the web had that the port lacks

- Same nav (POWER.md P1 item 3: "up to 2.2 m is a jump link, drops up to 5 m are drop links, gaps up to 3 m"); same free
  leap (`husk.js`); same seeds (`navField.js:46-47`). The port is faithful: the gap is in the design, not the port.
- The web's height answers that the port doesn't play: the **DRONE** (`drone.js`: flies at `hoverMin` 4 - `hoverMax`
  8 m, "climbs over pillars it would hit (sphere probe, no pathfinding)", orbs from 12-18 m; the port's `Drone.cs` is a
  NIGHT19 tests-first stub) and the **LANCER** (`lancer.js`: "perches on high points ... never walks: it blinks between
  perches", 70 m hitscan for 25 after a 1.2 s lagging laser, "while you are hunted (camping) its laser follows twice as
  fast"; not ported, spawned as a husk: `Director.cs:437`, `:450`). SURVIVAL.md section 6 at 100 % camp: "Lancers get
  priority line on the player; Spitters target the player's current spot with puddles". The spitter half is ported.
- The web's P1 acceptance test (POWER.md P1 item 8): "spawns 60 ground enemies at random reachable cells on the real
  arena and reports: percent that reach within 3 m of a standing player inside 25 s, and any enemy motionless for over
  2 s while more than 5 m from the player. Pass thresholds: 95 percent reach, zero long-motionless." It tests a player on
  the ground only, never on a perch: the camping case was never judged.
- The web's player graph (`src/nav/navGraph.js`, ported as `Core/Nav/NavGraph.cs`) has hop / climb / double / dash
  edges ("hop/climb standing jump (+ double): start still, air-accelerate toward the target (ledge stairs)",
  `navGraph.js:13`; overheads `climb: 2.5`, `:276`), flown in a small Mover simulation. Its "climb" is the player's
  standing double jump, not a wall climb, and the graph is RIVAL's and the validator's, not the enemies'. Its
  build-by-simulation (every airborne edge flown with the real vertical rules and swept through the collider) is the
  model for validating new enemy links (section 5.2).

The same what-if with the two verbs apart (reached = all nodes; "> 5 m reached" = the nodes above 5 m):

| map | climb <= / drop <= (m) | reached | > 5 m reached | can get down | dead perches |
|---|---|---|---|---|---|
| ARENA | - / 5 (today) | 69 % | 0 % | 71 % | 576 |
| ARENA | - / 10 | 69 % | 0 % | 88 % | 576 |
| ARENA | - / 12 | 69 % | 0 % | 91 % | 576 |
| ARENA | - / 14 | 69 % | 0 % | 97 % | 576 |
| ARENA | 5 / 5 | 71 % | 0 % | 71 % | 576 |
| ARENA | 10 / 5 | 86 % | 51 % | 71 % | 576 |
| ARENA | 5 / 12 | 71 % | 0 % | 91 % | 576 |
| DRYDOCK | - / 5 (today) | 79 % | 26 % | 88 % | 0 |
| DRYDOCK | - / 12 | 79 % | 26 % | 99 % | 0 |
| DRYDOCK | 5 / 5 | 87 % | 56 % | 88 % | 0 |
| DRYDOCK | 10 / 5 | 93 % | 76 % | 89 % | 0 |
| DRYDOCK | 5 / 12 | 89 % | 60 % | 99 % | 0 |

## 2. How shipped games get melee hordes onto high ground

The numbers convert Source / CoD units at 1 unit = 1 inch (0.0254 m) [unverified conversion].

### 2.1 Left 4 Dead (Valve)

- **Climbing is reactive, not linked.** Mike Booth, "The AI Systems of Left 4 Dead" (AIIDE 2009),
  https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf (text extracted from the PDF):
  - "To ensure the Common Infected horde are always dangerous, they have the ability to rapidly climb".
  - "Climbing is algorithmic, using a similar technique to local obstacle avoidance". It "solves two major issues:
    navigating the complex geometry of the post apocalyptic world of Left 4 Dead; navigating over movable physics props".
  - The steps are hull traces. Probe for an obstacle ahead, find the ceiling, then scan up for the first clear trace. A
    downward trace "finds the precise height of the ledge to be climbed". Downward traces "that step backwards determine
    the forward edge of the ledge". Then "pick closest match animation from dozens of motion captured climb animations
    of various height increments".
- **The specials answer height.** From the same PDF: "Boomers spawn ahead, since they are slow and can't chase well";
  "Smokers attempt to select areas above the Survivor team"; "75% of Mobs come from behind". A Survivor bot "out of
  place for any reason ... is teleported near the team when no human is looking at it".
- **Nav limits.** Source SDK 2013 `nav.h`,
  https://raw.githubusercontent.com/ValveSoftware/source-sdk-2013/master/src/game/server/nav.h (fetched). For builds other
  than Counter-Strike:
  - `StepHeight` 18 (0.46 m)
  - `JumpCrouchHeight` 64
  - `ClimbUpHeight` 200 (5.1 m), "height to check for climbing up"
  - `DeathDrop` 400 (10.2 m), "distance at which we will die if we fall"; Counter-Strike's is 200
  - `CliffHeight` 300

  A comment tying the larger DeathDrop to zombies taking no fall damage was reported but not seen in the fetched text
  [unverified].
- **Nav attributes.** The Valve Developer Community's "Nav Mesh Editing",
  https://developer.valvesoftware.com/wiki/Nav_Mesh_Editing:
  - JUMP, CROUCH, NO_JUMP and STAIRS, with no CLIMB attribute. "One-way connections ... are usually used for 'jump-down'
    links from a ledge to the ground."
  - L4D adds EMPTY (no wanderers, e.g. rooftops), NO_MOBS and STOP_SCAN ("Infected bots won't navigate through this"):
    https://developer.valvesoftware.com/wiki/List_of_L4D_Series_Nav_Mesh_Attributes.
- **The Tank's frustration** (left4dead.fandom.com/wiki/The_Tank, a secondary source). If it can't reach a Survivor "for
  a prolonged amount of time, the Tank will instantly die". It throws rocks at survivors it sees and can't reach.
  `z_frustration_lifetime` is 20 s (https://commands.gg/l4d2/z-frustration-lifetime).

### 2.2 Call of Duty Zombies (Treyarch)

- **Secondary source.** callofduty.fandom.com/wiki/Zombies_(Treyarch): "If a zombie cannot hit a player in a certain
  time period, it will respawn elsewhere ... if the players go too far away from a zombie, it will respawn near them".
  Der Riese was "the first map where zombies can climb over areas and jump down".
- **BO3's shipped script.** `zombie_utility.gsc` `round_spawn_failsafe`,
  https://bo3explorer.zeroy.com/zombie__utility_8gsc_source.html:
  - It checks every 30 s (`level.failsafe_waittime`), plus 10 s for crawlers.
  - A zombie that "hasnt moved 24 inches in 30 seconds" is killed and put back in the round's count
    (`zombie_total++`). One that tore a board down in the last 8 s is skipped.
- **Cleanup on `zm_giant`.** https://bo3explorer.zeroy.com/zm__giant__cleanup__mgr_8gsc_source.html:
  - A zombie at least 5 s old is deleted while nobody looks. "Nobody" means `N_CLEANUP_FOV_COS` 0.766, about 80 deg.
  - It must be outside the active zones and more than 500 units (12.7 m) from every player, or 435 if the player is
    ahead. It is then respawned with its stored health.
  - The check runs at least every 3 s.
- No documented BO4 / Cold War "anti-camping" feature was found [unverified / not found].
- In CoD the barrier (boards, rebuilt by the player) is the designed choke: the player holds a lip, and the zombie has
  to climb in through it (MAP-RESEARCH.md / NIGHT11-RESEARCH cover the map side).

### 2.3 Others

| Game | What lets the horde reach you up high | Source |
|---|---|---|
| World War Z | Zombie pyramids: "if you don't topple them, they'll pile high enough to reach any high ground you've managed to secure"; up to 500 on screen. No heights published. | https://blog.playstation.com/archive/2019/12/16/horde-mode-z-update-launches-for-zombie-shooter-world-war-z-tomorrow/ |
| Dying Light | Two tiers. Biters are "slow and stupid" and can't climb high ledges (rooftops are the refuge). Virals "are really fast and they can climb, and they show up typically when you make noise". | https://www.gamewatcher.com/interviews/dying-light-interview/11576 ; dyinglight.fandom.com Biter / Viral (secondary) |
| Days Gone | Freakers "always find a way to climb up to whatever 'safe' high ground"; ladders are their weakness (a choke). Hordes are 25-500. | https://whatculture.com/gaming/days-gone-13-essential-tips-tricks-the-game-doesnt-tell-you?page=11 (secondary); GDC 2018 "Freak-O-System" talk not read |
| Killing Floor 2 | Stuck check every 0.5 s; suicide after 5 s stuck (1 s with no nav path). Teleport closer after 5 s unseen and irrelevant, 7 s cooldown, not within 10 s of spawning, only while 12 or more AI remain. Searches "NonExploited" nav points when the target is off the mesh. | https://github.com/XpecV52/KF2-Source `KFAIController.uc` (no licence: reference only) |
| Vermintide 2 | Offline "ledgelator" smart objects on Autodesk Gameware: `jump_up_max_height` 5.1 m (above it, down only); animation buckets 1 / 3 / 5 m; gap jump 4 m. Link costs: ledge / jump / door / plank x1.5, teleporter 5, fire 10. Clan rats run at 4.75 m/s. Unreachable targets: move to the nearest mesh point and run an `unreachable_timer`. Rat Ogre `jump_slam`: weight ramps over 3-9.1 s of chase at 3-15.6 m, a 45 deg ballistic leap, landing snapped to the mesh within 0.5 m, arc checked by segmented raycasts. | https://github.com/Aussiemon/Vermintide-2-Source-Code (decompiled, no licence: reference only) |
| DOOM (2016) | Arenas "carry a lot of verticality"; "imps can jump long or short distances using the same animation that is tweaked at runtime"; attack tokens. Designer-placed traversal links [unverified]. | https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016- ; GDC 2018 "Embracing Push Forward Combat" (not read) |
| ULTRAKILL | Cyber Grind: "a 16x16 grid of tiles that changes each wave" (the perch moves). Mindflayers "frequently teleport near V1"; the Virtue's beam "is not affected by height or cover". | ultrakill.fandom.com (secondary) |
| Deep Rock Galactic | Glyphids walk on walls; patch notes: "Make Molly and the large spiders prefer to walk on floor", "Enemies are now less likely to go straight through thin cave walls". No technical write-up found. DRG: Survivor swapped NavMesh for a flow field over a box around all enemies plus the player. | https://www.deeprockgalactic.com/updatesold ; https://unity.com/blog/optimizing-deep-rock-galactic-survivor-for-mobile |
| Risk of Rain 2 | Separate ground and air node graphs per hull size; the director spawns at each monster's min/max distance from a random player, retargets every 1-10 s, caps at 40 monsters. | https://riskofrain2.wiki.gg/wiki/Developer_Console ; riskofrain2.wiki.gg/wiki/Directors (secondary) |
| Halo 2 | Designer-placed firing positions; a failed search switches behaviour instead of retrying a blocked path. | https://www.gamedeveloper.com/programming/gdc-2005-proceeding-handling-complexity-in-the-i-halo-2-i-ai |
| Factorio | Biters path over 32 x 32 chunk abstractions with "Reverse Resumable A*". | https://factorio.com/blog/post/fff-317 |

### 2.4 The patterns

1. **Climb-up is capped at about 5 m everywhere.** L4D checks for climbing up to 200 units (5.1 m); Vermintide 2's
   ledge links go up to 5.1 m and are down-only above that. Higher ground is reached by authored routes (CoD's
   barriers and jump-downs, Days Gone's ladders, Vermintide's ledgelator objects) or by a special tier (Dying Light's
   Virals, WWZ's pyramids, Vermintide's Rat Ogre leap).
2. **Drop-down is much longer than climb-up.** L4D's DeathDrop is 400 units (10.2 m) outside CS; Recast-style tools
   generate drop-downs automatically.
3. **Unreachable or stuck enemies are recycled, not left standing.** CoD BO3 requeues after 30 s with under 24 inches
   of movement; KF2 teleports after 5 s unseen and suicides after 5 s stuck; L4D's Tank dies after its 20 s
   frustration. All of them do it out of sight.
4. **Height is punished by other enemies.** Smokers spawn above you; Boomers spawn ahead; the Tank throws rocks; the
   Virtue's beam ignores cover. The web's own design had this too (the DRONE, the LANCER, the Spitter's puddles at 100 %
   camp).
5. **The refuge is designed, not accidental.** Dying Light's rooftops are safe from Biters only. CoD's safe-looking
   spots get a respawn failsafe.

## 3. Open-source tech

| Project | Licence | What it gives | Notes |
|---|---|---|---|
| DotRecast, https://github.com/ikpil/DotRecast | Zlib [lic] (`LICENSE.txt`, GitHub API) | C# Recast + Detour; `netstandard2.1; net8.0; net9.0; net10.0`; "Dependency-Free"; off-mesh connections (`DtOffMeshConnection`); `DtJumpLinkBuilder` (`src/DotRecast.Detour.Extras/Jumplink/`, fetched: HTTP 200) with EDGE_JUMP, EDGE_CLIMB_DOWN, EDGE_JUMP_OVER | Its jump links are drop-downs and gap jumps; climb-up would be a reversed link. Default tool config: climb-down 1.5-3.2 m, edge jump down up to 2.5 m. A Recast navmesh would replace the web's grid: the exactness rules (CLAUDE.md rules 3-4) make it a reference, not a drop-in. `Detour.Extras` pulls Newtonsoft.Json. |
| recast4j, https://github.com/ppiastucki/recast4j | Zlib [lic] | The Java origin of DotRecast's jump-link builder | reference |
| recastnavigation, https://github.com/recastnavigation/recastnavigation | Zlib [lic] (`License.txt`, GitHub API) | `dtOffMeshConnection` (`DT_OFFMESH_CON_BIDIR`), `rcConfig.walkableClimb` ("Maximum ledge height that is considered to still be traversable"), `dtQueryFilter::setAreaCost` per-area multipliers | Off-mesh links are placed by hand in RecastDemo; no jump generator in the main repo |
| Unity AI Navigation (NavMesh Link), https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshLink.html | package licence not checked; legacy NavMeshComponents MIT [lic] | Width, cost override, bidirectional, area type; auto links only "Drop-Down" and "Jump-Across" | **Not allowed for gameplay here** (CLAUDE.md rule 4: the sim is pure C#). Useful only as a vocabulary. |
| Flow Field Tiles, Elijah Emerson, Game AI Pro ch. 23, https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter23_Crowd_Pathfinding_and_Steering_Using_Flow_Field_Tiles.pdf | (a book chapter) | 1 x 1 m cells in 10 x 10 sectors, 8-bit costs (255 = wall), portal-graph A* then per-sector fields | No vertical links; a portal / link edge is the natural place for them (inference) |
| danjm-dev/flow-field-pathfinding; kingstone426/NativeFlowField; LifeIsGoodMI/Unity-Flow-Fields | MIT [lic] each | Grid flow fields (Unity / DOTS) | 2D; nothing SKYGRIND's layered grid lacks |
| rnixik/spider-navigation-plugin, https://github.com/rnixik/spider-navigation-plugin | MIT [lic] | UE4 nav points on floors, walls and ceilings with A* | The "surface graph" idea for a wall-walking swarmer |
| Warframe "Getting off the NavMesh" (GDC 2015) / Game AI Pro 3 ch. 21, https://www.gameaipro.com/GameAIPro3/GameAIPro3_Chapter21_3D_Flight_Navigation_Using_Sparse_Voxel_Octrees.pdf | (a book chapter) | Sparse voxel octree 3D flight | For the DRONE if it ever needs real 3D paths; the web's drone uses probes, not paths |

**Fit for SKYGRIND.** The web's layered grid is already a multi-level graph: a node per floor per column, links
between them, and a Dijkstra field over the reverse links. Adding link kinds is the smallest change. Recast / DotRecast
would replace a ported, golden-tested system. No library is needed. The one technique worth borrowing is
DotRecast / recast4j's jump-link *builder idea*: sample the edges, fly a trajectory, keep the links whose arc is clear.
The repo already does this for RIVAL in `NavGraph.cs`.

## 4. What the gap costs today (the camping loop, read from code)

The perch loop, read from code and **not measured in Play**:

1. You stand on a perch more than 2.2 m up with no route.
2. The field seeds the floor within 4 m under you, so husks walk to the wall's foot.
3. The free leap fires every 3 s from a bad spot, with no line-of-sight or headroom check.
4. Husks that do land up there can't come down past 5 m.
5. On the four ARENA spire tops (576 nodes) nothing spawns at all.
6. The camp score only measures horizontal lingering (`Director.cs:141-171`, a 10 m radius). Its HUNTERS (swarmers
   and a charger) are picked through the same reachability filter, so they never appear on a dead perch.
7. What's left on a perch: the ported Spitter, which lobs at 6-24 m with line of sight and aims at your spot while
   you're hunted. The web's DRONE and LANCER aren't played.

## 5. Recommendation for SKYGRIND

Enemy behaviour is GG's to decide (CLAUDE.md rule 7). Every number below is a proposal with its source, and each needs
GG's yes. New gameplay numbers the web never had go in `Design/unity-config.json` (rule 5), e.g. a `design.nav.traverse`
block.

### 5.1 (a) The traversal verbs

| Verb | Who | Limits (proposed) | Timing (proposed) | Why these numbers |
|---|---|---|---|---|
| **LONG DROP** | every walker (husk, swarmer, spitter, charger) | 5-12 m down, orthogonal, landing clearance as `drop` | fall under the husk gravity (12 m: 1.0 s), then a 0.35 s landing stagger (a punish window) | L4D DeathDrop 10.2 m (non-CS builds). ARENA "can get down" 71 % -> 91 % at 12 m (97 % at 14); DRYDOCK 88 % -> 99 % at 12 m. Fixes "can't get out of floors". |
| **CLIMB** | husk, spitter: 2.2-5 m; swarmer: 2.2-10 m; charger: none | a vertical riser (`Riser()`), headroom `rise + bodyHeight` over the source, no overhang up the face, a lip node with standing room | husk: 0.3 s tell (hands on the lip), climb at 3 m/s, 0.25 s mantle (5 m: 2.2 s). Swarmer: 0.1 s tell, 6 m/s (10 m: 1.8 s). A punch / knockback on a climber drops it (fall + stagger) | L4D `ClimbUpHeight` 5.1 m; Vermintide `jump_up_max_height` 5.1 m with buckets 1 / 3 / 5 m. The swarmer tier is Dying Light's Viral / DRG's glyphids. Climb speeds have no published source: they're picks, made slower than a run so a climber is an exposed target. |
| **LEAP-UP link** (replaces the free leap) | husk | from a reached node 2-8 m out to a lip up to 6 m higher; the arc flown and swept through the collider at build | the existing nav-jump tell 0.12 s, then `LeapVelocity` (apex +1 m); at most one per husk per 3 s (`husk.leapCooldown`) | Vermintide Rat Ogre: arc checked by segmented raycasts, landing snapped within 0.5 m. Keep the free leap only when the husk has line of sight to the landing and a clear vertical ray to the peak. |
| **VAULT** | all | <= 1.2 m, already a `jump` link | a vault pose instead of the crouch | cosmetic: the link exists (`NavGridBuilder.cs:423-433`) |
| **AUTHORED link** | per link (`kinds`) | in the map file, e.g. `navLinks: [{a, b, verb: "climb" / "ladder" / "leap" / "drop", kinds, bidir}]`, both ends snapped to grid nodes within 0.5 m and validated like their verb | the verb's timing; a ladder at 2 m/s | CoD's barriers and jump-downs, Days Gone's ladders (a known choke). This is how ARENA's 9 m ledges, 24 m spires and DRYDOCK's 11 m hull deck get a route: the what-if shows no generic verb under 10 m reaches them. |

**Map rule (MAP-DESIGN.md section 1.3, proposed addition).** Every standable surface has a husk route in, unless it is
tagged a *ranged perch*: a spot designed to be answered only by spitters, drones or lancers, at most a few per map and
small. Every surface has a way down within 6 s.

### 5.2 (b) Adding them to the grid flow field

- **Link kinds.** `Link.Climb = 4`, `Link.LongDrop = 5`, `Link.Leap = 6`, `Link.Authored = 7`, plus a per-link `rise`
  so one kind serves two climb limits.
- **Per-kind masks.** Replace `Flow.Usable(kind, jumps)` (`NavField.cs:195`) with a per-kind test (allowed kinds and
  `rise <= kind.climbMax`). Fields go from two to one per distinct mask: husk, swarmer and walker (the charger), so
  three Dijkstras where today there are two. The cost of the extra field was **not measured** (ARENA: 11,228 nodes,
  80k links, 2,500 pops a step).
- **Costs** in the grid's units (m-equivalents at the kind's run speed), the same shape as today's `jump` cost
  `1 + 1.5 + dy`:
  - climb `1 + speed x (tell + rise / climbSpeed + mantle)`; husk, 5 m: `1 + 6 x 2.22` = 14.3
  - long drop `1 + speed x (sqrt(2h / g) + stagger)`, with g = 23.1 m/s2; 10 m: `1 + 6 x 1.28` = 8.7
  - leap `1 + speed x (tell + flight time)`
  - authored: its verb's cost, or a designer override

  Cross-check: Vermintide prices ledges and jumps at x1.5 of walking and teleporters at 5.
- **Build and validation**, reusing `NavGridBuilder`'s helpers (`Riser`, `ClearLine`, `HasFloor`, the column scan):
  - riser `|n.y| < wallMaxAbsNormalY` 0.3
  - source headroom `>= rise + bodyHeight` 1.9
  - rays up the face at `probeRadius` 0.45 m off the wall, no hit below the lip
  - a lip node that isn't an `edge` node, with at least two standing neighbours
  - leaps and authored leaps flown at 120 Hz with `Husk.Move`'s rules and swept through the collider, as
    `NavGraph.cs` flies RIVAL's edges, accepting a landing within 0.5 m of the target node
- **Exactness (CLAUDE.md rules 3-5).** Keep the web's grid export and the `flowField` golden vectors untouched:
  - Build the new links as a Unity-side extra layer (`Core/Nav/NavLinksExtra.cs`), appended to the CSR arrays at load
    when `design.nav.traverse.enabled`. With the flag off the grid is the web's, bit for bit.
  - The validator (`MapValidate.cs`) and `SKYGRIND/Sync Port Data` build the layer the same way, and the run snapshot
    records the flag.
- **Execution.** A `NavTraverser` beside `HuskNavJumper` (`HuskBrain.cs:312-382`) that owns the body for the verb:
  1. the tell pose
  2. a kinematic move along the face (gravity off)
  3. the mantle
  4. Hit or knockback during 1-3 means a fall plus the stagger.

  `HuskBrain.Navigate` asks for it where it asks for `AskJump` today (`:882-890`, `:906-912`). New events (`navClimb`,
  `navDrop`) are added beside the web's `navJump`, and the web's names stay as they are (rule 7).
- **Fix the comment.** `NavJumper.cs:1-2` ("until then nothing jumps").

### 5.3 (c) The route judge and the camp metric

**ROUTE JUDGE (static, headless).** Extend `MapValidate.cs` or add a `SimHarness routes` command, with the same grid
and links the game loads. For every node a player can stand on (a grid node with a zone tag >= 0; refine later with
RIVAL's graph reach), compute:
- `T_up(P)` = the field cost from the nearest ground spawn node to P / the kind's speed (husk 6 m/s), per gate state in
  price order and all open;
- `T_down(P)` = the cost from P to the nearest ground node / speed;
- the height tax `T_up(P) - T_up(F)`, with F the cheapest reached node within 10 m across and below P.

Proposed pass rules:

| Rule | Threshold | Today |
|---|---|---|
| Dead perches (no seed a ground husk reaches: no spawns, no hunters) | **0** | ARENA 576, DRYDOCK 0 |
| Every standable node reached by husks, except tagged ranged perches | `T_up <= 12 s` from the nearest spawn node | ARENA 69 %, DRYDOCK 79 % reached at all |
| Height tax | `<= 6 s` | not computed |
| Every node can get down | `T_down <= 6 s` | ARENA 71 %, DRYDOCK 88 % can get down at all |
| Ranged perches | at most 3 per map, each <= 150 nodes, each within 24 m of a reached floor (spitter range) | not tagged |

Reasons for the numbers:
- 12 s is the far edge of the spawn ring (`director.spawnMax` 40 m = 6.7 s at 6 m/s) plus one 5 m climb (2.2 s) and
  slack.
- 6 s is about one free leap's cooldown cycle twice (3 s): a perch may slow the horde, never stop it.

The report is the per-band table of section 1.4 plus the failing components with their extents, so a designer can add
an authored link.

**CAMP JUDGE (dynamic, seeded, headless).** `SimHarness`, as `autopilot` / `farm` run today. The base is the web's P1
stress test (POWER.md P1 item 8): 60 walkers at random reachable cells must reach within 3 m of a standing player
inside 25 s at 95 %, with none motionless for over 2 s while more than 5 m away. Extend it to perches:
- For each of the K = 10 worst perches (the highest `T_up` components plus every validator spot above 2.5 m), park the
  AUTOPILOT with movement off and its aim on, at rounds 5 and 10, against a fixed run seed.
- Run the same at the ground baseline: the player spawn plaza.
- Measure:
  - time to first melee contact (a husk within `husk.meleeRange` 2.2 m in 3D)
  - damage taken per minute
  - time survived
  - husks motionless more than 2 s
  - husks recycled
- Proposed pass:
  - first contact <= 20 s at round 5
  - damage per minute on any perch >= 50 % of the ground baseline (the **camp ratio**)
  - survival on the best perch <= 2x the ground baseline
  - 0 husks motionless over 5 s on screen
- A judge is a measurement, so these thresholds are starting values to tune against GG's play (the
  `feel-done-means-measured` rule from NIGHT17).

### 5.4 (d) Anti-camp that keeps perches fun but risky

1. **Climbers with a tell.** Climb speed is under run speed, the head shows at the lip for the 0.25 s mantle, and a
   punch knocks a climber off: the perch becomes "hold the lips". This is CoD's barrier and WWZ's "topple them" as a
   skill moment, not a wall. It needs no new enemy, only the verb.
2. **Height-aware camp score.** `DirectorMath.StepCamp` (`Director.cs:141-171`) is horizontal only. With the player
   on a node whose `T_up` is over 6 s, or unreached, use `lingerTime` 3 s instead of 8 and `riseTime` 3 s instead of 6.
   These are new numbers in the design config.
3. **Hunters that can always come.** When `PickNode` finds nothing (a dead perch, today's `Director.cs:603-605`), the
   HUNTERS spawn at the perch's authored climb points or nearest reached floor. They are climbing swarmers (10 m tier)
   instead of no spawn at all.
4. **An unreachable failsafe.** A walker with no route to you (field `Infinity` or no progress) is handled by
   visibility:
   - Off-screen it already relocates after 3 s (`ai.watchdog.relocateAt`).
   - On screen, after 8 s, it plays a 0.6 s retreat tell and goes back into the round's queue with its health kept,
     as BO3's `round_spawn_failsafe` (30 s / 24 in) and `zm_giant` cleanup (stored health) do.

   8 s sits between KF2's 5 s and L4D's 20 s Tank frustration.
5. **Ranged pressure that scales with height.** Ported as the web designed them (rule 7: GG's designs, rebuilt as they
   are):
   - the DRONE, which hovers 4-8 m with orbs from 12-18 m (`Drone.cs` is a stub)
   - the LANCER, a perch sniper at 70 m whose laser follows twice as fast while you're hunted (not ported)

   SURVIVAL.md section 6 already names them as the 100 % camp response. The Spitter's "aim at your spot when hunted"
   is ported.
6. **Perches that pay and cost.** Keep the relays and perks on perches (MAP-DESIGN's verb-gated spots) as reasons to go
   up. Height lowers melee pressure, while lifting spitter and drone pressure and the camp score. Optional: the
   GRINDER's core reshape (`zones.grinderReshapeEvery` 3) as ULTRAKILL's Cyber Grind, where the high ground moves
   between waves.

**Order of work (proposed).**
1. LONG DROP and the route judge first: cheap, and they fix "can't get out" (ARENA "can get down" 71 % -> 91 %).
2. The CLIMB verb plus the climber tell.
3. Authored links for ARENA's 9 m ledges and spires and DRYDOCK's hull deck and ribs, until the judge passes.
4. The height-aware camp score and the hunters fallback.
5. The DRONE and LANCER ports, if GG wants the web's answers to height.

## 6. Not checked

- Anything in Play: how often the free leap lands, how husks behave at a wall's foot, frame cost of a third field.
- Whether a player can stand on DRYDOCK's 14 m wall tops (tag -1) or ARENA's unreached spots that the validator
  calls player-only by graph only.
- The what-if links skip the real `Riser` / `ClearLine` / capsule checks; real counts will be somewhat lower.
- BO4 / Cold War anti-camp systems; DOOM's traversal-link authoring; DRG's wall-walking nav; the Days Gone GDC talk.
- The `com.unity.ai.navigation` package licence (irrelevant: not allowed for gameplay).

The scripts used for section 1.4-1.5 (`navreach.cjs`, `navcomps.cjs`) were in the session scratchpad and are not in
the repo.
