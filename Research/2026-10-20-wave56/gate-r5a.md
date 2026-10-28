# NIGHT56 gate r5, part A (LEVELS): the fresh batched critic's verdicts (build Builds/n56-r5, b6ab1f10)

Rubric: Research/2026-10-20-wave56/critic-gate.md. Evidence: Research/2026-10-20-wave56/gate-r5a-brief.md and the slates
(Logs/n56/slate.json, Logs/n55/slate.json). I read no builder report. The only builder files I read are the notes files the brief names:
Research/2026-10-20-wave56/L.md (L3's per-room notes) and Research/2026-10-20-wave55/R.md (the g7 / g5 pieces). I treat both as claims.
Images: I opened 16 of 30, all from the brief's list. Bot numbers are marked (bot).

Read-only commands I ran myself:
- `python -I Tools/night/n56-room.py planted`: exit 0. `... table`: the output is byte-identical to Logs/n56/bot-r5/room-table.txt.
- `python -I Tools/night/n56-room.py agree` on the l5f / l5i / l5k Player logs.
- `python -I Tools/night/n53-identity.py table` over the 18 rooms: byte-identical to Logs/n56/bot-r5/identity.txt.
- `python -I Tools/night/n55-r-flat.py` on verbs37 x5, on both r5 and r1.
- grep / sed / ls on the logs.
- Read-only git: `git show` / `git diff` to check that the iceline rooms g5 / g7 / g8 did not change between a59e54ac (the g7 / g5 contract
  run) and b6ab1f10, and to check the clear55 code diff between r2 and r5.
- Where a printed line needed explaining, I read the code of n56-room.py (foothold, flank_eyes, in_view) to see what it actually computes.

One disclosure: for A2 I read the a156 log lines before I opened the entry frames. The frame lines below describe only what the images show.

---

## A1. L1 the room checker (attempt 2)

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | (1) The table covers all 18 rooms (Logs/n56/bot-r5/room-table.txt:4-21). (2) The r2 defect is fixed. Every room with no foothold now prints "n/c  not measured: no foothold (R5 FAIL)" (e.g. :12 g4, :13 g5), and :23 prints "IN VIEW PASS 4 / 18 rooms (... a room with no foothold is not measured, not a PASS)". The entry-eye reading is kept apart on :24 ("15 / 18"). (3) "Beside" is now a distance: n56-room.py:341-343 tests each node against the foothold box at BESIDE_M = 10.0 (:63), on top of the not-behind test (:339-340). (4) My planted run: 7 cases, each FAIL or n/c as intended (R2 g2 + spur, R5 a3 footholds out, R8 kb3 one job out, IN VIEW kb3 held, IN VIEW a3 ambushed, a3 with no foothold = "not measured", span p3 "11.9 m from the foothold (> 10: not beside it)"), then "PLANTED: every check FAILs its planted case (trusted)" (room-planted.txt:1-8). (5) R5's test (a box taller than the 1.6 m eye, free of the walls, reaching 4-8 m inside the entry; n56-room.py:166-189) matches levels.md:73 | every doneWhen clause is now computed, and no room passes on a clause it skipped. **Flag:** 10 m is a proposed number. Neither the slate nor levels.md names one (NIGHT56.md:113 says only "beside the foothold (not behind it)"). It is measured on just 2 rooms (a3, p3), because 13 of 18 rooms have no foothold |
| 2 Frames | - | n/a | the entry is a tool; the brief lists no frames | - |
| 3 Nothing lost | - | n/a | no game code | - |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | a tool | - |

**GATE L1: PASS** (r2: FAIL (1) -> r5: PASS).

## A2. L2 spawns in view (attempt 2)

Frames (1920x1080), one line each, from the images only:
- `Logs/feel/n56-r1/d1off/a452/shot-N29-K52-n52-foundry-a3-entry.png` (before): a brown corridor. Two tiny enemies and two white half-formed spawns stand far down the hall by a red glow. "8 LEFT".
- `Logs/feel/n56-r5/l5f/a156/shot-N29-L56-n52-foundry-a3-entry.png` (after): the same corridor, now with green and violet wall bands. Two red-orange enemies stand close at lower right in a red glow, with a large red tell at the right edge and another on the left pillar.
- `Logs/feel/n56-r1/d2off/a452/shot-N29-K52-n52-iceline-g2-entry.png` (before): a blue hall. One red spawn tell in the centre, no enemy formed. "7 LEFT".
- `Logs/feel/n56-r5/l5i/a156/shot-N29-L56-n52-iceline-g2-entry.png` (after): the same hall with bands. Two yellow-shielded enemies and one red-orange enemy on a ledge, about 10-15 m away. Teach line "Punch the shield or flank".
- `Logs/feel/n56-r1/d3off/a452/shot-N29-K52-n52-kiln-kb2-entry.png` (before): a kiln hall. One enemy on the left ledge, one beside the left box, two small red tells centre-right.
- `Logs/feel/n56-r5/l5k/a156/shot-N29-L56-n52-kiln-kb2-entry.png` (after): the same view with rose bands. Two enemies by the left box, and on the right a large red spawn glow with a formed enemy and small ones.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **in view** room-table.txt:23 "IN VIEW PASS 4 / 18" against >= 15. The entry-eye count (:24) is 15 / 18, but it drops the foothold-eye and beside clauses L1 now measures. **a156 agreement**, my run: "AGREE foundry / iceline / kiln: 3 / 12 rooms (25 %; L2: >= 80 %)". **(bot) stall**, Logs/n56/bot-r5/flow.out: :11 foundry 27.7 %, :37 iceline 36.5 %, :60 kiln 25.5 %, against <= 25 % (flow-base.out :11 / :24 / :32 gives 26.2 / 32.0 / 29.2). (bot) Worst room: iceline g3 70.0 % (flow.out:30; it was 64.6 at r2). **Met:** I7 PASS on all five levels (room-table.txt:28-32). Clear times 122.6 / 220.3 / 92.2 / 86.8 / 135.1 s against 130.2 / 233.2 / 92.6 / 95.3 / 141.0 s, so none is worse. verbs37 27/27, 21/21, 31/31, 31/31, 31/31 (summary47.txt:224, 266, 308, 350, 392) | three doneWhen lines miss. Stall misses on every reading. (bot) Note: deaths foundry 15 -> 21 and iceline 2 -> 9 against flow-base |
| 2 Frames | 1 | PASS | the six frames | a3's far enemies are now close, and g2's single tell is now three enemies. kb2 changes less. Confound: colour56's bands are on only in the after frames |
| 3 Nothing lost | 1 | PASS | verbs37 x5 PASS (above); I7 x5 PASS | the rooms are level data (both sides) |
| 4 Look | 1 | PASS | the frames | enemies and tells are red-orange; the shields are yellow (punched) |
| 5 Not basic | 1 | PASS | g2 / a3 after frames | the first frame of the room shows the fight |

**GATE L2: FAIL (1)** (r2: FAIL (1); r5 fails the same line on more counts). Measured reason: in view 4 / 18 strict (>= 15), a156
agreement 3 / 12 = 25 % (>= 80 %), and (bot) stall 27.7 / 36.5 / 25.5 % on foundry / iceline / kiln (<= 25). Smallest change that would pass: a foothold
block (> 1.6 m, 4-8 m inside the entry) in the 13 rooms without one, then the wave-1 nodes beside it, and iceline g3 off the walk line.

## A3. L3 the seven failing rooms, by ADDING pieces with jobs

Frames (1920x1080), from the images only:
- `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g8-entry.png` (before): "THE STAMPEDE". A long aisle between tall blue-banded columns, a sunken channel in the centre, and one red-orange charger on the left with its red charge lane drawn on the floor.
- `Logs/feel/n56-r5/l5i/a156/shot-N29-L56-n52-iceline-g8-entry.png` (after): the same aisle. The bands are violet, not blue. A thin orange-glowing floor strip runs along the left column bases, mostly hidden behind the charger's red lane. Nothing else changed.
- `Logs/feel/n56-r2/l2s/a156/shot-N29-L56-n52-span-p5-entry.png` (before): "SPOOL HALL". A lavender ramp on the left, a pillar in the centre, enemies in red glows on the right.
- `Logs/feel/n56-r5/l5s/a156/shot-N29-L56-n52-span-p5-entry.png` (after): the same frame. The only differences are some band hues and the gun's panel (white here, yellow before). No pad is in view.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **identity FAIL rooms:** Logs/n56/identity-base.txt has 7 (:3 kb2 I4, :6 a3 I1, :10 g3, :11 g4, :14 g8, :15 v2 I2, :20 p5 I4). Logs/n56/bot-r5/identity.txt has **5** (:3 kb2 I4, :6 a3 I1, :10 g3, :11 g4, :15 v2), so g8 (:14 "PASS 1.40/1.10") and p5 (:20 "2.5 m") are fixed. My re-run is identical. 5 > 2. **Contract 14/14 for g8 / p5:** not in the lead's evidence. Logs/n53 holds only contract-025924.md (g7, g5), and L.md:106-107's "14/14" / "15/15" are claims: not evidenced. **Bot contact > 0 per added piece:** no log prints one. flow.out:26's iceline lava counter ("out of lava 720 steps, held at an edge 11401", against flow-base.out:13's 253 / 9050) is level-wide and cannot be tied to THE ASH PIT: not evidenced. **(bot) walk** iceline / span / kiln GREEN 10/10 (flow.out:37, 78, 60): met | 5 of 7 rooms still fail identity. L.md:113 itself says "7 -> 5 ... SHORT". a3 / g3 / g4 / v2 were not built, and kb2's lip was taken out again (L.md:105) |
| 2 Frames | 0 | FAIL | the four frames | someone not told what changed would see only the band colour. g8's pit is a sliver behind the charger's lane, and p5's 1 m pad move is not in the frame |
| 3 Nothing lost | 1 | PASS | verbs37 iceline 21/21 (summary47.txt:266), span 31/31 (:350), kiln 31/31 (:308) | (bot) cost: g8 deaths 0 -> 7 and stall 17.5 -> 26.3 % (flow-base.out:22 vs flow.out:35) |
| 4 Look | 1 | PASS | g8 after | the pit glows red-orange, the danger family |
| 5 Not basic | 0 | FAIL | the frames and the five unbuilt rooms | GG would see no change in the first minute of any of the 7 rooms |

GG-persona line per room (as GG, who said "random lava pits random pillars that serve no purpose"):
- **a3** (nothing built, I1 69 %): "Same mirrored corridor, columns both sides; I run straight down the middle." (r5 a3 entry frame)
- **g3** (nothing built, I2 1.28): "Why ride that wall? Walking the lane is just as fast." (no frame; a156: 1 of 3 wave-1 enemies seen at 9.3 m)
- **g4** (nothing built, I2 1.18): "I walk in and there's one guy 18 m away; the ride is the long way round." (no frame; a156 "seen 1 | nearest 18.5")
- **g8** (THE ASH PIT added): "There's a strip of lava along the wall. Nothing tells me it's a firebox. It's a random lava pit, and it keeps killing me." ((bot) 7 deaths)
- **v2** (nothing built, I2 1.17): "The ride runs over the decks I'm already walking on." (no frame)
- **kb2** (lip added, then taken out; I4 1.50 m): "The pad still sits on my running line; I bounce off it by accident." (r5 kb2 entry frame: unchanged geometry)
- **p5** (pad moved 1 m): "I can't see any difference." (the p5 pair)

**GATE L3: FAIL (1, 2, 5)**. Measured reason: identity FAIL rooms 7 -> 5 (<= 2 wanted); g8 / p5 contracts and per-piece bot contacts
not in evidence; neither changed room reads in its entry frame. Smallest change that would pass: re-cut three of a3 / g3 / g4 / v2 / kb2 with a
piece that does a job, print each contract and contact count, and frame the piece from the door.

## A4. S1-doors (= NIGHT55 P1-doorDark): doors and links lit

Frames (1920x1080), from the images only (the two the meter names as flattest):
- `Logs/feel/n56-r5/vi/verbs37/f1350.jpg`: "THE SHIELD". The player faces a flat dark-blue wall up close, with one banded corner at the right edge. No door, no floor line, no next room.
- `Logs/feel/n56-r5/vp/verbs37/f1550.jpg`: "DOOR OPEN". An almost black, dark-green frame with one thin red-orange vertical line through the reticle. No jambs, no floor edge, no lit room beyond.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n55-r-flat run on r5: foundry 0 flat (lowest 0.0386 f0500), **iceline 2** (f0850 0.0253, f1350 0.0244), kiln 0 (0.0309), span 0 (0.0320), **spire 2** (f1550 0.0224, f1600 0.0269). That is 4 of 180, against 0 wanted. r1 (the before): 7 (vf f0500 0.0197; vi f0850 0.0149, f0900 0.0181, f1350 0.0219; vk f0850 0.0128; vp f1550 0.0203, f1600 0.0250) | 7 -> 4, not 0. Spire f1550 barely moved (0.0203 -> 0.0224) |
| 2 Frames | 0 | FAIL | the two frames | the spire door frame shows no geometry edges and no lit room. There is no door / link frame for foundry, kiln or span ("a frame per level": not evidenced) |
| 3 Nothing lost | 1 | PASS | verbs37 x5 PASS (summary47.txt:224, 266, 308, 350, 392) | - |
| 4 Look | 1 | PASS | the frames | no tint, no flash, HUD text in range |
| 5 Not basic | 0 | FAIL | f1550 | GG still walks through a dark door ("doors not visible ... like dark") |

**GATE S1-doors: FAIL (1, 2, 5); NIGHT55 P1-doorDark: FAIL (1, 2, 5)** (r2: FAIL (1, 2, 4, 5); line 4 now has frames and passes).
Measured reason: 4 flat frames remain (iceline f0850 / f1350, spire f1550 / f1600; 0 wanted), and the spire door is still near-black.
Smallest change that would pass: light spire's f1550 / f1600 link (jambs plus a floor edge plus the next room), and keep the camera off
the wall at iceline f1350.

## A5. P2-clear (clear55, attempt 2)

Frames (1920x1080), from the images only:
- `Logs/feel/n56-r5/c55i/a455/shot-N29-C55-n52-iceline-g2-hold.png` (ON): the g2 hall with bands, "DOOR OPEN" at the top, a "21 m" waypoint, and below the reticle "ROOM CLEAR · UNDER PAR" (white) over "15.3 S · PAR 30 S" (cyan).
- `Logs/feel/n56-r5/c55ioff/a455/shot-N29-C55-n52-iceline-g2-card.png` (OFF): the same view and waypoint, "DOOR OPEN", **no card**, and no bands (the -skygrindOff55 side).

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | summary47.txt:50 (c55i): "GREEN 2; ... holds 2, cards 2". g2: "hold served 0.500 s on the loop's clock (band 0.4-0.6) ... sim time moved during the hold False \| card 'ROOM CLEAR · UNDER PAR \| 15.3 S · PAR 30 S' ... min font 24 px". "par source: n52-iceline.json campaign.rooms[g2].par" (g3 the same: 16.8 S · PAR 25 S). The other levels come from r2's ON sides: Logs/feel/n56-r2/summary47.txt:173 / :189 / :205 / :221 "hold served 0.500 s" with cards "14.0 S · PAR 22 S", "13.5 S · PAR 50 S", "16.1 S · PAR 20 S", "6.8 S · PAR 45 S", and hold frames listed in c55f / c55k / c55s / c55v. git diff af050c9b..b6ab1f10 on UiClear55.cs changes only the off gate (`game.Clear55Beat == null`) | hold, card, par with its source, and a frame per level are all met |
| 2 Frames | 1 | PASS | the ON / OFF pair | the card is there ON and absent OFF, seen without being told |
| 3 Nothing lost | 1 | PASS | :57 c55ioff "RED ... row clear55 OFF (NIGHT54: no clear beat) ... holds 0, cards 0 ... card '' ... alpha 0.00"; :64 c55foff the same ("holds 0, cards 0", alpha 0.00); verbs37 x5 PASS | the OFF side is NIGHT54's look again (r2 printed "cards 2 ... alpha 1.00") |
| 4 Look | 1 | PASS | ON frame | the text is >= 16 px (24 px min); cyan is information; no full-screen effect |
| 5 Not basic | 1 | PASS | ON frame | a time vs par card at every room clear |

Flag: the kiln / span / spire OFF sides were not re-run on r5. The fix is one shared code path, and two levels show it.

**GATE P2-clear: PASS** (r2: FAIL (2, 3) -> r5: PASS).

## A6. P0-g7g5 (NIGHT55 iceline g7 / g5 re-cuts, attempt 2)

Frames (1920x1080), from the images only:
- `Logs/feel/n56-r5/v5i/a356/g7/shot-N56-n52-iceline-g7-mid.png`: a ramp climbs ahead between two red-orange glowing floor grates. A cyan-trimmed block sits beside it, a cyan-lit structure stands ahead right, and three red spawn tells are on the right. A yellow wedge shape (a parryable glob's trail, by its colour) is at a ledge.
- `Logs/feel/n56-r5/v5i/a356/g5/shot-N56-n52-iceline-g5-mid.png`: a banded aisle with a low dark cyan-trimmed box ahead left, a tall banded wall on the right, and a white light column ahead. No runway, chute or crane shape is in view.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | Logs/n56/bot-r5/identity.txt:13 g7 I1 "PASS 52 %", I2 1.22/1.75, I4 4.0 / 4.3 m (before, Logs/n53/identity-e53-final.md:13: 99 %, 1.22/1.26, 3.5 / 2.7). :12 g5 I1 50 %, I2 1.12/4.66/1.25/2.85 (before :12: 76 %, 1.07/1.11/1.25/1.26); I5 iceline 2 -> 1. So nothing is worse. **Contract:** Logs/n56/contract-g7g5.txt:10-11 "iceline g7 ... 14/14 \| PASS", "iceline g5 ... 14/14 \| PASS" at a59e54ac. My read-only compare shows g5 and g7 unchanged in n52-iceline.json and iceline.plan.json from a59e54ac to b6ab1f10 (only g3's waveNodes43 changed). Pieces with jobs in iceline.plan.json: g5 THE CRANE RUNWAY (line), THE POUR CHUTE (climb), THE CRANE HOOK / CAB (cover); g7 THE KEEPER'S TOWER (line), THE COAL CONVEYOR (climb). verbs37 iceline 21/21 (summary47.txt:266). (bot) walk iceline GREEN 10/10 (flow.out:37) | every number line is met; the contract is now evidenced (it was not at r2) |
| 2 Frames | 0 | FAIL | the two mid frames | g7: I can name a ramp climbing over a burning grate (a climb over a hazard). g5: I cannot name the crane runway or the pour chute; the frame shows a box and a wall. So the doneWhen's "a fresh review ... names the piece and its job" is met for g7 only |
| 3 Nothing lost | 1 | PASS | verbs37 iceline 21/21 | (bot) g7 falls 92 -> 136 per 10 walks (flow-base.out:21 vs flow.out:34); deaths 2 -> 1 |
| 4 Look | 1 | PASS | g7 mid | the grates are red-orange danger, the trim is cyan, and the yellow is a parry tell |
| 5 Not basic | 0 | FAIL | g5 mid | as GG: g5 still reads as a corridor with blocks ("random pillars"); g7's climb over the grate would read |

**GATE P0-g7g5: FAIL (2, 5)** (r2: FAIL (1, 2, 5); line 1 now PASS). Measured reason: g5's mid frame shows no named piece. The
crane runway and the pour chute are not in view, so its job cannot be named. Smallest change that would pass: frame g5 at the runway (or
light it) so its job reads from the door.

## A7. P0-mirrors (NIGHT55)

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **identity FAIL items** 5 (bot-r5/identity.txt :3, :6, :10, :11, :15) against <= 7: met (14 at EXT53). **Pieces with jobs:** g7 / g5 (above) and g8 THE ASH PIT (iceline.plan.json pits[5] "job": "hazard"): met. p5 adds no piece (THE CAPSTAN SPRING moved 1 m; flagged, not failed). **verbs37 on the touched levels:** iceline 21/21, span 31/31, kiln 31/31: met. **(bot) walk** 10/10 on iceline / span / kiln: met. **"a452 GREEN on each touched level": not evidenced.** a452 ran only on r1 (Logs/feel/n56-r1/summary47.txt:88 / :125 / :162, foundry / iceline / kiln), before g8's pit and p5's pad, and never on span. r2-r5 have no a452 side. a156 GREEN x5 on r5 (summary47.txt:6, 17, 25, 33, 41) frames only the entry. The notes' "ride / pad / cover contacts > 0 printed" is not printed for any added piece | the a452 line and the notes' contact counts are missing |
| 2 Frames | 0 | FAIL | the brief: "as A3" (g8, p5 pairs) and A6 (g7, g5 mid) | of the four re-cut rooms, only g7's piece reads in its frame |
| 3 Nothing lost | 1 | PASS | verbs37 x5 PASS | level data on both sides |
| 4 Look | 1 | PASS | the A3 / A6 frames | hazards are red-orange |
| 5 Not basic | 0 | FAIL | A3 / A6 GG-persona lines | g8's pit reads as a random lava strip, p5 shows nothing new, and g5 reads as a corridor of blocks |

**GATE P0-mirrors: FAIL (1, 2, 5)**. Measured reason: no a452 run on the shipping rooms (iceline after g8, span ever), and no
per-piece contact counts. In the frames, only g7's piece reads. Smallest change that would pass: a452 on iceline / span / kiln on the
build, the contact count per added piece, and the GG-review frames per re-cut room.

---

## Verdicts

- GATE L1: PASS (r2 FAIL (1) -> PASS)
- GATE L2: FAIL (1): in view 4 / 18 strict, a156 agreement 25 %, (bot) stall 27.7 / 36.5 / 25.5 % (<= 25)
- GATE L3: FAIL (1, 2, 5): identity FAIL rooms 7 -> 5 (<= 2); g8 / p5 contracts and contacts not evidenced; changes not visible
- GATE S1-doors (NIGHT55 P1-doorDark): FAIL (1, 2, 5): 4 flat frames (0 wanted), spire door f1550 still dark
- GATE P2-clear: PASS (r2 FAIL (2, 3) -> PASS)
- GATE P0-g7g5: FAIL (2, 5): g5's piece not visible in its frame (line 1 now PASS)
- GATE P0-mirrors: FAIL (1, 2, 5): a452 not run on the shipping rooms; contacts not printed; only g7's piece reads
