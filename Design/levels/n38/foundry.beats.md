# LEVEL 1 THE FOUNDRY, n38: the beat sheet of the two new rooms (NIGHT38 builder F, card F1; written before any geometry)

Level 1 keeps n37-foundry's rooms a1-a5 (GG: "the tutorial ... clearly incentivized the movement"), its rest a6 and THE WICK's rooms a7-a9
exactly as built (DECIDE38 D-4). Two rooms are new, placed after the CRUCIBLE (a5, the level's first "combine") and before the rest:

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1-5 | a1 INTAKE .. a5 CRUCIBLE | kept (Design/levels/n36/foundry.beats.md) | teach / practise / combine | | |
| 6 | **b1 1.6 THE YARD** | 36 x 40 x 12 | combine, in the open | KESTREL from the walls, rides between fights | 40-60 s |
| 7 | **b2 1.7 THE GANTRY** | 20 x 77 x 14 | test (a flight room) | the ride CHAIN (two gates no jump crosses) | 20-35 s |
| 8 | a6 1.8 REST | kept | rest | | 5 s |
| 9-11 | a7 THE FLUE, a8 PIERCE, a9 THE STACK | kept (NIGHT37 W) | teach / boss | | |

Generator: `node Tools/night/gen-n36-foundry.cjs --n38` (the stack and the n37 rooms are W's and untouched: the flag only inserts b1 / b2 and
renumbers the display names after them); plan `Design/levels/n38/n38-foundry.plan.json`; level `Design/levels/n38-foundry.json`;
the campaign plays it behind `design.n38.foundry38` (levelSwap `n37-foundry` -> `n38-foundry`). Regenerate: `bash Tools/night/n38f-regen.sh foundry`.

## Rules this sheet is held to (NIGHT36's, plus NIGHT38's)
- One job per room; every solid has `job` + `why` (n36-jobs.cjs); a walkers' climb to every raised enemy spot.
- GG 2026-10-13: "wide open arenas with a roof"; "you should be flying through and ... killing the enemies before you enter the next room";
  "they can't be too exploitable"; the pace must not stop at a room. So: the YARD is open (36 x 40 m, roof 12 m, three floor planes at most,
  the exit seen from the door), its enemies switch on AHEAD on the line you ride; the GANTRY's gates sit beyond every jump's envelope.
- A gate is proven, not eyeballed (NIGHT38 method 1, D-6): each gap is tagged `gate: {verb, from, to}` for builder E's checker
  (`n38 gates`); only a gate the checker passes ships, and the row turns on only then.
- Spacing numbers: Design/levels/n35-ride-distances.json (ride 10.45 m/s held; planes 3-12 m apart chain; a pitch-0 jump-off 4.84 m out,
  ~5.5 m along) and PLAN.md's first envelope guesses (jump 6.9 m, jump-dash 21 m, slide-hop 12.8 m) until E's envelopes38.json lands; the gap
  is one constant in the generator (GANTRY_GAP), re-set from the measured table.

## b1 1.6 THE YARD (combine, in the open: KESTREL + ride walls, enemies on the route)
- **Job / verb:** the first OPEN arena: everything a1-a5 taught (fire, charge, ride, parry, launch) in a wide roofed yard, fought on the move.
  The line runs up the middle on two long ride walls; the fight comes to the line.
- **Line:** spawn (0, 2) > run north up the centre lane > ride WALL W's east face (plane lx -4, lz 8..24: 16 m, about 1.5 s held) firing the
  KESTREL from the wall (a charged shot pierces the husk line in the lane) > jump off across onto WALL E's west face (plane lx +4, lz 20..34;
  planes 8 m apart, in the measured chain list) > off onto the floor at (0, 36) > the exit (0, 38). The floor under the walls is dry: a missed
  ride costs nothing but the line.
- **Space:** three planes: the floor (0), two side GALLERIES (2.5 m: lx -18..-12 and 12..18, lz 12..32) where spitters stand, each with a
  RAMP at its south end (the walkers' climb, 2.5 m over 6 m) and a health pickup on the west one; the ride walls' tops (7 m) are not a plane
  (no one stands there). The exit is on the floor, its cyan pool seen down the lane from the door.
- **Enemies (arena36 flow; nodes on the route, AHEAD of the line):** wave 1: 3 husks in the lane ahead (0, 14) (-1.5, 18) (1.5, 22): a charged
  shot from the door or the wall pierces them. Wave 2: 2 spitters on the galleries (-15, 22 @2.5) (15, 24 @2.5) + 3 husks from the lane's far
  end (0, 30) (-8, 28) (8, 28): ride through them. Wave 3: 2 spitters (-15, 28 @2.5) (15, 18 @2.5) + 4 husks on the line (0, 26) (-2, 32)
  (2, 32) (-8, 16): the last kill opens the door on the far side of the line.
- **On entry:** the two cyan-trimmed walls in a line up the middle (the route), the galleries' amber lights left and right (where the
  spitters will be), the exit's cyan pool past the walls at the far end, white overheads down the lane.
- **Reward:** a health pickup on the west gallery (high ground pays); the doors unlock on the last kill (0.15 slow).
- **Text:** none (every verb is known); the room name only.

## b2 1.7 THE GANTRY (test: the ride chain; a flight room of two gates)
- **Job / verb:** the ride as the only way across: two gaps longer than any jump the level owns (jump, double jump, dash, jump-dash,
  slide-hop: the KESTREL has no boost), each spanned by FOUR ride walls whose faces alternate west / east (a chain: no wall-jump chain gets
  across, it ends after 3 in the air). A fall costs a climb back to the gate, never health.
- **Line:** spawn (0, 2) on the takeoff floor (lz 0..5) > jump at the lip onto R1's east face (plane lx -4) > jump off across onto R2's west face
  (plane lx +4) > R3 > R4 > off onto LEDGE L1 (lz 35..41; the checkpoint) > gate 2 the same way (R5..R8) > the landing floor (lz 71..77) > the
  exit (0, 75). With ride38 (a held ride keeps its height and the look drifts it) a ride that sags drifts back up by looking up.
- **Gaps:** gate 1 lz 5..35, gate 2 lz 41..71 (GANTRY_GAP = 30 m each, wall to wall of the floors): each a dry PIT 4 m deep. The walls stand
  from the pit floor to the roof (no top to stand on). Pit 1's recovery: steps up the west wall at its south end to the takeoff floor. Pit 2's:
  steps up to L1, and from L1 a drop back into pit 1 (one way: the room's spawn is always reachable, levelgen's pit check; L1 is never
  reached on foot from the start).
- **Enemies:** none (a test of the verb, not a fight; GG: "fast pace platforming / flying around").
- **On entry:** the gantry's line of eight cyan-trimmed walls marching away over the dark pits, L1's cyan pool halfway, the exit's pool at the
  far end (the goal in sight, Celeste's rule).
- **Gates (for E's checker):** gate 1: verb `ride`, from the takeoff lip (0, 0, lz 4) to L1 (0, 0, lz 37); gate 2: from L1's lip (0, 0, lz 40) to
  the landing (0, 0, lz 73). PASS needs the ride chain across with >= 10 % margin and no jump / dash / jump-dash / slide-hop / wall-jump chain.
- **Text:** at the lip: "HOLD {jump} ON THE WALLS. CHAIN THEM." (lesson `reached` on L1).

## Round 2 changes (measured, after builder E's checker landed: Design/levels/n38/GATES38.md)
- The `gate` tag sits on each LANDING deck (L1, the far floor), `runUp` 6: the takeoff floor and L1 are 8 m deep, the far floor 10 m (a chain
  arrives at up to ~20 m/s and must land before the door).
- Two walls per gate, not four: W1 (west, its east face) from 6 m back along the takeoff floor to 11 m short of the landing, W2 (east)
  overlapping its end to the far floor. E's held-ride tries ride the first wall until 1-12 m remain to the landing; four 12 m walls whose
  first ended 17 m out read "ride: does NOT cross" though the walk bot crossed them 3 / 3.
- One lesson (the far floor, prompt "HOLD {jump}: CHAIN THE WALLS."): a ride flies over L1 straight into gate 2, and a `reached` lesson on
  L1 needs the feet grounded there.
- The recovery steps on the EAST wall (W1 stands in the west half).
- Checker: the ride crosses +11 % (+15 % with --ride38); the best try without a ride -7 %; RED only on two slide-hop + double tries that
  rode both walls (`rides 2` in --verbose): E's classifier judges a try by its planned atoms. With `uses |= atom == "ride" && rides > 0`
  (probed locally) both gates are GREEN (+18 %, best other -69 %).

## Checks (the slice is done when)
1. `node Tools/night/n36-jobs.cjs Design/levels/n38/n38-foundry.plan.json` GREEN; the n36 and n37 plans regenerate byte-identical.
2. levelgen 0 errors (Reports/LEVEL-n38-foundry-validate.md).
3. E's gate checker GREEN on gate 1 and gate 2 (`n38 gates`); then `design.n38.foundry38.enabled` true.
4. The walk bot (`n38f walk foundry`, Bot/Walk38Foundry.cs): b1 cleared by the autopilot, b2 crossed by the scripted chain, on 5 seeds.
