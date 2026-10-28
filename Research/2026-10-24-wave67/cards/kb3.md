# kb3 THE PRESS HALL (level 3 THE KILN, n57-kiln; slot kb3: rect x -10..10, z -225.5..-197.5, h 14; doors d5 / d6 at x -2..2)

NIGHT69 builder R, **FIX candidate r2b** (after a1's level review, Logs/n69/r1/review-level.md ## kb3; a1 measured r1d, 12819ed5):
`node Research/2026-10-25-wave69/kb3/r1/make-kb3-n69r1.cjs r2b`. **Rebuilt from the blank rect:** doors, spawn (-8.5, 0, -203) yaw 0, exit
(0, 0, -224), rect and height kept. **Archetype: CHUTE** (THE PRESS BED, 3 x 10 m, every melee body's way north) between two PITS (THE
QUENCHES, 2 m deep, dry).

**Verb: launch** (NIGHT68 probe, Research/2026-10-24-wave68/kb3/r0-probe/skip-launch.txt: launch36 5 on shipped seed 1). Red 1.13 -> bar 1.33.

**How the verb wins (the design's claim; NOT reached: every variant 0.94-1.12): (a) it pays, (d) the hazard kills enemies but not you.**
A body more than pitDropM 1.5 m under the floor is ended after 2 s (SurvivalSim.N43.Rooms.cs:7), whatever its hp; a player is returned
(fallReturn43, 10 health: without it every run got faster and the ratio fell, r2c 0.94). Along the bed a punch meets the queue (a burst,
60 in 3.5 m: 4-6 kills); off the bed's side a body drops into a quench or slams the bank's face. The probes: a clean throw flies 11-14 m,
and the M3 pilot meets its body on the bed's axis, so no launched body has ended in a quench (R.md cause 2).

**Pieces (each with its job; K = kept from the shipped room exactly, N = new):**
- N **THE EAST QUENCH** pit x 1.5..9.4, z -217..-207, floorY -2; **THE WEST QUENCH** pit x -4.5..-1.5, z -217..-207, floorY -2: a body off
  the bed's edge is ended; the forge wall's ride crosses the east one. Rims lit on the bed's edges and the east quench's north / south.
- N **THE PRESS BED** x -1.5..1.5 (floor): the bodies' single way north (the queue); the health on it (the risky leg).
- N **THE PRESS BANK** deck x -10..-4.5, z -214.5..-207, 2.5 m (short side 5.5 m: not a P9 plane): shuts the spawn's diagonal; its faces are
  burst faces; a stand out of the charger's line.
- N **THE BANK STAIR** (-1.5, -205.75) -> (-7.5, -205.75), w 2.5, rise 2.5, on the apron: the walkers' climb to the bank (GG NIGHT20), from the
  apron only (r1c's ramp from the far floor took the bodies over the bank, past the bed).
- N **THE DROP PRESS** hull x 3..4.5, z -203..-201.5, 3.5 m (not to the roof): the entry's foothold (R5: a1 FAILed without one), the
  charger's stun post on the east apron, a burst face.
- N **THE CRANE RAMP** (-3, -221) -> (5, -221), w 3, rise 4.5: the walkers' climb onto the far gallery; the exit strip runs south of it.
- N **THE HAMMER VENT** pad (-6, 0, -200.5) -> the bank (-7.25, 2.5, -210.5): 3.3 m off walk line 0 (I4: a1 FAILed at 0.20 m).
- N **THE DOOR KERB** (x -10..-2, 1.7 m, noride): keeps the walk pilot off the entry wall's ride (NIGHT57 round 4).
- K **THE FORGE WALL** (ride, cyan, 17 m, face W; air65), K **THE FAR GALLERY** (4.5 m, the spitters' roost; a node deep on it so its lip
  cuts a sight: P4), K **THE QUENCH VENT** pad.
- Waves 1-3 as shipped; **wave 4 eight husks** (was four husks + four swarmers: a swarmer, 10 hp, is never a launch target). Melee nodes on
  the far floor (z -218.5..-224.5). line65 [(-5,0,-204), (-1,0,-206.5), (0,0,-211), (0,0,-216)].

**a1's BLOCKING lines:** R5 and I4 fixed (above). M3 and M2 not fixed: the review's changes (the east quench to z -204, wave 1 husks, line65 to
the mouth) measured as r3a: 0.94, M3 FAIL 3/3, flow A deaths 1 / falls 88, B stall 23.9 %: worse on every line, so not taken.
**Patch share:** 3 / 16 shipped pieces kept = 19 %. **Stays:** the ride face, two pads, the 4.5 m and 2.5 m tiers with a walkers' climb to
each, one drop press (of four), the health.
**Aims:** M1 area PASS; M2 >= 1.33 (>= 14 / 20, STYLE >= refusal - 5); M3 a LAUNCHED body in a pit; M4 deco PASS, patch <= 25 %.
**Guards:** deaths A 0 / B 2, stall A 22.7 %, air65 as shipped. **Measured:** Research/2026-10-25-wave69/kb3/fix-r2b/ (Y1 / Y2 harness).
**Open:** falls into the quenches (flow A 50-69 a 10-seed set; shipped 0): a pit beside the walk, the corner NIGHT69 names.
