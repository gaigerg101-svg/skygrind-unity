# LEVEL 4 THE SPAN, n38: the beat sheet (NIGHT38 builder O2, card L4; written before any geometry)

n37-span's copied rooms s1-s7 (n33-span's, with their random boxes and lava) are gone. Five new rooms p1-p5, then NIGHT37's kept rooms
o1 THREAD LINE, o2 SWING ANCHOR and o3 THE CANYON (THE LOOM) at the SAME world coordinates (copied from Design/levels/n37/span.plan.json:
THE LOOM's anchors, ledges, stations and the Loom37 pilot read them). Generator: `Tools/night/gen-n38-span.cjs` (requires only n36-lib.cjs
and the n37 plan for o1-o3); file `Design/levels/n38/span.json`, id `n38-span`; the campaign plays it behind `design.n38.span38`
(levelFrom `n37-span` -> levelTo `n38-span`). THE LOOM's level side (Game/SurvivalSim.Loom37.cs) runs on n38-span too while span38 is on.

## Rules this sheet is held to
- One job per room (teach / practise / combine / test / boss), one verb; GG 2026-10-13: open roofed arenas where enemies appear on the
  line ahead, flight rooms where each gap opens only to the level's verb (the GRAPPLE), nothing random, no stop.
- Every solid carries `job` + `why` (n36-jobs); every raised spot enemies stand on has a climb route (rule 3).
- A GATE (D-6): the intended verb crosses with >= 10 % margin of its envelope, no other single verb or combo crosses. Here every gate is a
  VOID of 30 m (nothing to stand on; `respawnY` -3: a fall is back at the room's start, a share of health) with the landing 10 m HIGHER
  than the take-off. The grapple (design.n32.grapple32: range 40 m, 2 charges, one back per 4 s, a kill gives one back) crosses it in ONE
  pull: the ring 5 m over the landing, 3 m past its lip (35.6 m from the eye at the lip, under 0.9 x 40). Nothing else climbs 10 m: the
  best non-grapple apex is the THUNDERCLAP chain's 8.48 m (Design/levels/envelopes38.json, jump + double + boost x3, which also reaches
  60 m at the take-off's height: so the gate is a RISE, not a length). Builder E's `n38 gates` proves each gate on the real Mover.
- Round 1 (the first sheet had 56 m voids crossed by TWO pulls 4 m up): the checker found the boost chain within 5 % of crossing them and
  proves the grapple by one hook from the take-off, so the gates became one pull up a 10 m rise.
- No ride across: the room's side walls over a void carry a `noride` LINER (a 0.2 m slab on the wall, `noride: true`, dull, no cyan), and
  every landing's faces are `noride` (the boost chain reaches the face; a ride up it would skip the gate). No ride panel stands in a
  flight room.
- The canyon's rules start in level 4's flight rooms (`loomRoom37` rooms): a pull's arrival stops there (3 m/s up at most), a 0.8 s wall
  ride gives a charge back (o1's tower teaches it).
- Movement numbers used for spacing: walk 8 m/s, jump 1.5 m, dash 6 m (3 pips), held ride 10.45 m/s (a 10 m panel is about 1 s);
  grapple: Design/unity-config.json design.n32.grapple32 (read by the generator, which FAILS when a route pull is over 0.9 x range).

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge.

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1 | 4.1 STITCH YARD | 32 x 36 x 12, roofed | teach | the STITCHER (hold to stream) + the first pull up | 30-40 s |
| 2 | 4.2 FIRST GAP | 36 x 50 x 22, roofed | teach (flight) | look up and pull: one gate (30 m, 10 m up) | 3-8 s |
| 3 | 4.3 KNOT YARD | 40 x 40 x 14, roofed | practise | fight on the move: pulls between the balconies, kills give pulls | 40-60 s |
| 4 | 4.4 PULL LINE | 40 x 92 x 32, roofed | test (flight) | two rising gates, one charge each | 4-12 s |
| 5 | 4.5 SPOOL HALL | 36 x 36 x 14, roofed | combine | STITCHER + whip + pulls; the last fight before the rehearsals | 40-60 s |
| 6 | 4.8 THREAD LINE (kept, o1) | NIGHT37 | rehearse | pull, ride, pull | - |
| 7 | 4.9 SWING ANCHOR (kept, o2) | NIGHT37 | rehearse | the moving anchor, nail and tear | - |
| 8 | 4.10 THE CANYON (kept, o3) | NIGHT37 | boss | THE LOOM | 15-22 s bot band |

## 4.1 STITCH YARD (teach: the STITCHER, then the first pull)
- **Line:** spawn (0, 2) facing north > run over the STITCHER on the axis (0, 5) > three husks switch on AHEAD on the line (lz 16-20),
  nail them on the run > the second wave: two spitters on the WEST TERRACE (lx -16..-6, lz 20..36, 4 m up) and four swarmers by the exit >
  PULL to the ring over the terrace's front (-8, 9, 24) and land on it (or take the ramp) > drop off the terrace's east side > the exit
  (0, 34).
- **Solids:** the terrace (`line`), its ramp (lx -11, lz 10 -> 20, rise 4, `climb`: the walkers' and the player's way up), two cover
  blocks on the east half (`cover`: the husks' line bends round them; a STITCHER stream from behind one).
- **Enemies:** wave 1 3 husks on the route (0, 16), (4, 19), (-3, 20); wave 2 2 spitters on the terrace (-12, 30 @4), (-9, 34 @4) + 4 swarmers
  (8, 30), (12, 32), (4, 33), (12, 26).
- **On entry:** a wide lit yard, the STITCHER glowing on the axis, the terrace and its ring at the back left, the exit's cyan pool at the
  back right. **Reward:** the STITCHER (the room's award, also a pickup). **Text:** "STITCHER: HOLD TO STREAM." (husksDead 3),
  "{grapple} PULL TO THE RING." (reached the terrace).

## 4.2 FIRST GAP (teach, flight: one gate)
- **Line:** spawn (0, 2) > the entry pier (lx -4..4, lz 0..8) > its lip (0, 8) > jump, LOOK UP, PULL to the ring (0, 15, 41) > drop onto
  the landing (lx -6..6, lz 38..46, 10 m up) > the exit (0, 48 @0: a 10 m drop off the landing's north end to the door).
- **The gate:** void lz 8..38 (30 m), landing +10 m (`gate {verb grapple, id p2-gateA}`; n38 gates PASS, no other try near).
- **No enemies** (the pull is new: alone first). **Text:** "LOOK UP. {grapple} PULL." (reached the landing); the landing's cyan pool.

## 4.3 KNOT YARD (practise: pulls in a fight; kills feed the pulls)
- **Line:** spawn (0, 2) > husks rush from the middle (on the line) > two BALCONIES (west lx -20..-12 and east 12..20, lz 14..30, 5 m up),
  each with its ramp, spitters on them > four rings over the floor in a diamond at 10 m (0, 12), (-8, 22), (8, 22), (0, 32): pull ring to
  balcony to ring while the swarmers chase > the exit (0, 38).
- **Solids:** the balconies (`line`), their ramps (`climb`), a low central block (`cover`, 1.2 m: a jump, a STITCHER stream over it).
- **Enemies:** wave 1 4 husks (0, 18), (-4, 20), (4, 20), (0, 24); wave 2 2 spitters on the balconies (-16, 24 @5), (16, 24 @5) + 2 husks
  + 2 swarmers; wave 3 4 swarmers + 2 husks. Every kill gives a grapple charge back (design.n32.grapple32 killRefill).
- **Text:** "KILLS GIVE PULLS." (husksDead 6).

## 4.4 PULL LINE (test, flight: two rising gates)
- **Line:** spawn (0, 2) > entry pier lip (0, 8) > gate A: the ring (0, 15, 41) > PIER B (lx -6..6, lz 38..50, 10 m up) > run its 12 m to
  its lip (0, 50) > gate B: the ring (0, 25, 83) > PIER C (lx -6..6, lz 80..88, 20 m up) > the exit (0, 90 @0: the drop off the pier's
  north end, one way, into the next fight).
- **The gates:** A: void lz 8..38, landing +10; B: void lz 50..80, landing +10 (ids p4-gateA, p4-gateB; n38 gates PASS both, with and
  without ride38). One charge each: gate B spends the last one (a missed pull means a 4 s wait on pier B: the cost of a miss).
- **No enemies** (the test is the line). **Text:** "TWO CHARGES. TWO PULLS." (reached pier C).

## 4.5 SPOOL HALL (combine: the STITCHER, the whip, pulls; the last fight before the rehearsals)
- **Line:** spawn (10, 2) (p4's door) > a mixed wave on the line north-west > the EAST GALLERY (lx 0..18, lz 22..30, 4 m up, its ramp at
  lx 15) where spitters stand > two rings at 10 m: (6, 16) in front of the gallery (pull up level with the spitters, whip one off) and
  (-6, 22) over the west lane > a 2 m wall across the west lane splits the husks > the exit (-7, 34) toward THE THREAD LINE (o1), whose
  door is at world x -17. (Round 1: a wall-to-wall gallery hid the exit and the route graph found no way down behind it.)
- **Enemies:** wave 1 4 husks + 2 swarmers on the line; wave 2 2 spitters on the gallery + 3 husks; wave 3 4 swarmers + 2 spitters.
- **Reward:** a health pickup on the gallery's middle (before the rehearsals). **Text:** "{grapple} AT AN ENEMY: WHIP." (husksDead 4).

## 4.8-4.10 kept (NIGHT37 O): o1 THREAD LINE, o2 SWING ANCHOR, o3 THE CANYON (THE LOOM). Unchanged geometry, rows and doors.

## The checks
1. `node Tools/night/n36-jobs.cjs Design/levels/n38/span.plan.json` GREEN (every solid a job + why).
2. The generator's reach check: every route pull under 0.9 x the grapple range.
3. Builder E's `n38 gates` on every `gate` tag: the grapple crosses (>= 10 % margin), nothing else does.
4. The walk bot (`Bot/Walk38Span.cs`, harness `n38span walk`) through p1-p5; THE LOOM's level check on n38-span (`n38span loom`).
