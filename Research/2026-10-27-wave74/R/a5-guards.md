# a5 THE POUR: guards on the final room file (NIGHT74 R; 2026-10-27 23:20-00:05, tree n74-R; the room file edited at commit time)
Commands, all from the n74-R tree; the generated levels are in the scratchpad (never committed); every output under evidence/a5/ opens with its command and sha.
- **Red (the old a5, level generated with `--slots=b1,a8`):** `node Tools/night/gen-n65-foundry.cjs --run --slots=b1,a8 --out=<sc>/base`, then `ROOMS=a5 LEVEL_DIR=<sc>/base bash Tools/night/n70-red.sh 6 n65-foundry evidence/a5/red-base` (gen-base.txt, red-base-run.txt); `n65-air.cjs` on its plan (red-base/a5/air.txt).
- **NIGHT65's file as it stood:** `bash Research/2026-10-27-wave74/R/room-guards.sh a5 cand0 b1,a8,a5 launch` (cand0-guards.txt, cand0/a5/*).
- **Final (the file plus two pads):** `bash Research/2026-10-27-wave74/R/room-guards.sh a5 cand1 b1,a8,a5 launch` (cand1-guards.txt, cand1/a5/*): gen, n70-red.sh (room check, flow A / B / V, pads), n65-air, n70-throws.sh (20 seeds), launch pilot, skip v2 on seeds 1-20, area with the skip's touches, deco.
- **The rules meter:** `python -I Tools/night/n74-room-rules.py room <file>`: evidence/a5/rules-a5-start.txt (NIGHT65's file, then the old room), and the final file below.

| guard (NIGHT71 5.1; DECISIONS D-3; NIGHT74 brief R2) | red (old a5) | NIGHT65's file | final a5 | read |
|---|---|---|---|---|
| G1 levelgen --walk beyond the base's own | 1 ERROR (the roofless a3, allowed) | same | same | PASS |
| G3 room check: nothing FAILs where the red PASSes | FAIL R5, IN VIEW fallback PASS; PASS R2 R8 I1 I2 I4 | PASS all | PASS R2 R5 R8 IN VIEW held 6.0 of 6, I1 26 %, I2 1.38, I4 | PASS |
| G3 walk GREEN 10/10; deaths at most the red's + 1 | 10/10; A 1, B 0 | 10/10; A 1, B 0 | 10/10; **A 0**, B 0 | PASS |
| G3 stall at most the red's + 10 points (seeds 1-10) | A 42.9 (bar 52.9) | 29.7 | **29.9** (B 38.2, red B 43.7) | PASS |
| nothing lost: n65-air hand-offs at least the red's | 18 (4 of 4 flights on one path) | 33 | **40** (3 of 4 flights handed on) | PASS |
| nothing lost: pads at least the red's | 3 (hold52: pads 2, 3, 4 PASS) | **1** | **3** (hold52: all three launched, landed within 0.03 / 0.09 / 0.03 m) | PASS (this is why the pads were added) |
| nothing lost: ride faces, footholds kept | ride 18 m, I2 2.14; no foothold | ride 14 m, I2 1.38; foothold | same as NIGHT65's | PASS |
| no enemy-kind change | spitter, husk | swarmer, husk, spitter | same as NIGHT65's | **the file's waves differ from n52's (swarmers added, spitters three to one): flagged, unchanged tonight** |
| launch pilot | n/a | GREEN | GREEN: launched body husk #4, the target, dead | PASS (launch.txt) |
| the verb cannot be skipped (skip v2, seeds 1-20) | n/a | M2 VERB PASS 1.29 | **M2 VERB PASS 1.25 vs bar 1.25 (default), exercised 18 / 20 (need 14)**: at the bar, not above it: the pads give a refuser more to do | PASS, fragile |
| rules meter, core | 5 / 5 | 5 / 5 | **5 / 5** (planes 2, free high 3, low 4, loops 7, rear 0, view 0.00 s) | PASS |
| rules meter, extra | 2 / 3 (EXPOSE 82 % RED) | 3 / 3 | **3 / 3** (dash 1.57 s, exposure 50 %, 0 tiers unreached) | PASS |
| label: area lint with touches | n/a | FAIL 4 / 6 | FAIL 4 / 6: P1 (the last wave's spitter nodes are 2.5 m up, not 3.5), P9 (the gantry's 7.5 x 6 m cannot hold two climb tops 8 m apart) | label |
| label: decoration | n/a | 11 of 16 pass | 12 of 18 pass; the FAILs are pieces the walk bot does not touch (the two new pads among them) | label |
| label: walk bot style | DULL 71.0 / 67.9 % | 75.3 / 63.0 | 70.0 / 61.7 %; verb pilot 55.3 % and SAVAGE reached | label |

## What was tried (evidence/a5/; a5-exp.cjs and room-exp.sh reproduce each variant)
1. First pad try, the return pad at the gantry's west end and the lift at the south floor's east side: room check **I4 FAIL** (0.30 m and 0.06 m from proof walk lines); not used.
2. Second try (the final): the return pad at the gantry's east end (8, 2.5, -141.8), the lift on the apron (0.5, 0, -131.3): I4 PASS, meter 5 / 5 and 3 / 3, stall A 29.9.
3. **The lead's RED (n74-t5 vn, b1,a8,a2,a5: row 10 "WallJump events 0"):** S's wallprobe (zones opened: the Player's pick) read a5's west perimeter wall as a 25 m clear run (-8.7, -137.4) that took the pick from a7's 18 m; THE WEST CRATE (1.2 m, x -10..-8.4, z -125.8..-124.2) breaks it into two runs of 12 m: pick 18 m in a7 again, ride 1.32 s, rows 9 / 11 PASS (evidence/a5/wallprobe-a5.txt, d52d57aa5). A 0.6 m rib at the wall first did not break it (the lane is 1.3 m off the face). Full guards on that file (evidence/a5/cand2, `room-guards.sh a5 cand2 b1,a8,a2,a5 launch`): room check PASS; walk A stall 29.1 % (bar 52.9), deaths 0; B stall 39.0, deaths 1; flow-V 10 / 10; pads 3 (all PASS); n65-air hand-offs 50 (red 18); launch GREEN; **skip v2 M2 VERB PASS 1.37 vs 1.25** (was 1.25 at the bar); area FAIL 4 / 6 (P1, P9, labels), deco 12 of 19.
**Not checked:** a Player frame of the room (no Player in a builder tree), the pair, the SPOT, verbs37 on the generated level, the smoke, the room's look; whether the lift's landing on the tap block lets a player camp it out of the spitter's reach (the pilots do not use it).
