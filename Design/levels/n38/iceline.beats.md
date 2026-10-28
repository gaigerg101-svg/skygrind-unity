# LEVEL 2 THE ICE LINE, n38: the beat sheet (NIGHT38 builder S2, card S2; written before any geometry)

NIGHT37's level 2 (`n37-iceline`) opened with n35-iceline's rooms i1a..i7 copied as they were: GG 2026-10-13, "old rooms that look very
ugly with just random boxes and lava ... the rooms are too tight ... you're encouraged to just start shooting immediately". They go.
New rooms j1..j4 come first, then NIGHT37's two rehearsal rooms, the walk-in and THE CANAL, kept as NIGHT37 built them (PLAN.md section
2 L2: "KEEP s1-s4").

Generator: `node Tools/night/gen-n37-iceline-boss.cjs --n38` (the same file writes n37-iceline without the flag; the s rooms are one
code path for both); file `Design/levels/n38/iceline.plan.json` -> `Design/levels/n38-iceline.json`, id `n38-iceline`; the campaign plays
it behind `design.n38.iceline38` (levelSwap `n37-iceline` -> `n38-iceline`). Regen: `bash Tools/night/n38s2-regen.sh`.

## Rules this sheet is held to
- One job per room, one verb, a line you can draw (n36 method). Every solid carries `job` + `why` (n36-jobs.cjs GREEN).
- Open roofed arenas (PLAN.md 2: 24-40 m across, 10-16 m roofs, at most three floor heights, the exit seen from the entrance) with
  enemies switched on ON the line ahead, so the fight starts while you move (Hakita: "no points where you wait"; Doom's push-forward).
- One FLIGHT room (`flight38`): a chain of gates, each forcing the level's verb, the goal in sight, a fall costs a climb and returns you
  to the gate. Every `gate` passes builder E's checker (`n38 gates`, D-6: the verb crosses with >= 10 % margin, no other verb or combo
  the level owns crosses). A gate the checker cannot judge (the skate gate: the checker's box world has no ice) is proven by this
  builder's harness (`iceline38 skate`) against E's measured envelope, and says so.
- Level 2 owns (`owns38`): jump, double, dash, slide, ride, walljump. Not the THUNDERCLAP boost (level 3), not the grapple (level 4).
- Every number below comes from `Design/levels/envelopes38.json` (builder E, measured on the real sim; ride38 off) or the config:
  the best ground reach without a boost is 18.83 m (jump + dash + double; 20.14 m to a landing 2 m lower); facing walls chain at plane
  gaps 3-11 m and miss at 12+; a pitch-0 ride jump-off goes 4.89 m out (10.38 with the double). Skating: the canal's pumped lap average
  21.2 m/s (design.n37.stag37.slip.base, harness `stag37 speeds`); ice glide decays 12 %/s (design.n30.glide).
- Ride walls are 5 m tall (THE CANAL's lane walls under stag38, the same family); walls beside a skate gate are `noride` (dull, no
  cyan trim) so only the speed crosses it.

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge (n36-lib). `@y` = height.

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1 | 2.1 THE GAUNTLET | 26 x 30 x 10 | teach | the PULSE gauntlet: a tank dies to one punch | 25-35 s |
| 2 | 2.2 THE RINK | 36 x 40 x 12 | combine (arena) | skate the ice loop, ride the island, punch the tanks | 40-60 s |
| 3 | 2.3 THE RUN | 16 x 176 x 12 | test (flight) | skate gate, ride gate, ride chain | 25-40 s |
| 4 | 2.4 REST | 10 x 10 x 5 | rest | a breath, health | 5 s |
| 5-8 | s1 2.8 LANES, s2 2.9 SLIPSTREAM, s3 2.10 THE GATE, s4 2.11 THE CANAL | NIGHT37's | rehearse x2, link, boss | THE STAG | 60-90 s |

## 2.1 THE GAUNTLET (teach: the PULSE gauntlet; an open roofed arena)
- **Job / verb:** the level's weapon. Grab the gauntlet at the door; a TANK (shrugs off the KESTREL) dies to one punch.
- **Line:** spawn (0, 2) > the GAUNTLET pickup on its plinth light at (0, 4.5) > the tank switches on ALONE in the middle of the floor at
  (0, 16), walking at you: punch it > wave 2 comes off the north deck (lz 24..30, 2.0 m) down its two ramps (lx -8 and 8, lz 19 -> 24):
  husks with a tank behind them > ride the west or east panel (lx -9 / 9, lz 8..22, 5 m tall) past them to the deck > the exit (0, 28).
- **Space:** 26 m wide, 10 m roof: room to circle the tank and to ride over a pack; the two ride panels are the line over the floor.
- **Enemies:** wave 1: one tank at (0, 16) (alone: it is seen from the door, and it is slow). Wave 2: 3 husks + a tank on the deck
  (raised nodes at lz 27), down the ramps (the climb route: n36-jobs rule 3).
- **On entry:** the gauntlet's plinth lit cyan under the first light, the tank's spot lit amber in the middle, the deck at the back with
  its two ramps, the two panels' cyan trims either side, the exit's cyan pool on the deck.
- **Reward:** the exit; a health pickup on the deck (the high ground).
- **Text:** at the door: "GRAB THE GAUNTLET. PUNCH THE TANK." (lessons31: the weapon row reads it); then "THE TANK DIES TO ONE PUNCH".

## 2.2 THE RINK (combine: skate, ride, punch; an open roofed ice arena)
- **Job / verb:** everything the level owns at once, on ice. A skate loop round a central island; the island's long sides are ride walls.
- **Line:** spawn (0, 2) onto the ice > the loop is anticlockwise (east lane north, the north end west, the west lane south): skate up the
  east lane (lx 9..18) pumping dashes > ride the island's east face (lx 6, lz 10..30, 5 m tall) over the husks on the lane > the north
  end > back down the west lane > the exit (14, 38) at the NE corner, seen from the door straight up the east lane.
- **Geometry:** the whole floor is ice. The ISLAND (lx -6..6, lz 10..30, 5 m tall, flat top): its east and west faces are ride panels
  (cyan), the loop's inside. One corner perch at the NW (lx -18..-12, lz 34..40) at 1.5 m with a ramp down to the west lane (the
  spitters' stand and their climb route).
- **Enemies (on the line ahead):** wave 1: 4 husks on the east lane ahead of you (nodes lz 18, 24, 30, 34): they meet you head on while
  you skate. Wave 2: 2 husks + a tank at the north end (nodes lz 34-36): the tank on the ice is the punch target. Wave 3: 2 spitters on
  the NW perch + 2 husks on the west lane: ride the island's west face past the globs (parry them back).
- **On entry:** the east lane's long run of ice under white lights, the island's east trim beside it, the exit's cyan pool beyond at the
  NE corner, the perch's amber light at the far left.
- **Reward:** the doors unlock on the last kill; a health pickup on the island's north end.
- **Text:** "SKATE THE LOOP. RIDE THE ISLAND." (6 words).

## 2.3 THE RUN (test, flight: the skate gate, the ride gate, the ride chain; `flight38`)
- **Job / verb:** the level's flight room: three gates in a row, each forcing one verb; the exit's light in sight from the first lane.
- **The run-up (lz 0..56):** a 56 m ice lane (lx -5..5) from the door: pump dashes to the skating speed.
- **G1 THE SKATE GAP (lz 56..80, 24 m):** a void (a dry pit 4 m deep) between the lane's lip and a dry deck (lz 80..92). Intended: SKATE:
  the lane's speed carried into a jump (+ the double, + the dash). Nothing else reaches: the best ground try is 18.83 m (E's envelope),
  24 m is 27 % past it. The void's side walls are `noride` hulls (lx -8..-5 and 5..8, roof high): no ride along them. Proven by
  `iceline38 skate` (the real sim on the level file: a pumped skater lands, a walking start from the lip does not).
- **G2 THE RIDE WALL (lz 92..120, 28 m):** a void from the deck's lip; a 5 m ride wall along its west side (face east at lx -4, lz 90..122)
  to the next dry deck (lz 120..130). Intended: RIDE (jump at the wall, hold, ride the 28 m, jump off onto the deck). The best ground
  reach is 18.83 m (34 % short). The east side is `noride`. Tagged `gate {verb: ride}` on the deck: E's checker judges it.
- **G3 THE CHAIN (lz 130..170, 40 m):** a void with three short ride walls, alternating sides, 8 m apart plane to plane (in E's chain list
  3-11 m): W1 (face east at lx -4, lz 128..144), E1 (face west at lx 4, lz 140..156), W2 (face east at lx -4, lz 152..170). No wall alone
  spans the void; the room's own walls are lined with `noride` hulls along it; the chain (ride, jump off, re-press onto the next) is the
  only crossing. Landing: the exit deck (lz 170..176). Tagged `gate {verb: ride}`.
- **Falls:** every void is a dry pit 4 m deep with a stair at its south end back up to the gate's takeoff (a fall costs a climb, never
  health; you retry the gate at once).
- **Enemies:** none (changed while building: enemies switched on at a room's entry would be 120 m away across voids they cannot cross;
  the flight room tests movement only, the two arenas before it carry the fights).
- **On entry:** the lane's white lights in a row to the first lip, the cyan trims of the ride walls beyond, the exit's cyan pool high at
  the far end (the goal in sight).
- **Text:** "SKATE. DASH OFF THE ICE." at the lane (5 words).
- **Measured while building (round 1):** G1 is a skate gap in the glide's sense, not a top-speed gate: a dash begun on the ice adds
  design.n30.glide.dashAdd (Core/Glide30.cs), so a jump + ice dash + double from only 6 m of ice reaches 34.1 m and the pumped lane
  28.3 m (harness `iceline38 gates`); every try from this lip is on ice, so the gate's "other verbs" are the same verbs off dry ground
  (E's envelope 18.83 m, 22 % short of 24 m). G2 PASS by E's checker (ride +31 %, the best other try -21 %). G3: E's ride try releases
  only near the landing, so it never chains (RED by the checker, its best other try -19 %); the chain is flown by `iceline38 gates`
  (the real Mover rides W1, E1, W2 and lands on D3).
- **Round 3 (E's stricter checker, port/stage3 198ae14d: the Mover's dash jump and the PULSE blast are owned tries; dash jump + double
  37.44 m, dash jump + blast 57.26 m / 60.77 m 2 m lower):** geometry only. G2 and G3's voids are now 64 m (the best other try's 2 m-lower
  reach + 5 %, read from envelopes38.json); G2's wall runs the whole void; G3's three walls are 25.3 m each, overlapping 4 m, W2 6 m over
  a 10 m D3. `n38 gates`: G2 PASS +20 % (best other -11 %), G3 PASS +15 % (best other -11 %); with --ride38 both +15 %. G1 stays 24 m as a
  flow beat, NOT a D-6 gate (the dash jump and the blast clear it off dry ground). Walk 10/10.

## 2.4 REST (rest)
- A breath before THE STAG's rehearsals. No enemy. A health pickup in the middle; white light, a cyan pool on the door.

## s1-s4 (kept: NIGHT37's, the same generator code)
- s1 2.8 LANES (rehearse the lane change under spitter fire), s2 2.9 SLIPSTREAM (the pacer), s3 2.10 THE GATE (the walk-in),
  s4 2.11 THE CANAL (THE STAG; stag38: 5 m lane walls in 30 m runs, the island's ride trims, the read pass).

## The checks
1. `node Tools/night/n36-jobs.cjs Design/levels/n38/iceline.plan.json`: GREEN (every solid a job + why; no lava in this level).
2. `levelgen`: 0 errors (the walk proofs, the routes, the doors, the lights).
3. `n38 gates Design/levels/n38/iceline.plan.json` (E): G2 and G3 GREEN; `iceline38 skate` (S2): G1 GREEN.
4. The walk bot through j1..j4 (`Bot/Walk38Iceline.cs`), then the s rooms as NIGHT37's.
