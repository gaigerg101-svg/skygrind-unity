# NIGHT71 REPORT (THE BUILD GG PLAYS; t0 10:46:36 MST, 2026-10-26)

Order follows NIGHT71.md section 9. Numbers cite the files under Logs/ and Research/ named beside them. Anything not run is "not checked".

## 1. What is still not good
- The play build has **eight new rooms, not ten** (the aim). Levels 1, 2, 3 and 5 each have some rooms; the rest stayed shipped because a guard or a Player check failed (section 4).
- **Three rooms fail a Player check that their guards do not show:** 3.2c THE ASH FLOOR (kb2c; with it in the kiln, verbs37 reads 30/30 against 31), and 5.2 THE NAVE and 5.5 THE BELL WALLS (with both new on the spire, verbs37 reads RED on the level's first wall, rows 9 and 11: no ride at all). Not traced.
- **No new row is ON.** Every B and F row is OFF. The ones with numbers: voice71 counter GREEN but its critic awaits the iceline and span verbs37 with the row alone; words71 counter GREEN, SPOT NOTHING OBVIOUS; wyrm71 stagger 18,991 against 41,472 (SHORT); cyan71 role check fails (flinch 370, idle 516 player-cyan px); kicks71 probe GREEN, SPOT did not name the kick; wash71 f3-f4 0.255 against 0.25 (SHORT); glow71 RED on the PULSE check.
- **The bench (F1) was not run:** the vsync arm is built and the worst-frame source is named only as "outside the game's own update, not found from code".
- **Nobody has played it.** GG's last recorded play reaction is 2026-10-20 (OWNER.md).
- Most of the night's checks were headless (the reds and the walks); the Player checks were the lead's and few. The guards are not the verdict; GG is.

## 2. GG's play card
PLAY71.md (committed; the play build is Builds/n71-play, made from the freeze n71-freeze). In short: the eight rooms by level (1.6 THE KNOCKOUT YARD, 1.10 THE SPILLWAY; 2.4 THE ORB YARD; 3.2 THE CHARGING FLOOR, 3.3 THE PRESS HALL, 3.3c THE TAP RUN, 3.3d THE CRACKED FLOOR; 5.6 THE CHOIR), how to reach them (TITLE > PLAY > the level select; DOORFREE on F8 or RB 3 s), the numbers as information, the keep / drop boxes, and what is not in the build. The calls for GG are in DECIDE71.md (Q-1 to Q-7).

## 3. What was built, per track (numbers and files)
- **Rooms (R; Research/2026-10-26-wave71/R/ledger.md):** eleven rooms held every guard in R's tree against today's reds; in the main tree the lead guarded and kept kb2 (ee94d5b14), kb2c (r5, ed93ab362), kb3 (8a8239934), kb3c, kb3d (7c8aeecfb), g4 (76fe21c99), v6 (spire2 alone). kiln5 (kb2 kb2c kb3 kb3c kb3d together): ROOM PASS for all; walks stall 18.9 / 28.2 / 24.5 / 33.1 / 35.7 against the bases 18.6 / 25.1 / 23.1 / 33.0 / 39.7, deaths 0; air 9=9, 14 vs 12, 6=6, 16 vs 14, 5=5; pads at the base. g4 (iceline4 alone): ROOM FAIL R5 and I2, both on the base too; walk stall 20.6 vs 25.5, deaths 0; air 68 vs 56; pads 3=3. v6 (spire2): ROOM FAIL R5 (base too); walk 19.3 vs 18.9; air 23 vs 18.
- **Level rows (the play build):** the four rebuilt level files, turned on in the play build's config only: n65.foundry65 (rooms 1.6, 1.10), n67.kiln67 (3.2, 3.3, 3.3c, 3.3d), n67.spire67 (5.6), n67.iceline67 (2.4).
- **Bosses and colour (B; Research/2026-10-26-wave71/B/ledger.md):** voice71 (one order on THE CROWN): counter `both drawn` 209 OFF / 0 ON (Logs/player/n47-reel-n71-p1-on-vcoff.log, -vc.log); SPOT names the change (Logs/n71/spot/voice71). words71 (the orders name the hand you hold): counter 837 OFF / 0 ON. wyrm71 round 2 (the body inside BossMotion63's guard): idle 3,364 (bar 3,000, met), stagger 18,991 (bar 41,472, NOT met). cyan71: role check BOSS FAIL (flinch 370, idle 516 ON; stagger 0 changed px). room71 (L1; the pick-a-room start): ON starts 3.3 THE PRESS HALL and enters it 2.98 s after the run began; OFF ignored. L3 (ICEBREAKER's orders): diagnosed, SHORT. L5 (kill55's marks): already met since NIGHT60. L8 (reshape71): SHORT, a sim change.
- **Feel (F; Research/2026-10-26-wave71/F/ledger.md):** F1 the vsync arm (-skygrindVsync 0|1) and a worst-frame source print: 154 of 156 worst frames' extra time is outside the game's update; the engine part is not found. F3 kicks71: probe GREEN (ON hit 1 / kill 2; OFF 0 / 0); the SPOT pair was not aligned and named HUD differences (SHORT). F4 taps: headless 6 inside a freeze, 3 fired; the Player reel 4 of 4 fired on both sides (no press-fired tap in that reel; not reproduced). L10 press71's first half (-skygrindPress71, a launch flag): built, headless 6 of 6. L4 wash71: ON peak +0.255 against +0.25 (SHORT by 0.005; OFF +0.384). L6 glow71: RED (both sides "not ok: PULSE"). L2 pace71: no fix (the source is not named).
- **Cleanup (D; Research/2026-10-26-wave71/D/ledger.md):** ten commits on the main branch: the three D1 rows (flow46, drone49, teach42) and seven D2 batches (N1-N4, batches 8, 9, 10). 25 files, 359 lines deleted, 20 changed. The cleanup critic (Logs/n71/verdict-D1.md, verdict-D2.md) passed both. Notes left for NIGHT72: textHex and faintHex in UiHotbar.cs are now written but never read; some comments still describe the removed rows; teach42 may be an open question (DECIDE71 Q-6).
- **Tools:** Tools/night/n71-*.cjs and n71-*.sh (the clock, runner, slate, rows, close check, reels, gates) ran the night; the close check was run in --pre and must pass the final time.

## 4. What failed (numbers)
- **Rooms out of the play build:** g8 (I2 1.17 against 1.40); g7 (stall A 46.7 against bar 39.3; air 4 against 95); g2 (verb-aware walk stall 59.8 against 54.3); g5 (walk stall A 51.7 against 51.1 after its IN VIEW fix); g3 and g6 and g1 (landed in R's tree; g3 not in the main tree's play build; g6 and g1 not guarded in the main tree); v2 (5.2 THE NAVE) and v5 (5.5 THE BELL WALLS): guards good, spire verbs37 RED rows 9 and 11 (no ride on the first wall: 0.00 s) when both new; kb2c (3.2c): kiln verbs37 30/30, and R's r5 fix was not confirmed in the Player.
- **Shipped-map labels under the iceline row (n52-iceline):** verbs37 25/29 RED with the row ON; 29/29 PASS with every night row off (-skygrindOff65). R's trace: the reel's old-map gate is bypassed by the level swap; zero grapple nodes in either level. Not fixed; a Player verb label, not a lost node (DECIDE71 Q-3).
- **Test failures found and fixed:** N71RoomTests 4/5 (B changed the test fixture, not the code: GameConfig caches a path per config instance; the four assertions are unchanged; the lead accepted it and recorded the deviation at NIGHT71-LOG 13:43); N71KicksTests 4/5 (a >/>= gap boundary; F fixed the code, 6550100f8).
- **Guards that failed and were fixed by R (the fixes are in R's tree; v2, v5 and g3 are not in the play build):** the IN VIEW fallbacks: v2 5 of 5 nodes failing (the shipped v2: 0 of 4), v5 vista 3.2 of wave 1 (the shipped: 4.0 PASS), g3 3 of 3 nodes failing (the shipped: 0 of 3); kb2 (I1 mirror 63 %); kb3 (hand-offs 3 against 6); v2 (hand-offs 3 against 33); a2 (walk stall +18); a3 (hand-offs 19 against 45, pads 1 against 2); a4 (IN VIEW fail); a5 (pads 1 against 3).
- **A spot that named nothing:** words71's SPOT on the tell frame (NOTHING OBVIOUS); kicks71's SPOT (the frames were not aligned).
- **Late builds:** PB0 was assembled at 1:33 (12:19), against the plan's 1:15, with four rooms; PB1 at 4:54 (15:40); the final play build at 5:45 (16:31).
- **The runner ran a red on a level before it was regenerated** (kiln4, 14:26-14:33): the output was discarded, the job fixed (028b, 029b).
- **Commit pressure:** commit free dipped to 1.9 GB (12:24) and 1.1 GB (14:40), and one fork failed (13:02, errno 11), each during a builder's or a red's spike; the build and Player steps that ran were not lost to it.

## 5. Not checked
- The bench F1 (the pairs and median of 3): not run.
- B5 blast48 / guns48 retakes (a748): not run.
- F3 kicks SPOT with aligned frames; words71 SPOT on a frame with the order line; room71 SPOT on the level select.
- cyan71 on the other bosses (only the foundry's wick boss was run).
- tour65 on the kiln (not run).
- The per-commit gates of the D2 batches (only the last batch ran the gates; D's critic note).
- The shipped iceline's label under the row (the reel gate; DECIDE71 Q-3).
- The Editor suite beyond the touched classes (N46/N49/FeelHud/Hud20/N71* and the wyrm and cyan classes).
- The audit's remaining CANDIDATE rows (Research/2026-10-26-forge71/DEADCODE-candidates.md) other than D1's three, and DECIDE45 D-3's six old rows: listed, not decided (DECIDE71 Q-5).
- The E71 longest gap (below).

## 6. Your calls (DECIDE71.md)
Q-1 keep / drop / change for each of the eight rooms. Q-2 the night's OFF rows (what to play in a special build). Q-3 the iceline label. Q-4 freeing the disk (Logs ~207 GB, Builds ~79 GB). Q-5 the six DECIDE45 D-3 rows (untouched). Q-6 teach42 (D-11). Q-7 the minor findings.

## 7. NIGHT72's seed (the first 30 minutes)
1. Flip GG's kept rows ON (the four level rows are already ON in the play build; keep, drop or change per PLAY71) and delete the dropped rooms' rows from the play build only.
2. Trace the spire's first-wall RED (rows 9 and 11) with v2 and v5 new (R's verbs-wall tool; the harness re-performs the reel's pick) and the kiln's verbs total 30/30 with kb2c (its wall walk: R's r5 fix).
3. The rooms left out: g3 and g5 and g2 and g7 and g8 and g6 and g1 (R's list; the IN VIEW fallbacks green for g3, v2 and v5 after the last fix); re-guard by the same red rule and verbs37 on each new level in a Player.
4. The B and F rows: voice71 (its verbs37 with the row alone on the iceline and span), wyrm71 (a stagger that clears 41,472 within BossMotion63's guard), words71 (a SPOT frame with the order line), cyan71 (flinch and idle to 0 player-cyan px), kicks71 (an aligned SPOT pair), wash71 (0.005 under the bar), glow71 (the PULSE check).
5. The bench F1 in a quiet window (no dotnet, no Player beside it) and the B5 retakes.

## 8. The close lines, every ROW line, and E71's numbers
Close check lines and ROW lines: see the output of `node Tools/night/n71-close-check.cjs --pre` pasted below by the lead at the close.

ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
ROW: n67.iceline67 enabled=false; SLOTS: g1=old g2=old g3=old g4=old g5=old g6=old g7=old g8=old
ROW: n67.kiln67 enabled=false; SLOTS: kb2=old kb2c=old kb3=old kb3c=old kb3d=old
ROW: n67.spire67 enabled=false; SLOTS: v2=old v5=old v6=old

(The four rows are OFF in the repo; their slot files are all old; the play build's rebuilt levels are generated under Logs/n71/play/ and turned on only in Builds/n71-play's config. The close check's `--pre` output is the record: copy its lines here at the close.)

**E71 (the forge's experiment, DECISIONS D-8):**
- Work minutes: the span with at least one builder running is 0:16 to 5:26 (the spawn of R, B, F and D to R's last message at 16:12): about 310 min of 480, or 65 %. Minute-level gaps inside that span are not measured. (Measured from the spawn and the messages, not from the transcripts.)
- Longest gap with no agent running and no commit: not measured to the minute. The known quiet stretches were the lead's own Player and build steps, each under 20 minutes.
- Rooms and rows in Builds/n71-play: eight rebuilt rooms (in four level files, the four level rows ON); the night's n71 rows ON: none.
- Cleanup commits and lines removed: ten commits; 25 files; 359 lines deleted.
