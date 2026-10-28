# Level design notes: LAYER 1, THE KILN (NIGHT24, builder L)

What the first campaign layer taught. GG asked to "study how it is built, balance it, what is shown". Everything below was measured on the headless
bot (`campaign`, RIVAL-style autopilot "normal", the real sim) or read from the report unless it says "not checked". Sources: `Design/levels/layer1.plan.json`,
`Reports/LEVEL-layer1-validate.md`, `Design/levels/LEVEL-FORMAT.md`.

## How it is built

One plan file, one command (`levelgen`), one report. Rooms and doors become zones (a door is a thin zone of its own: locked it is a solid force field,
open it is free), the room's wall ring is generated with a gap at each door, and `DrydockPlan.Build` does the rest (the same code as the DRYDOCK map,
so the nav grid, the zone parts and the plan checks are shared). Core rules are `Core/Campaign24.cs`; the sim glue is `SurvivalSim.Campaign24.cs`.
A campaign run is one endless round (the `Campaign24Spawner` swallows the Director's calls); a room is a wave script spawned at the room's own nodes
from `Mulberry32(seed, room)`, so a retry replays the same spawns. The door behind locks when the player's feet enter the next room's zone; the clear
opens the exit door and the next zone free. A death restarts the room after `retryDelay` 1.2 s of game time (health.downTime is 1.5 s).

## Sizes

| area | kind | size (m) | tiers | par (s) |
|---|---|---|---|---|
| r1 THE LOADING BAY | room | 52 x 48 | 2 (floor, mezzanine 4 m by a 12 m ramp) | 48 |
| r2 THE CASTING FLOOR | room, NO DASH, awards the THUNDERCLAP | 52 x 48 | 3 (floor, platform 3 m, catwalk 6 m; two ramps) | 44 |
| r3 THE SLAG HALL | room, PISTOL ONLY | 56 x 56 | 2 (floor, shelf 5 m by a 14 m ramp), a foundry wall with the slide tunnel | 47 |
| ka THE VENT HALL | link | 44 x 28 | pit (platforming), vent wall 7 m, landing deck 6 m | 7 |
| kb THE FLUE | link | 24 x 24 | corridor between two walls, exit deck 3.4 m | 8 |
| boss THE FURNACE | boss | 60 x 58 (the fight is in the worm's own room) | - | 110 |

The map JSON is 2.6 MB at `floorSize` 256. The size follows the walkable area (the nav grid is the bulk): the first cut with one floor slab over the
bounding box was 6.2 MB; one slab per room and per door halves it. A 6-area layer fits 256 m; the pre-snake line would not have.

## Par times and the bot

Par = the MEDIAN of the bot's clear times over 10 seeds (x 1.0), recomputed after the layout changed: r1 48.5, r2 44.2, r3 46.6, vent hall 6.6, flue 8.0 s.
The whole layer, seed 1, with the boss: 258.0 s (4.3 min). The boss is not played by this bot: the worm's room is off the nav grid, so builder B's
WyrmBot plays the fight on its own rig (152.1 s, 1 bot death, seed 1) and the composed sim's worm is brought down by the autopilot with its health
refilled each step; `Campaign24` sees it die and ends the layer. The player's first Player reel (n24-3) cleared rooms 1-5 in 47.9 / 45.2 / 49.7 / 7.5 / 8.1 s.

## Balance numbers

Enemy counts per wave (husk + swarmer + spitter): r1 4+2, 4+4+1, 6+4+2; r2 6+2, 5+3+1, 6 swarmers + 5 + 1; r3 7+4, 6+2 spitters, 6+5.

The first counts (3 husks, then 3+2 swarmers) gave a 100 % first-try clear in all three rooms: the bot cleans husks. What kills the bot is spitters,
swarmers in numbers and the damage it carries from room to room (health is not refilled at a door). Over 10 seeds the first-try clear rates were
100 / 100 / 100 % at the start and 90 / 80 / 90 % at the end (90 / 75 / 80 % over 20 seeds; target 60-90 %). Ten seeds are noisy (about +-15 %:
changing room 2's counts moved room 3's rate from 40 % to 90 %), so tune on 20 and re-check after every layout change. The LIMIT rooms are the hard
ones for a human (the bot ignores them mostly: it keeps shooting the revolver).

## What the report checks (and what it found)

Spots on the grid; the player's route (RIVAL's graph) with each leg's time; doors close the nav; **every tier reachable by enemies from the floor**
(walkers: walk and drop links only); pit recovery; wall line against stepped route; slide tunnel by the real Mover. It found, before any play: two
encounter nodes standing inside ramp footprints (no grid node), a boss spawn inside a ramp, and the tier rule failing when a ramp is removed (a negative
test: removing room 1's ramp fails the report). The pit check fails when the recovery stair is removed (76 of 76 cells stuck).

## The three traversal kits

- **Vent hall (platforming):** a 12 m pit across the whole hall, two hop platforms with jump pads (a pad's landing is 2.3 m from the next pad so it does
  not re-trigger), a recovery stair in the corner (-2.4, -1.2, then the floor), and the vent pad that throws you over the 7 m wall. The bot crosses in 6.6 s.
- **Flue (wall riding):** a 5 m corridor between two 18 m walls, run along its length. Measured with the real Mover (`campaign --shaft`): a wall ride needs
  speed along the wall (minSpeed 8), so the shaft is a corridor, not a well; each wall jump gives about +1 m, three per airtime; two wall jumps reach
  3.7 m, so the exit deck is 3.4 m, not 6. Wall line 3.2 s (0.9 s to the corridor's mouth + 2.3 s climbing); the stepped route on the east side (two
  steps of 1.7 m) 8.6 s on RIVAL's graph. The bot takes the stepped route (it has no wall jump).
- **Slide tunnel (optional):** 3 m wide, 10 m long, a 1.2 m ceiling in the foundry wall of room 3. The slide collider is 1.0 m, a swarmer 0.75 m, a husk 1.8 m:
  the player slides through in 1.39 s, a standing player is stopped, husks go round by the east half. A flank, never the only way.

## What is shown

TITLE: MODE row CAMPAIGN / SURVIVAL / THE GRIND (CAMPAIGN only with the campaign row on); a banner on entering a LIMIT room (PISTOL ONLY, NO DASH), a name card
and `weaponEarned` for the THUNDERCLAP at the end of room 2; the lighting kit (trim in cyan, mint or blue per room, one room light, one pulsing cyan guide
light at the exit, brighter once the room is clear). Not checked on the Player: the banners' look, the LUM meter on the lit frames, the TITLE reload flow.

## Layer 2: do differently

1. Run `levelgen` from the first sketch: every plan mistake of layer 1 (a node in a ramp, a door past the wall, a pit with no way out) was caught by the report,
   not by playing. Keep the report's checks as a gate.
2. Put the boss on the nav grid (or give the bot a boss mode) so the whole layer is one bot run and one number; today the boss is B's rig plus a sim kill.
3. Tune the encounters on 20 seeds, with the previous room's damage in the start state, and print the median health at each door.
4. Carry health across a door deliberately (a small heal at the checkpoint?): the rate is dominated by it. GG's rule is no passive regen mid-round; a door
   heal is a design call, not made here.
5. Zone boxes start at y 0: a pit is in no zone. Either make the zone boxes start lower for pit rooms or keep the "pit is the room's space" rule in the report.
6. Make the wall line REQUIRED somewhere only after a player test: the flue's wall jump is optional (a stepped route exists) because the bot, and a player who
   cannot yet wall-jump, must not be stopped.
7. Link pars are crossing times; consider no par for links and a par only for rooms.

# NIGHT25: the teaching campaign, the difficulty curve across the ten stages (builder L)

Sources: `Design/levels/*.plan.json`, `Design/levels/CURRICULUM.md`, `Reports/LEVEL-<id>-validate.md`, the headless bot (`campaign <plan> --seeds n`, `tutorial --seeds n`,
RIVAL-style autopilot "normal", the real sim). Enemy-seconds = the sum over a room's enemies of the seconds a player who knows the tool needs (husk 1.5, swarmer 0.8,
spitter 3, charger 4, lancer 5, drone 3, warden 6: `Tools/night/levels610-estimate.json`'s numbers) plus 0.8 s per extra wave: a size measure for comparing rooms, not a
prediction. "first-try" = the share of seeds that cleared the room with no death in it.

| id | level | new thing | enemy kinds | rooms: enemies / enemy-seconds | LIMIT | bot first-try by room (seeds) | par (s) |
|---|---|---|---|---|---|---|---|
| tutorial | 1 THE TUTORIAL | one verb per stage: walk, jump, slide, wall jump, pad, dash, pistol, fist, a fight | husk | 1.4: 3 / 5, 1.5: 3 / 5, 1.6: 8 / 13 | none | 100 / 100 / 100 % (30; stages 1.1-1.3 are links: 30/30 cleared) | 3 5 3 3 4 5 12 14 |
| layer1 | 2 THE KILN | locked doors, LIMIT rooms, the first boss (the WYRM) | husk, swarmer | r1 9 / 13, r2 49 / 60, r3 44 / 56 | r2 NO DASH, r3 PISTOL ONLY | 100 / 80 / 83 % (30) | 16 71 65 (+ links 7 8, boss 110) |
| level3 | 3 SPITTERS AND COVER | the spitter; strafing and cover | + spitter | 7 / 16, 18 / 33, 15 / 29, 14 / 30 | r2 NO JUMP | 100 / 78 / 60 / 70 % (40) | 15 33 29 27 |
| level4 | 4 VERTICAL | the drone; tiers, grapple nodes | + drone | 7 / 16, 25 / 56, 19 / 41, 18 / 44 | none | 100 / 73 / 70 / 78 % (40) | 14 43 34 30 |
| level5 | 5 CHARGERS AND LANCERS | the charger; the lancer in 5.4 | + charger, lancer | 7 / 19, 21 / 51, 18 / 43, 16 / 45 | r2 NO SLIDE | 100 / 83 / 68 / 68 % (40) | 17 43 40 41 |
| level06 | 6 THE BULWARK | the shield (the warden), a gauntlet before the boss (SHORT marker) | + warden | 17 / 22, 25 / 52, 28 / 72 | none | 100 / 80 / 60 % (30) | C's pars (capped) |
| level07 | 7 WARDENS | area denial, a recap gauntlet | all so far | 16 / 21, 23 / 56, 12 / 17, 26 / 67 | r3 MELEE ONLY | 97 / 87 / 87 / 60 % (30) | C's pars |
| level08 | 8 SPEED | par times and routes | (no new kind) | 22 / 27, 23 / 47, 31 / 61 | none | 100 / 90 / 73 % (30) | C's pars |
| level09 | 9 THE MONOLITH | a boss arena (gauntlet + SHORT marker) | (no new kind) | 20 / 30, 26 / 54, 20 / 40 | none | 100 / 50 / 50 % (30) | C's pars |
| level10 | 10 FINALE | every verb; the duel | all | 32 / 50, 24 / 54, 25 / 58, duel 4 / 20 | f1 PISTOL ONLY, f2 NO DASH, f3 NO SLIDE | 80 / 53 / 53 / 87 % (30) | C's pars |

Rules checked (GG: "they should all build up to each other"):

1. **The first room is easier than the previous level's last room** (enemy-seconds; bot first-try is 100 % for every first room, so the size is the measure):
   tutorial 1.6 13 s, level 2 r1 13 s (equal: it was 27 enemies / about 40 s before NIGHT25, a spike; now 9 enemies, husks and two swarmers); level 3 r1 16 s against the KILN's last combat room 56 s;
   level 4 r1 16 s against 30 s; level 5 r1 19 s against 44 s; level 6 b1 22 s against 45 s; level 7 w1 21 s against 72 s; level 8 p1 27 s against 67 s; level 9 m1 30 s against 61 s;
   **level 10 f1 50 s against level 9's last room 40 s: breaks the rule** (a PISTOL ONLY room with 32 enemies); asked of the lead for C's file.
2. **The spitter first appears in level 3.** The tutorial has husks only. Level 2 had 7 spitters in its three rooms in NIGHT24; they are husks now (the layer's rates were retuned: 100 / 80 / 83 %).
   Chargers first appear in level 5, drones in level 4, lancers in level 5 (5.4), wardens in level 6 (the shield lesson; level 7 teaches the denial). Swarmers are level 2's one extra kind.
3. **Level 2 teaches nothing the tutorial did not:** NO DASH (the dash: tutorial 1.3), PISTOL ONLY (the pistol: 1.4), the vent hall's pit and pad (1.2, 1.3), the flue (wall jump: 1.3), the slide tunnel
   is no longer on the critical path. New in level 2 are only the door-and-LIMIT structure, the swarmer, the THUNDERCLAP award and the boss, whose plan key is `"boss": "wyrm"` (W's generic boss hook; the default).
4. **Bands (30 seeds for 6-10).** Out of 60-90 after the first room: level 9 rooms 2 and 3 (50 / 50 %), level 10 rooms 2 and 3 (53 / 53 %, the finale is meant to bite but the band is the lead's rule),
   level 4 and 5's rooms are in band; the first rooms read 100 % on purpose (the breath: making them
   harder lowered the next rooms, because health carries across a door, so the damage budget is spent late).
5. **What the bot cannot say:** it plays a LIMIT room with its normal tools where it can (a PISTOL ONLY room is the pistol), it never uses the grapple or the wall jump outside the tutorial's scripted pilot,
   and one drone seed in level 4 stalls (the autopilot does not engage a drone hovering at 7 m). The rates measure the encounter's size, not a human's skill.

Tutorial polish from the Player reel (`Logs/feel/p1/tut/tutorial/<stage>/`, build n25-5, 240 frames per stage): what each stage shows and what reads badly.

- 1.1 a lit door, cyan trim and the move / look prompt in the pad glyphs (the reel ran on a pad). **A big "01 ROUND" banner shows over the stage:** it is the SURVIVAL round card and has no place in the tutorial (to the lead).
- 1.2 the tunnel's wall and the pit read, the prompt shows the jump glyph. 1.3 wall jump: the flue is a narrow corridor, the prompt "OFF THE WALLS" shows with the jump glyph; **jump pad:** a thin ring and a glow on the vent
  wall, but the landing deck is hidden behind the 7 m wall, so the pad's destination is not in view (a lower wall would show the deck but let a jump clear it; kept: the prompt names the pad). **Dash:** the pit is a dark band, the
  prompt reads "JUMP, THEN [LB] TO DASH". 1.4 husks show as orange capsules, **the {fire} glyph renders as a tiny garbled mouse icon ("Za")** (to builder F). 1.5 the husk is big and close, the pulse line reads.
  1.6 two husks in view, nothing hides the obstacle.
- The plan format has no `lighting` block (the trim, the room light and the guide light are made per room automatically by `CampaignLighting24`); nothing was added there.

## What the levels taught (NIGHT25 close, builder L)

- **Teach by geometry, not text.** The tutorial's obstacles do the teaching (a pit that needs a jump, a 1.2 m tunnel that needs a slide, a flue that needs wall jumps, a pad over a wall, a dash gap); the prompt only names the key once. Levels 3-5 and the forks of 8-10 are rooms that differ by shape (sightline, colonnade, nest, low roof, stack, gantry, lane, stampede, shelf; race track, plaza, yard, pit hall, vertical room, shaft). C's generated rooms for 6 and 7 are still the one template room; they are the weakest of the ten.
- **Every mandatory crossing needs a recovery and a bypass for the bot.** A pit has a stair and a respawn line; a slide tunnel or a wall line that is the only way is proved by the real Mover (`routeVia`, the tunnel row, the wall row) and is mandatory only in the tutorial; in the levels it is a CUT with a long way round, and the report prints both times. A bypass that is too narrow (a 2 m channel between a baffle and a flue wall) lets the bot (and a player) skip the weave: it was found by a stalled seed.
- **Health carries across a door, so the damage budget is spent late.** A harder first room only lowered the next rooms' clear rates; first rooms are breaths and read 100 % on the bot by design.
- **The bot is a measure of size, not skill.** A rate band of 60-90 % first-try is hit by tuning enemy counts per room on 30-40 seeds; a seed that stalls (a drone hovering at 7 m, a pillar edge) is a bot limit, not a level fault, if a human can finish.
- **The walks that failed on the Player (levelwalk, budget 235 s game time) are mostly the budget.** In the harness the plain autopilot clears layer1, level5, 06, 07 and 10 in every seed (30 of 30) with the rates in the table above; the sum of the bot medians of layer1 (16 + 71 + 65 s plus the links) and level 10 (46 + 75 + 101 s) is already over 235 s, so those walks stop at room 2-3 by the clock, not by a stall. Raise the walk's budget to the sum of the pars x 1.5 before judging the autopilot.
- **CampaignPilot25 was tried and not shipped.** A pilot wrapping the autopilot with a punch-the-shield script for wardens, a MELEE ONLY close-in and a stall escape was measured in the harness (30 seeds): the warden script killed the bot (level 6 rooms 2-3: 0 / 0 % first-try against the autopilot's 83 / 70 %: a warden swings from 2.4 m after a 0.8 s tell, the gauntlet reaches 2.4 m); with it off and only the stall escape and the MELEE ONLY close-in, level 6 and 7 matched the autopilot (83 / 67 %, 90 / 77 %) and level 10 rooms 2-3 fell to 37 / 50 % from 63 / 67 %. The file is not in the tree. The one thing that stayed: the top-up cap (`design.n25.lesson.maxTopUps`, 12): a room never respawns its last wave for ever for an enemy-dependent lesson (a test covers it). The "98 enemies in layer1 room 2" the Player walk showed is not a top-up (layer1 has no lessons): room 2 holds 49 enemies, so 98 is one death and a restart.
- **Open threads:** the tutorial's "01 ROUND" banner and the garbled `{fire}` glyph (to the lead and F); the pistol pedestal pickup and static training targets of stages 1.4 are not built; levels 6 and 7 want their own geometry (the same job as 8-10); level 9's boss par is the old 1:01 kill x 1.2, not re-measured.

# NIGHT29: levels 1-3 from scratch (the lead; details in Design/levels/DESIGN29.md and Design/levels/CURRICULUM.md)

What GG's words changed (2026-10-09: "every single level needs a complete arena overhaul completely from scratch"; "small ... closed ... claustrophobic ... action packed"; "a room based off the mechanics"): a room is 8-20 m wide, closed (a collider roof, no sky gap), dark with cyan / green wall-mounted and ceiling strips, ONE amber accent light, a pool at the goal; each fight room has a verb and a DEVICE that forces it (a hazard floor, a gap, a tunnel, a lane), never a locked door; an empty lit rest room between fights; lights, props and door frames are rows in the plan (sockets and authored positions), never scattered.

What measurement changed (R's real-Mover proofs): a running jump covers about 3.15 m, jump + second jump 3.5 m, jump + dash 8 m: so the dash gap is 6 m and the PULSE blast gap 10 m (8 m is crossable without the blast); the wall line tops out at 3.8-7 m (the flue is a 28 m corridor with a 3.4 m deck, not a 14 m shaft); a 1.2 m tunnel is no walkers' route on the nav grid, so the swarmers wait at its mouth; a 0.5 m step is a jump link for walkers.

What the Player showed (the reels): the Glacier arena was pale lavender because the roof was an Unlit white slab (vertex colours ignored) and the walls the Stone look, not because of fog: flat dusk violets and a deeper blue ice fixed it (mid-frame luminance 0.45 -> 0.075-0.122). A harness bot that dies restarts a boss at full HP and hides a too-fast camper: judge boss rules in the Player.

Open: the human feel of every room (nobody has played); the tank rooms; three skate lanes (five need a new, wider arena); pars are the pilot's medians, not a human's.

# NIGHT26: levels 1-3, the level select and ranks (builder L)

- Level 1 grew from 8 to 14 stages (386 m became 680 m long, floorSize 1060): the platforming run and the wall-ride hall after 1.3, a husks-only floor and a boss room at the end. Every room is 28 m wide (the NIGHT25 tutorial already was: GG's "too wide" is the old layer 1's 52-56 m rooms, which level 3 now cuts to 32 m). One verb per stage, each pit with its stair and the `respawnY` stage respawn. The validator found two real faults on the way (a 3 m end ledge put the exit inside the pit; a wall slab down to the pit floor split the pit's cells from the stair); both fixed in the plan.
- The graph bot cannot play the tutorial (its stages need the real verbs): `TutorialPilot25` got scripted stages p1 (jump, dash, double jump per gap), p3 (the flue's wall line, state reset on entering the room) and p4 (the same hop per pit); its absolute z values (`Lips1`, `Lips4`, `S5Lip`) move with the plan.
- A 7 m gap with a 4 m island failed (the dash overshoots the island): the islands are 9 m. An 8 m pit under the wall left the validator's player graph with no route; 7 m passes (the graph's reach, not the Mover's).
- Reels and bots on level 1: stages 1.5 and 1.8 stalled on the Player because the pilot held the click while the PULSE blast is now the left trigger (`TutorialPilot25` holds altFire when pulseLT is on); the `tutorial` reel stopped at ~120 s wall because its loop was bound by frames (now game time, cap 420 s, plus a 600 s wall guard); the hulk is fought by `HulkPilot26` (a copy of builder B's harness bot without the human lag).
- Not done: M's platforming kits (`Design/levels/kits`, hall route measured 6.18 s against 13.47 s) were not spliced into level 1; my own stages 1.4-1.7 stand, so the wall-ride hall still has no floor-versus-wall time. The rank blocks' STYLE cuts of levels 1-3 are guesses.
- Ranks: `Core/Rank26.cs` (letters per column from the plan's `rank` block, else the config's defaults from the pars; aggregate S5 A4 B3 C2 D1 minus restarts; P). Style thresholds are guesses. KILLS is kills over the plan's wave enemies (a boss room plans none: a level with no waves reads 100 %).

# NIGHT30: lava, the room cards, LEVEL 4 (builder L; the rooms and numbers are in Design/levels/CURRICULUM.md, the cards in Design/levels/DESIGN30.md section H)

## What the ROOM CARDS and the six ROOM QUESTIONS changed
- Every fight room of levels 1-3 got a card (DESIGN30 B and H) and its six answers before it was built. These were the questions that changed rooms: Q4 (two routes, the faster one skilful) and Q6 (a lying surface).
- **From Q4:** wall lanes added in f8 (E wall), k2 (W wall over the lava) and f5b (W lane, wall jump, E lane); walkers' ramps moved behind the enemies (i1b, f10's north ramp), so walking up is the slow way; a bridge (i6) and a catwalk (k1) make the loops.
- **From Q6:** every respawn pit became lava (f2, f3, f9, i2, i4, k2; f5 and k4's floors). f10's red-orange pool sat on the safe island and moved onto the strip it warns of. k1's green floor strip hid under the new ramp and moved.
- The Player frames answered Q2 (what you see first) and found what the SVGs could not:
  - f2's 1.0 m trench showed no lava from the door. The fix: lava 0.6 m deep, the spawn 2.5 m from the lip, and the trench's own red-orange light.
  - f10's entry was dark.
  - i6's pillar and k1's press, each a room's hero, read as blank shapes.
  - i1b's deck read as a blank wall.
  - Each got one change in iteration 2.

## The lava format (LEVEL-FORMAT.md)
- A pit row with `lava: true` has `floorY` = the surface, 0.6-1.0 m below the floor, and no `respawnY` in its room. levelgen writes the map's top-level `lava: [{rect, y, room}]`, and a `lava` validator row checks the depth band, the respawnY, the array entry and the collider.
- Grapple nodes are a room row `grappleNodes: [[x, y, z]]`. levelgen writes them as the map's `grappleNodes` `{x, y, z}` (SurvivalMap's format).
- Every lava trench has a climb-out proof (a walk with jump presses).

## What the frozen Mover forced (measured with LevelValidate29 proofs and `ridetrace30`)
- **f2:** the trench is 2.2 m, not 2.5. The running jump lands 2.59 m past the lip, and 2.5 m left 9 cm of margin.
- **f5b island:** its top is 0.3 m, not 2.0. A wall-ride entered at running speed stays near 0.4 m high and does not climb; f5's lane climbs to about 4.3 m only because its rider started slowly (a crate stalled its run-up, which also hid the problem).
- **f5b wall jump:** a wall jump off the W lane kicks the rider 14 m across to the E wall at about 2.4 m, so f5b's route is the card's lane chain. The deck dropped to 2.0 m because 3.0 m was above the E lane's arrival height.
- **f5b ledge:** the card's 6.0 m ledge is unreachable (the E lane tops out about 4.4 m) and is not built.
- **i5's rails:** kept at 2.2 m. 1.2 m rails would let a player walk the rail tops past the blocks the lane switches teach.
- **Risers:** the Mover does not walk up a 0.3 m riser (config `stepHeight` 0.4 notwithstanding). Each riser is a hop, so catch stairs need jump presses, in proofs and in the pilot.
- **Validator proofs:** a `jumpDash` proof's `reach` includes walking along a lava floor after the landing; it is not the jump's distance.

## Bots and lava
- With `design.n30.lava` off a 0.6-1.0 m trench is a soft-lock for the autopilot (no jump links out). On, the knock-up gets players out. The pilot still died in f5 / f5b's lava (348 / 911 room retries over 10 seeds).
- The fixes were room fixes first: a 5.5 m entry floor so the run-up and jump stay off the lava, a crate off the run-up line, f5's north rim steps, f5b's catch stair. Then pilot scripts: a rim to hop to in each lava room, ride retries from the entry.
- Result: f5 17 and f5b 2 room retries, foundry 10/10.

## The ceiling-light-over-the-title check
- A ceiling light 18-30 degrees above the eye (1.6 m) and within 13 degrees of straight ahead, seen from the room's spawn, sits behind the room title. The window was calibrated on the r2 Player frames of f5b, i6 and f9.
- Nine sockets were in it (f4, f5b, f9, i1a, i2, i6, k1, k2, s6) and all were moved. The script is builder L's scratch `titleline.cjs`, not in the repo; run it again after any lighting pass.

## For the next level pass
- Prove each movement idea with a proof row or `ridetrace30` before drawing it on a card (two cards here asked for heights the Mover cannot reach).
- Look at a Player frame from the room's spawn for Q2 before calling a room done; an SVG cannot show darkness, a hidden lava surface or a light behind the title.
- A lava room needs a rim every player can hop out of, close to wherever a short jump lands.
- Keep the walkers' ramp behind the enemies, so the fast way up is the wall line.
- Item 9's iteration 2 changed lights and readability only. The rooms' layouts got one structural iteration each; a second one is still to do.
