# LEVEL 5 THE SPIRE, n38: the beat sheet (NIGHT38 builder C2, card L5; written before any geometry)

Level 5 is rebuilt for flight. The NIGHT33 rooms v1-v8 that n37-spire copied are gone. Eight new rooms take their place, then come the three
NIGHT37 rooms, kept as they are: v9 FALLING STONES, v10 THE RING, and v11 THE SPIRE (THE CROWN). The level's verb is the **jump-dash**
(dash, then jump inside the dash; VERBS row 7). The level's weapon is the **TORCH** (VERBS row 33). The TORCH is picked up in the first
room, so every fight on the level can use it. Until NIGHT38 the TORCH was handed over only at the spire's walk-in.

- Generator: `Tools/night/gen-n38-spire.cjs`, then `bash Tools/night/n38c-regen.sh`. It writes `Design/levels/n38/spire.plan.json`, id
  `n38-spire`. Rooms v1-v8 come from `n36-lib.cjs`. Rooms v9-v11 are built by the same function as n37-spire's
  (`gen-n37-spire-boss.cjs` `addBossRooms`), so their plan rows are identical (checked).
- The swap: `design.n38.spire38` (levelFrom `n37-spire` -> levelTo `n38-spire`; N38Blocks bit 5). The same bit turns on the **gate meter**
  (below), and only in a room whose row carries `flight38` / `meter38`.
- Movement numbers: `PortExport/config.json` and `design.stamina`. Envelopes come from builder E's `Design/levels/envelopes38.json` once it
  lands. Until then: the real sim's run jump reaches 7.08 m on the flat, the jump-dash 21 m, the jump-dash apex is 1.85 m (Harness
  `crown37 jumps`), and the capsule is 1.8 m tall.
- Round 1 change (levelgen): **one gate per room**. levelgen's pit rule wants a miss to climb back to the room's spawn, so each gate has its
  own room, the pad is the spawn, and the door before it is the gate's checkpoint. Every edge also sits on a whole metre (the nav grid's
  cells are centred on half metres), so the gaps are 17 m and 18 m, not 17.5 m.

## The gate room (used four times: v3, v4, v5, v7)
A room 12 m wide and PAD + gap + 7 m long, with five parts:
1. The **pad**: lz 0..10, the room's spawn. It holds the gate meter (`meter38`), lit by a cyan pool.
2. The **gap**: a dry pit 5 m deep, the full width of the room. A miss costs the climb back up a recovery stair, never health. The stair is
   three steps of 1.25 m along the takeoff face, each 3 x 3 m, all within 3 m of the edge.
3. The **ceiling**: a slab from 1 m before the edge to the far side of the gap. Its underside is 4.0 m up (the jump-dash apex 1.85 m + the
   1.8 m capsule + 0.35 m). It caps the double jump's and a jump's rise. Its front edge is lit white, so the gate reads.
4. **Liners**: 1 m thick, on both side walls over the gap, tagged `noride` (E's / M's flag): a dull wall that can't be ridden.
5. The **landing floor**: 7 m to the door.

The room row also carries:
- `flight38: true`
- the LIMIT `pistolOnly`, which uses Campaign24's room limit with the banner "ONE JUMP-DASH PER PAD". The owned list comes back at the
  door, so the TORCH and the THUNDERCLAP return. The THUNDERCLAP's mid-air boost (-aim x 10 m/s) would otherwise add 10 m/s of carry.
- `routeVia` across the gap (levelgen's route graph).

The ceiling slab carries the gate tag `gate {id, verb: "jumpDash", from, to, gap, meter: 1}` for E's checker.

**The gate meter** (spire38; numbers in `design.n38.spire38.meter`):
- Standing on a pad sets the stamina bar to exactly one jump-dash's cost: perGate x (`design.stamina.boost.cost` +
  `boost.dashJumpExtra`) = 0.75 of the 1.0 bar. The bar is set down to that as well, never more.
- In a `flight38` room the bar does not refill by itself. Only the pad fills it.
- In a `flight38` room the grapple is off (the grapple35 pull goes to any surface within 40 m, so it would cross every gap). It comes back
  at the door.
- So each gap gets exactly one jump-dash, and nothing can be spammed gap to gap. Without the meter, a full bar pays for a jump + two air
  dashes (7 + 6 + 6 m), which comes close to the jump-dash's 21 m. With the meter, the best no-jump-dash line is a jump + one dash + a
  half dash (about 16 m).
- The event `spire38Meter` {x, y, z, bar} fires when a pad fills the bar (for GG's sound). The HUD's stamina bar shows the 0.75.
- **The gap:** at least 10 % under the jump-dash's reach (D-6: at most 18.9 m), and over every line that doesn't use it (run jump, double
  jump, slide-hop, jump + dash). E's checker (`n38 gates`) has the final word, and only gates it passes ship.

## Rooms
Coordinates are room-local: `lx` m east of the centre line (every room is centred on x = −10, as v9 is), `lz` m north of the room's south
edge.

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1 | 5.1 THE LANDING | 14 x 22 x 9 | teach (calm) | the TORCH: pick up, burn up close | 10-15 s, no threat |
| 2 | 5.2 THE NAVE | 30 x 38 x 14 | practise (open roofed arena) | TORCH + jump-dash through a fight | 40-60 s |
| 3 | 5.3 BELFRY I | 12 x 34 x 9 | teach / test (gate room, 17 m) | the jump-dash, taught on the gate | 5-15 s |
| 4 | 5.4 BELFRY II | 12 x 35 x 9 | test (gate room, 18 m) | jump-dash | 5-10 s |
| 5 | 5.5 BELFRY III | 12 x 35 x 9 | test (gate room, 18 m) | jump-dash | 5-10 s |
| 6 | 5.6 THE CHOIR | 34 x 42 x 16 | combine (open roofed arena) | jump-dash between three heights, TORCH close, parry | 50-70 s |
| 7 | 5.7 THE LONG BELL | 12 x 35 x 9 | test (gate room, 18 m) | the last gate before the rehearsals | 5-10 s |
| 8 | 5.8 THE VESTRY | 12 x 12 x 6 | rest | a breath, health, the checkpoint | 5 s |
| 9-11 | v9 FALLING STONES, v10 THE RING, v11 THE SPIRE | as NIGHT37 | rehearse, rehearse, boss | as NIGHT37 | as NIGHT37 |

## 5.1 THE LANDING (teach, calm)
- **Job / verb:** take the TORCH and burn something with it.
- **Line:**
  1. Spawn at (0, 2) facing north. The TORCH stands on the axis at (0, 6) at 1.0 m, in view from the spawn. Walk onto it.
  2. Three stand-ins stand in a tight row at (−1.5, 11), (0, 12) and (1.5, 11). Two short walls (lx −7..−3 and 3..7, lz 9..14) make an
     alcove, so they are met up close. Burn them.
  3. The door is at (0, 20).
- **Enemies:** none. This is the calm start: the stand-ins don't fight back.
- **On entry:** a white light over the TORCH and an amber light over the stand-ins (the room's one accent). The exit pool is lit cyan.
- **Reward:** the TORCH (a `weaponPickup`, `weapon: "torch"`; Campaign24 gives it through the loadout once).
- **Text:** "THE TORCH: HOLD {fire} CLOSE", shown until the player passes the alcove.

## 5.2 THE NAVE (practise: an open roofed arena)
- **Job / verb:** fight on the move: the TORCH up close, the jump-dash to close the distance. The arena is open and the exit is visible
  from the entrance.
- **Space:**
  - The nave floor runs up the middle.
  - Two side aisles are decks 2.4 m up (lx −15..−9 and 9..15, lz 6..32), each with a ramp at both ends (0.4 m per metre over 6 m). These
    are the walkers' climb route.
  - Four pillars stand in the nave (±5, 12) and (±5, 24) as cover, floor to roof.
  - The roof is 14 m up. That makes three floor heights at most (floor, aisles, and the jump-dash's arc).
- **Line:**
  1. Spawn at (0, 2).
  2. Run the nave north. Each wave appears AHEAD on your line (nodes at lz 14-30), so you meet it moving.
  3. Jump-dash from the nave up onto an aisle to reach the spitters. Burn them on the deck.
  4. Drop back into the nave and take the exit at (0, 36).
- **Enemies (arena36 flow; nodes on the route ahead):**
  - Wave 1: 3 husks and 2 swarmers in the nave.
  - Wave 2: 2 spitters on the aisles plus 2 husks.
  - Wave 3: 1 heavy and 3 swarmers.
- **On entry:** the nave's white ceiling lights lead north, with an amber trim on the west aisle. The exit pool is lit cyan at the far end.
- **Reward:** health on the north end of the east aisle (the high ground). The doors open on the last kill.

## 5.3 BELFRY I, 5.4 BELFRY II, 5.5 BELFRY III (the flight rooms, one gate each)
- **Job / verb:** the jump-dash gate, three times in a row, each behind its own door (the checkpoint). BELFRY I's gap is 17 m, with the
  text "{dash} DASH, THEN {jump}: FLY" shown until the landing is reached. BELFRY II and III are 18 m.
- **Line:**
  1. Spawn on the pad (0, 2). The meter fills.
  2. Run north. Dash at about lz 8.5, jump inside the dash, and fly under the ceiling.
  3. Land on the far floor and take the door.
- **Enemies:** none. These are flight rooms.

## 5.6 THE CHOIR (combine: the big roofed arena)
- **Job / verb:** everything the level has: the jump-dash between three heights, the TORCH close, and the parry (spitters' yellow globs).
- **Space:**
  - The floor at 0.
  - The raised dais in the middle (lx −6..6, lz 14..26, 1.5 m), with ramps on its south and north sides.
  - Two side galleries at 2.4 m (lx −17..−11 and 11..17, lz 6..36), each with a ramp at both ends (the walkers' route up).
  - Pillars as cover at (±8, 6) and (±8, 36).
  - The roof is 16 m up.
- **Line:**
  1. Spawn at (0, 2).
  2. Up onto the dais.
  3. Jump-dash from the dais's edge onto a gallery (5 m across, 0.9 m up). Burn the spitters there.
  4. Back down, then the exit at (0, 40).
- **Enemies:**
  - Wave 1: 4 husks and 2 swarmers.
  - Wave 2: 2 spitters on the galleries plus 2 husks and 1 heavy.
  - Wave 3: 2 heavies and 4 swarmers.
- **Reward:** health on the dais. The doors unlock on the last kill.

## 5.7 THE LONG BELL (the last gate)
- An 18 m gate room, the last test of the jump-dash before FALLING STONES and THE RING rehearse it for THE CROWN.

## 5.8 THE VESTRY (rest)
- **Job:** a breath before the rehearsal rooms and THE CROWN. No enemy. Health in the middle, the checkpoint, and a cyan pool at the door
  (x = −10, the door d8 into v9).

## The checks this sheet is held to
- `node Tools/night/n36-jobs.cjs Design/levels/n38/spire.plan.json`: every solid has a job and a why, and every perch has a climb. Round 1:
  GREEN.
- levelgen: only the known ERROR row, v11's chasm (the fall rule by design). Round 1: that row only.
- E's `n38 gates`: every gate PASSES (the jump-dash crosses with >= 10 % margin, and no other line crosses). A gate that fails is
  re-spaced or cut, never shipped.
- The row `spire38` is turned on only when every gate is GREEN.
