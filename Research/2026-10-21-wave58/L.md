# NIGHT58 builder L: L0 n57-kiln's exits (row kiln57, ON) and L1 level 4 as n58-span (row span58, off)

Builder L's notes (NIGHT58.md section 4 L; t0 13:59:29; L started 14:20). Every number below was printed by the command named beside it;
"not checked" where it was not. Bot numbers are the walk pilot's (n46flow), never a person's.

## L0 cause first: where the harness puts the bot at the next room (14:22-14:40)

How it was printed: n46flow counts "put at the next room" (Harness.N46.Flow.cs: a link not crossed in max(30 s, 3 x par), or a cleared
fight room not left in 15 s) but prints no room. A ONE-LINE print at that counter (`SKIP58 seed .. room .. feet (x, y, z) ..`) was added
to the harness in the worktree for the trace only and NOT committed (the harness is not L's file; the counter itself is unchanged), then
`dotnet run --project Tools/SimHarness -c Release -- n46flow --level n57-kiln --seeds 10` (and `--first 11`).

The baseline reproduces: 14 (seeds 1-10) and 15 (seeds 11-20), as Logs/n58/kiln-base.txt / kiln-base-B.txt.

| room | kind | seeds 1-10 | seeds 11-20 | where the bot stands when it is moved (feet x, y, z) | the room's exit |
|---|---|---|---|---|---|
| kb2c (new, slot B) | room, cleared | 6 | 6 | (6.4, 0.0, -181.2) on 8 of 12 (the rest (4.9..6.1, 0..1.6, -178.8..-181.1)) | (0, -191.5), door d5 x -2..2 |
| kb2b (n52's link) | link | 3 | 5 | by the link's exit point / its south wall: (-0.3, -161.5), (-1.7, -159.3), (-6.6, -159.2), (-2.6, -158.5), (-6.1, 0.6, -159.0), (-5.0, -149.0), (-6.8, -159.2), (-0.5, 1.6, -157.0) | (0, -157.5); kb2c starts at z -163.5 |
| kb3 (new, slot C) | room, cleared | 4 | 2 | (0.6, 0, -219.2) x2, (-1.6, -225.3), (-4.4, 1.4, -208.4), (2.7, 0, -217.5 / -217.7) | (0, -224), door d6 x -2..2 |
| kb3c (new, slot DE) | room, cleared | 1 | 2 | (4.5, 0, -241.0), (4.5, 0, -240.0), (-7.5, 0, -253.0) | (-6, -251.5), door d6b x -8..-4 |

The cause, read against the plan (Design/levels/n57/kiln.plan.json):
- **kb2c (12 of 29):** THE FAR BRIDGE (deck x 1..9, z -184.5..-181.5, 4.5 m) and THE FAR RAMP (x -7..1 along z -183, width 3, rising east
  to 4.5 m) make ONE barrier across the room from x -7 to the east wall; the only walk south to the exit is the 2 m west passage (x -9..-7)
  or over the ramp. The bot clears the room in the north half, heads straight for kb3's spawn (-8.5, -203) and pins at (6.4, 0, -181.2):
  the corner of THE COOLING WALL's south end (x 8.4..9, z ends -181.5) and the far bridge's north face, 3 m from the ash vent (4, -179).
- **kb3 (6 of 29):** the same shape: THE GALLERY RAMP (x -5..3 along z -221, width 3, z -222.5..-219.5, rising east to 4.5 m) plus THE FAR
  GALLERY (x 3..10, z -225.5..-218.5, 4.5 m) wall off the exit door (x -2..2, z -225.5) from the nave from x -5 to the east wall; the walk
  round is the west end (x -10..-5) beside the press gantry's ramp foot and the quench vent (-8.5, -224.5). The bot pins on the ramp's
  north face where it is 2.5-3.5 m high ((0.6, -219.2), (2.7, -217.6)).
- **kb2b (8 of 29):** n52's link, unchanged; its goal is its exit point (0, -157.5), 2 m short of the door d4 (z -159.5..-163.5) and 6 m
  short of kb2c's zone (z < -163.5). The bot reaches the exit point and idles in the door's mouth (south wall, x -7..0). Seeds that enter
  carry their run through. n52-kiln's count by room: see below.
- **kb3c (3 of 29):** two at (4.5, 0, -240.5) (east of THE TAP HOOD / the run wall's line) and one at the door d6b's west jamb (-7.5, -253).

So the exits are walled, not unlit: in kb2c and kb3 the room's top tier and its ramp form a full-width wall between the fight and the
exit door, with a narrow way round at one end. The fix is in the level data (gen-n57-kiln.cjs): a walk lane past the barrier on the side
the fight ends.

## L0 the fix: three cuts, HELD (not committed as level data; the generator diff is `Research/2026-10-21-wave58/L0-held.patch`)

Every cut: `node Tools/night/gen-n57-kiln.cjs`, levelgen `--walk` (0 errors each time; levelgen's own walk prints "DID NOT reach the last
exit; zone at the end kb1" on HEAD's plan too: not new), `--finish`, then n46flow `--level n57-kiln` on seeds 1-10 and 11-20.
- **Cut 1** (kb2c: the far bridge's east 2 m an arch, underside 2.5 m; kb3: the gallery's west 2 m an arch, the ingot stack 2.5 m east):
  put at the next room 14 / 15 -> 11 / 9 (kb3 6 -> 1; kb2c 12 -> 9: the bot pinned further west on the bridge's face); deaths 8 / 5;
  stall 24.4 / 21.5 %.
- **Cut 2** (kb2c: the whole bridge an arch; the spawn (-7.5, -169) -> (-1, -166), on the line from the link's exit through the door; at
  (-1, -169) levelgen's spawn31 refused it: wave 1's nodes 9.6-11.2 m in the cone): 4 / 8 (kb2b 8 -> 0; kb2c now pinned at (2.2, 0, -184.2)
  UNDER the arch, between the slag step and the far ramp's top); deaths 4 / 3; seeds 11-20 SHORT 9/10 (kb3c cap 1); n57-l-room I2 kb2c
  FAIL 1.07 (the ride no longer a shortcut: the walk goes under the bridge).
- **Cut 3** (kb2c: the bridge x 1..7 an arch, x 7..9 THE LANDING PIER solid (the ride's landing); the slag step 2.5 m east; kb3 as cut 1;
  the kb2c spawn as cut 2):

| measure (n46flow --level n57-kiln) | before 1-10 | before 11-20 | before 21-30 | after 1-10 | after 11-20 | after 21-30 |
|---|---|---|---|---|---|---|
| MAIN put at the next room (<= 3 per set) | 14 | 15 | 14 | **3** | **1** | **0** |
| clear (150-240 s) | 177.0 | 169.9 | 167.1 | 162.1 | 176.5 | 168.3 |
| stall (<= 21.5 % + 10 % = 23.65 %) | 18.9 | 20.5 | 18.8 | 23.2 | 22.4 | 18.8 |
| deaths (<= 3 per 10 + band) | 2 | 6 | 1 | 3 | **8** | 2 |
| GREEN walks | 10/10 | 10/10 | 10/10 | 10/10 | 10/10 | 10/10 |

  n57-l-room.py: kb2c PASS (I2 2.18, as NIGHT57), kb3 PASS; identity I5 / I7 PASS. The contract (`n57-l-contract.sh kb2c,kb3`): kb3 C 14/14
  PASS (dwell 1.10 -> 1.80 s, grace 0.8); **kb2c C 13/14 FAIL: dwell 0.70 -> 1.75 s (grace 0.5)**: the spawn moved 4.5 m from the door's
  middle on its axis (T-119's shape). Deaths pooled over seeds 1-30: before 9, after 13 (> 9 x 1.10; seeds 11-20's 8 is outside the before
  sets' spread 1..6): the lead's rule (fixed before the 21-30 numbers) says HELD. Deaths sit in kb3c / kb3 (rooms not changed): the harness
  gives a moved bot back the HP it entered the room with, so fewer moves bring the bot into kb3c with less HP (not proven: a hypothesis).
- **For the next night:** apply L0-held.patch (`git apply`), then re-cut kb2c's spawn: back at (-7.5, -169) with the link's walk given a lane
  past the west liner, or 3 m+ off the door's axis (re-check spawn31 and the contract's dwell). Then deaths on seeds 1-30 against 9.

## L1 level 4 as n58-span (15:02-15:20): the card and the generator; no new room yet (span58 OFF)
- The A-9 card: docs/ROOMS.md "Level 4 as n58-span": slot **P2** (n52's link p2 4.2 FIRST GAP, between p1 and p3; p4 is the pull's test link
  and stays whole), the room p2 4.2 THE GAP NEST, the role the drone (flyer) over the void, alone with fodder (R-2, R-3): kinds 3 -> 4.
- `Tools/night/gen-n58-span.cjs`: NIGHT57's slots idea on span's own plan (Design/levels/n52/span.plan.json read, never written);
  writes Design/levels/n58/span.plan.json; `--finish`; `--check`. With no slot built (NEW = [] tonight): "generated Design/levels/n58/span.json
  vs n52-span.json beyond the names: none" and "shipped Design/levels/n58-span.json vs n52-span.json beyond the names: none" (exit 0).
  levelgen on the n58 plan: the same 22 report errors as Reports/LEVEL-n52-span-validate.md (the void pits' recovery, lessons o1.1 / o2.1).
- Not built: the room's geometry, n58-l-room.py / n58-l-contract.sh, FeelReel.N58.Level.cs (a158). MAIN combat rooms 3 -> 3 (not met).
- Level-id lists naming n52-span (Assets/Scripts, Tools, Design): not checked tonight (no room to load yet).

## Not checked
- Anything in a Player (a157 / a158 frames, verbs37 on n57-kiln / n58-span): builders run no Player.

## L1 EXT (THE GAP NEST)
Builder L, NIGHT58 extension, 19:51-20:25 (worktree from port/stage3 at cb78a886). span58 stays OFF; Design/unity-config.json untouched.
- **Built** (commit 2400f7e7): `slotP2` in Tools/night/gen-n58-span.cjs, `NEW = ["p2"]`. p2 4.2 THE GAP NEST, one cut, exactly per the card
  (docs/ROOMS.md "Level 4 as n58-span"): the near deck (y 0, z 79.5..87.5, widened from n52's 8 m pier to x -10..18), THE FAR DECK
  (n52's gate A landing, y 10, kept whole with its gate p2-gateA: the pull's way in, the ring at (0, 15, 46.5) kept), THE GAP between
  (void, floorY -14), THE SIDE GANTRY on the east wall (a ramp 10 m over 28 m, 5 m wide: the walkers' long way round), THE GANTRY HEAD
  (the far deck's east arm), THE RIGGER'S WINCH on the near deck (the foothold's cover, R5), THE LAMP POST and the white WORK LAMP pool
  on the head. Every new piece has a `job` and a `why`.
- **Room fields:** kind link -> room, arena36, par 3 -> 30 (between p1's 20 and p3's 35; written reason in `par_why`), n52's LIMIT
  pistolOnly dropped (`limit_why`: the gantry is the long way, so the limit guarded nothing), waves exactly the card's (w1 drone 2 + husk 3,
  w2 drone 3 + swarmer 4; no adjustment), roles43 flyer + fodder twice; spawns on the far deck + head (w1) and the far deck only (w2: the
  State change); n52's two walk proofs kept + a third up the gantry; respawnY / fallReturn43 / pull38 / routeVia / the lesson kept.
  The drone is an existing kind (level 2's flyer, as n57-kiln's kb3c); no new enemy, no new number in config.
- **levelgen** (`levelgen Design/levels/n58/span.plan.json ... --walk`): plan checks 0 problems; 22 -> 21 report errors (n52's p2 east
  pier pit row is gone with the pit; NO new error; a first try had one, `lights p2-L5 not flush`, fixed by moving the pool onto the head).
  New p2 rows: "enemies reach every tier: p2" PASS (2 raised tops, 5 nodes), "completable: p2" PASS (3 proofs, the gantry's 4 legs in
  6.82 s). The `--walk` line reads "room 1 exit at 3.0 s; DID NOT reach the last exit" on n58-span and the SAME on n52's plan
  (run 20:17, its report's one changed line, the source path, restored): not new.
- **n46flow seeds 1-10** (n52-span = before, row-off level; n58-span = after; my before run matches Logs/n58/flow-base.txt exactly):

| number | n52-span (before) | n58-span (after) |
|---|---|---|
| GREEN walks | 10/10 | 10/10 |
| stall % | 14.5 | 13.4 |
| deaths | 0 | 0 |
| clear (sum of room medians) | 88.7 s (3 rooms) | 132.2 s (4 rooms) |
| level time median | 111.4 s | 147.2 s |
| put at the next room | 0 | 3 (the harness does not print which room: not checked) |
| falls | 107 | 173 (p2 61) |
| DULL % of fight | 18.4 | 16.0 |
| p2 4.2: clear / deaths / stall / dwell / w1 in view / sight | a link (not measured) | 34.9 s / 0 / 10.2 % / 0.0 s / 3/5 / 52.1 m |
| p3 clear | 27.8 s | 36.3 s (p3 unchanged: the bot arrives from a fight now) |
| p1 / p5 clear | 26.6 / 34.3 s | 26.6 / 34.4 s |

- Evidence: Logs/n58/ext-L/flow-n52-span-s1.txt (before), Logs/n58/ext-L/flow-n58-span-s1.txt (after).
- **Fingerprint** after 2400f7e7: `fingerprint` EQUAL; `fingerprint --off58` EQUAL (seed 1: 12 hashes, kills 35, earned 4808).
- **Not checked:** seeds 11-20; which room the 3 put-at-next-room events are in (likely p2: a cleared room not left in 15 s; not
  measured); n58-l-room.py / n58-l-contract.sh / n53-identity.py on p2 (not built / not run); `n38 gates` on p2-gateA with the gantry
  beside it; any Player frame (a158); level-id lists naming n52-span. A second cut was not made (no walk failed, the bot was not stuck).
