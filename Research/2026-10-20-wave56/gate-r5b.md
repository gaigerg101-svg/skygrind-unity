# NIGHT56 gate r5, part B (GAMEPLAY, ENEMIES, COLOUR, BOSSES): the fresh batched critic's verdicts (build Builds/n56-r5, b6ab1f10)

Rubric: Research/2026-10-20-wave56/critic-gate.md. Evidence: Research/2026-10-20-wave56/gate-r5b-brief.md, the slates (Logs/n56/slate.json,
Logs/n55/slate.json), DECIDE56.md D-18 / D-24 / D-25, NIGHT56-LOG.md. I read no builder report (G.md, V.md, L.md). I read the code of two
tests named by the brief or the log (N56FocusPauseTests.cs, Wyrm55Tests.cs) only to see what they assert.
Images opened: 15 of 30, all from the brief's list. Read-only commands I ran:
- `python -I Tools/night/n56-colour.py` on Logs/feel/n56-r5/{v5f,v5i,v5k,v5s,v5v,v5foff,v5voff}/a356 `--kinds entry,mid`, on r5 verbs37 x5
  (vf, vi, vk, vs, vp), and on r2's v1f / v1i / v1k / v1s / v1v (the rim56-off baseline)
- `node Tools/night/n55k-teach.cjs` on r5 verbs37 x5
- grep / sed / ls / cat on the logs, plus one `git log -1` to place 9d133979 after a59e54ac

Bot numbers are marked (bot). "Line N" means rubric line N.

---

## B1. G1 rank56 (sim row; ships OFF)

Frame, described before its numbers:
- `Logs/feel/n56-r5/g1on/a256/shot-N56-G-rank.jpg`: a close kill. One husk flashes white with a cyan muzzle burst. A large cyan "+10"
  floats beside it, chunks fly, and three red-orange husks stand on the left. The rank reads "C COOL +28 MULTIKILL" (the "C" glyph sits on
  top of the word COOL). Also on screen: "2 LEFT" and health 150 with a red segment.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **(bot)** Logs/n56/bot-r4-rank/flow.out:20, 48, 66, 84, 102 (rank56 forced on, a59e54ac) vs Logs/n56/bot-r4/flow.out:20, 48, 66, 84, 102 (off, 9d133979, a checkpoint commit after a59e54ac). DULL: foundry 50.6 vs 50.5 %, iceline 73.7 vs 74.7, kiln 26.0 vs 25.9, span 13.2 vs 13.2, spire 12.0 vs 12.0. PAYOFFS (:21 etc.): "RIDE KILL refund per ride kill by rank: DULL 0.00 ... SKYGRIND 0.00" on every level, both sides. A room reaches SAVAGE (kiln 3/3), also with the row off. Golden: NIGHT56-LOG.md:128 "golden 0 mismatches, 22/22". StyleSim's records are not printed by name, and the "39 sets" total line (:22) predates rank56 | the DULL share has to fall by >= 30 points. It moves -1.0 to +0.1 points. The refund half of the row pays 0.00 everywhere |
| 2 Frames | 0 | FAIL | the rank frame above; Logs/feel/n56-r4/summary47.txt:8 g1off "RED a256 ... rank56 on False ... the rank left DULL on frame 35 with a husk alive ... top rank COOL ... heal at COOL 10.00 HP: x1.00" | the rank also leaves DULL on frame 35 with the row OFF. The only difference is heal 10 -> 12 HP at COOL, and that cannot be seen in the frame |
| 3 Nothing lost | 0 | FAIL | Logs/n56/suite-r5.txt:5 "N56RankTests: tests: 2 / 3 passed, 1 failed ... FAIL ... RowOff_TheHealAndTheRefund_AreTheSameAtEveryRank: heal36's amount" (NIGHT56-LOG.md:133-134: "Expected: 10.0d But was: 0.0d ... cause not checked"); against that, fingerprints --off56 EQUAL (:128) and r4 g1off prints the old x1.00 heal | the row-off test is RED in the Editor on HEAD, and its cause was not checked |
| 4 Look | - | n/a | a sim row | - |
| 5 Not basic | 0 | FAIL | the frame; g1on vs g1off | +2 HP per close kill at COOL, with the row shipped OFF. GG would not notice it in the first minute |

**GATE G1: FAIL (1, 2, 3, 5)**. Measured reason: (bot) rank56 on leaves the DULL share unmoved (foundry 50.6 vs 50.5 %, iceline 73.7 vs
74.7 %; >= 30-point fall wanted) and the RIDE KILL refund is 0.00 at every rank; the Editor's row-off test is RED (2 / 3).
Change from r2: the row-on run and a256 now exist, and they confirm the miss. The row-off test failure is new.
Smallest change that would pass: a style-gain change, which is outside this slate entry, so the entry closes SHORT.

## B2. G2 kill55's fix (= NIGHT55 P1-kill), attempt 2

Frames, described before their numbers:
- `Logs/feel/n56-r5/k5on/a355/shot-N55-K-hit-next.jpg`: a foundry corridor with sage and violet wall bands. The hit husk is a white flash
  with a cyan muzzle star, three red-orange husks stand behind it, a burst of orange-brown block chunks hangs above, and orange debris lies
  across the floor. The reel's staging card "ROOM CLEAR · UNDER PAR 10.2 S · PAR 18 S" and "DOOR OPEN" show while husks still stand. The
  rank reads "D DULL +3 KILL".
- `.../k5on/a355/shot-N55-K-hit-after.jpg` (+0.5 s): the same view. The hit husk is back to plain red-orange with its yellow band. I see no
  crack or hole on its body. There are dark-red splats and debris on the floor, and the rank reads "D DULL".

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | Logs/feel/n56-r5/summary47.txt:167 k5on "**RED a355 KILL** ... kill 3: held 5 frames (0.083 s) ... kill 6: held 5 frames (0.083 s) ... rules kill 0.050 + kill55 0.040 = 0.090 s ... kills 1 -> 3 in 0.967 s ... debris at +1.500 s: kill 1: 10 alive ... kill 6: 16 alive ... changed next frame 97.3 %, at +0.500 s 13.9 % ... **mark pixels (changed, V < 0.12) at +0.500 s 0.04 % (want >= 1.0 %)** ... marks laid 3, holes 3, drawn 1 \| ladder ok, cap ok, 3 in 1 s ok, debris ok, spaced ok, **damage NO**"; k5off :172 "changed next frame 97.8 %" (so the next-frame change is the hit flash, which the OFF side has too); **a354 m5on :177 "jitter 0.230"** against <= 0.191 (r2 m2off, Logs/feel/n56-r2/summary47.txt:144 "jitter 0.191"); m5on "HOLD frames 13 (out 13: 0.217 s ...)" | a355 is RED on its own line: the marks fell from 0.12 % (r2) to 0.04 %, against 1 %. The jitter got worse (0.212 at r2, 0.230 now). The holds are fixed: the 3rd and later kills now hold 5 frames (0.083 s >= 0.08) |
| 2 Frames | 1 | PASS | the two frames; the r2 blind pair (ON picked 2 of 2, both orders) stands per the brief | the debris and chunks read at once. The "husk that shows damage" does not: hit-after shows an intact pill |
| 3 Nothing lost | 1 | PASS | :172 k5off "kill55 on False ... kill55 records 0 ... debris ... 0 alive ... ladder NO"; verbs37 x5 PASS (:224, 266, 308, 350, 392: 27/27, 21/21, 31/31, 31/31, 31/31) | the OFF side is the old flat kill |
| 4 Look | 1 | PASS | the frames; :167 the kills are 0.48 s of play apart | the debris is in the enemy family and the flash is white. There is no full-screen effect, and at about 2 kill flashes a second it stays under 3 a second |
| 5 Not basic | 1 | PASS | hit-next | a carpet of debris and flying chunks shows in the first fight |

**GATE G2: FAIL (1). NIGHT55 P1-kill: FAIL (1)** (same lines; P1-kill's "a354 jitter <= 0.191" also fails). Measured reason: a355 RED,
with hit-mark pixels 0.04 % at +0.5 s against >= 1 %; a354 jitter 0.230 against <= 0.191.
Change from r2: the hold line is now met, while the marks and the jitter got worse.

## B3. G3 tokens56 (sim row)

No frames (a sim rule).

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **(bot)** Logs/n56/bot-r4-tokens/flow.out (on, a59e54ac) vs Logs/n56/bot-r4/flow.out (off). Details below the table | three clauses fail: the cone on span, the ranged clause on kiln and spire, and idle on every level |
| 2 Frames | - | n/a | the brief: a sim rule, no frames | - |
| 3 Nothing lost | 1 | PASS | NIGHT56-LOG.md:128 fingerprints plain / --off55 / --off56 EQUAL; suite-r5.txt:7 "N56TokensTests: 2 / 2"; verbs37 x5 PASS on r5 with tokens56 on | (bot) clear iceline 250.6 s vs base 233.2 (+7.5 %), spire 149.0 vs 141.0 (+5.7 %): slower, no slate line |
| 4 Look | - | n/a | sim | - |
| 5 Not basic | 1 | PASS (bot) | the cone share off 35.8-62.7 % -> on 78.6-90.0 % | most attacks would now start on screen, which a player would notice (bot only) |

Line 1, clause by clause (all bot):
- **Cone:** TOKENS :22, 50, 68, 86, 104 on: foundry 85.4 %, iceline 82.4 %, kiln 90.0 %, **span 78.6 %**, spire 86.7 %; pooled 2039 / 2418 =
  84.3 % (my sum of the five lines); off: 62.7 / 45.0 / 40.9 / 39.7 / 35.8 %. Pooled it passes; per level, span fails.
- **Ranged:** RANGED :23, 51, 69, 87, 105 have "dash" samples 0/0, 0/0, 0/2, 0/2, 0/1, which is 5 dash shots on all five levels, and
  "dash / stand NaN %" on foundry and iceline. So the dash clause is not evidenced. The fast+dash proxy is 46 / 0 / **293** / 41 / **90 %**.
- **Idle:** IDLE :24, 52, 70, 88, 106: "per enemy ... p90 19.6 / 30.1 / 33.0 / 19.9 / 16.0 %, over 20 %: 34 (9.3 %) / 132 (21.4 %) / 62
  (19.9 %) / 38 (9.8 %) / 23 (6.9 %)". Drones idle 31.4-34.5 %. Off, over 20 % was 10.8 / 23.1 / 14.7 / 7.1 / 8.3 %: kiln and span got worse.
- **Deaths:** 13 / 5 / 8 / 0 / 0 against base 15 / 2 / 8 / 1 / 0 (Logs/n56/flow-base.out:11, 24, 32, 40, 48). The flow table's band is
  "baseline seeds 1-10 vs 11-20, the largest gap" (Tools/night/n46-flow-table.cjs:32). Its 11-20 off run, Logs/n56/g3-band-off.txt (who
  ran it is not stated; the brief does not name it), gives deaths 20 / 3 / 12 / 1 / 0, a gap of 5, and every level sits inside it.
- **Golden / fingerprints:** EQUAL (log :128).

**GATE G3: FAIL (1)**. Measured reason (bot): per-enemy idle over 20 % on every level (iceline 132 enemies, 21.4 %; p90 30-33 % on iceline /
kiln); the dash hit rate is not measurable (5 dash shots in all), and fast+dash / stand is 293 % on kiln and 90 % on spire (<= 50 % wanted);
span's cone share is 78.6 % (< 80).

## B4. G4 THE KILN WYRM's tail (design.n55.wyrm55, sim)

No frames.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **(bot)** Logs/n56/wyrm-r5-on.txt:42-44 "rows [wyrm55] ... killed 40/40 (caps 0); kill time median 86.8 s, p90 178.3 s ... deaths median 1 (**total 32**)" vs Logs/n56/wyrm-base.txt:5 "38/40 \| 122.6 \| 256.9 ... \| 68". **Pinning:** suite-r5.txt:9 "Wyrm55Tests: 2 / 2" (NIGHT56-LOG.md:132). Wyrm55Tests.cs:37-56 pins the contact (body / head / lunge), spit (count / speed / damage) and shard damage, plus the lunge's tell. It pins nothing for the dive, the squeeze or the sweep | 40 / 40, median 86.8 s (60-120) and p90 178.3 s (<= 180) pass. **Deaths 32 > 10 fails**, and "every attack pinned" is only partly met |
| 2 Frames | - | n/a | brief: no frames | - |
| 3 Nothing lost | 1 | PASS | fingerprints EQUAL off (:128); wyrm-base.txt is the row-off fight | - |
| 4 Look | - | n/a | sim | - |
| 5 Not basic | 1 | PASS (bot) | median 122.6 -> 86.8 s, deaths 68 -> 32 | a shorter, less punishing fight is noticed (bot only) |

**GATE G4: FAIL (1)**. Measured reason (bot): 32 deaths in 40 fights against <= 10, and the dive, squeeze and sweep are not pinned by a test.

## B5. V1 colour56 (attempt 3; NIGHT55 P0-decor closes with it, D-18)

Frames, described before their numbers:
- `Logs/feel/n56-r5/v5i/a356/g2/shot-N56-n52-iceline-g2-entry.png`: a navy-blue hall with dark pillars. Broad violet bands run high on the
  walls, with mint-green strips on the pillars and walls. Two yellow-shielded husks stand in the middle distance and a husk group sits on a
  ledge with a red glow. The dark-to-light structure of the walls is kept, and no single colour covers the frame. Teach line: "Punch [ZR]
  the shield or flank".
- `Logs/feel/n56-r5/v5s/a356/p1/shot-N56-n52-span-p1-entry.png`: lavender walls and floor, with broad mid-blue bands and lime strips.
  Cyan waist dashes and cyan grapple rings show, and two red-orange husks stand on the left. The lavender is the level's base. The blue key
  now reads as a band, not as the whole room.
- `Logs/feel/n56-r5/v5f/a356/b1/shot-N56-n52-foundry-b1-entry.png` (ON): tan walls on the left and dark-green pillars on the right. Broad
  sage bands run at 3-5 m, and saturated violet strips run along every wall. There are two cyan light columns and three husks.
- `Logs/feel/n56-r5/v5foff/a356/b1/shot-N56-n52-foundry-b1-entry.png` (OFF): the same room with thin mint strips only. It has no violet and
  no broad bands.
- The spire frame opened under B7 (v5 mid) is olive / lime green from edge to edge, with pink-crimson strips and a lime frieze.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n56-colour.py runs. Details below the table | "nothing in the world above the tells' saturation" fails on 28 of 36 ON frames, and on spire colour56's own frieze is one of the things above them |
| 2 Frames | 1 | PASS | foundry b1 ON vs OFF | violet strips and broad bands; anyone sees the change without being told |
| 3 Nothing lost | 1 | PASS | summary47.txt:136 / :147 "RED a356 ... colour56 OFF ... nothing built (NIGHT55's look)"; verbs37 x5 PASS | - |
| 4 Look | 1 | PASS (flags) | the frames | no whole-frame tint, and the walls keep their value structure. Flags: spire's accent measures hue 350-352 (pink-crimson, "danger 27, magenta 33 deg", :125), between the danger and RIVAL hues. The spire's heavy yellow is the level's own: OFF 5.9-26.2 % vs ON 3.4-13.6 % per frame (v5voff vs v5v) |
| 5 Not basic | 1 | PASS | foundry ON / OFF; r2 blind pairs (foundry, kiln) ON 2 of 2, both orders | GG would see colour in the first minute. As he would judge it, though: one flat band at the same height on every wall still reads as wallpaper |

Line 1, clause by clause:
- **Saturation (fails).** The TOTAL lines show "sat (whole world)" passing on only 1/6 (foundry), 5/12 (iceline), 0/6 (kiln), 2/6 (span)
  and 0/6 (spire) frames. Examples: v5i g2 entry "world S p99.5 0.89 ... vs tells 0.62 FAIL, world over the tells 3.00% of the frame"; v5k
  kb2c entry "world over the tells 5.90%"; v5v v2 mid "3.34%". On spire, colour56's own frieze is above the tells on 6/6 frames (v5 mid
  "FRIEZE ... S median 0.92 p99.5 1.00 (vs tells 0.98: FAIL)"). So r5's spire desaturation (b6ab1f10) did not reach the frame. Kiln's
  frieze fails on 4/6.
- **Cover.** Passes on 35/36 frames; it fails on foundry b1 mid ("key|accent 2.6% FAIL", a frame filled by a close husk).
- **Accents pairwise:** "least gap 45 deg (iceline / span) -> PASS".
- **Sky:** "sky warm 0.00% PASS" on 36/36 frames.
- **Tint:** "tint 6/6, 12/12, 6/6, 6/6, 6/6": PASS on every frame (worst spire v6 entry 55 % of the world, threshold 60 %; luma IQR >= 0.075).
- **Blind pairs:** r2's foundry and kiln pairs stand (brief).

**GATE V1: FAIL (1). NIGHT55 P0-decor: FAIL (1)**, closing with V1's verdict (D-18; also "usable cyan stays the brightest class" is printed
by no tool, and iceline / kiln have no OFF side on r5).
Measured reason: the world's saturation is above the tells' on 28 of 36 ON frames (up to 5.9 % of the frame on kiln kb2c). On spire,
colour56's own frieze renders at S median 0.89-0.93, p99.5 1.00, against tells at 0.98-0.99.
Change from r2: line 4 now passes. The iceline / span whole-frame tint is gone, and a spire OFF side shows the yellow is the level's.

## B6. V2 rim56

Frames, described before their numbers:
- `Logs/feel/n56-r5/v5k/a356/kb2/shot-N56-n52-kiln-kb2-mid.png`: a tan / brown kiln hall with broad rose bands, blue strips and cyan edge
  trims. Four orange husks with yellow bands: one big at the right, three on ledges. The husks are orange on orange-tan walls, and the cyan
  trims are the brightest things in the frame.
- `Logs/feel/n56-r5/v5s/a356/p5/shot-N56-n52-span-p5-mid.png`: a dim lavender hall with blue and lime bands and cyan strips. The enemies are
  small dull-red silhouettes in the distance (four, plus a faint red pill on a ramp). They read as red marks, not as the brightest or
  warmest things. Teach line: "Yank an enemy with [R]".

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n56-colour.py, r5 a356 (rim56 ON). Details below the table | under the slate's line, 3 % of enemies on the mid frames pass, against 90 % wanted. Even the meter's own looser contract reaches only 47 % |
| 2 Frames | 0 | FAIL | the two ON frames; there is no OFF frame for kiln / span; foundry b1 ON vs OFF (B5) shows the same-looking husks (meter body V 0.62 / 0.62 / 0.57 on vs 0.55 / 0.46 / 0.52 off) | no visible change; the enemies are not the brightest things in either frame |
| 3 Nothing lost | 1 | PASS | :136 / :147 "rim56 OFF (... the enemy material's far rim lifted: no)" | - |
| 4 Look | 1 | PASS | the frames | red-orange stays the enemies' colour |
| 5 Not basic | 0 | FAIL | the frames | GG would not notice the enemies getting brighter |

Line 1 in detail:
- **The slate's line** (rect mean V and S over the world's p90), mid frames only: foundry 0/5 enemies, iceline 0/8, kiln 1/7, span 0/5,
  spire 0/5, which is 1 of 30 (3 %).
- **Over entry + mid frames:** "original ... rect over world p90" 0 % / 6 % / 40 % / 31 % / 0 %.
- **The meter's looser contract** (warm body pixels over the environment's p90): 21 / 76 / 67 / 94 / 0 %, and 14 / 30 on the mid frames.
- **r2 with rim56 off** (my run on r2's v1*): original 0 / 0 / 40 / 31 / 0 %, looser 0 / 35 / 60 / 94 / 0 %. So rim56 moved the slate's
  line only on iceline (+6 points).

**GATE V2: FAIL (1, 2, 5)**. Measured reason: on the mid frames, 1 of 30 enemies in view has rect mean V and S above the world's p90 (>= 90 %
wanted); against r2's rim-off frames, only iceline moved (0 -> 6 %).

## B7. V3 yellow56

Frames, described before their numbers:
- `Logs/feel/n56-r5/v5f/a356/a5/shot-N56-n52-foundry-a5-entry.png`: a foundry corridor of tan and green walls with violet strips. A catwalk
  runs between two lava channels whose edges glow orange-yellow. On the left stands a pill with a bright yellow outline. Three husks stand
  ahead, with a cyan health cross and a red block above.
- `Logs/feel/n56-r5/v5v/a356/v5/shot-N56-n52-spire-v5-mid.png`: olive / lime-green walls and floor, pink-crimson strips and cyan strips.
  Three red husks stand on a ramp, and two huge saturated yellow spheres hang at the right.
- Seen in B8's pair: THE CROWN's TORCH has a yellow top plate on r2 OFF and a grey one on r5 ON.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n56-colour.py on r5 verbs37. Details below the table | yellow outside the parry tells goes over 0.5 % on foundry (12 / 36 frames, up to 1.70 %) and on spire (28 / 36 frames, up to 23.94 %) |
| 2 Frames | 1 | PASS | the CROWN pair (B8) | the gun plate's yellow is gone. That is visible, though small |
| 3 Nothing lost | 1 | PASS | :136 "yellow56 OFF (gun plates moved off the yellow arc 0 ...)"; verbs37 x5 PASS | - |
| 4 Look | 0 | FAIL | the spire frame; spire verbs37 per frame, for example f0500 "V3 yellow 23.94% (gun 6.06%, bright V >= 0.5 2.52%) ... parry orbs by shape 0" | dim mustard-yellow world pixels and the gun cover 6-24 % of most spire frames. Yellow does not yet mean parry |
| 5 Not basic | 0 | FAIL | the frames | a player on spire still sees yellow everywhere, and the foundry's lava edges glow yellow |

Line 1 in detail:
- **verbs37 x5, yellow per frame:** vf "median 0.43%, max 1.70%, frames over 0.5%: 12 / 36"; vi "max 13.67%, 3 / 36"; vk "max 0.07%,
  0 / 36"; vs "max 0.05%, 0 / 36"; vp "median 8.54%, max 23.94%, frames over 0.5%: 28 / 36".
- **a356:** foundry 1/6 frames pass (worst 2.45 %, a5 entry); spire 0/6 pass (worst 13.57 %, v5 mid).
- **"Boss / enemy yellow parts listed in DECIDE56":** grep finds no such list, so this clause is not evidenced.

**GATE V3: FAIL (1, 4, 5)**. Measured reason: yellow outside parry tells and enemy rects is over 0.5 % on 28 of 36 spire verbs37 frames
(median 8.54 %, max 23.94 %) and 12 of 36 foundry frames (max 1.70 %).

## B8. V4 the boss palettes' second round (= NIGHT55 P0-wickGrade, P0-hotWall, P1-wyrmTurn)

Frames, described before their numbers:
- `Logs/feel/n56-r5/b2on/a155/shot-N55-crown37-p1.jpg` (ON): THE CROWN's red-orange flame ring with a yellow cube inside. The walls are dark
  olive-grey on the left and dark green on the right, crossed by a green band with a pink stripe. The TORCH is grey. HUD: "LEG 1 / 3".
- `Logs/feel/n56-r2/b2off/a155/shot-N55-crown37-p1.jpg` (OFF): the same view and the same ring. The left wall is mustard-yellow, the
  TORCH has a yellow plate, and there is no band. Against ON, the walls are visibly more yellow. The ring looks the same.
- `Logs/feel/n56-r5/b1on/a155/shot-N55-wick-p1.jpg`: a grey-teal graded shaft. THE WICK is a dark lantern with a yellow-green core. There
  are no cyan strips on the shaft walls in view. HUD: "THE WICK LEG 1 / 3", "THE STACK".
- `Logs/feel/n56-r5/b3on/a155/shot-N55-wyrm-turn.jpg`: an umber / brown kiln with an orange-yellow lava grid on the floor. Acid-green edge
  lines run along every ledge, and a large translucent lime-green block on orange feet fills the right third. HUD: "KILN WYRM LEG 1 / 3".
  "PUNCH THE HEAD TO STAGGER IT" is shown large over the centre.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | summary47.txt:157 (b2on), :152 (b1on) and :162 (b3on). Details below the table | two of the four doneWhen clauses fail: THE CROWN's tells kept and the ride panels' cyan |
| 2 Frames | 1 | PASS | the CROWN pair; the WICK p1 frame | THE CROWN's walls visibly lose their yellow, but the pair also differs in colour56's band and yellow56's gun plate. THE WICK's shaft reads graded |
| 3 Nothing lost | 1 | PASS | r2 :154 b1off and :164 b2off "graded live False ... GRADE RED"; r4 :28 b3off "graded live False ... THE WYRM'S TURN RED: the turn frame's walls H 110"; verbs37 x5 PASS | the OFF sides are the old look |
| 4 Look | 1 | PASS (flags) | the frames | the danger hues stay on the ring and the lava. Flags: on THE WYRM the molten tells are not the most saturated class (:162 "Floor S 0.863: NO"; walls near danger 0.92 / 0.71 / 0.93, by design per D-24), and the turn frame still carries acid-green highlights and a large lime shape |
| 5 Not basic | 0 | FAIL | the frames | THE WICK's teal shaft and THE WYRM's orange turn read in the first minute. THE CROWN still looks like itself: only a shadow shift on the walls |

Line 1 in detail:
- **THE CROWN (fails).** "TELLS ... graded n 362 S 0.845 H 10 vs raw n 533 ... the most saturated other class Body S 0.756: **NO**". That
  is 362 / 533 = **0.68 kept**, against >= 0.8 (r2: 0.76, so worse). Walls near danger p2 0.14 (ok; r2 0.29), and "dH 60 ... differs True".
- **THE WYRM (passes).** "THE WYRM'S TURN GREEN: the turn frame's walls H 33 (inside 15-55: ok)".
- **THE WICK's hot wall (passes).** "HOT WALL GREEN ... hot patch S 0.935 (n 294) vs the other walls S 0.315 ... the patch is the most saturated".
- **THE WICK's ride panels (fail).** "THE RIDE PANELS' CYAN CUE ... p1 14 / 0 %, burn 76 / 0 %, cold 61 / 0 %, turn 205 / 13 %, p2 207 / 14 %"
  (r2 b1off: 0 / 0 / 0 / 8 / 8 %). The panels carry no cyan on the burn and cold frames, which are the hot-wall frames.

**GATE V4: FAIL (1, 5)**. Measured reason: THE CROWN's ring tells kept 362 / 533 = 0.68 (>= 0.8 wanted; r2 0.76); THE WICK's ride panels show
0 % cyan on the p1, burn and cold frames (13-14 % only at the turn and p2).

**NIGHT55 P0-wickGrade: FAIL (1)**:
- THE WICK passes every clause: walls 159 / 134 / 106 deg from danger, dH 140, tells S 0.973 with Body 0.894 below, cyan 5.4:1, b1off RED.
- THE CROWN fails. A third of the ring's danger-tell pixels no longer read as tells after the grade (362 of 533 kept). The reel prints
  GRADE RED on b2on.

**NIGHT55 P0-hotWall: FAIL (1, 2)**:
- What passes: no-burn red-orange 0.003 / 0.010, against OFF 0.041 / 0.047 (under half); burn S 0.935, the most saturated region; the
  seams at hue 228; b1off HOT WALL RED.
- What fails: the notes' cyan ride strip "where the player rides" shows 0 % on the burn and cold frames.
- Not evidenced at r5: no burn frame is listed, and the burn is placed "with wick55's ride-only rule suspended".

**NIGHT55 P1-wyrmTurn: FAIL (1)**:
- What passes: the turn walls at H 33 (inside 15-55); the body is far from cyan; b3off RED.
- What fails: THE WYRM's p1 -> p2 change. :162 prints "p1 -> p2 walls dH 4 dV -0.047 (>= 20 deg or >= 0.10: NO)".
- Also failing: "its head within 30 deg of its molten body". The body class is "H p1 75 / turn 84" against the molten tells at H 21, and
  the turn frame shows a large lime-green block.

## B9. P1-wickWindow (NIGHT55 wick55)

No frames.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | **(bot)** Logs/n56/wick-bot.out "skilled-on": 40/40, median 57.5 s, deaths total 7, "dealt in windows median 57 %", "hot-wall burns 0 ... of 183 hits taken (0 %)"; "casual-on": 40/40, median 60.9 s. Wick55Tests 4/4 (NIGHT56-LOG.md:87; suite-r5.txt:8). **Player:** Logs/feel/n56-r4/summary47.txt:13 bfon "**RED: FIGHT MODE** ... damage dealt while the bar read OPEN 26 % (>= 50 %) ... core punches 0, inside a window 0 (the first at never: no frame) ... alive at 150.0 s (bar 40 %)"; :18 bfoff, the same numbers, and its header reads "w55 True" | every bot line passes. The slate's "a454 GREEN in the Player" fails: no core punch inside a window, and the ON and OFF Player runs print identical numbers |
| 2 Frames | 0 | FAIL (not evidenced) | the notes want "a454 prints >= 1 core punch inside a window on a named frame" | there is no such frame ("never") |
| 3 Nothing lost | 1 | PASS | fingerprints --off55 EQUAL (:128); Wick55Tests includes "every attack exactly as wick54 ships" (log :62, now passing) | - |
| 4 Look | - | n/a | sim | - |
| 5 Not basic | 0 | FAIL | bfon = bfoff | in a Player, wick55 on cannot be told from off |

**GATE P1-wickWindow: FAIL (1, 2, 5)**. Measured reason: the Player FIGHT MODE is RED on both sides (core punches 0, damage while OPEN 26 %
< 50 %, alive at 150 s), and its OFF run prints "w55 True" and the same numbers. The bot passes (57 % in windows, median 57.5 s).

## B10. P1-teachText (NIGHT55 brief55), attempt 2

No frames listed for the overlap. I saw teach lines in the a356 frames I opened for other entries: "Punch [ZR] the shield or flank",
"Pull to the ring with [R]", "Yank an enemy with [R]".

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | my n55k-teach.cjs run: foundry "lines 3, bad 0; a card over the counter in 0 frames"; iceline same; span same; **kiln "lines 6, bad 0; a card over the counter in 1 frames (worst 2.4 px) BAD"**; **spire "lines 2, bad 0; a card over the counter in 1 frames (worst 2.4 px) BAD"**; "RED teachText". Every line is <= 6 words with a glyph and <= 337 frames | "0 px overlap" is not met. It is 1 frame at 2.4 px on two levels (r2: 94 frames, 42.1 px). Kiln's "Jump B, aim down, fire ZR" is counted ok by the tool |
| 2 Frames | 0 | FAIL (not evidenced) | no before / after frames listed | - |
| 3 Nothing lost | 0 | FAIL (not evidenced) | verbs37 x5 PASS (:224-392). No OFF side of brief55 on r5 | the RED off line is not given |
| 4 Look | 1 | PASS | the a356 frames above | the teach text is about 28-30 px at 1080p (>= 16), and there is no restyle |
| 5 Not basic | 1 | PASS | telemetry: a line at f0 on every level | - |

**GATE P1-teachText: FAIL (1, 2, 3)**. Measured reason: a teach card overlaps the counter plate for 1 frame by 2.4 px on kiln and on spire
(0 px wanted); n55k-teach prints "RED teachText".

## B11. P0-smoke (NIGHT55), attempt 3

No frames.

| line | score | PASS / FAIL | evidence | reason |
|---|---|---|---|---|
| 1 Numbers | 0 | FAIL | details below the table | the smoke itself is clean and printed its causes. Two premortem lines are not met: the second planted steal and the outside-smoke pause test |
| 2 Frames | - | n/a | harness | - |
| 3 Nothing lost | 1 | PASS | :3165 "backgroundBehavior restored to ResetAndDisableNonBackgroundDevices" | the smoke-only switch is undone |
| 4 Look | - | n/a | - | - |
| 5 Not basic | - | n/a | - | - |

Line 1 in detail:
- **What is met:**
  - Logs/n56/smoke-steal-n56-r5.out: "steal at +61 s: ... foreground pid before 73452 after 115432, SetForegroundWindow True, took True"
    and "[smoke] 232.5s DONE: all steps passed".
  - Logs/player/n56-smoke-steal-n56-r5.log:763 "53.2s n55 focus LOST: flow Playing, cursor Locked" and :2442 "PASS tape: 486 steps ... EQUAL".
  - No builder was running (the builders' last reports came at 04:33-04:38, NIGHT56-LOG.md:123; the smoke ran 04:56-05:00).
- **Not met, 1 (the ~200 s steal).** The steal at +201 s took focus from "foreground pid before 28328", not from the game. After the first
  steal's window closed, focus went to pid 28328 and never came back. :3165 prints "focus changes 1, unfocused 69032 of 89206 frames", so
  there is one focus-loss line, not two. (Running 77 % of the time unfocused and still passing is itself a strong focus-proof result.)
- **Not met, 2 (the outside-smoke pause test).**
  - N56FocusPauseTests (3 / 3, suite-r5.txt:4) asserts only the pure predicate `SurvivalGame.PausesOnLockDrop(Flow.Playing, wasLocked
    true, locked false, ...)`, a dropped cursor lock. It never drops focus or drives the flow to Paused.
  - In both Player steal smokes a real focus loss left the cursor locked and dropped no lock: r5 :763 "cursor Locked", :3165 "dropped
    cursor lock 0"; r1 Logs/player/n56-smoke-steal-n56-r1.log:88 "cursor Locked", :191 "dropped cursor lock 0".
  - So the rule the test pins was not triggered by any real focus loss on record. "Outside -skygrindSmoke the game still pauses on focus
    loss" is not evidenced.

**GATE P0-smoke: FAIL (1)**. Measured reason: the focus-pause test asserts a lock-drop predicate, while both Player logs show a real focus loss
with "cursor Locked" and "dropped cursor lock 0" (no evidence the game pauses on focus loss outside the smoke); the second planted steal did
not take focus from the game ("focus changes 1").

---

## Verdicts

- GATE G1: FAIL (1, 2, 3, 5)
- GATE G2: FAIL (1); NIGHT55 P1-kill: FAIL (1)
- GATE G3: FAIL (1)
- GATE G4: FAIL (1)
- GATE V1: FAIL (1); NIGHT55 P0-decor: FAIL (1) (closes with V1, D-18)
- GATE V2: FAIL (1, 2, 5)
- GATE V3: FAIL (1, 4, 5)
- GATE V4: FAIL (1, 5); NIGHT55 P0-wickGrade: FAIL (1); NIGHT55 P0-hotWall: FAIL (1, 2); NIGHT55 P1-wyrmTurn: FAIL (1)
- GATE P1-wickWindow: FAIL (1, 2, 5)
- GATE P1-teachText: FAIL (1, 2, 3)
- GATE P0-smoke: FAIL (1)

Lines that passed for the first time at r5, for the record:
- G2's 3rd-and-later kills now hold 0.083 s.
- V1's whole-frame tint passes on every frame.
- V4's THE WYRM turn is at H 33 (inside 15-55), and THE CROWN's p2 walls are out of the near-danger band (0.14).
- G4's median (86.8 s) and p90 (178.3 s) are inside their band (bot).
- P1-wickWindow passes every bot line.
