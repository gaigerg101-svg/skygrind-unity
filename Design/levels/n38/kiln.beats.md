# LEVEL 3 THE KILN, n39 rebuild: the beat sheet (NIGHT39 builder K4, card K4; written before any geometry)

GG, 2026-10-13 ~14:05 (Research/2026-10-14-wave39/GG-REQUEST.md, asks 1, 5, 6, 7): level 3 "needs to be completely overhauled ... encourage far
more mechanics, have a new arena, and keep the worm AI the same"; "encourage more movement, more use of the shotgun"; "all the rooms in the 3rd
level feel boring, uninspired, and randomly placed"; the ghost weapon after the shotgun (fixed by the lead in d52c5042: award ids only).

This sheet replaces NIGHT38 K3's n38-kiln (gates RED 0/4 on builder E's checker: the PULSE blast beat G1 / G2, the dash jump and the blast beat
G3 / G4; NIGHT38-LOG close table). Same theme (the kiln: molten metal at dusk), same file names (`Design/levels/n38/kiln.plan.json`,
`Design/levels/n38-kiln.json`, id `n38-kiln`, generator `Tools/night/gen-n38-kiln.cjs`, rewritten), same row (`design.n38.kiln38`: a
`n35-kiln` -> `n38-kiln` swap in Curriculum25.N38Levels), on only when the gates are GREEN on `n38 gates` AND the walk clears every room 10 / 10.

## Why K3's gates failed, and the one rule that fixes it
K3's gates were low ledges (4.5 / 5 m) and long voids (23 / 24 m). Builder E's checker (envelopes38.json, the real sim) has two non-boost moves
that beat both shapes: the PULSE gauntlet's LT blast (jump + blast: apex **7.75 m**, reach 29.65 m; the gauntlet is owned from level 2's j1) and
the dash jump (reach 27.24 m, + double 37.44 m; dash jump + blast 57.26 m). No void the boost crosses is longer than what the blast or the dash
jump crosses, so **no horizontal gate is provable on level 3**. Height is: every non-boost apex is at most 7.75 m (the blast); x 1.1 = 8.53 m.

**ROUND 2 (supersedes the 9.5 m below): every cliff is 12 m.** Builder E's checker (b13c7802) flies the PULSE blast + a wall jump off the
noride face: the feet reach 11.31 m and it landed on every 9.5 m cliff (RED 0/4). K4's sweep on E's checker: a LEANING face (a ramp wedge in
front of the cliff, runs 3.5 / 4.5 / 5.5 / 7 m, and 6 m at 10.5 m) is RED on all five (the blast and the dash jump ride the slope: 240-370
cheats each); a HIGHER cliff passes: 12 m +34 %, 12.5 m +29 %, 13 m +24 %, each by `boost x3 p-89 @0.05 + double@apex then shot` (three shots
and the double), the best other try at 12 m `pulse p89 @0.3 + face x2 + apex double` stopping -101 % short at the top's height. The level uses
12 m (the most margin for the climb). THE STACK's tiers are 12 and 24 m; flight-room roofs 20 m (THE STACK 32 m).

**The rule: every gate on level 3 is a CLIMB.** A noride cliff, wall to wall, its top at **9.5 m** over the take-off:
- over every non-boost apex + 10 % (7.75 x 1.1 = 8.53 m: the blast; jump + double 2.87; dash jump + double 3.41; the corner wall-jump cheese on
  the real Mover 3.50 m, NIGHT38 `n38kiln corner`);
- under the THUNDERCLAP CLIMB by a wide margin: jump, shoot straight down (0.05 s), double jump at the apex, shoot down again = **13.36 m**
  (envelopes38 `jump+double+boost x2`, K3's sequence): +41 %; three shots + double 16.26 m. Two shots without the double (9.61 m) only
  scrape it: the climb is the lesson.
- no void, no lintel, no pit: a miss drops you back on the floor you jumped from (no recovery stair, no soft-lock, no health lost);
- the cliff's faces and the room's side walls near it are `noride` (a ride's jump-off + double cannot climb them; wall jumps off noride walls are
  the Mover's: the corner cheese is 3.50 m); no ride wall within reach of a gate.
The numbers are read by the generator from `Design/levels/envelopes38.json` and asserted (the generator stops when a bound fails).

ULTRAKILL's rocket-ride decay (arena-boss-web.md section 1) is used only as a LEVEL constraint: one shot is never enough for a gate (6.76 m
apex), two shots scrape it, the climb (shot, double, shot) clears it. No verb changes.

## Rules this sheet is held to (n36 method; NIGHT38 rules kept)
- One job per room (teach / practise / combine / test / rest / boss), one verb, a movement line you can draw.
- Every solid carries `job` (line / cover / climb / hazard / marker / reward) and `why` (n36-jobs GREEN: lava within 4 m of an enemy spot or
  route; a climb route in every fight room with a raised spot: GG "there always really needs to be a route that enemies can climb up").
- **Gates live only in FLIGHT rooms** (links, no enemies). A fight room's raised decks always have a walkers' ramp, so in a fight room the boost
  is the FAST way up (a shortcut and a reward), never a gate (a ramp would skip it; D-6).
- Fight rooms are OPEN ROOFED ARENAS (`arena36: true`): 30-44 m across, 14-18 m roofs, asymmetric and vertical (Doom 2016's "combat chess";
  Doom Eternal's "jungle gym": arena-boss-web.md section 1), the exit visible from the entry, enemies switched on AHEAD on the line.
- At least one room rewards a boost **through** enemies: the pack stands under the line up, and the shot that lifts you is fired down into them.
- The THUNDERCLAP is the loadout id `shotgun` everywhere (the room award and the pickup; G0 b).
- `owns38`: jump, double, dash, slide, boost, ride, walljump (+ the checker adds the PULSE blast itself: the gauntlet carries from level 2).
  No grapple (design.n35.grapple35.fromLevel 4).

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge (n36-lib). `@y` is a height.

## The rooms
| # | id | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|---|
| 1 | kb1 | 3.1 THE CHARGE | 20 x 26 x 10 | teach (fight) | the THUNDERCLAP up close; ONE boost onto the 5 m RIM (its stair is the walkers') | 20 s |
| 2 | kb1b | 3.1b THE FLUE | 16 x 26 x 16 | teach (flight) | G1: the THUNDERCLAP CLIMB (jump, shoot down, double, shoot down) up a 9.5 m cliff | 10 s |
| 3 | kb2 | 3.2 POUR HALL | 36 x 44 x 16 | practise (fight) | a boost THROUGH the pack onto the 6 m CRUCIBLE; pours either side; a ride wall | 45 s |
| 4 | kb2b | 3.2b THE STACK | 16 x 34 x 26 | test (flight) | G2a / G2b: two climbs, 9.5 m then 19 m | 15 s |
| 5 | kb3 | 3.3 CASTING FLOOR | 40 x 46 x 18 | combine (fight) | ride over the pour, boost to the LADLE (8 m), slug the heavy, parry, launch into the pour | 60 s |
| 6 | kb3b | 3.3b THE SPOUT | 16 x 30 x 16 | test (flight) | G3: a climb out of a run (a 4 m dry pit before the cliff) | 12 s |
| 7 | kb4 | 3.4 REST | 10 x 10 x 5 | rest | a breath, health, the checkpoint | 5 s |
| 8 | k5 | 3.5 THE RING | as n35-kiln k5 | boss | THE KILN WYRM (the AI and its paths unchanged) in a NEW arena (below) | B1's band |

### 3.1 THE CHARGE (teach, fight)
- **Line:** spawn (0, 2) > the THUNDERCLAP on its plinth (0, 6) > three stand-ins in a tight group at (-3, 11) (0, 12) (3, 11): one blast up
  close > two husks switch on ahead at (-5, 17) (5, 17) > the RIM (a 5 m deck across the west half, lz 18..22): ONE boost (jump, shoot down:
  6.76 m) puts you on it, its stair at the west wall is the walkers' (and the slow way) > the door (0, 24).
- **Text:** "GRAB THE THUNDERCLAP" (at the plinth); "JUMP, SHOOT {fire} DOWN: BOOST" (at the rim's foot); teach35 `l3.charge`.
- **Reward:** the THUNDERCLAP (award `shotgun`); health on the rim.

### 3.1b THE FLUE (teach, flight): gate G1
- **Line:** spawn (0, 1.5) > 8 m run-up up the axis > the lip of the floor at lz 10 (the cliff face at lz 11.5) > THE CLIMB > the cliff top
  (lz 11.5..16, 9.5 m, noride faces, a cyan strip on its lip: where you land) > drop north > the door (0, 24).
- **Space:** 16 m wide, roof 16 m; noride liners on both side walls from the door to the cliff's back; amber light over the take-off, a white
  light down the cliff face, the cyan lip strip.
- **Text:** "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN." (5 words of verbs; lesson reached at the top).

### 3.2 POUR HALL (practise, fight): the boost THROUGH the pack
- **Line:** spawn (0, 2) > the first wave (4 husks + 2 swarmers) switches on in the TROUGH at the CRUCIBLE's south foot (lz 12..16), straight
  ahead on the line > jump into them, shoot DOWN: the blast hits the pack and lifts you onto THE CRUCIBLE (lx -6..6, lz 16..26, 6 m: over
  every non-boost apex but the blast; its walkers' ramp climbs its north face from the far floor) > wave 2 on the side galleries (3 m, ramps)
  and the far floor > ride the east wall's cyan panel north > the exit (0, 42), visible from the entry past the crucible.
- **Space:** floor 0; THE CRUCIBLE in the middle; two POURS (lava, lx -13..-9 and 9..13, lz 10..32) between the floor and the galleries
  (lx -18..-14, 14..18, 3 m): the walkers' lines down the gallery ramps pass along them (rims); one ride panel along the east gallery's inner
  face (VERBS rows 9 / 11 on this map).
- **Enemies:** wave 1 ahead in the trough; wave 2 two spitters on the galleries + four husks on the far floor; wave 3 a heavy on the crucible +
  three swarmers.
- **Reward:** health on the crucible.

### 3.2b THE STACK (test, flight): gates G2a, G2b
- **Line:** spawn (0, 1.5) > run-up > G2a: the climb onto TIER 1 (9.5 m, lz 10..20: 10 m deep, a 6 m run-up on it) > G2b: the climb onto TIER
  2 (19 m, lz 20..25) > drop north > the door (0, 32).
- **A miss on G2b** drops you on tier 1 (retry from there), or off its edge to the floor (G2a again): never a pit, never health.
- **Text:** "CLIMB." (lesson reached at the top).

### 3.3 CASTING FLOOR (combine, fight)
- **Line:** spawn (0, 2) > wave 1 ahead on the floor (lz 10..14): four husks + a heavy (the slug) > the POUR in the middle (lava, lx -8..8,
  lz 18..28) with two cyan RIDE PANELS standing in it (lx -3 and 3, facing out: ride across it, or punch husks into it) > THE LADLE (lx 12..20,
  lz 14..34, 8 m, east): one climb (shot, double, shot) or its long walkers' ramp from the north floor > spitters on the WEST GANTRY (4 m,
  lx -20..-14): parry their globs back from the floor or the ladle > the exit (0, 44).
- **Enemies:** wave 1 four husks + heavy ahead; wave 2 two spitters on the west gantry + three husks by the pour's rims; wave 3 heavy + two
  spitters + two swarmers.
- **Reward:** health on the ladle (the high ground you climb to pays).

### 3.3b THE SPOUT (test, flight): gate G3
- **Line:** spawn (0, 1.5) > run-up > the lip (lz 9) > a 4 m dry pit (its recovery stair in the west corner) > the cliff (9.5 m, lz 13..17) >
  drop > the door (0, 28). The climb has to carry forward over the pit: shoot down-and-back, not straight down.

### 3.4 REST and 3.5 THE RING
- REST: a health pickup, the checkpoint, the door to the boss room.
- THE RING (k5): n35-kiln's boss-room row copied (THE KILN WYRM, the trigger, the dressing); the worm fights in its own room far from the map.

## The worm's NEW arena (the AI the same)
Measured (`n38kiln trace k5 --level n35-kiln`, this worktree, 2026-10-13): n35-kiln's k5 loads **`Design/maps/wyrm26-arena.json`** through
`design.n26.bosses.wyrm26.arena` (SurvivalSim.Wyrm24.cs Wyrm24Ensure: `Wyrm26On` true, `wyrm24Is26` true), arena half 30, `arenaHalf` 30. The
hard-coded `design.n24.wyrm.arena` (wyrm-arena.json) is NIGHT25's worm only.
- The new arena is a new file, `Design/maps/wyrm39-arena.json` (generator `Tools/night/gen-n39-wyrm-arena.cjs`): the SAME shell (half 30, height
  30, the stepped corners, floor, ceiling, the spawn nook), so `arenaHalf` and Wyrm.cs's clamp are unchanged; the SAME paths file
  (`Design/bosses/wyrm26.json`: the AI and every route unchanged); a new interior built for the THUNDERCLAP: boost perches at 6 / 9.5 / 13 m on the
  ring outside the routes, ride slabs, two crucible stacks; every piece kept 2.9 m clear, in 3D, of every densified worm route.
  As built (round 1): 29 boxes = the 18 shell boxes + 6 perches (one at 6 m, three at 9.5 m, two at 13 m), 3 ride slabs, 2 stacks. gen-wyrm26.py's
  stricter rule (no route point anywhere under a piece) leaves no room for a single 6 m perch; the old arena's own pillars sit ON the routes
  (0.00 m) and its 6 m platform 1.61 m from one (measured by the generator's probe).
- k5's `crumble31.boxes` are re-pointed at the new arena's two 13 m perches (the camp test's perches crumble at half health, as NIGHT31); the 6 m
  perch is kept.
- **The path override is the LEVEL's**, not a config row: n38-kiln's k5 row carries `wyrmArena: "Design/maps/wyrm39-arena.json"`, read by
  `SurvivalSim.Kiln39.cs` (K4's file). n38-kiln plays only while kiln38 swaps it in, so the arena is behind kiln38 with no new row; every
  other map has no field and keeps `design.n26.bosses.wyrm26.arena`. Wyrm24Ensure reads only the config path today, so two call sites in the
  lead's file are needed: `node Tools/night/n39k-wyrm-hook.cjs` (measured with it applied, then reverted: n38-kiln's k5 builds
  wyrm39-arena.json, 29 boxes, half 30, arenaHalf 30; n35-kiln's k5 still builds wyrm26-arena.json, 31 boxes; fingerprint and --off39 EQUAL).

## Gate table, round 2 (12 m cliffs; E's checker with the blast + wall-jump tries, 2260 tries per gate)
| gate | room | top | measured |
|---|---|---|---|
| G1 | 3.1b THE FLUE | 12 m | PASS +34 %; best other `pulse p89 @0.3 + face x2 + apex double` -101 % |
| G2a | 3.2b THE STACK | 12 m | PASS +34 %; best other the same, -101 % |
| G2b | 3.2b THE STACK | 24 m (12 over tier 1) | PASS +34 %; best other the same, -101 % |
| G3 | 3.3b THE SPOUT | 12 m (4 m pit) | PASS +34 %; best other: none |

Walk round 2 (`n38kiln walk`, 10 seeds): every playable room 10/10, level median 177.5 s (bot deaths kb2 4, kb3 4). Before the fix 4 seeds
stood 900 s at CASTING FLOOR's pour: the two ride panels' ends stood 1 m out of the lava, a 3.6 m mouth on the room's axis that the cleared
room's route ran into; the panels now stand inside the lava only (lz 18.2-27.8) and the pilot rounds a pour by its nearer side once the room
is cleared (Walk38Kiln.cs).

## Gate table, round 1 (9.5 m: superseded; `n38 gates`, measured 2026-10-13 round 1)
| gate | room | intended | void | top | measured |
|---|---|---|---|---|---|
| G1 | 3.1b THE FLUE | the climb | none | 9.5 m | PASS +49 % (boost x3 p-89 @0.05 + double@apex); best other try: none |
| G2a | 3.2b THE STACK | the climb | none | 9.5 m | PASS +69 %; best other: none |
| G2b | 3.2b THE STACK | the climb from tier 1 | none | 19 m (9.5 over tier 1) | PASS +55 %; best other: none |
| G3 | 3.3b THE SPOUT | the climb over a 4 m pit | 4 m | 9.5 m | PASS +49 %; best other: none |

1500 tries per gate on five lines (0, +-2, +-4 m), the PULSE blast included. The walk bot's trace shows a double jump pressed against the face
IS a wall jump (it kicks the player 3 m back out: kb1b, t 2.10 s), so a person's climb will often include one; the climb still lands.
**The blast + wall-jump family** (not in the checker's tries) is K4's own probe on E's rig and runner, `n38kiln blastwall`: 864 tries per gate
(blast pitch 30-89 at 0.05 / 0.15 / 0.3 s, a press 0.1-1.2 s later at the face, a second press none / +0.2 / +0.4 / +0.6 s): **0 land on any of
the four cliffs**, but the feet reach **11.03 m** over the take-off (blast p89, a wall jump, the double) while the wall jump carries them ~3 m out
from the face. Landing: GREEN; apex: over 9.5 m. A person who finds a better stick than the probe's might land it: E's checker should fly this
family (sent to the lead).

## Changes from the sheet above (as built)
- THE LADLE (3.3) is lx 12..20, lz 12..28 with its ramp lz 28..44 (the sheet's lz 14..34 left no room for an 8 m ramp).
- THE SPOUT's pit runs 3 m back under the take-off floor with the stair in a west notch (K3's NIGHT38 shape: levelgen's pit recovery passes it;
  a stair along the pit's west end did not: 13 of 13 pit cells stuck).
- Lesson rects in the flight rooms are 12 m wide, not wall to wall (a rect over the noride liners found a grid cell on a liner's top: "NOT reachable").
