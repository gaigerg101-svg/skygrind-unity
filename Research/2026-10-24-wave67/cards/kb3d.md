# Card kb3d: 3.3d THE CRACKED FLOOR -> THE KILN MOAT (builder S, NIGHT68 r1h; slot kb3d of n67.kiln67; shipped n57-kiln)
Rect [-12, -277.5, 12, -257.5], 24 x 20 x 12 m roofed (level 3's smallest); entry door x -8..-4 at z -257.5, exit (0, 0, -275.5); **spawn
(-7.5, 0, -262) not moved**. Evidence Research/2026-10-24-wave68/kb3d/; patches r1/patch-r1*.cjs. Every ratio below is information only.
**Archetype: PIT** (FORGE67b research.md section 2): launch into it; the melee come at you past it. THE KILN MOAT is 108.6 m2 (23 % of
the room, inside the family's 15-30 %).
**Verb: launch** (`verb65: "launch"`). Probe (kb3d/probe/skip-launch.txt, seed 1, n57-kiln): `M1 FAIL | room kb3d, ratio 1.10 vs bar 1.25
(default: no --bars), exercised 1/1`, launch36 4 / 0 / 0; pierce 1.11, ride 0.79. Loadout shotgun, revolver, gauntlet (the pilot switches).
The red (shipped, I1-I4): 1.01, bar 1.25. r1g: `M2 VERB FAIL ... pooled ratio 0.99 vs mainBar 1.25`, 16 / 20 (kb3d/r1g/skip.txt).
**How the verb wins: it pays something nothing else pays.** A body more than 1.5 m under the room's lowest node for 2 s is ended
(design.n43.rooms43 endS 2, pitDropM 1.5, on), whatever its health; a throw that meets a body or a wall bursts for 60 in 3.5 m (design.n36
.launch36). The gun pays 40-150 hp of shots per body. The throw flies along the punch's aim (r1 M3: 11.1 m), and the punch aims at the body
coming at you, so the pit must lie behind the body: here every enemy crosses the moat by one 2 m bridge, and a punch at the bridge's north
mouth more than about 5 degrees off its axis puts the body over the edge. The player who falls in is returned to the bridge's mouth for 10
health, never killed (the row's fallReturn43, Campaign24.cs:95-104, DECIDE43 D-5, as THE GANTRY): data the game has, not a new rule.
**Why not lava (r1-r1c, measured):** THE SPILL as lava beside the causeway killed the walk bot 4 / 3 and 5 / 1 (shipped 0 / 0); spawn31
refused spawns on its rim (8.5 / 9.6 m from the door, min 12). The moat is dry, its near rim lipped, every spawn 12.7-14.2 m away beyond it.
**Pieces (r1f):**
1. **THE KILN MOAT** (dry pit 2 m deep; 2.5 m was refused by levelgen's pit recovery): west of the bridge x -7.5..-1, z -275..-267 (8 m:
   r1g's 4 m was overflown by the M3 throw, 7.9 m), east x 1..6, z -274..-267, and under the kiln wall's lane x 6..11.4, z -271..-267; and
   **THE BRIDGE** (x -1..1, 8 m, on the door-exit line): the throws' landing and the enemies' one crossing. Lit by white pools on its floor
   (a hole with a floor). **THE SPILL** and THE BREACH (the shipped lava) are gone.
2. **THE POURING LIPS** (added, 0.9 m, the moat's north rim either side of the bridge): the near floor's fight never meets the edge; bodies
   punched at the bridge fly off its sides south of them. The collider pieces added.
3. **THE KILN WALL** ride (kept) now crosses the moat onto THE MOUTH LEDGE: a ride over a gap. The breach vent (moved north of the lip)
   throws you over it onto THE CLINKER HEAP.
4. **Moved:** the middle iron plate north of the lips (its cyan pool `marks` it), the east one onto the exit's strip (its pool cut: the checker
   counts two cyan markers; the exit's pool trimmed to 1.8 m for levelgen). **Cut:** THE CRUCIBLE. **Kept:** the heap, its ramp and stack, the ledge and its ramp, both kiln posts (the
   charger's stun posts), the west plate, both liners, the health.
5. **Waves:** every wave beyond the moat (the exit's strip z -276.5, 14.8-16.1 m from the door): 1 and 3 (each with a burrower) on the strip only (r1f: a burrower spawned on the ledge sat under
   it, the walk bot out of time), 2 on the far floor, the ledge and the heap; wave 2's four swarmers -> four husks (the punch kills a 10 hp
   swarmer outright); waves 1 and 3 as shipped (wave 3, the burrower and the charger, is the room's combine job).
**line65:** (3, 0, -265.5) beside the bridge's north mouth -> (0, 0, -267.5) the mouth -> (-3, 0, -270) the throw over the west cell.
**Lint (the shipped room, sha 2dfefc48, no touches): FAIL 2/3** -> P4 0 / 6 (the ledge, the heap, both posts, the liners: touches not
read): judged on the round's touches (r1-r1e PASS 4/4 with them); OPEN (the moat and the lips keep the floor under 12 m); P17 the kiln wall.
**Aims:** M1 every applicable rule. M2 pooled over seeds 1-20 >= mainBar (the lead's, from the red), >= 14 / 20 exercised, the refusal fired,
STYLE >= min(refusal + 15, max(refusal, 95)). M3 `n65verb launch`: "in a pit yes" on line65's throw. M4 the lips (added) and the moved pieces
pass the checker. Guards (the slate): deaths 0 / 0, stall A 39.7 %. r1g (4 m moat): clears 10 / 10, deaths 0 / 0, stall A 39.5 %, IN VIEW
5.0 of 5, falls 18 / 21 (returns); M3 FAIL on r1g and r1h (r1h: a 4-kill burst on the bridge, "in a pit no"). r1h's guards: not measured.
**The NIGHT67 finding it answers:** lava beside the fight killed the walk bot (kb3 19 / 13, v6 15 / 12) and the throws landed where the bot
fights; the hazard on the throw's line was the untested case (NIGHT67-REPORT section 7).
