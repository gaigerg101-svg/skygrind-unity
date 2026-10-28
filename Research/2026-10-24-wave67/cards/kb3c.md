# Card kb3c: 3.3c THE TAP RUN -> THE TAP RING (builder S; NIGHT68 r3a; slot kb3c of n67.kiln67; shipped n57-kiln)
Rect [-11, -253.5, 11, -229.5], 22 x 24 x 12 m roofed; exit (-6, 0, -251.5) through door x -8..-4 at z -253.5; **spawn (-6.5, 0, -234) not
moved**. Starts from NIGHT67's r3c file (03d276b4). Evidence Research/2026-10-24-wave68/kb3c/ (r1 .. r3b); patches r1/, r3/, S-r1g.cjs.
**Archetype: RACETRACK** (FORGE67b research.md section 2): the ring round the tapping furnace; one ride, the run wall over the tap run. A
partial fit: the furnace (the central block) is 2.5 m, the family asks 3.5 m or taller.
**Verb: ride** (`verb65: "ride"`). The red (the shipped room on I1-I4, Logs/n68/red-kiln/kb3c/skip.txt): pooled 0.97, bar 1.25. r3a on I1-I4
(kb3c/r3a/skip.txt): `M2 VERB FAIL | room kb3c, verb ride, pooled ratio 0.99 vs mainBar 1.25`, sets 1.21 / 0.86, 15 / 20 (r1g 1.09).
**How the verb wins: the route without it is longer, with a hold at its end (the card critic: "name the hold").** THE HOLD: the last wave's
two spitters (and its two drones) spawn on THE SPOUT TOWER (3.5 m) and stay there: a spitter keeps 6-24 m from you and lobs (config spitter
.minRange / maxRange), where the four swarmers it replaces ran down to the floor (NIGHT67: "the clear is kill-bound"). THE SPOUT PARAPET cuts
the far floor's line to them, so from the floor the KESTREL cannot finish them from where wave 1 is fought; the run wall's line (north, over
the lava) stays open and lands the rider among them. On foot the tower is the far floor and THE TOWER RAMP: the long way (not measured).
**Pieces (r3a; each added, removed or moved by double / half since r3c):**
1. **THE RUN WALL** (ride, cyan, 18 m on the east wall, base on the floor) over **THE TAP RUN** (4 x 10 m; r3a: drained from lava to a dry
   pit 2 m deep, lit by a white pool on its floor): the ride and its gap. r1g's slow seeds and both verb-aware deaths were rides dropping
   into the lava; a fall now returns the rider to the apron for 10 health (the row's fallReturn43, DECIDE43 D-5, never a death), and a body
   in it is ended after 2 s (rooms43). The walkers still go round.
2. **THE SPOUT PARAPET** (added r1f: 1.5 m on the tower's west edge north of the ramp's landing): the hold. **THE SPOUT TOWER** (3.5 m) and
   **THE TOWER RAMP** (7 m along the far wall): the landing, the roost, the walkers' climb.
3. **THE RUN CURB** (added r1: 0.9 m on the run's west rim): r3c's set A death (the walk bot in the run) gone: deaths 0 / 0 on r1 and r1e.
4. **Cut:** THE SKIM WALL (r1f: a second ride face on the fighting floor, the critic; one marked face now), THE SPOUT (r1g: the hull from the
   tower to the roof touched 3 / 20, no job).
5. **Kept:** THE TAPPING FURNACE (6 x 9 m, 2.5 m) with its two climbs 8.9 m apart (P9), the stack, the vents; THE FETTLERS' WALK and its
   ramp; THE TAP HOOD (R5); THE INGOT PILE; the health on the tower.
6. **Waves:** 1 as NIGHT67 (2 drones, 4 husks; the far floor and the tower's front); 2 two drones and two spitters on the tower only (P1; three
   spitters raised stall A to 33.8 % vs the ceiling 33; three kinds were refused by levelgen's budget).
**line65:** (5.5, 0, -233.5) the apron -> (9.6, 1.7, -236.5) the attach -> (9.6, 1.7, -246) along the run -> (8.5, 3.5, -249.5) the tower.
**Lint (the shipped room fails P1, P4, P9: Logs/n67/area-baseline.txt):** P1 the last wave on the 3.5 m tower; P4 every tall solid touched or
cut; P9 the furnace's two climbs 8.9 m apart. r3a PASS 6/6; the decoration checker PASS 9 / 9 (kb3c/r3a/area.txt, deco.txt).
**M3:** `n65verb ride --shoot`: GREEN on r1f, r1g, r3a (kb3c/r3a/m3.txt); r1 seed 1 RED with and without the curb (kb3c/r1/m3-curb-ab.txt).
**Guards on r3a (kb3c/r3a/guards):** stall A 32.3 % (ceiling 33; r1g 35.1), deaths 0 / 1 (ceilings 0 / 2), falls 8 / 17 (returned).
**Aims:** M1 PASS. M2 pooled >= 1.25 (a ride within 0.10 above it is UNSURE), >= 14 / 20 exercised, the refusal fired, STYLE >= min(refusal +
15, max(refusal, 95)). M3 GREEN. M4 the parapet and the curb pass the checker. Guards (the slate): deaths A 0, B 2, stall A 33 %.
**Honest state:** the ride has not beaten its bar on any file (r3a 0.99, r1g 1.09; r1f 1.24 was pre-I1); the per-seed spread (0.67-1.80)
is wider than any change's effect; r3b (wave 1 at the apron) read 1.01 and lost THE TOWER RAMP's job (4 / 20). The hold is the card's lever
left; if r2 reads under 1.25 the room is SHORT for the ride (NIGHT67's shipped-room probe read launch 1.44 on one seed: a verb change needs
its red first, the lead's call).
