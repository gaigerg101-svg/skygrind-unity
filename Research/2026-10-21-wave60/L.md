# NIGHT60 builder L: B4 level 4 step 1, THE GAP NEST's slot split (row design.n58.span58, OFF)

Worktree from port/stage3 at 5e4e716d, 23:23-00:5x. span58 stays OFF; Design/unity-config.json untouched. Every number below is on the level
built with `node Tools/night/gen-n58-span.cjs --slots=p2`, levelgen `--walk`, `--finish`; that build is NOT committed (n58-span.json, its
StreamingAssets copy, Design/levels/n58/span.plan.json and Reports/LEVEL-n58-span-validate.md stay the NEW = [] build: `--check` "none"
beyond the names, and `git diff` empty after the default rebuild). p2 stays out of NEW.

## The split (gen-n58-span.cjs slotP2; the model: gen-n57-kiln.cjs slot DE)
n52's link p2 (x -18..18, z 37.5..87.5, 50 m door to door) -> the approach link **p1b 4.1b THE RIGGERS' WALK** (x -4..4, z 73.5..87.5, 12 m
tall, d1 where n52's is, no pieces) + the door **d1b** (x -2..2, z 69.5..73.5, checkpoint) + the room **p2 4.2 THE GAP NEST** (x -15..13,
z 37.5..69.5; d2 where n52's is). The card kept: the near deck (the door's, y 0, z 63.5..69.5), n52's gate A landing kept whole as the far
deck (y 10; its gate's `from` moved to the new lip z 64), THE GAP (30 -> 14 m), the side gantry (ramp 10 m over 17.5 m on the east wall), the
winch (R5), the work lamp, the waves (w1 drone 2 + husk 3, w2 drone 3 + swarmer 4), no ride wall (the pull the one fast line, DECIDE59 D-10).
pull38's lip, fallReturn43's rects, routeVia, the first proof and frames36.mid follow the lip. The generator asserts the long side <= 32 m,
density >= 2.5, every new piece's card field + why, and a roof over the drones' hover (cut 3).

## The cuts (contract: `OUT=Logs/n60/L/contract/n58-span-p2 bash Tools/night/n59-l-contract.sh n58-span p2`; flows: n46flow --level n58-span)
| number | NIGHT59 c3 (before) | cut 1 | cut 2 | cut 3 |
|---|---|---|---|---|
| room (m) | 36 x 50 x 22 | 23 x 32 x 16 | 28 x 32 x 16 | 28 x 32 x 22 |
| contract C n/m | 11/14 (1 size; 2 density; 5 ride wall) | 12/14 (5 ride wall; 8 bot clears) | 12/14 (5 ride wall; 8 bot clears) | **12/14 (1 size: height 22; 5 ride wall)** |
| density | 0.72 (13 pc) | 2.85 (21 pc) | 2.68 (24 pc) | 2.68 (24 pc) |
| contract clears A / B | 10/10 / 10/10 | 9/10 / 9/10 | 3/10 / 4/10 | 10/10 / 10/10 |
| room table p2 (R2 R5 R8 IN VIEW I1-I4) | PASS | IN VIEW FAIL (3.3 of 5) | PASS (5.0 of 5) | PASS (5.0 of 5) |
| levelgen errors (n52's build 22) | 21 | 20 | 20 | 20 (p2's pits 5 -> 3; enemies reach p2 PASS; completable p1b / p2 PASS; spawn31 PASS) |
| level walks A / B | 10/10 / 10/10 | 7/10 / 7/10 | 5/10 / 4/10 | **10/10 / 10/10** |
| level stall A / B | 12.1 / 13.3 % | 32.2 / 25.4 % | 32.6 / 29.4 % | 15.1 / 13.3 % |
| deaths A / B | 0 / 1 | 0 / 0 | 0 / 0 | 0 / 0 |
| put at the next room A / B | 0 / 0 | 0 / 1 | 1 / 0 | 1 (p2) / 2 (both p3) |
| p2 falls A / B | 51 / 70 | 40 / 50 | 38 / 32 | 55 / 76 |
| p2 clear A / B | 35.4 / 35.4 s | 34.7 / 63.1 s | 47.6 / 68.8 s | 31.1 / 37.0 s |

Files: contract outputs Logs/n60/L/contract-c1, contract-c2 and (cut 3, the backlog's OUT) contract/n58-span-p2; flows
Logs/n60/L/flow-n58-span-p2-c<n>-<A|B>.txt; room tables room-n58-span-c<n>.txt; levelgen reports LEVEL-n58-span-validate-c<n>.md.
Planted: `bash Tools/night/n59-l-contract.sh n58-span planted` FAILS (trusted) on cut 1 and cut 3 (p1 without its health and pads, 7/9).
- Cut 1 (the split, 23 m wide, 16 m roof): size and density PASS; the gantry's foot first at z 69 (0.5 m off the entry wall) left every
  far-deck node unreached by walkers (levelgen "enemies reach every tier: p2" ERROR): its foot moved to z 67 before any number. 8 of 40 seed
  runs capped at 240 s in p2's wave 2, the bot at the exit strip (x 0-2, z 38.5) and one drone left over the decks.
- Cut 2 (28 m wide: the west void beside the far deck 4 -> 9 m, the exit strip's sight line; wave nodes on the decks' front edges z 48-48.5
  for IN VIEW; three pieces: the sheave block, the far deck's two landing bollards): IN VIEW 3.3 -> 5.0 of 5; caps 8 -> 24 of 40.
- **The cause (cuts 1-2):** 37 of the 40 drones left at the caps were at y 15.4 = the 16 m roof less the drone's 0.6 m radius. Drone.cs's
  targetY is the ground below + hover (drone.hoverMin 4 .. hoverMax 8 m): over the 10 m decks that is 14-18 m, into a 16 m roof; a drone
  whose climb probe meets the roof stops moving (Director20's roof check / climb stall, a NIGHT20 block: whether it is on in the campaign
  is not checked; the drones stayed pinned for the cap's 240 s). n52's 22 m roof clears
  13 (the lamp post) + 8 + 0.6 = 21.6 m.
- Cut 3 (the roof back to n52's 22 m): walks 10/10 both sets, the contract's 8 bot clears PASS; size FAILS on the height alone.

## MAIN and GUARDS (fixed in NIGHT60-BACKLOG.md B4 before any number)
- MAIN p2 contract 14/14: **not met, 12/14.** Not reachable as written with the card: (1) 5 ride wall (a ride wall >= 17 m is a C item;
  the card keeps the pull the one fast line: GG's call, DECIDE59 D-10), (2) 1 size's height (10-16 m) against drones over a 10 m deck (the
  cause above: a 16 m roof capped 32 of 80 seed runs). The long side (32 m) and the density (2.68) pass. Planted run: FAILS (trusted). Walk
  10/10 / 10/10.
- GUARD span stall <= 14.5 % + band (n52-span 14.5 / 13.8 %, band 0.7: <= 15.2 %): 15.1 / 13.3 % PASS (A by 0.1).
- GUARD deaths in band (n52-span 0 / 2): 0 / 0 PASS.
- GUARD put at the next room <= 1 per set: 1 / 2, **B FAILS**. Cause (a worktree-only one-line print in Harness.N46.Flow.cs after
  `linkSkips++`, not committed; Logs/n60/L/putat-c3.txt): A's one is p2 (a cleared room not left in 15 s), B's two are p3 (n52's room,
  NIGHT59 L's cause: the bot arrives from a fight with seed-dependent end positions; NIGHT58 EXT's build had 3 / 0, all p3).
- GUARD p2 falls within the void rooms' range (A p3 55 / p5 42, B p3 78 / p5 57): 55 / 76 PASS.

## Not checked / notes
- Any Player frame (B5's), verbs37, identity beyond the room table's I1-I4, the colour grade of the new link p1b (unity-config's per-room
  grades have no p1b key: not checked), the `n38 gates` on p2-gateA's new from.
- n52-room-check.py's 8 bot clears prints "0/0 seeds" when n46flow's row reads "(9/10, cap 1)" (its regex wants ")" after k/n): the item
  still FAILs, as it should, but its numbers read 0/0.
- The calls this leaves for GG / the lead: a ride wall that is not a way across THE GAP (on the far deck's side) or the contract's ride wall
  item waived for this card; a height band that admits a flyer room over a 10 m deck (>= 22 m here), or a drone that does not stop under a
  roof (Director20's NIGHT20 roof check; a sim change for every drone: not built, not checked), or a lower far deck (it breaks n52's teach
  gate kept whole).

# NIGHT60 builder L: B14 kb3c's tap-hood pocket (n57-kiln, kiln57 ON; branch n60-b14 from port/stage3 abd32586, 00:38-02:0x)

The cause (the lead's B6, NIGHT60-LOG.md; Logs/n60/b6-kiln-B-perseed.txt): a closed 3.4 x 4 m pocket at the tap run's head, x 7..10.4,
z -233.5..-229.5, between THE TAP HOOD (x 2..7, floor to roof), the door's east liner, THE RUN WALL (x 10.4) and the lava. On n46flow's 1 m
planner grid (Harness.N46.Flow.cs Plan: lava cells and body-height solids + 0.4 m blocked) its cells x 7.5..9.5 / z -233..-230 (and x 10.5
north of the run wall) have no free neighbour: NO PATH until the 15 s rule puts the bot in kb3d.

Every number: `dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n46flow --level n57-kiln --seeds 10 --first <1|11|21> --per-seed
--trace --no-geo` (Logs/n60/L/b14/<base|c1>-<A|B|C>.txt), with a worktree-only line printing each put-at's seed and room after
`linkSkips++` in Harness.N46.Flow.cs (not committed; the file is unchanged in every commit). NO PATH lines are split by where the bot
stood: the pocket (z > -233.5) or the lava rect.

## Cut 1: THE TAP HOOD 2 m narrower on its east side (x 2..7 -> 2..5)
The pocket opens on foot onto the east floor through x 5..7 (grid cells x 5.5 / 6.5 free); the hood keeps its job (the foothold, R5: the
room table's box [2, 0, -234.5]..[5, 12, -230.5] PASS), its place beside the door and its south face (the ring's north crossing); the run
wall (the ride) untouched.

| seeds | NO PATH kb3c (pocket / lava) before -> cut 1 | put at (all kb3c) | kb3c deaths | n57-kiln stall | walks |
|---|---|---|---|---|---|
| 1-10 | 1012 (923 / 89) -> 0 | 1 -> 0 | 1 -> 1 | 20.3 -> 20.6 % | 10/10 |
| 11-20 | 1591 (1329 / 262) -> 30 (0 / 30) | 2 -> 1 (seed 20) | 6 -> 4 | 22.9 -> 22.0 % | 10/10 |
| 21-30 | 2171 (1932 / 239) -> 376 (0 / 376) | 5 -> 1 (seed 22) | 1 -> 2 | 22.7 -> 21.3 % | 10/10 |

- The pocket's lines are 0 on every set. What is left: the bot over the lava after the clear (feet at or above -0.2 m, so the pilot's lava
  rule is off and the detour plans from a lava cell whose neighbours all burn): seed 20 (B, 30 lines, the run's middle), seeds 22 (38)
  and 24 (338, the run's SOUTH end, x 8-10, z -247.5..-243) on C.
- The put-ats left are not the pocket: seed 20 stalls at (-7.5, -253.0) on its path's cell 18 / 20 at the door d6b's west jamb (the path to
  kb3d's spawn at x -7.5 hugs the 4 m door's west edge, after a blow-pipe launch); seed 22 stalls at (2.5, -238.0), path cell 4 / 21, at
  the furnace's north-east corner.
- Deaths: every kb3c death but one is at (10.0, -0.8, -247.2) (before and after): the run's south end, where the pilot's lava rule picks
  the nearest edge (south, 0.3 m) and the spout ledge's 3.3 m face stands there: a second trap, not the pocket.
- GUARDS on cut 1: kb3c deaths over seeds 1-30 8 -> 7 PASS (set C 1 -> 2); stall <= 23.65 % per set PASS; the kb3c contract 14/14 PASS
  (Logs/n60/L/b14/contract-c1.txt: 22 x 24 x 12, 2.84 / 100 m2, ride 18.0 m, dwell 1.10 -> 0.85 s, hold min 10 hits over 4 spots, pads 2/2;
  the before file was not found under Logs/n57-n59: kb3c is in gen-n57-kiln.cjs's NEW, whose rule is the contract 14/14); n59-l-room
  table kb3c PASS (R2 0 dead-end cells of 150, was 148; IN VIEW vista 6.0 of 6; I1-I4 PASS; Logs/n60/L/b14/room-c1.txt); levelgen 0 errors.
- MAIN on cut 1: NO PATH 1591 -> 30 and put-ats 2 -> 1 on seeds 11-20: not 0 / 0 (the pocket's share is 0 / 0; the rest above).

## Cut 2: the tap run's south end 2 m short of the spout ledge (pit z -247.5..-233.5 -> -245.5..-233.5; its west rim the same)
The run's south end was a corner of lava, THE RUN WALL and the spout ledge's 3.3 m north face: the pilot's lava rule steers to the nearest
edge (there the south one, 0.3 m), into the face, and the bot burned there (every kb3c death but one, before and on cut 1). Now a 2 m
floor strip (x 7..10.4, z -247.5..-245.5) lies between the lava and the face, open to the east floor; the run wall's ride and its landing
on the ledge are untouched (contract ride 18.0 m); THE TAP RUN 4 x 14 -> 4 x 12 m.

| seeds | NO PATH kb3c before -> cut 1 -> cut 2 | put at (kb3c) | kb3c deaths | n57-kiln stall | level deaths | walks |
|---|---|---|---|---|---|---|
| 1-10 | 1012 -> 0 -> **0** | 1 -> 0 -> **0** | 1 -> 1 -> **0** | 20.3 -> 20.6 -> 20.9 % | 1 -> 1 -> 0 | 10/10 |
| 11-20 | 1591 -> 30 -> **0** | 2 -> 1 -> **1** (seed 14) | 6 -> 4 -> **0** | 22.9 -> 22.0 -> 21.5 % | 7 -> 5 -> 1 | 10/10 |
| 21-30 | 2171 -> 376 -> **0** | 5 -> 1 -> **0** | 1 -> 2 -> **0** | 22.7 -> 21.3 -> 20.4 % | 1 -> 2 -> 0 | 10/10 |

- MAIN (seeds 11-20): NO PATH 1591 -> 0 met; put-ats 2 -> 1 NOT met. Seeds 1-10 and 21-30 no worse (NO PATH 0 / 0, put-ats 0 / 0).
- The put-at left (seed 14) is a third cause, outside the pocket and the run: on its path to kb3d's spawn the bot steps on THE BLOW PIPE
  (pad (-6.5, 0, -251), 2.5 m north of d6b's mouth), is thrown onto the fettlers' walk, comes back down past its path's next waypoint and
  stands at (-7.5, 0, -253.0) at d6b's west jamb on "path 22/24" for 15 s (cut 1's seed 20 did the same: "path 18/20"). The pilot only
  advances a waypoint it comes within 1.2 m of (Harness.N46.Flow.cs, F1Input), so a launch past one leaves it steering at a cell it stands
  in. Not changed here (the blow pipe's place is its own card field): a new backlog line for the lead.
- GUARDS on cut 2: kb3c deaths over seeds 1-30 8 -> 0 PASS; stall <= 23.65 % per set PASS (20.9 / 21.5 / 20.4); the kb3c contract 14/14
  PASS (Logs/n60/L/b14/contract-c2.txt, OUT Logs/n60/L/contract/n57-kiln-kb3c-c2: dwell 1.10 -> 0.00 s, deaths 0, hold min 10 hits over
  4 spots, pads 2/2, ride 18.0 m); n59-l-room table kb3c PASS (Logs/n60/L/b14/room-c2.txt; IN VIEW 5 / 5 rooms); levelgen 0 errors;
  walks 10/10 x 3. Not checked here (the lead's, in the Player): verbs37 n57-kiln 31 / 31, a157 in view kb3c >= 3 / 6.

# NIGHT60 builder L: B13 kb2c's respawn faces a wall (n57-kiln, kiln57 ON; branch n60-b13 from port/stage3 2771de00, with B14, 01:46-)

The cause (the lead's B1, NIGHT60-LOG.md; frames Logs/feel/n60-b1/l0spawn/a157/shot-N29-L57-n57-kiln-kb2c-{spawn,entry}.png): the respawn
(-2, 0, -168) faces yaw 45 (south-west) at the west wall and THE NEAR BRIDGE's face 2-4 m off. NIGHT58 EXT (Research/2026-10-21-wave58/
L0-ext.md) chose that yaw because spawn31 (LevelValidate29.cs: every node >= 6 m from the spawn; a node inside the spawn's first view cone,
half 58.26 deg round its yawDeg, >= 12 m) fails facing the fight: wave 1's four middle-channel nodes sat 10.8-12.4 m from the spawn. The
spawn's place itself (out of n46flow's 4 m door circle; on the straight walk line from kb2b's exit (0, -157.5) through d4) is kept.

## Cut 1: wave 1's nodes 2 m south, the spawn facing them
- waveNodes43[0] (and nodes): (0.5, -178.5) (-1, -180) (1, -180) (1.5, -179) -> (0.5, -180.5) (-1, -180.75) (1, -181) (2, -180.5): still
  on the middle channel's ash between the bridges (north of the far ramp's edge z -181.5), now 12.75-13.3 m from the spawn.
- spawn yawDeg 45 -> -10 (down the middle channel, at wave 1's centroid; LevelValidate29 / n46-flow-geo: +yaw turns toward -x).
- levelgen 0 errors; spawn31 PASS (42 nodes; nearest 9.96 m is another room's, as before).
- MAIN proxy: n46-flow-geo.cjs (B12's yaw-aware count) kb2c "w1 in view" 1 / 4 -> 4 / 4 (Logs/n60/L/b13/geo-base.txt, geo-c1.txt).
  The MAIN itself (>= 2 of wave 1 seen in the Player's spawn frame, a157 -skygrindSpawnShots60) is the lead's: not checked here.
- Room table (Logs/n60/L/b13/room-c1.txt): kb2c PASS (vista 5.0 of 5 from the mast's flanks; w1 entry 5.0 of 5, nearest 13.5 -> 15.5 m),
  kb3 PASS, IN VIEW 5 / 5, I5 PASS (kb2c, kb3c), I7 PASS.

| seeds (n46flow --per-seed --trace --no-geo) | put at, per set (<= 5) | deaths (pooled <= 9.9) | stall (<= 23.65 %) | walks | kb2c stall / dwell / clear |
|---|---|---|---|---|---|
| 1-10 before (B14 cut 2, Logs/n60/L/b14/c2-A.txt) -> cut 1 | 0 -> 0 | 0 -> 2 (kb3c 1, kb3d 1) | 20.9 -> 18.9 % | 10/10 | 22.4 -> 17.1 % / 3.5 -> 2.8 s / 42.5 -> 34.0 s |
| 11-20 | 1 -> 1 (kb3c) | 1 -> 1 (kb3d) | 21.5 -> 21.6 % | 10/10 | 18.5 -> 19.4 % / 1.7 -> 3.3 s / 37.5 -> 39.9 s |
| 21-30 | 0 -> 1 (kb3c) | 0 -> 0 | 20.4 -> 18.6 % | 10/10 | 19.3 -> 17.0 % / 5.8 -> 3.6 s / 39.9 -> 33.1 s |

Deaths pooled 1 -> 3 (in band); none in kb2c. The put-ats are kb3c's (B15's blow pipe or not: not traced).
- Contracts (`OUT=Logs/n60/L/contract/n57-kiln-b13 bash Tools/night/n59-l-contract.sh n57-kiln kb2c,kb3`; Logs/n60/L/b13/contract-c1.txt):
  kb2c C 14/14 PASS (dwell 0.70 -> 0.65 s, grace 0.5; deaths 0; hold min 5 hits over 4 spots; pads 1/1; ride 17.0 m); kb3 C 14/14 PASS
  (dwell 1.10 -> 1.80 s, grace 0.8, as NIGHT58 EXT's cut 4; deaths 0).
- All GUARDS pass on cut 1; one cut was enough. Not checked here (the lead's, in the Player): the spawn frame (a157 -skygrindSpawnShots60:
  >= 2 of wave 1 seen), verbs37 n57-kiln 31 / 31, a person's restart facing.

# NIGHT60 builder L: B15 kb3c's blow pipe launches the bot past d6b (n57-kiln, kiln57 ON; branch n60-b15 from port/stage3 33ef339f, with B13)

## The cause (four traces, n46flow --per-seed --trace with the worktree-only put-at print)
Seeds 20 (B14 cut 1), 14 (B14 cut 2), 17 and 27 (B13's level, Logs/n60/L/b13/c1-B/C.txt): kb3c cleared, the bot walks its grid path to
kb3d's spawn (-7.5, -262; the path's last cell clamps to (-7.5, -253) at d6b's mouth); from the east floor the path crosses THE BLOW PIPE
(pad (-6.5, 0, -251), radius 1 m, 2.5 m north of d6b's mouth): the bot is thrown onto the fettlers' walk ((-7.1, 5.0, -248.5),
(-9.1, 7.0, -247.2), (-10.4, 5.0, -247.7) in the air), comes down the walk ramp past its next waypoint and stands at (-7.5, 0, -253.0) by
d6b's west jamb on "path 21/23" (the pilot advances only a waypoint it comes within 1.2 m of) until the 15 s rule. The pad sat on every
line out of the room: the west lane's (the loop proof down x -6.5) and the east floor's (along z -250..-252).

## Cut 1: THE BLOW PIPE onto the furnace top's north-east corner
pad (-6.5, 0, -251) -> (0, 2.5, -238.5), target (-9.5, 4.5, -240) -> (-9.5, 4.5, -236), apexAbove 2.5 kept. Its job kept (the upward line
onto the fettlers' walk, the high ground level with the drones); the furnace top is off every floor line (the planner blocks the 2.5 m
furnace), 2.06 m from the furnace ramp proof's top (-2, 2.5, -239) and 2.24 m from the tap vent's landing (-1, 2.5, -240.5). No floor spot
in the far west is off the exit lines: the west lane carries the loop proof and the north half's way out, the strip south of the walk
ramp its foot's traffic, the south strip the east half's way out.
- levelgen 0 errors; kb3c's player route 2 -> 2.25 s (walk); room table kb3c PASS (unchanged), IN VIEW 5 / 5, I5 / I7 PASS.
- MAIN: kb3c put-ats 0 / 1 / 1 -> **0 / 0 / 0** on seeds 1-30 (the level's put-ats 0 / 0 / 0; NO PATH lines 0 / 0 / 0).
- GUARDS: kb3c deaths 1 / 0 / 0 -> 1 / 0 / 0 (the one is seed 9, 24.0 s in, at (10.0, -0.8, -237.8) in the tap run mid-fight: the same
  death at the same step on B13's level, from B13's shift; B14's level had 0 / 0 / 0); stall 18.8 / 18.8 / 17.9 % (<= 23.65); level
  deaths 1 / 0 / 0; walks 10/10 x 3; the kb3c contract 14/14 PASS (`OUT=Logs/n60/L/contract/n57-kiln-b15 ...`: dwell 1.10 -> 0.35 s,
  pads 2/2: the blow pipe launched, apex 7 m, landed at (-9.51, 4.5, -236), 0.01 m off; hold min 10 hits over 4 spots).
- Not checked here (the lead's): verbs37 n57-kiln 31 / 31, a157 in the Player, a person's read of a pad on the furnace (the card's
  Molecule row named the blow pipe "from the far west corner onto the furnace"; the generator's was onto the walk).
