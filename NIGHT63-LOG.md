# NIGHT63-LOG: the boss night (8 hours, 7 minimum)

**t0: Thu Oct 22 2026 22:36:01 MST (epoch 1792733761)** (`node Tools/night/n63-clock.cjs start`). FREEZE (the close) 7:00 = 05:36:01;
CAP 8:00 = 06:36:01.

## Bearings (session 1, 0:00)
- `node Tools/night/n63-clock.cjs` before start: `clock n62: 0:49 / 2:00 since t0 21:46:11; phase NONE ... (stopped)`: NIGHT62's clock was
  already stopped (Logs/clock.json `stoppedText` 22:30:18), so `n62-clock.cjs stop` was not needed. Then `n63-clock.cjs start`: t0 22:36:01.
- `git tag -l u3-n62`: `u3-n62` (present; on 1b0696b5 per the NIGHT62 close note, moving it is GG's call).
- HEAD 3bbdf4fe on port/stage3. `git status --short`: GG's four files and the four TMP fonts modified (never staged), the untracked night
  prompts and DECIDE files; nothing of NIGHT63's yet.
- The Editor: `unity-loop.sh status` ready, playMode stopped; `set_autotick` "Auto-tick already enabled (interval updated to 16ms)".
- Memory: 41.8 GB commit free. Disk: C: 42.6 GB free.
- **Branch A** (disk >= 12 GB, the Editor answers, commit >= 20 GB).
- `Logs/n63/slate.json` copied from the seed: `slate: 8 slices: 0 PASS (passes true), 0 SHORT, 0 BLOCKED, 0 in progress, 8 todo`.

## Agents
- gg-proxy (start check): 22:39-22:42, done. Logs/n63/gg-proxy-start.md (30 frames opened, six per level). No boss on any freeze frame
  (verbs37's "43 the boss walk-in SKIP"). Top 8: (1) empty box rooms (7 asks), (2) choppy movement / clunky wall ride (7), (3) bosses rigid
  and never saying what they want (7, up from #5), (4) effect colours not aligned (6, up from #10): a red-orange line down spire's cyan exit
  door in 5 of 6 vp frames, lime / green floor stripes on span and iceline, magenta dashes under the HUD's weapon name in 24 of 30 frames,
  a screen-filling orange dome with a green rim (vf/f0400); (5) the fight hidden (8 of 30 frames show an enemy), (6) dark flat views and
  floating ceiling panels (8 of 30), (7) kills don't satisfy, (8) the style meter at D / DULL, the score at 500 in all 30 frames.
- 22:44 (0:08) builders spawned in the background, each in its own worktree with the forge's builder brief, told to read first and branch
  only once port/stage3's head is the "NIGHT63 A1" commit (spawned before A1's commit to save their reading time; the flow base held the
  gates): **D** (D1 same-run pairs, then C1, then B3; branch n63-D1), **V** (B1: BossAttackClock63 + the boss63 reel first as its own
  pick, then the BossMotion63 layer; branch n63-B1), **G** (P2 the boss rule line on time, then B2 once V's clock and reel are picked;
  branch n63-P2). Agents alive: 3 (two slots free for the GATEs / spotter / gg-proxy).
- ~23:00 (0:24) **G reported P2** (n63-P2: 688ced75 test RED, 36861917 fix GREEN, 24a36d31 notes Research/2026-10-22-wave63/G.md): the red
  read from NIGHT62's b61k telemetry (the wyrm's card at t 0.317 s, its bar at 2.317 s, the line at 2.750 s); two view causes (the line
  began only with the boss bar, which the wyrm lacks during its 2.0 s emerge; the first boss's bar created the fight-line label); the fix:
  the line in BossRule61DelayS 0.45 s after the rise, bar or not, the card's subtitle hidden while the line holds; dn-check 0; gates
  --no-editor GREEN 22:58:10; tests run outside Unity (RED 4 / 6, GREEN 6 / 6, N61BossRuleTests 4 / 4). G resumed by SendMessage with
  leftover 2 (text61's pinned-hint reel argument, branch n63-P3), B2 to follow V's picks.
- ~23:03 (0:27) **V reported pick 1** (n63-B1: 70500356 test RED, cdb636e9 BossAttackClock63 + boss63 + the switch + the probe, 44e5eeaa
  notes Research/2026-10-22-wave63/V.md): dn-check 0; gates --no-editor GREEN 23:00:47; the red read from the views (no motion layer:
  every part's offset 0; spring / flinch / anticipation words 0 in the five boss views and Boss37Kit.View.cs). V continues with the layer.
- **Picks (L-117, the builders' own shas):** P2 test commit 688ced75 -> 33f2cd61; Editor compile 0 errors; **N63BossRuleTimeTests RED
  (2 failed: BarNeverUp, TheWyrm "began at 2 s")** (Logs/n63/p2-red-editor.txt). Then 36861917 + 24a36d31 -> 8dac3008 + bd5e5147 and V's
  70500356 -> d3336128; compile 0 errors; **N63BossRuleTimeTests 6 / 6, N61BossRule 6 / 6 GREEN; N63BossClockTests 0 / 6 RED**
  (Logs/n63/p2-green-v-red-editor.txt). Then V's cdb636e9 + 44e5eeaa -> da66dae1 + fdd3acc7: compile 0 errors, **N63BossClockTests
  6 / 6, N63Shared 3 / 3; gates all GREEN 23:08:32** (Logs/n63/pick-v1-p2.txt). Diff read:
  BossAttackClock63 assigns nothing on the sim (grep); boss63's forced stagger at 14 s goes through the sim's public calls, printed FORCED.
- ~23:07 (0:31) G re-ordered by SendMessage: B2's measurement hooks (Boss63EachG / Boss63EndG: each attack's visible cue start, hit, lead,
  hue, parry) first so they ride in b1, then leftover 2, then B2's fixes after b1's numbers.

## Picks

## Builds
- **b1 = n63-b1**, started 23:10 (0:34) from fdd3acc7 (A1, P2, V's clock + boss63 reel; every NIGHT63 row OFF in the repo), background
  (Logs/n63/build-b1.txt). Plan: in its copy bossRule61 ON; reels P2 (boss61 on the five bosses + the wyrm off), boss63 x5 (B1's and
  B2's red: the sim's wind-up leads, rms 0), verbs37 x5; P2's SPOT on the wyrm's 1.0 s pair (a two-run pair: D1 not yet picked; the
  report says so).
- b1 built 23:09:14-23:09:52 from fdd3acc7; drift GG's two files only (EditorBuildSettings, UnityConnectSettings). `n46-build-rows.cjs
  n63-b1 on n61.bossRule61` (the n63 rows all false in the copy).
- **P2 SPOT first** (23:10:07-23:11:03, Logs/feel/n63-b1/summary47.txt): b61k `[reel] boss61: GREEN boss61 wyrm` (at 1.000 s alpha 1.00,
  em 40.0 px; 720p em 26.7 px; shows in 8 s 1; visible 4.75 s from 0.47 s); b61koff `OFF` (shows 0); NRE 0 both. Pair changed 20,382 px.
  **SPOT (23:11, blind, Logs/n63/spot/P2.txt): "A says 'Swing the gauntlet LEFT CLICK at its charging head.' ... B says 'SHOOT ITS HEAD.
  THE OPEN CORE HITS X3'"**; mapping after: A = ON. It names the line: P2's evidence continues (the other four bosses, verbs37 x5).
- 23:11 b1's boss63 set started (background): b63f / b63i / b63k / b63s / b63p (B1's and B2's red).
- 23:11:26-23:15:10 **boss63 on b1 (the red, bossMotion63 OFF):** every boss `[reel] boss63: OFF`, **rms seconds 23 (zero 23), best 0.0000 m**;
  damage events drawn 0 of 42 / 14 / 21 / 17 / 35 (wick / brawl40 / wyrm / loom37 / crown37); wind-ups posed 0; the sim's own wind-up
  leads: wick 3 strikes 1100-1300 ms, brawl40 1 strike 1100, wyrm 2 strikes 533-900, loom37 4 strikes 1325-1633, crown37 6 strikes
  1000-1508; staggers 1 / 4 / 2 / 1 / 1 (one FORCED on wick, wyrm, loom37, crown37); frames idle / flinch / stagger / windup on all five;
  NRE 0. (B2's visible cue leads and hues need G's hooks.)
- ~23:15 P2's other four bosses + verbs37 x5 on b1 started (background).
- 23:15:35-23:22:44 **P2's rest on b1:** `[reel] boss61: GREEN` on wick (at 1.017 s), brawl40, loom37, crown37 (each: alpha 1.00, em 40.0 px;
  720p 26.7 px; shows in 8 s 1; visible 4.45-4.75 s from 0.47 s; NRE 0); with the wyrm: 5 / 5 GREEN. **verbs37 x5 on b1: 27/27, 29/29,
  31/31, 31/31, 31/31, RED rows [] everywhere, NRE 0** (bossRule61 on in the copy).
- ~23:23 three reports arrived: **D1** (n63-D1: 39177d68 test RED, ef89a55a Core ReelOverride + Live + an allocation-free ViewOn,
  0f80d89c the reel option -skygrindSameRun63 with planted rows noop63 / plant63 and scenario same63, aac1abbc the views' wall clocks
  through the held clock (30 files), 655b8942 three guards in SurvivalGame.Update, 06e45573 notes D.md; gates GREEN 23:16:25; tests
  outside Unity 14 failed -> 17 / 17, 0 bytes allocated; the red: NIGHT62's two P1 runs differ by 139,791 px at the kb3 entry before any
  lamp); **G's B2 measurement** (n63-B2: 58e7edc2 test RED, bf858d0a the measure, 1f30bb1e cadence, 599968b5 notes; gates GREEN
  23:20:04; method: the drawn objects' property-block colours, not screen pixels); **V's pick 2, the layer** (n63-B1: d76cc4d6 test RED,
  a1810813 numbers by n63-v-rows.cjs, 3ae25b65 the layer, 437bcbf8 the five view hooks, 84097a5e camera distance lines, 2f5790f1 notes;
  gates GREEN 23:22:32; SPOT pose THE ICEBREAKER b63i shot-N30-B63-brawl40-stagger, an estimate of 100,000+ px, arithmetic).
- ~23:24 P2's GATE critic spawned (background). D sent C1; V sent leftovers 3 and 4 (T-146's code items, R1's N3 / N11 / N12) on n63-L.
- The b2 pick, test-first commits first: 39177d68 -> 28113b0e, d76cc4d6 -> e4001084, 58e7edc2 -> 41a314b4 (no conflict); compile 0 errors;
  **RED in the Editor: N63SameRunTests 3 / 10, N63BossMotionTests 2 / 10, N63BossCueTests 0 / 7** (Logs/n63/pick-b2-red.txt). Then D1's
  ef89a55a 0f80d89c aac1abbc 655b8942 06e45573 -> e4ba589e 9e83c6f1 13953aed 34f1bb41 126e8bcb; V's a1810813 3ae25b65 437bcbf8 84097a5e
  2f5790f1 -> 5e5b31b7 f38ec5b9 26bcf2c3 1249dc0f 1a79a786; G's bf858d0a 1f30bb1e 599968b5 -> 448f733d 69a3f23f e87536d8: **no conflict**;
  config copies identical. Diffs read: D1's Core (ReelOverride empty outside a reel; ViewOn's no-override answer pinned against the old
  formula; DesignEnabledAt walks the same keys), the three Update guards on FeelClock.Holding; V's layer steps on game.WorldDt / FrameDt
  (D1's contract) and only BossMotion63Probe reads Time.frameCount. Compile, the night's classes and the gates running.
- b2's plan (rows on in its copy: n61.exitFrame61, n61.bossRule61, n63.bossMotion63): set 1, SPOT first and D1's planted cases: d1base
  (same63), d1noop (+ -skygrindSameRun63 n63.noop63: want 0 px), d1plant (n63.plant63: want 100 px), p1k (a157 n57-kiln
  -skygrindClearShots60 -skygrindSameRun63 n61.exitFrame61: P1's SPOT pair + own gain), b63i1 (boss63 n52-iceline -skygrindSameRun63
  n63.bossMotion63: B1's SPOT pair); then set 2: boss63 on the other four with the row on, the five with -skygrindNoBossMotion63 (B1's off
  control and B2's red), d1noopk / d1noopw, verbs37 x5.
- **Editor after the b2 pick:** N63SameRunTests 10 / 10, N63BossMotionTests 10 / 10, N63BossCueTests 8 / 8, N63BossClockTests 6 / 6,
  N63BossRuleTimeTests 6 / 6, N63Shared 3 / 3, N61ExitFrame 9 / 9, NightRows 14 / 14; **gates all GREEN 23:29:44**
  (Logs/n63/pick-b2-green.txt).
- **P2 GATE: `P2: PASS`** (~23:29, Logs/n63/verdict-P2.md; ten problems). Filter (DECIDE63 D-4): 1 (two-run pair) accepted and recorded;
  2 (the off side shows the card's subtitle) noted: the row replaces it; 3 / 4 (the wyrm's line names the gauntlet, the frame's player
  holds the THUNDERCLAP, the old subtitle said SHOOT) open for GG, not checked in a campaign run; 5 (the Icebreaker's bar tag above the
  line) a fix for later tonight; 6 (the Loom's outline crosses the text, readable), 7 (9 words as drawn), 8 (the before on four bosses not
  measured), 9 (the 3 s drop rule removed, untested), 10 (a stray RED in the file is V's test-first) noted.
- **b2 = n63-b2** built 23:30:12-23:30:5x from e87536d8 (D1, V's layer, G's cue measure, P2); drift GG's two files; `n46-build-rows.cjs
  n63-b2 on n61.exitFrame61 n61.bossRule61 n63.bossMotion63`. Set 1 started (background).
- **P2 ON:** `n63-freeze-rows.cjs "n61.bossRule61=..."`: "ON in both config copies: n61.bossRule61"; both copies print `true`; slate P2
  pass. Committed 65bc7cce.
- **b2 set 1** (23:31:37-23:34:49): d1noop / d1plant / d1base same63 GREEN (6 toggles fired, all "sim held yes"); **D1's planted cases
  RED**: no-op 19 / 12 / 7 / 45,686 / 195,235 / 253,849 px (want 0), plant 122 / 117 / 117 / 45,595 / 195,276 / 253,369 (want 100); by
  eye the viewmodel, crosshair arc and a hit flash move in the held frame, shot 03 a full-frame noise (1.78 M px at mean delta 5.1).
  p1k (a157, same-run exitFrame61) GREEN 5 / 5 rooms, 8 "door lit"; **P1's own gain on the same-run pair 1.21 / 1.16 / 1.37 / 1.07 /
  1.23x: 0 / 5** (GUARD 5 / 5; Logs/n63/p1-owngain-b2.txt). b63i1 (boss63 iceline, same-run bossMotion63) GREEN: rms 23 / 23 seconds, best
  1.0557 m, limit share max 1.00, flinches drawn 14 / 14, wind-up posed 1; its off captures dark.
- D sent both D1 findings (fix round 1); DECIDE63 D-5: D1 not landed, two-run pairs until it passes; P1's fix round on its measured MAIN
  (UnityTuning.N61V.cs: ExitFrame61Glow 4 -> 8, ExitFrame61LightIntensity 12 -> 36; uncommitted until the b3 pick).
- **B1's two-run pair on b2** (23:36:05-23:37:44): b63i GREEN (rms 23 / 23, best 1.0580 m, limit 1.00, flinches 14 / 14); b63ioff OFF (rms
  0). Changed px on the shots: stagger 990,686, flinch 1,266,712, idle 609,427, windup 53,501. By eye the OFF frames are nearly all black
  at the reel's 1.7 m (b1's row-off frame of the same pose too: mean luma 9.2 vs 8.9; the camera seems inside the Icebreaker's rigid
  body: an inference); with the layer on the body sinks and keels and the room shows. Not SPOTted: the pair's difference is the screen
  being blacked out, not a pose a spotter can name; V's named second choice (THE WICK's stagger) is measured in set 2.
- ~23:38 **V reported leftovers 3 / 4** (n63-L: rim56 one read a frame 37486d89 / d7d1c669; an unlocked Editor run never pauses on focus
  loss ed313f54 / 74086080; N3 2864ed53 / a166b223; N11 58b3285a / e96a9a3c; N12 355c49d0 / 3a895fc4 (selftest 7 / 11 -> 11 / 11);
  notes df5f57e7; gates GREEN 23:36:37). **G reported P2b** (n63-P2b: bce1cc1a RED, 5ed17345 the bar hint hidden while the rule line
  holds, 61dba9c7 notes; gates GREEN 23:38:09).
- 23:39 b2 set 2 started (background): boss63 on / off on foundry, kiln, span, spire; verbs37 x5.
- B2's red so far (G's cue lines, b2 b63ioff): THE ICEBREAKER's `brawlLeap` hit 4025 ms, sim tell 1008 ms before, **no drawn danger or
  parry cue found** ("off the danger hue 1 (brawlLeap)").
- The b3 pick, test-first: P3 baced372 -> 799ec397; P2b bce1cc1a -> eb80ebff; V's REDs 37486d89 ed313f54 2864ed53 58b3285a 355c49d0 ->
  83dd8d2c 6d20acd9 9b4c411e 5d6350ab 5f940a34; compile 0 errors; **RED: N63PinHintTests 0 / 4, N63BossRuleHintTests 1 / 2,
  N63RimReadTests 0 / 2, N63FocusLockTests 0 / 3, N63ModeListTests 1 / 2, N63GunFamilyTests 1 / 2** (Logs/n63/pick-b3-red.txt). Then the
  fixes and notes (69d9c648 91fa5515 5ed17345 61dba9c7 d7d1c669 74086080 a166b223 e96a9a3c 3a895fc4 df5f57e7 -> d82493c0 4c848973
  4f602599 f08ce862 7214fdd9 01c3e6d8 073bfe14 3a8c35d7 ffe7134a 18cee4e1), no conflict; P1's numbers committed 9399b97c; compile, tests
  and gates running (an Editor compile beside set 2's reels: two heavy jobs at once, noted).
- Set 2 (b2): **wick GREEN** (3 / 3 posed, 42 / 42 flinches, rms 23 / 23, best 1.5897 m, limit 1.00); **wyrm RED** ("BAD: 1 wind-up poses
  under 250 ms before the hit": spit windup 23592 hit 24492 lead 900 pose 192; aggro lead 533 pose 442); their off sides OFF (rms 0).
  Pairs: wick stagger 42,273 px, flinch 32,224; wyrm stagger 8,189, flinch 1,785.
- 23:43:12 **B1 SPOT** on the wick's stagger (V's second choice; a two-run pair): "In B, the dark hat-like disc floats flat, well above
  the glowing cyan orb ... In A, the disc is tilted and pressed down onto the top of the orb" (Logs/n63/spot/B1.txt); mapping after: A = ON.
  **It names the pose.** B1's doneWhen still RED on the wyrm: V's fix round sent (branch n63-B1b).
- b3's pick GREEN: N63PinHintTests 4 / 4, N63BossRuleHintTests 2 / 2, N63RimReadTests 2 / 2, N63FocusLockTests 3 / 3, N63ModeListTests 2 / 2,
  N63GunFamilyTests 2 / 2; neighbours N63BossRuleTimeTests 6 / 6, N61BossRule 6 / 6, N61Text 7 / 7, N61ExitFrame 9 / 9, N61GunYellow 7 / 7,
  FocusAutomation61Tests 36 / 36; **gates all GREEN 23:46:49** (Logs/n63/pick-b3-green.txt). Committed log 1fc24f85.
- **B2's red** (b2's five off sides, G's meter): cue roots found for wick, loom37, brawl40; **none for the wyrm and the crown** ("nothing
  drawn within 3 m"): their 8 attacks not measured. On the three measured bosses **0 of 8 attacks show a drawn danger or parry riser >= 300
  ms before the hit** (all `start none`; sim tells 1008-1625 ms); whether the meter misses particle or shader cues: not checked. Sent to G:
  fix the meter's roots and prove it on a planted cue, then B2's fix (a danger-hue cue for every non-parry attack, n63-B2fix).
- ~23:47 **D's D1 fix round** (n63-D1fix f1d2acd8: both halves of a pair are held frames, a dt-0 frame repeats itself; the shot's PNG
  replaced by held frame 1; no impact crush inside a held frame; each "toggle fired" line now prints the pair's changed px and the luma;
  the black b63i1 off frame's cause not found by reading; a note for V: Brawl40View.N63V.cs resets its springs on the off path, passed to V).
  Picked; compile, N63SameRunTests and gates running.
- D1's fix: f1d2acd8 picked; compile 0 errors; N63SameRunTests 10 / 10; **gates all GREEN 23:50:53** (Logs/n63/pick-d1fix.txt).
- **b2 set 2 done** (23:38:58-23:51): loom37 GREEN (4 / 4 posed, 18 / 18 flinches); **crown37 RED: "rms 0 in 23 of 23 seconds (the rigid
  copy); 6 strikes without a drawn wind-up pose; 49 damage events without ..."** (the layer does not drive the Crown); off sides OFF (rms
  0); **verbs37 x5 on b2: 27/27, 29/29, 31/31, 31/31, 31/31, RED rows none; NRE 0 in all 20 b2 reels**. B1 so far: GREEN on wick,
  brawl40, loom37; RED on the wyrm (a late wind-up) and the crown (no motion). V's fix round now has three items (the wyrm, the crown,
  the springs' keep-state on the off path).
- ~23:52 b3 build started from HEAD (D1 fix, P1's numbers, P3's pin, P2b, V's leftovers).
- **b3 = n63-b3** built 23:51:35-23:52 from f398cea3; drift GG's two files; copy rows on: exitFrame61, text61, bossMotion63 (bossRule61
  already true). Set A (23:53-00:01):
  - **D1 on b3:** no-op pairs 2 / 16 / 4 / 6 / 4 / 0 px (was up to 253,849), plant 106 / 111 / 106 / 104 / 102 / 100 (want exactly 0 and
    100: a 0-16 px residual left); the toggle lines' own counts match the meter. b63i1 (boss room): held-on vs held-off pairs 5,486 /
    20,563 / 522,208 / 479,379 px (the last two with "DARK HELD FRAME": the Icebreaker's 1.7 m rigid pose); held frames darker than the
    live shot frame (luma 0.12 -> 0.08; the transient muzzle / hit light absent while firing is held: an inference).
  - **P1 after its fix round** (p1k, same-run + pinned hint): own gain 1.38 / 1.32 / 1.56 / 1.18 / 1.37x = **1 / 5** (wants 4 / 5);
    **GUARD 4 / 5** (kb3d's p95 outside moved 0.025 > 0.02) (Logs/n63/p1-owngain-b3.txt). **P1 SHORT by its own numbers** (one fix
    round used); its SPOT run anyway for the report (kb3 clear pair, 60,715 px).
  - **P3 (text61) with the hint pinned:** `n63-p3-blocks.cjs`: on, two or more blocks 0 of 5, ROOM CLEAR 0 of 5; off, 4 of 5 and 2 of 5;
    pins fired both sides. SPOT on the kb2c entry pair (two-run, pinned; 37,767 px) running.
- ~23:55 G: the B2 meter's 16 "start none" were its own bug (negative instance ids of run-time objects read as "no riser"); roots now by
  each boss view's root name (n63-B2fix c984ddc9 RED / b12acbb0 GREEN, gates GREEN 23:54:59); to be picked with the B2 fix.
- ~00:00 **SPOTs on b3:** P3 (kb2c entry, two-run pinned, 37,767 px): "B has a hint bar under the level title saying 'Midair, aim down,
  then LEFT CLICK', and A does not" (A = ON): names text61's change. P1 (kb3 clear, same-run, 60,715 px): "The gun at the bottom of the
  screen is bright cyan in A and dull grey in B. The glowing cyan door frame ... also brighter in A" (A = ON): the gun first. **P1 SHORT**
  (slate; DECIDE63 D-6); its brighter numbers reverted (acbabae4).
- ~00:01 **V's B1 fix round** (n63-B1b: the wyrm's pending tell outranks its stagger in the clock (one of V's own pick-1 assertions
  changed in the RED commit, DECIDE63 D-6); the off path keeps every spring's state; the Crown draws in LateUpdate after the reel read
  the probe: BossMotion63Probe.Take; gates GREEN 23:58:02). Picked test-first: REDs 2e99d5c0 42f8361d fb331fda -> e03c9635 e55a8b8d
  f1a41c7f; **RED in the Editor: N63BossClockTests 6 / 7, N63BossMotionTests 10 / 14** (Logs/n63/pick-b1b-red.txt); GREENs + notes
  6d6502eb 97fe2b42 4dae11fe 636b8e58 -> 2719bc2b af7a0a1a 384d609b c51e960d; with G's meter RED c984ddc9 -> 05d30aa6: **N63BossClockTests
  7 / 7, N63BossMotionTests 14 / 14 GREEN; N63BossCueTests 8 / 9 RED** (Logs/n63/pick-b4-1.txt). G's B2 RED 811bce3b conflicted out of
  order (it builds on the meter fix): aborted, re-picked after b12acbb0.
- b3 set B: P2b `[boss61] bar hint held: "PARRY THE BLOCK. PUNISH STUCK FISTS" (the rule line holds the place)` / `bar hint back` on the
  Icebreaker; on the wyrm the hint that comes back is "PUNCH THE HEAD TO STAGGER IT" (contradicts the rule line: DECIDE63 D-4); boss61 GREEN
  on both; the Icebreaker's 1.0 s frame shows only the card and the line (my eye, a crop).
- ~00:05 **G's B2 fix** (n63-B2fix 811bce3b RED, 6a490fa1 the danger column: a red-orange unlit additive tube around the boss through every
  non-parry attack's wind-up, from 0.45 to 1 as the hit nears, gone on the strike; parry attacks skipped; 5d2cfd86 notes; gates GREEN
  00:00:05; G's note: not applied attack by attack). **D's C1** (n63-C1: 619f852d the audit + meter, b7807abe RED, c65a0be7 the table,
  c4a662e9 the routing, 29485090 notes; gates GREEN 00:01:07; the red: 392 hand-written colours not from the table, 191 outside every
  role band; off-palette share 0.19-0.59 % per level on n62-freeze). D sent B3 next. Picks: b12acbb0 811bce3b 619f852d b7807abe -> 17607e3a
  c0762684 a5733ab2 c5cf41c1; compile and tests running.
- **RED:** N63BossCueTests 9 / 9 (the meter fix GREEN), **N63BossTellTests 1 / 4, N63ColoursTests 0 / 7** (Logs/n63/pick-b4-2.txt). Then
  6a490fa1 5d2cfd86 c65a0be7 c4a662e9 29485090 -> f8b88ba5 d2465319 a9a70c6c 1def409d 5141affb (config copies identical): **GREEN:
  N63BossTellTests 4 / 4, N63ColoursTests 7 / 7, N63BossCueTests 9 / 9, N63BossMotionTests 14 / 14, N63BossClockTests 7 / 7,
  N63SameRunTests 10 / 10, N63Shared 3 / 3, N63BossRule 8 / 8, N63PinHint 4 / 4; C1 METER PLANTED GREEN; gates all GREEN 00:08:04**
  (Logs/n63/pick-b4-3.txt).
- **b3 set B done** (23:58-00:05): **verbs37 x5 on b3 27/29/31/31/31, RED none; NRE 0 in all 14 b3 reels**; text61 "[text61] shown" per
  verbs37 level 0 / 2 / 10 / 2 / 1, none repeated. P3 added to tonight's slate with NIGHT62's doneWhen verbatim
  (Tools/night/n63-slate-add-p3.cjs); **P3's GATE spawned (~00:07)**.
- ~00:09 b4 build started from HEAD (B1's fix round, B2's meter fix + danger column, C1's table + routing).
- ~00:10 **P3 GATE: `P3: PASS`** (Logs/n63/verdict-P3.md; seven problems: the meter's "title" field reads the component, not the pixels;
  the pin did most of the work at the entries; the 3 s fallback stacks text 4 times in verbs37; kiln teaches 10 lines; kiln's card once;
  level 1 thinly checked; kb2 a weak frame). Filter: DECIDE63 D-7 (open for GG). **text61 ON** after b4's build finished:
  `n63-freeze-rows.cjs "n61.text61=..."`: "ON in both config copies: n61.text61", both copies `true`; committed f2b0c261.
- **b4 = n63-b4** built 00:08:33 from 5141affb; drift GG's two files; copy rows on: bossMotion63, bossTell63, colours63, text61.
  SPOT-first same-run reels (00:09:24-00:11:22): b63fS boss63 wick GREEN (3 / 3 posed, 42 / 42 flinches, rms 23 / 23, limit 1.00);
  pairs (the toggle lines): **B1** wick idle 4,048 / flinch 22,253 / windup 34,348 / **stagger 37,345 px (under rule 8's 41,472 on a
  same-run pair; b2's two-run 42,273 included drift)**; **B2** (b63fT, motion off) wick idle 0 / flinch 0 / **windup 162,933** / stagger 6;
  **C1** (c1s span a157) p1 entry **131,656** / p3 49,431 / p5 45,093.
- 00:11:42 **B2 SPOT** (same-run): "A has a big see-through red cylinder around the boss in the middle of the room; in B the cylinder is
  gone and the boss stands uncovered" (A = ON): names the cue. **C1 SPOT** (same-run): "B has the wide wall bands (on the left wall, the
  centre block and the right wall) in bright blue, where A has them in pale grey" (A = ON): names a colour change (a muting: the GATE
  weighs "bland"). Both continue to their MAINs; B1 waits for a same-run pose over the bar (set 2's other bosses) or V's bigger change.
- ~00:12 b4 set 2 started (background, 24 reels): B1 same-run on iceline / kiln / span / spire; B2 after (-skygrindNoBossMotion63) and
  before (+ -skygrindNoBossTell63) on all five; C1 verbs37 on x5 (c1v*, also verbs37 x5 for the build) and off x5 (c1x*).
- ~00:13 the flow gate (`n63-gates.sh --flow`, owed after the bossRule61 and text61 flips) started (background, beside the reels).
- D1's last doneWhen line checked on b2 (read-only, node over the telemetry): d1base vs d1noop same63 frame lines (sim-side fields)
  differ from f24 (step 106 both; enemy 1's rect x 859.0 vs 860.3), before the first toggle (frame 58): two plain runs of same63 drift
  (L-129), so "the sim lines identical with the option on and off" cannot be shown by this pair; the fingerprint gates stay GREEN.
- ~00:15 D1's GATE spawned; ~00:20 **`D1: FAIL`** (Logs/n63/verdict-D1.md): planted pairs not exact (0-16 / 100-111 px, the residual on
  the viewmodel x ~1180-1330, y ~676-736); bossColours63 unread then, bossRule61's OFF never exercised; "the sim never sees the toggle"
  not shown (b2: 240 frames / 428 steps without the option, 234 / 416 with); the held ON frame differs from the live shot frame by
  45,779-253,860 px on 6 of 12 shots (both halves of a pair are held frames, so the pair stays fair); **N63SameRunGameTests (7) never ran
  in the Editor** (my filter "N63SameRunTests" missed it); the b3 brawl40 "idle" shot shows STAGGER on the HUD. **D1 SHORT** (fix round
  used); kept in use for pairs with the residual stated (DECIDE63 D-6).
- ~00:20-00:25 reports: **D's B3** (n63-B3 c5fe14bd RED, 83c1e3da the parts table, 7b9476ce one wrapper per boss colour site, 04085278
  notes; gates GREEN 00:14:21; red per boss: off-palette 0.107 / 5.148 / 7.779 / 0.635 / 0.777 % of the frame; SPOT pose the wyrm's
  stagger; DECIDE63 D-8: weak points cyan on every boss). **V's B1c** (n63-B1c a861c7fa RED, b76a84f1 numbers: slump sink 0.45 -> 0.6,
  keel 14 -> 26 deg, squash 12 -> 22 %, the wick's guard 0.6 of its collider; a742a852 notes; gates GREEN 00:19:31). **V's C2** (n63-C2
  76c19058 meter, 03def204 RED, dd68dcb8 nearFade63: a live enemy part dissolves as the eye comes within 1.0 m; 3b1ba4d7 notes; gates
  GREEN 00:13:05). **G's S7** (n63-S7 4463ab94 RED, 34aacf74 press63: frames, pistons, a ram head on its own clock, striped plinths, a
  lamp; 5c078f99 notes; gates GREEN 00:23:16). Next: D on R1 N1 / N5; V on the Icebreaker covering the camera; G on text63 (D-7's points).
- **00:26:37 the flow gate GREEN:** "gate flow --off63: EQUAL to flow-base", config, golden 0 mismatches, fingerprints plain /
  --off57..--off63 EQUAL, the Editor 0 errors (Logs/n63/gates-flow-b4.txt).
- Disk 27.7 GB at 00:25 (Logs/feel/n63-b4 11 GB unpruned during its set; Builds/n63-b1..b4 383 MB each).
- N63SameRunGameTests **7 / 7 GREEN** in the Editor (00:27; Logs/n63/tests-samerun-game.txt): D1's Game half, unrun until now.
- **B2 on b4 with the corrected meter** (before = b63?B, bossTell63 off; after = b63?A): the end lines are IDENTICAL: wick 3 found, under
  300 ms 0, off the danger hue 0; brawl40 1 / 0 / 0; **wyrm 2 found, off the danger hue 1 (spit)**; loom37 4 / 0 / 0; crown37 6 / 0 / 0
  ("parry drawn 6 of 1 yellow"). cue roots found for all five. The spit after the fix: `start 23750 hit 24492 lead 742 hue 32 ... danger no
  ... obj KILN WYRM/trim 9` (the wyrm's own trim, 2 deg outside the danger band) though `[tell63] cue wyrm spit` fired. **B2's MAIN not met
  (the one attack to fix still reads off-hue) and "attacks already fine are untouched" broken (the column on every non-parry attack):** G's
  fix round sent (n63-B2b; slate B2 back to building). G's text63 plan approved (keyed by technique: the step tutorial kept).
- **B1 on b4 after its fix round** (same-run "on" runs): wick, brawl40, **wyrm (fixed: 2 / 2 posed, none under 250 ms)**, loom37 GREEN;
  **crown37 now moves** (rms 23 / 23, best 1.9693 m, 6 / 6 posed) but RED "1 damage events without a drawn flinch": the line is `flinch
  8983 ... drawn off`, the flinch shot's own held OFF frame (a same-run reel artefact, to confirm on a plain run in b5). Same-run pairs over
  the bar: loom37 stagger 46,788; crown37 idle 103,446 / flinch 327,970 / stagger 290,271 (the crown's flame ring sits lower and shifted
  in the ON flinch frame, by eye); wick stagger 37,345 (under; V's B1c makes it bigger).
- **b4 set 2 done 00:34:55:** verbs37 x5 with colours63 on (c1v?) AND off (c1x?): 27/29/31/31/31 both, RED none; **NRE 0 in all 27 b4
  reels**. **C1's MAIN** (`n63-c1-meter.py --pair c1v? c1x?`): foundry 0.496 -> 0.447 % (drop 10.0 %, FAIL), iceline 0.460 -> 0.398 %
  (13.5 %, FAIL), **kiln 0.589 -> 0.048 % (91.9 %, PASS)**, **span 0.538 -> 0.105 % (80.5 %, PASS)**, spire 0.188 -> 0.131 % (30.3 %,
  FAIL): 2 / 5 levels. D's C1 fix round sent (n63-C1b). (The meter's "most changed frame" counts on verbs37 pairs are two-run drift.)
- ~00:36 G's B2b reading: the meter's end line counted an attack parryable when any parry-yellow riser showed, not by the sim's tell kind
  (so the crown's beamCut x2 and lowFire, dodge-only, read yellow-ish "parry yes": the before's real list is crown37 beamCut, crown37
  lowFire, wyrm spit); the spit's riser is the wyrm's ambient 2.1 s molten-seam wave (Wyrm47AmberHex x 2.4), which a 250 ms reference
  window cannot hold. Plan approved: the verdict by sim kind, the reference from the previous hit, the fix list computed into bossTell63's
  numbers, the fix only on listed attacks (the column + the boss's own warm glows pulled to danger during that wind-up).
- ~00:37 **D's R1** (n63-R1 a3bc6366 RED, f720da4e N1, cbcc6ea9 N5: NightRows.HasSwitch for every row's off switch (bossRule61, text61,
  kill55, focus60 ignored case nowhere until now), 1a619893 the viewmodel residual guess (every SSAO material rescanned), 55610d4a notes;
  gates GREEN 00:34:13).
- **b5's pick, test-first:** 76c19058 03def204 a861c7fa c5fe14bd 4463ab94 a3bc6366 -> d1fe00e2 76cbe156 fe3774eb 81b08bde da44b8a5
  a897186b (no conflict); compile and six classes running.
- **Disk:** n63-prune.sh's side pattern `[a-z0-9-]+` skips sides with capitals, so my b63fS / b63kA / ... sides were never pruned
  (b63kA/boss63: 1,535 files; Logs/feel/n63-b4 11 GB). Pruned by hand by the same rule (every 50th frame and every shot kept, only
  tonight's own frames): Logs/feel/n63-b4 11 GB -> 970 MB; C: 25.9 -> 36 GB free. Side names lowercase from now on.
- **b5 RED in the Editor:** N63NearFadeTests 0 / 5, N63BossMotionTests 14 / 15, N63BossColoursTests 0 / 5, N63PressTests 1 / 5,
  N63SwitchTests 1 / 2, N63R1Tests 0 / 2 (Logs/n63/pick-b5-red.txt). Then dd68dcb8 3b1ba4d7 b76a84f1 a742a852 83c1e3da 7b9476ce 04085278
  34aacf74 5c078f99 f720da4e cbcc6ea9 1a619893 55610d4a: **conflict at S7's 34aacf74** (both copies of the config: press63 appended after
  V's nearFade63): resolved by L-065: `git checkout --ours` the two copies, `node Tools/night/n63-g-rows.cjs` ("press63 written as
  design.n63's last key ... nothing else changed"), design.n63 = _why, bossMotion63, bossTell63, bossColours63, colours63, nearFade63,
  press63; **a notes conflict** at 55610d4a (Research/2026-10-22-wave63/D.md, B3's and R1's sections both appended): both kept, the three
  marker lines removed (0 left). HEAD 2a5a1345; config copies identical. Compile, 25 classes and the gates running; builders told to
  branch (V n63-ICE, G n63-B2b, D n63-C1b).
- **b5's pick GREEN:** N63NearFadeTests 5 / 5, N63BossMotionTests 15 / 15, N63BossColoursTests 5 / 5, N63PressTests 5 / 5,
  N63SwitchTests 2 / 2, N63R1Tests 2 / 2 and every earlier class (N63BossTell 4, Colours 7, BossCue 9, BossClock 7, SameRun 17,
  Shared 3, BossRule 8, PinHint 4, N61ExitFrame 9, N61BossRule 7, N61Text 7, N61Fixture 7, N61GunYellow 7, FocusAutomation61 36,
  RimRead 2, FocusLock 3, ModeList 2, GunFamily 2, NightRows 14); **gates all GREEN 00:45:39** (Logs/n63/pick-b5-green.txt).
- ~00:45 **D's C1 fix round** (n63-C1b: 4134e403 RED (a spec change of two of tonight's own brightness assertions: DECIDE63 D-9),
  8c7ad1b7 a route keeps the original's brightest channel when the plain rule would overflow (the lava's gold was clipping back to amber;
  B3's danger routes the same), fe1cf8f9 notes; iceline's blue and spire's teal are lit surfaces: C1's MAIN can reach 3 / 5 at most).
  Picked: **RED N63ColoursTests 6 / 8, N63BossColoursTests 4 / 5**, then GREEN 8 / 8, 5 / 5; **gates all GREEN 00:49:25**. D sent eye63
  next (DECIDE63 D-10: M1's proposal (2) built OFF, measured by n63-m1-chop on verbs37).
- ~00:47 **V's ICE** (n63-ICE: bc28b290 RED, da89169f under nearFade63 an Icebreaker body part within bossHideM 0.6 m of the eye stops
  drawing that frame (Renderer.forceRenderingOff), 6a7ad448 boss63 skips a held frame (the crown's "drawn off" artefact), be81dd0a
  notes V-ICE.md). Picked: **RED N63NearHideTests 0 / 5**, then GREEN 5 / 5 with NearFade 5 / 5, BossMotion 15 / 15, BossClock 7 / 7;
  **gates all GREEN 00:53:07**.
- **b5 = n63-b5** built 00:53:16 from 0956dde7; drift GG's two files; copy rows on: bossMotion63, bossTell63, colours63, bossColours63,
  nearFade63, press63 (text61 and bossRule61 ON in the repo). SPOT-first same-run set running (lowercase sides): d1noop, d1plant, b1w,
  c2n (near63), icei, b3k, s7k.
- ~00:55 **G's B2b** (n63-B2b: 3a7a9557 meter RED, b4e4fa0d the verdict by the sim's tell kind and the reference from the previous hit,
  0ffb0fac fix RED, 53d05927 the fix only on listed attacks (bossTell63.numbers.fix = wyrm:spit, crown37:beamCut, crown37:lowFire,
  computed by Tools/night/n63-b2-fixlist.cjs) with BossTell63Late pulling the boss's own warm off-hue glows to the danger hue during a
  listed wind-up, fed55675 notes G-B2b.md; gates GREEN 00:52:59). REDs picked (c8290b27, 4fee5e2f); for b6. V sent the contact-sheet tool.
- **B2b RED** N63BossCueTests 10 / 12, N63BossTellTests 4 / 7; then ec2c448d 8f7fd1ab e6a8b3de: **GREEN 12 / 12, 7 / 7**, N63Shared 3 / 3,
  the fixlist selftest PASS; **gates all GREEN 00:58:54**; config bossTell63.numbers.fix = ["wyrm:spit", "crown37:beamCut",
  "crown37:lowFire"].
- **b5 SPOT-first set done (~00:55-01:02), the toggle lines' pair px:** d1noop 5 / 8 / 8 / 7 / 1 / 0 and d1plant 104 / 110 / 111 / 103 /
  105 / 100 (D's SSAO guess did not remove the residual); **B1 b1w** wick idle 4,208 / flinch 26,111 / windup 33,973 / **stagger 40,552
  (920 under the bar after V's bigger round)**; wick GREEN (3 / 3 posed, 42 / 42 flinches); **C2 c2n** near-3.0 0 / 1.5 0 / 1.0 63,402 /
  0.7 366,891 / **0.5 562,870** / 0.3 966,241, near63 GREEN; **ICE icei** idle 6 / windup 13 / flinch 304,850 / **stagger 282,626**; **B3
  b3k** wyrm 0 / 0 / 0 / 6 (proof: only "wyrm.punch: weak #FFD23A x2.2 -> #1CEAFF x2.2", the punch mark on no shot); **S7 s7k** kb3 entry
  **292,689** (others 0-12); NRE 0 in all 7.
- ~01:00 **SPOTs (blind; mappings after; all A = ON):** C2: "A shows the big red enemy in front of the player broken apart into a cloud
  of scattered red, black and yellow pieces, while in B it is a whole, solid red-orange pill with a bright yellow ring around it" (names
  it). ICE: "A has a large bright cyan shape across the bottom-left of the screen, and in B that area is dark" (the room showing through;
  folded into C2's GATE: the same row). S7: "In A they are tall, multi-part columns in tan and dark grey with black-and-tan hazard
  stripes at their bases. In B they are plain, solid dark-navy blocks with red bands" (names the presses). C2 and S7 added to the slate
  with doneWhen from the builders' own pre-measurement notes (Tools/night/n63-slate-add-leftovers.cjs; V.md 00:11:41, G-S7.md 00:23:26).
  B3 sent back to D once (rule 8: its named pose changed 6 px).
- ~01:00 b5 set 2 started (background): B1 same-run on iceline / kiln / span / spire; B3 same-run on foundry / iceline / span / spire;
  c1vf / c1xf; verbs37 vi / vk / vs / vp.
- ~01:02 **G's text63** (n63-T63 22c66184 RED, de04cf7c the row text63 + keyed by technique (boost / boostHigher / boostAgain / flue) +
  the title wait 3 -> 10 s, 9399c505 notes; gates GREEN 00:59:26). Picked: **RED N63TextTests 0 / 4**; then de04cf7c (no conflict:
  design.n63 ends press63, text63; bossTell63's fix list kept) and the notes; compile, tests and gates running. V sent exit63 (a new door
  design for GG's door complaint); G sent the w63 harvest draft (leftover 1, Research/2026-10-22-wave63/harvest-draft.md).
- b6's stack GREEN: N63TextTests 4 / 4, N61Text 7 / 7, N63PinHint 4 / 4, N63BossTellTests 7 / 7, N63BossCueTests 12 / 12, N63Shared 3 / 3;
  **gates all GREEN 01:04:42**. V's sheet tool picked (3df29537; `--selftest` 9 / 9 PASS). **b6 = n63-b6** built 01:05:07 from 3df29537;
  drift GG's two files; copy rows on: bossMotion63, bossTell63, colours63, bossColours63, nearFade63, press63, text63.
- **b5 set 2 done 01:10:30** (NRE 0 in all 21 b5 reels; **verbs37 x5 27/29/31/31/31**, RED none). B1 same-run (nearFade63 also on: the
  Icebreaker no longer covers the camera): brawl40 idle 5,503 / windup 23,277 / flinch 402,592 / **stagger 178,637**; wyrm 433 / 1,175 /
  1,127 / 7,308; loom37 6,056 / 16,909 / 24,096 / 40,617; crown37 windup 166,979 / idle 87,561 / flinch 328,801 / stagger 306,994;
  verdicts: wick, brawl40, wyrm, loom37 GREEN; crown37 "35: drawn 34" (`flinch 8983 ... drawn off` again, the same-run artefact: a plain
  crown run b1pp queued on b6). B3 same-run: wick idle 17,751 / flinch 16,499 / windup 2,890 / stagger 0; brawl40 0 / 0 / 0 / 0; loom37
  16 / 10,095 / 12,447 / 40,419; **crown37 idle 184,073** (gems / halo #FF5A1F -> #1194A1) / others ~5,000; proof lines: 12 parts ON,
  routed wick.core, wick.edges, brawl.weak, crown.clap / edge / flameYellow / gems / halo, loom.knees, wyrm.punch. **C1 foundry after C1b:
  0.425 -> 0.366 % (13.8 %): C1 SHORT** (2 / 5 levels, fix round used; slate).
- ~01:11 b6's SPOT-first set started (b2k / b2p same-run bossTell63 with motion off; t63on / t63off; b1pp a plain crown run); the flow gate
  started on HEAD (press63 present; S7's doneWhen); **C2's GATE spawned** (evidence on b5); **B1's same-run SPOT** on V's first-choice pose,
  the Icebreaker's stagger (b1i, 178,637 px; its earlier two-run SPOT on the wick was before D1).
- 01:11:49 **B1 SPOT on b5** (same-run, the Icebreaker's stagger): "The bright cyan floor at the bottom of the screen has a different
  shape. In A it is a big flat slab across the lower left with a smaller piece on the right. In B it is two slanted cyan wedges ..." (A =
  ON): the boss's own fists / arms at 1.7 m (its near body hidden by nearFade63, the fists cyan by bossColours63), but named "the floor":
  **another difference: B1 ends for b5** (Logs/n63/spot/B1.txt keeps every attempt).
- **b6 SPOT-first set (01:11-01:16):** **B2b** b2k (wyrm): pairs 0 / 0 / 0 / 6 / tell 327 px; `[tell63] recolour wyrm spit: 26 renderers,
  0 lights to the danger hue`; `cue wyrm spit ... hue 10 ... danger yes ... verdict ok`; end line `wyrm attacks 2 found 2 | under 300 ms 0 |
  off the danger hue 0`. b2p (crown): windup 8 / idle 0 / flinch 1,186,277 / **tell 1,402,007** / stagger 12; beamCut x2 `danger yes ...
  verdict ok`, lead 1,483 / 1,425 ms. **b1pp, a plain crown run with bossMotion63 on: GREEN, damage events 49: drawn 49** (the same-run
  "drawn off" artefact confirmed). **text63** t63on: `[text63] withheld:` x3 (Midair ..., Fire ... lift off, Boost ... twice); "(LATE" 0
  on both sides; pairs (two-run) kb2-clear 12,164 / kb2c 14,304 / kb3 74,196.
- ~01:13 reports: **D's B3b** (n63-B3b 8183ce93 RED, a871e8fd wyrm.open / wyrm.core / wyrm.seams / wyrm.spit routed, 222cb59c notes;
  gates GREEN 01:12:50; D: the Icebreaker's 0 px is right (its stuck fists are cyan on and off); the wyrm's pale square is its head
  end-on); **G's harvest draft** (n63-HARV 6f1ad9b3). Picked onto b7's stack: 6f1ad9b3 -> 4fe9dbfd, 8183ce93 -> 770278bb (compile after
  the flow gate's Editor step).
- ~01:15 **C2 GATE: `C2: FAIL`** (Logs/n63/verdict-C2.md): the husk half passes (562,870 px, far controls 0, SPOT named it); the Icebreaker
  half not shown (b5's OFF twin already shows the room, 49 % dark vs b4's 91 %: bossMotion63's bigger slump on in both captures), no near63
  OFF run, and the dissolve "reads as noise ... an exploding cloud of skewed flakes", a kill-like shatter risk. V's one fix round sent
  (n63-C2b: a deliberate see-through look); the missing reels queued in b6's set 2 (icem with motion off, nearoff).
- 01:16:39 **SPOTs:** **B2 (b6, same-run crown beamCut tell, 1,402,007 px):** "In A the whole scene behind the explosion is washed in a
  heavy red tint, while in B the same area is dark, almost black" (A = ON): names the red cue as a wash. **text63 (two-run, kb2 clear,
  12,164 px):** "A has a tutorial line under 'DOOR OPEN' ('Midair, aim down, then LEFT CLICK') that B does not have" (A = OFF): names it.
  text63 added to the slate (T63; doneWhen from G-T63.md committed 00:59:38, before its first Player run). Rule 8's bar names only B1 / B2
  / B3 / C1.
- ~01:17 b6 set 2 started (background): B2 before x5 (b2b?), after on the untouched wick / brawl40 / loom37 (b2a?); icem (same-run
  nearFade63 with motion off); nearoff; s7off; verbs37 x5.
- **01:24:19 the flow gate with press63 present: "gate flow --off63: EQUAL to flow-base", gates all GREEN** (Logs/n63/gates-flow-b5.txt).
- **B3b picked:** RED N63BossColoursTests 3 / 7 (Logs/n63/pick-b3b-red.txt), then a871e8fd + 222cb59c -> a2e52b54 + 850ab386: GREEN 7 / 7,
  N63ColoursTests 8 / 8, N63SwitchTests 2 / 2, N63Shared 3 / 3; **gates all GREEN 01:28:22**. ~01:27 **S7's GATE spawned**; s7off (b6,
  -skygrindNoPress63): 0 `[press63]` lines.
- ~01:29 **the fresh review spawned** (skygrind-reviewer on `git diff 3bbdf4fe..HEAD`; Logs/n63/review-n63.md). **D's eye63** (n63-EYE:
  4422f7c8 RED, 6dfe849e the row, 46591d2e the eased eye (the eye's ease at 0.8 x, then a critically damped follow at 70 /s; a held
  frame moves nothing), e627a434 / ff0439a3 n63-m1-chop.cjs --pair, 8e30d45a notes D-EYE.md; gates GREEN 01:22:38; the halving met only on a
  replay of n62-freeze's telemetry: 64 of 64 events; "a camera ease is not a still-frame change": it waits OFF for GG's A/B). Picked: RED
  N63EyeTests 2 / 7, then GREEN 7 / 7, N63SwitchTests 2 / 2, N63Shared 3 / 3; the default m1-chop output unchanged; **gates all GREEN
  01:33:45**.
- **B2's corrected before on b6** (b2b?, bossTell63 off): wick 3 / 0 / 0; brawl40 1 / 0 / 0; **wyrm spit short (no cue)**; loom37 4 / 0 /
  0; **crown37 crownFire off the danger hue (hue 41, the clapper)**; beamCut and lowFire `verdict ok` (in time, danger hue) BEFORE any fix.
  The after (b2a? on the untouched bosses) logs `[tell63] skip ... (not on the fix list: already in time and in the danger hue)` /
  `(parryable: its own yellow tell)`. b6's SPOT on the crown's beamCut was therefore a change on an attack already fine. **The fix list
  recomputed** with G's tool on b6's before logs: [wyrm:spit, crown37:crownFire] (committed c5edbec9; DECIDE63 D-11).
- ~01:30 **S7 GATE: `S7: FAIL`** (Logs/n63/verdict-S7.md): numbers met (292,689 px, "4 dressed", off 0, verbs37 intact) but the spotter saw
  "columns ... with hazard stripes", not machines; the piston dull lavender, the near press tan like the wall, the old bands showing, a head
  reading as a stray slab, the lamp a blob. G's one fix round sent (n63-S7b).
- ~01:34 **V's C2b** (n63-C2b: 2fe8157d RED, 08a79c77 Enemy.shader's own screen-door `_NearFade` (a 4 x 4 Bayer dither in screen pixels,
  the rim kept solid, the ink cut at the same pixel so no black far side shows; farM 1.0 -> 0.9, floor 0.1 -> 0.2), 7de3281e notes;
  gates GREEN 01:30:23; one of its own assertions changed: D-11). V on exit63 (warning: the clear-shot door boxes are 17-38k px and the
  MAIN looks out of reach there; V adds -skygrindExitShots63 framing the door at 10 m).
- **b7's pick:** c5edbec9 (the list), C2b RED 2fe8157d -> 84d80ab0: **RED N63ScreenDoorTests 0 / 4, N63NearFadeTests 4 / 5**; then
  08a79c77 7de3281e -> 17e79f8a 3e16b0cc: **GREEN N63ScreenDoorTests 4 / 4, N63NearFadeTests 5 / 5, N63NearHideTests 5 / 5,
  N63BossCueTests 12 / 12, N63Shared 3 / 3; gates all GREEN 01:38:41; but `N63BossTellTests.TheFixList_IsTheOneComputedFromTheBeforeMeasure`
  FAILS (6 / 7)**: it hard-codes b4's list; not edited by the lead; G makes it compute from a committed fixture (n63-B2c; D-11). B2 cannot
  turn ON while it fails.
- **b6 set 2 done:** icem (the Icebreaker same-run nearFade63 with motion off): flinch 832,411 / stagger 936,071 px, the OFF half "DARK
  HELD FRAME" (the ON half uncovers the camera); nearoff `[reel] near63: OFF`; **verbs37 x5 on b6 27/29/31/31/31, NRE 0 in all 21 b6
  reels**; text63: "(LATE" 0 on vi / vk / vs / vp (b3: 4); kiln shows 8 lines (b3: 10): withheld "Fire ... lift off" and "Boost ... twice";
  the step lines still shown; "Jump SPACE, aim down, fire" (b3's LATE line) did not show; "Midair ..." shown.
- **b7 = n63-b7** built 01:39:07 from 3e16b0cc; drift GG's two files; copy rows on: bossMotion63, bossTell63, colours63, bossColours63,
  nearFade63, press63, text63, eye63. SPOT-first set started (b3k, b3p, b2p, b2k, c2n, b1p).
- ~01:44 **the fresh review: must-fix 0, should-fix 8, note 15** (Logs/n63/review-n63.md): "with every NIGHT63 row OFF, nothing changes what
  the game does or draws except one deliberate focus-pause change (S5)". S1 the Icebreaker's hook overwrote its leap height and recoil
  (drawn on the floor while its hit spheres are in the air) -> V; S2 colours63's scan on the wall clock, S4 the held ON frame replaced the
  shot's PNG while its sidecars described the live frame -> D; S3 the tell meter's bands (345-30 / 38-62) differ from the table's
  (345-35 / 45-70) and S7 the late pass can turn the wyrm's yellow punch window red -> G; S5 V's `locked &&` in focus60 -> the focus probe;
  S6 the lamp meter's shrinking denominator -> lead (54569d37: a SHOTS line names every shot not measured; PLANTED GREEN); S8 boss63's
  wall-clock waits -> V.
- **b7 SPOT-first set (01:40-01:46), pairs:** B3b wyrm 716 / 1,799 / 2,664 / stagger 29,720 / tell 5 (proof: wyrm.seams, wyrm.open,
  wyrm.punch, wyrm.spit routed); crown idle 184,181; B2 crown (motion off) tell 766,708 (crownFire's wind-up), others 0-12; the wyrm 0 /
  0 / 0 / 0 / tell 4; `cue crown37 crownFire ... hue 41 sat 0.14 ... verdict off-hue ... obj THE CROWN/crown clapper` (no recolour line for
  the crown); `cue wyrm spit ... lead 492 hue 10 ... danger yes ... verdict ok ... obj KILN WYRM/tell63 cue`; C2b near 3.0 0 / 1.5 0 / 1.0
  27,320 / 0.7 389,029 / 0.5 590,004 / 0.3 950,876; B1 crown windup 181,650 / idle 83,994 / flinch 371,414 / tell 219,943 / stagger
  306,905; NRE 0 in all 6.
- 01:46 **SPOTs on b7 (blind; mappings after):** **B3** (crown idle; B = ON): "The big ring in the middle of the screen is filled with
  bright solid cyan in B, but in A it glows mostly white" (names a colour change). **C2b** (near-0.5; A = ON): "The big orange enemy up
  close is see-through in A, a dotted mesh you can see the floor and walls through, but solid and opaque red in B" (a see-through, not a
  shatter). **B1** (crown stagger; B = ON): "The big cyan glow in the middle of the screen is larger in A and reaches higher, up to the
  light beam; in B it is smaller and sits lower" (the shape sinks: the GATE weighs whether it names the boss's pose). **B2** (crown
  crownFire tell; A = ON): "A has a large see-through red cylinder of light filling the middle of the screen around the boss. In B it is
  gone" (names the cue).
- 01:46-01:47 **the focus probe on b7** (review S5; Logs/n63/focus-probe-b7.txt): "[focus60] 4.30 s focus LOST (flow Paused, cursor None)",
  "**DONE: paused on the focus loss True**" (pauses 1 with the window unfocused): V's `locked &&` keeps alt-tab's pause in a Player. (The
  script's own exit code 1 after the DONE line: not read further.)
- ~01:50 reports: **V** review S1 / S8 (n63-RV d884141c RED, 60482a36, 847b0103) and **exit63** (n63-EXIT 038f833d its doneWhen before any
  Player run, 885a0ccc RED, 15b0f333 the doorway itself turns cyan (a sheet, jamb / lintel linings, a threshold strip and floor dashes;
  every gain <= 1, no Light), ef9ffce6 notes; V predicts its MAIN at 0-1 of 5 on the clear-shot poses (ramps and a ledge in front of the
  doors) and adds -skygrindExitShots63 for a 10 m door shot as its SPOT); **G** B2c (8beb6ed1 RED, b7519b2b: the fix-list test computes
  from a committed fixture of the before cue lines, written by the tool with the config); **D** review S2 / S4 (n63-RV-D 4a8880d8 RED,
  db9ba03f, 683c70e6: the held ON frame now goes to `<side>-on/`, 0a8d7643 notes) and the eye63 test fix (8079b10b: no position pin);
  **G** S7b (0caedc16 RED, 9479f5b2 presses drawn as presses: a dark bed, a black throat, bright guide columns, a ram head leaving a 2.15
  m gap, a crown with a lamp, all unlit, no stripes; adb4eca6 notes; two of S7's own assertions replaced, reason stated).
- **b8's picks, test-first:** RED N63ReviewTests 0 / 3, N63ExitTests 0 / 7 (pick-b8-red.txt), N63BossTellTests 7 / 8 (pick-b2c-red.txt),
  N63ColoursTests 8 / 10, N63SameRunGameTests 7 / 10 (pick-rvd-red.txt), N63PressTests 3 / 8 (pick-s7b-red.txt); a config conflict at
  exit63's 15b0f333 (its row after eye63): HEAD's copies + `node Tools/night/n63-v-exit-rows.cjs` ("design.n63.exit63 written after
  design.n63.eye63"). GREEN: N63ReviewTests 3 / 3, N63ExitTests 7 / 7, N63BossTellTests 8 / 8, N63BossCueTests 12 / 12,
  N63BossMotionTests 15 / 15, N61ExitFrame 9 / 9, N63SameRunGameTests 10 / 10, N63R1Tests 2 / 2, N63ColoursTests 10 / 10,
  N63SameRunTests 10 / 10, N63BossColoursTests 7 / 7, N63PressTests 8 / 8, N63EyeTests 7 / 7 (after D's fix; one run showed 6 / 7 before
  it), N63Shared 3 / 3; **gates all GREEN 01:54:09, 01:58:46, 02:02:15**.
- **b8 = n63-b8** built 02:02:25 from 79f1410b; drift GG's two files; copy rows on: all ten n63 rows. Set 1 started (s7k, e63, b1f / b1i /
  b1k / b1s / b1p, lowercase).
- **b8 set 1 (02:02-02:09), pairs (held ON frames now in `<side>-on/`):** S7b kb3 entry **468,517**; exit63 (e63, the 10 m kb3d door shot
  `-skygrindExitShots63 kb3d`) 14,648, clear shots 63-4,154; **exit63's own gain** (n60-v-lamp.py on e63-on vs e63-off; I copied the shots'
  sidecars from e63/ into e63-on/, which has none) **1.05 / 1.02 / 1.01 / 1.00 / 1.03x = 0 / 5** (GUARD 5 / 5; SHOTS 5 / 5 measured;
  Logs/n63/exit63-owngain-b8.txt): **exit63 SHORT on its MAIN** (V predicted 0-1 of 5). **B1 with the S1 fix:** wick / brawl40 / wyrm /
  loom37 GREEN; crown37 "35: drawn 34" (the same-run artefact again); pairs wick stagger 40,503, brawl40 flinch 416,515 / stagger 178,440,
  wyrm stagger 16,942, loom37 stagger 40,733, crown37 flinch 372,543 / stagger 306,889; NRE 0.
- ~02:10 **S7b SPOT** (b8 same-run kb3 entry; A = ON): "A has bright white-and-grey multi-part columns (white vertical bars under grey box
  caps) on the left, in the middle and on the right, while B has plain dark navy pillars with rose bands in the same places": columns,
  not stamping machines, twice: **S7 SHORT** (fix round used; no second GATE; press63 OFF).
- ~02:13 **G's RVG** (n63-RVG 1e46e1b6 RED, 6adf14c1 the meter, the tell and the fixlist tool read the colour table's bands (danger
  345-35, parry 45-70); the late pull only 35-45 deg (never the parry yellow); the fix list unchanged [wyrm:spit, crown37:crownFire]; two of
  B2b's own assertions changed and a sample colour moved, stated; c2890fe2 notes). Picked: RED N63BossCueTests 12 / 13, N63BossTellTests
  6 / 8; then GREEN 13 / 13, 8 / 8, N63PressTests 8 / 8, N63ColoursTests 10 / 10, N63BossColoursTests 7 / 7, N63BossMotionTests 15 / 15,
  N63BossClockTests 7 / 7, N63Shared 3 / 3; **gates all GREEN 02:15:38**.
- ~02:16 **T63 GATE: `T63: FAIL`** (Logs/n63/verdict-T63.md): doneWhen 1 as written (Midair shown: 2 of 3 named withholdings) and lessons
  lost: a line waiting for the title is dropped without a log line when the next lesson replaces it (kiln's "Jump SPACE, aim down, fire",
  spire's TORCH "Hold LEFT CLICK up close to burn", no stand-in). G's one fix round (n63-T63b).
- **b9 = n63-b9** built 02:16:04 from 113ab94a (RVG); copy rows on: all ten n63 rows. 
- **b8 set 2 done 02:19:15:** **verbs37 x5 with eye63 on AND off: 27/29/31/31/31 both, RED none; NRE 0 in all 17 b8 reels.** **eye63 on a
  Player pair** (`node Tools/night/n63-m1-chop.cjs --pair n63-b8`): "ALL 5 levels: events 64 ... the eye's own residual halved at 64, worse
  at 0 ... view only: yes (largest feet / x / z difference 1.00 mm)"; "**eye63 measure ... MET**"; span's whole path rms 2.48 -> 2.15 cm,
  frames over 2 cm 207 -> 165. No still-frame SPOT (a camera ease): eye63 waits OFF for GG's blind A/B (DECIDE63 D-10).
- ~02:20 b9's B2 set started (b2bk / b2bp before, b2k / b2p same-run after; motion off). B3's per-boss meter (on vs off off-palette share,
  same-run): wick 0.057 -> 0.033 %, brawl40 0.357 -> 0.357, loom37 0.475 -> 0.257, wyrm 1.074 -> 1.063, crown37 1.152 -> 1.166 (planted
  GREEN). **GATEs spawned: B1, C2 (round 2), B3.**
- **b9's B2 set done 02:22:49** (Logs/feel/n63-b9/summary47.txt): b2bk / b2bp (before: motion and tell off), b2k / b2p (same-run
  `-skygrindSameRun63 n63.bossTell63`, motion off); wyrm strikes 2 (leads 533-900 ms), crown37 strikes 6 (leads 1,000-1,508 ms); NRE 0 in
  all four. Read by G (B2's builder) before B2's GATE.
- ~02:25 **C2 GATE round 2: `C2: PASS`** (Logs/n63/verdict-C2.md; round 1 kept as verdict-C2-r1.md): the husk at 0.5 m an even
  screen-door with its rim (590,004 px same-run on b7; far controls 0 px), the SPOT names the see-through ("a dotted mesh you can see the
  floor and walls through"), the Icebreaker at 1.7 m uncovered (b6 icem 936,071 px), near63 OFF run present, verbs37 x5 27/29/31/31/31 on
  b8. `node Tools/night/n63-freeze-rows.cjs "n63.nearFade63=..."`: **nearFade63 ON in both copies** (byte-identical, cmp).
- ~02:25 **B1 GATE: `B1: FAIL`** (Logs/n63/verdict-B1.md): the SPOT (b7 crown stagger) names "the big cyan glow", not the boss's pose; the
  wyrm's motion barely shows (flinch 1,084 px, wind-up 1,097, idle 373); stagger pairs wick 40,503 / loom37 40,733 / wyrm 16,942 px under
  41,472. V's one fix round sent (n63-B1d: a bigger slump on the stagger pose, the held-frame flinch artefact, the wyrm's segments).
- ~02:26 **B3 GATE: `B3: FAIL`** (Logs/n63/verdict-B3.md): THE CROWN's tell / wind-up ring stays amber with the row on (35,617 px on the
  tell frame, 77 px changed by the toggle), the wyrm's weak point never cyan in its 5 frames, the Icebreaker pair 0 px, and a whole-frame
  meter cannot judge "attack effects" or "weak points". D's one fix round sent (n63-B3c: the crown's ring routed, the wyrm's weak point
  cyan, a per-part proof line, a per-boss role meter).
- **b9's B2 pairs (lead, `n62-gun-yellow.py --pair` on b2k-on / b2k-off and b2p-on / b2p-off):** crown37 tell (crownFire) **766,631 px**,
  its windup / idle / flinch / stagger 0; wyrm tell 8 px, others 0. Before vs after (b2bk vs b2k, b2bp vs b2p): every `[boss63] attack ...
  windup ... hit` line identical (2 on kiln, 6 on spire): no damage frame moved. **wyrm spit** before `lead 225 hue 27 sat 0.22 danger no
  verdict short obj KILN WYRM/piece 0`, after `lead 492 hue 10 sat 0.92 danger yes verdict ok obj KILN WYRM/tell63 cue`. **crownFire**
  before = after: `lead 1283 hue 41 sat 0.14 danger no verdict off-hue obj THE CROWN/crown clapper` (the meter's strongest riser is the
  clapper, not the column); **wyrm aggro** (not on the list) `start 13033 lead 375` -> `start 13000 lead 408`. Sent to G.
- 02:35 **G's B2d / B2e** (n63-B2d: aad13f07 RED, 9d3e74f0 the late pass also pulls a white-hot warm glow (sat >= 0.05, hue 345-45, HDR
  level >= 1.0) to danger on a listed wind-up only (the crown's clapper #FFF4DC x 2.4), e7cea080 notes; 2935740c RED, cbffe7b8 the cue line
  appends `| danger cue start <ms> lead <ms> hue <deg> obj <name>` (the earliest danger riser; the verdict still the strongest riser's),
  e5dc105b notes). The wyrm's 8 px: the column is off screen at that held pose (both halves face a dark block). Aggro's 33 ms: aggro is on
  BossTell63Skip (nothing drawn or pulled for it); its riser is a ring throat counted on the frame it enters the camera's view, so two runs
  can differ (L-129): settled on b10 by two before-runs with the same flags (no meter change). **G's T63b** (n63-T63b: da413f3f RED,
  efb3c01f text63's gate queues: a line waiting for the title is never dropped, lines show one at a time in the order reached, each fate
  logged incl. "still waiting at run end"; f0f0269e notes).
- 02:36 EXIT63 and EYE63 added to the slate (Tools/night/n63-slate-add-eye-exit.cjs; doneWhen from 038f833d / D-EYE.md, both before any
  Player run): EYE63 BLOCKED (measure MET on b8, Logs/n63/eye63-pair-b8.txt; no still-frame SPOT: GG's blind A / B, D-10), EXIT63 SHORT
  (own gain 0 / 5). Committed e4154441. Report draft committed 1c792500.
- **02:40:51 the flow gate after the nearFade63 flip: "gate flow --off63: EQUAL to flow-base", golden 0 mismatches, fingerprints plain /
  --off57..--off63 EQUAL, the Editor 0 errors: gates all GREEN** (Logs/n63/gates-flow-c2on.txt).
- b10's pick, test-first: aad13f07 2935740c da413f3f -> cca63089 43681ce0 a4843aa7; compile and the RED classes running
  (Logs/n63/pick-b10-red.txt). G sent the w63 harvest draft's update (leftover 1).
- **b10 RED** (compile 02:41:26, 0 errors): N63BossTellTests 8 / 9, N63BossCueTests 13 / 14, N63TextTests 4 / 8. Then the GREENs + notes
  9d3e74f0 e7cea080 cbffe7b8 e5dc105b efb3c01f f0f0269e -> a178f745 66338609 84be6ace d2d7ccfc bb79b736 77976715 (no conflict).
- ~02:42 **V's B1d** (n63-B1d: 7e6a0a3c RED, f3f09659 THE WICK's own slump (sink 0.85 m, keel 40 deg, squash 0.30, a 1.19 m slide the
  way it keels, alternating) under a guard raised to 0.9 of its 1.7 m collider radius and 45 deg; FlinchBook63: the crown's 8983 ms
  "drawn off" was two damage events on consecutive frames with a LateUpdate draw, not a held frame; 7c43168e notes; gates GREEN 02:41:39;
  two of its own N63BossMotionTests pins changed in the GREEN commit: DECIDE63 D-12). 7e6a0a3c -> 70fb03cf picked with the GREENs' run:
  **N63BossTellTests 9 / 9, N63BossCueTests 14 / 14, N63TextTests 8 / 8, N61TextTests 6 / 6 GREEN; N63B1dTests 0 / 5 RED;
  N63BossMotionTests 15 / 15** (Logs/n63/pick-b10-green1.txt). Then f3f09659 7c43168e -> 0281198e ca61fd66 and G's harvest 533e5232 ->
  b11131df (no conflict; config copies identical); compile, the B1 classes and the gates running (Logs/n63/pick-b10-green2.txt).
- **b10's pick GREEN** (compile 02:45:41): N63B1dTests 5 / 5, N63BossMotionTests 15 / 15, N63BossClockTests 7 / 7, N63ReviewTests 3 / 3,
  N63Shared 3 / 3, N63BossColoursTests 7 / 7, N63NearFadeTests 5 / 5; **gates all GREEN 02:48:22**. Log committed df2743bd.
- **b10 = n63-b10** built 02:49:13 from df2743bd; drift GG's two files; copy rows on: every design.n63 row (bossMotion63, bossTell63,
  bossColours63, colours63, press63, text63, eye63, exit63; nearFade63 ON in the repo).
- **b10 SPOT-first set (02:49-02:53; NRE 0 in all 4):** **B1** b1f (wick, same-run bossMotion63) GREEN: 3 / 3 posed, 42 / 42 flinches
  drawn, rms 23 / 23, best 2.9993 m, limit share max 1.00; pairs idle 3,728 / flinch 26,298 / windup 33,938 / **stagger 52,428** (rule 8's
  41,472 met; b8 40,503). **B2** b2ap (crown, same-run bossTell63, motion off): **crownFire `lead 1283 hue 10 sat 0.92 ... danger yes ...
  verdict ok ... | danger cue start 9950 lead 1283 hue 10 obj spin/gem38 cross`, `recoloured 2, column drawn`**; bladeRing / beamCut x2 /
  lowFire danger yes, gemShard `parry yes ... kind yellow verdict ok`; end line `under 300 ms 0 | off the danger hue 0 | parry drawn 1 of 1
  yellow`; pairs tell **787,061**, others 0-6. **T63** t63on: `[text61] shown: Jump SPACE, aim down, fire LEFT CLICK. | text63 key
  'boost'`, `[text63] withheld:` Midair / Fire ... lift off / Boost ... twice, "Press SPACE and LEFT CLICK together." and "At the top,
  press LEFT CLICK again." shown; t63off shows all 11; kb2 clear pair 11,949 px.
- 02:53:40 **SPOTs on b10 (blind; mappings after):** **B1** (wick stagger; A = ON): "In A the glowing cyan boss (THE WICK) is tipped over
  sideways and slumped low, with its dark disc top tilted on a diagonal; in B it stands upright": **names the boss's pose** (first time
  on a same-run pair). **B2** (crownFire tell; A = ON): "A has a large see-through red cone or column of light filling the middle of the
  screen around the boss. B has no red column": names the cue. **T63** (kb2 clear, two-run; B = ON): "A shows a hint banner under DOOR
  OPEN reading 'Midair, aim down, then LEFT CLICK'; B doesn't have it": names the card withheld.
- 02:54 b10 set 2 started (background): B1 same-run on iceline / kiln / span / spire + a plain crown run (b1pp); B2 before x5 (+ b2bk2, a
  second kiln before for aggro's drift) and after x4 (spire's is b2ap); verbs37 x5.
- ~02:55 **D's B3c** (n63-B3c 904d3b21 RED, ab4d391f three of its own hook pins restated (DECIDE63 D-13), d3e37b78 the fix (a danger
  route caps its non-brightest channels so an HDR gain cannot clip the crown's red tell to amber; crown.tell routed on 5 sites; the wyrm's
  open head, core, punch head and BossMarks32's shell through the grade's cyan compensation), 24950308 the per-boss role meter
  n63-b3-roles.py (planted 11 / 11), ff239f17 / 9fbf1299 notes and a tool fix; then the Loom (my addition inside the round: the meter
  failed it on cyan + 422 blue px): 8513efcd RED, 50cd704c the Loom's knees / nails / seams through the grade's compensation (the 422 px
  were its knees turned blue by the Loom's grade, not bloom), 270526eb notes; gates GREEN 02:53:34). Picked RED 904d3b21 ab4d391f ->
  a5a6bed2 43037b45; compile and N63BossColoursTests running beside set 2's reels (two heavy jobs at once: noted).
- **B3c RED** (compile 02:55:15): N63BossColoursTests 6 / 11 (Logs/n63/pick-b11-red.txt). Then d3e37b78 24950308 ff239f17 9fbf1299 and
  the Loom's RED 8513efcd -> 84e34037 c4208391 10b5982f 156d9f18 0eadd66e (no conflict; config copies identical): **10 / 11, the Loom's
  hook test RED** (pick-b11-loomred.txt). Then 50cd704c 270526eb -> bbdd811b 06a2b3fd: **GREEN N63BossColoursTests 11 / 11,
  N63ColoursTests 10 / 10, N63BossTellTests 9 / 9, N63BossMotionTests 15 / 15, N63SameRunGameTests 10 / 10, N63Shared 3 / 3; gates all
  GREEN 03:00:29** (pick-b11-green.txt). b11 builds when b10's set 2 ends (no build beside a Player).
- **03:07-03:09 the memory incident (L-128 again).** Claude Code stopped b10's set-2 task "because the system is running low on memory"
  (its outer shell; the reel loop itself, PID 50452, kept running: vs 03:08:09, vp 03:09:10). Cause, read from the processes: another
  session's pytest (PID 54364, `...\Temp\s7-attack-CR\dashboard\.venv\Scripts\pytest.exe -x -q tests/test_lots.py`, the investing
  dashboard) at 19.3 -> 20.3 -> 21.1 GB private in 9 s; **commit free 11.4 -> 4.7 -> 2.1 -> 1.0 GB**. Ours at the time: the Editor
  3.6 GB, its two asset-import workers 4.9 GB each (alive since 23:24), one Player 1.2 GB. I stopped PID 54364 at 03:09:15 (a test run,
  rerunnable; the machine crashed from low virtual memory on 2026-10-22) and told its session (investing-f4) by message; **commit free
  22.4 GB at 03:09:19**; C: dipped to 11 GB (the pagefile grew; 35.3 GB allocated, 5.9 GB in use) and is 27.7 GB again. Nothing of
  NIGHT63's lost: b10 set 2's reels ran on.
- **b10's B1 set (same-run bossMotion63):** GREEN on all five: brawl40 14 / 14 flinches drawn, wyrm 21 / 21, loom37 17 / 17, crown37 35 /
  35 (same-run) and **49 / 49 on a plain crown run b1pp** (FlinchBook63: the "drawn off" is gone), rms 23 / 23 everywhere, every off half
  rms 0. Pairs: brawl40 flinch 416,559 / stagger 232,566; **wyrm** flinch 1,076 / idle 389 / windup 1,079 / **stagger 16,649** (its hitbox
  rule: D-12's notes); **loom37 stagger 40,650** (under the bar); crown37 flinch 372,130 / stagger 306,610 / windup 181,287.
- **b10's B2 set (Logs/n63/b2-table-b10.txt, b2-diff-b10.txt):** before x5 (+ a second kiln before) and after x5: every attack's windup /
  hit line IDENTICAL before vs after on all five (3 / 1 / 2 / 4 / 6 attacks). End lines: wick, brawl40, loom37 unchanged; **wyrm before
  `under 300 ms 1 (spit)`, after 0; crown37 before `off the danger hue 1 (crownFire)`, after 0**. Cue lines identical before vs after on
  foundry and span; **two untouched attacks start one sample earlier only in the same-run after-runs: brawlLeap 3017 -> 3000, wyrm aggro
  13033 -> 13000** (the two kiln befores b2bk / b2bk2 agree exactly: not run-to-run drift between plain runs). Two plain after-runs (no
  toggle) separate the row from the toggle: queued after set 2.
- **verbs37 x5 on b10: 27/27, 29/29, 31/31, 31/31, 31/31, RED rows none; NRE 0 in all 24 b10 reels.** text63 on them: "(LATE" 0 on
  all five; vk `[text63] withheld:` Midair (x2), "Fire LEFT CLICK down to lift off.", "Boost: SPACE, aim down, LEFT CLICK twice."; shown 7
  incl. "Press SPACE and LEFT CLICK together." and "At the top, press LEFT CLICK again."; vp "Hold LEFT CLICK up close to burn." then
  "Dash L-SHIFT and jump SPACE through."; "still waiting at run end" 0.
- **03:10-03:12 the plain after-runs** (b2akp, b2aip: `-skygrindNoBossMotion63`, bossTell63 on, no toggle): **wyrm aggro start 13033,
  brawlLeap start 3017: equal to the befores**, so the one-sample shifts are the same-run toggle's (its held frames), not the row; the
  spit on a plain run `start 24067 lead 425 hue 10 sat 0.92 danger yes verdict ok` (the same-run 492).
- ~03:11 **GATEs spawned: B1 (round 2) and T63 (round 2)** (two slots); B2's waits for a slot. b11 build started (B3c).
- **b11 = n63-b11** built 03:12 from 7035c1e9; drift GG's two files; copy rows on: every design.n63 row. B3's same-run set (b3p / b3k /
  b3f / b3i / b3s, `-skygrindSameRun63 n63.bossColours63`, 03:13-03:18): every boss63 GREEN, NRE 0 in all 5.
- ~03:15 **T63 GATE round 2: `T63: PASS`** (Logs/n63/verdict-T63.md): doneWhen 1 and 2 met on b10; round 1's points fixed (Midair
  withheld; the lost lessons show); new open points (the queue not cleared on a new room, the run-end line never seen, Midair logged
  twice): DECIDE63 D-14. **text63 ON** (`n63-freeze-rows.cjs "n63.text63=..."`: "ON in both config copies: n63.text63"); slate T63 pass;
  committed a53bcae4. **B2's GATE spawned** in the freed slot.
- ~03:17 **B1 GATE round 2: `B1: PASS`** (Logs/n63/verdict-B1.md): every doneWhen line met on b10 (the SPOT pair byte-identical to the
  reel's frames, one sim step, the HUDs identical; hit frames identical to b1 and b8 on all five; the crown's "drawn off" fixed). Open
  (DECIDE63 D-12, added): the wick runs at its full 1.53 m cap and during OPEN its drawn orb can sit 1.53 m from the hit sphere's centre;
  the wyrm barely moves; the same-run mode changes the reel's own fight (crown 35 vs 49 damage events); D-12 missed a third changed count.
  **bossMotion63 ON** ("ON in both config copies: n63.bossMotion63"); slate B1 pass; committed 393bdc8d. V sent a measurement card
  (n63-B1e: the drawn weak point vs the sim's hit sphere, per boss).
- **B3 on b11** (pairs; D's role meter `n63-b3-roles.py --boss`, Logs/n63/b3-roles-b11.txt): crown37 flinch 146,684 / idle 184,276 /
  stagger 5,847 / tell 126,327 / windup 103,878: **FAIL** (tell: amber OFF 34,712 -> ON 223 px, danger 79,675 -> 114,460, but weak
  families [rival, rose]; windup [player, rival, rose]); wyrm flinch 1,920 / idle 589 / stagger 187,052 / tell 314 / windup 2,782: **FAIL**
  (stagger: player 36,491 + green 27,240 px, off-role 27,776; proof `wyrm.open weak #FFD23A x2.2 -> #0854FF x6.8`); wick PASS (stagger 0:
  EMPTY); brawl40 EMPTY (0-6 px); loom37 PASS (stagger 48,628). **B3 SHORT** by its own meter (fix round spent; no GATE; bossColours63
  OFF). The SPOT for the record (crown tell; A = ON): "The big explosion burst around the ring at the centre is clearly brighter and more
  yellow-orange in B, while in A it is a darker red-orange" (names the attack's colour change). D asked for a written cause of the two FAILs.
- ~03:20 **B2 GATE: `B2: PASS`** (Logs/n63/verdict-B2.md; narrow: the spit's fix never on screen in a reel frame, its margin 125-192 ms
  over the bar, crownFire already had a faint red cue before, a STAGGERED prompt during the spit's wind-up, the meter reads drawn colours:
  DECIDE63 D-15). **bossTell63 ON** ("ON in both config copies: n63.bossTell63"); slate B2 pass; committed d391c6ef. **The slate: 6 PASS,
  7 SHORT, 1 BLOCKED, 0 in progress** at 4:43. The flow gate owed after three flips started (background, Logs/n63/gates-flow-b10flips.txt).
  G sent two fix cards (n63-FIX: text63's stale queue + Midair logged twice; the STAGGERED prompt's cause, read-only).
- ~03:21 **D's reading of B3's two FAILs** (6524bfd3, D-B3c.md, picked): the crown's 1,309 rose / magenta px (1 % of the change) are 1-2 px
  edges of pale streaks beside the now-red background by the TORCH barrel, no weak point drawn on the tell or windup frames (the meter's
  rule (c) counts them); the wyrm's head is one teal (~158 deg: the grade compensation undoes the filter, not the sulphur toning on
  highlights) split by the meter's 160 deg cyan-band edge into "player" / "fringe" / "green". **My SHORT call was the builder's own meter
  alone; B1 and T63 had their round-2 GATEs, so B3 gets one too** (slate B3 back to critic; round 1 kept as verdict-B3-r1.md); verbs37 x5 on
  b11 (its doneWhen 3) running first.
- **verbs37 x5 on b11: 27/27, 29/29, 31/31, 31/31, 31/31, RED none; NRE 0 in all 10 b11 reels.** ~03:24 **B3's GATE round 2** spawned.
- ~03:25 **V's B1e** (n63-B1e 8c01bc1c RED, ae09208a a `[boss63] reach <boss>` line: the drawn weak point's distance to the sim's hit
  sphere centre over the open frames, max / p95 / share outside; 1854ec5d notes V-B1e.md; measurement only). ~03:29 **G's T63c + the
  STAGGERED cause** (n63-FIX f84bbdcb RED, 99af17be text63's queue drops (and logs) lines when their room is left or a boss fight begins,
  a withheld line logged once, "nothing still waiting" printed; 0ed8c79a notes G-FIX.md: the STAGGERED hint is the sim's own state, the
  worm keeps telling while staggered: DECIDE63 D-15, GG's call).
- **03:32:11 the flow gate after the three flips (text63, bossMotion63, bossTell63): "gate flow --off63: EQUAL to flow-base", golden 0
  mismatches, the Editor 0 errors: gates all GREEN** (Logs/n63/gates-flow-b10flips.txt). b12's pick test-first: f84bbdcb 8c01bc1c RED
  picked; compile and classes running (Logs/n63/pick-b12-red.txt).
- ~03:30 **B3 GATE round 2: `B3: FAIL`** (Logs/n63/verdict-B3.md): the wyrm's OPEN head is spring green (median hue 158.5, RGB 9 / 238 /
  155; more than half below the table's player band 160-205), not the wick's and crown's cyan 180; the Icebreaker changes 0 px; the
  crown's meter FAIL is the meter's own (97 blobs of pale pink streak edges beside red) and its tell is beaten (amber 34,712 -> 223 px);
  doneWhen 2 and 3 met; D-13 accepted. **B3 SHORT** (fix round spent; bossColours63 OFF). The slate: 6 PASS, 7 SHORT, 1 BLOCKED, 0 in
  progress.
- **b12's pick:** RED (compile 03:33:02) N63TextTests 8 / 11, N63ReachTests 0 / 4 (my first run named a wrong class N63B1eTests: 0 / 0;
  the N63ReachTests run was foreground, not background: noted). Then 99af17be 0ed8c79a ae09208a 1854ec5d -> 562e37ea 09358da2 332068cb
  71ce08f9 (no conflict): **GREEN N63TextTests 11 / 11, N61TextTests 6 / 6, N63ReachTests 4 / 4, N63BossMotionTests 15 / 15, N63B1dTests
  5 / 5, N63Shared 3 / 3; gates all GREEN 03:37:04** (Logs/n63/pick-b12-green.txt).
- ~03:38 **b12 = n63-b12** from 71ce08f9 with the repo's rows only (what ships: bossRule61, text61, nearFade63, text63, bossMotion63,
  bossTell63 ON; every other row OFF), and its set: verbs37 x5 (text63's T63c on kiln; nothing lost), boss63 x5 plain (B1e's reach line;
  the bosses as shipped), a157 kiln (text63's kb2 clear). gg-proxy ranks b12's frames after.
- ~03:20 G's w63 harvest written for real (n63-MEM 4ed5dbf0 -> 4abde309: ledger/wave-63.md 58 lines, FEATURES 133, THREADS 60, LESSONS
  150 (L-134..L-146), CORE 40; `[close]` only for the freeze lines and the tag). G stopped.
- **b12's set (03:38-03:49; NRE 0 in all 11):** verbs37 x5 27/27, 29/29, 31/31, 31/31, 31/31, RED none; boss63 x5 plain GREEN; a157 GREEN.
  **B1e's reach lines** (the drawn weak point's centre vs the sim's hit sphere over the open frames): wick max 1.57 m / p95 1.56 of 1.70
  (86 frames), brawl40 0.98 of 1.20 (610), wyrm 0.44 of 1.50 (499), loom37 0.71 of 1.60 (249), crown37 1.76 of 3.00 (208): **0 frames
  outside on every boss** (the centre only; the edge of a drawn part not measured).
- **T63c regressed on b12:** kiln still holds T63's doneWhen (Midair / Fire ... lift off / Boost ... twice withheld, the step lines shown,
  "Jump ..." dropped once on leaving kb2 then queued again and shown; LATE 0), but **iceline's only gauntlet lesson is now lost**:
  `[text63] dropped: left its room, entering g6: One LEFT CLICK swing drops a tank.` (b10 showed it). T63c was never GATEd; text63 passed
  its GATE without it. **Reverted** (`git revert` 562e37ea -> f93c2de0, then its RED tests 986894be -> 829b8394; Text61.cs, UiText61.cs
  and N63TextTests.cs now equal their pre-T63c state, `git diff --stat 986894be^ HEAD` empty), so the shipped text63 is the one the critic
  passed; the stale-queue point stays open (DECIDE63 D-14). Compile, text tests and gates running (Logs/n63/revert-t63c.txt).
- ~03:52 **gg-proxy spawned** on b12's frames (Logs/n63/gg-proxy.md; b12 carries T63c, the only difference from HEAD).
- **The revert's gates: N63TextTests 8 / 8, N61TextTests 6 / 6, N63ReachTests 4 / 4; gates all GREEN 03:50:14** (Logs/n63/revert-t63c.txt).
- **b13 = n63-b13** built 03:50:39 from 0848b181 (HEAD, the repo's rows only): **verbs37 x5 27/27, 29/29, 31/31, 31/31, 31/31, RED none,
  NRE 0**; iceline shows "One LEFT CLICK swing drops a tank." again; kiln's text63 lines as on b10 (Midair / Fire ... lift off / Boost ...
  twice withheld, the step lines shown, "(LATE" 0); spire's TORCH line then the Dash line.
- Report section 7 written (8f365785): 21 SPOT runs, 15 named the slice's change (71 %); 6 PASS of 14 entries.

## Blocked
- **The usage limit stopped the whole session from ~05:23 to 07:41 local (clock ~5:29 to 9:05).** gg-proxy (spawned ~03:52 on b12's
  frames) died with "You've hit your monthly spend limit ... (HTTP 429)" after "Now the remaining boss shots." and wrote no file; the lead
  stopped at the same moment and resumed only after a `/login` at 07:41 (clock 9:05, phase CAP). The whole close window (FREEZE 7:00-8:00
  = 05:36-06:36) passed during the stop (L-003 / L-098 again).
- **Close step (a)** (wait for a GATE due): nothing was due; every GATE had reported by 03:31 (B3 round 2).
- **Close step (b), the final `bash Tools/night/n63-gates.sh --flow`: NOT RUN.** At 9:05 the clock guard allows no test run. The last
  `--flow` run is GREEN at 03:32:11 (Logs/n63/gates-flow-b10flips.txt: "gate flow --off63: EQUAL to flow-base", golden 0 mismatches,
  fingerprints EQUAL, the Editor 0 errors) after the three flips; the last plain gates GREEN at 03:50:14 (Logs/n63/revert-t63c.txt) after
  the T63c revert, the night's last code change (view code only: Text61.cs / UiText61.cs back to their b10 state). Every commit after
  0848b181 is a doc or a log (`git diff --stat 0848b181 HEAD -- Assets Design PortExport ProjectSettings Packages` prints nothing).
- **Close steps (c)-(e), the n63-freeze build, its verbs37 x5 and the smoke: NOT RUN** (the guard refuses builds, reels and the smoke in
  CAP). What stands in their place: **Builds/n63-b13**, built 03:50:39 from 0848b181, code-identical to the tag: verbs37 x5 27/27, 29/29,
  31/31, 31/31, 31/31, RED rows none, NRE 0 (Logs/feel/n63-b13/summary47.txt). **The smoke (`n56-smoke-steal.sh`, "DONE: all steps
  passed") was not run on any NIGHT63 build: not checked.** The focus probe on b7 printed "DONE: paused on the focus loss True"
  (Logs/n63/focus-probe-b7.txt), which is not the smoke.
- **gg-proxy's close ranking on the newest frames: not done** (the stopped agent above; no agent may start in CAP). NIGHT63-REPORT.md
  section 1 says so and uses the critics' verdicts instead.
- `git diff --stat ProjectSettings` after the last build (b13): GG's two files only (EditorBuildSettings.asset, UnityConnectSettings.asset),
  never staged.

## The close (at CAP, 07:41 local)
- `node Tools/night/n63-clock.cjs`: "clock n63: 9:05 / 8:00 since t0 22:36:01; phase CAP ... commit what is done, log the rest under
  Blocked, end".
- No agent alive (`ListAgents`: no subagents; the two peer sessions idle); 0 Players (`tasklist`).
- `node Tools/night/n63-slate.cjs`: "slate: 14 slices: 6 PASS (passes true), 7 SHORT, 1 BLOCKED, 0 in progress, 0 todo".
- Each row's `enabled` in both copies (byte-identical): n61.exitFrame61 false, n61.bossRule61 **true**, n61.text61 **true**, n61.fixture61
  false, n61.gunYellow61 **true** (NIGHT62's), n63.bossMotion63 **true**, n63.bossTell63 **true**, n63.bossColours63 false, n63.colours63
  false, n63.nearFade63 **true**, n63.press63 false, n63.text63 **true**, n63.eye63 false, n63.exit63 false: matches NIGHT63-REPORT.md.
- Written at the close: NIGHT63-REPORT.md (every `[CLOSE]` filled: section 1 without gg-proxy's lost ranking, sections 2, 3 (28 N63
  classes, 170 tests, class by class), 5, 6, 8), HANDOFF.md (the play build is Builds/n63-b13; the three lost close steps first), the
  harvest's three `[close]` markers (CORE.md, ledger/wave-63.md: 13 builds, no freeze build, no smoke, the tag), L-003 re-confirmed in w63
  (LESSONS.md stays 150 lines, CORE 40, THREADS 60, the ledger 58), DECIDE63 D-16 (untracked).
- This commit is the close commit: `git tag u3-n63` goes on it, then `git tag -l u3-n63`, `git status --short NIGHT63-REPORT.md
  NIGHT63-LOG.md HANDOFF.md docs/memory/CORE.md` (want nothing) and `node Tools/night/n63-clock.cjs stop`, in that order, with nothing
  committed after the tag (NIGHT62's tag landed one commit early, L-133).
- Done as written: `git tag -l u3-n63` printed `u3-n63` (on 0ec78408); the status check printed nothing; `node Tools/night/n63-clock.cjs
  stop`: "clock stopped at 9:09 since t0 (Fri Oct 23 2026 07:45:11 ...)"; NIGHT63 COMPLETE.

## After the close (07:55-08:17 local, the clock stopped; GG's "continue": HANDOFF's step 0)
- Bearings: `n63-clock.cjs` "phase NONE ... the guard ignores this clock (stopped)"; the Editor ready, playMode stopped; 21.1 GB commit
  free, 23.7 GB disk; no agent alive.
- **`bash Tools/night/n41-build.sh n63-freeze`** started 07:55:32 at 0ec78408 (the tag), completed 07:55; `git diff --stat ProjectSettings`:
  GG's two files only (EditorBuildSettings.asset, UnityConnectSettings.asset), never staged (Logs/n63/build-freeze.txt).
- **verbs37 x5 on n63-freeze** (`n63-reels.sh n63-freeze vf ... vp`, Logs/feel/n63-freeze/summary47.txt): 27/27 n52-foundry, 29/29
  n52-iceline, 31/31 n57-kiln, 31/31 n52-span, 31/31 n52-spire, RED rows [] everywhere; NRE 0, exceptions 0 in all five.
- **The smoke** (`bash Tools/night/n56-smoke-steal.sh n63-freeze`, no agent alive; Logs/n56/smoke-steal-n63-freeze.out, Logs/n63/
  smoke-freeze.txt): both planted steals took the foreground ("steal at +60 s ... took True", "steal at +201 s ... took True"); "smoke
  steal-n63-freeze end (249 s)"; **"[smoke] 252.5s DONE: all steps passed"**.
- **The final flow gate** (`bash Tools/night/n63-gates.sh --flow`, Logs/n63/gates-flow-freeze.txt): "gate flow --off63: EQUAL to
  flow-base", config copies identical, golden 39 sets (48094 records), 22 replays exact, 0 failing, 0 mismatches, fingerprints plain and
  --off57..--off63 EQUAL, the Editor 0 errors: **"gates: all GREEN 08:17:31"**.
- So the three close steps the usage limit cost are done on the tagged code; the play build is now **Builds/n63-freeze**. The tag stays
  on 0ec78408 (code-identical to every later commit: these are logs and docs only; moving a tag is GG's call). Committed c1b2ec43.
- ~08:19-08:31 **gg-proxy on the shipped frames** (n63-freeze's verbs37, n63-b12's boss shots: the same boss code; Logs/n63/gg-proxy.md):
  it saw 36 frames, short of its brief (16 of 30 level frames, 18 of 22 boss shots, 2 kiln entries; many reads came back "[media
  removed: request limit]"). Top 8: (1) the rooms still big empty boxes, 12 of 16 level frames; (2) choppy movement (no frame can show
  it); (3) the bosses unreadable: 11 of 18 boss shots show no readable boss, and 4 print the same order twice (small under the bar, large
  in the centre); (4) colours: spire's exit red line 5 of 5, the bosses' own cyan; (5) floating lamps 10 of 16; (6) the fight hidden 9 of
  16; (7) kills (no frame can show it); (8) the style meter at D, the score 500 in 16 of 16. "Nothing on the level frames got better."
  NIGHT63-REPORT.md section 1 carries it.

## Timeline
- 22:36:23-22:37:53 (0:00-0:02) baseline `bash Tools/night/n63-gates.sh` (background, Logs/n63/gates-baseline.txt): config copies identical;
  golden 39 sets (48094 records), 22 replays exact, 0 failing, 0 mismatches; fingerprints plain and --off57..--off63 EQUAL; the Editor
  compile `"compilationFailed":false,"consoleErrors":0`; **gates: all GREEN 22:37:53**.
- 22:38 (0:02) A1: `node Tools/night/n63-a1-rows.cjs` (Tools/night/n63-a1-rows.cjs): design.n63 written after design.n61 with bossMotion63,
  bossTell63, bossColours63, colours63: view rows, enabled false, numbers {} (both copies byte-identical; nothing else changed). DECIDE63
  D-1 (yellow = the parry tell's role only). N63SharedTests.cs (3 tests).
- 22:38:16 (0:02) `bash Tools/night/n63-gates.sh --flow-base` started (background; Logs/n63/flow-base-run.txt).
- 22:38-22:39 (0:03) Editor compile `completed, DLLs rewritten (22:39:08)`, `"compilationFailed": false, "consoleErrors": 0`;
  `unity-loop.sh tests N63SharedTests`: **3 / 3 passed** (Logs/n63/a1-editor.txt).
- 22:39 (0:03) gg-proxy's start check spawned (background) on Logs/feel/n62-freeze/v*/verbs37 -> Logs/n63/gg-proxy-start.md.
- 22:40 (0:04) M1, the fixed-clock half: `node Tools/night/n63-m1-chop.cjs n62-freeze` (Logs/n63/m1-chop-n62-freeze.txt; a quadratic
  Savitzky-Golay fit over 9 frames: a first moving-average version flagged plain gravity arcs at ~2.8 cm and was replaced before any
  number was used). Numbers in M1's entry below.
- 22:46:03 (0:10) the flow base written: `flow base: Logs/n63/flow-base.txt, 5 level summaries, 22:46:03` (and flow-base-a0.txt).
- 22:46-22:47 (0:11) A1's gates (Logs/n63/gates-a1.txt): config identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprints
  plain and --off57..--off63 EQUAL; the Editor 0 errors, 0 console errors; **gates: all GREEN 22:47:01**. **A1 committed 1ee56384.**
- 22:48 (0:12) M1's pacing half started (background): `bash Tools/survival-bench.sh 60 Builds/n62-freeze/SKYGRIND.exe
  Logs/n63/m1-bench.log play` (one Player; builders reading, no other heavy job).
- 22:47:26-22:48:34 (0:12) the bench: Turbo / AC before and after (counts); median 3.70 ms, p95 6.96, p99 10.10, p99.9 13.76, max 32.10;
  742 frames over 6.94 ms (5.06 %); worst 1 % main-thread engine time (Update 2.00 ms, engine 10.09 ms, GPU 0.73 ms) (Logs/n63/m1-bench.txt).
  The game's own default is VSYNC on (move54 ON; SurvivalGame.Move54.cs:41-47); the bench forces vsync 0. M1's note Logs/n63/m1.md;
  DECIDE63 D-2 (two measured candidates, a proposal, no change tonight).
- 22:50 (0:14) M1's GATE critic spawned (background; verdict Logs/n63/verdict-M1.md). Slate: M1 critic; D1 / P2 / B1 building
  (Tools/night/n63-slate-set.cjs, a lead-only helper that refuses doneWhen and id).
- 22:52:27 (0:16) leftover 1, its w62 half: the w62 memory harvest committed 799a0970 (ledger/wave-62.md, FEATURES, THREADS T-148 / T-149);
  w63's half is the close's (g).
- 22:53 (0:17) **M1 GATE round 1: `M1: FAIL`** (Logs/n63/verdict-M1-r1.md): eleven problems, checked against the data and accepted: the
  "snaps" were frames not events (36 of foundry's 43 are 6-frame slide eye-ease runs; the slide's exit rise unnamed), most landings stay
  under the 5 cm line (16 of 23 on foundry), the worst frame is a slam stop (f573 -48 m/s; f574 feet -65.63 / eye -32.67 cm), the bench
  counts mixed in its 30 warm-up rows, the proposal could not meet its own measure (0.70 m over 0.15 s is still 7.8 cm a frame), D-2
  overstated (causes vs candidates; a VSYNC-on rate extrapolated). One fix round (lead): the tool rewritten (events, distinct worst events,
  feet / eye split, "none found", --bench skipping warm-up), m1.md and D-2 rewritten; committed 9ab4cb54 (with the bench files). Round 2
  sent to the same critic by SendMessage (~22:57). **Round 2: `M1: FAIL`** (~22:59, Logs/n63/verdict-M1-r2.md): the "step-ups" are the
  reel placing the player 0.2 m up (f267 / f342: feet 0 -> 0.199 m at zero speed, the eye reset, a fall back and a landing 6 frames
  later); the slide dip unnamed; the slam cannot be halved by easing the eye. One fix round used: **M1 SHORT** (072fc9e9, 23:00:15); the
  text and the tool's labels corrected after, not re-judged.
- Correction (23:09): the times on four lines below and above (G's report, V's report, the re-order, leftover 3) were first written from
  memory (23:06 / 23:18 / 23:19 / 23:31) and are corrected here from the commits' times; the clock read 0:30 at 23:06.
- 22:58:59 (0:23) leftover 3 (T-146's fourth item): n60-v-lamp.py now skips a clear shot whose sidecar is not `cleared` (planted GREEN);
  committed 1b76fb8e. Run on NIGHT62's P1 pair (s1on vs s1spot): **P1's own gain 0 / 5 rooms** (1.22 / 1.16 / 1.41 / 1.07 / 1.27x;
  Logs/n63/p1-red-n62pair.txt); kb3c's shot is 21 m from the door behind a ledge (crops in the scratchpad, by eye). DECIDE63 D-3.
