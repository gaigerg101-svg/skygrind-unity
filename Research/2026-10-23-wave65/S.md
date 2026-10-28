# NIGHT65 builder S notes (b1 THE YARD, a8 PIERCE, a4 PARRY)

Branch n65-S from 2f3e4d94 (the lead's 0:05 HEAD). t0 = 2026-10-23 13:58:02. The night is branch C (no Player): rooms are built and judged
sim-side.

## Cards turn (0:06-0:30)

### What I read
- The brief, CLAUDE.md, PRINCIPLES.md, VERBS.md, NIGHT65.md, and the slate (b1, a8, a4, ROOM).
- From the forge: research-rooms.md, history.md sections 1-3, code-scout.md, redteam.md.
- The n52 plan rows and pieces for b1 / a8 / a4 (Design/levels/n52/foundry.plan.json); LEVEL-FORMAT.md (ramps start at y 0; pits and the
  recovery step <= 1.3 m; lava; routeVia / reachFrom in LevelValidate24.cs:157-170, 233-237).
- The checkers: n56-room.py (R2 / R5 / R8 / IN VIEW) and n53-identity.py (I1 / I2 / I4 / I5 / I7).
- hold52 --pads: Harness.N52.Hold.cs:442-491. A pad PASSes when it launches and lands within 1.5 m and 0.3 m of its target.
- n46flow's air %: Harness.N46.Flow.cs:21, in-fight steps not grounded.
- The red: the lead's Logs/n65/red/summary.md and its per-room files. The n64 flow base: Logs/n64/flow-base.txt.

### The red frames
- Logs/feel/n64-freeze/vf/verbs37's telemetry puts frames f0750-f0950 on b1's west rail (camera x -9.1, z 159.8-179.3 in the reel's
  +z frame). Those are the only red frames of my rooms.
- No vf frame is in a4 (z 89-108) or a8 (z 342-364). Those two wait for tour65 on rc0.
- The red plan PNGs were drawn into a scratch folder in my tree (deleted); the lead's copies are Logs/n65/red/<id>/plan.png.

### Facts the cards rest on (read, not run)
- **The parry:**
  - A parried glob homes on its shooter at 32 m/s, hits for 24, then bursts for 40 in 3 m. A wall in the way sets it off at the wall
    (Spitter.cs:452-479; Parry36.cs:166-180).
  - spitter.hp is 60, so one parry kills a spitter.
  - The KESTREL does 25 a shot, 0.4 s apart (weapons.revolver).
- **The walk bot** parries melee wind-ups only (AutopilotInput.cs:455-461), so no n46flow number can show a glob parry.
- **The launch:** launch36 throws a husk about 12 m (16 m/s x 0.8 s) and bursts for 60 in 3.5 m. husk.hp is 40.
- **The style meter:** style.thresholds start at 0 / 90 for DULL / COOL. A husk kill is worth 30, so a 3-husk room barely leaves DULL
  (a8's red is 83.5 % DULL).
- **verbs37 rides b1's 32 m west rail today** (VerbsWall39.Pick, the longest wall). Card b1 keeps a 32 m flush face there.
- The a8 red was flown by the scripted Pierce43 pilot (Walk43Foundry; BaseId n43-foundry). The rebuilt row's generic autopilot is not like
  for like.

### Calls for the lead
- **openings65.json:** b1 "ambushed" (kept), a8 "held" (new: W1 is 4 husks seen down the lane), a4 none (one W1 enemy cannot meet held's
  >= 4; this avoids an I7 clash with R's a3 / a5).
- **I7 depends on R:** if R's a5 card becomes "ambushed", b1 and a5 clash, and one of us changes.
- **a8's waves** go from [husk x3] to [husk x4] + [husk, husk, spitter]. Two reasons:
  - The style meter cannot leave DULL on 3 kills.
  - Held needs 4 in view.
  - The risk is the deaths guard (red 0 / 0).
- **a8's ledge has no walk route up** (I2: the landing is unreachable on foot). Husks reach it by their leap (husk.leapMinHeight 1.5, the
  ledge 1.6). The validator's route is from `reachFrom` (the hatch).
- **b1's gantry is a slab over open floor:** nav under a slab is not checked. The fallback is written in the card.

### du -sm of my worktree
Printed in the report.

## Card gates (0:47-1:07)
- **Gate 1 (Logs/n65/cardgate.md): all three FAIL.** Fixed in 0982e8a6 ("cards fixed"):
  - b1's MAIN beats the red per set;
  - a4's block is off the liner test, plus the booth;
  - a8's hatch ramp and the pit stair.
  - The convention I had wrong: depths are measured from the door's centre, 2 m outside the room's edge.
- **Re-gate (Logs/n65/cardgate-S2.md): b1 PASS; a8 and a4 FAIL.** Fixed in 103090f9 ("cards fixed 2"):
  - a8: heights set against ai.grid jumpUp 2.2 (the ledge 2.4 m, the pit -2.3 m with a 7-riser stair).
  - a4: the block moved 2 m east of the window strip; P9 closes the north lip; the screens run to the roof.
  - a4: Spitter.cs:176 read and the wave-1 fast-shot skip named.
  - What I had wrong: nav.stepHeight 0.4 / topInset 0.7 are NavGraph's numbers. The enemies' grid (and R2 / I2) uses ai.grid step 0.6 and
    jumpUp 2.2.

## Round 1 (1:00-1:15): room files at 295319c8 on n65-S1 (from 641f3e29)
Commands, every output saved with the command and the sha at its top:
- `node Tools/night/gen-n65-foundry.cjs --slots=latest --run` -> "levelgen --walk: 0 ERROR". Its "plan checks: 1 problems" line is b2's
  spawn 21, a kept room, and "bot walk: DID NOT reach the last exit; zone at the end a1" stops in a1; neither touches my rooms. Whether both
  print on an all-old assembly: not checked.
- `bash Tools/night/n65-red.sh 6 n65-foundry tmpS-r1` (sha 295319c8, 15:11), copied per room to `Research/2026-10-23-wave65/<id>/r1/`
  (room-check.txt, flow-A.txt, flow-B.txt, pads.txt, plan.png).
- `node C:/Users/gaige/dev/SKYGRIND-unity/Tools/night/n65-table.cjs tmpS-r1` (the table below; the tool is not on my branch).

| room | room check (red) | air % A / B (red) | stall % A / B (red) | DULL % A / B (red) | deaths A / B (red) | clear s A / B (red) | pads (red) |
|---|---|---|---|---|---|---|---|
| b1 | PASS (FAIL R5) | 54.0 / 55.2 (54.5 / 58.8) | 23.9 / 25.4 (33.8 / 30.1) | 62.4 / 49.4 (54.2 / 44.1) | 0 / 0 (1 / 1) | 35.4 / 34.4 (30.8 / 38.7) | 2 / 2 (2 / 2) |
| a8 | PASS (FAIL R2, IN VIEW) | 44.9 / 38.9 (29.1 / 32.1) | 59.9 / 61.9 (42.3 / 46.0) | 52.4 / 45.4 (83.5 / 87.5) | 0 / 0 (0 / 0) | 25.5 / 26.9 (6.4 / 5.4) | 1 / 1 (none) |
| a4 | FAIL IN VIEW "-" (FAIL R5) | 39.8 / 40.1 (47.2 / 54.5) | 40.0 / 43.4 (31.2 / 40.3) | 85.7 / 71.6 (57.5 / 56.4) | 0 / 1 (0 / 0) | 21.8 / 16.4 (15.1 / 15.2) | none (none) |

**Read honestly, against each card's MAIN:**
- **b1: MAIN missed on both lines.**
  - DULL got worse: 62.4 / 49.4 against an aim of <= 40 each.
  - Air is flat to down: 54.0 / 55.2 against >= 59.
  - The guards improved: deaths 1 / 1 -> 0 / 0; stall 33.8 / 30.1 -> 23.9 / 25.4; R5 now PASSes; pads 2 / 2.
  - The red's top rank (SKYGRIND on A) is gone (AIRBORNE now). The lava kills and the lava edge that held the red's bot are both gone; which
    of them fed the red's style is not checked.
- **a8: MAIN met on DULL and air; a stall guard broken.**
  - DULL 52.4 / 45.4 (aim <= 75); air 44.9 / 38.9 (aim >= 35); the pad PASS; the room check PASS on every item (I2 1.45, IN VIEW held 4 / 4).
  - **Stall 59.9 / 61.9 is out of the red's 42.3-46.0 band.** "dead" is 16.2 s a seed (no enemy alive, the door shut): most likely wave 1
    waiting on lesson 0 while the generic autopilot does not walk to the ride wall's foot (afterLesson43 0). Not checked.
  - Clear 25.5 / 26.9 against 6.4 / 5.4: 7 enemies against 3, and the generic pilot against the scripted one.
- **a4: worse than the red on every play number; the shape holds on the checker.**
  - DULL 85.7 / 71.6 (aim <= 50); air 39.8 / 40.1 under its guard (47.2 / 54.5); a death on B (red 0 / 0); kill gap max 28.4 s; stall over the
    band on A.
  - The booth hides the spitter from a walker, so the bot searches and waits.
  - R5 and I1 now PASS (8 %). IN VIEW FAILs on the ambushed clauses (opening "-"), where the red was n/c.
- **A same-geometry residue the comparisons must carry:**
  - a3 (an unchanged n52 export in my assembly) reads DULL 65.1 / 73.9 against the red's 56.8 / 69.6. That is +8.3 / +4.3 pts with no a3
    change.
  - a3's deaths read 7 / 3 against 6 / 2; a2's air 73.3 against 73.9.
  - So a DULL gap under about 8 pts is not a room's effect. Why an unchanged room moves (the level's other rooms change the nav grid or the
    level file) is not checked.

**The third card pass (Logs/n65/cardgate-S3.md) and DECIDE65 D-4 (no fourth pass: its items are round 1's BLOCKING list):**
- **a8:** "I2 1.28 by a 1.91 m grid jump onto P8's side". The checker on the built geometry prints I2 1.45 PASS (a8/r1/room-check.txt, at
  295319c8 and again at 5aa0c0dc). Answered by measurement; the round-2 fallback is a 2.3 m curb on P8's south edge.
- **a4:** "the ramp's top is a safe firing spot inside minRange 6 (Spitter.cs:175)". **Fixed in the room file at 5aa0c0dc:** the stand is
  3.0 m with no ramp, and reachFrom is on the stand; levelgen 0 ERROR.
  - Re-run (a4/r1/ now holds the 5aa0c0dc files; the first a4 r1 files are at 0299d1ce):

| a4 at 5aa0c0dc | red A / B | measured A / B |
|---|---|---|
| room check | R5 FAIL, I1 FAIL | R5 / R2 / R8 / I1 PASS (12 %), IN VIEW FAIL ("-") |
| air % | 47.2 / 54.5 | 53.5 / 54.3 |
| stall % | 31.2 / 40.3 | 45.2 / 46.1 |
| DULL % | 57.5 / 56.4 | 86.5 / 80.5 |
| deaths | 0 / 0 | 0 / 0 |
| clear s | 15.1 / 15.2 | 33.3 / 30.1 (10 / 10) |

  - "P1 still borders W2's west spitter's sight": not re-measured after the raise. Open.

**Round 2 starts from these (each adds or moves a piece with a job):**
- **b1:** pads on the packs' and the bot's chase lines (padLaunch near enemies, air kills). Settle whether husks cross the 1.8 m slag walls
  (the re-gate: jumpUp 2.2) before anything else; if they do, the walls go to 2.3 m.
- **a8:** lesson 0's rect across the apron's west half, so the file comes out as anyone nears the wall, not only at its foot (the stall).
- **a4:** a way for the spitter's line to find the player sooner: the booth's window wider or lower, or the stand nearer the entry. This is
  the trade between the parry's forcing and the bot's clear, and it needs the lead's read of the parry line.

## Round 2 (1:25-1:50): room files at 62c54d48 on n65-S1
The lead's list: (1) a8's stall, a guard; (2) a4 worse than the red, no climb route; (3) b1 judged by the pilots, not the bot's STYLE alone.

**What each change is for:**
- **a8, lesson 0's rect -> [-6, -347.5, 1, -342.5]** (covers the spawn). The cause of round 1's stall, read in
  `n46flow --rooms a8 --seeds 3 --per-seed`: maxStall 8.0 s on every seed and dead 16.2 / 10.9 / 11.7 s. The walk pilot stood at the spawn,
  and wave 1 (afterLesson43 0) waited for the lesson rect at the wall's foot until the 8 s hold.
- **a8, P10 THE RAMP CURB:** a 2.3 m curb on P8's south edge, so no grid jump onto the ramp's side (I2 1.45 -> 1.85).
- **b1, P2 / P3 raised 1.8 -> 2.4 m**, over ai.grid jumpUp 2.2, so the packs must come through the gates.
- **a4, the gallery replaces the booth:**
  - P3, a 3 m, 11 m deck; P4, THE GALLERY STAIR, the climb route under the spitters' fire 7.5-13.5 m out; P5, THE FENCE, which blinds the floor
    within minRange 6.
  - Four tries to get R2 to PASS, each read in the nav grid:
    1. a 2 m deck with a parapet on it: no nav node at the tier's centre (a validator ERROR);
    2. two deck boxes: the nav links break at their seam;
    3. one 2.5 m deck: its end cell was a dead end;
    4. the fence on the floor ending 2 m short: the drop's landing is "standable" (n56-room.py walk_cells), and R2 PASSes.

`bash Tools/night/n65-red.sh 6 n65-foundry tmpS-r2` at 62c54d48, copied to `<id>/r2/`; the table by n65-table.cjs:

| room | room check (red) | air % A / B (red) | stall % A / B (red) | DULL % A / B (red) | deaths A / B (red) | clear s A / B (red) | pads |
|---|---|---|---|---|---|---|---|
| b1 | PASS (FAIL R5) | 52.2 / 50.4 (54.5 / 58.8) | 27.0 / 26.7 (33.8 / 30.1) | 53.1 / 51.0 (54.2 / 44.1) | 2 / 0 (1 / 1) | 33.8 / 33.3 (30.8 / 38.7) | 2 / 2 |
| a8 | PASS (FAIL R2, IN VIEW) | 60.2 / 56.1 (29.1 / 32.1) | 53.7 / 46.9 (42.3 / 46.0) | 49.5 / 60.1 (83.5 / 87.5) | 0 / 0 (0 / 0) | 14.0 / 14.4 (6.4 / 5.4) | 1 / 1 |
| a4 | FAIL IN VIEW "-" (FAIL R5) | 44.7 / 45.7 (47.2 / 54.5) | 44.9 / 45.9 (31.2 / 40.3) | 77.8 / 76.9 (57.5 / 56.4) | 0 / 0 (0 / 0) | 19.8 / 22.5 (15.1 / 15.2) | none |

**Read honestly:**
- **a8:** better on every MAIN line and its dead time.
  - Stall: B 46.9 sits 0.9 over the red's 46.0, A 53.7 is 7.7 over. The guard is still missed on A.
- **b1:** DULL on A improved 9.3 pts with the walls, which is at the noise scale.
  - Air dropped about 2 and 5 pts.
  - Deaths on A went 0 -> 2 (red 1); the deaths guard is missed on A.
  - Its verdict waits on T's launch pilot, as the lead says.
- **a4:** better than its round 1 on every line, still worse than the red on DULL, stall and air.
  - It now has its climb route and R2 PASS.
  - The parry waits on T's parry pilot.
- **The same-geometry residue stands:** a3, unchanged, reads the same as in round 1: DULL 65.1 / 73.9 against the red's 56.8 / 69.6.

### Round 2, gg-proxy's r1 BLOCKING folded in (room files at c737f197; Logs/n65/r1/review-gg.md)
Answered by name:
- **b1, "the far yard is one anvil: add a second slag line with two 3 m gates at about z -171, one beside THE WEST RAIL": DONE.**
  - P12-P14 THE NORTH SLAG LINE at z -170.5..-171.5, **2.4 m** rather than the review's 1.8 (over ai.grid jumpUp 2.2, as P2 / P3).
  - Two gates: GATE FW x -9.4..-6.4 under the rail, and GATE FE x 1..4. P14 closes the lane under the gantry slab.
  - The north pad moved to [-4, 0, -173.5]; wave 3's fourth node is [3, 0, -178], north of the line; proof 1 goes through GATE FE.
- **a8 (1), "the tailrace's only job is its own way out: wave 2's husks on its floor, the health by the pad": PARTLY.**
  - The health is in: [1, -1.4, -352.5], beside THE TAILRACE PAD.
  - Husks on the pit floor read levelgen ERROR "not reached by walkers": the validator's walkers never stand on a pit floor.
  - So wave 2's two husks start on the tailrace stair's head landing ([5.5, 0, -357.5], [6.5, 0, -357.5]) and come up out of it behind the
    file.
- **a8 (2), "the ride attaches at about 1.5 m and must reach 2.4 m: a 1.2 m step deck at the wall's south foot": DONE as a ramp.**
  - P11 THE TAKE-OFF RAMP: a 1.2 m run-up, x -6.6..-5.1, z -344.5..-347.5.
  - A 1 x 2.5 m step deck got no nav node, a levelgen ERROR (the tier check), so the take-off is a ramp.
  - The ramp leaves a 1 m floor passage into the lane, so the floor proof and walkers pass.
  - Whether a jump off its top attaches at about 2.7 m: not checked (my arithmetic: 1.2 + jump.height 1.5).
- **a4, "make the west wall (x -4, z -89..-102) a ride wall with a cyan strip": NOT TAKEN this round.**
  - The window strip it rides into was replaced in round 2 by THE GALLERY, whose stair is the approach.
  - A ride wall beside that stair: I2 would read about 1.19 by my arithmetic (start [-3.2, -89.5], landing on the gallery at [-3.5, 3, -98.5];
    walk about 9.5 m against an 8 m ride). That is a new FAIL where the red reads n/c.
  - The moving-rider parry is left to the parry pilot on the stair.

`bash Tools/night/n65-red.sh 6 n65-foundry tmpS-r2` at c737f197, copied to `<id>/r2/` (the 62c54d48 r2 files are at 7a01a113):

| room | room check (red) | air % A / B (red) | stall % A / B (red) | DULL % A / B (red) | deaths A / B (red) | clear s A / B (red) | pads |
|---|---|---|---|---|---|---|---|
| b1 | PASS (FAIL R5) | 53.8 / 54.6 (54.5 / 58.8) | 26.8 / 28.7 (33.8 / 30.1) | 62.9 / 52.8 (54.2 / 44.1) | 0 / 0 (1 / 1) | 35.3 / 33.9 (30.8 / 38.7) | 2 / 2 |
| a8 | PASS, I2 1.88 (FAIL R2, IN VIEW) | 59.3 / 54.0 (29.1 / 32.1) | **44.3 / 46.8** (42.3 / 46.0) | 55.7 / 63.1 (83.5 / 87.5) | 0 / 0 (0 / 0) | 14.6 / 14.8 (6.4 / 5.4) | 1 / 1 |
| a4 | FAIL IN VIEW "-" (FAIL R5) | 44.7 / 45.7 (47.2 / 54.5) | 44.9 / 45.9 (31.2 / 40.3) | 77.8 / 76.9 (57.5 / 56.4) | 0 / 0 (0 / 0) | 19.8 / 22.5 (15.1 / 15.2) | none |

- **a8:** stall is now in the red's band on A and 0.8 over on B. The lesson rect and wave 2's start moved it.
- **b1:** DULL moved +9.8 / +1.8 against the 62c54d48 run with geometry the bot reads differently, which is inside the noise. Deaths 0 / 0
  (red 1 / 1). The verdict is the launch pilot's.
- **a4:** unchanged from 62c54d48.

### Round 2, the verb pilots and the level-design review (branch n65-S2 from 17e6c525)
**Carried over:** my round-2 files were restored from my own n65-S1 head (fdc16c8e, byte-equal). This branch has T's pilots
(Tools/SimHarness/Harness.N65.Verb.cs, Tools/night/n65-verbs.sh).

**The pilots' rule** (Harness.N65.Verb.cs:96-109): the pose is line65[0] and the yaw is along its first segment; pierce aims along that
segment. So each line65 now starts where the verb happens:
- b1: in GATE M facing north, [0, 0, -158] -> [0, 0, -161].
- a8: in the lane's nav column facing north, [-5.5, 0, -352] -> [-5.5, 0, -359].
- a4: on THE GALLERY STAIR facing the spitters, [-3, 1.5, -94] -> [-3, 3, -97.5].

**The level-design review r1 (Logs/n65/r1/review-ld.md), answered by name (room files at 515bcb60):**
- **b1, "add THE LADLE, a free-standing ride wall facing THE WEST RAIL": DONE.**
  - P15 THE LADLE [-4.6, 0, -170.5]..[-4.0, 5.5, -159.5], ride true, a cyan strip: the canyon hand-off (ride the rail, wall-jump across, ride
    the ladle).
  - Placed between the two slag lines, not at the review's z -165..-177, which crosses P12. Both pieces are kept: the gates serve the launch
    (the pilot's 3-kill burst is in GATE M) and the ladle is the hand-off.
  - The north line's middle gate moved onto the centre line (GATE FM x -1.5..1.5), so the exit stays in view from the door.
  - Wave 1's west node moved to [-2.8, 0, -160.2].
  - **VerbsWall39's pick moved** (`n39views wall --level n65-foundry`): from b1's west rail (the red, f0750-f0950) to "20 m in b1 at (10.7,
    0.2, -168.1) along (0, 1)", the east room wall. Rows 9 / 10 / 11 PASS in that probe; verbs37 in a Player is the lead's.
- **a8, "THE SLUICE's 0.6 m top at 2.4 m is a catwalk: raise it to 6 m with ride true": DONE.** The lane is two ride walls 2.5 m apart (the
  hand-off) plus a cyan strip; I2 1.92; IN VIEW held 4 / 4 PASS; I1 PASS.
- **a4, "move wave 2's second spitter to a floor node on the exit apron behind THE EAST SCREEN": DONE**, at [3.5, 0, -104.5] behind THE FENCE.
  - **The opening is set to "held"**, the card's meaning. IN VIEW now reads "held: 0.0 of wave 1's 1 expected seen (>= 4 PASS)": a FAIL by
    construction with one wave-1 spitter.
  - The round's hand-off theme: a4 has none this round. A pad onto the gallery was tried and dropped: I4 FAIL at 1.91 m, a death on A,
    DULL 88.6 / 90.8.

`bash Tools/night/n65-red.sh 6 n65-foundry tmpS-r2` at 515bcb60 (copied to `<id>/r2/`; b1 and a4 are unchanged after it, a8 only by its
line65, which no flow number reads):

| room | room check (red) | air % A / B (red) | stall % A / B (red) | DULL % A / B (red) | deaths A / B (red) | clear s A / B (red) | pads |
|---|---|---|---|---|---|---|---|
| b1 | PASS (FAIL R5) | 52.3 / 51.0 (54.5 / 58.8) | 31.9 / 32.3 (33.8 / 30.1) | **38.1 / 50.8** (54.2 / 44.1) | 0 / 1 (1 / 1) | 35.9 / 35.2 (30.8 / 38.7) | 2 / 2 |
| a8 | PASS (FAIL R2, IN VIEW) | 54.3 / 59.6 (29.1 / 32.1) | 51.3 / 49.9 (42.3 / 46.0) | 56.4 / 56.4 (83.5 / 87.5) | 0 / 0 (0 / 0) | 17.2 / 16.5 (6.4 / 5.4) | 1 / 1 |
| a4 | FAIL IN VIEW held (FAIL R5) | 45.2 / 47.0 (47.2 / 54.5) | 44.5 / 44.9 (31.2 / 40.3) | 78.3 / 80.3 (57.5 / 56.4) | 0 / 1 (0 / 0) | 20.9 / 19.3 (15.1 / 15.2) | none |

**The verb pilots** (each file opens with its command and sha):

| room | pilot | file (sha) | verdict |
|---|---|---|---|
| b1 | launch | b1/r2/verb-launch.txt (515bcb60) | **GREEN**: launch36 1, bursts 1 (hits 3, kills 3), from GATE M (the red's twin: GREEN, 3 kills) |
| a4 | parry | a4/r2/verb-parry.txt (515bcb60) | **GREEN**: parried 1 glob, the return killed the spitter. The spitter it faced was at (3.3, 0, -102.2), on the floor, not on the gallery (most likely the gallery's spitter walked off the north-end drop; not checked) |
| a8 | pierce | a8/r2/verb-pierce.txt (c7ac953b) | **RED**: the charged shot hit 0 bodies, from three poses in the lane |

**a8's pierce, diagnosed:** the tracked run (`--pose -5.5,0,-352,0`) shows the nearest husk, about a second after spawning, at (-1.8, 2.5,
-358.7): east of the sluice's north end and 2.5 m up, not in the lane. So the file does not form in the lane within the pilot's fixed 1.9 s
(V65WaveWait's 1.0 s plus the 0.9 s charge). Round 3: trace where wave 1 goes off the ledge, and make the lane its only way.

**Read honestly:**
- **b1:** its verb works in the room by the pilot (3 kills in one burst). DULL A 38.1 is 16 pts under the red, B 50.8 is 6.7 over (mean 44.5
  against 49.2). Air is still 2-8 pts under the red.
- **a8:** the verb is not shown. Its MAIN lines (air, DULL) beat the red; stall is out of the red's band again (51.3 / 49.9), most likely
  because the hatch rail funnels the file to a standing pilot.
- **a4:** its verb works by the pilot; every bot play number is still worse than the red, with a death on B.

## Round 3 (2:22-3:50; branch n65-S3 from d602157a)
The lead's list:
- (1) verb65 on my rows and T's verb-aware pilot;
- (2) a8's pierce RED: trace wave 1 and shape the lane;
- (3) a8's stall in the red's band;
- (4) a4: decide the gallery by the pilots and give it a hand-off.
Then the GATE's one target: `n65verb skip --verbs65` GREEN (refusing the verb costs 25 % of the clear, a clear or deaths).

**Each change and its job:**
- **verb65** on the rows: b1 launch, a8 pierce, a4 parry (b877f6b3).
- **a8, P7's south face to z -360:**
  - `hold52 --spots -5.5,0,-352 --hold 8 --trace` showed wave 1 leaving the ledge east (two into the pit) and in the lane only at about 6 s.
  - At z -359.5 the face sat on the nav cells' centres, so no drop link from the ledge to the lane existed.
  - Now the drops exist, the file is in the lane by 2 s, and n65verb pierce is GREEN (3 bodies, 3 kills) (b877f6b3).
- **b1, GATE M closed** (gg-proxy r2 BLOCKING): every pack comes through GATE W. The ladle is 1 m north; the west wave-1 node is in the
  canyon (8756aa33).
- **a4, P8 THE BELLOWS PAD:** the hand-off from the tell block onto the gallery's south end, in the spitters' fire (656cd3e5).
- **Tried and dropped:**
  - a8: a sluice run on to the ledge (it broke the only walkable route to the exit); a 6-husk wave 1; a hatch gate with one gap (no skip
    gain, a validator ERROR).
  - a4: a return pad (its arc clipped the fence); an east-wall ride (I2 1.08, a new FAIL).

**gg-proxy r2 (Logs/n65/r2/review-gg.md), answered by name:**
- **b1, close GATE M: DONE.**
- **a8 (1), wave 1 off the ledge into the lane: answered by the root cause**, the missing drop link. The nodes stay on the hatch; the trace
  puts all four in the lane by 2 s.
- **a8 (2), husks on the pit floor: not possible** (the validator's walkers never stand on a pit floor, levelgen ERROR in round 2). The pickup
  is on the pit floor by the pad (round 2).
- **a4, the east-wall ride: tried, not kept** (I2 1.08).

**The level-design review r2 (Logs/n65/r2/review-ld.md), answered by name:**
- **b1, THE SILL in GATE W: DONE as a 1 m hump of two ramps** (7ee162ca). A solid 1 m step would stop the walk proofs: the Mover has no
  step-up. "Close GATE M": done (8756aa33).
- **a8, the sluice's east face a ride: DONE** (P14, 7ee162ca).
- **a8, wave 1 in the lane: tried (7ee162ca / d075ee04), not kept** (e252b885). The skip test stayed RED (19.1 s against 14.3 s) and stall
  left the red's band (48.8 on B). The drop-link fix already files them into the lane by 2 s.
- **a4, the east take-off for an east ride:** no east ride is built (I2), so it does not arise.

**Also tried on a8's skip, not kept:**
- a 6-husk wave 1;
- a hatch gate with one gap;
- a charger leading the file (one: 16.1 s against 17.4 s, the only run where the pierce paid; two: 18.9 s against 16.3 s).

**The round's last state: room files at 854034e0** (b1's line65 only after e252b885). Evidence in `<id>/r3/` with the command and the sha
at the top of each file:
- the plain sim set: e252b885;
- the verb-aware flows A / B, the skip test (5 seeds), the verb pilots and the iso pictures: e252b885;
- b1's verb-launch.txt: 854034e0.

| room | skip test (--verbs65, 5 seeds) | verb pilot | verb-aware flow A / B: DULL %, deaths, verb performed | plain pilot A / B (red) |
|---|---|---|---|---|
| b1 | **GREEN**: 27.6 s with the launch vs 39.1 s refused; STYLE above DULL 79.1 vs 41.4 % | launch **GREEN** (burst: 2 hits, 2 kills) | DULL 24.5 / 23.9 (red 54.2 / 44.1), deaths 0 / 0, launch36 18 / 16 | air 57.9 / 55.9 (54.5 / 58.8), stall 31.0 / 34.4 (33.8 / 30.1), DULL 48.5 / 57.3 (54.2 / 44.1), deaths 0 / 0 (1 / 1), clear 37.3 / 35.8 (30.8 / 38.7); room check PASS |
| a8 | **RED**: 23.0 s with the pierce vs 14.5 s refused; STYLE 47.1 vs 51.4 % | pierce **GREEN** (3 bodies, 3 kills) | DULL 52.1 / 55.6, deaths 0 / 0, charged shots 39 / 35 | air 58.9 / 60.2 (29.1 / 32.1), **stall 45.8 / 42.9 (42.3 / 46.0: in band)**, DULL 55.4 / 53.4 (83.5 / 87.5), deaths 0 / 0, clear 17.1 / 18.1; room check PASS |
| a4 | **GREEN**: 17.6 s with the parry vs 25.2 s refused; STYLE 40.2 vs 13.7 % | parry **GREEN** | DULL 61.5 / 70.7 (red 57.5 / 56.4), deaths 0 / 0, parries 17 / 12 | air 52.0 / 51.0 (47.2 / 54.5), stall 27.6 / 37.5 (31.2 / 40.3), DULL 84.8 / 82.4, **deaths 1 / 1 (0 / 0)**, clear 25.4 / 24.2 (15.1 / 15.2); IN VIEW held FAIL (one wave-1 spitter) |

**Read honestly:**
- **b1:** refusing the launch costs 42 % of the clear, and its verb works in the room (two pilots). Its plain-pilot numbers are mixed:
  - stall B is 0.6 over the red's band;
  - DULL's mean is 52.9 against 49.2;
  - the exit is out of the door's view since GATE M closed.
- **a8:** every guard is in the red's band; the pierce lands 3 kills. **Refusing it costs nothing for this pilot**: the verb-aware pilot is
  slower (23.0 s against 14.5 s). I have no room shape that flips it yet. The charger was the one lever that moved it (+8 %).
- **a4:** refusing the parry costs 43 %, the parry pilot is GREEN, and the verb-aware pilot parries 17 / 12 times. The plain pilot still
  dies once per set (red 0 / 0) and is slower; it cannot parry globs.

## The GATE and the fix round (3:54-4:10; branch n65-S4 from fd9c565b)
**The GATE (Logs/n65/verdict-b1.md, verdict-a8.md, verdict-a4.md):** all three FAIL, and no room can PASS on branch C (a Player line not
checked caps a room at UNSURE; DECIDE65 D-8).
- b1 misses its card's MAIN (air >= 59, DULL <= 40) and set B's stall.
- a8's skip test is RED.
- a4 misses DULL and breaks the deaths guard.
- The critic also found that the skip test's "refused" runs in a4 (and b1's seed 2) never refused a press. **a4's skip GREEN is therefore
  not evidence that the parry is needed** until builder T's fix.

**The fix round, for GG's play build:**
- **a4 (f0bbd485):**
  - P8 THE BELLOWS PAD apexAbove 2 -> 0.8, as the lead asked (hold52 --pads: miss 1.06 -> 0 m).
  - The deaths were not the pad's. With the pad fixed, n46flow read deaths 4 / 0; `--per-seed` put each death on a seed whose pilot dwelt
    11.6-14.2 s at the entry door, trading shots with the gallery's spitter over the gallery's edge.
  - So **P1 THE TELL BLOCK is widened to x -2.0 and raised to 3.6 m**: it hides the door and the spawn from the gallery, while the stair's
    foot (the glob's line) stays open.
  - Deaths 0 / 0 on both sets.
- **b1 (57a7d41a):**
  - **P17 THE NORTH SILL** in GATE FW (the level-design review round 3).
  - Both sills' ramp runs are 2 m (26.6 deg). With the review's 4 m runs I1 read 72 %, a FAIL: the gantry mirrors onto both sills. With 2 m
    runs it reads 56 %.
  - The launch pilot from the rail waypoint is GREEN (`--pose -9,2.5,-158.5,0`).

Evidence in `<id>/fix/` (sha 57a7d41a, each file with its command):

| room | room check | pads | plain pilot A / B (red) | verb pilot | skip test (5 seeds) |
|---|---|---|---|---|---|
| a4 | R2 / R5 / R8 / I1 / I4 PASS; IN VIEW held FAIL (one wave-1 spitter) | 1 / 1 PASS, miss 0 m | **deaths 0 / 0 (0 / 0)**; stall 25.3 / 21.0 (31.2 / 40.3); air 54.1 / 49.6 (47.2 / 54.5); DULL 84.5 / 80.0 (57.5 / 56.4); clear 33.7 / 26.6 s (15.1 / 15.2) | parry GREEN | GREEN 21.4 s vs 32.3 s (the critic's tool caveat applies) |
| b1 | PASS (I1 56 %) | 2 / 2 PASS | deaths 0 / 0 (1 / 1); stall 34.0 / 28.1 (33.8 / 30.1: A +0.2); air 54.3 / 57.6 (54.5 / 58.8); DULL 66.0 / 53.1 (54.2 / 44.1); clear 39.1 / 33.2 s (30.8 / 38.7) | launch GREEN from the rail (3 kills) and from line65[0] (1 kill) | GREEN 22.7 s vs 37.0 s |

**Still not good, honestly:**
- **a4:** the walk bot's DULL and clear stay worse than the red (it cannot parry).
- **b1:**
  - its card's MAIN (air >= 59, DULL <= 40) is missed by the plain bot;
  - stall A is 0.2 over the red's band;
  - the exit is out of the door's view since GATE M closed.
- **a8:** unchanged in the fix round; its skip test stays RED.

## The density pass (DECIDE65 D-9, 4:24-4:35; branch n65-S5 from af10b9cb; room files at cb4e5ff6)
GG: "big empty boxes" (gg-proxy's #1, three nights); "don't just randomly plce stuff it needs intension and personality" (2026-10-17).
Rules for the pass:
- dressing and cover, each with a job and a role colour (rule 8);
- the verbs' own pieces unmoved;
- view-only props have no collider, so none sits on a walkable floor where a player would walk through it.

**Pieces added (each with its job):**
- **b1:**
  - the furnace's **chimney** to the roof (landmark: the room's tall mark seen from the door; stone);
  - a **canyon inlay** from GATE W's sill to GATE FW's (route cue on the launch line; accent);
  - two **hanging lamps** under the GATE W and far-yard ceiling lights (fixtures; kit lamp amber);
  - d6's **door frame** and a **cyan wall light lintel at 4.75 m** (the lit exit, seen from the door over both slag lines since GATE M
    closed; gg-proxy r3);
  - an east **pilaster** (wall skin; stone);
  - a north **banner** beside d6 (landmark);
  - **P18 THE CASTING TROUGH**, a 1.0 m low cover in the far yard facing the floor (wave 3's packs), below a rider's line, with its lowWall
    look. It is the one collider added, and it brought set A's stall back into the red's band.
- **a8:**
  - the hatch's **banner** on the north wall (landmark: where the file comes from);
  - a **lane inlay** down x -5.5 (route cue: the file's line and the pierce's aim; accent);
  - a **lane lamp** (fixture);
  - two east **pilasters** (wall skin);
  - the hatch's **hoist chain** (landmark; metal);
  - d10's **door frame** (the lit exit).
- **a4:**
  - a **kiln mouth** in the east wall, 7 m from the gallery's end (landmark: what the spitters guard; amber, kept off the gallery so the
    yellow tell stays the hot colour there);
  - a **stair-top inlay** (route cue where the glob's line meets the gallery; accent);
  - a **fill lamp** (fixture);
  - d4's **door frame** (the lit exit);
  - a **cornice** along the gallery's deck edge (the gallery reads as a walkway).

One validator ERROR along the way: the lintel light must sit flush on the wall, its centre 0.05 m off the surface. Fixed before the commit;
levelgen --walk 0 ERROR.

**Guards at cb4e5ff6** (`<id>/dens/`, each file with its command and sha):

| room | room check | n46flow A / B (red) | verb pilot |
|---|---|---|---|
| b1 | PASS (I1 55 %) | clears 10 / 10; **stall 31.8 / 29.7 (33.8 / 30.1: both in band; A was 34.0)**; deaths 0 / 0 (1 / 1); air 54.0 / 51.6 (54.5 / 58.8); DULL 61.5 / 61.9 (54.2 / 44.1); pads 2 / 2 | launch from the rail GREEN (3 hits, 3 kills) |
| a8 | PASS | clears 10 / 10; stall 45.8 / 42.9 (42.3 / 46.0); deaths 0 / 0; air 58.9 / 60.2; DULL 55.4 / 53.4; pad 1 / 1 | pierce GREEN (3 bodies, 3 kills) |
| a4 | R2 / R5 / R8 / I1 / I4 PASS; IN VIEW held FAIL (as before) | clears 10 / 10; stall 25.3 / 21.0 (31.2 / 40.3); deaths 0 / 0 (0 / 0); air 54.1 / 49.6 (47.2 / 54.5); DULL 84.5 / 80.0; pad 1 / 1 | parry GREEN |

**Not checked:** how the dressing reads in a Player frame (branch C). The iso pictures `<id>/dens/iso.png` are the sim-side view.
