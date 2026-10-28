# NIGHT59 builder L: L1 level 4's THE GAP NEST (row design.n58.span58, OFF)

Worktree from port/stage3 at e4aad59a, 21:13-22:16. span58 stays OFF; Design/unity-config.json untouched. The level data built with
`--slots=p2` was used for every number below and is NOT committed (n58-span.json stays n52-span's beyond the names, as NIGHT58 EXT left it).

## (1) The checkers with the level as an argument (commit a8c08840)
- `python -I Tools/night/n59-l-room.py <px>-<level> table|room <id>|identity|planted`: n57-l-room.py without 'kiln'. Openings: n52's
  openings53.json per level, overlaid by `Design/levels/<px>/openings<NN>.json` (new: `Design/levels/n58/openings58.json`).
- `bash Tools/night/n59-l-contract.sh <px>-<level> [rooms|planted]`: n57-l-contract.sh with the plan, report, row (design.<px> whose
  levelTo is the level), off flag (--off<NN>) and slots file (new: `Design/levels/n58/slots58.json`, p2's base = n52's p1, the fight
  before the link it replaces, as NIGHT57's slot DE) read from the argument. Output: Logs/n59/L/contract/<level>/.
- The planted case on n57-kiln: n59-l-room.py's table is byte-equal to n57-l-room.py's; its planted output equals n57's beyond the
  label; the contract's jobs file and its kb3 row are byte-equal to n57-l-contract.sh's on the same outputs
  (`| kiln kb3 | kb3 | 20 x 28 x 14 | 2.68 (15 pc) | 3 | 17.0 | 2 | 1.10 -> 1.85 (0.8) | 10/10 / 10/10 | 0 | 20.5 | 32 (3) | 2/2 | 14/14 | PASS |`).
  On n58-span every room planted case FAILs; the contract planted (p1 without health / pads) FAILs on both levels.
- Fix found while proving: span's first fight room p1 has no entry door (entered from the start), so the I1 plant uses the first fight
  room with an entry door (kiln: kb2 as before; span: p2).

## (2) The put-at 0 -> 3 cause (one-line print in Harness.N46.Flow.cs, worktree only, NOT committed)
All 3 (seeds 1-10, NIGHT58 EXT's build) were in **p3, not p2**: p3 cleared and not left in 15 s, the bot at (-7.4, 0, 25.4) twice
(against p3's foothold pillar x -7..-6.4, z 11..28) and (-0.1, 1.6, 6.6) once, all -> p4. Seeds 11-20: 0. p3's geometry is n52's; the
bot arrives from a fight now (seed-dependent end positions). After cut 2: 0 / 0.

## (3) The cuts (each measured: n46flow --level n58-span seeds 1-10 / 11-20; the contract; the room table)
| number | NIGHT58 EXT build (c1 = cut 1 only) | cut 2 | cut 3 |
|---|---|---|---|
| contract p2 C n/m | 7/14 | 10/14 | 11/14 (pad52 PASS, miss 0.08 m; min hold hits 2 over 8 spots) |
| room table p2 (R2 R5 R8 IN VIEW I1-I4) | PASS | PASS | PASS (I4: 1 pad, 4.5 m off the walk lines) |
| I7 openings | FAIL (p1, p2 both vista) -> cut 1 PASS | PASS | PASS |
| walks | 10/10 / 10/10 | 10/10 / 10/10 | 10/10 / 10/10 |
| put at the next room | 3 / 0 | 0 / 0 | 0 / 0 |
| stall (level) | 13.4 / 14.1 % | 12.1 / 13.3 % | 12.1 / 13.3 % |
| deaths | 0 / 1 (p5) | 0 / 1 (p5) | 0 / 1 (p5) |
| p2 falls | 61 / 79 | 51 / 70 | 51 / 70 |
| p2 clear | 34.9 / 36.9 s | 35.4 / 35.4 s | 35.4 / 35.4 s |

Base (n52-span, Logs/n58/flow-base*.txt): stall 14.5 / 13.8 %, deaths 0 / 2, put-at 0 / 0, void-room falls p3 55 / 78, p5 42 / 57.
- Cut 1: p2's opening vista -> held (openings58.json): I7 FAILED with p1 (n52's vista room) before it; p3 is ambushed.
- Cut 2: THE SPLICE LANDING (deck 3 x 6 m at 5 m beside the side gantry: the middle tier, a w1 node at 5 m, the room's health) and the
  near deck's lip rails (1.1 m; the pull's lane x -2..2 open). Contract 3 tiers / 6 spawn tiers / 6 health pickup: FAIL -> PASS.
- Cut 3: THE SPLICE SPRING (a pad from the landing up onto the gantry head): the contract's 5 upward line. The bot does not use it
  (flow equal to cut 2).

## Not met / not built
- MAIN not met: the contract's **1 size** (36 x 50 x 22: long side band 20-32, height 10-16) and **2 density** (>= 2.5 per 100 m2 =
  45 pieces at 1800 m2) cannot pass in slot P2 as one room: door to door is 50 m. The next step is the slot split (a <= 32 m room plus an
  approach link, as gen-n57-kiln.cjs's slot DE made two zones of one link). **5 ride wall** is against the card: n52's two noride liners
  keep the pull the one fast line across THE GAP (a GG / lead call, not made here).
- Not checked: any Player frame (a158, FeelReel.N58.Level.cs not written), verbs37 on n58-span, the `n38 gates` on p2-gateA.
