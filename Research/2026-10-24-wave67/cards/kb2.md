# Card kb2: 3.2 THE CHARGING FLOOR -> THE ROOSTS (builder S, NIGHT68 r1h; slot kb2 of n67.kiln67; shipped n57-kiln)
Rect [-10, -103.5, 10, -75.5], 20 x 28 x 14 m roofed; entry door x -2..2 at z -75.5; exit (0, 0, -101.5); **spawn (1.5, 0, -80.5) not moved**.
Evidence Research/2026-10-24-wave68/kb2/ (probe/, r1/ .. r1f/); patches r1/patch-r1*.cjs. Every ratio below is information, not an M2 verdict.
**Archetype: GALLERY** (FORGE67b research.md section 2: the parry family, globs come down the lane; the decks 4.5 m up at the lane's far end).
P11 applies and fails by the slot: every point of the rect but the door's north-west corner lies 10.2 m or less from the spawn -> exit lane
((10, -103.5) 10.2 m, (-10, -75.5) 12.5 m), so no roost can stand 12-24 m off it. The lead's call (the critic: CHUTE was a mislabel).
**Verb: parry** (`verb65: "parry"`). Probe (kb2/probe/skip-parry.txt, seed 1, n57-kiln): `M1 PASS | room kb2, ratio 1.36 vs bar 1.25 (default:
no --bars), exercised 1/1`, globs parried 1 / 0 / 0. Entry loadout `owned shotgun, in hand shotgun` (the KESTREL comes at kb2b). The red
(shipped, I1-I4): 1.18, bar 1.38. r1e 1.15 (stall A 20.2); r1g `M2 VERB FAIL ... pooled ratio 1.02 vs mainBar 1.38` (kb2/r1g/skip.txt).
**How the verb wins: the other answer is weaker, and the route without it is longer.**
- Weaker: a spitter has 60 hp. The returned glob flies back to its own shooter (Parry36.cs:90-92, parry36 on) for 2 x 12 = 24 and parry36's
  return burst 40: 64, one parry per spitter. The THUNDERCLAP kills one point blank (8 x 8) but its 8-degree cone is about 2 m wide at 15 m:
  how many pellets land at 15-22 m is NOT measured. Pays too: parry36 heals and refills a dash (not timed).
- Longer: the roosts stand 4.5 m up, 15-22 m from the pack's floor; the walker's way up is one ramp each (the east one 19 m from the door's
  end, 14 m); no pad reaches them. The boost (the room's lesson) and the flue wall's ride are a human's quick ways up.
**Pieces (r1f; each added, removed or moved by double / half):**
1. **THE EAST ROOST** (the charging deck halved, 8 x 8 -> 8 x 5 m, 4.5 m) and **THE WEST ROOST** (added, 8 x 5 m): the spitters' perches,
   flanking the 4 m exit gap. The west roost is the collider piece added.
2. **THE ROOST'S LIP** (added r1h, 4.5 m, 4 x 3 m, x 6..10, z -98.5..-95.5): the flue wall's ride lands on the east roost again (r1-r1g
   ended it over the east lane's floor: room-check I2 1.10-1.11 FAIL, shipped 1.44). **THE DECK RAMP** (9.5 -> 14 m, foot z -84.5): the
   walkers' way up, round the flue wall (19 m in r1f raised stall A to 23.7 %).
3. **THE WEST RAMP** (added, 9.5 m on the west wall; r1c's 19 m raised stall A to 24.9 % vs the shipped 18.6 %, so it stays 9.5).
4. **THE BARROW** (added, 0.9 m, 2 x 3 m, where THE STACK stood): the pack's pivot by the door; OPEN.
5. **Cut:** THE STACK, THE RUN'S POST, THE DECK'S FLUE (floor-to-roof pillars, GG 2026-10-12), THE BARROW RUN, its ramp and THE DRAUGHT,
   THE TIPPING LIP, THE BELLOWS (a pad onto the east roost: the refusal's shortcut, the critic). **Kept:** THE FLUE WALL ride, the boost lesson
   (rect trimmed off the deck ramp), THE SLAG CAKE (moved to the west roost's foot: the small boost), the health, both door liners.
6. **Waves:** as shipped (1: 3 husks, 4 swarmers on the floor; 2: 2 spitters, 3 husks; 3: 4 swarmers, 2 spitters), 2 and 3 on the roosts
   only (r1e-r1g's three lone spitters kited the walk bot on the floor: r1g stall A 31.5 %, seed 10 out of time). **line65:** (1.5, 0, -81) the door -> (0.5, 0, -88) the pack's floor -> (0, 0, -93), facing both roosts.
**Lint (the shipped room, sha 2dfefc48, no touches): FAIL 2/5** -> P1 (the last wave 2 / 10 nodes high): waves 2-3 on the roosts only;
P4 (the flue wall and liners, touches not read): the pillars cut, the rest judged on touches; P9 (the 8 x 8 deck, climb tops 2.0 m apart):
both roosts 5 m deep; P11 (GALLERY): fails by the slot, above. r1g: 5/6 (P11), deco 6 / 6 (kb2/r1g/area.txt, deco.txt).
**Aims:** M1 every applicable rule (P11 above). M2 pooled over seeds 1-20 >= mainBar (the lead's, from the red), >= 14 / 20 exercised, the
refusal fired, STYLE >= min(refusal + 15, max(refusal, 95)). M3 `n65verb parry`: a return kills a spitter (r1e GREEN). M4 the moved / added
pieces pass the checker. Guards (shipped, this tree, r0/guards-shipped): deaths 0 / 0, stall A 18.6 %, room check I2 1.44 PASS.
**The NIGHT67 finding it answers:** g7's parry could not separate the gun from the glob; here the only gun is weak where the spitters stand.
**Honest state:** no rebuild has beaten the shipped room's own parry (1.18): r1 1.09 (pre-I4), r1e 1.15, r1f 1.07, r1g 1.02; 1-3 parries a run.
