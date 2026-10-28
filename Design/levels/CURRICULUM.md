# CURRICULUM.md: the teaching campaign (NIGHT25, builder L)

GG (2026-10-08): "work mechanic to mechanic ... start off with just movement ... give him a weapon ... don't be afraid to break it up into stages ... 10 levels ... they should all build up to each other."
Template: ULTRAKILL 0-1 (a row of single-verb obstacles, the first ranged enemy deep in the first level, a boss returns later as a common enemy), the
Exposition / Validation / Challenge triplet, DOOM's "problem, then tool", CoD Zombies' small bare first map paced by doors (`Research/2026-10-08-wave25/RESEARCH.md` S1-S3).

## NIGHT30 (builder L): levels 1-4 as the campaign plays them with `design.n30.levels` on (plans `Design/levels/n30/{foundry,iceline,kiln,span}.plan.json`)

**The switch.** `Core/Curriculum25.Order` reads `design.n30.levels.order` while `design.n30.levels.enabled` is on: `n30-foundry, n30-iceline, n30-kiln, n30-span`,
then `level3 ... level10` as before. Off (or with `-skygrindOff30`), it reads `design.n25.curriculum.order`, NIGHT29's `n29-foundry, n29-iceline, n29-kiln, level3 ...`.
It is a flow row with no sim bit. The n29 plans are untouched, and the n30 ids sit beside them in `design.maps.list` (campaignOnly). `unlocks.json` rows:
`n30-kiln` earns the THUNDERCLAP, `n30-span` earns the NAILGUN. Generators: `Tools/night/gen-n30-*.cjs` through `n30-lib.cjs`, rebuilt by
`bash Tools/night/n30-regen.sh` (levelgen on each; 0 errors on all four at 22:25 on 2026-10-09). Rows on in this tree: levels, lava, span, glide.

**walk30, 10 seeds, measured 22:25 (Walk29Pilot with builder S's Span30Pilot; the boss rooms are not played):** foundry 10/10 full runs, median
107.9 s; iceline 10/10, 77.5 s; kiln 10/10, 84.5 s; span 10/10, 141.0 s. NIGHT29's i1 had 9 deaths and i6 16 over 10 seeds; the n30 equivalents
are i1a 0, i1b 2, i6 6.

| level | verb | weapon (start / earned) | boss |
| --- | --- | --- | --- |
| 1 n30-foundry THE FOUNDRY | move, jump, dash, wall-jump, wall-ride over lava, slide, the PULSE blast | pistol, fist, PULSE gauntlet | THE LANTERN (f12, room `hulk`) |
| 2 n30-iceline THE ICE LINE | the punch on a tank, the PULSE blast, ice lanes | the same; nothing earned | THE GLACIER (g1) |
| 3 n30-kiln THE KILN | the shotgun in a crowd, wall lines over lava | THE THUNDERCLAP (k1 pickup; `award` and unlocks row) | THE WYRM (k5) |
| 4 n30-span THE SPAN | THE ARCLINE grapple (`grappleNodes` rows, owned with `design.n30.span`) | THE NAILGUN (s1 pickup; `award` and unlocks row) | THE LOOM (s8, builder B) |

Rooms in order, with their lesson lines from the plans and the walk30 numbers (cleared of 10, median seconds, deaths):

| room | size w x l x h | enemies (waves) | lesson line(s) | device | walk30 |
| --- | --- | --- | --- | --- | --- |
| f1 INTAKE | 8x14x5 | - | MOVE. LOOK. WALK TO THE LIGHT. | | 10/10, 1.1 s, 0 |
| f2 STEP | 10x18x6 | - | {jump} JUMP THE LAVA | a 2.2 m lava trench, 0.6 m deep | 10/10, 1.8 s, 0 |
| f3 DASH | 10x22x6 | - | JUMP, THEN {dash} TO DASH | a 6 m lava trench (a jump then a dash crosses 7.72 m, a double jump 5.65 m) | 10/10, 1.5 s, 0 |
| f4 SHAFT | 10x52x9 | - | {jump} JUMP WALL TO WALL | the wall-jump flue (no lava: NIGHT29's f4 had no pit) | 10/10, 3.5 s, 0 |
| f5 WALL-RIDE ROOM | 14x24x10 | husk / husk (stand-ins) | RIDE THE WALL. AIM: TIME SLOWS. | a lava floor from the start, the west lane, north rim steps | 10/10, 6.2 s, 17 |
| f5b LAVA RUN (new) | 14x36x10 | 2 spitter | LAVA BURNS. STAY ON THE WALLS. | lava wall to wall; W lane, wall jump, E lane, a 2.0 m deck; a 0.3 m island; a catch stair | 10/10, 4.0 s, 2 |
| f6 REST | 5x8x4 | - | - | a health pickup | 10/10, 0.3 s, 0 |
| f7 SLIDE | 8x18x4 | 2 swarmer | HOLD {slide}: SLIDE UNDER | a 1.2 m tunnel | 10/10, 6.4 s, 0 |
| f8 PISTOL | 14x20x7 | husk / husk + spitter | {fire} SHOOT. HOLD {fire} TO CHARGE. then SWITCH ANY TIME. COOLDOWNS KEEP RUNNING. | a 3.2 m shelf, 1.2 m staggered cover, an E wall lane | 10/10, 5.0 s, 0 |
| f9 PULSE | 16x25x8 | 3 husk / 2 husk | FIVE KILLS. THEN {altFire} LAUNCHES. | a 10 m lava trench only the blast crosses | 10/10, 8.2 s, 8 |
| f10 FIGHT (combat loop) | 18x26x9 | 4 husk / 2 husk + 3 swarmer / husk + 2 spitter | RIDE THE RING. AVOID THE STRIP. | a hazard strip, a 2.0 m centre platform with a pillar, two wall lanes | 10/10, 23.9 s, 0 |
| f11 REST | 5x10x4 | - | - | | 10/10, 0.5 s, 0 |
| f12 THE LANTERN | 20x20x14 | boss | - | builder B's ring | not played |
| i1a TANK ALONE (new) | 12x20x6 | 1 tank | THE TANK DIES TO ONE PUNCH | two 1.2 m covers, two wall lanes, one red-orange top light | 10/10, 3.4 s, 0 |
| i1b TANK AND HUSKS (new) | 14x22x7 | 4 husk + tank | PISTOL THE SMALL. PUNCH THE TANK. | a 2.0 m deck ring, its ramp behind the tank, the W lane's wall jump lands on it | 10/10, 7.7 s, 2 |
| i2 PULSE ROOM | 16x25x8 | 5 husk | FIVE KILLS. {altFire} LAUNCHES. | a 10 m lava trench | 10/10, 6.9 s, 4 |
| i3 REST | 5x8x4 | - | - | the first ice | 10/10, 0.3 s, 0 |
| i4 FIRST LANE | 10x26x6 | - | HOLD FORWARD. DASH SIDE TO SWITCH. | an ice lane between two lava strips | 10/10, 3.5 s, 0 |
| i5 TWO LANES | 14x26x7 | 4 husk | DASH SIDE TO SWITCH LANES | two lanes, two forced switches, a 2.0 m far bank | 10/10, 7.1 s, 1 |
| i6 ICE FIGHT (combat loop) | 18x28x9 | 2 husk / 2 husk + tank / 2 husk + tank | TANKS FIRST. PUNCH THEM. | 1.2 m banks, a 2.4 m bridge between them, the lit pillar | 10/10, 18.3 s, 6 |
| i7 REST | 5x10x4 | - | - | a health pickup | 10/10, 0.5 s, 0 |
| g1 THE RINK | 28x52x12 | boss | - | builder B's Glacier | not played |
| k1 LOADING BAY (combat loop) | 18x22x8 | 8 husk / 4 swarmer | GRAB THE THUNDERCLAP | the press ring, a 2.4 m catwalk over the cluster | 10/10, 16.1 s, 0 |
| k2 CASTING FLOOR | 18x28x9 | 6 husk / 4 husk + 2 spitter | SHELF NEEDS THE PISTOL | a lava casting pit under a 2 m bridge, a W wall lane over the lava, a 2.4 m shelf | 10/10, 10.3 s, 16 |
| k3 REST | 5x8x4 | - | - | a health pickup | 10/10, 0.3 s, 0 |
| k4 FLUE | 14x30x12 | 4 husk / 4 husk + heavy | RIDE UP. SHOTGUN CLOSE. | a lava floor beside a 2.5 m walkway, the 22 m W lane, a 3.6 m gantry | 10/10, 11.1 s, 21 |
| k5 THE RING | 26x26x12 | boss | - | the Wyrm | not played |
| s1 TOLLGATE | 12x18x7 | 4 swarmer | NAILGUN: HOLD TO STREAM. | the NAILGUN on a lit plinth, a 2.4 m toll deck | 10/10, 7.6 s, 0 |
| s2 FIRST HOOK | 10x22x10 | - | {grapple} GRAPPLE THE RING. | one node at y 8.5 over a 4 m wall, lava at its foot | 10/10, 2.4 s, 0 |
| s3 THE GAP | 12x33x12 | - | SWING. LET GO AT THE TOP. | two nodes over a 15 m lava gap (a jump + dash falls in) | 10/10, 2.9 s, 0 |
| s4 HOOK FIGHT | 18x26x12 | 4 husk / 6 swarmer | NAILS FOR FLYERS. | gantries 1.2 / 3.0 / 4.0 m with walkers' ramps, two nodes | 10/10, 24.3 s, 0 |
| s5 REST | 6x10x4 | - | - | a health pickup | 10/10, 0.5 s, 0 |
| s6 CHAIN | 14x36x12 | 2 spitter | HOOK. RIDE. HOOK. | two nodes and the W lane over a lava floor, a 3.0 m deck, catch steps | 10/10, 5.9 s, 1 |
| s7 ESCALATION | 20x28x12 | 5 husk / 5 husk / 6 swarmer / 2 spitter | USE THE NODES. | the pylon in a 2 m lava moat, a ring of four nodes | 10/10, 43.6 s, 11 |
| s8 THE LOOM | 34x34x22 | boss | - | an 18 x 18 deck in an 8 m lava ring, `loomLegs` at the deck's corners, four static nodes | not played |

Not checked by walk30: the grapple crossings in the validator (it runs the Mover, not the rope; the span pilot crosses them in the sim), the boss rooms.

## NIGHT29 (builder R): levels 1-3 rebuilt from scratch (plans `Design/levels/n29/{foundry,iceline,kiln}.plan.json`)

GG: do not preserve anything. The old plans (`tutorial`, `level2`, `level3`, `layer1`) stay untouched; the three new plans are small CLOSED rooms from `DESIGN29.md` sections B, D, E, generated by `Tools/night/gen-n29-{foundry,iceline,kiln}.cjs` (through `n29-lib.cjs`) and checked by `levelgen` (all three reports: 0 errors). Map ids `n29-foundry`, `n29-iceline`, `n29-kiln` (files `Design/levels/n29/<name>.json`); NOT yet in `design.maps.list` or `design.n25.curriculum.order` (the lead wires them).
Each room is a `"n29": true` plan row: its own `height`, a collider `roof`, a door that is exactly the 4 m gap between two rooms (`doorFit`), light SOCKETS (`lights`) and authored `props`; `LevelValidate29` proves every one with real collider rays (rows `lights`, `props`, `doorFit`, `closedRoom`, plus `blast gap` and `dash lesson gap` on the real Mover).

| room | footprint w x l x h (m) | verb | what forces it (measured on the real Mover where it says so) | enemies |
|---|---|---|---|---|
| f1 INTAKE | 8 x 14 x 5 | move, look | a lit corridor, a pool at the door | 0 |
| f2 STEP | 10 x 18 x 6 | jump | a 3.5 m pit across the room | 0 |
| f3 DASH | 10 x 22 x 6 | dash | a 6 m pit: the real Mover's double jump falls short, a dash crosses (rows `dash gap`, `dash lesson gap`) | 0 |
| f4 SHAFT | 10 x 52 x 9 | wall-jump | a 28 m flue 4 m wide and a 3.4 m deck: the wall line reaches it in 3.2 s with 3 wall jumps (row `routes: f4`) | 0 |
| f5 WALL-RIDE ROOM | 14 x 24 x 10 | wall-ride, the slow-mo aim | hazard floor (`hazard`), the west wall lane, two husk stand-ins on an east ledge; `lessonHook: wallRiddenAndFired` | 2 husks |
| f6 REST | 5 x 8 x 4 | none | a health pickup (`healthPickup` prop) | 0 |
| f7 SLIDE | 8 x 18 x 4 | slide | a 1.2 m tunnel, 8 m long (row `slide tunnel`) | 2 swarmers |
| f8 PISTOL | 14 x 20 x 7 | shoot, charge | 1 + 3 husks, a spitter on a shelf up a ramp | 5 |
| f9 PULSE | 16 x 25 x 8 | the LT blast | a 10 m pit: a jump then a dash crosses 8 m, the blast aimed 25 degrees up crosses 12 m (row `blast gap`) | 8 husks |
| f10 FIGHT | 18 x 26 x 9 | everything | a hazard strip with a safe low platform, two wall lanes, a ring route | 18 (12 husk, 4 swarmer, 2 spitter) |
| f11 REST | 5 x 10 x 4 | none | the Lantern's glow (orange `danger` pool) at the far door | 0 |
| f12 THE LANTERN | 20 x 20 x 14 | boss | `boss: hulk` (the fight is the Lantern arena map) | boss |
| i1 TANK CORRIDOR | 8 x 24 x 6 | the gauntlet | 6 husks and a `tank` | 7 |
| i2 PULSE ROOM | 16 x 25 x 8 | the LT blast | as f9, 5 husks | 5 |
| i3 REST | 5 x 8 x 4 | none | the first ice sheen (`surfaces` ice, `maxY` 0.5) | 0 |
| i4 FIRST LANE | 10 x 26 x 6 | the skate lane | a 3 m ice lane over a pit on both sides, a 1 m lip at its end | 0 |
| i5 TWO LANES | 14 x 26 x 7 | lane switch + dash | two ice lanes, a block in each, a husk wave on the far bank | 6 |
| i6 ICE FIGHT | 18 x 28 x 9 | all | an ice sheet between two banks | 2 tanks + 10 husks |
| g1 THE RINK | 28 x 52 x 12 | skating, catching up | `boss: glacier` (the fight is builder B's rink map) | boss |
| k1 LOADING BAY | 18 x 22 x 8 | the shotgun | the THUNDERCLAP pickup (`pickup`, `award`) in view, a ring route round the press | 8 husks + 4 swarmers |
| k2 CASTING FLOOR | 18 x 28 x 9 | pit and bridge | a 2 m bridge over the pit, spitters on a far shelf | 10 husks + 2 spitters |
| k3 REST | 5 x 8 x 4 | none | | 0 |
| k4 FLUE | 12 x 30 x 12 | wall lane up | a hazard floor, the west wall lane, one east gantry | 8 husks + 1 heavy |
| k5 THE RING | 26 x 26 x 12 | the worm | `boss: wyrm` (the arena is the existing worm map) | boss |

Where the plan differs from the card (the measurement decides, the card's words stay in `DESIGN29.md`): f3's gap is 6 m (not 5.5) and f9 / i2's gap is 10 m (not 7): RIVAL's graph and the real Mover both cross 7 m with a jump then a dash, so only 10 m forces the blast; f4 is a 28 m flue with a 3.4 m deck (not 12 x 12 x 14 with a 12 m climb: the real Mover's wall line tops out at 3.8 to 7 m); f7's swarmers wait at the tunnel's mouth (the nav grid treats a 1.2 m tunnel as no walkers' route, so nodes beyond it fail the "enemies reach every tier" row); f5 keeps the existing `hazardFloor27` cycle (1.5 s telegraph, 8 s live, 12 HP per second), not the card's continuous 8 HP per second; k4 has one gantry (a wedge ramp starts at y 0 and a 0.5 m step is a jump link for walkers).
Pars: f1-f3 (1.1, 2.6, 3.0 s), i1 (6.0 s) and k1-k2 (18.9, 16.8 s) are the headless bot's median over 10 seeds; every other par is an ESTIMATE (the autopilot cannot play a flue, a tunnel, a blast gap, a hazard floor or a boss, and no scripted pilot exists for the new rooms yet).
Hooks for the other builders: room key `lessonHook: "wallRiddenAndFired"` (S: Lesson25 kind, until then the room clears on a `reached` lesson); room key `standIns` (f5: where the static and the strafing target stand; the waves hold two husks meanwhile); enemy kind `tank` (S); `pickup` / `weaponPickup` props with `weapon: "thunderclap"` and room `award` (S); `surfaces` rows with a `lane` block `{axis, line, halfWidth, dir}` (B's SKATE LANE rule); `lights` / `props` / door `height` (V).

## Rules for every stage and level

1. ONE new thing per stage or level. It appears alone, then is mixed on the next stage.
2. Triplet. **E**xposition: the obstacle in view plus one prompt line. **V**alidation: a safe attempt; the door stays shut until the lesson clears; a miss respawns at the stage start (no fail state, no soft-lock: every pit has a recovery, the check is in `LevelValidate24`). **C**hallenge: a short gated proof.
3. No enemy in the room while a movement verb is taught (1.1-1.3). The first fight is husks only (1.6). The first ranged enemy (spitter) is level 3. A boss returns later as a common enemy.
4. A level's first room is easier than the last room of the level before it (a breath); measured as the bot's first-try clear rate and median health at the door.
5. Prompts: at most 6 words, 18 px or more, shown once, gone after first use (the first-fifteen prompt system, `Core/FirstFifteen.cs`). Each has a keyboard and a pad variant; the pad variant uses the glyph name, never a key.
6. A lesson clears on a `lesson` condition (Core/Lesson25.cs, pure): `jumpedPit`, `slidTunnel`, `wallJumped`, `dashedGap`, `padHop`, `shots`, `targetsHit`, `punchWhileFiring`, `flickCrossed`, `husksDead`. Combat rooms clear by waves as in layer 1.
7. Long cooldowns hold (CORE.md): the dash lesson uses the game's own dash cooldown; nothing here shortens it.
8. Pars: not measured yet. The column "par budget" is the design target in seconds for a competent first-time player; the real par is the median of the headless bot over 10 seeds (written into the plan by `campaign`, never hand-typed). First-try clear target 60-90 % per combat room.

## The tutorial: layer id `tutorial` (6 stages, no death fail state)

| id | name | the ONE new thing | how the world teaches it (E / V / C) | prompt (KB / pad) | LIMIT | enemies | par budget | pass check (lesson clear) |
|---|---|---|---|---|---|---|---|---|
| 1.1 | LOOK AND WALK | move + look | E: an empty hall, a lit door at the far end, a cyan guide light. V: walk to it. C: none (the shortest stage). | `WASD MOVE  MOUSE LOOK` / `L STICK MOVE  R STICK LOOK` | none | none | 15 | link: reach the exit within 4 m |
| 1.2 | JUMP AND SLIDE | jump, slide | E: a 3 m pit across the hall, then a 1.2 m ceiling tunnel. V: the pit has a recovery stair at its side (a fall respawns at the stage start or climbs the stair); the tunnel is the only way on. C: pit then tunnel in one run. | `SPACE JUMP` then `CTRL SLIDE` / `A JUMP` then `B SLIDE` | none | none | 30 | `jumpedPit` (feet cross the far lip without touching the pit floor) then `slidTunnel` (slide state inside the tunnel rect end to end) |
| 1.3 | WALL AND AIR | wall-jump, jump pad, dash | one alcove per verb: a flue (walls 5 m apart) with a 3.4 m exit deck (reuses layer 1's measured line); a pad that throws you onto a ledge; a 7 m gap that needs the dash. E: each alcove in view from the hall. V: a recovery floor under each. C: all three in a row, door opens at the end. | `JUMP ON A WALL` / `A ON A WALL`; `PAD LAUNCHES YOU`; `SHIFT DASH` / `RB DASH` (the pad map in `Core/Gamepad.cs`) | none | none | 45 | `wallJumped` (>= 2 wall jumps), `padHop` (pad trigger then land on the ledge), `dashedGap` (dash state starts before the gap and lands beyond it) |
| 1.4 | THE PISTOL | the KESTREL, the reticle cooldown arc | E: the pistol on a pedestal behind a low rail, three husks in a pit below that cannot reach you (the problem first). V: pick it up; 3 static targets; the cooldown arc is explained by firing faster than it recharges. C: 3 slow husks walk a ramp up to you. | `LMB FIRE` / `RT FIRE`; second line `WATCH THE ARC` | none | 3 training targets, then 3 husks (slow: the husk is the web's slowest) | 40 | `shots` (>= 6) then `targetsHit` (3 of 3) then `husksDead` (3) |
| 1.5 | THE FIST | the PULSE GAUNTLET: melee while firing, the pulse charge | E: a husk behind a gap too wide to walk and a ledge that only a charged flick crosses. V: punch a husk while holding fire (both at once, N25 meleeFire); each hit builds the charge. C: charge, flick across the gap, land. | `V MELEE WHILE FIRING` / `X MELEE WHILE FIRING`; `HITS BUILD THE PULSE` | none | 2 husks (one at the near edge, one past the gap) | 45 | `punchWhileFiring` (a punch and a shot inside the same 0.25 s) then `flickCrossed` (flick impulse, then feet past the gap) |
| 1.6 | FIRST FIGHT | movement + pistol + fist together | a closed 40 m room, two tiers joined by a ramp (enemies reach every tier); two waves of husks; a checkpoint; the exit door opens on clear. No pick here (the first BIG PICK is the end of level 2). | none (a name card: `FIRST FIGHT`) | none | wave 1: 3 husks; wave 2: 5 husks | 50 | waves clear (as a combat room); bot first-try clear 60-90 % |

Tutorial total par budget about 225 s (3.8 min) for the headless bot; a human 6-8 minutes with the prompts read.

## Level 2 to 10 (the layer id is stable: `layer1` stays the id of THE KILN so existing tests and tapes keep working; its display name and `design.maps.list` label say LEVEL 2)

| lvl | name | the ONE new thing | how the world teaches it | prompt (KB / pad) | LIMIT (per room) | enemies (new in bold) | boss / beat | par budget (s, all rooms) | pass check |
|---|---|---|---|---|---|---|---|---|---|
| 2 | THE KILN | rooms with locked doors, LIMIT rooms, the first boss | today's layer 1 trimmed to three rooms + vent hall + flue + furnace; r1 is easier than the tutorial's 1.6 | room banners only | r2 noDash (awards THUNDERCLAP), r3 pistolOnly | husk, swarmer, spitter appears only as one at the end of r3 (a taste; level 3 makes it the subject) | THE KILN WYRM, with its boss bar; BIG PICK at the end | 260 (measured 258 s, layer 1) | bot per-room clear 60-90 %, boss dies (bossDown) |
| 3 | SPITTERS AND COVER | the ranged enemy; strafing and cover | pillars and low walls as hull cover in every room; a spitter alone in room 1 behind a wall so the player sees the glob, then mixed | `STRAFE  BREAK LINE OF SIGHT` / `L STICK STRAFE` | r2 noJump (strafe, not hop, solves it) | husk, swarmer, **spitter** (the subject) | a mini-beat: a spitter nest room (4 spitters on a shelf, a husk screen) | 240 | first room (1 spitter) rate >= 90 %; room 3 60-90 % |
| 4 | VERTICAL | air control, the grapple + ARCLINE; drones | one tall hall (a 14 m shaft with grapple nodes on its walls, a pad stack) as a link first (E, V), then a combat room whose upper tiers hold the drones; ramps for walkers to every tier | `GRAPPLE TO A NODE` / `LB GRAPPLE` | none | husk, spitter, **drone** | a drone swarm room; no boss | 250 | `lesson` shaft climbed by grapple (link), bot clears rooms with drones overhead |
| 5 | CHARGERS AND LANCERS | crowd control: the slide-kick and the pulse flick | a long hall with a charger lane (the charge is dodged by sliding or flicking aside); a lancer behind a pillar line | `SLIDE KICKS THEM` / `B SLIDE` | r2 noSlide (the hall forces jump + flick) | husk, spitter, **charger**, **lancer** (a lancer only in the last room) | a gauntlet: charger pair then lancer | 260 | per-room 60-90 %; the pulse charge is used (Fist24 flick count > 0 in the bot's run, or logged SHORT) |
| 6 | THE BULWARK | shield break (headshot or melee) | the boss alone in an arena with 2 husk adds; a shield wall in room 1 (a husk with a shield, if the kind exists, else the BULWARK's own) | `BREAK THE SHIELD: HEADSHOT OR PUNCH` (an exception to 6 words: split in 2 lines) | none | BULWARK (boss), husks | BULWARK boss, wired by a hook; not ready = the room clears with a logged `SHORT bulwark` marker | 200 | boss dies or SHORT marker printed |
| 7 | WARDENS | area denial; a recap | wardens (the web's zone-denial kind) in a room with narrow safe lanes; then the mini-gauntlet: every enemy so far in four short waves | `LEAVE THE WARD` / `MOVE OUT OF THE FIELD` | r3 meleeOnly (fist-only room, husks only) | husk .. lancer, **warden** | mini-gauntlet of all kinds | 300 | per-room 60-90 %; warden room first-try >= 60 % |
| 8 | SPEED | par time; routes (the wall line is faster than the stairs) | no new enemy: three rooms with a wall route, a pad route and a grapple route each saving seconds; the rank (S/A/B/C by par) is the lesson | `BEAT THE PAR` | none | husk, swarmer, charger, drone | none (a highway stub is allowed only if everything else is DONE) | 210 | the fast route beats the stepped route in the report (already a `wallRoute` check); rank shown |
| 9 | THE MONOLITH | a boss that changes the arena (floor, adds, orb parry) | the first slice of `Core/Enemies/Monolith.cs`; husks recycled as its adds (a boss returns as a common enemy) | `PARRY THE ORB` / `X PARRY` | none | MONOLITH (boss), husks | MONOLITH wired by a hook; if the cut items are not done, the room clears with a logged `SHORT monolith` | 240 | boss dies or SHORT marker |
| 10 | FINALE | every verb and enemy; the duel | three rooms mixing everything (every LIMIT flag once, one room each), then one duel room with no adds | none | r1 pistolOnly, r2 noDash, r3 noSlide, duel none | all kinds; the WYRM's tail pieces as common enemies | the final boss (the WYRM's big brother or the rival, `Research/2026-10-08-wave24/DESIGN24-bosses.md`); not ready = SHORT marker | 330 | bot first-try 60-90 %; the duel logged |

## What the pipeline must do for this (builder L's queue, plan format changes)

- A `lesson` clear condition on a room or link: `lesson: {"kind": "jumpedPit", "n": 1}`; `Core/Lesson25.cs` holds the pure rule (a small state machine fed per step with the player's grounded / slide / dash / wall-jump flags, shots, hits and the room's rects); the number in `n` is a plan row, the thresholds (4 m, 0.25 s) are `design.n25.lesson` rows with `_why`.
- A per-stage respawn: a fall below `killPlaneY` or into a pit with no recovery returns the player to the stage's `spawn` with no death, no DOWN screen, no style loss (a `stageRespawn` flag on the room).
- `LevelValidate24` new rows: every pit has a recovery (exists); every lesson is achievable by the bot (the bot's input script satisfies the lesson on a headless run, or the row is a logged "not bot-checkable: reason").
- `Campaign24`: `levels` list (tutorial, layer1 ... layer10) in `design.n25.curriculum`; CAMPAIGN starts `tutorial`, then `layer1`; a level is one plan file + one config row.
- Dependencies on the N25 shared commit: `N25Blocks` bits (tutorial, curriculum, meleeFire, pulseCharge) and the 1.5 stage needs builder C's melee-and-fire and pulse charge; until they land, 1.5's lessons print as `SHORT (blocked on C)` and the room opens on the shots alone.
- Level 4 needs a grapple/ARCLINE node kit in the plan format (`grapple` rows exist in DRYDOCK's format). Levels 6 and 9 need boss hooks from the lead (one line each in `SurvivalSim.Campaign24.cs` is mine; the boss start call in `SurvivalSim.cs` is the lead's).

## Status

Updated after levels 3-5 (headless bot, "normal", the real sim; pars are bot medians; `campaign <plan> --seeds n`). What differs from the tables above:

- **Tutorial** (`tutorial.plan.json`, id `tutorial`): 8 areas (1.3 is three links: wall jump, jump pad, dash). Prompts are `{placeholder}` formats. `tutorial --seeds 30`: 30/30 cleared, no soft-lock. With meleeFire + pulseCharge on (`--c25`) 20/20. Stage 1.4's "training targets" are 3 husks (no static target entity); the pistol pedestal pickup is not built (the player starts with the KESTREL); stage 1.5 has 3 husks (one pulse notch is 7 hits); the flick is required by the lesson rule, a jump plus double jump also crosses the 7 m pit. Stage pars (s): 3 / 5 / 3 / 3 / 4 / 5 / 12 / 14.
- **Level 2 THE KILN** (`layer1.plan.json`, id stays `layer1`): first-try 90 / 80 / 90 % over 10 seeds (rooms 1-3), the boss as before.
- **Level 3 SPITTERS AND COVER** (`level3`): four rooms, r2 NO JUMP. First-try 100 (the breath) / 78 / 60 / 70 % over 40 seeds; pars 15 / 33 / 29 / 27 s.
- **Level 4 VERTICAL** (`level4`): four tall rooms (walls 24 m), decks at 4 / 8 / 6 / 3 m each with a ramp, two grapple nodes in 4.2 (the bot does not use the grapple; the prompt names it). First-try 100 / 73 / 70 / 78 % over 40 seeds; pars 14 / 43 / 34 / 30 s. No LIMIT room in level 4 (the table said none). Bot limit: one seed in 40 stalls in 4.2 with a drone alive at 7 m (the autopilot does not engage it); a human can shoot it.
- **Level 5 CHARGERS AND LANCERS** (`level5`): r2 NO SLIDE; lancers only in 5.4. First-try 100 / 83 / 68 / 68 % over 40 seeds; pars 17 / 43 / 40 / 41 s.
- The first room of levels 3-5 is a breath and reads 100 % by design (the band 60-90 is for the later rooms): making it harder only lowered later rooms, because health carries across a door.
- Levels 6-10: builder C's (`levelNN.plan.json`); not measured here. W's boss hook (plan room key `"boss": "wyrm" | "bulwark" | "monolith"`, needs design.n25.bosses.enabled) is the way a plan names its boss.
Written at the first 40 minutes: design only, nothing measured. The par and clear-rate columns fill in from `curriculum` / `tutorial` harness runs; this file is updated in place with the real numbers.
- Levels 6 and 7 are builder C's generated plans. **Levels 8, 9 and 10 are forks by builder L** (C's ids, waves, LIMIT flags and par semantics kept; the plan files are L's now, `Tools/night/gen-levels610.py` no longer produces them) with hand-authored geometry:
  - **Level 8 SPEED** (race track): 8.1 a two-baffle chicane with a slide tunnel cut (the report prints the long way 8.3 s and the cut 5.28 s); 8.2 a four-baffle weave with a wall-jump flue up to a bridge deck (wall line 8.13 s against the weave's 14.04 s); 8.3 a straight with an 8 m pit: a jump pad hops it, a grapple node hangs over it, a narrow west-edge bridge walks it, a stair recovers. First-try 100 / 83 / 60 % (30 seeds); pars 33 / 56 / 82 s.
  - **Level 9 THE MONOLITH**: 9.1 a plaza of seven 7 m pillars; 9.2 a yard of five 8 m container stacks; 9.3 the gate: a 12 m monolith stand-in in a ring of four pillars, two gate posts at the exit; then the boss room `"boss": "monolith"` (kind boss; par 73 s = the composed-sim bot's 1:01 kill x 1.2, W's current number not re-measured). First-try 97 / 83 / 77 % (30 seeds, rooms 1-3); pars 32 / 36 / 34 s. The first room reads 97, not 100: a breath is a breath, one death in 30.
  - **Level 10 FINALE**: 10.1 (PISTOL ONLY) an 8 m pit (pad, grapple node, west bridge, stair) then a wall with a slide-tunnel cut (long way 7.81 s, cut 5.69 s); 10.2 (NO DASH) the vertical room: decks at 4 and 9 m by ramps, a pad onto the lower deck, walls 24 m; 10.3 (NO SLIDE) the wall-ride shaft: a 17 m flue to a 3.4 m catwalk over a three-baffle weave (wall line 6.99 s against 11.18 s); then THE DUEL (`"boss": "rival"`, par 211 s = W's mean 2:56 x 1.2) and THE ELDER FURNACE (`"boss": "final"`, par 397 s = 5:31 x 1.2), each in its own arena by W's hook (the map rooms are stubs, the doors checkpoints). First-try 100 / 63 / 67 % (30 seeds, rooms 1-3); pars 46 / 75 / 101 s.
  - New validate rows for these: `shortcuts` (a tunnel cut against the long way, both times printed) and `wallRoute.after` (the deck to the exit on the graph, so the wall line is timed door to door). One seed in 30 of level 8 and level 10 can still stall a bot between rooms (the autopilot catches on a pillar edge); no human soft-lock was found.
  - The boss rooms need design.n25.bosses.enabled (and the rival row); with them off a boss room clears at once.

## Pars of levels 6-10 (builder C): par = min(bot median, parCap)

The headless bot is slow against shields and with fists only, so its median over 10 seeds is not a par a human should be held to (level 6 room 2: 197 s; level 7 room 3,
meleeOnly: 409 s). Each room of `level06..level10.plan.json` carries `botMedian` (the bot's median over 10 seeds), `parCap` and `par = min(botMedian, parCap)`.
`parCap` = 2.5 x the human-style combat-time estimate: the sum over the room's enemies of the seconds a player who knows the tool needs (husk 1.5, swarmer 0.8,
spitter 3, charger 4, lancer 5, drone 3, warden 6), plus 0.8 s per extra wave and an 8 s walk (`Tools/night/levels610-estimate.json`, with its `_why`; the numbers are for pars
only, the sim never reads them). Written by `Tools/night/gen-levels610.py --pars Tools/night/levels610-pars.json` (the bot medians come from `levels610-pars.py`).
Three rooms are capped: level 6 room 2 150.5 s (bot 196.8), level 7 room 2 161.2 s (bot 189.8) and room 3 62.0 s (bot 408.7); every other room keeps the bot median.
Bot first-try rates are unchanged by the cap (the same 10 seeds: level 6 100 / 70 / 80 %, level 7 100 / 90 / 90 / 70 %). A human can earn S inside a capped par; the bot's
own median is no longer the target.

## Levels 6 and 7: authored geometry (builder C), measured on the bot over 30 seeds

Source: `Tools/night/levels67-kits.py` (one kit per room) through `Tools/night/gen-levels610.py` (levels 8-10 are builder L's now and are not written by it). Every room validates
(`levelgen`: 0 errors, every tier reachable by walkers). The WARDEN is the web's shield kind, so level 6 teaches opening a shield by geometry:
- 6.1 SCRAP YARD (a breath, 100 %): three N-S cover lanes of scrap walls and crates, two low scrap walls; husks and swarmers.
- 6.2 THE SHIELD WALL (83 %): a low (1.3 m) wall across the room to peek and shoot over (a headshot opens the shield), 6 m flank gaps at both ends (the warden cannot face the wall and
  the flank at once), a 1 m pedestal in front of the wall to punch from; one warden per wave at a time behind husks and swarmers.
- 6.3 BULWARK ANTE (70 %): a long approach, pillars and two raised catwalks (3 m, a ramp each) at the north end; wardens, chargers, husks; leads to the boss door.
- 6.4 THE BULWARK: a `kind: boss` room, `boss: bulwark`, through Campaign24's BeginBoss; the campaign bot kills it (36 s on seed 1, 7 retries earlier in the level).
Level 7:
- 7.1 THE GATEHOUSE (a breath, 100 %): a gate wall with a 12 m gap, two free-standing pillars.
- 7.2 THE WARD LANES (90 %): four serpentine N-S cover walls, a warden walks each lane (area denial by lane, not a field).
- 7.3 THE FISTWORKS (meleeOnly, 90 %): a grid of 2 m pillars (fists matter), a 2 m platform on the east side by a ramp from the middle. SHORT: the pit with a pulse-flick shortcut the
  brief asked for is not in the level: a pit beside the route trapped the campaign bot (the bot fell in and stayed; 8 of 10 seeds never reached room 4), so the room has the platform and ramp only.
- 7.4 THE MINI GAUNTLET (73 %): one arena with three heights: the floor, a 2 m central deck, 4 m mezzanines on both walls, a ramp to each; every kind met so far in four waves.
Pars: `par = min(bot median over 30 seeds, parCap)` as above: level 6 room 2 174 s (bot 232 s), level 6 room 3 178 s (bot 205 s), level 7 room 2 161 s (bot 184 s), room 3 62 s (bot 516 s), room 4
131 s -> the bot median 118.5 s; first rooms 26.2 s and 25.0 s. Bot first-try rates: level 6 100 / 83 / 70 %, level 7 100 / 90 / 90 / 73 %.

## NIGHT26: levels 1-3 as they are now (builder L; supersedes the tables above for these three; levels 4-10 are still NIGHT25's and LOCKED on the select)

Play order (design.n25.curriculum.order, design.maps.list labels): 1 `tutorial` THE FOUNDRY, 2 `level2` THE ICE LINE, 3 `layer1` THE KILN, then `level3`..`level10` (numbered 4-11 in their labels; the select lists ten slots, so `level10` is not shown). The ids stay stable for tests and tapes. Unlocks (Design/levels/unlocks.json, builder W's): level 1 THUNDERCLAP (shotgun), level 2 NAILGUN, level 3 TORCH (the RATTLER is not ported).

| lvl | id | rooms (28 m wide for level 1, 32 m for level 3) | boss | LIMIT | plan `rank` (time s / kills % / style) |
|---|---|---|---|---|---|
| 1 THE FOUNDRY | tutorial | 1.1 look and walk, 1.2 jump and slide, 1.3 wall jump / jump pad / dash (as NIGHT25), then the PLATFORMING RUN: 1.4 dash islands (a 39 m pit, two 9 m islands, 7 m gaps), 1.5 pad hops (a 24 m pit, one island, two pads), 1.6 a wall-jump shaft (the 1.3 flue again), the WALL-RIDE HALL 1.7 (one continuous wall x -9..-7 over three 7 m pits), then the pistol, the fist, first fight, 1.8 the foundry floor (husks only, two waves) | THE SLAG HULK (`hulk`, builder B) | none | 300/420/600/900, 100/85/65/40, 2500/1500/700/250 |
| 2 THE ICE LINE | level2 | one boss room (`glacier`, builder B; the course map and the surfaces are B's and M's); this plan is only the entry hall | THE GLACIER | pistolOnly | 150/210/330/480, kills n/a, 1500/900/400/150 |
| 3 THE KILN | layer1 | the loading bay, casting floor and slag hall cut from 52 / 52 / 56 m to 32 m, the vent hall (pit, two pad hops, vent wall) and the flue, no THUNDERCLAP award in room 2 (level 1 gives it) | THE WORM (`wyrm`, builder B's wall-riding arena) | r2 noDash, r3 pistolOnly | 600/780/1020/1500, 100/85/65/40, 4000/2500/1200/500 |

Measured tonight (SimHarness, seed 1, the real sim): `levelgen` 0 errors for all three (reports `Reports/LEVEL-{tutorial,level2,layer1}-validate.md`); the tutorial pilot clears all 14 stages with 0 stage respawns (bot medians: dash islands 5.2 s, pad hops 3.6 s, shaft 4.7 s, wall hall 5.5 s, foundry floor 10.3 s; the pars in the plan are those rounded up). With the Player's blocks (`tutorial --n26 --seeds 3`: pulseLT, weaponIdentity, level1 on; the loadout KESTREL + PULSE only; the LT held for the pulse in stage 1.5; `HulkPilot26` fights the boss) 3 of 3 seeds clear all 14 stages, medians: pistol 5.5 s, fist 14.3 s, first fight 11.8 s, foundry floor 14.0 s, THE SLAG HULK 34.8 s with a perfect-aim bot (builder B's human-lag bot: 60.6 s); the pars of stages 1.5 and 1.8 are 15 s. Without `--n26` the boss stage reads 0.0 s (the hulk block is off: the room clears at once). Level 3: the campaign bot clears rooms 1-5 (17.5 / 64.9 / 54.1 / 6.6 / 8.0 s against pars 16 / 71 / 65 / 7 / 8), the flue's wall line 3.2 s against the stepped route 8.6 s. Not measured: the floor-vs-wall time of the WALL-RIDE HALL (the report times only a climbing line), style thresholds (no run measured style), a human's times.

## NIGHT27: levels 1-3 rebuilt tight (builder A; supersedes the NIGHT26 section for these three; the NIGHT26 plans are kept in `Design/levels/n26/`)

Rules (NIGHT27.md, GG 2026-10-09: "the arenas are still way too wide ... a complete overhaul on the first three stages"): a combat room is 24 m wide x 38 m long x 14 m tall with a 2.5 m wall-ride lane on both long walls; the wall route must be at least 30 % faster than the floor route (`LevelValidate27`, `levelgen` prints the ratio per room; computed from config speeds, NOT measured on the Mover); a wave is within the cost budget `design.n27.budget` (10 + 3 x wave index points; husk 1, swarmer 0.5, spitter 2, heavy 6); 35-55 kills per level; no random LIMIT rooms (`pistolOnly`, `noDash`, `noJump` are gone from levels 1, 3 and 4; level 2's intro is the one place the loadout is cut and its banner says why).

| lvl | id | rooms (plan par s) | boss | notes |
|---|---|---|---|---|
| 1 THE FOUNDRY | tutorial | 10 stages, one lesson each: 1 move + look (3), 2 jump + pad (8), 3 dash the gap (4+3), 4 wall-jump shaft (8), 5 the wall-ride hall (a telegraph-only strip that never hurts; 6 is the slide), 6 slide under (6), 7 pistol + charge (10), 8 THE PULSE (21: five kills fill the pips, the LT blast launches over a gap), 9 one fight, 8 enemies of 2 kinds, the shotgun awarded (14), 10 the boss (97) | THE LANTERN (plan id `hulk`; boss1 on) | plan par sum 174 s; the pilot's measured total is 183.1-183.9 s on 3 seeds (3 s over the 180 s aim) |
| 2 THE ICE LINE | level2 | `i1` the pulse hall (65: gauntlet only, banner reason), `i2` the forced wall-ride stretch (8: a damaging strip, live 8 s), `ice` the chase (120) | THE GLACIER (pulse-only chase; boss2 on) | the pulse hall's bot median is 65 s with some runs over 300 s (the bot is poor at melee) |
| 3 THE KILN | layer1 | `r1` 24 (3 rooms: 24, 27, 25), links `ka` 7 and `kb` 8, the worm's room 110 | the worm (as NIGHT26, visuals unchanged) | 49 kills; wall route about 1.9 s vs floor 3.55-3.75 s per room |
| 4 (slot) | level3 | 4 rooms 24 x 38 (18, 21, 23, 29) | none | spitters and cover; 52 kills; wall route 1.9 s vs floor 3.79-4.37 s; no `noJump` |

Boss cards (the one verb, the one weapon, the window the player causes): THE LANTERN: verb WALL RIDE, weapon any gun for the lit seconds and the PULSE to finish; the shield is down only on the face that looks at the wall the player is riding (a short grace, `design.n27.boss1.shield.grace` 0.35 s). THE GLACIER: verb SKATE + PULSE-LAUNCH, weapon the gauntlet only; a launch that lands in the 1.5 s window opens at a 12 m gap; a miss costs 20 m of gap, never health; three hits kill.
