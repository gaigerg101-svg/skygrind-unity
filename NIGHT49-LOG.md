# NIGHT49 LOG (the lead)

t0 = GG's go, 2026-10-19 ~11:44 MST (NIGHT49.md written 11:43). Hard cap 4 h: 15:44 MST. The close starts by 15:25 (the
prompt says 3:40 after t0; the close table is printed by the cap whatever is unfinished). Base: port/stage3 at u3-n48 (a8fe95cd)
is an ancestor of HEAD a5f78da8, which is the base (checked at 11:47: HEAD = a5f78da8, the NIGHT49 planning set on top).

## CLOSE TABLE (13:56 MST, t0 11:44; the freeze ran at 13:22-13:53 with the builders stopped)

Read this first. The night shipped ONE view row (lights49) and six n47/n48 view and level rows from NIGHT48 and NIGHT47. GG's charge
bar (G1) is NOT back: the judges chose the old arc40 three times out of three. The grade (G2-G4 environment) and the kit (G5) are OFF.
GREEN is written only where the row's done-when was met on frames and the blind pair; SHORT and OFF carry their reason.

| row | state | reason / measure | frame or sheet path |
|---|---|---|---|
| G1 charge element (H) | SHORT (charge49 OFF) | arc40 beat candidate 1 3-0, candidate 2 3-0, candidate 3 3-0 (three blind pairs); GG decides (DECIDE49 D-16, D-18) | Logs/n49/sheets/h49-pairs-all.png; Logs/feel/n49-r2/h1080/a149/shot-N30-H49-pair-{arc40,c3}-50-1920x1080.png; Logs/n49/sheets/r2-h-c3-centre.png; reel a149 GREEN both sizes (1920x1080, 1280x720; nearest 50 px) |
| G2 lighting (L, lights49) | GREEN (tie) ON | landmark x1.5 on the five levels (reel a449 PASS x5); off-role cyan / white 0 on all five; blind sheet TIED on five rows (the change is small at sheet size) | Logs/feel/n49-r3/l-on-<level>/a449/shot-N29-L49-<level>-landmark.png; sheet Logs/n49/judge/l49-sheet/sheet.png |
| G3 arenas (V grade + L lights) | SHORT | the arena grade is OFF (below); the lights alone tie, they do not reshape the arenas | as G4 |
| G4 colours (V, look49) | SHORT (look49 OFF) | meter foundry 4/4 and spire 4/4 PASS; span 3/4, iceline 2/4, kiln 2/4; blind sheet won or tied 2 of 5 (kiln 2-1, spire 3-0 for new; foundry, iceline, span for old) | Logs/feel/n49-r3/v-<level>/a349/shot-N49-V-<level>-combat-{off,on}.png; sheet Logs/n49/judge/v49-sheet/sheet.png; Logs/n49/sheets/r1-v-combat.png |
| G5 impact kit (X, fx49) | SHORT (fx49 OFF) | kit pair won 3-0 for ON; wash passes on torch 0.22, pulse-punch 0.18, parry 0.09, pulse-blast 0.20, kestrel-charge 0.23; the KILL is +0.34 ON = +0.34 OFF (the web death burst in SurvivalGame.Init.cs, enemy colour, outside X's files) | Logs/feel/n49-r3/x1080/a249/ (X49-sheet-on, X49-pair-P/-Q; key in Logs/n49/judge-keys/x49-kit-pair.json) |
| G6 ULTRAKILL / DOOM effect (all four) | OFF (not shipped) | only lights49 ships; the four were not judged together | DECIDE49 D-19 |
| P0 baseline flip (OnPathThrough 48) | DONE | fingerprints EQUAL plain / --off47 / --off48 / --off49 | c16a86c9 |
| P0 view / level rows ON | DONE: climbTop48, foes48, loomHull48, wickRideParry47, wickIntro47, wickFinale47 | verbs37 26 / 20 / 25 / 30 / 30 on the freeze build (RED rows [] on all five, NRE 0); smoke DONE all steps passed (237.5 s, NRE 0) | Logs/feel/n49-freeze/summary47.txt; Logs/n49/smoke-freeze.txt |
| P0 blast48 | OFF | the lance's frames with the row on were never taken; not shipped | DECIDE49 D-13 |
| P0 guns48 | OFF | the FOV punch's frames with the row on were never taken | DECIDE49 D-13 |
| P0 wyrmTell48, wyrmDive48 | OFF | worm band OUT, 40 seeds: shipped median 122.6 / p90 256.9 s; forced on 148.7 / 220.2 s (p90 limit 180) | Logs/n49/meters/wyrm-default.txt, wyrm-r48.txt; DECIDE49 D-15 |
| P0 kilnBoost48 | OFF | not measured alone on the n49 build | DECIDE49 D-15 |
| P0 finale47, stampede47 | OFF | no frame taken with the row on (D-7) | DECIDE49 D-7 |
| P0 flame48, loomReach48 | OFF | flame48's a548 frames not retaken (T-95); loomReach48 D-37 | NIGHT48-LOG |
| P0 drone49 (NIGHT48 drone part) | OFF | the drone keeps its NIGHT47 look (the old drone won three pairs) | DECIDE49 D-2 |

Measures: the test suite on a9b1442d: 2208 run, 2206 passed, 2 failed (both fixed in 4c5b99bd; the shared tests re-run in batch 105 / 105;
the full suite was not re-run). Flow table (n47-flow-table.sh, commit 4c5b99bd): walks 20/20 on all five levels; worse past the band:
L2 clear time 184.8 -> 220.4 s and L3 stall share 31.7 -> 40.3 % (both were worse at NIGHT47 too). The table's level 1 is n47-foundry:
the n48-foundry that climbTop48 ships is NOT in this table (not checked). Judges: 18 valid blind votes (the first charge pairs' 6 black-frame
votes were discarded). Forge measure (FORGE-NOTES): the look rows G1-G6 closed with both a meter and a pair: 5 of 6 (G6 was not built).

Builds: n49-r0 (5f212064), n49-r1 (8d63b440), n49-r2 (218ac85f), n49-r3 (1bdd8131), n49-freeze (a9b1442d); test fixes 4c5b99bd.
Shipped: a9b1442d = lights49 ON on top of the P0 rows; fx49, look49, charge49, drone49 OFF; -skygrindOff49 turns lights49 off.

## STOP POINT (12:41 MST): round 1 read on n49-r1; round 2 ordered

Close table so far (the lead's looks in the log below; GREEN is written only by the lead):
| row | state | evidence |
|---|---|---|
| P0 baseline flip | DONE | c16a86c9; fingerprints EQUAL plain / --off47 / --off48 / --off49 |
| P0 rows ON | DONE for 6 rows | 5f212064: climbTop48, foes48, loomHull48, wickRideParry47, wickIntro47, wickFinale47 |
| P0 gates | GREEN on 5f212064 and on the integrated HEAD 8d63b440 | golden 22/22, fingerprints EQUAL, dn-check 0, Editor compile 0 errors |
| P0 verbs37 (n49-r0) | 26 / 20 / 25 / 30 / 30 PASS, NRE 0 | Logs/feel/n49-r0/summary47.txt |
| P0 blast48, guns48, wyrmTell48, wyrmDive48, kilnBoost48 | OFF with reasons | DECIDE49 D-13, D-15 (worm band OUT, p90 220 s forced on) |
| P0 finale47, stampede47, flame48, loomReach48, drone49 | OFF with reasons | DECIDE49 D-7 |
| G1 charge (H) | SHORT | n49-r1 reel a149 GREEN on its numbers; blind pairs: arc40 beat c1 3-0 and c2 3-0 (DECIDE49 D-16). Round 2: candidate 3 |
| G2-G4 grade (V) | SHORT | a349 RED: saturation 6-7 % vs 10-30 % (DECIDE49 D-17). Round 2 ordered |
| G2-G4 lights (L) | SHORT | a449 off-role 0 on all five levels; landmark RED on foundry, iceline, kiln (suspected fallback to one light). Round 2 ordered |
| G5 kit (X) | SHORT | a249 RED on the wash rule (+0.51 off / +0.52 on, the flash itself). Round 2 ordered |
| G6 ULTRAKILL / DOOM effect | NOT STARTED | judged with the four at the close |
Frame paths: Logs/feel/n49-r1/<side>/ (h1080, h720, x1080, v-<level>, l-on-<level>, l-off-<level>), sheets Logs/n49/sheets/.
The NIGHT48 close table (NIGHT48-LOG.md) is the base state.

## Log

- 11:48 lead: read NIGHT49.md, CLAUDE.md, HANDOFF.md, NIGHT48-LOG's close table, NIGHT48-BRIEF.md, CODE-FACTS.md, UNIFY.md (head),
  n48-shared.cjs. Unity Editor status `ready`, playMode `stopped` (unity-loop.sh status, 11:48). Drone gate target: EnemyViews.Drone.cs
  (foes48 at :280-283); reel capture target: FeelReel.cs:264 (Capture).
- 11:48 lead: G0 starts (the shared commit: the drone gate, Look49's tokens, the Capture fix, the scaffolding; see DECIDE49 D-1..D-6).
- 11:49 lead: Tools/night/n49-shared.cjs written (from n48-shared.cjs); `--dry` found every anchor once (11 files + 1 copy); run for real.
  Writes: design.n49 (five view rows, all enabled false: charge49, look49, lights49, fx49, drone49), UnityTuning.Look49.cs (the one
  palette, seeded from the web colours), FeelReel.N49.cs (a149 H, a249 X, a349 V, a449 L), N49SharedTests.cs, the routing in
  FeelReel.N27.cs, the drone49 gate in EnemyViews.Drone.cs, the Capture fix in FeelReel.cs, Tools/night/n49-lookmeter.py.
  The drone row is named drone49 (not foes48drone) because NightRows.ViewOn reads the night from the name (DECIDE49 D-2).
- 11:50 lead: dn-check (no Editor, `Tools/night/dn-check.sh`): 0 errors in all four projects.
- 11:51 lead: Tools/night/n49-gates.sh --no-editor: config copies identical; golden 39 vector sets (48094 records), 22 replays (22
  exact), 0 failing, 0 mismatches; fingerprint plain / --off48 / --off49 EQUAL (12 hashes, seed 1: 4808 earned, 35 kills, same run).
- 11:51 lead: Editor compile (`unity-loop.sh compile`): DLLs rewritten; errors: compilationFailed false, consoleErrors 0.
- 11:52 lead: `unity-loop.sh tests N49Shared`: 2 / 2 passed.
- 11:52 lead: G0 committed: c5a6b7bf (the shared commit; 15 files, explicit paths; GG's four files not staged).
- 11:53 lead: builders H, V, L, X spawned as worktree agents from c5a6b7bf (NIGHT49-BRIEF.md; round 1 ~60 min each). Their rows
  stay OFF in config; their reel phases a149 / a349 / a449 / a249 are dispatched from FeelReel.N49.cs.
- 11:58 lead: P0 step 2 (the baseline flip): RowsBaseline.OnPathThrough 47 -> 48, commit c16a86c9. Fingerprints measured before and
  after: plain, --off47, --off48, --off49 all EQUAL (12 hashes, seed 1, kills 35, shots 80, earned 4808).
- 12:00 lead: P0 step 3: Tools/night/n49-rows-on.cjs (generic over design.n47 / n48 / n49; --dry first; both copies written).
  Reasons: Logs/n49/p0-reasons.json.
- 12:01 lead: P0 step 4 (the rows ON whose frames are GREEN in NIGHT48's close table): climbTop48 (level, G1), foes48 (G12; its drone
  part is drone49, OFF), loomHull48 (G36), wickRideParry47 / wickIntro47 / wickFinale47 (G9). Sim and numbers rows (wyrmTell48,
  wyrmDive48, kilnBoost48, guns48) and blast48 wait for the n49-r0 meters and verbs37 (DECIDE49 D-7). flame48, loomReach48, finale47,
  stampede47 stay OFF.
- 12:01 lead: P0 step 5 gates on the rows-on config: config copies identical; golden 22 / 22 exact, 0 mismatches; fingerprint plain /
  --off48 / --off49 EQUAL. Commit 5f212064 (the rows + the script).
- 12:02 lead: the batch build `n27-batch-build.sh 5f212064 n49-r0` started (background; batch copy detached at 5f212064).
  Next: verbs37 on the five campaign levels, one Player at a time: n48-foundry (level 1, climbTop48 ON), n47-iceline, n47-kiln,
  n47-span, n47-spire.
- 12:04 lead: `n27-batch-build.sh 5f212064 n49-r0`: Build succeeded, 0 errors, 519 warnings, 439 MB, 1 min 28 s (Builds/n49-r0).
- 12:05-12:13 lead: verbs37 on n49-r0 (Logs/feel/n49-r0/summary47.txt): n48-foundry 26/26, n47-iceline 20/20, n47-kiln 25/25,
  n47-span 30/30, n47-spire 30/30; RED rows [] on all five; NRE 0 / exceptions 0 on all five. (NIGHT48's n47-r3 numbers were
  26 / 20 / 25 / 30 / 30: the same.) The Player's frame cap 1800 (the recorder's count) was not hit by the verbs reels.
- 12:30 lead: builder V reported round 1 (BUILT, not measured): look49 (G2-G4 environment). Cherry-picked onto port/stage3 as
  41cbdef3 (the grade, the per-level values, UnityTuning.V49.cs, PostFx, RoofView27), 3293b2e3 (look49's 11 numbers + V49Tests),
  daafd826 (FeelReel.N49.Look.cs, reel a349). V's per-level hues: foundry 158, iceline 216, kiln 72, span 280, spire 124 (each at
  least 27.6 deg from every role). Proposal to L (not a change): Look41's kiln room light (hue 33) sits 11 deg from the parry yellow.
- 12:20-12:30 lead: round 1 reports from H, V, L, X (all BUILT, none measured). Cherry-picked in order: V 41cbdef3..daafd826, L
  fd0bea9e..6504f328, H 50e64d27..1574b18d, X 006ef379..8d63b440 (all clean). dn-check on the integrated HEAD: 0 errors.
- 12:25 lead: full gates on 8d63b440 (Tools/night/n49-gates.sh): golden 22/22 0 mismatches, fingerprints EQUAL x3, Editor compile
  compilationFailed false, consoleErrors 0.
- 12:27 lead: batch build n49-r1 from 8d63b440: 0 errors, 439 MB, 35 s. The four view rows are turned ON in the BUILD COPY only
  (node Tools/night/n46-build-rows.cjs n49-r1 on n49.charge49 n49.look49 n49.lights49 n49.fx49); the OFF side passes -skygrindOff49.
- 12:29-12:38 lead: round-1 reels (Tools/night/n49-reels.sh n49-r1 h x v l-on l-off, serial): a149 GREEN on 1920x1080 and 1280x720
  (rest hidden, 0 under the bars, flash 2/2, nearest changed pixel 23.5-26.3 px). a249 RED (wash +0.51 off / +0.52 on, the flash
  itself). a349 RED on four levels (saturated 6-7 %), spire RED on dark/mid too. a449: off-role 0 on all five; landmark RED on
  foundry, iceline, kiln (identical numbers suggest a fallback light), PASS on span and spire. NRE 0 / exceptions 0 on every reel.
- 12:33-12:50 lead: H's blind pairs (3 judges each, run TWICE: the first set got black crops from my wrong crop window, discarded;
  the key line of n49-pair.py was inverted, fixed and the keys rewritten with the sha-checked mapping): arc40 beat c1 3-0 and c2
  3-0. Both candidates retired; round 2 builds a third (DECIDE49 D-16). Judges' outputs stay in the conversation, not in the repo.
- 12:41 lead: round-2 orders to H, V, X, L (SendMessage; the builders resumed). The lead's frame looks so far: H's a149 centre
  crops (Logs/n49/sheets/r1-h-centre.png), the H pair sheet (Logs/n49/sheets/h49-pairs-all.png).
- 12:55 lead, the look at V's combat frames (Logs/n49/sheets/r1-v-combat.png, look off | on, five levels): foundry and iceline read
  darker and cooler with look on, HUD cyan exact; spire's cool teal reads well; n47-span's look-on floor and far wall turn PINK,
  close to RIVAL's magenta family (magenta pixels 0, but the read is wrong: rule 8); n47-kiln's look-on reads olive mud. Sent to V as
  an addendum to round 2 (the saturation order stays).
- 13:20 lead: round 2 reports from H, X, L cherry-picked: H f7c828eb..1eff6c55 (candidate 3 only, pair frames), X fddddabf (the
  KESTREL charged view Pistol39View.cs is the wash source, +0.51 off / +0.52 on: routed with fx49, the critical tier smaller in
  wash), L a20ce76e (the landmark cap: a light moves in toward its target when the intensity cap cannot reach x1.5; a non-lava pit
  counts as a fall edge only if 1.0 m deep). dn-check 0 errors after each pick. V's round 2 is still running.
- 13:05 lead: round-2 reels on n49-r2 (rows ON in the build copy; OFF side -skygrindOff49): H a149 GREEN both sizes (candidate 3:
  nearest 50 px); X a249 kill +0.32 / torch +0.30 ON (wash still RED), kestrel-charge ON 0.23 (was 0.52), kit pair won 3-0 for ON;
  V a349 foundry 4/4 PASS, iceline 1/4, kiln 2/4, span 0/4, spire 0/4 (round 2 is worse than round 1); L a449 landmark PASS on all five,
  off-role 0, the blind sheet TIE on five rows. H's pair (candidate 3 vs arc40): arc40 3-0 (the third loss): G1 SHORT (DECIDE49 D-18).
  Round 3 orders: X (25 min, the three effects' wash), V (30 min, dark/mid back on four levels).
- 13:25 lead: round 3 integrated: V 5d7e4553 -> 1fb4b54e, X 60d1f1a7 -> 1bdd8131 (dn-check 0 each; gates on 1bdd8131 green).
  Batch build n49-r3 (1bdd8131): 0 errors. Round-3 reels (n49-r3, rows on in the build copy): X kill ON +0.34 (fails; OFF +0.34:
  the web death burst, not in X's files), torch 0.22, pulse-punch 0.18, parry 0.09, pulse-blast 0.20, kestrel-charge 0.23 (pass);
  V foundry 4/4, spire 4/4, span 3/4, iceline 2/4, kiln 2/4; L landmark PASS x5, off-role 0 x5.
- 13:19 lead: V's look blind sheet (5 combat rows, look49 off vs on, 3 judges): new won kiln 2-1 and spire 3-0; old won foundry
  1-2, iceline 0-3, span 2-1 (1 tie). Won or tied 2 of 5 (bar 4 of 5): look49 OFF; G2-G4 environment SHORT (DECIDE49 D-19).
- 13:22 lead: FREEZE. lights49 ON (commit a9b1442d; the rows-on through Tools/night/n49-rows-on.cjs). Freeze gates on a9b1442d:
  golden 22/22 0 mismatches; fingerprint plain / --off48 / --off49 EQUAL; Editor compile 0 errors. Freeze build Builds/n49-freeze
  (a9b1442d, batch): 0 errors, 488 warnings, 14 s. Next: verbs37 x5 (Logs/feel/n49-freeze/summary47.txt), the smoke, the batch
  suite, the flow table.
- 12:36 lead: wyrm (n47human, 40 seeds): shipped rows OFF median 122.6 / p90 256.9 / min 45.6 s, OUT. With wyrmTell48 +
  wyrmDive48 forced ON: 148.7 / 220.2 / 76.2 s, OUT (p90). Rows stay OFF (DECIDE49 D-15).
- 12:14 lead: the worm's band meter (n47human --boss wyrm, 40 seeds) started in the background: Logs/n49/meters/wyrm-default.txt (the
  config as it ships) and wyrm-r48.txt (--rows wyrmTell48,wyrmDive48). Decides wyrmTell48 / wyrmDive48 (DECIDE49 D-7).
