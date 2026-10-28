# NIGHT56 builder L (levels): notes

Worktree branch `worktree-agent-abb2a0239ec4a05ed`, based at the shared commit 6b1c00c9. No Editor, Player, build or reel run by this builder:
every number below is a script / harness number from the worktree's plans and level files. Rooms are level data edited in place (no row;
the comparison side is Builds/n55-freeze).

## L1 the room checker (Tools/night/n56-room.py)
`python -I Tools/night/n56-room.py table` (the 18 rooms), `room <level> <room>`, `planted`, `--rev <rev>`. It imports n53-identity.py (I1-I4,
I5, I7 unchanged) and adds, per combat room:
- **R2 no dead ends:** the walk graph is the level file's navGrid nodes in the room's rect (lava surfaces out), kept to STANDABLE nodes (a
  same-height neighbour along x and along z: 1 m wall tops and ledge strips are not floor) in the strongly connected part that holds the
  commit feet (reach and return), plus the ride walls (I2's ends, both faces, both ways) and the pads (disc -> target) as edges; coarsened to
  2 m x 2 m x 0.5 m cells. A dead end is a cell with exactly one neighbour cell. The first cut (no standable / no SCC filter) found 1-6
  "dead ends" per room, all on wall tops, roofs (y 32) and pit bottoms (y -14): not floor. Bridges are printed, not gated.
- **R5 a foothold:** a free-standing box (not flush with the room's walls: liners are walls) of the room's zone, its top above the eye
  (body.eyeHeight 1.6 m), its footprint reaching 4-8 m inside the entry door (depth along the door's inward normal). The first cut accepted
  wall liners (kb2's east liner at x 11-12): rejected.
- **R8 every job:** each solid of the zone (n53-identity's solids: boxes >= 0.5 m, ramps) carries a non-empty `job` or `job36`.
- **IN VIEW per opening:** wave 1's nodes (waveNodes43[0], else nodes + highNodes as Campaign24.cs:913 / :323-324), each enemy's node by
  SpawnWave's own rule (a random start, then the first node >= minSpawnDist 12 m from the feet; the last try takes any), the feet = where the
  first wave fires (Committed43: the first point inside the door's edge that is commitM 3 m from the room's spawn). A node is seen when the
  web collider ray (the level file's collision boxes + wedges) reaches its body (+1.0 m) or head (+1.6 m), inside the 53.3 deg horizontal
  half-angle (camera.fovDeg 74 at 16:9). held / vista: expected enemies seen from the foothold's better flank (entry eye if no foothold) >= 4.
  ambushed / ambusher: every node in use 8-15 m from the door's centre, <= 15 m from midfield, not behind the foothold (the entry eye's ray
  does not pass through it), and no line into the entry corridor (blocked to an eye 3 m outside the door).
- **w1 entry (a156):** the expected enemies of wave 1 seen at the a452 entry frame (frames52.entry with its yaw and pitch: frustum + ray) and the
  nearest node: the number the a156 meter prints on the Player.

**A finding while building it (measured, not assumed):** minSpawnDist 12 m acts on the feet at the commit. Where the spawn stands < 3 m from
the room's edge, the feet are 3 m in and close nodes are skipped; in foundry a3 measured with the feet at spawn + 3 m (the first cut), all four
wave-1 enemies would land on the one node >= 12 m away. With the door-edge feet the odds are uniform there. L2 must place each wave's nodes in
one distance band from the feet, or SpawnWave piles the wave on the few far nodes.

**Planted cases** (`planted`, exit 0 = every check FAILs its plant): R2 iceline g2 @u3-n52 + a 3-cell spur FAIL (1 dead end at the spur's tip);
R5 foundry a3 @u3-n52 with its 2 footholds taken out FAIL; R8 kiln kb3 @u3-n52 with one job taken out FAIL (1 untagged of 17); IN VIEW
NIGHT52's own rooms unchanged: iceline g4 @u3-n52 (held) FAIL 1.0 of 1 seen, foundry a3 @u3-n52 (ambushed) FAIL 4 of 4 nodes 20-27 m from the
door with a line into the corridor. A PASS exists too (spire v6 held: 6.0 of 6 seen), so the check is not "FAIL everywhere".

### The RED table at 6b1c00c9 (before any room is touched)
| room | opening | R2 | R5 | R8 | in view | w1 entry (a156) | identity | verdict |
|---|---|---|---|---|---|---|---|---|
| kiln kb2 | vista | PASS 0 | FAIL | PASS 0 | FAIL 2.7 / 6 seen | 2.7 of 6 seen, nearest 13.0 m | FAIL I4 | FAIL R5,IN |
| kiln kb2c | ambushed | PASS 0 | FAIL | PASS 0 | FAIL 4 / 4 nodes fail | 0.6 of 3 seen, nearest 16.0 m | PASS | FAIL R5,IN |
| kiln kb3 | held | PASS 0 | PASS | PASS 0 | FAIL 2.2 / 5 seen | 2.2 of 5 seen, nearest 16.0 m | PASS | FAIL IN |
| foundry a3 | ambushed | PASS 0 | PASS | PASS 0 | FAIL 4 / 4 nodes fail | 4.0 of 4 seen, nearest 19.5 m | FAIL I1 | FAIL IN |
| foundry a5 | held | PASS 0 | FAIL | PASS 0 | FAIL 3.0 / 3 seen | 3.0 of 3 seen, nearest 14.3 m | PASS | FAIL R5,IN |
| foundry b1 | ambushed | PASS 0 | FAIL | PASS 0 | FAIL 3 / 3 nodes fail | 0.0 of 3 seen, nearest 28.3 m | PASS | FAIL R5,IN |
| iceline g2 | held | PASS 0 | PASS | PASS 0 | FAIL 0.2 / 1 seen | 0.5 of 1 seen, nearest 15.0 m | PASS | FAIL IN |
| iceline g3 | ambushed | PASS 0 | FAIL | PASS 0 | FAIL 1 / 1 nodes fail | 3.0 of 3 seen, nearest 15.6 m | FAIL I2 | FAIL R5,IN |
| iceline g4 | held | PASS 0 | FAIL | PASS 0 | FAIL 1.0 / 1 seen | 1.0 of 1 seen, nearest 18.5 m | FAIL I2 | FAIL R5,IN |
| iceline g5 | ambushed | PASS 0 | FAIL | PASS 0 | FAIL 8 / 8 nodes fail | 3.3 of 5 seen, nearest 15.3 m | PASS | FAIL R5,IN |
| iceline g7 | held | PASS 0 | FAIL | PASS 0 | FAIL 1.0 / 3 seen | 1.0 of 3 seen, nearest 12.1 m | PASS | FAIL R5,IN |
| iceline g8 | ambushed | FAIL 1 | FAIL | PASS 0 | FAIL 2 / 2 nodes fail | 0.5 of 1 seen, nearest 23.5 m | FAIL I2 | FAIL R2,R5,IN |
| spire v2 | ambushed | PASS 0 | FAIL | PASS 0 | FAIL 5 / 5 nodes fail | 3.0 of 5 seen, nearest 18.8 m | FAIL I2 | FAIL R5,IN |
| spire v5 | vista | PASS 0 | FAIL | PASS 0 | FAIL 3.2 / 4 seen | 3.2 of 4 seen, nearest 16.7 m | PASS | FAIL R5,IN |
| spire v6 | held | PASS 0 | FAIL | PASS 0 | PASS 6.0 / 6 seen | 6.0 of 6 seen, nearest 16.1 m | PASS | FAIL R5 |
| span p1 | vista | PASS 0 | PASS | PASS 0 | FAIL 2.0 / 3 seen | 3.0 of 3 seen, nearest 22.0 m | PASS | FAIL IN |
| span p3 | ambushed | FAIL 1 | PASS | PASS 0 | FAIL 4 / 4 nodes fail | 2.0 of 4 seen, nearest 17.0 m | PASS | FAIL R2,IN |
| span p5 | held | PASS 0 | FAIL | PASS 0 | FAIL 3.0 / 6 seen | 3.0 of 6 seen, nearest 15.5 m | FAIL I4 | FAIL R5,IN |

IN VIEW PASS 1 / 18; R5 PASS 6 / 18; R2 PASS 16 / 18; R8 PASS 18 / 18. I5 / I7 PASS on all five levels.

## L2 spawns in view (slice 1: 606d2c1f; the meter: b4f3b543)
`python -I Tools/night/n56-l-wave1.py` (written from the L1 plans; `bash Tools/night/n56-l-regen.sh` regenerates each level the way it was last
generated: kiln / foundry through gen-n52-*.cjs --finish, the others through n52-loop-regen.sh; `n56-l-leveldiff.py` shows only the edited rows
changed: waveNodes43 / waveNodes43_why, and waves / waves56_why in the three re-dealt rooms). Each room's wave 1 gets its own spots
(waveNodes43[0]; `r.nodes` untouched; later waves keep theirs): an original spot that passes is kept, a failing one is replaced by the nearest
passing floor spot (height kept where one exists), no two within 3 m. held / vista spots are seen from ONE foothold flank (the checker scores
the better flank) and in the a452 entry frame, >= 12 m from both feet; ambushed spots follow the ambush rule, < 12 m from both feet (one
minSpawnDist band: SpawnWave spreads the wave evenly).
- Re-dealt (the same enemies, nothing added or removed): foundry a5 [3, 2, 3] -> [5, 3], iceline g2 [1, 3, 3] -> [4, 3], span p1 [3, 6] -> [4, 5].
- Left failing: iceline g4 (one drone, then two: 3 in all) and g7 (wave 1 [tank, spitter, spitter] is at LevelValidate29's budget, 10 of 10
  points: a fourth body was 10.5, a level-report ERROR; and a 4-body wave 1 lets wave 2 overlap it, NIGHT46's g7 deaths 0 -> 8). Both get
  every wave-1 body in view (1 of 1, 3 of 3) and stay FAILs of the ">= 4" count.

| number | before (6b1c00c9) | after (606d2c1f) |
|---|---|---|
| n56-room.py IN VIEW | 1 / 18 | **16 / 18** (doneWhen >= 15) |
| I7 openings | PASS x5 levels | PASS x5 levels (openings53.json untouched) |
| level reports | - | kiln 0, foundry 0, iceline 0 errors; spire 1 / span 22 errors, all pre-existing (no content change in the tracked reports) |
| bot n46flow clear (sum of medians) | foundry 130.2, iceline 233.2, kiln 92.6, span 95.3, spire 141.0 s | foundry 122.6, iceline 220.1, kiln 92.2, span 85.0, spire 135.1 s (none worse: PASS) |
| bot n46flow stall | foundry 26.2, iceline 32.0, kiln 29.2 % | foundry 27.7, iceline 37.1, kiln 25.5 % (gate <= 25: **RED** on all three) |
| bot walks | 10/10 x5 | 10/10 x5 (GREEN) |

The stall rise is iceline g2 (33.0 -> 48.0 %: two wardens at once) and g5 (37.1 -> 59.0 %: the ambush now 7-10 m off, the bot fights where it
stands); kiln fell (kb2 / kb3 show their packs, 29.2 -> 25.5). Tried and reverted: the ambushed spots at the far end of the 8-15 m band (>= 12 m
from the feet): foundry 29.3 %, iceline 37.1 % (g3 / g5 have no spot there), kiln SHORT 9/10: worse. The baselines were already over 25 % on all
three levels; the walk pilot stands to fight an enemy inside ~10 m, so the ambush rule's 8-15 m and the stall gate pull against each other. A
sharper gate (proposal): stall no worse than the baseline + 3 points per level (foundry PASS, kiln PASS, iceline FAIL by g2 / g5).

**The a156 meter** (FeelReel.N56.Room.cs, dn-check-wt 0 errors): one line per fight room at a452's entry frame,
`[reel] a156: <room> entry <file> | wave1 <n> | alive <a> | seen <s> | nearest <d1> | second <d2> | nearestAlive <dA> | drawn <r> | campaign <id>`;
`seen` = inside the camera's viewport AND reached by GameWorld.RayIntersect from the camera (body +1.0 m or head +1.6 m). The verdicts are the
checker's: `python -I Tools/night/n56-room.py agree Logs/player/n47-reel-<tag>-l2f.log ...` (held / vista seen >= 4; ambushed seen >= 1 within
15 m; agreement over foundry / iceline / kiln, doneWhen >= 80 %). Tested on two planted lines (one agrees, one does not: exit 1).
Reels for the lead (the build with 606d2c1f + b4f3b543):
`bash Tools/night/n56-reels.sh <tag> l2f,n52-foundry,a156 l2i,n52-iceline,a156 l2k,n52-kiln,a156 l2s,n52-span,a156 l2v,n52-spire,a156`
then `python -I Tools/night/n56-room.py agree Logs/player/n47-reel-<tag>-l2f.log Logs/player/n47-reel-<tag>-l2i.log Logs/player/n47-reel-<tag>-l2k.log`.
Frames for the critic: `shot-N29-L56-n52-<level>-<room>-entry.png` (a3, b1, g2, g5, kb2, kb3: the entry shows the fight), beside n55-freeze's
a452 `K52-...-entry` frames of the same rooms.

## L3 the seven failing rooms (Tools/night/n56-l-l3.py; maps: n56-l-map.py, the mirror-free cells: n56-l-i1free.py)
| room | check | before | after | what was added (its job) | contract (n53-contract-all.sh) | sha |
|---|---|---|---|---|---|---|
| kiln kb2 | I4 | pad 1.50 m off the circuit | PASS 2.2 m, then **taken out**: FAIL 1.50 m | THE SPOUT LIP, a 1.2 m kerb 0.3 x 2.2 m between the pad's recess and the east lane (cover); the circuit's proof walked outside it (x 7 -> 7.75) | 14/14 (dwell 1.95 -> 2.20 s, grace to 2.25; pads 2/2). **But the whole-level walk bot:** n46flow n52-kiln 10 seeds clear 92.2 -> 110.1 s (+19 % over the baseline: L2's gate is +10 %), stall 25.5 -> 32.5 %; at 20 seeds clear 91.7 -> 89.8 s (the 10-seed clear was the seed cascade) but stall 26.1 -> 34.5 % again (kb2c, kb3 too: the 1.6 m lane beside THE RUNNER holds the pilot): the lip is out (kb2off in n56-l-l3.py) | d619427d, out in the next commit |
| span p5 | I4 | pad 1.50 m off the walkway line | **PASS** 2.5 m | nothing added: THE CAPSTAN SPRING moved 1 m toward the pit's lip (a pad's move; its launch re-probed: pads 1/1 landed) | **15/15** (dwell 2.75 -> 0.00 s) | 29b8d4b6 |
| iceline g8 | I2 | 1.08 / 1.10 | **PASS** 1.40 | THE ASH PIT (the roundhouse's firebox dump, lava rules 0.8 m down as g7's GRATE; 7.9 x 4 m across the west lane from the wall's face to THE PIT, z -230.5..-226.5): the west lane wall rides over it (hazard); the west proof re-walked east of THE PIT | **14/14** (hold probe 3 spots, 1 / 1 / 1 hits; dwell 0.90 -> 0.30 s). The first cut (z -237.5..-233.5): 13/14, a 0-hit camping spot on the west gallery: reverted. Hazard contact: the pilot's lava rule out of lava 129 steps, held at its edge 3151 steps (10 walks). **Bot cost:** g8 deaths 0 -> 7 in 10 walks, clear 40.8 -> 44.4 s, iceline stall 37.1 -> 37.3 % | 20bb4fbb |
| iceline g3 | I2 | 1.28 | 1.28 (not built) | tried THE SPILL PIT (dry, 1.5 m deep, across the chute): I2 1.95, but the level report: `completable: g3 ERROR` (the real Mover cannot climb its 1.5 m north lip) and `enemies reach every tier: not reached by walkers: node (-9.5, 0, -79)` (an r.nodes spot, which L2 may not move, stands in the chute): reverted. A 1 m pit gives ~+0.8 m (1.33: too thin); a block across the lane next to the ride face risks the ride (0.1-0.15 m clearance to the rider's capsule). | - | - |
| iceline g4 | I2 | 1.18 | not built | the walk beside the east lock wall is the EAST BRIDGE (NIGHT46 A3) straight onto the gallery ramp: only a re-cut of the bridge / ramp makes the ride a shortcut (an added hazard in the canal does not: the bridge crosses it) | - | - |
| spire v2 | I2 | 1.17 | not built | the ride runs along the west wall over the decks the walkers use (R NIGHT55: a re-cut, ~40 min) | - | - |
| foundry a3 | I1 | 69 % | not built | n56-l-i1free.py: < 60 % needs > 28.5 m2 of new solid on mirror-free floor; the free cells off every walk line, pad and spot are 85 m2 but in three scraps (beside the door, west of the ride face, along the south wall): ~30 m2 of filler gives 59.96 %: three pieces whose only job would be the metric (GG w35: "random pillars that serve no purpose"): not built | - | - |

Identity FAIL rooms **7 -> 5** (p5, g8 fixed; kb2 fixed and taken out again for its bot cost; a3, g3, g4, v2 left): the doneWhen (<= 2) is SHORT. The four left need re-cuts (a ramp or a
bridge moved, a lane re-laid), not added pieces: each is ~40 min with its contract, past what this night had after L1 / L2. Contacts per added
piece: kb2's lip (cover) has no per-piece bot count (no harness prints one; R NIGHT55 said the same): its contract's completable walk passes it
(7.49 -> 7.59 s) and the pads probe lands 2/2 beside it; p5's pad: launched and landed (pads 1/1); g8's ash pit: the pilot's lava rule above.

## S1-doors (slice 1: 24334c25; the leftover after L1-L3's first attempts)
The lead's r1 numbers (n55-r-flat.py on verbs37 x5, every 50th frame): 7 flat frames (foundry 1, iceline 3, kiln 1, span 0, spire 2). From each
frame's telemetry camera and its picture (vi f0900: one flat blue panel of g1's east wall during the wall jump; the reel logs no yaw): five
white wall lamps flush to the face each camera is against (Tools/night/n56-l-doors.py): g1-L56a / L56b on g1's east wall, g1-L56c on its south
wall, kb3-L56a on kb3's north-east wall, v2-L56a on THE BELL's south face. Level reports: lights PASS (flush by rays, one accent per room).
Slice 2 (368514a6): foundry vf f0500 slides NORTH (the telemetry has no yaw, but the camera's Unity z runs 46.0 -> 47.9 -> 49.5 at 11.6 m/s
forward: web -z), so its frame is d2's arch 7 m ahead over a2's dark north floor. a2 cannot take a lamp (gen-n52-foundry.cjs --finish, not
this builder's file, keeps every room but a3 / a5 / b1 bit-equal to n48-foundry's, lights included): the lamp is d2's, in a3's row, a white
wall lamp on d2's east cheek 1 m inside a3 facing the doorway (two cuts failed the level report first: under the lintel "outside room a3's
floor plan"; inside the cheek "not flush"). Not measured on frames: the lead's verbs37 x5 + `python -I Tools/night/n55-r-flat.py
Logs/feel/<tag>/v?/verbs37` (before 7 on r1, the target 0).

## L3 round 2 (the lead's 02:40 ask: an added ride wall for g4 / v2, a one-sided piece for a3), worked through, not built
- **g4:** an added ride wall is a shortcut on I2's geometry wherever its landing is a tier the walkers climb to: beside the NE gallery ramp it
  scores ~1.7 (the 3D walk includes the 4 m climb), onto the NW ledge ~1.8; but the ride holds its attach height (verbs37: "the most a ride rose
  above its attach point 0.16 m"), so a ride that lands on a 4 m gallery needs a 4 m attach the floor does not give (jump 1.5 m + double), and
  the NW ledge (2 m) is a jump link from the far floor anyway (walk ~7.2 m: 1.2). Every honest spot gives a ride nobody can use or a walk
  that is not longer: the room's walk is the EAST BRIDGE straight onto the gallery ramp; a re-cut (the bridge or the ramp moved) is the fix.
- **v2:** the same test: a ride wall up THE BELL (5 m, walked round to its stair at z 53: walk ~34 m vs a 10 m ride, 3.4 on I2) needs a 5 m
  attach; no floor or 2 m deck beside it gives one. Not built (it would pass I2 and teach nothing).
- **a3:** a one-sided piece must add > 28.5 m2 of footprint whose mirror is empty floor; n56-l-i1free.py's mirror-free cells off every walk
  line, pad and wave-1 spot are three scraps (the entry floor east of the door, west of the ride face, the south wall's east strip): ~30 m2 of
  filler reads 59.96 %, three pieces with no job but the number. An island in the casting pit (mirror-free: the pit's west half has no
  solid) either touches a pit edge or sits within the 3 m gap jump of one, so the walkers cross the pit on it and the ride stops being the
  shortcut (I2 1.64 would fall). Not built.
- **g3:** I2 1.28 is the face against the room's west wall (0.4 m from it, not ridden); the ridden face (E) is the lane the walkers use flat.
  The dry pit across the chute (+0.8 m per metre of depth: 1.5 m was too deep for the Mover, 1 m gives ~1.33) and a sill across the lane next
  to the ride face (0.1-0.15 m from the rider's capsule) were the only additions found; not built.

## Second attempts after gate-r2 (04:20-05:10, after the usage-limit stop 03:09-04:20)
- **clear55 (626dc329):** gate-r2 B2 FAIL 2 / 3 (the off side drew the card at alpha 1.00). The cause: PlayerChecks sets
  NightRows.ForceOffFrom from -skygrindOff55 after the Ui is built, so BuildClear55's Clear55.On(cfg) read ON; the hold (Clear55Beat) is built
  at the first frame and obeyed the switch. The card's listener and RenderClear55 now need Clear55Beat. dn-check-wt 0 errors. Not run: the
  c55*off reels must print "cards 0".
- **L1 (25ab1748):** gate-r2 L1 FAIL 1. IN VIEW reads NOT MEASURED (n/c, not a PASS) when R5 finds no foothold, with the entry-eye reading
  printed beside it. Ambushed rooms' "beside the foothold" is now a distance: each wave-1 node within BESIDE_M 10 m of the foothold box (the
  slate and levels.md name no number: a proposal) and not behind it. Planted 7/7 (+ a3 @u3-n52 with its footholds out reads not measured; +
  span p3, whose one node at 11.9 m FAILs beside). **The table: IN VIEW 4 / 18 strict** (kb3, a3, g2, p1; 13 rooms have no foothold);
  **15 / 18 with the entry-eye fallback** (the r2 reading; p3 now FAILs beside). Adding the 13 footholds is level work not started (each
  would move the held rooms' eye and needs its contract).
- **L2 attempt 2 (c4b725d6):** g3's wave 1 onto the circuit's west leg (each spot still passes the ambush rule). g5 has no ambush-passing spot
  within 3 m of its walk lines (they run down the door's axis, which the corridor rule excludes): not moved. Bot n46flow n52-iceline x10:
  **stall 37.3 -> 36.5 %** (the gate <= 25: 11.5 points over; its baseline 32.0), clear 223.8 -> 220.2 s, 10/10. Also measured: the shared
  commit's level files give the baseline table exactly (iceline 32.0 %, g5 37.1 %), and putting g5's own spots back (g5off) left g5 at
  60.3 %: per-room bot stall follows the RNG of the rooms before it (g2's and g3's changes move g5's run), so a per-room stall is noisy.
- **S1-doors slice 3 (cd3999a7):** r3 (slice 1 in): foundry 0, kiln 0, span 0 flat; iceline f0850 0.0137, f0900 0.0238, f1350 0.0220;
  spire f1550 0.0205, f1600 0.0249. g1-L56a moved 3.5 -> 2.4 m (f0900's frame covers ~0.9-2.7 m of the wall at 1.2 m) and v2-L56a 3.2 -> 2.3 m
  (f1550 / f1600: ~1.6-2.6 m of THE BELL's face at 0.35 m). f0850 (riding g1's south wall at 15 m/s, 0.35 m off it) and f1350 (0.35 m off the
  east wall) see ~0.5 m of wall: only a lamp at the reel's own sample point would change them (not done).

**Contract notes (unmeasurable as written, the sharper reading used):**
- held / vista ">= 4 enemies or tells visible": g2 and g4 open on a ONE-enemy wave 1 (a warden; a lone wave is never overlapped by
  overlap46: floor(1 x share) = 0), so no node placement can show 4. The count is printed as written; L2 decides per room whether wave 1
  grows (level data) or the room is one of the <= 3 the doneWhen allows to fail.
- "spawns 8-15 m from the entry door ... none with a line into the entry corridor" is read as the ambushed / ambusher clause (held / vista say
  "at any distance"); "beside the foothold (not behind it)" as "the entry eye's ray to the node does not pass through the foothold".
