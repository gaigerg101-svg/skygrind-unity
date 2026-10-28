# a2 THE RANGE: guards on the final room file (NIGHT74 R; 2026-10-27 22:35-23:45, tree n74-R; the room file edited at commit time, see the commit)
Commands, all from the n74-R tree; the generated levels are in the scratchpad (never committed); every output under evidence/a2/ opens with its command and sha.
- **Red (the old a2, level generated with `--slots=b1,a8`):** `node Tools/night/gen-n65-foundry.cjs --run --slots=b1,a8 --out=<sc>/base`, then `ROOMS=a2 LEVEL_DIR=<sc>/base bash Tools/night/n70-red.sh 6 n65-foundry evidence/a2/red-base` (gen-base.txt, red-base-run.txt). Level 1 has no slate (n70-red.sh's L67 branch is for the other levels), so no SLATE= and no planted verb on the red; `--plant-verb65 a2:pierce` on the red is red-base/a2/flow-Vplant.txt.
- **Final candidate (level generated with `--slots=b1,a8,a2`):** `bash Research/2026-10-27-wave74/R/room-guards.sh a2 cand2 b1,a8,a2 pierce` (cand2-guards.txt; gen-cand2.txt; cand2/a2/*).
- **The rules meter:** `python -I Tools/night/n74-room-rules.py room <file>` (cherry-picked from the lead's c80cc0822 as f084bc4ea: skip that commit at the pick), evidence/a2/rules-a2.txt (the NIGHT65 geometry, read as the one-stand-in and the two-stand-in file, which share it, then the old room), rules-a2-old.txt (the old room) and rules-a2-final.txt (the final file).

| guard (NIGHT71 5.1; DECISIONS D-3; NIGHT74 brief R2) | red (old a2) | final a2 | read |
|---|---|---|---|
| G1 levelgen --walk beyond the base's own | 1 ERROR (the roofless a3's closedRoom, allowed) | the same 1 ERROR | PASS (gen-cand2.txt) |
| G3 room check: nothing FAILs where the red PASSes | FAIL R5, IN VIEW n/c; PASS R2 R8 I1 I4 | PASS R2 R5 R8 IN VIEW held 4.0 of 4, I1 19 %, I4 | PASS (cand2/a2/room-check.txt) |
| G3 walk GREEN 10/10; deaths at most the red's + 1 | 10/10; 0 | A 10/10 deaths 0; B 10/10 deaths 0 | PASS |
| G3 stall at most the red's + 10 points (seeds 1-10) | A 12.2 (bar 22.2) | **A 17.3** (B 27.7, the red's B 11.9, not part of the bar) | PASS |
| nothing lost: n65-air hand-offs; pads; ride faces, footholds | 0; 0; none | 10 (1 flight handed on); 1 (launched, apex 4.5 m, landed 0.07 m off); foothold added | PASS (air.txt, pads.txt) |
| no enemy-kind change | husk | husk | PASS; **waves: the file's six husks, n52's room had two (NIGHT65's file; unchanged tonight), flagged** |
| the verb cannot be skipped (skip v2, seeds 1-20) | n/a | M2 VERB PASS: pooled 1.53 vs bar 1.25 (default), sets 1.53 / 1.46, exercised 20 / 20 | PASS (skip.txt) |
| pierce pilot | n/a | GREEN: one charged shot from line65[0] kills both stand-ins | PASS (pierce.txt) |
| rules meter, core (PLANES, COVER, LOOPS, REAR, VIEW) | 3 / 5 (COVER, LOOPS RED) | **5 / 5** (planes 2, free high 2, loops 2, rear 0, view 0.00 s) | PASS (evidence/a2/rules-a2.txt; re-run at the end of the commit message) |
| rules meter, extra (CROSS, EXPOSE, CLIMB) | 2 / 3 (EXPOSE 96 % RED) | **3 / 3** (dash 1.02 s, exposure 58 %, 0 tiers unreached) | PASS |
| label: area lint with touches | FAIL P4 (no touches read) | PASS 3 / 3 (OPEN 9.5 m, P4 3 / 3, P5) | label (area.txt) |
| label: decoration | n/a | 5 of 6 pass; FAIL THE PRESS PAD (touched 0 / 20 seeds, need 6: the walk bot never uses the pad) | label (deco.txt) |
| label: patch share | n/a (no rooms-base for level 1) | not run | not checked |
| label: walk bot style | DULL 21.1 / 23.6 % | DULL 39.1 / 32.5 %; verb pilot 17.6 % (the red under the same pilot, planted: 42.0 %) | label |

## How the final room was reached (evidence/a2/, room-exp.sh and a2-exp.cjs reproduce each variant)
1. The NIGHT65 file as it stood: room check PASS, but **stall A 30.5 / B 27.1 against the bar 22.2** and, from the meter, LOOPS 1 (bar 2), EXPOSE 62 %, CLIMB RED (the 2 x 2 m feed step is neither a tier nor a step to the meter, so the ramp reached no tier).
2. Variants on the stall, ten seeds each (A / B): one stand-in 22.2 / 27.2 (**n65verb pierce RED: "one body or none: no pierce"**, skip M2 UNSURE 9 / 20 exercised: the verb is not tested with one pill, so not used); one stand-in placed mid-breech 19.7 / 25.3 (same RED); wave 1 on the floor 25.0 / 22.2; n52's two-husk wave 19.5 / 19.0 (room check IN VIEW FAIL: needs 4 expected seen); the pills on the old floor line 43.8 / 38.2; no pills 38.8 / 49.4; wave 1 moved 6 m south: levelgen REFUSED (spawn31: an in-cone node needs 12 m or more from the door).
3. The meter's three REDs, fixed in the room file: the stack free-standing, the feed step merged into the breech and the ramp's top 0.4 m north, wave 2's nodes behind the free stack, the third walk proof round the stack's west side. Result: meter 5 / 5 and 3 / 3, room check PASS, **stall A 17.3 / B 27.7**, pierce GREEN, M2 1.53. Why the stall fell is not isolated: the harness has no position trace for the walk bot's fight steps (`--trace` prints only the cleared-room path); a free stack changes the bot's paths.
The one-stand-in file and the NIGHT65 file are kept as a2-one-standin.json and a2-two-standins.json (the latter = git's version at t0b).
**Not checked:** a Player frame of the room (no Player in a builder tree), the pair, the SPOT, verbs37 on the generated level (the lead's), the smoke, the room's look on any screen.
