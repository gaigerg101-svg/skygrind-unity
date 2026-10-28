# NIGHT56 gate r2: the fresh batched critic's verdicts (build Builds/n56-r2, af050c9b)

Rubric: Research/2026-10-20-wave56/critic-gate.md. Evidence: Research/2026-10-20-wave56/gate-r2-brief.md, the slates (Logs/n56/slate.json,
Logs/n55/slate.json). I did not read builder reports, except Research/2026-10-20-wave55/R.md where the brief names it as the list of pieces.
Images opened: 28 of 30 (all from the brief's list). Read-only commands I ran: `n56-room.py planted`, `n56-room.py agree` (l2f / l2i / l2k),
`n56-colour.py` on v1f, v1i, v1k, v1s, v1v, v1foff and v1koff, `n55k-teach.cjs` (r2 verbs37 x5), `n55-r-flat.py` (r2 verbs37 x5), plus ls, grep,
sed and git log. Where a printed line needed explaining, I read the code (n56-room.py in_view / foothold / walk_cells, UiClear55.cs,
Smoke55Tests.cs) to see what it actually measures. I did not treat its comments as evidence. Bot numbers are marked (bot).

Two problems with the blind pairs (for the lead):
- **The pair folders leak the key.** In both folders each file's size and mtime match its source frame. For example,
  Logs/n56/pairs-r2-kill/pair-1-1B.jpg is 388524 B, the same as Logs/feel/n56-r2/k2on/a355/shot-N55-K-debris.jpg. I listed the folder before I
  looked at the images. My picks below come only from what the images show, but anyone who lists the folder can un-blind the pairs.
  Re-encode the pair files and reset their mtimes before the next gate.
- **The kill pair tests two rows at once.** Its two frames differ in debris (kill55) and also in the wall bands (colour56; the OFF side ran
  -skygrindOff55, which turns both off). So the pair can be told apart by the walls alone.

---

## A1. P0-pick

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | `git log e7d5b59b..6b1c00c9`: 13 commits, each "(cherry picked from commit ...)", in the order of HANDOFF.md:8-12 (26121bd5 fd71daf1 169e162c / 91f9c48e 47c64d8b 5fe9b6cc 283f3c41 / 3c8fd885 1b996462 / edfee264 876dc97e e25d89bb 34b55c1d); 8288aec2 skipped, with the reason in DECIDE56.md D-3; NIGHT56-LOG.md:22-23 "config copies identical; golden 39 sets / 48094 records, 22 replays (22 exact), 0 failing, 0 mismatches; fingerprints plain / --off54 / --off55 EQUAL"; NIGHT56-LOG.md:26 "golden 0 mismatches, 22/22 exact; fingerprints plain / --off55 / --off56 EQUAL; Editor compilationFailed false, consoleErrors 0"; Builds/n56-r1/SKYGRIND.exe at 00:24 (ls), Logs/n56/build-r1.out "status completed" | every commit is picked or skipped with a DECIDE line; the --off56 and Editor-compile gates come from the shared commit's gate, which contains the pick |
| 2 Frames | - | n/a | brief: does not apply to a pick | - |
| 3 Nothing lost | 1 | PASS | Logs/feel/n56-r1/summary47.txt "verbs37: PASS: total 27/27 / 21/21 / 31/31 / 31/31 / 31/31"; fingerprints --off55 EQUAL | the NIGHT54 side is unchanged |
| 4 Look | - | n/a | no visual change | - |
| 5 Not basic | - | n/a | brief | - |

Caveats (not scored): build-r1.out prints "result undefined; errors undefined". The exe exists and r1's sides ran with 0 NREs.

**GATE P0-pick: PASS**

## A2. L1 the room checker (Tools/night/n56-room.py)

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | Logs/n56/bot-r2/room-table.txt:1-21 prints the table over the 18 rooms. My `planted` run prints all 5 cases (R2 g2, R5 a3, R8 kb3, IN VIEW g4 held, IN VIEW a3 ambushed) as FAIL, then "PLANTED: every check FAILs its planted case (trusted)", exit 0. R2's graph includes rides and pads (n56-room.py:368-382); R8 reads job / job36 (:459). **But IN VIEW does not measure the doneWhen's clauses:** (a) for ambushed rooms, "beside the foothold" is coded as "not behind the foothold, seen from the entry eye" (:328-331). There is no distance to the foothold, and the test is skipped when R5 finds no foothold. Even so, the PASS text says "beside the foothold" for 6 ambushed rooms where R5 FAILs (kb2c, b1, g3, g5, g8, v2). (b) For held / vista rooms, "seen from the foothold's eye height" falls back to the entry eye when there is no foothold ("(no foothold flank)": kb2, a5, g4, g7, v5, v6, p5). | 11 of the 16 IN VIEW PASSes do not measure what the doneWhen asks, and 6 of them print a claim that is false |
| 2 Frames | - | n/a | a tool (the brief lists no frames) | - |
| 3 Nothing lost | - | n/a | no game code | - |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | a tool | - |

**GATE L1: FAIL (1)**. Smallest change that would pass: when R5 finds no foothold, IN VIEW prints "not measured: no foothold" and does not count
as a PASS. For ambushed rooms, test each wave-1 node's distance to the foothold box (use the threshold named in levels.md or the slate).
Then re-run the table and the planted cases.

## A3. L2 spawns in view

Frames, described before reading any numbers:
- `Logs/feel/n56-r1/d1off/a452/shot-N29-K52-n52-foundry-a3-entry.png`: a brown and teal corridor with a cyan light column. Two or three tiny enemies stand far down the hall beside a red glow. "8 LEFT".
- `Logs/feel/n56-r2/l2f/a156/shot-N29-L56-n52-foundry-a3-entry.png`: the same corridor, now with green and violet wall bands. Two large enemies stand close at bottom right in a red spawn glow, and a red tell sits on the left pillar.
- `Logs/feel/n56-r1/d2off/a452/shot-N29-K52-n52-iceline-g2-entry.png`: a blue hall. One red spawn tell in the centre, no enemy formed yet. "7 LEFT".
- `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g2-entry.png`: the same hall with green bands. Three enemies (two with yellow shields, one on the ledge) and a tell, about 10-15 m away.
- `Logs/feel/n56-r1/d3off/a452/shot-N29-K52-n52-kiln-kb2-entry.png`: a kiln hall. One enemy on a left ledge, one beside the left box, two small red tells centre-right.
- `Logs/feel/n56-r2/l2k/a156/shot-N29-L56-n52-kiln-kb2-entry.png`: the same view with rose bands. Two enemies by the left box, and two or three in a red glow on the right.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | room-table.txt:23 "IN VIEW PASS 16 / 18" (on the meter judged in A2); room-table.txt:27-31 I7 PASS on all five levels; my `agree` run "AGREE foundry / iceline / kiln: 12 / 12 rooms (100 %)"; **(bot) stall** Logs/n56/bot-r2/flow.out:11 foundry 27.7 %, :34 iceline 37.1 %, :54 kiln 25.5 %, against <= 25 % (baselines Logs/n56/flow-base.out:11 / :24 / :32: 26.2 / 32.0 / 29.2 %); clear times 122.6 / 220.1 / 92.2 s against 130.2 / 233.2 / 92.6 s (none worse); verbs37 PASS x5 (summary47.txt:280, 322, 364, 406, 448) | the stall line FAILs on all three levels. **Builder L's proposal** (baseline + 3 points: 29.2 / 35.0 / 32.2) gives foundry PASS, kiln PASS, **iceline FAIL** (37.1, 2.1 points over 35.0), so the proposal fails too. Worst rooms (bot): iceline g3 64.6 %, g5 59.0 % (g5 was 37.1 %, flow-base.out:19) |
| 2 Frames | 1 | PASS | the frames above | in g2 one tell became three enemies, and in a3 far enemies became close ones; kb2 changes only a little. Confound: the after frames also have colour56 on |
| 3 Nothing lost | 1 | PASS | verbs37 27/21/31/31/31 on r2 | no verb lost |
| 4 Look | 1 | PASS | the frames | enemies and tells are red-orange; the shields are yellow (punch the shield). Wall colours are judged under V1 |
| 5 Not basic | 1 | PASS | g2 / a3 frames | the first frame of g2 now shows the fight |

**GATE L2: FAIL (1)**. Smallest change that would pass: get iceline, foundry and kiln stall to <= 25 % (bot). Iceline is the furthest off at
37.1 %. Start with g3 (64.6 %) and g5 (59.0 %): move their wave-1 nodes onto the walk line. Re-run n46flow x10 on HEAD. The original
line is the gate (D-16).

## A4. G1 rank56

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL (not evidenced) | Logs/n56/bot-r2.out:1 "at af050c9b rows none", so flow.out's STYLE lines (:20, :45, :60, :75, :90: DULL 50.5 / 74.4 / 25.9 / 12.9 / 12.0 %) are the row OFF. There is no row-on run by the lead, a256 was not built, and StyleSim's golden records are not printed by name (the r2 gates print only "golden 0 mismatches"). For the record only (who ran it is not stated, so it is not counted): Logs/n56/g1-style-on.txt ("rows forced on: rank56") against g1-style-base.txt shows DULL foundry 56.0 -> 56.0, iceline 69.6 -> 71.1, kiln 31.9 -> 30.7. That is no 30-point fall. The "RIDE KILL refund" is 0.00 at every rank in both files | there is no evidence of the row on, and even the unattributed run would miss by about 30 points: rank56 scales the payoffs, not the style gain |
| 2 Frames | 0 | FAIL | a256 not built | the "a256 shows the rank leaving DULL" line has no frame |
| 3 Nothing lost | 1 | PASS | NIGHT56-LOG.md:69 r2 gates "golden 0 mismatches, 22/22; fingerprints plain / --off55 / --off56 EQUAL" | the row OFF is exact |
| 4 Look | - | n/a | a sim row | - |
| 5 Not basic | 0 | FAIL | ships OFF; the payoff is a heal multiplier; the a355 hit-next frame (rank56 on in the copy) still reads "D DULL" right after a kill chain | GG would not notice it in the first minute |

**GATE G1: FAIL (1, 2, 5)**. Smallest change that would pass: rank56 has to move the DULL share (a style-gain change), not only the
payoffs. Then the lead runs n46flow with rank56 forced on and prints the result beside the baseline, and a256 frames a combat frame
above DULL. Otherwise G1 closes SHORT with this measured reason.

## A5. G2 kill55's fix (= NIGHT55 P1-kill)

Frames, described before reading any numbers:
- `Logs/feel/n56-r2/k2on/a355/shot-N55-K-hit-next.jpg`: a husk flashing white in the centre with a cyan muzzle spark, three red husks behind it, a burst of large dark-red chunks above, and debris on the floor. The "ROOM CLEAR · UNDER PAR" card and "DOOR OPEN" show while four husks still stand (the reel's own staging). Rank "D DULL +3 KILL".
- Blind pair (both orders, the frame 1.5 s after a kill chain): one frame has a clean floor with scorch decals only. The other has about 40 red-orange debris chunks across the floor, plus green and violet wall bands.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | Logs/feel/n56-r2/summary47.txt:129 "RED a355 KILL ... ladder ok, cap ok, 3 in 1 s ok, debris ok, spaced ok, damage NO"; "changed next frame 97.3 %", but k2off (:134, kill55 off) prints "changed next frame 97.9 %". So the next-frame change is the hit flash, not kill55. The marks: "mark pixels (changed, V < 0.12) at +0.500 s 0.12 % (want >= 1.0 %)". The 3rd and later kills are "held 4 frames (0.067 s)" against a want of >= 0.08 s (the rules give 0.080 s, but the screen holds 0.067 s). "kills 1 -> 3 in 0.967 s" ok; debris "10 ... 16 alive" at +1.5 s ok; spaced "0.150 s held over 4 kills" ok; **a354 jitter 0.212** on m2on (:139) against <= 0.191 (m2off :144: 0.191); a354 on a chain prints "HOLD frames 11 ... 0.183 s" (:139) | a355 is RED (no marks); the held frames are under 0.08 s; jitter is worse than off |
| 2 Frames | 1 | PASS | the pair and the hit-next frame | the debris frame and the clean frame tell apart at once |
| 3 Nothing lost | 1 | PASS | verbs37 PASS x5; k2off "kill55 records 0", debris 0 | the OFF side is the old kill |
| 4 Look | 1 | PASS | the frames | the debris is in the enemy's red-orange family, the flash is white, and there is no full-screen effect |
| 5 Not basic | 1 | PASS | the pair | debris that carpets the floor is seen in the first fight |

Blind pair: **order 1: B; order 2: A** (each time the frame with the debris).

**GATE G2: FAIL (1). NIGHT55 P1-kill: FAIL (1)** (same lines). Smallest change that would pass: a355 GREEN, meaning mark pixels >= 1 % at
+0.5 s (darker, larger marks on the husk) and the 3rd and later holds showing >= 0.08 s on screen (5 frames). Also bring m2on's a354
jitter back to <= 0.191.

## A6. V1 colour56 (and NIGHT55 P0-decor, folded in by D-18)

Frames, described before reading any numbers:
- Colour pair-1 (foundry b1 entry "THE YARD"): one frame has thin lime strips on the walls and along the floor edges. The other has broad flat green bands at 3-5 m and violet strips on every wall.
- Colour pair-2 (kiln kb2 entry): one frame has thin green strips. The other has broad rose bands and blue strips on both walls.
- Seen in the other entries' frames: `c55s/a455/shot-N29-C55-n52-span-p1-hold.png` is lavender from edge to edge with lime bands; `c55v/a455/shot-N29-C55-n52-spire-v2-hold.png` is olive-green from edge to edge with crimson-pink bands.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n56-colour.py runs: cover PASS on every ON frame (foundry 6/6, iceline 12/12, kiln 6/6, span 6/6, spire 6/6); "palette accents pairwise: least gap 45 deg ... PASS"; sky "warm 0.00%" on every frame; **"nothing in the world above the tells' saturation": sat FAIL on 36 of 36 ON frames** (for example v1i g2 entry "world S p99.5 1.00 vs tells 0.71"); **the whole-frame tint: iceline "tint 0/12" (tint 68-95 % of the world, luma IQR 0.06-0.20), span "tint 0/6" (75-88 %)**; the OFF sides also pass cover on 5 of 6 frames each (foundry off key 5.2 %, kiln off key 4.5 % + accent 4.1 %), so the 3 % line does not tell ON from OFF | two doneWhen lines FAIL: saturation, and the whole-frame tint on iceline and span |
| 2 Frames | 1 | PASS | the pairs | the bands are seen without being told |
| 3 Nothing lost | 1 | PASS | verbs37 PASS x5 (colour56 on in r2); summary47.txt:113 / :124 "RED a356 ... colour56 OFF ... nothing built (NIGHT55's look)" | the OFF side is NIGHT55's |
| 4 Look | 0 | FAIL | iceline / span tint (above); the span p1 hold frame is lavender all over. The spire frames have 5.6-22.96 % yellow pixels (n56-colour V3: "yellow 22.96% (gun 9.07%)" on v5 mid), and no spire OFF side shows whether colour56 adds that yellow. Spire's accent #e66787 (hue 345) reads crimson-pink, between danger red and magenta, on the v2 hold frame | a whole-frame tint FAILs by rule; yellow is for parry only |
| 5 Not basic | 1 | PASS | the pairs | GG would notice it in the first minute. As he would judge it, though: one flat band at the same height on every wall of every room is wallpaper, not direction. On iceline and span the key is the walls' own hue (key cover 41-61 %), so those levels gain no new colour |

Blind pairs: **colour pair-1: order 1: B; order 2: A. Colour pair-2: order 1: B; order 2: A** (each time the frame with the broad bands).

**GATE V1: FAIL (1, 4)**. Smallest change that would pass:
- give iceline and span a key hue that differs from their walls' own (so the key is a band, not the frame), and re-run the tint check
- bring the world's p99.5 saturation under the tells' on every frame
- add a spire OFF side so the yellow can be attributed

**NIGHT55 P0-decor: FAIL (1)**:
- "accent share up >= 3x vs off" fails on kiln (at the accent hue: kb2 entry 5.6 % vs 4.1 %, x1.4; kb2c 5.7 % vs 3.6 %, x1.6; kb3 6.8 % vs 4.8 %, x1.4)
- foundry passes (1.8-2.9 % vs 0.0 %)
- iceline has no OFF side (not evidenced)
- "usable cyan stays the brightest class" is not printed by any tool run on these frames (not evidenced)
- the accents are >= 20 deg from the role hues (44 / 46 / 36 deg): PASS

## A7. V4 the boss palettes' second round (= NIGHT55 P0-wickGrade, P0-hotWall)

Frames, described before reading any numbers:
- `Logs/feel/n56-r2/b1on/a155/shot-N55-wick-burn.jpg`: a grey graded shaft with a red-orange burning wall panel in the centre (orange heat lines) and a yellow-hot column on the right. A red-orange band, about 80-120 px deep, runs along all four screen edges. HUD: "THE WICK LEG 1 / 3", and a grey "STAGGER".
- `Logs/feel/n56-r2/b2on/a155/shot-N55-crown37-p1.jpg`: THE CROWN's red-orange flame ring with a yellow cube, olive walls on the left, green walls on the right, and a green band with a pink stripe across the walls.
- `Logs/feel/n56-r2/b2off/a155/shot-N55-crown37-p1.jpg`: the same view and the same ring; the walls are slightly more yellow, and there is no band. If I had not been told, I would see no grade change. The only visible difference is colour56's stripe.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | summary47.txt:159 (b2on): THE CROWN "TELLS ... graded n 400 ... vs raw n 527 (kept >= 0.8 ...) ...: NO", which is 400 / 527 = **0.76**; "walls near danger ... p2 0.29 (<= 0.25: NO)"; "graded vs raw walls dH 11 (>= 25) dS -0.094 (|.| >= 0.10) ... differs False" (GRADE RED). **THE WYRM: no a155 room: not evidenced.** THE WICK (:149): "HOT WALL GREEN ... hot patch S 0.935 (n 294) vs the other walls S 0.312 ... (the patch is the most saturated)" PASS. **The ride panels' cyan cue (:149): "p1 14 / 0 %, burn 76 / 0 %, cold 61 / 0 %, turn 205 / 8 %, p2 207 / 8 %"**, identical on b1off (:154) | THE CROWN's tells kept is under 0.8; THE WYRM is not measured; the ride panels have no cyan cue (0 % at p1 / burn / cold, the same as OFF) |
| 2 Frames | 0 | FAIL | crown p1 ON against OFF | THE CROWN's grade cannot be seen; THE WICK's burn panel does read |
| 3 Nothing lost | 1 | PASS | :154 b1off "GRADE RED ... HOT WALL RED"; :164 b2off "graded live False"; verbs37 PASS | the OFF side is NIGHT54's look |
| 4 Look | 1 | PASS (flag) | the burn frame | the danger colour sits on the burning wall. **Flag:** the burn frame has a red-orange band along all four screen edges (a screen-edge wash). Its cause and the OFF burn frame are not in the evidence. It is the CORE "never again" kind (screen borders) and P1-edges' target |
| 5 Not basic | 0 | FAIL | crown frames | THE CROWN looks unchanged; only THE WICK's burn reads |

**GATE V4: FAIL (1, 2, 5)**.
**NIGHT55 P0-wickGrade: FAIL (1)**. THE WICK passes: walls 159 / 129 / 105 deg from danger, tells 608 graded vs 152 raw, cyan 5.4:1, dH 140, and b1off RED. THE CROWN's grade is RED, as above.
**NIGHT55 P0-hotWall: FAIL (1)**:
- what passes: burn S 0.935 vs walls 0.312, the most saturated region; no-burn red-orange p1 0.003 / cold 0.009 vs OFF 0.041 / 0.047 (under half); the seams at hue 228, 144 deg from danger; b1off RED
- what fails: its notes' cyan ride strip (critic-firsttime #2) is missing, with 0 % cyan on the ride panels
- also: the burn was placed with "wick55's ride-only rule suspended for the placement", so a burn from the real fight is not shown

Smallest change that would pass:
- keep the grade off THE CROWN ring's mask (tells kept >= 0.8)
- move the p2 walls out of the near-danger band (<= 0.25) with dH >= 25 or |dS| >= 0.10
- give THE WICK's ride panels a cyan strip (cyan share > 0 on every phase frame)
- an a155 room for THE WYRM

---

## B1. P0-smoke

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **met:** Logs/n56/smoke-steal-n56-r1.out:34 / :37 "SetForegroundWindow True, took True"; Logs/player/n56-smoke-steal-n56-r1.log:88 "53.8s n55 focus LOST: flow Playing", :157 "191.5s n55 focus LOST"; :191 "input summary: focus changes 4, unfocused 2078 of 68669 frames"; :145 "PASS tape: 486 steps ... EQUAL"; :192 "DONE: all steps passed". **Not met:** the notes' "outside -skygrindSmoke the game still pauses on focus loss (a test)". Smoke55Tests.cs has 5 tests (NoTransitionIsNoCause, TheSmokesPadStartIsItsOwn, EveryOtherPauseIsNamedAndResolved, TheVerdictNamesTheMostDirectCause, TheSmokesPadsAreKnownByName), and they test only the smoke guard's classifier. None runs the game without -skygrindSmoke and asserts a pause, and grep finds no other test that does. **"with no agent active"** was not met either: three builder agents were running (NIGHT56-LOG.md:65) | the smoke itself is clean under real steals; the outside-smoke pause has no test |
| 2 Frames | - | n/a | harness only | - |
| 3 Nothing lost | 1 | PASS | :66 "backgroundBehavior ... -> IgnoreFocus for the smoke" | the smoke-only switch is named |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | harness | - |

**GATE P0-smoke: FAIL (1)**. Smallest change that would pass: one test, either EditMode on SurvivalGame's focus path or a Player run
without -skygrindSmoke, that drops focus while Playing and asserts flow -> Paused. Then one steal smoke with no agent running.

## B2. P2-clear (clear55)

Frames, described before reading any numbers (one hold frame per level):
- `c55f/a455/shot-N29-C55-n52-foundry-a3-hold.png`: a corridor with "DOOR OPEN" at the top and "ROOM CLEAR · UNDER PAR / 14.0 S · PAR 22 S" under the reticle (the time line in cyan).
- `c55k/a455/shot-N29-C55-n52-kiln-kb2c-hold.png`: rose bands, the teach chip "Midair, aim down, then ZR", and the card "2.5 S · PAR 35 S".
- `c55s/a455/shot-N29-C55-n52-span-p1-hold.png`: a lavender room with the card "16.1 S · PAR 20 S".
- `c55v/a455/shot-N29-C55-n52-spire-v2-hold.png`: an olive-green room with the card "6.8 S · PAR 45 S".

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | summary47.txt:173 / :189 / :205 / :221 "hold served 0.500 s on the loop's clock (band 0.4-0.6)" in 8 of 8 rooms, "sim time moved during the hold False", "par source: n52-<level>.json campaign.rooms[<id>].par", "min font 24 px"; iceline from r1 (Logs/feel/n56-r1/summary47.txt c55on: g2 "hold served 0.500 s") | hold, card, par source and a level each are all met |
| 2 Frames | 0 | FAIL (not evidenced) | only ON frames are listed; the OFF log prints the same card | the change (a 0.5 s hold) cannot be seen in a still, and the card appears on both sides |
| 3 Nothing lost | 0 | FAIL | the OFF sides print "row clear55 OFF (NIGHT54: no clear beat) ... holds 0, cards 2 ... card 'ROOM CLEAR · UNDER PAR \| 14.0 S · PAR 22 S' ... alpha 1.00" (:180; also :196, :212, :228, and r1 c55off). UiClear55.cs:33 builds the card only when `Clear55.On(cfg)`, and Clear55Alpha stays 0 unless the card's root is active (:74, :106). So an alpha of 1.00 means the card is drawn with -skygrindOff55 | the OFF side is not NIGHT54's look (the card leaks through the off switch) |
| 4 Look | 1 | PASS | the frames | the text is >= 16 px (24 px min), the cyan is information, and the card has no full-screen effect. The span / spire tints are V1's |
| 5 Not basic | 1 | PASS | the frames | a card at every room clear |

**GATE P2-clear: FAIL (2, 3)**. Smallest change that would pass: make the off switch the hold obeys also stop BuildClear55 /
RenderClear55. Re-run c55*off so they print "cards 0", and give the critic one OFF frame beside each hold frame.

## B3. P0-t125

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 1 | PASS | Logs/player/n47-reel-n56-r2-t1.log:79 (and summary47.txt:233) "6 taps of 6 sim steps ... fire events 6 (THUNDERCLAP 6; want >= 4: met)"; the fix is named: "T-125 input: the sim read AutopilotInput (N24StartCampaign's campaign bot ...) -> ReaderInput (the reel's forcedCombat reader, handed back as a252 does: FeelReel.N52.Skin.cs:132)"; a252 at summary47.txt:238 "GREEN a252 skin52 \| built True, checks 8/8", the same verdict as Logs/feel/n52-freeze/summary47.txt:311 "checks 8/8" | >= 4 fire events, the fix named, and the KESTREL's a252 unchanged. a152's own RED is hud52's lesson chip, which the brief excludes |
| 2 Frames | - | n/a | reel code only | - |
| 3 Nothing lost | 1 | PASS | verbs37 PASS x5 | - |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | reel code only | - |

**GATE P0-t125: PASS**

## B4. P0-t99

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | Logs/player/n47-reel-n56-r2-t99.log:149 "[reel] done: 2807 frames written, 0 failed, 56 s wall": 0 failed PASS. **"no index reused (printed by the reel's done line)": the done line prints no such count.** My count: Logs/feel/n56-r2/t99/verbs37/ has 1800 files f0000-f1799 and .../a452/ has 1007 files f0000-f1006. 1800 + 1007 = 2807, with no gaps, so nothing was overwritten inside a folder. But each phase numbers from f0000, so no recorder sequence went past index 1799. Every verbs37 / a152 / a252 reel tonight writes exactly "1800 frames written" (summary47 vf..vp, t1, t2) at 36-75 s of wall time. That is consistent with the 1800 cap still being there (not proven either way) | the doneWhen's own print is missing, and "past 1800" is only the sum of two folders |
| 2 Frames | - | n/a | reel code | - |
| 3 Nothing lost | 1 | PASS | t99 log:136 "verbs37: PASS: total 27/27" | - |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | - | - |

**GATE P0-t99: FAIL (1)**. Smallest change that would pass: one phase that records more than 1800 frames into one folder (indices past
f1799), and a done line that prints "indices reused 0, highest index N".

## B5. P1-teachText (brief55)

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n55k-teach.cjs run: kiln "lines 6, bad 0; a card over the counter in 94 frames (worst 42.1 px) BAD"; spire "lines 2, bad 0; a card over the counter in 94 frames (worst 42.1 px) BAD"; "RED teachText". Every other line is ok (16 lines, each <= 6 words with a glyph, <= 337 frames) | "0 px overlap of any toast with the counter plate" is not met. The overlap fix is not in r2 |
| 2 Frames | 0 | FAIL (not evidenced) | no frames listed | - |
| 3 Nothing lost | 1 | PASS | verbs37 PASS x5 | - |
| 4 Look | 0 | FAIL (not evidenced) | no frames: the text px cannot be checked | - |
| 5 Not basic | 1 | PASS | telemetry: a new line at f0 on every level | the text is seen at once |

**GATE P1-teachText: FAIL (1, 2, 4)**. Smallest change that would pass: move kiln's and spire's card off the counter plate (0 px), then
re-run n55k-teach on verbs37 x5 with the frames listed.

## B6. P1-doorDark

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n55-r-flat.py run on the kept frames: foundry 1 flat (f0500 std 0.0197), iceline 3 (f0850 0.0141, f0900 0.0179, f1350 0.0220), kiln 0, span 0, spire 2 (f1550 0.0203, f1600 0.0249): 6 of 180 frames, where the want is 0 (r1: 7, NIGHT56-LOG.md:57-58) | 6 flat frames remain; the door / link frame per level is not given |
| 2 Frames | 0 | FAIL (not evidenced) | no frames (the doneWhen asks for a frame per level) | - |
| 3 Nothing lost | 1 | PASS | verbs37 PASS x5 | - |
| 4 Look | 0 | FAIL (not evidenced) | no frames | - |
| 5 Not basic | 0 | FAIL (not evidenced) | no frames | - |

**GATE P1-doorDark: FAIL (1, 2, 4, 5)**. Smallest change that would pass: light the links seen at foundry f0500, iceline f0850-f0900 /
f1350 and spire f1550-f1600. Then run n55-r-flat on every frame (0 flat) and frame one door per level.

## B7. P0-g7g5

Frames, described before reading any numbers:
- `Logs/feel/n56-r2/l2i/a156/shot-N29-L56-n52-iceline-g7-entry.png`: a blue hall under a "THE MIX" banner. A central blue column banded green, a ramp up to a walkway on the left, lava strips on the floor on the left, two red spawn tells about 18 m ahead, and a cyan light column on the right. Teach line: "Swap guns with D-pad by range".
- `.../shot-N29-L56-n52-iceline-g5-entry.png`: a blue aisle between two rows of identical columns, with a block ahead and a cyan light shaft on the right. Red spawn glows at both lower corners, with spiky enemies half under the HUD panels. Teach line: "Save ZL for the second pack".

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | Logs/n56/bot-r2/identity.txt:13 g7 I1 "PASS 52 %" (was 99 %, Logs/n53/identity-e53-final.md:13), I2 "1.22/1.75" (was 1.22/1.26), I4 4.0 / 4.3 m (was 3.5 / 2.7). identity.txt:12 g5 I1 50 % (was 76 %), I2 1.12/4.66/1.25/2.85 (was 1.07/1.11/1.25/1.26). verbs37 iceline 21/21 (summary47.txt:322). (bot) walk iceline "GREEN 10/10", every room 10/10 (flow.out:24-34). **The contract 14/14 for each room is not in the evidence** (only in R.md:19-20, a builder's report): not evidenced. The pieces are named only in R.md:10-11 ("the west the walkers' lock, the east the riders' firebox"; "the west the pour, the east the crane") | I1 < 60 % with nothing worse, 21/21 and 10/10 all PASS; the contract is not evidenced |
| 2 Frames | 0 | FAIL | the two entry frames; no before frames | a fresh review cannot name the pieces from these frames: there is no firebox, lock, crane runway or pour chute in view (g7 shows a column and a ramp, g5 an aisle of columns) |
| 3 Nothing lost | 1 | PASS | verbs37 iceline 21/21 | note (bot): g7 falls 92 -> 156 and g5 stall 37.1 -> 59.0 % against flow-base (L2's waves in the same rooms) |
| 4 Look | 1 | PASS | the frames | lava and tells are red-orange; the light columns are cyan |
| 5 Not basic | 0 | FAIL | the frames | as GG would judge it: g5's entry is a corridor of identical pillars and g7's is a column and a ramp. These are "random pillars that serve no purpose" until the named pieces read from the door |

**GATE P0-g7g5: FAIL (1, 2, 5)**. Smallest change that would pass: the lead prints the contract 14/14 for g7 and g5, and gives one
frame per room with the named piece in view (the crane runway, the firebox), lit and placed so its job reads from the door.

---

## Verdicts

- GATE P0-pick: PASS
- GATE L1: FAIL (1)
- GATE L2: FAIL (1)
- GATE G1: FAIL (1, 2, 5)
- GATE G2: FAIL (1); NIGHT55 P1-kill: FAIL (1)
- GATE V1: FAIL (1, 4); NIGHT55 P0-decor: FAIL (1)
- GATE V4: FAIL (1, 2, 5); NIGHT55 P0-wickGrade: FAIL (1); NIGHT55 P0-hotWall: FAIL (1)
- GATE P0-smoke: FAIL (1)
- GATE P2-clear: FAIL (2, 3)
- GATE P0-t125: PASS
- GATE P0-t99: FAIL (1)
- GATE P1-teachText: FAIL (1, 2, 4)
- GATE P1-doorDark: FAIL (1, 2, 4, 5)
- GATE P0-g7g5: FAIL (1, 2, 5)

Blind pairs (answered blind, each order on its own): kill pair order1 B order2 A; colour pair-1 order1 B order2 A; colour pair-2 order1 B
order2 A.
