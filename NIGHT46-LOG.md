# NIGHT46 log: the flow night (NIGHT46.md; DECIDE46.md)

t0 = 22:56:35 (`date`, Sat Oct 17 2026). Hard cap 04:41:35. Builds ~00:46, ~02:01, ~03:11; freeze ~04:01.
GG's words since the forge: none (he will not test; DECIDE46 stands). Disk C: 14 GB free at t0. Editor: playMode stopped.

## G0
- 22:58 rows-switch-on: --dry 30 / 19, applied 30 / 19. 22:58 n46-shared.cjs: 32 files; configs cmp equal; 0 duplicate keys.
- 22:59 gates on the combined tree: dn-check 0; golden 0 failing, 0 mismatches, 22 / 22 exact; fingerprint EQUAL plain / --off45 /
  --off46; n45rows GREEN (0 failing); campaign --tape PASS; Campaign24Tests 12 / 12. 22:59:54 forced import of the 8 new assets, Editor
  compile: compilationFailed false, consoleErrors 0.
- 23:01 commits: e2587143 "NIGHT46 rows switch-on" (19 files), ac789358 "NIGHT46 shared commit" (33 incl. the script), a8506416 the
  forge research. (Committed before the EditMode batches so builders branch from it; the batches run next.)
- 23:03 builders launched from a8506416: F1, A1, A2, A3, B1, V1. integrate.sh map set (Logs/night46-picked.txt); n46-gates.sh (+ --off46),
  n46-reels.sh, n46-rows-on.cjs made from the n45 copies.
- 23:02-23:04 EditMode: N46Shared 4 / 4, NightRows 14 / 14, Lesson25 16 / 16, Campaign24Tests 12 / 12 (MapChoice, N3, N4 running).
- 23:04-23:14 MapChoice: no result (the run_tests 600 s timeout, COMMAND_FAILED; 5 new map cases): re-run inside P0's MapC batch.
- 23:17 A3 CARDS READY 717fb669 (F-1..F-4 a3 / b1 / a8 / b2; I-1..I-3 g4 / g8 / g3 THE TIPPLE). PASS 23:17 with rulings: b1 keeps
  THE SPILL's crane rails; wave 1's first ranged bodies go where the spawn sees them (>= 2 with line of sight; A3's los.cjs: 0 / 10
  today); g3's air line broken by a one-way drop is fine.
- 23:18 A1 CARDS READY 4e725676: p1 THE HOOK YARD (walled, ~860 m2, the CABLE DRUM loop), p3 THE TIE-OFF (bridge, ~940 m2: two
  11 m strafe decks; void band 11 m past the door; falls 10 HP), p5 THE SPOOL HALL (islands, ~890 m2, the GREAT SPOOL); fallReturn43
  lips on every void incl. links p2 / p4 / o1 / o2. PASS 23:18: the pull reach measured before the far edges are built.
- 23:19 A2 CARDS READY 8e9cd32f (+ 49fbbc59 the guarded plan copy): v2 THE NAVE -> THE FALLEN BELL (nave; fight floor ~450 m2,
  the four pillars cut, a one-way 3.6 m drop in), v6 THE CHOIR -> THE ROPE CHOIR (islands; 1412 -> ~820 m2; wave 1 on the island, 6 / 6
  nodes in view vs 0 / 5), belfries 4 -> 3 gates each with its own job. PASS 23:19: v6 keeps a door-level entry; v2's 18 m jump-dash
  stays optional (the walk proves it).
- 23:08 d19963ee CLAUDE.md rule 7 / rule 8 lines (DECIDE46: DECIDE44 D-4).

## The card gate
- 23:12 B1 CARDS READY 9f68336e: walk-in option A (k5 rebuilt at the arena's 60 x 60 in n46-kiln, the worm's layer moved onto k5, d8's
  gap cut; Boss36WalkIn holds, speed kept); wyrm46 trials T1-T4 (stagger 4 -> 6 s, open 1.25 -> 2 s; aggro / window; pool x0.5 only if
  needed); east ride panels into kb1b / kb2b / kb3b, kb2b's second cliff cut. PASS 23:12 (boss arena: the floor's reason stands).

## Builders
- 23:15 V1 round 1 (4d3c4aad, Research/.../V1.md): every verb changes the screen on its press frame; shake already trauma-squared rotation
  only, FOV kicks and the landing dip exist. Measured (`n46feel unseen`, 300 s autopilot): 14 / 21 hurts from outside the view on the
  Survival arena (7 from behind), 5 / 10 on DRYDOCK, no warning before. Gaps: G1 no off-screen wind-up warning, G2 a non-kill hit moves
  no camera, G3 a body kill gets no punch / kick, G4 the THUNDERCLAP's shot shakes like its kill (0.42), G5 two full-screen effects
  (parry flash, hurt static: GG w30). Sim latencies (swap 15 steps, presses in a freeze) listed for next night. Round 2 sent: G1-G5.
- 23:38 F1 round 1 (a02696d1): `n46flow` + baseline (Research/2026-10-17-wave46/F1.md, F1-baseline.txt). Seeds 1-10, the ON game:
  L1 walks 8/10, stall 31.8 %, longest 6.1 s, dwell 15.9 s, dead 54.9 s, 25 deaths, clear 203 s; L2 10/10, 37.1 %, 7.2, 12.7, 36.1, 0
  (137 fall returns), 186 s; L3 3/10, 38.8 %, 11.2, 12.3, 26.0, 5, 214 s (kb3's burrower stuck at y -6 in 5 seeds holds the door); L4
  10/10, 17.4 %, 3.7, 6.8, 15.6, 1, 128 s; L5 10/10, 12.4 %, 4.3, 8.7, 11.4, 0, 190 s. Band (seeds 1-10 vs 11-20): stall 6.8, longest
  4.8, dwell 4.1, dead 3.3, deaths 4, clear 20.2. Bosses not measured by this pilot.
- 23:39 A2 round 1 (d272ec0c v2 THE FALLEN BELL, 12b207d7 v6 THE ROPE CHOIR, 1186bf76 gates 4 -> 3, 7476b1a3 reel a246): levelgen 0
  new errors (n41-spire's own 1); gates GREEN 3 / 3; wave 1 in view v2 5 / 5 from the lip, v6 6 / 6; n46spire walk 10 seeds: v2
  69.0 -> 75.1 s (0 deaths vs 1), v6 115.0 -> 89.0 s, level 208.8 -> 206.4 s; v5 14.8 s (SHORT of ~10 s). The 3.6 m lip built 2.4 m
  (enemies step 0.4 m).
- 23:35 V1 round 2 (d58e96b6 feel46 G1-G5, ee29d28e / 7b9d25f0 reel a546): the off-screen wind-up warned 14 / 14 (Survival), 5 / 5
  (DRYDOCK); with the warning's range 0, 0 / 14 and 0 / 5 (the check catches a failure). fingerprint EQUAL x3.
- 23:34-23:39 the Editor stopped answering (the G0 batches MapChoice / N3 / N4 timed out at 600 s): 0 CPU, the machine idle 43 min (the
  unfocused stall, memory unfocused-autotick). 23:39:42 woken by a foreground moment (AttachThreadInput); since then it runs at ~1.2
  cores but no Pipeline command answers (set_autotick, status time out): read as the queued test requests running. Polling.
- 23:42 picked F1 (ed3a422d, cefa3a65), A2 (f60b5789 .. 94422712), V1 (05c77501 .. 7b9d25f0). Gates: 0 duplicate keys, configs cmp,
  dn-check 0, golden 0 / 22, fingerprint EQUAL plain / --off45 / --off46. The meter sent to A1 / A3 / B1 (copy for runs only).
- 23:55 F1 round 2 (a97319c3 fd93f6ac 781e3b08 b06cdf20; all OFF): seeds 1-10 vs the baseline: retry46 foundry walks 8 -> 10 / 10,
  clear 203 -> 167 s; overlap46 dead time down past the band on every level, kiln walks 3 -> 7 / 10, spire 190 -> 161 s, BUT g7 deaths
  0 -> 8 (THE THAW LOCK's wave 3 enters by the door); teach46 a2 dead 11.3 -> 8.2 s; stuck46 (new: ends a burrower stuck 6 s) kiln
  3 -> 8 / 10, kb3 holds 5 -> 0; reveal46 1.99 -> 1.00 s on a9 / o3 / v11. Four sim rows together: every level 10 / 10, kiln 214 -> 119 s,
  foundry 203 -> 138 s, g7 4 deaths. stuck46 approved; overlap46 sent back: no overlap when the next wave enters at the door.
- 00:01 B1 round 1 (fe5736dd .. 406ef27a; all OFF; kiln46 + walkin46 + wyrm46 go ON together): walked in through d8 at 8.00 -> 8.00 m/s
  (largest step 0.07 m, no teleport); wyrm46 (stagger 8 s, open 2.5x, pool x0.9) walked in, 20 seeds: median 38.3 s (27.1-55.7), killed
  20 / 20, 0 deaths, damage taken > 0 on 20 / 20 (median 28), staggered share 63 %, crumble 20 / 20 (today 185.5 s, 7 / 10 killed). Same
  numbers with the teleport: 104.7 s (RED). kb3: no burrower (straight-line head vs the pour), kb1b / kb2b / kb3b ride run-ups, kb2b's
  second cliff cut. Level 3 meter n45 -> n46: walks 3 -> 9 / 10, clear 213.9 -> 155.5 s, dwell 12.3 -> 5.7, dead 26.0 -> 15.6, deaths
  5 -> 9 (more seeds reach kb3). Hand-off (view): the ring draws only while the worm fights -> sent to V1.
- 00:03 A3 round 1 (8aadcb7f, da146e04, d68752f1, 09da58a4, f394c30c): L1 stall 31.8 -> 30.0 %, longest 6.1 -> 4.2 s, dead 54.9 -> 51.4 s;
  a3 loop n -> y, longest 6.1 -> 3.8 s; b1 wave 1 in view 0 / 6 -> 4 / 4, dead 13.2 -> 9.4 s, clear 104.7 -> 85.6 s; a8 dead 9.8 -> 0.7 s;
  L2 inside the band (g3 THE TIPPLE loop y, wave 1 4 / 4 in view; g8 2 / 2). b2's fight unproven (the pilot never crosses gate 2).
- 00:02 A2 round 2 (a4707844, 7330c3ce; eb34193f inside): v5 13.3 -> 8.7 / 10.0 s (target met); v6 clear 124 -> 87 s, level 190 -> 156 s
  (past the band); v2's stall target SHORT (in band); a crater round the bell tried and cut (slower).
- 00:03 disk 9.2 GB (six worktrees): DECIDE46 D-1: deleted the numbered reel frames (f*.jpg) of 145 folders of nights <= n42 (652,227
  files; summaries and shot PNGs kept): 212 GB free at 00:03:44.
- 00:04-00:06 picked B1 (1 ROOMS.md conflict: both sides kept), A3 (1 ROOMS.md conflict: Tools/night/n46-rooms-resolve.sh), A2 r2, F1 r2
  (fa83a29f stuck46, ae25df81 reveal46, 45488d5b). Gates at 45488d5b: 0 duplicate keys, cmp, dn-check 0, golden 0 / 22, fingerprint EQUAL
  plain / --off45 / --off46, n45rows GREEN.
- 00:08 picked V1 round 3 (59a44d7c: the worm's arena draws from k5's door while Wyrm46ArenaHere); dn-check 0, fingerprint EQUAL.
- 00:07 the live Editor still answers nothing (polls 23:46-00:06 all time out): build r1 in the batch copy (memory batch-build-and-editor-
  stall; Unity -batchmode BatchBuild24 at 45488d5b).

## Build r1 (batch copy, 45488d5b)
(This section was first appended at 00:14 with a shell heredoc, against CLAUDE.md rule 9; it was re-written with the Edit tool at the
close, 02:13, with the same facts.)
- 00:09:47 `[batch24] build Succeeded: 0 errors, 519 warnings, 334 MB` -> Builds/n46-r1 (42.8 s). Rows ON in its copy (n39-build-rows):
  foundry46 iceline46 kiln46 spire46 walkin46 wyrm46 retry46 overlap46 teach46 stuck46 reveal46 (span46 not built yet; feel46 off: its
  reel pairs it).
- 00:10 the live Editor answers again (status playMode stopped); set_autotick enabled.
- Reels (n46-reels.sh, Logs/feel/n46-r1/summary45.txt; devices Keyboard, Mouse, Xencelabs: no pad listed; 0 NRE in all five):
  - a446 n46-kiln GREEN: walked in, BossWalkIn True, Wyrm46ArenaHere True, speed 8.00 -> 8.00 m/s, largest frame move 0.13 m, the
    worm's centre (0.0, -327.5), crumble fallen 2 (the reel's cheat). Looked at: door-kilnmouth-cone shows d8's lit door, not the arena
    (V1's ring fix is in r2); mid shows the worm, KILN WYRM bar LEG 1/3, "PUNCH THE HEAD TO STAGGER IT"; level 1's "STEP 1" card still up
    (the reel's teleport: T-76's kind).
  - a246 n46-spire RED (frames 12 of 14; bodies all placed). Looked at v2-lip: the room reads, the bell does not read as a bell (a dark
    box with the MEND sign); v6-mid-0 reads. Sent to A2.
  - a346 n46-foundry GREEN (5/5, b1 3/3 husks on the rim, b2 3/3); b1-entrance's title reads "1.3 RIDE" (asked A3 to check the camera).
  - a346 n46-iceline RED (frames 8/9; bodies 12/12, g3 swarm 8/8). g3-entrance reads (three husks in view, the card, the hopper).
  - a546 RED: "NO HUSK PLACED" for hit / kill / aim41, feel46's probe 0 on both passes (feel46 not on in the on pass), ran in SURVIVAL;
    9 frames failed. Sent to V1.
- 00:15 P0: the whole suite started in the live Editor (00:14 the first call ran 0 batches: n42-suite needs `<tag> Core|Game`).
- 00:20:51 Core: 1127 / 1129 (Logs/n42-suite-n46c.txt). FAIL N46SharedTests.EveryRowOff_IsNight45sOrder and
  TheOffSwitch_LeavesNight45sOrder (4 / 4 alone at G0): Lesson25Tests.cs:93-99 sets N38 / N40-N43 ForceOff with no restore and the L
  batch runs before N. Fix written (Tools/night/n46-fix-sharedtests.cjs: Switches() saves / clears / restores them), applied after the
  Game batches.
- 00:16-00:27 builders: A1 round 1 (4e725676, d6c22481, 23477d8e; n46-span p1 THE HOOK YARD / p3 THE TIE-OFF / p5 THE SPOOL HALL, lip
  returns on p2 / p4 / o1 / o2): reach 9 / 9 (real Mover, every landing >= 2.3 m inside); walk 10 / 10; n46flow vs n40-span: stall
  15.7 vs 17.4 %, dead 15.6 vs 15.6, clear 146.4 vs 128.0 s (in band), deaths 4 vs 1, dwell 16.4 vs 6.8 s (SHORT, past the band: round
  2). V1 a5ef767f (a546 fixed: feel46 was on in r1's on pass; the behind wind-up id; husk placement 8/6/5/4/3 m; hud33 hides the parry
  flash in today's game). F1 1cb79880 (reel a646, the flow rows' frames), b738899a (g7: entryM 12 + growthMax 2), 37699cee (the
  candidate flow table + Tools/night/n46-flow-table.sh). A2 87d47351 (the bell on its side, mouth to the lip, v2-bell-lamp under it).
- 00:27 F1's candidate table (port/stage3 at 59a44d7c, n46 levels + retry46 / overlap46 / teach46 / stuck46 / walkin46 / wyrm46, seeds
  1-20, mean of the two sets): L1 walks 16 -> 20 / 20, dead 53.3 -> 30.2 s, clear 211 -> 138 s; L2 dwell 14.8 -> 10.5, dead 36.1 -> 30.3;
  L3 walks 7 -> 19 / 20, dwell 14.2 -> 6.2, dead 26.1 -> 12.1, clear 219 -> 123 s; L4 (still n40-span's copy) dead 15.6 -> 10.6; L5
  clear 191 -> 160 s. No metric worse past the band on any level. With b738899a: L2 deaths 4.5 -> 1.5.
- 00:37:13 Game: 1046 / 1046 (Logs/n42-suite-n46g.txt). **P0: Core 1127 / 1129 + Game 1046 / 1046; the 2 Core FAILs were the test's
  leak, fixed: 9435a6c7; Core L then N re-run 00:44: 59 / 59; N46Shared alone 4 / 4.**
- 00:37-00:40 picked F1 (2989faa5 g7 holds, 69d45ac9 reel a646, 96189d6d flow table), V1 (c6a93395), A2 (920c6a33 the bell), A3
  (0289c5c2, 000335ac, 6ac2ffd8, ff9eca50), A1 (cards: 1 ROOMS.md conflict resolved by the script; a2c7508e, 6acc23ca, 9715d7bf r2:
  dwell 16.4 -> 8.7 s, in the band; n46-span vs n40-span: stall 13.2 vs 17.4 %, dead 11.2 vs 15.6, clear 126.7 vs 128.0, deaths 2 vs 1).
  Gates at 9715d7bf: 0 duplicate keys, cmp, dn-check 0, golden 0 / 22, fingerprint EQUAL plain / --off45 / --off46, n45rows GREEN, Editor
  compile 0 errors.

## Build r2 (batch copy, 9715d7bf)
- 00:42:44 `[batch24] build Succeeded: 0 errors, 518 warnings, 334 MB` (40.7 s) -> Builds/n46-r2. Rows ON in its copy: every n46 level
  row (span46 now built) + walkin46 wyrm46 retry46 overlap46 teach46 stuck46 reveal46. Reels: a446 a246 a346 x2 a146 a546 a646 x5.
- 00:42-00:49 r2 reels (Logs/feel/n46-r2/summary45.txt; 0 NRE, 0 exceptions in all 11): a446 GREEN (8.00 -> 8.00 m/s, no teleport);
  a246 GREEN 12 / 12; a346 n46-foundry GREEN 5 / 5 (rooms named: b1-entrance in b1); a346 n46-iceline GREEN 8 / 8; a146 RED 4 / 8
  (p3-late not saved, p5 not placed); a546 GREEN 18 / 18 moments, 28 / 28 frames, fixes ran on the on pass; a646 GREEN on all five
  levels (rows read True; b1-restart, g7 wave 2 held by growthMax, kb2 wave 2 released with 3 standing, o3 / v11 / a9 reveal frames).
- Looked at: a446 door: the arena's lit lines now show through d8 (V1's ring fix), small, from REST; v2-lip: the bell still reads as a
  box (a dark face rectangle); p3-entrance: a big block in the middle, no enemy, no landmark in view; p1-mid reads.
- 00:55 blind pairs (Tools/night/n46-blind-pairs.cjs, seed 4646, six feel46 off / on pairs; a reviewer agent + the lead, labels hidden):
  reviewer picked ON in 4 / 6 (behind-050, hit-000, kill-150, hurt-100), "same" in 2 (kill-000, pumpkill-050), OFF in 0; the lead (blind
  on 3 pairs) ON in 3 / 3 (behind-050, kill-000, hurt-100). Both ON: G1 (the off-screen warning) and G5 (the hurt static at the edges);
  the kicks (G2-G4) do not meet N-4's rule. V1: split feel46 (`kicks: false`). The off pass showed "GAMEPAD CONNECTED" mid-reel (T-73).
- 00:53 B1 round 2 (2671f495, b6deece4): retryKeep 1 + aggroSpeedP3 0.8: walked in, 40 seeds: median 38.3 s, max 300 -> 85.1 s, most
  deaths in a fight 8 -> 1, killed 40 / 40, damage taken > 0 40 / 40, crumble 40 / 40; run-ups ride 0.73-0.90 s into each cliff.
- 00:56-00:58 verbs37 on r2 (rows on; -skygrindMap n46-*): L1 26 / 26, L2 20 / 20, L3 25 / 25, L4 30 / 30, **L5 27 / 30 RED** (rows 9
  wall ride, 11 jump-off boost, 13 slide bounce: "WallRideStart 0 ... speed 0.00 -> 0.00 m/s" on the reel's picked 30 m wall; n41-spire
  was 30 / 30). 0 NRE. Sent to A2 (fix in the level's data, never the reel).
- 00:59 picked B1 r2 / r3 (1c1934e9, c7d810e2, 210bc199), A2 r4 (74c5c068 the bell as a stepped stack), V1 r4 (84b5f8c2 feel46 kicks
  false, the bottom chevron lifted). Gates at 84b5f8c2: 0 duplicate keys, cmp, dn-check 0, golden 0 / 22, fingerprint EQUAL x3,
  n45rows GREEN.
- 01:04 the flow table at 84b5f8c2 (bash Tools/night/n46-flow-table.sh Logs/feel/n46-flow/r3pre; baseline = n45 levels --off46,
  candidate = n46 levels + retry46 overlap46 teach46 stuck46 walkin46 wyrm46; seeds 1-20): L1 walks 16 -> 20 ✓, dead 53.3 -> 30.2 ✓,
  clear 210.6 -> 137.9 ✓; L2 dead 36.1 -> 32.4 ✓, the rest in band; L3 walks 7 -> 19 ✓, dwell 14.2 -> 6.2 ✓, dead 26.1 -> 12.1 ✓,
  clear 218.5 -> 123 ✓; L4 walks 20 -> 19 ✗ (seed 17 capped in p3: two spitters on a 4 m perch, hp 1), dead 15.6 -> 6.2 ✓, clear
  127 -> 103.6 ✓; L5 clear 191 -> 152.1 ✓, the rest in band. Sent L4's cap to A1.
- 01:09 picked A2 e2a950b2 (verbs37 fix by data: v5's noride liners back; v6's ride panel flush; the reel's wall pick moves from a wall
  whose start sat inside v5's wall box to v6's designed ride wall; headless rows 9 / 10 / 11 / 13 PASS). 01:12 picked A1 8677348e (r3:
  the reel's waits; p3's clamp off the door axis, the KNOT given a mass; anchorages 2.5 m: n46-span seeds 1-20 walks 20 / 20).
  Gates at 8677348e: 0 duplicate keys, cmp, dn-check 0, golden 0 / 22, fingerprint EQUAL x3, n45rows GREEN.

## Build r3, the candidate (batch copy, 8677348e)
- 01:15:26 `[batch24] build Succeeded: 0 errors, 518 warnings, 334 MB` (26.8 s) -> Builds/n46-r3. Rows ON in its copy: every n46 level
  row + walkin46 wyrm46 retry46 overlap46 teach46 stuck46 reveal46 feel46. Reels a446 a246 a346 x2 a146 a546 a646 x5 + verbs37 x5.
- 01:16-01:26 those reels all GREEN (a146 8 / 8 now) but verbs37 L1 20 / 26, L2 16 / 20, L3 21 / 25 RED on the grapple rows ("the grapple
  is NOT owned" with "grapple owned True"), L4 30 / 30, L5 30 / 30 (A2's fix holds). 01:28 cause: **the rows were never on in r3's copy**:
  n39-build-rows.cjs threw on n46.feel46 (its regex stops at feel46's nested "numbers" object) and wrote nothing; the lead printed only
  its last line. With foundry46 off, n46-foundry is not in the campaign order, so grapple35LevelPlace = 0 and the grapple counts as here
  (SurvivalSim.Move35.cs:70-76): a test-setup error, not a game regression (r2's copy did apply its rows: L1-L4 PASS there). Fix:
  Tools/night/n46-build-rows.cjs (parses the JSON); rows ON in r3's copy at 01:30; every r3 reel + verbs37 re-run (the first set kept in
  Logs/feel/n46-r3/summary45-rowsoff.txt).
- 01:29-01:39 the re-run with the rows ON: a446 / a246 / a346 x2 / a146 / a546 GREEN (a146 8 / 8: p1 / p3 / p5 entrances with wave 1 in
  view); verbs37 L1 26 / 26, L2 20 / 20, L4 30 / 30, L5 30 / 30; L3's Player was closed by the lead at 01:35 (taken for a leftover: it was
  the loop's own v3 Player; the loop's waiting shell had been stopped for low memory: another session's jest run held ~15 GB); v3 re-run
  alone at 01:40: 25 / 25. 0 NRE everywhere.
- 01:41 looked at r3: p3-entrance (wave 1 4 in view, the KNOT lit above), p5-entrance (the spool platforms with wave 1), v2-lip (the
  stepped bell, the MEND on its shoulder, wave 1 in view), a446 lip (the arena seen through d8's boss door); greyscale sheet of p3 / p5 /
  v6 / g3 entrances: every enemy reads against the room.

## The freeze
- 01:45 `node Tools/night/n46-rows-on.cjs` (reasons: the frames and numbers above): ON foundry46 iceline46 kiln46 span46 spire46 walkin46
  wyrm46 retry46 overlap46 teach46 stuck46 reveal46 feel46; shift46 / flow46 off. RowsBaseline.OnPathThrough 45 -> 46: fingerprint
  EQUAL plain / --off45 / --off46 (no re-record). n45rows' wiring pin RED ("rows every NIGHT46+ sim row off" vs the now-ON defaults: the
  pin compared Init's AllOff with the defaults) -> fixed to AllOff: GREEN. dn-check 0, golden 0 / 22, campaign --tape PASS, Editor 0
  errors, Core L+N 59 / 59, Game N3+N4 41 / 41. Commit 71905b12.
- 01:47 the frozen config copied into Builds/n46-r3 (cmp equal; the five n46 levels equal; the code differs from r3's 8677348e only in a
  fingerprint-only constant and comments): **n46-r3 = the frozen game**.
- 01:47-01:51 the smoke on n46-r3 (devices: no pad listed by name; "HID-compliant system multi-axis controller"): `[smoke] 228.0s DONE:
  all steps passed`, 0 NullReference, 0 exceptions (Logs/player/n46-smoke-n46-r3.log). **Play build: Builds/n46-r3.**
- 01:54 the flow table at 71905b12 (Logs/feel/n46-flow/freeze/table.md): no ✗ on any level.
- 01:55 leftover round: V1 builds shift46 (the state-change light) for an r4 at ~03:05; r3 stays the play build unless r4 passes verbs
  and the smoke.
- 02:05 picked V1 eb5a1e6a (shift46, off); gates GREEN (dn-check 0, fingerprint EQUAL x3, n45rows GREEN). 02:06 build r4 (batch, 31.3 s);
  reels a546 -skygrindShift46 on n46-foundry / n46-spire / n46-kiln: GREEN (changes drawn 2 / 2, 4 / 4, 1 / 1; 0 NRE).
- 02:09 blind pairs of shift46 (seed 46; the lead blind on 3, a reviewer agent on 5): b1 last wave ON / ON; b1 clear ON / OFF (the reviewer
  called the wrong frame cyan); v2 last wave - / "same"; v6 clear - / ON (but "a bright cyan bar at the crosshair": the slab lands at the
  screen centre); k5 crumble OFF / ON. N-4's rule not met: **shift46 stays OFF; r4 does not replace r3** (no new verbs / smoke needed).

## Close table (2026-10-18)
| card | state | measured | date |
|---|---|---|---|
| G0 | DONE | rows-switch-on 30 / 19 + n46-shared.cjs 32 files; gates GREEN (dn-check 0, golden 0 / 22, fingerprint EQUAL plain / --off45 / --off46, n45rows GREEN, campaign --tape PASS, Editor 0 errors); N46Shared 4 / 4, NightRows 14 / 14, Lesson25 16 / 16, Campaign24Tests 12 / 12 | 23:04 |
| P0 | DONE | Core 1127 / 1129 (2 = Lesson25Tests' leaked flags, fixed 9435a6c7; L+N 59 / 59) + Game 1046 / 1046; the live Editor stalled 23:34-00:10 (G0's MapChoice / N3 / N4 lost, re-run in the suite) | 00:44 |
| the card gate | DONE | B1 / A3 / A1 / A2 PASS 23:12-23:19, each with its rulings | 23:19 |
| F1 meter | DONE | n46flow + baseline (seeds 1-10, band from 11-20) + Tools/night/n46-flow-table.sh; final table at 71905b12: no ✗ | 01:54 |
| F1 rules | DONE | retry46 / overlap46 (holds entryM 12, growthMax 2) / teach46 / stuck46 / reveal46 ON; with the four flow rows L1 walks 16 -> 20, L3 7 -> 19; a646 GREEN on all five levels | 01:45 |
| A1 | DONE | span46 ON: p1 / p3 / p5 rebuilt + lip returns; reach 9 / 9; walks 20 / 20; L4 stall 18.7 -> 14.5 %, dead 15.6 -> 6.2 s, clear 127 -> 96.3 s; a146 8 / 8 | 01:45 |
| A2 | DONE | spire46 ON: v2 THE FALLEN BELL, v6 THE ROPE CHOIR, gates 4 -> 3; verbs37 L5 27 / 30 -> 30 / 30 by data; L5 clear 191 -> 163.1 s. SHORT inside: the bell reads as a stepped stack (T-81) | 01:45 |
| A3 | DONE | foundry46 / iceline46 ON (7 changes by audit row; g3 THE TIPPLE); L1 dead 53.3 -> 30.2 s; b1 wave 1 0 -> 4 / 4 in view. SHORT inside: g3's own stall / dwell targets (T-80) | 01:45 |
| B1 | DONE | kiln46 + walkin46 + wyrm46 ON: walked in 8.00 -> 8.00 m/s (was a 0 m/s teleport); bot median 38.3 s (was 161 s), max 85.1 s, 40 / 40; L3 walks 7 -> 19 / 20, clear 218.5 -> 123 s | 01:45 |
| V1 | DONE (feel46) / SHORT (kicks, shift46) | feel46 ON with G1 + G5 (blind pairs both ON; warned 14 / 14 and 5 / 5); the kicks OFF (blind "same", T-77); shift46 built, blind pairs mixed (2 of 5 agreed), OFF | 02:09 |
| verbs 1-5 | DONE | n46-r3 with the rows on: 26 / 26, 20 / 20, 25 / 25, 30 / 30, 30 / 30, 0 NRE (r3's first set ran rows-off by a tool fault: L-081) | 01:40 |
| the smoke | DONE | n46-r3 with the frozen config: "[smoke] 228.0s DONE: all steps passed", 0 NRE | 01:51 |
| memory | DONE | ledger wave-46, THREADS consolidated (96 -> 29 lines), CORE state line, FEATURES, LESSONS L-081-L-085 | 02:00 |

**Play build: Builds/n46-r3** (the freeze 71905b12's config; verbs and the smoke above).

## Flow table (Logs/feel/n46-flow/freeze/table.md; n46flow, one pilot, seeds 1-20; NIGHT45's levels --off46 -> the n46 levels with retry46 overlap46 teach46 stuck46 walkin46 wyrm46; 2026-10-18 01:53)
| level | walks | stall % | longest stall s | dwell s | dead s | deaths | clear s | worse past the band | state |
|---|---|---|---|---|---|---|---|---|---|
| L1 | 16 -> 20 / 20 ✓ | 31.1 -> 29.8 | 5.3 -> 3.1 | 15.2 -> 12.3 | 53.3 -> 30.2 ✓ | 26.5 -> 25.5 | 210.6 -> 137.9 ✓ | none | DONE |
| L2 | 20 -> 20 | 35.8 -> 37 | 7.1 -> 7.4 | 14.8 -> 14.7 | 36.1 -> 32.4 ✓ | 2 -> 1.5 | 196.5 -> 184.8 | none (stall +1.2 < 6.8, longest +0.3 < 4.8) | DONE |
| L3 | 7 -> 19 / 20 ✓ | 35.4 -> 31.7 | 8.8 -> 4.7 | 14.2 -> 6.2 ✓ | 26.1 -> 12.1 ✓ | 6 -> 6 | 218.5 -> 123 ✓ | none | DONE |
| L4 | 20 -> 20 | 18.7 -> 14.5 | 3.7 -> 3.9 | 7.9 -> 6.4 | 15.6 -> 6.2 ✓ | 2 -> 3.5 | 127 -> 96.3 ✓ | none (deaths +1.5 < 4) | DONE |
| L5 | 20 -> 20 | 12.9 -> 14.6 | 4.3 -> 4.3 | 8.5 -> 8.2 | 11.4 -> 10.4 | 0.5 -> 0 | 191 -> 163.1 ✓ | none (stall +1.7 < 6.8) | DONE |
| band | - | 6.8 | 4.8 | 4.1 | 3.3 | 4 | 20.2 | | 01:53 |
✓ = better past the band; no level is worse past the band on any metric. Bosses are not in this pilot's numbers (B1's harness: the worm).

- 01:05 close skeleton (insurance): if the night stops here, the play build stays n45-r4; r2 (Builds/n46-r2) has every row OFF in the
  repo's config and passed verbs37 on L1-L4 only (L5 27 / 30); the ON candidates are the rows above, none frozen yet.
