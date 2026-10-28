# g3 THE TIPPLE RIDES: guards on the room file (NIGHT74 R; 2026-10-28 00:00-00:50, tree n74-R; the file is NIGHT71 R's last commit e2cecc1ba, unchanged)
Commands, all from the n74-R tree; generated levels are in the scratchpad (never committed); every output under evidence/g3/ opens with its command and sha.
- **Level 2 has no night slate:** `Research/2026-10-27-wave74/R/slate-r.json` (id, level, verb ride) as NIGHT71's brief says for a room no slate holds; `ice-guards.sh <room> <tag> <slots> <verb>` runs the chain.
- **Red = the shipped room, level generated with `--slots=old`** (`bash Research/2026-10-27-wave74/R/ice-guards.sh g3 base old ride`: evidence/g3/base/g3/*, gen-base.txt, evidence/g3-base-guards.txt); **candidate = `--slots=g3`** (`... g3 cand1 g3 ride`: evidence/g3/cand1/g3/*). cand0 is NIGHT71's r1 file as the lead's tree had it (the IN VIEW fallback FAIL, 3 of 3); cand1 is its last commit.
- Slot list for the level: R's room on top of the lead's iceline list: `g4,g1,g6,g3` (S's g4, g1, g6 are unchanged by R).

| guard (NIGHT71 5.1; DECISIONS D-3) | red (shipped g3) | cand0 (r1, the tree's) | cand1 (the last commit) | read |
|---|---|---|---|---|
| G1 levelgen --walk | 0 ERROR | 0 | 0 | PASS |
| G3 room check: nothing FAILs where the red PASSes | FAIL R5 foothold and I2 (ride 1.28); fallback IN VIEW PASS 0 of 3 | FAIL R5; IN VIEW fallback **FAIL 3 of 3** | FAIL R5 only; IN VIEW ambushed fallback **PASS 0 of 3**; I2 PASS (east ride 2.31) | PASS (cand1) |
| G3 walk GREEN 10/10; deaths at most the red's + 1 | 10/10; 0 | 10/10; 0 | 10/10; 0 (B 0, V 0) | PASS |
| G3 stall at most the red's + 10 points (seeds 1-10) | A 71.6 (bar 81.6) | 67.2 | **62.6** (B 65.3, V 62.6) | PASS |
| nothing lost: n65-air hand-offs / flights on one path | 22 / 2 of 3 | 28 / 4 of 4 | **28 / 4 of 4** | PASS |
| nothing lost: pads | 2 (hold52 PASS both) | 2 (PASS) | **2** (landed 0.09 m and 0.06 m off) | PASS |
| ride pilot `n53ridekill ride` (n70-red's) | GREEN | GREEN | **GREEN** (kill on the wall yes, +0.50) | PASS |
| patch share (the rebuild's own rule: at most 25 % kept) | n/a | PASS 0 % kept | PASS 0 % kept | PASS |
| the Player's verbs37 wall pick (S's wallprobe, zones opened) | g1's 23 m east lane, row 9 PASS 1.73 s | | **unchanged with g3 in** (lists g4,g1,g6 and g4,g1,g6,g3: 23 m in g1; g3's longest run 18 m): rows 9 and 11 PASS | PASS (evidence/g3/wallprobe-g3.txt) |
| rules meter, core / extra | 4 / 5, 2 / 3 (COVER 4 between-height pieces, EXPOSE 67 %) | 5 / 5, 3 / 3 | **4 / 5, 3 / 3 (VIEW RED)** | see below |
| label: area lint with touches | FAIL 2 / 3 (P4) | FAIL 3 / 5 | FAIL 3 / 5: P4 2 of 4 solids touched, P9 (the tipple head is 7 x 7 m, two climb tops 8 m apart cannot fit) | label |
| label: skip v2 M2 | UNSURE (no verb65) | UNSURE (0 / 20 exercised) | UNSURE: 0 of 20 exercised, the verb-aware pilot never rides: the iceline's walk bot meets the fight at the door (the red's stall is 71 %) | label |
| label: `n65verb ride --shoot` | RED (0 shots from the wall) | RED | RED: "ride yes 2.4 s on the wall, 17.4 m along it, shots from the wall 0" | label |
| label: decoration | n/a | 1 of 13 pass | 1 of 13 pass (the walk bot touches 2 of 4 tall solids) | label |

## The meter's VIEW, in numbers
cand1 reads 0 of 3 wave-1 nodes seen at entry; cand0 (nodes beside the hopper, 16 m out) read 3 of 3 in view but FAILed the room check's ambushed fallback ("a line into the entry corridor", 16.1 m from the door). Both can hold only with an occluder between the corridor and the room that is not between the spawn and the room: the corridor eye is 3 m outside the door's centre (z -57), the spawn 4 m inside (z -64), the door aperture 4 m (x -2..2), so at z -68 the corridor sees x within 7.3 m and the spawn only within 5.4 m (53.3 deg half-angle): no node is in the spawn's view and out of the corridor's. A baffle just inside the door would do it and would fill the first frame (gg-proxy's complaint about the old g3: "a flat wall over 55 % of the frame"). The room check's rule is the one the shipped room passes; the meter's is tonight's.
**Not checked:** a Player frame of the room (no Player in a builder tree), the pair, the SPOT, verbs37 on the generated level (the lead's side vi), the smoke, the room's look; the room has 1 prop (the health pickup) and 8 lights, like the shipped room's 2 props: no decoration was added.
