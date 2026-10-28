# NIGHT56-LOG: LEVELS FIRST (lead + three builders + batched critics)

t0: Tue, Oct 20, 2026 11:57:12 PM (the first `date` of the run). Close table no earlier than 06:47:12 (6:50); at least 7 h at 06:57:12;
hard cap 7:30 at 07:27:12 (2026-10-21).

## A0 bearings (0:00)
- HEAD e7d5b59b is 2 commits past u3-n55 (c1a1a273): 3996e2db + e7d5b59b, both the NIGHT56 forge's research / red-team docs only.
  Base: HEAD (DECIDE56 D-1).
- Disk C: 32 GB free (> 12 GB).
- Baselines (Tools/night/n56-base.sh in the detached worktree C:\Users\gaige\dev\SKYGRIND-n56base at e7d5b59b, D-2):
  n46flow n52 levels x10 seeds (Logs/n56/flow-base.txt / .out, 00:00:53-00:08:28), the bot, not a person:
  | level | stall | longest | air | kill gap med / max | dwell | dead | deaths | clear |
  |---|---|---|---|---|---|---|---|---|
  | foundry | 26.2 % | 2.8 s | 55.1 % | 2.6 / 17.9 s | 15.2 s | 18.6 s | 15 | 130.2 s |
  | iceline | 32.0 % | 7.8 s | 35.1 % | 1.5 / 182.6 s | 9.6 s | 27.6 s | 2 | 233.2 s |
  | kiln | 29.2 % | 4.3 s | 45.0 % | 1.7 / 29.0 s | 3.6 s | 6.2 s | 8 | 92.6 s |
  | span | 13.1 % | 4.0 s | 55.8 % | 1.4 / 11.4 s | 4.9 s | 6.2 s | 1 | 95.3 s |
  | spire | 18.1 % | 3.9 s | 57.8 % | 2.0 / 35.8 s | 6.9 s | 8.4 s | 0 | 141.0 s |
- THE KILN WYRM baseline (n47human --boss wyrm --level n52-kiln --seeds 40; Logs/n56/wyrm-base.txt, the bot): killed 38/40 (2 caps),
  median 122.6 s, p90 256.9 s, min 45.6 s, deaths 68 total, dealt in windows 70 %: band OUT.
- P0-pick (00:01-00:03): 13 commits picked in HANDOFF's order (D-3), one conflict resolved (D-4); 8288aec2 skipped. port/stage3
  e7d5b59b -> d893f26a. Gates on the pick (n55-gates.sh --no-editor, 00:13): config copies identical; golden 39 sets / 48094 records,
  22 replays (22 exact), 0 failing, 0 mismatches; fingerprints plain / --off54 / --off55 EQUAL.
- Shared commit (00:13-00:17, Tools/night/n56-shared.cjs): design.n56 rows rank56, tokens56 (sim, G), rim56, colour56, yellow56 (view, V),
  all off, empty numbers; FeelReel.N56.cs (a156 L / a256 G / a356 V) routed from FeelReel.N27.cs; N56SharedTests. n56-gates.sh 00:16:
  golden 0 mismatches, 22/22 exact; fingerprints plain / --off55 / --off56 EQUAL; Editor compilationFailed false, consoleErrors 0;
  N56SharedTests 2/2 in the live Editor. Tools: n56-gates.sh, n56-reels.sh (floor 6 GB), n56-prune.sh, n56-slate.cjs, n56-base.sh.
- Builders spawned one at a time (Opus 5.5, own worktrees at 6b1c00c9, `df` between: 34 -> 33 -> 33 GB): L 00:17, G 00:19, V 00:21.
  G told at 00:25: G4 fills the existing sim row design.n55.wyrm55 (D-9).

## P0: n56-r1 (00:24-)
- 00:24 Builds/n56-r1 from 6b1c00c9 (D-8: every NIGHT55 row's code, no builder work yet); the copy's config carries design.n56; drift
  EditorBuildSettings / UnityConnectSettings only (GG's files, not staged).
- 00:26-00:30 the smoke with planted focus steals (n55-smoke-steal.sh n56-r1; shipping config, every row off): steals at +67 s and +210 s
  (notepad pids 132524, 142304, each closed after 3 s by its PID); `[smoke] 230.0s DONE: all steps passed`; NRE 0
  (Logs/n55/smoke-steal-n56-r1.out, Logs/player/n55-smoke-steal-n56-r1.log). The log has no "focus lost" line (its start line: focused
  True, runInBackground True, backgroundBehavior -> IgnoreFocus for the smoke).
- 00:32 rows on in the r1 copy: n55.grade55 wick55 step55 ssao55 decor55 kill55 brief55 clear55; reels (21 sides) 00:32-.
- THE WICK bot on HEAD (Tools/night/n56-wick-bot.sh, Logs/n56/wick/, bot): skilled pilot wick55 off 40/40, median 45.5 s, p90 53.7,
  deaths 2, dealt in windows 6 %, hot-wall burns 298 of 397 hits (75 %); wick55 ON 40/40, median 57.5 s, p90 62.7, deaths 7, windows 57 %,
  hot-wall 0 of 183 (0 %). Casual pilot off 40/40 median 52.6 s, deaths 15, windows 4 %; ON 40/40 median 60.9 s, deaths 8, windows 57 %.
  (B's NIGHT55 numbers reproduced exactly.)
- a155 reel verdicts (the reel's own lines; the critic judges): THE WICK GRADE GREEN (walls 159 / 130 / 105 deg from danger; tells kept),
  HOT WALL RED on input ("burns 0 -> 0 after 8 placement(s)"); THE CROWN GRADE RED (walls at the turn 4 deg from danger; near-danger
  share 0.26-0.31 > 0.25; tells kept 395 / 493 = 0.80); FIGHT MODE RED on input both sides ("core punches 0", pilot shots 0). Sent to V
  (V4) at 00:39 as reel numbers, not a gate verdict.
- r1 reels 00:34-01:01 (Logs/feel/n56-r1/summary47.txt; NRE 0 and "0 failed" on all 23 sides): a255 STEP RED (kiln kb2c step UP 0.40 m
  crossed; camera 0.067 m on vs 0.090 m off = x0.74, want <= x0.50); a255 SSAO "NO DEFECT" in 4 rooms (OFF x0.87-1.22 of the floor noise,
  never 2x); a355 RED on input (kills 0.508-0.516 s of play apart > chainGapS 0.45: chain index stays 1; debris 10 per kill at +1.5 s,
  damage +0.5 s 9.2 %: ok); a455 GREEN 2/2 iceline (hold 0.500 s, card time + par from the plan); a555 GREEN (THUNDERCLAP fires 0 -> 6);
  decor55 accent share (n55-decor-share.py, entry frames) foundry 0.22 -> 1.77 %, iceline 0.13 -> 2.82 %, kiln 0.16 -> 1.79 %; foundry's
  accent p95 luminance 0.870 > usable cyan 0.837. a454 (NIGHT54's WICK proof) GREEN on FLARE 0 but core punches 0.
- 01:01 the r1 batched critic spawned (Research/2026-10-20-wave56/gate-r1-brief.md: 11 NIGHT55 entries, 20 images, blind pairs
  Logs/n56/pairs-r1 with the key outside).
- r1 verbs37 x5 with NIGHT55's rows on (01:03-01:09, D-12): foundry 27/27, iceline 21/21, kiln 31/31, span 31/31, spire 31/31, RED rows
  none. brief55's teach text (n55k-teach.cjs): every line ok (<= 6 words, a glyph, <= 360 frames: 16 lines, bad 0) but a card over the
  counter plate 94 frames (42.1 px) on kiln and spire: RED. Flat frames (n55-r-flat.py on the kept frames, every 50th): foundry 1, iceline
  3, kiln 1, span 0, spire 2 (7 / 180): doorDark RED.
- **Gate r1 (01:23, Research/2026-10-20-wave56/gate-r1.md, the first gate at 1 h 26 min):** P0-ssao DONE-no-defect; FAIL P0-smoke,
  P0-wickGrade (THE CROWN only), P0-hotWall, P1-wickWindow, P0-step, P0-decor (blind pairs ON 2/2 both orders; cyan not the brightest on
  3/12; foundry strips 16 deg from parry), P1-kill, P2-clear, P0-t125, P0-t99. Fixes routed (D-15).
- Editor tests 01:26: Smoke55Tests 5/5; Wick55Tests 3/4 (FAIL RowOn_EveryAttack_ExactlyAsWick54Ships: wickSlam legs 7 vs 1): to G.
- P0-smoke's steal fixed (D-17): n56-smoke-steal.sh on n56-r1 (rows off) 01:30-01:34: steals at +66 s / +204 s "took True";
  `[smoke] 53.8s n55 focus LOST`, `191.5s focus LOST`, "focus changes 4, unfocused 2078 of 68669 frames"; `[smoke] 231.0s DONE: all steps
  passed` (Logs/n56/smoke-steal-n56-r1.out, Logs/player/n56-smoke-steal-n56-r1.log). Three builder agents were running (dotnet only).

## r2 (01:36-)
- 01:36 pick: L 0e3e6024 606d2c1f b4f3b543 93644530; G 0881ea58 bda83337 54cd7acb; V 174e2b7d 145fd17d 283cdc5f bf2dbee0 7afdb2ea (12, no
  conflict). Gates 01:40 GREEN (golden 0 mismatches, 22/22; fingerprints plain / --off55 / --off56 EQUAL; Editor clean). 01:41
  Builds/n56-r2. Rows on in its copy (D-19). Reels 01:44- (Logs/feel/n56-r2); the lead's bot numbers (Tools/night/n56-lead-bot.sh,
  Logs/n56/bot-r2/) beside them.
- r2 numbers (the reels' and the meters' own lines; the critic judges): a156 GREEN x5 and agree 12 / 12 rooms (foundry / iceline / kiln);
  n56-room.py IN VIEW 16 / 18, I7 PASS x5, planted exit 0; bot n46flow (HEAD af050c9b, rows off): clear foundry 122.6 s, iceline 220.1,
  kiln 92.2, span 85.0, spire 135.1 (none worse than base); stall foundry 27.7 %, iceline 37.1 %, kiln 25.5 %, span 15.7 %, spire 17.0 %;
  deaths foundry 15 -> 21; style DULL foundry 50.5 %, iceline 74.4 %. a355 ladder ok (kills 3-6 held 0.080 s, chain 1 -> 6), 3 in 1 s ok,
  debris ok, change next frame 97.3 %; a354 with kill55 on: jitter 0.212 (off 0.191), held 0.183 s (off 0.150). a155: THE WICK GRADE GREEN,
  HOT WALL GREEN (burn S 0.935 vs walls 0.312; ride panels' cyan 0-8 %); THE CROWN GRADE RED (p2 near-danger 0.29; dS -0.094). a455
  GREEN 2/2 on foundry, kiln, span, spire (RED off). a152 THUNDERCLAP fire events 6 (want >= 4); a252 GREEN 8/8. t99: one Player run
  verbs37 + a452, 2807 frames written, 0 failed (folders 1800 + 1007, indices contiguous). verbs37 x5 27 / 21 / 31 / 31 / 31. teach: lines
  ok, the card over the counter 94 frames on kiln / spire (fix not in r2). Flat frames 6 / 180 (foundry 1, iceline 3, spire 2). V1 (V's
  meter): cover PASS x5, sky PASS, tint FAIL iceline 12/12 and span 6/6, sat FAIL 36/36 (also on the off sides).
- 02:20 the r2 batched critic spawned (gate-r2-brief.md: 7 NIGHT56 entries + 7 NIGHT55 re-gates; 28 images; blind pairs
  Logs/n56/pairs-r2-colour, pairs-r2-kill).

## r3 (02:22-)
- 02:22 pick (D-20): L 29b8d4b6 d619427d 20bb4fbb 24334c25 a9758b5e; G 0020e42f; V 02ec805a ac9dfb9e c094bde5 b856245f 1e5f8286 (no
  conflict). Gates 02:26 GREEN. 02:27 Builds/n56-r3. Wick55Tests 4/4 in the Editor (was 3/4: G's 0020e42f, the test compared the raw
  design.n37 legs). Rows on in the copy: r2's + rim56, yellow56. Reels 02:29- (Logs/feel/n56-r3).
- r3 numbers: verbs37 x5 27 / 21 / 31 / 31 / 31; flat frames 5 (iceline f0850 / f0900 / f1350, spire f1550 / f1600); identity FAIL rooms
  7 -> 4; rim56 unmoved (enemies over the world's p90 0 / 35 / 60 / 94 / 0 %); yellow foundry 2.47 %, spire v5-mid 18.78 %.
- **Gate r2 (02:56, gate-r2.md):** PASS P0-pick (passes true) and NIGHT55 P0-t125; FAIL L1 (no-foothold rooms counted), L2 (bot stall
  only: 27.7 / 37.1 / 25.5 %), G1, G2 / P1-kill, V1, V4 / P0-wickGrade / P0-hotWall, P0-decor, P0-smoke (no focus-pause test), P2-clear
  (the off side draws the card), P0-t99, P1-teachText, P1-doorDark, P0-g7g5. Blind pairs: ON picked in both orders 3 / 3 (kill, foundry,
  kiln). L4 and S4-room2 SHORT (conditional on L2). Second attempts routed to L / G / V at 03:00.
- r4 (02:49-): 12 commits picked (L b1e2be06 368514a6 4a4a2ef4; G 409fd2ac 3340436b 40196c0c a45b0fa1; V ff8a2da5 8cb6223f 4d4eb6ea
  7a4432d6 2d7c56c7); gates 02:54 GREEN; Builds/n56-r4 02:55; rows on: r3's + tokens56; reels 02:56- (Logs/feel/n56-r4); the lead's bot
  runs with rank56 / tokens56 forced on (Logs/n56/bot-r4-rank, bot-r4-tokens). iceline g7 / g5 contract 14/14 each, identity PASS
  (Logs/n56/contract-g7g5.txt).

## STOP at the usage limit (03:09, 3 h 12 min after t0)
- The usage limit cut the lead at 03:09. The three builders were stopped by the lead (their commits are on their branches:
  worktree-agent-abb2a0239ec4a05ed L, worktree-agent-a47a42bfe897e2983 G, worktree-agent-ad4ce01a176d99096 V; pick anything after
  b1e2be06..4a4a2ef4 / a45b0fa1 / 2d7c56c7). A focus-pause test for P0-smoke was drafted and reverted uncompiled (not committed).
- Not done: the r4 critic; the checkpoint freeze and tag u3-n56-r2; the freeze n56-freeze, tag u3-n56; NIGHT56-REPORT.md; HANDOFF's
  NIGHT56 block; the w56 harvest. The play build stays Builds/n55-freeze (u3-n55). At the resume (NIGHT56.md section 11): after 6:30,
  skip to the freeze; after 7:30, write the close table from what is gated.

## RESUME (04:20, 4 h 23 min after t0; the pause ran 03:09-04:20)
- The r4 reels and the lead's three bot runs had finished during the pause (all done 03:12, bot 03:18). The builders were resumed by
  SendMessage at 04:21 (D-23: last commits 05:10). No r4 critic (D-22).
- r4 numbers (the reels' and meters' own lines): a256 GREEN (rank56 on: the rank left DULL on frame 35; heal at COOL 12.00 vs 10.00 off);
  a155 FIGHT MODE RED both sides (shots 27 now, core punches 0); a155 THE WYRM GRADE GREEN, turn wall hue 22 deg (walls 5 / 10 / 4 deg from
  danger by design, D-24); verbs37 x5 27 / 21 / 31 / 31 / 31; teach card over the counter 94 -> 1 frame (2.4 px) on kiln / spire (0 px
  wanted: still RED); flat frames 5 (iceline f0850 / f0900 / f1350, spire f1550 / f1600); V1 cover x5, sky x5, tint FAIL iceline / span; V2
  enemies over p90 14 / 39 / 60 / 100 / 0 %; V3 foundry 2.45 %, spire 13.62 %.
- Bot (the lead's runs, HEAD a59e54ac, 10 seeds, n52 levels): rank56 forced on: DULL foundry 50.6 % (off 50.5), iceline 73.7 (74.7), kiln 26.0
  (25.9), span 13.2 (13.2), spire 12.0 (12.0): unmoved. tokens56 forced on: attack starts inside the sim cone foundry 85.4 % (off 62.7),
  iceline 82.4 (45.0), kiln 90.0, span 78.6; ranged fast+dash / stand foundry 46 %, iceline 0 %, span 41 %, kiln 293 % (2 dash shots in all);
  per-enemy idle over 20 %: foundry 9.3 %, iceline 21.4 %, kiln 19.9 %, span 9.8 %; deaths foundry 13 (base 15), iceline 5 (2), kiln 8 (8),
  span 0 (1), spire 0 (0); clear iceline 250.6 s (base 233.2: +7.5 %), spire 149.0 (141.0: +5.7 %). Spire TOKENS 86.7 %; all five
  levels pooled 2039 / 2418 = 84.3 %.
- 04:29 the P0-smoke focus-pause rule as a pure function + N56FocusPauseTests 3/3 in the Editor (2d38852e; dotnet gates GREEN).
- The builders' final reports: L 04:34 (17 commits), V 04:33 (23), G 04:38 (10). G4 (bot): THE KILN WYRM with wyrm55 on 38/40 -> 40/40,
  median 122.6 -> 86.8 s, p90 256.9 -> 178.3 s, deaths 68 -> 32 (deaths <= 10: SHORT by G's own run).

## r5 (04:37-, the last build of the builders' work; D-25)
- 04:37 pick: L 626dc329 25ab1748 c4b725d6 cd3999a7 2fc22326; G 7bd73b88 5990fff9; V 6cb33d16 86a6d4de 5d1f1cc1 58efff9a 99d0dc2d d25fade6
  166b0578 8cb7ea79 (no conflict). Gates 04:37 GREEN (golden 0 mismatches, 22/22; fingerprints plain / --off55 / --off56 EQUAL; Editor clean).
  04:38 Builds/n56-r5; rows on in the copy: every built row (n55 grade55 wick55 step55 decor55 kill55 brief55 clear55 wyrm55; n56 rank56
  colour56 rim56 yellow56 tokens56). 27 reel sides 04:39-; the lead's bot (Logs/n56/bot-r5: identity FAIL rooms 5) and the WYRM with wyrm55.
- Editor tests 04:41-04:43 on HEAD b6ab1f10 (Logs/n56/suite-r5.txt): N55BossGradeTests 5/5, N55KillTests 5/5, N55RoomClearTests 6/6,
  N56FocusPauseTests 3/3, N56SharedTests 2/2, N56TokensTests 2/2, Wick55Tests 4/4, Wyrm55Tests 2/2, N55SharedTests 2/2, Smoke55Tests 5/5;
  N56RankTests 2/3 (FAIL RowOff_TheHealAndTheRefund_AreTheSameAtEveryRank: "heal36's amount Expected: 10.0d But was: 0.0d"; G reported 3/3 on
  its harness; cause not checked; rank56 ships off).
- r5 reels 04:39-04:55 (27 sides, Logs/feel/n56-r5): verbs37 x5 27 / 21 / 31 / 31 / 31 with every built row on; a156 x5; a355 RED (kills 3+
  held 5 frames = 0.083 s; marks NO); a354 jitter 0.230. The lead's bot on b6ab1f10: identity FAIL rooms 5; iceline stall 36.5 %; THE WYRM
  with wyrm55 40/40, median 86.8 s, p90 178.3 s, deaths 32 (Logs/n56/wyrm-r5-on.txt). The steal smoke on n56-r5 (no builder running) 04:56-05:00:
  both steals "took True", `53.2s n55 focus LOST`, DONE all steps passed (232.5 s).
- **The final gate (two parallel critics, D-25): gate-r5a.md (levels, 05:17) PASS L1 and NIGHT55 P2-clear; FAIL L2, L3, S1-doors /
  P1-doorDark, P0-g7g5, P0-mirrors. gate-r5b.md (play / colour / bosses, 05:22) FAIL G1-G4, V1-V4, P0-decor, P0-wickGrade, P0-hotWall,
  P1-wyrmTurn, P1-kill, P1-wickWindow, P1-teachText, P0-smoke.** g8's THE ASH PIT: the critic's GG line "a random lava pit"; the revert
  conflicted in the generated level file and was aborted (D-28: GG's call).

## FREEZE (05:23-05:37)
- 05:23 design.n55.clear55 ON (Tools/night/n56-rows-on.cjs, Logs/n56/freeze-rows.json; D-29); gates 05:24 GREEN (golden 0 mismatches,
  22/22 exact; fingerprints plain / --off55 / --off56 EQUAL; Editor compilationFailed false, consoleErrors 0); N55SharedTests 2/2,
  N56SharedTests 2/2, N55RoomClearTests 6/6 (Logs/n56/suite-freeze.txt); commit 04bef918.
- 05:25 Builds/n56-freeze, its OWN build from 04bef918; its config copy = the repo's (cmp); ON in it: n55.clear55 only.
- 05:27-05:31 verbs37 x5 on n56-freeze: foundry 27/27, iceline 21/21, kiln 31/31, span 31/31, spire 31/31, RED rows none, NRE 0 on every side;
  row 42 heal by aggression PASS ("heal36 1, health 50 -> 61") with rank56 off (D-26 settled).
- 05:32-05:36 the smoke on n56-freeze after 61 s of quiet, no agent running ("idle 1986 s"): `[smoke] 227.8s DONE: all steps passed`, NRE 0,
  "focus changes 0" (Logs/n56/smoke-freeze.out, Logs/player/n56-smoke-freeze.log).
- 05:37 tag **u3-n56** -> 04bef918.
- 05:44- the full EditMode suite in the live Editor in batches (Tools/night/n41-suite.sh, Logs/n41-suite-n56-freeze.txt; beyond the freeze's
  required batches): Skygrind.Core.Tests A-Z every batch passed, 0 failed; Skygrind.Game.Tests A-L passed, 0 failed; M and N returned no JSON
  twice each (too large for one 600 s call; the same "no JSON" for M in an earlier night's run, Logs/n41-suite-freeze2.txt): not counted;
  O-Z still running at the close table (see the last commit's message).

## CLOSE TABLE (written Wed, Oct 21, 2026 6:47:30 AM, 6 h 50 min after t0)

Play build `Builds/n56-freeze` (tag u3-n56 = 04bef918). Rows ON: design.n55.clear55 only (D-29). A row is switched off in a build's copy with
`node Tools/night/n46-build-rows.cjs <tag> off <night.row>`, or the whole night with `-skygrindOff56` (NIGHT55's game) / `-skygrindOff55`
(NIGHT54's). Level data (rooms) has no switch: the comparison side is Builds/n55-freeze (u3-n55). Bot numbers are the walk / boss pilot's.

### NIGHT56 slate (Logs/n56/slate.json)
| entry | state | before -> after (measured) | frames / log | row | off |
|---|---|---|---|---|---|
| P0-pick | **DONE, passes true** (gate-r2) | 13 NIGHT55 commits picked, 1 conflict, gates GREEN, n56-r1 built 00:24 | NIGHT56-LOG A0 / P0 | - | - |
| P0-gate55 | SHORT (the process ran; no critic passes a process) | NIGHT55 rows gated r1 / r2 / r5: clear55 PASS (ON), P0-t125 PASS, P0-ssao DONE-no-defect, the rest SHORT | gate-r1.md, gate-r2.md, gate-r5a/b.md | design.n55.* | - |
| L1 | **DONE, passes true** (gate-r5a) | no room checker -> n56-room.py (R2 / R5 / R8 / in view; planted 7/7 FAIL) | Logs/n56/bot-r5/room-table.txt | - (a tool) | - |
| L2 | SHORT (2 attempts) | in view 1 / 18 -> 16 / 18 (r2), now 15 / 18 entry-eye, 4 / 18 strict; a156 agree 12 / 12 (r2) -> 3 / 12 strict; (bot) stall foundry 26.2 -> 27.7, iceline 32.0 -> 36.5, kiln 29.2 -> 25.5 %; clears all faster | Logs/feel/n56-r5/l5?/a156; Logs/n56/bot-r5/flow.out | level data (ships, D-27) | Builds/n55-freeze |
| L3 | SHORT | identity FAIL rooms 7 -> 5 (p5, g8 fixed; a3, g3, g4, v2, kb2 left); g8 (bot) deaths 0 -> 7 | Logs/n56/bot-r5/identity.txt; l5i g8 / l5s p5 entries | level data (ships; g8 GG's call, D-28) | Builds/n55-freeze |
| L4 | SHORT | not started: conditional on L2 (gate-r2 L2 FAIL) | - | - | - |
| G1 | SHORT | (bot) DULL share 50.5 -> 50.6 % (30 points wanted); a256 heal at COOL 12 vs 10 | Logs/n56/bot-r4-rank/flow.out; n56-r4 g1on | design.n56.rank56 (OFF) | already off |
| G2 | SHORT (2 attempts) | kills 3+ held 0.050 -> 0.083 s, chain 1 -> 6; marks 0.04 % (1 % the reel's line); a354 jitter 0.191 -> 0.230 | Logs/feel/n56-r5/k5on/a355 | design.n55.kill55 (OFF) | already off |
| G3 | SHORT | (bot) attacks inside the sim cone 44.6 -> 84.3 % pooled (span 78.6); idle over 20 % iceline 21.4 % | Logs/n56/bot-r4-tokens/flow.out | design.n56.tokens56 (OFF) | already off |
| G4 | SHORT | (bot) THE KILN WYRM 38/40 -> 40/40, median 122.6 -> 86.8 s, p90 256.9 -> 178.3 s, deaths 68 -> 32 (<= 10) | Logs/n56/wyrm-base.txt, wyrm-r5-on.txt | design.n55.wyrm55 (OFF) | already off |
| V1 | SHORT (3 rounds) | key / accent cover PASS x5, sky PASS, tint fixed; blind pairs ON 2 / 2 both orders; world over the tells 28 / 36 frames | Logs/feel/n56-r5/v5?/a356; Logs/n56/pairs-r2-colour | design.n56.colour56 (OFF) | already off |
| V2 | SHORT | enemies above the world's p90: 1 of 30 (90 % wanted) | n56-r5 v5? a356 | design.n56.rim56 (OFF) | already off |
| V3 | SHORT | non-parry yellow on spire up to 23.9 %, 28 / 36 frames over 0.5 % | n56-r5 vp verbs37 | design.n56.yellow56 (OFF) | already off |
| V4 | SHORT (2 attempts) | THE WICK grade + hot wall pass (burn S 0.22 -> 0.935); THE CROWN tells 0.68 (0.8); ride panels 0 % cyan; THE WYRM turn 22 deg, walls 4 deg p1 -> p2 | n56-r5 b1on / b2on / b3on a155 | design.n55.grade55 (OFF) | already off |
| S1-doors | SHORT | flat verbs37 frames 7 -> 4; spire door f1550 dark | Logs/feel/n56-r5/v?/verbs37 | level data (lamps ship) | Builds/n55-freeze |
| S2-p1s | SHORT | not started (behind every P0 / L / G / V entry; the 71-min pause) | - | - | - |
| S3-finish | SHORT | not started (same) | - | - | - |
| S4-room2 | SHORT | only if L4 passed | - | - | - |

### NIGHT55 slate (Logs/n55/slate.json, field n56 = tonight's result)
| entry | tonight | row |
|---|---|---|
| P2-clear | **PASS** (gate-r5a): a455 GREEN x5 levels, off holds 0 / cards 0 | design.n55.clear55 **ON** |
| P0-t125 | **PASS** (gate-r2): THUNDERCLAP fires 6 of 6 taps | - (reel code) |
| P0-ssao | DONE-no-defect (gate-r1): OFF <= 1.22x the floor noise | ssao55 OFF |
| P0-smoke | SHORT (gate-r5b): DONE with steals that take focus + N56FocusPauseTests; the Player pause outside the smoke not shown | - |
| P0-wickGrade, P0-hotWall, P1-wyrmTurn | SHORT (gate-r5b): THE WICK passes; THE CROWN 0.68; ride panels 0 % cyan; THE WYRM 4 deg p1 -> p2 | grade55 OFF |
| P1-kill | SHORT (= G2) | kill55 OFF |
| P1-wickWindow | SHORT (gate-r5b): the bot passes; the Player pilot 0 core punches | wick55 OFF |
| P0-wyrm | SHORT (= G4): deaths 32 | wyrm55 OFF |
| P1-teachText | SHORT (gate-r5b): 94 frames / 42.1 px -> 1 frame / 2.4 px over the counter | brief55 OFF |
| P0-decor | SHORT (gate-r2; folded into V1, D-18) | decor55 OFF |
| P0-step | SHORT: x0.74 (want <= x0.50), not re-tried | step55 OFF |
| P0-mirrors, P0-g7g5, P1-doorDark | SHORT (gate-r5a): identity 5 rooms; g5's piece not in its frame (contract 14 / 14); 4 flat frames | level data |
| P0-t99 | SHORT (gate-r2): 2807 frames 0 failed; no phase can pass f1799 | - |
| P1-bossText, P1-fov, P1-fixtures, P1-edges, P2-finish, P2-brief, P2-speed | SHORT: not started | off |
| P1-ride | unchanged: SHORT by decision (NIGHT55 D-8) | ride55 off |
| P1-guns48 | unchanged: DONE at NIGHT55 (both rows off) | off |

### The forge experiment (NIGHT56.md 8)
Entries gated / attempted: 13 / 14 (NIGHT56); 17 of NIGHT55's 26 judged by a critic tonight. The first gate: 01:23 (1 h 26 min) on 11 entries
(NIGHT55: 1 of 26; 1 h 10 min). The usage limit still stopped everyone, at 3 h 12 min (NIGHT55: 2 h 28 min); two gates had landed before it.

### Close
NIGHT56-REPORT.md (for GG), HANDOFF.md (the NIGHT56 block), DECIDE56.md D-1..D-30 (untracked), docs/memory (CORE rebuilt; LESSONS L-098..L-102;
FEATURES; THREADS T-134..T-138 at its 60-line cap; IDEAS: M-1, X-1, X-2, E-2; ledger/wave-55.md, wave-56.md). Nothing pushed, nothing merged.
- 06:57:42 (7 h 00 min 30 s after t0): the full suite so far (Logs/n41-suite-n56-freeze.txt): the counted batches passed 1735 of 1735, 0 failed
  (Core A-Z, Game A-L); Game M and N "no JSON" twice each (not counted); Game P retrying, the rest still running in the live Editor (its
  totals are not part of this close). The freeze's own required batches (tonight's touched classes) passed at 04:43 / 05:24 except
  N56RankTests 2 / 3 (rank56 ships off).
