# LEVEL 1 THE FOUNDRY, n36: the beat sheet (NIGHT36 builder L, card L1; written before any geometry)

The level is new from the first room: no n29-n35 room, generator or prop survives (PLAN.md section 3, REVIEW.md root causes 7-8).
Generator: `Tools/night/gen-n36-foundry.cjs` (requires only `n36-lib.cjs`); file `Design/levels/n36/foundry.json`, id `n36-foundry`;
the campaign plays it behind `design.n36.level36` (levelSwap `n35-foundry` -> `n36-foundry`).

## Rules this sheet is held to
- One job per room (teach / practise / combine / test / rest / boss), one verb, a movement line you can draw (design-web.md rule 1).
- 10-20 s of no threat at the start; a new enemy first alone or as a pair, in a tight space, seen before it engages (rule 3).
- Every solid carries `job` (line / cover / climb / hazard / marker / reward) and `why`; no decor solids (no dress pass).
- Every raised surface an enemy can stand on has a walkers' climb route (rule 2, GG NIGHT20); every lava pool borders a place enemies
  walk or stand, within a launch's reach (lava burns enemies to death: Core/Lava30.cs; launching them into it is the point).
- Arena flow (n36 rooms only, ULTRAKILL's, PLAN.md table): entering a fight room locks its doors red; enemies switch on 1 s later,
  0.1 s apart, each in a spawn bubble with a light; the next wave 1 s after a wave dies; the last kill slows time to 0.15 for a moment;
  the doors unlock.
- Movement numbers used for spacing (PortExport/config.json + Design/levels/n35-ride-distances.json): walk 8 m/s; jump 1.5 m
  (a running jump clears about 5 m); dash 6 m (24 m/s x 0.25 s); slide collider 1.0 m (a 1.2 m ceiling forces it); a held ride
  10.45 m/s (a 10.5 m panel is about 1 s); a pitch-0 jump-off lands 4.84 m out; facing panels 3-12 m apart chain (we use 5 and 8).
  With wallgrip36 (M) a ride never lifts: decks reached from a ride are at most 1.2 m above the floor.

Coordinates: room-local, `lx` metres east of the room's centre line, `lz` metres north of its south edge (n30-lib's convention, copied
into n36-lib). Paths are `(lx, lz)` points; `@y` is a height above the floor.

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1 | 1.1 INTAKE | 12 x 24 x 6 | teach (calm) | move, look, jump, slide, dash | 10-20 s, no threat |
| 2 | 1.2 KESTREL | 14 x 20 x 7 | teach | fire (tap), charge (hold, pierces) | 20-30 s |
| 3 | 1.3 RIDE | 16 x 26 x 9 | practise | wall ride over a floor, then over lava | 20-30 s |
| 4 | 1.4 PARRY | 8 x 16 x 6 | teach | the parry (punch the yellow glob) | 15-25 s |
| 5 | 1.5 CRUCIBLE | 20 x 24 x 10 | combine | ride + parry + launch into lava | 40-60 s |
| 6 | 1.6 REST | 10 x 10 x 5 | rest | (a breath, health) | 5 s |
| 7 | 1.7 HALL | 5 x 12 x 5 | link (reveal build-up) | walk in | 3 s |
| 8 | 1.8 THE LANTERN | to builder B's spec | boss (the exam) | ride + parry | 15-22 s bot |

## 1.1 INTAKE (teach, calm)
- **Job / verb:** the calm start. Move, look, jump, slide, dash, each once, no enemy, no lava.
- **Line:** spawn (0, 2) facing north > walk to (0, 6) > JUMP up the 1.0 m step (lz 6..10, full width) > drop to the floor (lz 10) >
  SLIDE through the slot (lz 12..17, 4 m wide, 1.2 m ceiling) > run to the lip (0, 18) > JUMP + DASH over the 6 m trench (lz 18..24
  minus the far landing; 1.5 m deep, a stair back up on its east side) > the door (0, 23).
- **Enemies:** none (no threat; the door is open from the start).
- **On entry:** a narrow lit hall: the step's cyan edge strip at eye height 6 m ahead, the slot's low lit mouth beyond it, a cyan pool
  on the far floor past the trench (the goal light). Room name card, then `l1.dash`'s and the slot's lines at their spots.
- **Reward:** the KESTREL room's door opens as you land (the view of the stand-in line through it).
- **Text:** the step: "MOVE. LOOK. {jump} JUMP." (lesson); the slot mouth: "HOLD {slide}: SLIDE UNDER" (lesson); the trench lip: teach35
  `l1.dash`.

## 1.2 KESTREL (teach: fire, then charge)
- **Job / verb:** the KESTREL. A tap fires on the press (tap36); a hold charges and the charged shot pierces.
- **Line:** spawn (0, 2) > stand (0, 4) and fire at the three stand-ins in a row on the axis (0, 9) (0, 11) (0, 13): one charged shot
  pierces all three > the husk pair comes down the ramp from the north deck > back-pedal, tap-fire > walk the floor to the exit (3, 18).
- **Enemies:** wave 1: 2 husks (the first enemies, a pair) on the north deck (lz 15..20, 2.0 m, lx -7..-1): they switch on at the deck
  and come DOWN its ramp (lx -4, lz 15 -> 10, the climb route; a walker's 0.4 m per metre) toward you. The deck is reachable on foot by
  the same ramp.
- **On entry:** the three stand-ins lined up on the axis under a white ceiling light (the line a charged shot pierces), the deck and its
  ramp lit at the back left (where the husks will be), the exit's cyan pool on the right.
- **Reward:** the RIDE room's door; the first kills (no award: level 1 is the KESTREL only, design.n31.unlocks).
- **Text:** teach35 `l1.fire` at the door (lz 0..4), `l1.charge` at lz 4..7 (its stand-in line).

## 1.3 RIDE (practise: the wall ride over a floor, then over lava)
- **Job / verb:** the wall ride, practised twice: first over a safe floor, then over lava.
- **Line:** spawn (0, 2) > run NW to (-3.4, 2) > jump onto panel P1's EAST face (P1: lx -5..-4.4, lz 2..12.5, over the dry floor) >
  ride north to lz 12 > jump off east (4.84 m) onto panel P2's WEST face (P2: lx 0.6..1.2, lz 12..23; planes 5.0 m apart) > ride north
  over the lava > jump off onto the north floor (lz 23..26) > exit (0, 24). The ride is also reachable straight from the floor: run to
  P2's south end (0, 11) and jump onto its west face.
- **Lava:** lx -8..6, lz 13..22 (14 x 9 m, surface -0.8): the ride's hazard and the husks' grave. The long way round is the BRIDGE, a
  2 m dry strip along the east wall (lx 6..8): the only walk route between the floors.
- **Enemies:** wave 1: 2 husks switch on at the north floor (-4, 24.5) and (4, 24.5) and walk toward you over the bridge (their only
  route; flat, no climb needed): on the bridge they are 0-2 m from the lava, so a punch from a dash or a slide launches them in (launch36).
  Shooting them works too.
- **On entry:** P1's cyan trim stripe straight ahead-left over a dry floor; past it, P2's stripe over the lava's glow; the bridge along the
  right wall; the exit pool on the far floor.
- **Reward:** the far floor's cyan pool; a health pickup on the north floor beside the exit (a ride that ends in the lava costs health).
- **Text:** at the door: "HOLD {jump} on a WALL to RIDE. Let go to leap." (lesson); on P1: teach35 `l1.zoom` (aim on a wall slows time).

## 1.4 PARRY (teach: the parry; one spitter alone in a tight space)
- **Job / verb:** the parry: punch a yellow glob and it flies back and explodes (parry36).
- **Line:** spawn (0, 2) > step to (0, 4): the spitter on its perch at the far end (0, 14 @1.5) shoots > its glob flashes yellow (tells36) >
  PUNCH it > it returns and bursts on the spitter > climb the perch's stair (west side) or walk past to the exit (2.5, 14.5).
- **Space:** 8 m wide, 16 m long, 6 m high: no room to strafe a glob, no cover between you and the perch (the one way out is the punch).
- **Enemies:** wave 1: ONE spitter alone, on the perch (lx -2..2, lz 12..16, 1.5 m), seen from the door before it fires. Wave 2: a pair
  of spitters on the perch's two ends (-3, 14) (3, 14) (a stair each side: lx -4..-2 and 2..4, lz 9..12, the climb routes).
- **On entry:** the perch straight ahead, lit from above with a yellow pool (yellow = punch it), the spitter on it, the bare floor between.
- **Reward:** a parry heals and refills a dash (parry36); the door opens on the second wave's last kill.
- **Text:** at the door: "PUNCH {punch} THE YELLOW GLOB. IT FLIES BACK." (lesson; the text names the button).

## 1.5 CRUCIBLE (combine: ride + parry + launch)
- **Job / verb:** everything at once: ride over the lava under spitter fire, parry the globs, punch husks off the rims into the lava.
- **Line:** spawn (0, 2) > slide up the west rim (lx -9..-6) > jump onto panel W's EAST face (W: lx -4.6..-4.0, lz 6..16.5, standing in the
  lava) > ride north, punching the yellow globs from the deck > jump off onto the north deck (lz 19..23, 1.2 m) > dash-punch the husks on
  the rims into the lava > back across on panel E's WEST face (E: lx 4.0..4.6, 8 m from W, plane to plane) going south.
- **Lava:** lx -6..6, lz 6..17 (12 x 11 m, surface -0.8), the room's centre. The RIMS (lx -10..-6 and 6..10, lz 0..19) are the husks' only
  way round: every rim cell is within 4 m of the lava (a launch's reach).
- **Enemies:** wave 1: 2 husks (one per rim, from the deck's ends (-8, 21) (8, 21), down the rims). Wave 2: 1 spitter on the deck (0, 21 @1.2)
  + 2 husks. Wave 3: 2 spitters on the deck (-4, 21) (4, 21) + 1 husk. Climb route to the deck: a ramp at each end (lx -8 and 8, lz 17 -> 19.5,
  rise 1.2), down onto the rims.
- **On entry:** the lava's glow across the middle with the two panels' cyan stripes standing in it, the deck at the back with its two
  ramps, the rims either side.
- **Reward:** a health pickup on the deck's middle (the high ground you ride to); the doors unlock on the last kill (0.15 slow).
- **Text:** none new (every verb is known); the room name only.

## 1.6 REST (rest)
- **Job:** a breath before the boss. No enemy. A health pickup in the middle; the checkpoint. White light, a cyan pool on the door.
- **Line:** spawn (0, 1.5) > the pickup (0, 5) > the door (0, 8.5).

## 1.7 HALL (the walk-in)
- **Job:** the build-up: a short hall (5 x 12 m) from the rest's door to THE LANTERN's door. The boss hall's light shows at its end.
- **Line:** (0, 1) > (0, 11) > through the door into the boss hall. No cut, no Respawn (walkin36, builder B).

## 1.8 THE LANTERN's hall (boss: the exam)
- **Job:** the final exam of the level's two verbs: the wall ride (its core is reachable best from the walls) and the parry (its orbs are
  yellow; parried back they crack its shield).
- **Geometry:** to builder B's hall spec (B sends it by 0:40). Placeholder until then: 24 x 24 x 14, entered from the south door, a ring of
  ride panels (cyan stripes) round a central lit core, a lava floor cycle owned by B's sim.
- **Enemies:** the boss only; doors lock behind you on entry (B's reveal: it arrives in view).
- **Reward:** the level clear.

## The checker (`Tools/night/n36-jobs.cjs`) fails the level when
1. a box, ramp, tunnel wall or solid prop has no `job` in {line, cover, climb, hazard, marker, reward} or no `why`;
2. a lava pool has no enemy node or enemy route cell within the launch reach of its edge;
3. a fight room has a raised surface (above 0.6 m) where an enemy stands (a node) or that it must reach, with no `climb` ramp / stair in the room.
