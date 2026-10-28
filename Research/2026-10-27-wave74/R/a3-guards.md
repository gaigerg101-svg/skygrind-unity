# a3 THE RUNNER: guards on the final room file (NIGHT74 R; 2026-10-28 01:00-01:35, tree n74-R; supersedes the SHORT reading of f360cf025)
Commands as a2 / a5 (level 1 has no slate): the red = the old a3 with a2 and a5 in the level (`gen-n65-foundry.cjs --run --slots=b1,a8,a2,a5 --out=<sc>/base`, `ROOMS=a3 LEVEL_DIR=<sc>/base bash Tools/night/n70-red.sh 6 n65-foundry evidence/a3/red-base`, evidence/a3/red-base/a3/*); the candidate `bash Research/2026-10-27-wave74/R/room-guards.sh a3 cand2 b1,a8,a2,a5,a3 ride` (evidence/a3/cand2/a3/*, cand2-guards.txt). cand0 = f360cf025's roofless file as it stood. Variants: a3-exp.cjs, room-exp.sh, room-plan.sh.

| guard (NIGHT71 5.1; DECISIONS D-3) | red (old a3) | f360cf025 (cand0) | final (cand2) | read |
|---|---|---|---|---|
| G1 levelgen --walk beyond the base's own | 1 ERROR (the roofless a3's closedRoom) | same | same: 1 ERROR, the roofless a3's own | PASS |
| G3 room check: nothing FAILs where the red PASSes | R5 PASS, IN VIEW ambushed PASS; I1 FAIL 69 % | PASS | **PASS R2 R5 R8 IN VIEW ambushed (0 of 4 fail) I1 13 % I2 1.37 I4** | PASS |
| G3 walk GREEN 10/10; **deaths at most the red's + 1** (seeds 1-10) | 10/10; **4** (bar 5) | 10/10; **7** | 10/10; **2** (B 1; flow-V 0) | PASS (was FAIL) |
| G3 stall at most the red's + 10 points | A 42.1 (bar 52.1) | 22.9 | **18.8** (B 22.5, V 19.3) | PASS |
| nothing lost: n65-air hand-offs | **101** (14 pieces) | 19 (6 pieces) | **110** (14 pieces: ride 1, deck 1, ledges 10, pads 2; chain 14) | PASS (was FAIL) |
| nothing lost: pads | **2** (hold52 PASS both) | 1 | **2** (hold52: 0.13 m, 0.02 m) | PASS (was FAIL) |
| ride pilot `n65verb ride --shoot` | not run on the old room | GREEN | **GREEN** (4.3 s on the wall, 8 shots from it, 6 kills) | PASS |
| `n53ridekill ride` (n70-red's own ride line) | RED "no ride wall within 24 m of the spawn" | RED | RED: the instrument: zones locked, the same on the red and on every variant (ridekill.txt) | instrument, not read |
| the roof: a3 `roof` false, ring 15 m, no ceiling light, nothing hung from a roof | n52's | yes | **yes** (generator: only the allowed closedRoom row) | PASS |
| GG's 1.3 RIDE line `l1.wallrun` | in n52's a3 | in | **in** (rect [1, -64, 4, -59]) | PASS |
| no wave or enemy-kind change | husk, swarmer | NIGHT65's file | NIGHT65's file (as n52's kinds; counts differ, flagged earlier) | flagged |
| the Player's verbs37 wall pick on b1,a8,a2,a5,a3 (S's wallprobe, zones opened) | b1,a8,a2: 18 m in a7 at (1.1, 0.2, -326.2), ride 1.32 s, rows 9 / 11 PASS | | **the same pick** with a3 in (a3's longest run 1 m; a5's 13 m) | PASS (evidence/a3/wallprobe-a3.txt) |
| rules meter, with the spawn on the floor | 3 / 5, 3 / 3 (REAR, VIEW RED) | 4 / 5, 3 / 3 | **4 / 5 (VIEW RED: the ambush), 3 / 3**; loops 2, exposure 53 % | see the card |
| label: area lint with touches | n/a | PASS 4 / 4 | PASS 4 / 4 | label |
| label: skip v2 | M2 n/a | M2 FAIL 1.14 | **M1 PASS 1.28; M2 UNSURE 1.28 vs 1.25** (exercised 20 / 20; the ride band [1.25, 1.35)) | label |
| label: decoration | n/a | 8 of 12 | 13 of 20 pass (the walk bot does not touch the new blocks and the den hoist) | label |

## What was tried (a3-exp.cjs / room-exp.sh reproduce each; A / B deaths)
- 7 slag pigs and the pillar + the den hoist, pit as it was: **A 6 / B 6** (the hand-offs and pads fixed, deaths one over the bar); + a second health pickup on the swarm floor: **identical numbers** (the pickup changed nothing); the slag pit removed: **0 / 0**; the pit 7 m long (z -76..-69): **2 / 1** (the final); the pit 1.5 m wide: 3 / 4; the mould wall lowered to 1.2 m (wave 1 visible): deaths 4 / 3 but the room check's ambushed fallback FAILs (a line into the corridor), not used.
- The first den hoist target (3.4, -82.8): hold52 FAIL, landed 1.92 m short at the ladle rail's end; (6.5, -82.5) and (7.5, -82.5) landed at x 3.65 (2.85 and 3.85 m off); (5.5, -83) and (6.5, -83.5): 0.02 m. The final is (5.5, 2.5, -83).
- The lava pit was the cause of the deaths (burn plus swarm); it is NIGHT66's round-4 piece to deny low ride starts. Shortened, it still lies under the ride's middle.
**Not checked:** a Player frame of the room (no Player in a builder tree), the pair, the SPOT, verbs37 on the generated level (the lead's side vn), the smoke, the room's look; whether the blocks read as a jungle gym or as clutter in a frame (ten ledges in 20 x 26 m).
