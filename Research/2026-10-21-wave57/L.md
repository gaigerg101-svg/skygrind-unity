# NIGHT57 builder L: level 3 THE KILN rebuilt from scratch as n57-kiln (row design.n57.kiln57)

Builder L's notes (NIGHT57.md section 5 L; t0 09:33:47). Every number below was printed by the command named beside it; "not checked"
where it was not. Bot numbers are the walk pilot's (n46flow), never a person's.

## L0 the checkers read n57 (09:58-10:15)
- `Tools/night/n57-l-room.py`: imports Tools/night/n56-room.py and n53-identity.py UNCHANGED and points their three hard-coded n52 paths
  (the plan, the level file, the openings sidecar) at `--prefix n57` (default): `table`, `room`, `identity`, `planted`. The openings for
  n57: `Design/levels/n57/openings57.json` (a new file; n52's openings53.json untouched).
- `Tools/night/n57-l-contract.sh`: n52-contract-all.sh's jobs and table (n52-room-check.py unchanged) on n57-kiln; the base is
  design.n57.kiln57.levelFrom (n52-kiln) with `--off57`; a new room's base is the n52 room its slot replaces (`Design/levels/n57/slots57.json`:
  A/B/C replace kb2 / kb2c / kb3 in place; the DE pair replaces the link kb3b, base kb3), its row read under the new id.
- Planted (each must FAIL; L-091): `python -I Tools/night/n57-l-room.py planted`: the old tools' own seven cases still FAIL ("PLANTED:
  every check FAILs its planted case (trusted)"), plus seven n57 cases on kb2 (R2 a 3-cell spur, R5 the footholds out, R8 one job out,
  IN VIEW wave 1 at the door, I1 the room plus its mirror 100 %, I4 a pad on walk line 0, I7 kb2 = kb2c): "PLANTED n57: every check FAILs
  its planted case (trusted)". `bash Tools/night/n57-l-contract.sh planted` (kb2 without its health and pads): C 8/10 FAIL (trusted).
- No shared file was edited for L0.

## L2 the generator (10:05-)
- `Tools/night/gen-n57-kiln.cjs` reads n52's plan (never writes it), writes `Design/levels/n57/kiln.plan.json`; levelgen; `--finish` writes
  `Design/levels/n57-kiln.json` + the StreamingAssets copy (data.name n40-kiln as n52's). `--slots=` (no ids) builds every slot n52's:
  `--check` then prints "keys differing from n52-kiln.json beyond the names: none" (10:12): the pipeline reproduces n52-kiln exactly.
- Asserts: every room not rebuilt is n52's bit for bit (kb1, kb1b, kb2b, kb4, k5 never rebuilt), no other plan key changed, no new room
  copies an n52 box, every piece of a new room has a `job` naming its card field ("<field>: <what>") and a `why`.

## L3 the rooms
### kb2 THE CHARGING FLOOR (slot A) - PASS (10:15-10:25)
- levelgen 0 errors (after two fixes: the exit on the slag cake's edge, two nodes inside 12 m of the spawn in the entry's cone).
- `python -I Tools/night/n57-l-room.py room kiln kb2`: R2 0 dead-end cells of 153, R5 foothold (the flue wall, 5-22 m in), R8 0 untagged
  of 12, IN VIEW vista 7.0 of 7 from the foothold's flank; I1 15 %, I2 walk 24.4 / ride 17.0 = 1.44, I4 clear: ROOM kb2 PASS.
- `bash Tools/night/n57-l-contract.sh kb2`: C 14/14 PASS; density 2.50 (14 pc / 560 m2); 3 planes (0 / 2.5 / 4.5); ride 17.0 m; dwell
  1.95 -> 1.75 s (grace 0.7); clears 10/10 / 10/10; deaths 0 (seeds 1-10); stall 16.2 %; hold 4 spots min 26 hits; pads 2/2; bot clear median
  37.3 s (n52's kb2: 45.6 s, stall 27.7 %, deaths 6 over the same 10 seeds).
- The worm's beats on n57-kiln: `n47b beats --level n57-kiln`: still: glimpse 1, pass 1, cameo breaches 2, hits 2; jump: cameo breaches 2,
  hits 0 (n52-kiln: the same numbers).

### The a157 reel (10:20-10:26)
- `Assets/Scripts/Game/FeelReel.N57.Level.cs` (+ .meta): a156's room meter for n57 (per fight room: the entry frame, wave 1 seen / alive,
  nearest, drawn, the room id, the card title, the worm's beat counters). `bash Tools/night/dn-check-wt.sh <scratch>`: 0 errors in all four
  projects. Not run (builders run no Player).

### Slot DE: kb3c THE TAP RUN + kb3d THE CRACKED FLOOR (10:27-11:06)
- The slot replaces n52's link kb3b (THE SPOUT: a climb over a 4 m pit; the climb lesson stays kb2b's) with two rooms and the door d6b
  (x -8..-4, z -257.5..-253.5); d6 / d7 keep their rects (their `between` names the new rooms). kb4 and k5 unchanged (asserted).
- kb3d first try: n56-room PASS (after the opening was set to held: the card's), contract C 14/14 (dwell 0.40 -> 0.55 s, grace 0.8; clears
  10/10 / 10/10; deaths 0; stall 41.2 %; hold 3 spots min 13 hits; pads 1/1).
- kb3c took four cuts: (1) the furnace as the foothold (5 m in) and its 7.5 m ramp: levelgen "not reached by walkers" (too steep) and a
  dead-end pocket; (2) the furnace 8 m in on a 9 m ramp: R2 pocket at the ledge; the contract's dwell 0.40 -> 1.35 / 1.75 s FAIL
  (hold52 --dwelltrace: the walk pilot ran the ring's north crossing past the door); (3) the furnace down to the mid tier (2.5 m, 5 m ramp),
  the fettlers' walk the high tier, a free-standing run wall: dwell 1.10 PASS but an R2 pocket behind the wall's near end; (4) the run wall
  on the east wall, THE TAP HOOD (5 x 4 m, 1-5 m in, flush to the door's east jamb) the foothold: the ring's north crossing goes south of it.
  Now: n56-room PASS (R2 0 of 148, R5 the hood 3-7 m in, IN VIEW vista 6.0 of 6; I1 31 %, I2 1.40, I4 clear); contract C 14/14 (dwell
  0.40 -> 0.45 s; clears 10/10 / 10/10; deaths 2; stall 35.6 %; hold 4 spots min 19 hits; pads 2/2).
- Level: I5 1 ring room (kb3c) PASS; I7 openings kb2 vista, kb2c ambushed, kb3 held, kb3c vista, kb3d held PASS.
- The worm's beats with slots A + DE: glimpse 1, pass 1, cameo 2 / hits 2 (still), cameo 2 / hits 0 (jump): as n52-kiln.
- Stall per room is high in kb3c (35.6 %) and kb3d (41.2 %) against n52's kb2 27.7 / kb2c 12.3 / kb3 27.2 (the level guard: 25.5 % + band).

### The level walk after r1 (11:07-11:17; bot, n46flow --level n57-kiln, seeds 1-10 / 11-20)
- r1 (A + DE): clear 147.9 / 147.2 s (target 150-240: SHORT 2.1 s); stall 25.8 / 23.6 %; deaths 13 / 9 (n52-kiln --off57: 8 / 11; clear
  92.2 / 87.0 s; stall 25.5 / 26.8 %). The deaths sat in kb3c (7 / 5) and n52's kb3 (6 / 4 against 1 / 7 in n52-kiln): a bot with 0 deaths
  in the new kb2 reaches kb3 hurt (a death respawns it full).
- kb3c's wave 2 one drone fewer (wave 1 also one husk fewer read dwell 0.40 -> 1.25 s against the 1.2 s limit: kept at four husks):
  contract C 14/14, dwell 0.80, deaths 3.

### Slot B: kb2c THE CLINKER BRIDGES (11:20-11:38)
- Openings re-cut so B can be a vista: kb2 held (the same meter as vista: >= 4 seen; the card's "ground you take"), kb2c vista; I7 PASS.
- First cut: IN VIEW 1.2 of 5 (the near ramp rising across the view), I1 74 % (the two bridge + ramp bands mirrored), the spawn on the
  apron plate (levelgen: no walker reach from it). Round 2: the near bridge 1.5 m with its ramp on its far face, the spawn on the ash: n56-room
  PASS (IN VIEW 5.0 of 5, I1 55 %, I2 2.18); contract dwell 0.20 -> 1.40 s FAIL (hold52 --dwelltrace: the pilot circled the crane mast in the
  doorway, riding its faces). Round 3: the mast 3-5 m east of the door's axis, 0.65 m off the east liner (no way round its north side):
  contract C 14/14 (dwell 0.20 -> 0.55, grace 0.5; clears 10/10 / 10/10; deaths 0; stall 26.4 %; hold 3 spots min 19; pads 1/1).
- Level: I5 2 ring rooms (kb2c, kb3c: the limit) PASS; I7 PASS; IN VIEW 5 / 5 rooms.

### The level walk with A + B + DE (11:40-11:49; bot)
- clear 177.1 / 179.7 s (MAIN band 150-240 PASS); stall 26.0 / 25.8 % (base 25.5 / 26.8: inside 10 %); deaths 9 / 12 (base 8 / 11: +1 each,
  +10.5 % over 20 seeds: at the band's edge); per room deaths kb2 0 / 0, kb2c 0 / 0, kb3 (n52's) 6 / 6, kb3c 3 / 6, kb3d 0 / 0.
- Contract on the four new rooms (one run, 11:49): kb2 14/14, kb2c 14/14, kb3c 14/14, kb3d 14/14 (table in the report).
- Beats: glimpse 1, pass 1, cameo 2 / 2 hits (still); cameo 2 / 0 hits (jump): as n52-kiln.

### Slot C: kb3 THE PRESS HALL (11:53-12:21)
- Five cuts, all on THE ROOM CONTRACT's dwell (n52 kb3's 0.40 s, limit 1.2 s) and one pad: (1) a liner equal to n52's (the generator's
  "no n52 box" assert refused it: re-cut), a gallery node off the grid; dwell 1.15 but the hammer vent's arc met the third press (pad 1/2);
  (2) the vent between the west presses: dwell 2.50 (dwelltrace: the pilot stuck between the first press and the liner); (3) the press
  0.65 m off the liner and a fourth wave (the level's clear on seeds 11-20 was 146.4 s with three): 1.60; (4) the spawn 9.5 m in on the nave:
  1.85; full noride liners and a noride first press: 1.45; (5) the spawn 8.5 m from the door (n52 kb3's T-119 distance), the press gantry
  from 8 m in: dwell 1.10 PASS.
- Now: n56-room PASS (IN VIEW held 7.0 of 7, I1 / I2 2.21 / I4 PASS); contract C 14/14 (clears 10/10 / 10/10; deaths 0; stall 19.1 %; hold 3
  spots min 56; pads 2/2). The cameo card ("When the floor glows, jump.") is n52's lesson row; the beats print as n52-kiln's.

### The level with all five combat rooms new (12:22-12:30; bot, seeds 1-10 / 11-20)
- clear 168.2 / 171.9 s (MAIN 150-240 PASS); deaths 2 / 3 (n52-kiln 8 / 11); stall 19.0 / 21.5 % (25.5 / 26.8); per room deaths kb2 0 / 0,
  kb2c 0 / 0, kb3 1 / 1, kb3c 1 / 2, kb3d 0 / 0. The level walk's summed dwell is 18.8 / 16.8 s (n52-kiln 5.7 / 3.5): not a guard, but the
  bot spends more time near the doors of the new rooms (every room's own contract dwell passes its grace).
- Contract, one run (12:29): kb2 14/14, kb2c 14/14, kb3 14/14, kb3c 14/14 (deaths 4), kb3d 14/14. n57-l-room.py planted: both sets FAIL as
  they should. Beats: glimpse 1, pass 1, cameo 2 / 2 (still), cameo 2 / 0 (jump).

## verbs37 on n57-kiln (the lead's r3 Player run, 12:41; Logs/feel/n57-r3/summary47.txt `== vk57`): 26/28, RED 40 / 41, KESTREL 25-27 SKIP
- Cause (read, 12:38-12:48): the reel's staging, not the level. FeelReel.N35.Verbs.cs Open35 stages the ground verbs at the FIRST room (plan
  order) whose spawn has a clear lane >= 14 m: on n52-kiln that is kb2b (n52's kb2 crucible blocks the lanes), on n57-kiln the new kb2 (a
  ~21 m lane down the charging floor). kiln40's award {kb2b: revolver} is given on entering kb2b, so staged in kb2 the reel never gets the
  KESTREL: 25-27 SKIP by the reel's n40Start rule; row 34 swaps shotgun -> gauntlet (n52: -> revolver); rows 40 / 41 punch with the
  gauntlet in hand (weapon -1), which Combat.cs StepPunch drops (MeleeBlocked: no melee with the gauntlet in hand): parry36 0, launch 0.
- The level keeps the KESTREL: kb2b n52's bit for bit (asserted), the award keyed by its id; a run reaches kb2b before kb2c.
- Not fixed in L's files (moving kb2's spawn to kill its 14 m lanes would re-cut the room's checks with no time to re-verify, and bend the
  room to the reel); the small fix is the reel's (the hand back to a gun before row 40, or Open35 skipping rooms before the first award room).

## Not checked
- Anything in a Player (verbs37 on n57-kiln, a157, a452, frames): builders run no Player; the lead's reels.
- The ride landings on 4.5 m tiers (kb2, kb2c, kb3) are graph checks (I2) and the walk pilot's; whether a person can ride onto them: not checked.
- FeelReel.N46.Wyrm.cs names room kb3b (an old reel's spot): on n57-kiln that room is gone (the reel is not this night's).
