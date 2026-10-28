# g5 THE CRANE YARD: guards on the room file (NIGHT74 R; 2026-10-28 00:30-00:55, tree n74-R; NIGHT71 R's last commit d97365e35 plus THE ICE STACK 1 m south)
Commands as g3-guards.md: `ice-guards.sh g5 base old ride` (the red = the shipped room, evidence/g5/base/g5/*), `ice-guards.sh g5 cand0 g5 ride` (NIGHT71's file untouched: evidence/g5/cand0), `ice-guards.sh g5 cand1 g5 ride` (the final: evidence/g5/cand1). The Player's wall pick: `ice-wall.sh` (S's wallprobe, zones opened; evidence/g5/wallprobe-g5.txt).
Slot list for the level: R's room on top of the lead's iceline list: `g4,g1,g6,g5` (with g3: `g4,g1,g6,g3,g5`).

| guard (NIGHT71 5.1; DECISIONS D-3) | red (shipped g5) | cand0 (NIGHT71's file) | cand1 (stack at z -145..-141) | read |
|---|---|---|---|---|
| G1 levelgen --walk | 0 ERROR | 0 | 0 | PASS |
| G3 room check: nothing FAILs where the red PASSes | FAIL R5 (no foothold); fallback IN VIEW PASS 0 of 4 | FAIL R5 only, fallback PASS | **FAIL R5 only, fallback PASS 0 of 4** | PASS |
| G3 walk GREEN 10/10; deaths at most the red's + 1 | A 10/10, 0 | 10/10, 0 | **10/10, 0** | PASS |
| G3 stall at most the red's + 10 points (seeds 1-10) | A 41.1 (bar 51.1); B 46.6 | 48.8; B 42.5 | **A 39.5**; B 54.1 | PASS |
| flow-V (the verb-aware pilot) 10/10, stall at most the red's + 10 | 10/10, 50.6 | **SHORT 9/10** (seed 6 capped 240 s: a drone alive at (4.6, 11.4, -146.4)), 52.1 | **10/10, 48.8** | PASS (this is the fix) |
| nothing lost: n65-air hand-offs | 49 (6 of 7 flights on one path) | 58 (5 of 7) | **62 (5 of 7)** | PASS |
| nothing lost: pads | 3 (hold52 PASS x3) | 3 | **3** (0.01, 0.01, 0.04 m) | PASS |
| ride pilot `n53ridekill ride` | GREEN | GREEN | **GREEN** | PASS |
| patch share (at most 25 % kept) | n/a | PASS 25 % | **PASS 25 % kept** (the limit) | PASS |
| the Player's verbs37 wall pick (zones opened) | g1's 23 m east lane | | **unchanged with g5 in** (g4,g1,g6,g5 and g4,g1,g6,g3,g5: 23 m in g1, ride 1.73 s, rows 9 and 11 PASS; g5's longest run 14 m) | PASS |
| rules meter, core / extra | 5 / 5, 3 / 3 (exposure 58 %) | 5 / 5, 3 / 3 | **5 / 5, 3 / 3** (exposure 57 %) | PASS |
| label: area lint with touches | FAIL (P1, P4) | FAIL 3 / 6 | FAIL 3 / 6: P1 (the last wave's 2 ranged are not all on a 3.5 m plane), P9 (the shelf's 6 x 11 m cannot hold two climb tops 8 m apart), P17 (the two 4 m ice walls are shipped, short on purpose) | label |
| label: skip v2 M2 | UNSURE (no verb65) | UNSURE 6 / 20 exercised | UNSURE: 2 of 20 exercised | label |
| label: `n65verb ride --shoot` | RED | RED | RED ("ride yes 1.9 s, 4.7 m along it, shots from the wall 0") | label |
| label: decoration | n/a | 13 of 16 | 11 of 16 pass | label |

## What was tried (evidence/g5/; g5-exp.cjs and ice-exp.sh reproduce each variant)
NIGHT71's r7 (cand0) held every guard but flow-V (9 / 10, NOT HELD); its note says r6 and r7, each with wave 1 moved for the IN VIEW fallback, "tipped the walk". The cap seed ends with a drone alive against the stack's north-east corner. Variants on the stack, walk A seeds 1-10 and the verb-aware flow (A / V):
- the stack back at the shipped room's z (-145..-141): **A GREEN 10 / 10 stall 39.5, V GREEN 10 / 10 stall 48.8** (the final);
- the stack 10 m high (a 2 m gap under the roof): A SHORT 9 / 10 and V SHORT 9 / 10 (not used).
**Not checked:** a Player frame of the room (no Player in a builder tree), the pair, the SPOT, verbs37 on the generated level (the lead's side vi), the smoke, the room's look; why the corner traps a drone is not isolated (the harness has no position trace for a drone).
