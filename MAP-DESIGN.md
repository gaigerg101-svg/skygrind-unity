# MAP-DESIGN.md: SKYGRIND's new SURVIVAL map (NIGHT15, map night 1)

Written 2026-10-05 from 11:40 (times from `date`) for GG's request: "a map overhaul ... built to maximize movement and
replayability ... feel like CoD zombies and ULTRAKILL with personality, not a bland map". It sizes the map from
SKYGRIND's movement numbers (section 1), adds 13 minutes of layout research to `MAP-RESEARCH.md` (section 2), offers three
concepts (section 3), recommends one with its plan (section 4), checks the pitfalls (section 5) and lists what is GG's
to decide (section 6). **The map's identity is GG's decision (D-59); this document recommends, it doesn't decide.**

- **The plan's numbers live in one file:** `Design/maps/drydock.plan.json`. The plan picture (`Tools/map-plan-svg.cjs`
  -> `Logs/shots/n15-plan.png`) and the greybox build script (`Assets/Editor/Maps/Build_drydock.cs`) both read it.
- **Measured** means the web's own Mover output: `PortExport/trajectories/*.json`, measured by
  `scratchpad/traj-measure.cjs` (take-off to landing). **Formula** means derived from config keys, not run through the
  Mover. Every config key is in `PortExport/config.json`.

## 1. The distances the map must fit

### 1.1 The player

| Verb | Number | Source |
|---|---|---|
| Run | 8 m/s; no sprint (walk speed is the ground cap); reaches it in ~0.5 s | `ground.walkSpeed` 8, `ground.accel` 10; walk.json peak 8.00 m/s |
| Speed cap | 40 m/s horizontal | `speed.maxHorizontal` |
| Jump | 1.5 m apex in 0.36 s; a tap: 0.36 m | `jump.height`, `jump.apexTime`, `jump.releaseMult` 0.5; short_hop.json rise 0.36 m |
| Run + jump + double jump | **10.5 m** across, **2.19 m** up, 1.31 s in the air | `jump.doubleJumps` 1, `jump.doubleJumpMult` 0.9; run_jump_double.json (measured) |
| Bunny hop | 7-8 m a hop at 11-12.5 m/s take-off, peak 15.6 m/s | bunny_hop.json (measured) |
| Dash | 24 m/s for 0.25 s = 6 m; 3 pips = **18 m in 0.75 s**, then 1 pip per 1.43 s | `dash.speed`, `dash.duration`, `dash.maxPips`, `dash.regenPerSec` 0.7 (formula); dash.json peak 24.00 |
| Dash-jump | **21.2 m** across, 1.85 m up, 0.88 s | `dashJump.v0Mult` 0.8, `dashJump.gravityMult` 0.5; dash_jump.json (measured) |
| Slide | 11.6 m/s on the flat; down a 20 deg ramp **17.35 m/s** | `slide.floorSpeed`, `slide.friction` 1.5, `slide.slopeBoost` 1.6; slide.json, ramp_slide_down.json (measured) |
| Slide-jump | **8.9 m** across, 1.41 m up, take-off 13.9 m/s | `slide.jumpHorizontalMult` 1.2; slide_jump.json (measured) |
| Slam + boost jump | **4.74 m** up, 13.9 m across; the slam's shockwave 5 m round | `slam.speed` 48, `slam.shockwaveRadius`, `boostJump.v0Mult` 1.8, `boostJump.window` 0.15; slam_boost.json (measured) |
| Wall-ride | needs 8 m/s, lasts 1.2 s; the recorded ride **climbed to 7.6 m** and carried 17.1 m | `wallRide.minSpeed`, `wallRide.maxTime`, `wallRide.gravityMult` 0.15; wall_ride.json (measured) |
| Wall-jump | **12.2 m** across, 3.5 m up; 3 per airtime | `wallJump.outSpeed` 8, `wallJump.maxPerAirtime`; wall_jump.json (measured) |
| Jump pad | plain: **8 m** apex; aimed: lands on its target; 1 m radius | `jumpPad.apexHeight`, `jumpPad.radius`; trial_slamDeck.json rise 8.00 m (measured) |
| ARCLINE | hooks at up to 60 m; rope 4-55 m; swing cap 42 m/s; zip 34 m/s; a swing over a gap with the anchor 15 m up carried **59.2 m**, peak 23.3 m/s; costs 5,500 | `arcline.range`, `minLen`, `maxLen`, `softCap`, `zip.reelSpeed`, `price`; swing.json (measured) |
| Body | capsule 0.35 m x 1.8 m, eye 1.6 m | `body.capsuleRadius`, `capsuleHeight`, `eyeHeight` |

### 1.2 The enemies (their speeds and reach; the designs are GG's)

| Enemy | Number | Source |
|---|---|---|
| Husk | 6 m/s; leaps 1.5 m up to 10 m across; melee 2.2 m | `husk.speed`, `husk.leapMinHeight`, `husk.leapMaxHorizontal`, `husk.meleeRange` |
| Swarmer | 10 m/s (faster than a run); pounces 4.5 m | `swarmer.speed`, `swarmer.pounceRange` |
| Spitter | 4.5 m/s; fires at 6-24 m | `spitter.speed`, `minRange`, `maxRange` |
| Charger | 4.5 m/s; charges at 22 m/s from 6-26 m | `charger.speed`, `chargeSpeed`, `minRange`, `maxRange` |
| Lancer | hits at up to 70 m | `lancer.range` |
| Walkers' nav | step 0.6 m; climb **2.2 m**; drop 5 m; a gap of **4 cells (4 m)** at most 1 m up; 1.9 m headroom; nothing walkable is scanned above **39.5 m** | `ai.grid.step`, `jumpUp`, `drop`, `gapCells`, `gapUp`, `minClear`, `scanTop` |

### 1.3 What that means for the map (the design rules used in section 4)

| Rule | Number | From |
|---|---|---|
| A ledge everyone climbs | <= 2.0 m | under the walkers' 2.2 m and the standing double jump's 2.19 m |
| A ledge that needs a verb (walkers can't follow) | 2.5 m | over `ai.grid.jumpUp` 2.2 and the standing double jump 2.19; reached by a slide-jump off a kicker, a boost jump (4.74), a wall-ride or a pad |
| A gap walkers can't cross | >= 5 m | `ai.grid.gapCells` 4 |
| Gap reach per verb | run + double jump 10.5 m; dash-jump 21.2 m; ARCLINE up to ~59 m | section 1.1 |
| No ambiguous gaps | never 0.8-1.1 of the verb's reach (a gap you make "sometimes") | the web's MAP-AUDIT.md (F7: "ambiguous 0.8-1.1"); NIGHT5 fixed the LANDING's 4.5-5.25 m hops for it |
| A rideable wall | >= 14 m long, >= 8 m tall; a canyon 7 m wide | a 1.2 s ride at 8-12 m/s covers 9.6-14.4 m; the ride climbs 7.6 m; a wall-jump (12.2 m across) reaches the facing wall in its rising half |
| A training loop | >= 150 m a lap; lanes >= 12 m wide | a husk (6 m/s) falls 2 m/s behind a run, so a 172 m lap laps the horde every 86 s; a swarmer's 4.5 m pounce plus the 0.7 m body on both sides |
| Cover against spitters | a break in every line longer than 24 m in a training zone | `spitter.maxRange` 24; lancers (70 m) own the long open axes on purpose |
| Plain pads | land on decks <= 7.5 m; anything higher uses an aimed pad | `jumpPad.apexHeight` 8 |
| Crossing the map | 128 m edge to edge = 16 s at a run, ~8 s with dashes and slides (the old arena: 96 m, 12 s) | `ground.walkSpeed`, section 1.1 |

## 2. Research (13 minutes of web time, 11:40-11:53, on top of MAP-RESEARCH.md)

UNVERIFIED marks a community source or a search-result summary I didn't open; the CoD fandom wikis returned 402 to the
first research agents.

- **Kino der Toten:** "eight regular rooms, a pack a punch room, nine mystery box spawn locations"; the lobby (start) is
  "a mid size room with 4 zombie windows" and "2 openable doors ... both costing $750"; **two routes to the stage**, "one
  through the alley and electrical room on the left ... the other ... through the bar and dressing rooms on the right"; the
  Theater is "the biggest room" (UNVERIFIED: https://codsuper.fandom.com/wiki/Kino_Der_Toten ,
  https://codzombified.blogspot.com/p/kino-der-toten.html). The teleporter is linked at the lobby's mainframe, sends you
  to Pack-a-Punch for 30 s, then back; the PaP room's window "overlooks the theater" (UNVERIFIED:
  https://nazizombies.fandom.com/wiki/Mainframe). **Taken:** a mid-size start with two cheap doors; a loop of two routes
  to the big room; the upgrade spot overlooking the training room (THE FORGE on the hull deck above THE RING).
- **Der Riese:** three teleporters each linked by going there and running back to the mainframe within 30 s (UNVERIFIED:
  https://callofduty.fandom.com/wiki/Der_Riese , https://callofdutymaps.com/zombies/der-riese/). **Taken:** a quest that
  sends you out to every zone and back through the hub teaches the geography.
- **Mob of the Dead:** the cell block "connects most of the map together"; the docks are reached two ways (a gondola, a
  tunnel); the bridge (PaP) only after building the plane (UNVERIFIED:
  https://steamcommunity.com/sharedfiles/filedetails/?id=748241987 , https://callofduty.fandom.com/wiki/Mob_of_the_Dead).
  **Taken:** a hub that every zone touches, two ways into each side zone, and a vehicle you build as the run's goal (the
  ship).
- **Origins:** the excavation site in the centre with PaP on top; six generators power their areas; three giant robots
  walk lanes (UNVERIFIED: https://callofduty.fandom.com/wiki/Origins). **Taken:** the landmark in the centre with the
  upgrade on top of it.
- **ULTRAKILL 7-1:** a marble labyrinth hub; arenas reached and re-reached by backtracking with skulls; "a Large Box Arena
  [with] three floors of platforms" and "a central hookpoint for aerial combat"; "wall-jump up a wall to reach the higher
  floor" as a gate (https://ultrakill.wiki.gg/wiki/7-1:_Garden_of_Forking_Paths, read through WebFetch's summary).
  **ULTRAKILL 4-2:** "a large stage connecting to two sets of staircases" up to a temple; the skull arenas are a greenhouse
  and "an outdoor arena ... tight-knit stairways and platforms" (UNVERIFIED summary of
  https://ultrakill.wiki.gg/wiki/4-2:_God_Damn_the_Sun). **Taken:** a movement verb as the key to a place (KEEL TOP needs
  the wall-ride; LAUNCH RAIL needs the slide-jump); the hook point over the hub (the rib crossbars' grapple nodes).
- **DOOM Eternal:** "In combat arenas, the Slayer can make use of swing bars, portals, and jump pads to reach distant
  areas"; arenas built for "push-forward combat: vertical space for double jumps and aerial movement, multiple
  ground-level routes to avoid cornering" (UNVERIFIED search summaries; GameSpot's page returned 403:
  https://www.gamedeveloper.com/design/the-dark-souls-design-of-doom-eternal ,
  https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/). **Taken:** every
  zone has at least two ground routes and one vertical one.
- **Not found:** any published size of these maps in metres (only "mid-size" for Kino and Der Riese; TranZit "large",
  UNVERIFIED: https://www.keengamer.com/articles/features/others/classic-zombies-is-back/). Section 1's numbers size the
  map instead.

## 3. Three concepts

Each is buildable from tonight's primitives (boxes, wedges, pads, grapple nodes, the core grid: NIGHT15 rule 2) and the
web's sim, enemies and run systems unchanged.

### 3.1 DRYDOCK: "Finish the ship, or fight on its bones." (recommended)

- **Place and tone:** a sky-shipyard cut into a cliff at dusk. The last airship out is half-built in its cradle; the yard
  went quiet mid-shift. Blue-collar dread with one way out: the hull is the hope, the horde is the night shift. Mob of the
  Dead's escape plan in a Kino-sized yard, with ULTRAKILL's verbs as the keys.
- **The landmark:** THE MAST, 64 m (a 2 m box from the hull deck), with the hull's 24 m ribs around it, in the middle of
  the hub. You see it from the spawn (straight through the first door) and over every wall (walls are 8-14 m).
- **The zones (8):** THE GANGWAY (start) -> THE CRADLE (the hub, 750) -> THE RIBS (west, 1000) and THE SLIPWAY (east,
  1000) -> THE BOILERWORKS (north, 1250: the power) -> THE DRAFTING OFFICE (north-west, 1500: the armory wall), THE PIER
  (south-east, 1500: the exfil pier), THE GANTRY (north-east, 2000: the ARCLINE). The cost curve is the old map's (750,
  1000, 1250, 1500, 2000: `zones.costs`). Every side zone touches the hub through two doors (Mob's cell block).
- **Verb identity:**

| Zone | Favoured | Restricted (shown by shape) | Wall gun | Threat style (existing enemies) |
|---|---|---|---|---|
| GANGWAY | run, jump | none | THUNDERCLAP (`shotgun`, 750: the cheap first buy) | four spawn points around a small deck |
| CRADLE | slide, dash, pads | none (the hub) | COMET (`rocket`, 2000) in the keel corridor: splash for THE RING's horde | everything: the horde on THE RING |
| RIBS | wall-ride, wall-jump, bunny hop on the rib tops | ARCLINE (no anchors), pads (none) | STITCHER (`nailgun`, 1250) | swarmers from the rib tops (high spawns), close |
| SLIPWAY | slide, slide-jump, dash-jump | wall-ride (no long walls in the lanes) | LONGWAVE (`rail`, 3000): the 40 m lanes | chargers down the 40 m lanes, spitters on the deck |
| BOILERWORKS | slam, slide, punch | pads, ARCLINE, high jumps (a 4.5 m ceiling, painted with hazard stripes) | TORCH (`torch`) once ported | husk crowds in the aisles |
| DRAFTING OFFICE | run, punch / parry | dash lines (6 m rooms) | the web's ARMORY wall of every gun | a choke: few, at the door |
| GANTRY | ARCLINE, aimed pads, dash-jump | the ground (12 m below the platforms) | ZAPPER / VORTEX once ported | lancers and spitters across the gaps |
| PIER | slide, dash | none | none (the exfil) | the exfil's waves |

The port fires six guns tonight (`Loadout.Fires`, `Core/Combat.cs:719`: revolver, gauntlet, THUNDERCLAP, STITCHER, COMET,
LONGWAVE), so tonight's four built zones hang the four buyable ones; a gun the port doesn't fire yet hangs nothing
(`SurvivalSim.AddWallBuys`). Night 2's zones wait for the POWER guns (`QUEUE-power-guns.md`) or share the wall in the
drafting office. The perks likewise: only `Perks.Ported` stand (`Core/Perks.cs:128`), so the gangway's is MEND.

- **Training spots:** THE RING (172 m around the hull), THE SLIP (slide down 40 m, pad back up, ~110 m), THE ALLEYS
  (the rib canyon's outer loop, 192 m, with a cross alley). **Choke points:** the gangway door (8 m), the boilerworks'
  aisles, the drafting office's door.
- **Progression:** the power is in the boilerworks (three doors deep: 750 + 1000 + 1250 = 3,000, Kino's "4 doors,
  about 4,250" scaled to a 6-zone run); THE FORGE on the hull deck from round 1 (the web's rule), above THE RING (Kino's
  PaP window over the theater); the PROTOCOL's four relays in any order, each reached by its zone's verb: KEEL TOP (ribs:
  wall-ride the 12 m rib), LAUNCH RAIL (slipway: slide down lane A, slide-jump off the kicker onto a 2.5 m rail), THE
  FIREBOX (boilerworks: slide under a 1.5 m grate into the open furnace yard), THE HOOK (gantry: swing to the crane's tip);
  the secret relay is THE CROW'S NEST on the mast (30 m, a hidden aimed pad at the bow). The story: the relays are the
  launch sequence; finishing the PROTOCOL "launches" the ship (a proposal for GG: the exfil on the deck, section 6).
- **One visible expert shortcut per zone:** the gangway's door lines up with the south pad (straight onto the hull deck);
  THE RING's keel corridor cuts it in two; the rib tops are a high road, and from the 8 m rib a dash-jump (21.2 m) clears
  the 8 m wall into the ring; the slipway deck's west lip to the hull deck is an 18 m dash-jump; the boilerworks' grate is
  a slide-under; the gantry's first hook swings past the second platform.
- **Per-run variation (seeded from the run seed, fixed geometry):** the box's first spot (4 tonight, 6 on night 2), which
  HOLD spots the round's objectives use (4 tonight, 6 on night 2), the GOLD RUSH route (which zones its drops run
  through), and which zone the PROTOCOL wakes first. Proposals for GG in section 6 (they change run rules).
- **The old arena returns as** an ENDLESS map: the arena as it is, its 12 x 12 core re-shaped every wave from the web's
  presets (`core.presets`: flat, stairs, ring, towers, random) like the Cyber Grind (MAP-RESEARCH section 2). Tonight it
  is simply the MAP row's "ARENA".

### 3.2 TETHER: "Something came down the cable. Climb up and cut it."

- **Place and tone:** the ground anchor of a broken space elevator; the cable rises out of sight from the centre, its
  climber car stuck halfway with a red light. Cold, vertical, cosmic; ULTRAKILL's Layer 7 labyrinth stood on end.
- **The landmark:** the tether: a 2 m column from the hub to the top of the sky (all heights), with the car at 60 m.
- **The zones (7):** the map is stacked tiers around the anchor: THE APRON (start, ground), THE SPOOL YARD (hub ring,
  ground: training), THE TERRACES (wall-ride ledges, 6-12 m), THE SPOKES (four 30 m beams out from the anchor at 18 m:
  ARCLINE between them), THE VAULT (under the anchor: close quarters), THE COUNTERWEIGHTS (floating blocks at 20-30 m:
  pads), THE CROWN (the anchor's top at 34 m: THE FORGE). Costs 750 -> 2000 rising with height.
- **Verb identity:** ground ring = slide; terraces = wall-ride; spokes = ARCLINE; vault = slam / punch (low roof);
  counterweights = pads and dash-jumps; crown = all of them.
- **Strength:** the purest "make me move" map; the quest is literally a climb (each relay higher than the last).
- **Weakness:** stacked tiers are hard to read (where is the horde?), the walkers' nav loses the high tiers (they climb
  2.2 m: everything above the ring is high spawns only), the scan stops at 39.5 m, and falls are long walks back. Risky for
  CoD-style training, which needs ground loops.

### 3.3 THE DIAL: "The sun won't set until you hold every hour."

- **Place and tone:** a colossal sundial on a mesa at the end of the day; the shadow of its gnomon sweeps the zones as the
  rounds pass. Ritual, ancient, golden; Shadows of Evil's district rituals on one clock face.
- **The landmark:** the gnomon, a 70 m blade; its shadow points at the zone the PROTOCOL wakes.
- **The zones (8):** the dial face in the centre (the hub: the old arena's 12 x 12 core grid, re-shaped every round like
  the Cyber Grind), ringed by eight "hour" wedges, each a verb: slide terraces, wall-ride colonnades, pad towers, a grapple
  orrery, a sunken close-quarters crypt, an armory, the start, the exfil stair.
- **Strength:** the core grid (already ported) becomes the hub's living floor; per-run variation is built in.
- **Weakness:** radial symmetry is the old arena's problem again (GG: "bland", "symmetric"); eight wedges of the same
  shape read alike however different their insides are; axis-aligned boxes make a poor circle.

### 3.4 Why DRYDOCK (D-59: the recommendation; GG's call wins)

| | DRYDOCK | TETHER | THE DIAL |
|---|---|---|---|
| A place you can name and call out | the ship, the mast, the ribs, the slipway | the tether, the car | the gnomon |
| Ground training loops (CoD's base) | three (THE RING, THE SLIP, THE ALLEYS) | one (the ring) | the dial face |
| Verbs as keys (ULTRAKILL) | each zone's geometry is its verb (ribs = wall-ride, slipway = slide) | strongest | per wedge |
| Readable at a glance | a hub with spokes, asymmetric | stacked tiers: hard | symmetric: reads like the old map |
| Fits the nav and the primitives | yes (walkable 0-12 m, the mast is decoration) | the tiers above 2.2 m are walker-dead | yes |
| A run goal with a story | the launch sequence (build the ship: Mob of the Dead) | cut the cable | hold every hour |

DRYDOCK wins on readability and on the CoD loop (three training loops on the ground) while keeping ULTRAKILL's verb-keys
in every side zone. TETHER is the stronger movement statement and could return as a challenge map; THE DIAL is the best
home for the old arena's core (which is why section 3.1 sends the old arena to an ENDLESS mode instead).

## 4. DRYDOCK's plan

![DRYDOCK plan](Logs/shots/n15-plan.png)

`Logs/shots/n15-plan.png` (1396 x 1076, rendered 11:53 by headless Chrome from `Logs/shots/n15-plan.svg`, which
`node Tools/map-plan-svg.cjs Design/maps/drydock.plan.json Logs/shots/n15-plan.svg` writes). Looked at 11:54:
- **What a first-time reader sees in one second:** a hub ring around a central hull and mast, with three neighbours that
  plainly differ: a canyon of ribs to the west, two long ramps to the east, a small start deck to the south. It reads as
  a hub with spokes, not a grid.
- **What is still wrong:** the north row is three equal 40 x 28 m boxes (the grid the old arena was criticised for), the
  hub is mirror-symmetric east-west (the hull, the pads, the doors), and the labels overlap at the mast. Night 2 breaks
  the symmetry (the slipway deck reaching into the hub's north-east corner, a crane over the ring) and gives the north
  row three different shapes.

**Tonight's greybox (NIGHT15 U3-28.6)** builds the gangway, the cradle, the ribs and the slipway: the heart. The four
other zones are hatched on the plan; tonight they have no floor (no nav nodes), behind 14 m walls with a barrier above.
Two of the four relays sit on the hull deck tonight (THE BRIDGE, THE STERN: placeholders for THE FIREBOX and THE HOOK);
the power is in the ribs (two doors deep) and the ARCLINE on the slipway deck until those zones exist.

Key measurements (all in the plan file):
- THE RING: 31 x 55 m on its centre line, 172 m a lap; lanes 13 m (west and east of the hull) and 14-16 m (the ends).
- The hull: 14 x 40 m, 10 m tall, a 6 m keel corridor through it with four rib doors a side (4 m wide), the deck on top
  at 11 m reached by aimed pads at both ends; the rib posts (2 x 1 m) are cover in the lanes.
- THE RIBS: 8 slabs, 2 m thick, 27-29 m long, 8-12 m tall, 7 m apart, a 6 m cross alley. An alley has rideable wall on
  both sides: 40 m of wall per 20 m of route (the web audit measured 20.2 per 20 on the old SLAB GARDENS ring, against
  its target of 8).
- THE SLIPWAY: a 38 x 12 m deck at 10 m; two 40 m ramps at 14 deg (rise 10); a 4 m kicker (rise 1.5) at lane A's foot;
  LAUNCH RAIL's platform at 2.5 m, 5 m past the kicker's lip (a slide-jump carries 8.9 m and rises 1.41 m above the lip at
  1.5 m: 2.91 m, over the 2.5 m rail; walkers climb 2.2 m and can't).
- Gates: the gangway door 8 m (750); two 6 m doors to each side zone (1000).
- Spawns: 22 ground, 7 high over the four zones (the old map: 28 / 10 over six).

## 5. Pitfalls (MAP-RESEARCH section 3), checked

| Pitfall | DRYDOCK | Check |
|---|---|---|
| TranZit: too big, slow travel | 128 m across = 16 s at a run, ~8 s with dashes and slides; the hub touches every zone | ok |
| Zetsubou: setup cost before power and PaP | power three doors in (3,000 points: about round 3-4 at the old map's pace, not measured on this map); THE FORGE open from round 1 (the web's rule) | ok |
| Buried: one training path | three loops in three zones, and THE RING is in the hub | ok |
| Restriction everywhere | one zone restricts hard (the boilerworks' low roof), two restrict one verb each (no anchors in the ribs, no long walls in the slipway lanes); the hub and the gangway restrict nothing | ok |
| Overload in one room | THE FIREBOX (a relay: an objective with an elite surge, `protocol.surge`) sits in the boilerworks' open furnace yard, not under its low roof; HOLD rings never spawn in the boilerworks | ok, by placement (night 2) |
| Ambiguous gaps (the web's audit) | gaps are sized to well under a verb's reach (the rib alleys 7 m vs a 12.2 m wall-jump; the slipway lip 18 m vs a 21.2 m dash-jump: 0.85, inside the ambiguous band) | **one finding:** the slipway lip shortcut at 0.85 of a dash-jump; night 2 moves the deck 2 m nearer (16 m, 0.75) |

## 6. Proposals (GG's: gameplay numbers, enemy design, round order, run rules)

None of these is built tonight; each is a decision for GG.

1. **D-59, the map's identity:** DRYDOCK as section 3.1 (or TETHER / THE DIAL, or a mix).
2. **The zone verbs** (section 3.1's table): are the restrictions right (no anchors in the ribs, the boilerworks' 4.5 m
   roof)? The wall guns per zone need per-gun wall buys at map spots (`spots.wallBuys`), a Unity-side rule beside the
   web's single ARMORY wall.
3. **The gate costs:** the old map's 750 / 1000 / 1250 / 1500 / 2000 kept, keyed by the new zone ids
   (`data.zones[i].cost` in the map file; `config.json zones.costs` stays the web's for the old map).
4. **THE FORGE's access:** the web opens it from round 1. Proposal: the PROTOCOL's four relays open the hull deck's
   FORGE (Kino's link-the-teleporter, Der Riese's three links), with the deck's pads always open.
5. **The launch:** finishing the PROTOCOL turns the hull deck into the exfil site for the next exfil window (the ship
   leaves; Mob of the Dead's plane). An exfil rule change.
6. **Per-run variation:** the box's first spot, the HOLD spots, the GOLD RUSH route and the PROTOCOL's first zone drawn
   from the run seed (Mulberry32 per run, CLAUDE.md rule 6).
7. **The old arena** as an ENDLESS map whose core re-shapes every wave (the Cyber Grind), or kept as "ARENA" in the MAP
   row as tonight.
8. **Enemy mix per zone** (the threat styles in section 3.1 lean on the existing enemies' ranges; any per-zone spawn
   weighting is enemy design: GG's).

## 7. As built (NIGHT15 U3-28.6, 2026-10-05 13:00-13:27)

**What exists:** `Design/maps/drydock.json` (and its byte-identical Player copy), generated from the plan two ways that
write the same bytes: `dotnet run --project Tools/SimHarness -c Release -- mapgen Design/maps/drydock.plan.json
Design/maps/drydock.json` and, in the Editor, `SKYGRIND/Map/Build DRYDOCK` (`Skygrind.EditorTools.BuildDrydock.Build()`,
which also writes the authoring scene `Assets/Scenes/Maps/drydock-authoring.unity`, 133 markers, for GG to move a
platform and re-export). Four zones (THE GANGWAY start, THE CRADLE 750, THE RIBS 1000, THE SLIPWAY 1000); the rest of the
plan is unbuilt (no floor, walled off). TITLE's MAP row offers ARENA | DRYDOCK.

**Measured:**
- Validator (`Reports/MAP-drydock-validate.md`): `**findings: 0** (53 checks, 1 VERB)`; every zone reached in price order
  (the cradle's 129 graph nodes all reached, the slipway's 153 of 157); the farthest spot (perk QUICK on the deck) 12.9 s
  from the spawn, THE FORGE 7.5 s; the one VERB row is KEEL TOP (wall-ride: RIVAL's graph has no wall-ride).
- The AUTOPILOT (the smoke's step 24, builds 2 and 3, and `SimHarness newmap`): `round 6 / 6, cleared 5, 99 kills ...
  60 of 60 hashes equal` in 300 s, no stall (with the design's unported guns it reached round 9: it spends more now that
  every wall gun is real).
- The zones' moves on the real Mover (`SimHarness verbs`, `Logs/n15-verbs.txt`):

| Move | Result |
|---|---|
| THE SLIPWAY: slide down lane A from the deck | peak **28.8 m/s**, 2.4 s of slide (ramp_slide_down.json's 20 deg, 7 m ramp: 17.35) |
| LAUNCH RAIL: slide-jump off the 2.0 m kicker | **3 of 6** take-off points (z 24.0-24.8, the lip at 25) land on the 3.0 m rail |
| THE RIBS: wall-ride along a rib (wall_ride.json's input) | **1.20 s** (the full `wallRide.maxTime`), climbing to **7.68 m** (wall_ride.json: 7.68) |
| The four pads | each lands within **0.07 m** of its target (the crow's nest 30 m up: 0.01 m) |
| The deck-lip shortcut: dash + jump from the slipway deck to the hull deck | **3 of 4** dash timings land on it; dashing early (x 27) overshoots 37 m into the ring's west lane |

**What reads well (captures `Logs/shots/n15-b2/`, `n15-b3/`):**
- From above (`n15-drydock-above.png`) the built heart is the plan: the rib canyon, the hull and its arches in the ring,
  the deck and its two ramps, the gangway. A hub with spokes, not a grid.
- The hub from inside THE RING (`n15-b3/n15-drydock-hub.png`): the 24 m rib arches and the mast behind them; the lanes
  are wide enough to run a horde.
- The landmark from the slipway's far corner, 66 m away (`n15-b3/n15-drydock-landmark.png`): the mast stands clear
  above the arches.
- The rib alley (`n15-drydock-ribs.png`) reads as a wall-ride place: two tall walls 7 m apart, cyan rims on top.
- TITLE (`n15-b2/n15-title.png`): MAP ARENA | DRYDOCK on the run setup's line; the new map behind the menu.

**What is wrong:**
- **The mast hides inside the rib alleys.** From the ribs' corner a slab blocks it entirely (a 10 m slab 3.5 m away covers
  up to 70 deg, the mast sits at ~42 deg): the landmark is not "visible from everywhere" (build 2's capture).
- **One material for everything:** hull, ribs, walls and decks share the stone; the slipway's ramp barely reads as a
  slope (`n15-b3/n15-drydock-slipway.png`), nothing marks a rideable rib or a ride-less zone (MAP-RESEARCH's "verb-killing
  materials marked visibly" isn't done).
- **The mast is a plain 2 m post:** no crow's nest you can read, no beacon; the HUD's round counter overlaps it.
- **The hub is mirror-symmetric** and the gangway door frames the locked gate's huge "THE CRADLE 750" sign, not the
  mast (build 2's capture from the door).
- **LAUNCH RAIL has a second route:** RIVAL's graph reaches it by a dash-jump plus a double jump (~3.1 m), so it isn't
  slide-jump-only; kept as an expert alternative.
- The validator's player graph doesn't walk ramps (RIVAL's graph has no ramp links), so the bot reaches the deck by the
  return pad; the ramps are the player's.
- Running from the cross alley into a rib alley clips the slab's end at z -3 (8 -> 4.6 m/s): the alleys' mouths want a
  chamfer or a gap so the entry to a ride is clean.

**What night 2 changes (NIGHT16.md):** the four hatched zones with shapes of their own (the north row must not be three
equal boxes), the real FIREBOX and HOOK relays, every buyable, the hub's symmetry broken (the deck reaching into the
ring's north-east corner, a crane over the ring), a mast you can see from the ribs (taller arches cut low over the cross
alley, or a beacon above the rib tops' sightline), then the look night: materials that mark the verbs.
