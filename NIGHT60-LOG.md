# NIGHT60-LOG (OVERNIGHT RUN; NIGHT60.md; the lead + at most two builders)

**Start time (t0): 2026-10-21 23:05:36 MST** (epoch 1792649136; the run's first `date`, DECIDE60 D-10). No new M task after 05:35:36 (6 h 30),
no new S task after 06:35:36 (7 h 30); the close from 07:05:36 (8 h); the /goal's escape 07:35:36 (8 h 30) with the report written.

## Progress (one line per task: state, commit, evidence)
- 23:05-23:20 A0: base held (u3-n59 in HEAD, 20 GB, the Editor stopped and answering); tools n60-gates / n60-reels / n60-prune (a1e6216c);
  the flow baseline Logs/n60/flow-base.txt (5 levels, seeds 1-10, all GREEN 10/10); the check command `n60-gates.sh --flow`: all GREEN
  23:20:26 (flow EQUAL, golden 0 mismatches / 22 of 22 replays, fingerprints plain / --off57..--off60 EQUAL, Editor 0 console errors).
  Builder brief 5e4e716d. Builders spawned 23:22: L on B4, V on B2.
- **B1 DONE** (S, the lead, 23:23-23:42): a157 gained an opt-in `-skygrindSpawnShots60` (FeelReel.N57.Level.cs: each room also framed from
  its spawn pose; without the flag a157 is unchanged). n60-b1 a157 GREEN 5/5, 0 NREs (Logs/n60/b1-summary47.txt). From kb2c's spawn
  (-2, -168) yaw 45 the frame faces the west wall and the near bridge's face 2-4 m away; seen 0 of 5 live wave-1 enemies (nearest 8.3 m,
  off-screen); the entry view sees 5 / 5. Hand count of wave 1's nodes (waveNodes43[0]) inside the 53 deg half-view of the REAL facing:
  1 / 4 ((-1, -180) at 49.8 deg right); the harness's "w1 in view 4/4" is wrong: n46-flow-geo.cjs measures the bearing off -z and ignores
  the spawn's yawDeg (correct only for yaw-0 spawns). The lead looked at both frames (Logs/feel/n60-b1/l0spawn/a157/
  shot-N29-L57-n57-kiln-kb2c-spawn.png, -entry.png). Gates GREEN 23:41:33. Findings -> backlog B12 (S) and B13 (M).
- **B6 DONE (cause)** (S, the lead, 23:43-23:52): n46flow --level n57-kiln seeds 11-20 --per-seed --trace (Logs/n60/b6-kiln-B-perseed.txt).
  kb3c's stall sits in three seeds (15: 52 %, maxStall 9.1 s; 16: 61 %, 8.9 s, 2 deaths; 17: 29 %, 3 deaths); the other seven clear in
  13-25 s. ALL six kb3c deaths are in THE TAP RUN's lava (pit x 7..11, z -247.5..-233.5, floorY -0.8): five at (10.0, -0.8, -247.2), the
  run's south end against THE RUN WALL, one at (7.7, -0.8, -235.1). After the clear the bot stands at (7.4..10.0, -233.5..-232.8) and the
  pilot's grid path to kb3d's spawn prints NO PATH (1591 lines) until the 15 s rule moves it on (2 put-ats on this set). The spot is a
  closed POCKET, 3.4 x 4 m: THE TAP HOOD (hull x 2..7, z -234.5..-230.5, floor to roof) on the west, THE RUN WALL (x 10.4..11) on the east,
  the lava on the south, the room's north wall (z -229.5): reachable only out of the lava (its knock-up) or off the ride, left only
  through the lava. Not the start cell (Plan() unblocks it). So: the bot is knocked or rides into the pocket, burns in the run, and is
  stranded. A person can jump out through the burn; the shape is still a dead-end corner at the head of the room's hazard (L-087).
  Not L0's doing: L0 changed kb2c / kb3 only (the pocket's pieces are NIGHT57's). Finding -> backlog B14 (M).
- **B3 in flight** (M; the lead 23:49-00:05, then builder V from 00:05): the "parry halo" the NIGHT59 critic named (foundry a5-entry, box
  x 560-650, y 540-700) is a SPITTER in its parry tell, not a husk; its yellow is dim and very saturated (off: V median 0.45, S median 0.94).
  The probe never reports spitters or swarmers (EnemyViews.cs: SpitterView and the swarmer view never call FeelProbe.Enemy; the husk does at
  line 534), so every meter that subtracts enemy rects reads their pixels as world; ProbeMaxEnemies is 8. Builder V: (a) the two views
  report, (b) Tools/night/n60-v-yellow.py folds yellow touching an enemy rect into its tell (planted cases), (c) n60-build-num.cjs for a
  Player sKeep scan (D-11).
- **B2 verified, critic pending** (M; builder V 23:22-00:01, cd05c630 3524e84e 284a4783): the cause: the enemy x-ray (queue 3005, ZTest
  Greater, alpha 0.35 red-orange) drew over the marks (queue 2010). N60MarkTests RED on 3524e84e ("Expected: greater than 3005 / But was:
  2010", 00:07:55) and GREEN on 284a4783 (00:10:55). Gates GREEN 00:15:00. Builds/n60-b2 (kill55 ON in its copy): a258 MARKS AT THE FLASH
  END 15.29 % ON vs 0.42 % OFF vs 0.44 % old draw (want >= 5 %); the probe at the mark's centre V 0.76 -> 0.03 (old 0.52); the input line
  "mark queue 3006 vs the enemy x-ray's 3005 (x-ray setting on"; a355 ladder ok ON / NO OFF; n56-colour cover lines ON inside the OFF / old
  spread; verbs37 x5 with kill55 ON 27 / 29 / 31 / 31 / 31; 0 NREs in all ten reels. The lead looked at shot-N55-K-flash-end ON (a black
  hole with cracks on the body) vs old (a red-orange smudge). One-row critic started 00:35 (Research/2026-10-21-wave60/gate-b2-brief.md).
- **B4 SHORT** (M; builder L 23:22-00:33, picked abd32586): the slot split (the link p1b + the door d1b + the room p2 28 x 32 m, 24 pieces);
  p2's contract 11/14 -> 12/14 (size and density 0.72 -> 2.68 now PASS); FAIL: the ride wall (the card: the pull is the only fast way across)
  and the height (a 16 m roof capped 32 of 80 runs: the drones hover 4-8 m over the 10 m decks and stopped at the roof; cut 3 kept n52's
  22 m). Guards: stall 15.1 / 13.3 %, deaths 0 / 0, p2 falls 55 / 76 (in range), put at the next room 1 / 2 (set B over its <= 1: both in p3,
  n52's room). levelgen 22 -> 20 errors, none new. The shipped n58-span.json = n52-span's (`--check` exit 0). B5 waits on B4 -> GG.
- **B2 DONE: kill55 ON** (01:03-01:18): critic B2 PASS (Research/2026-10-21-wave60/gate-b2.md, 00:35-01:00; it re-ran n56-colour.py and
  n60-v-mark.py and matched the numbers; the build's config differs from the repo's in kill55's enabled only). Turned ON with
  Tools/night/n60-freeze-rows.cjs (a copy of n58's, "ON at NIGHT60 by the rule: ..."). Gates with kill55 ON: all GREEN 01:06:49 (golden 0
  mismatches, every fingerprint EQUAL: kill55 does not move the fingerprinted sim) and `--no-editor --flow` all GREEN 01:17:17 (flow --off60
  EQUAL to the base: the bot plays the five levels exactly as before; Logs/n60/gates-kill55-flow.txt). EditMode with kill55 ON: N55* 24 / 24,
  N55KillTests 6 / 6, N60MarkTests 1 / 1. The critic's play notes for GG: the wound is large (21.86 % of the body at +0.5 s); the hole now
  covers the hit's tinted impact cube for a few frames.
- **B3 BLOCKED after 3 attempts** (M; 23:49-01:39): picked 076310a5 (N60ProbeTests RED 01:15: "Expected >= 24 / But was 8"), 89803e51
  (the swarmer and spitter views report FeelProbe.Enemy, recording-only; ProbeMaxEnemies 8 -> 32; GREEN 01:20), dee75beb (n60-v-yellow.py),
  f6133f7e (n60-build-num.cjs). Builds/n60-b3 (yellow56 ON in its copy; kill55 ON from the repo): the reporter fired (a5's frames list
  5 enemies, NIGHT59's 4). The sKeep scan, a358 foundry + spire (Logs/n60/b3-main-*.txt, b3-halo-*.txt, b3-orbs-*.txt):
  off: MAIN 11 / 12 frames over 0.5 %, halo kept 90.5 %; sKeep 0.85: 0 / 12, 59.6 %; 0.75: 0 / 12, 64.7 %; 0.65: 7 / 12 (spire up to
  3.55 %), 82.2 %. No keep bound passes the MAIN and the halo guard (>= 80 %) together: the tell's pixels and spire's world yellow overlap
  in hue, saturation and value. The orbs guard (within 10 % relative) is also tight: spire v5-mid 13.44 % off -> 15.51 % at 0.85.
  yellow56 stays OFF; the repo's sKeep stays 0.85. The way on is a mask, not a colour bound: NIGHT60-PROPOSALS.md P1 (L). The
  instrumentation fix stays (every meter that subtracts enemy rects now sees spitters and swarmers). The meter's fresh review: pending.
- **B3's meter review** (01:21-01:44, Research/2026-10-21-wave60/review-b3-meter.md): METER: SOUND WITH A BOUND (it can fold real floor /
  plate yellow behind an enemy rect: 4 planted cases; no ON-frame verdict changed; the bound measured) -> backlog B16; its side finding (a
  5.3 %-of-frame enemy rect, two off-screen rects) -> B17.
- **B14 picked, Player check running** (M; builder L 00:37-01:41; 612530f0 cut 1 THE TAP HOOD x 2..7 -> 2..5, bdb2ec0b cut 2 the tap run's
  south end -2 m): n46flow seeds 1-10 / 11-20 / 21-30 before -> cut 2: kb3c NO PATH lines 1012 / 1591 / 2171 -> 0 / 0 / 0, put-ats 1 / 2 / 5 ->
  0 / 1 / 0 (the one left, seed 14, is THE BLOW PIPE launching the bot past d6b: B15), kb3c deaths 1 / 6 / 1 -> 0 / 0 / 0, stall 20.3 / 22.9 /
  22.7 -> 20.9 / 21.5 / 20.4 %; kb3c contract 14/14 (dwell 1.10 -> 0.00); room table PASS (in view 6 / 6); levelgen 0 errors; walks 10/10
  (Logs/n60/L/b14/). The backlog's MAIN said put-ats "2 -> 0 on seeds 11-20": 2 -> 1, missed by one from another cause; stated to the critic.
- **B7 attempt 1 picked, Player check running** (M; builder V 00:47-01:39; 3f5c9509 cause, c658cdba test first, 2771de00 fix): rim56's accent
  dim never reached decor55's string course (its own colour function skipped Colour56.Clamp: 85-97 % of the accent pixels unchanged on / off
  on NIGHT58's r1 frames); the fix scales the band x0.45 with rim56 on. N60RimTests RED on c658cdba (01:44:08: 0.776 vs 0.349) -> GREEN on
  2771de00 (2 / 2, 01:45:46). V's estimate: 88.8 % (uncalibrated); cause 2 (the lit gain washes out the brightest bodies' saturation) is V's
  attempt 2. Gates GREEN 01:48:51; Builds/n60-b4 (rim56 ON in its copy; B14's level data in it).
- **Resumed 04:01** after the usage limit (section 0 again: NIGHT60.md re-read in full, LESSONS 100-118 re-read (1-99 unchanged since
  23:05), the log and the backlog; the check command all GREEN 04:05:40, Logs/n60/gates-resume.txt). The B13 critic, builder L (B9) and
  builder V (B7 attempt 3) resumed by SendMessage (L-098).
- **B13 DONE: kb2c's respawn fix stays in level 3** (critic B13 PASS, Research/2026-10-21-wave60/gate-b13.md): seen 0 -> 5 of wave 1 from
  the spawn (two pre-B13 builds both 0), spawn31 0 errors, w1 in view 1 / 4 -> 4 / 4, guards in band, verbs37 31 / 31. Its notes: the flow
  gate's base predates B14 / B13 (re-made below before any row turns ON); the brief's levelgen command would rewrite the tracked report.
- **B8 attempt 1: MAIN not met; the reel's frame is not a clear** (picked 563795e2 test first: N60SharedTests RED 04:08:46 ("design.n60._why
  Expected True But was False") -> db599f4c GREEN 04:10:53; 9fec7fb5 the clear-shot reel; e51de119 the meter; gates GREEN 04:13:44;
  Builds/n60-b6 lamp60 ON in its copy): Logs/n60/b8-lamp.txt: door luma / room median >= 1.5x on 3 / 5 rooms ON, but OFF the same rooms read
  1.80 / 1.31 / 1.57 / 0.79 / 1.81x and the lamp's own gain (door luma on / off) is 1.00-1.06x: the lamp does not light the door in these
  frames. The lead looked at kb2c's "clear" frame: the HUD reads "15 LEFT" with swarmers alive and no cyan at the far door: the reel's
  clear shot is not taken at a clear. lamp60 stays OFF (repo); unverified. Next: attempt 2 (the clear-shot setup, then the lamp's reach).
- **B7 attempt 3 in the Player: MAIN met, every guard holds** (Builds/n60-b7, rim56 ON in its copy; Logs/n60/b7a3-rimpath.txt,
  b7a3-guards.txt): bodies over the world's p90 off 106 / 140 (75.7 %) -> on 127 / 141 (90.1 %) (want >= 90 %); inputs on "gain 1.10" /
  "x0.45", off "0.00" / "no"; world over the tells 3 / 40 off = 3 / 40 on (all kiln, the same frames: not rim56's); greyscale 25 / 40 off ->
  36 / 40 on; tint 39 / 40 = 39 / 40; 0 NREs in the twelve reels. Still needed before ON: verbs37 x5 with rim56 ON (running), the flow gate,
  a one-row critic. (Attempt 2 had read 90.8 % with the guard at 4 / 40; the gain 1.3 -> 1.1 cost 0.7 points of the MAIN and fixed the guard,
  as V's estimate said.)
- **B10 not met; kept off the repo** (builder L 04:18-04:56; n60-b10: 9e0639f7 test first, 8f458b87 cut 1): the cause is not NIGHT58's
  note: 31764 of the drones' 35700 idle steps on iceline are drones frozen mid-climb (campaign drones never get the NIGHT20 climb rules: the
  roof check, giving up a stalled climb; only the survival director's spawn turns them on): the same freeze that held B4's drones at the
  16 m roof. Cut 1 (design.n60.idle60, a sim row: campaign drones get the NIGHT20 climb rules, no new number): iceline idle > 20 % 18.4 /
  20.2 % -> 15.6 / 18.4 % (want <= 10 %: the rest are wardens / chargers holding the heavy token), the drones' idle share 32.6 -> 5.5 %;
  deaths 6 / 6 -> 9 / 6 (set A over its band); row off = the base line for line; fingerprints EQUAL. Not picked (DECIDE60 D-13).
- **Builders on the audit pass** (04:57): L (THREADS, FEATURES, VERBS) -> Research/2026-10-21-wave60/audit-L.md; V (UI text >= 16 px, the
  palette, pad-first) -> audit-V.md; both read-only, by 05:30.
- **B15 picked** (8419921a; builder L 02:13-02:44): THE BLOW PIPE moved off d6b's exit lines onto the furnace top's NE corner (it still
  launches onto the fettlers' walk; the pad check lands 0.01 m off). kb3c put-ats 0 / 1 / 1 -> 0 / 0 / 0 (seeds 1-30); NO PATH 0; kb3c deaths
  1 / 0 / 0 both sides (seed 9's lava death predates B15: B13's level); stall 18.9 / 21.6 / 18.6 -> 18.8 / 18.8 / 17.9 %; kb3c contract 14/14
  (dwell 1.10 -> 0.35, pads 2 / 2); room table PASS; levelgen 0 errors (Logs/n60/L/b15/). Player check in Builds/n60-b7.
- **B9 picked** (b43e1490 test first: N60FocusProbeTests 2 / 6, the 4 RED as the builder predicted, 04:21:30; b016ca87 the probe: 6 / 6
  GREEN 04:23:30): -skygrindFocusProbe60 (FocusProbe60.cs), the planted -skygrindFocusProbe60NoPause, Tools/night/n60-focus-probe.sh.
  Player runs (green and red) after the n60-b7 reel set.
- **B7 attempt 3 picked** (5d51b7be cause, 4d6a6f62 test first RED 04:24 ("Expected <= 1.1 and >= 1.0 / But was 1.3"), 4669d2be
  Rim60LitGain 1.3 -> 1.1, N60RimTests 4 / 4 GREEN 04:27:14): builder V's cause: the guard's one flipped frame is iceline g2-entry, where the
  enemy rects' red-orange / yellow pixels whiten as the lit gain rises (median S 0.706 off, 0.646 at 1.3, 0.502 at 2.205) under the world's
  capped 0.654; the band dim plays no part. Estimate at 1.1: MAIN 130 / 141 (92.2 %), the guard 3 / 40 (uncalibrated). Gates GREEN 04:30:09;
  Builds/n60-b7 (rim56 ON in its copy; B15's level data in it). The flow base re-made after B14 / B13 / B15 (the B13 critic's note).
- **B7 OFF by a guard** (02:47-02:58; Builds/n60-b5, attempt 2 = the band x0.45 + Rim60LitGain 1.3): MAIN met: bodies over the world's
  p90 off 108 / 140 (77.1 %) -> on 128 / 141 (90.8 %) (Logs/n60/b7a2-rimpath.txt; inputs on "gain 1.30" / "x0.45", off "no"); greyscale
  26 / 40 -> 37 / 40 and tint 39 / 40 = 39 / 40 hold; but "world over the tells" 3 / 40 (two OFF runs, n60-b4 and n60-b5, both 3) -> 4 / 40 ON
  (one iceline frame; the slate's guard <= 3 / 36): worse beyond the band, so rim56 stays OFF (Logs/n60/b7a2-colour.txt). Not run: verbs37 x5
  with rim56 ON, a critic. Next: one more attempt is allowed (find the iceline frame's tell; a gain between 1.3 and 2.205).
- **Stopped at the usage limit (~03:00)**: unpicked builder work: B15 (builder L, 42d52689: the blow pipe moved; put-ats 0 / 1 / 1 -> 0 / 0 / 0),
  B8 (builder V, 255f73ac 20d13d28 67dbc707 5c7c4097: design.n60.lamp60 + a157 -skygrindClearShots60 + n60-v-lamp.py); B9 (builder L, in
  flight); B13's critic (in flight: Research/2026-10-21-wave60/gate-b13.md when it lands). The close (section 8) not run: no n60-freeze, no
  tag u3-n60, no NIGHT60-REPORT.md.
- **B14 DONE: kb3c's pocket fix stays in level 3** (critic B14 PASS, Research/2026-10-21-wave60/gate-b14.md, 02:10-02:23): NO PATH 1012 /
  1591 / 2171 -> 0 / 0 / 0 (2.7 x the seed noise), put-ats 8 -> 1 over 30 seeds (6 of the 8 were the pocket; the one left is the blow pipe's
  launch at d6b, which already caused 2 of the 8: B15), kb3c deaths 8 -> 0, stall in band, contract 14/14, verbs37 n57-kiln 31 / 31, a157
  in view unchanged; the critic's re-run of seeds 11-20 matched c2-B.txt line for line. Its play note: THE BLOW PIPE near d6b can still throw
  a player off the exit line (B15).
- **B7 attempt 1 measured: MISS** (n60-b4 a357 x 5 levels on / off; Logs/n60/b7a1-rimpath.txt): bodies over the world's p90 off 108 / 140
  (77.1 %) -> on 120 / 139 (86.3 %), want >= 90 %; the input lines on "x0.45 (NIGHT60 B7)", off "no (NIGHT60 B7)" (checked per file: no
  leak); the accent the band fix reaches: "unreached" 0.0-0.8 % per frame (NIGHT58: 85-97 %); V1 cover 39 / 40. The body count is now 140
  (spitters and swarmers report since B3): not NIGHT58's 99-body set. Attempt 2 (V; c114d877 estimator, e6e11996 test first, 5cddef27
  Rim60LitGain 1.3 while rim56 is on): N60RimTests RED 02:18:25 ("Expected 1.3 / But was 2.205") -> GREEN 3 / 3 02:20:16; V's estimate
  91.9 % (uncalibrated). Its Player check comes with B13's in Builds/n60-b5.
- **B13 picked and verified in the Player, critic pending** (33ef339f; gates GREEN 02:26:49; Builds/n60-b5): a157 kb2c spawn at yaw -10:
  seen 5 of 5 wave-1 enemies (B1: 0 of 5), nearestAlive 9.6 m; the entry view still 5 / 5; a157 GREEN; verbs37 n57-kiln 31 / 31; 0 NREs. The
  lead looked at the spawn frame: down the middle channel, the near bridge on the left, wave 1 in the ash ahead, the cyan ride column on the
  right (B1's frame: the west wall). One-row critic started 02:37 (Research/2026-10-21-wave60/gate-b13-brief.md).
- **Audit (a)+(b) early** (02:30): 32 Player logs of tonight's reels and smokes, 0 with an NRE or an exception; the Editor console 0 errors,
  36 warnings (not read yet).
- **B13 done by builder L** (5f0af957): kb2c's spawn yaw 45 -> -10 (facing down the middle channel) and wave 1's four
  nodes 2 m south (12.75-13.3 m from the spawn: spawn31 still passes); n46-flow-geo "w1 in view" 1 / 4 -> 4 / 4; guards: put-ats 0 / 1 / 0
  -> 0 / 1 / 1, deaths pooled 1 -> 3, stall 20.9 / 21.5 / 20.4 -> 18.9 / 21.6 / 18.6 %, contracts kb2c 14/14 (dwell 0.70 -> 0.65) and kb3 14/14,
  room table PASS (Logs/n60/L/b13/). Picked after B14's critic re-runs the bot (B13 sits upstream of kb3c).
- **B17 cause noted, no change** (S, the lead, 02:02): FeelProbe.ScreenRect projects a body's world AABB corners and skips those behind the
  camera, so a body beside or straddling the camera plane yields a large rect running off-screen (the reviewer's [1286.9, 906.8, 779.8,
  1441.1] = the bottom-right corner): the honest AABB of a near body. Clamping to the screen changes no mask; dropping it would unmask a real
  enemy. A silhouette mask from the render is the real fix (L; not specced: low value).
- **L-113** (the lead's mistake, 01:50-02:07): n60-prune.sh's side pattern skipped hyphenated sides; C: fell to 9.7 GB; fixed (`[a-z0-9-]+`).
- **B16 DONE** (S, the lead, 01:52-01:57): n60-v-yellow.py folds a whole-frame yellow component into an enemy's tell only when >= 90 % of
  it lies within TELL_PX of a rect (NEAR_SHARE, the reviewer's bound). Planted: 10 / 10 as planted (Logs/n60/b16-planted.txt): the old eight
  print as before except case 7 (now "nothing folded": an over-count, never a false pass), and the reviewer's two new cases (a floor band
  and a tell-coloured plate behind an enemy) are RED on the old rule (it folds 0.06 / 0.14 % of world yellow) and GREEN on the bound
  (Logs/n60/b16-red-green.txt). On the sKeep scan's frames the verdicts are unchanged (0 / 0 / 7 / 11 of 12). No game code: no gates move.
- **B12 DONE** (S, the lead, 00:46-01:00): Tools/night/n46-flow-geo.cjs takes "w1 in view" off the spawn's facing (offFacing(): the bearing
  off -z plus the spawn's yawDeg, wrapped). Planted case: n57-kiln kb2c 4 / 4 -> 1 / 4 (the hand count, B1). Older cases: the geometry
  lines of n52-foundry, n52-iceline, n52-span, n52-spire, n58-span byte-equal before / after, and n57-kiln's other rooms too (only kb2c has a
  non-zero spawn yaw in the six levels; Logs/n60/b12/before-*.txt, after-*.txt). The --flow baseline re-made (the tool changed, not the
  game): it differs from the old one in kb2c's geometry column only (Logs/n60/b12/flow-base-before-b12.txt vs Logs/n60/flow-base.txt).
- **B3 code landed on builder V's branch** (00:05-00:46; 88476cef test first, ac964137 the swarmer / spitter reporters + ProbeMaxEnemies
  8 -> 32, bde692e0 n60-v-yellow.py, c233188c n60-build-num.cjs): the lead's pick, RED / GREEN, the meter's fresh review and the Player sKeep
  scan come next. Builder V took B7 at 00:47 (D-11).
- **B9 re-sized S -> M** (00:43; D-12): no Player path starts a run outside the smoke or a reel (reels set FeelReel.Running, which the pause
  rule skips; the smoke's SmokeGuard55 undoes the pause); Pause() logs nothing. It needs a new automation probe.

- **Session 3 (05:06, after a compaction)**: section 0 again (NIGHT60.md re-read in full; LESSONS 1-118, the log and the backlog read).
- **B15 DONE: the blow pipe fix stays in level 3** (critic B15 PASS 05:05, Research/2026-10-21-wave60/gate-b15.md): put-ats 0 / 1 / 1 ->
  0 / 0 / 0 on seeds 1-30 (the critic's re-run of seeds 21-30 byte-equal), guards in band, verbs37 31 / 31. Its question for GG: the pad
  still launches onto the fettlers' walk, but from the furnace top: the far end by d6b lost its way up (two launches now).
- **B9 attempt 1 in the Player: the game never paused** (05:09, Builds/n60-b7, `n60-focus-probe.sh n60-b7 green`; Logs/n60/b9/
  focus-n60-b7-green.txt): the steal took the foreground ("took True"), the Player printed "10.67 s focus LOST (flow Playing, cursor
  Locked)" and "pauses 0", DONE False. The cause: InputReader.Locked reads Cursor.lockState, and a standalone Player keeps it Locked after a
  focus loss, so SurvivalGame.PausesOnLockDrop never fires there: alt-tab never paused a Player (the Editor drops the lock, so only
  EditMode pinned it). Backlog B18.
- **B9 attempt 2 picked, Player MAIN met** (builder L 05:15-05:30; ff57b3c5 test first: N60FocusPauseTests + N60FocusRowTests RED 05:33
  (2 failures: "focus60._why", "Playing, the window lost the focus: Paused"), c6a6e196 the rule (PausesOnFocusLoss beside the lock line,
  only with design.n60.focus60 on; off under -skygrindOff60, -skygrindNoFocus60 and -skygrindBench) GREEN 8 / 9, the row commit 9 / 9
  05:37; the lead's comment fix 224d8338 (the pick had moved the NIGHT56 comment onto the new line); gates GREEN 05:40:21). Builds/n60-b8
  (focus60 ON in its copy): first three runs printed "focus losses 0": the script timed its steal off the Player's log, which reaches the
  file 6-14 s late, so the steal fell after the probe's 20 s window (L-115); fixed (228e860b: the steal 7 s after the focus step; MISSED
  printed when the probe sees no loss). Re-run 06:00-06:02 (Logs/n60/b9/focus-n60-b8-*.txt): row ON "11.34 s flow Playing -> Paused ...
  focus LOST", DONE True; -skygrindNoFocus60 "14.41 s focus LOST (flow Playing, cursor Locked)", pauses 0, DONE False (today's game); planted
  -skygrindFocusProbe60NoPause "set back to Playing by the planted probe 1", DONE False. Still needed before ON: the smoke with the row on,
  verbs37 x5 (running), a one-row critic.
- **B8 attempt 2: MAIN not met; lamp60 stays OFF** (picked 7ce08c33 the clear shot waits for the room's own clear; 51e4b04f test first:
  N60LampTests RED 05:2x ("Expected -193.37 / But was -195.5": the door's centre); 338cfa30 FaceAnchor GREEN 2 / 2 05:27; gates GREEN
  05:31:39). Builds/n60-b8 a157 -skygrindClearShots60 ON vs -skygrindNoExit58: every clear shot "room cleared yes (phase Cleared, alive 0)"
  (attempt 1's frame had "15 LEFT"); Logs/n60/b8a2-lamp.txt: door luma / room median >= 1.5x ON 4 / 5 rooms (kb3c 0.85x) vs OFF 2 / 5 (kb2
  2.40x, kb3d 2.60x already); the lamp's own gain (door luma on / off) 1.02-1.14x; GUARD 5 / 5. The lead looked at kb3's clear frame on / off:
  the lamp draws (a cyan frame a little wider round the door, 25 m away) but barely moves the door. Two attempts and ~3 h: stopped (Blocked).
- **B7: critic PASS** (05:58, Research/2026-10-21-wave60/gate-b7.md): re-ran the MAIN byte for byte (75.7 -> 90.1 %; ON beats the best OFF
  by 13 points, ~9x the OFF spread), the guards on kiln and iceline equal; notes: "about 90 %" (one body is 0.7 points; one pair not
  same-pose), 16 of the 21 bodies gained come from the darker accent; rooms read a little darker and flatter. rim56 turned ON in both
  copies (n60-freeze-rows.cjs, 06:03); the flow gate `n60-gates.sh --flow` running.
- **The audit pass merged** (05:20-05:45): audit-L.md (20 findings, 7a2d31cd) and audit-V.md (6, 1ba724c1 / 6b8b5834) -> backlog B18-B29.
  B19 / B20 settled from the code (a pad can take the round-end card: UiNav + A; it was never run; the pad toggles the x-ray); B21 (builder L,
  10adcfc8): T-91's 240 s cap not reproduced on n52-kiln (4 x 40 seeds; every cap left is a burrower 5.6 m under kb2c's floor); B24 (builder V,
  2026a6f4): the PULSE pips render at hue 332, 2 deg outside the meters' RIVAL arc (planted RIVAL 100 % inside): GG's call.
- **The fresh diff review of tonight's C# code** (B1, B2, B3, B7, B8, B9 + B8 a2 / B9 a2; 05:24-06:15; Research/2026-10-21-wave60/
  review-n60-code.md): 13 findings, 0 must-fix, 6 should-fix. The rim56 terminator (computed 0.47 -> 0.94) measured on the frames instead:
  body V spread median 0.50 off -> 0.52 on over 142 bodies, planted cel / flat GREEN (Logs/n60/b7-terminator.txt; DECIDE60 D-14: rim56
  stays ON). The other five wait for NIGHT61 with their next steps (D-15): rim56's row read twice a frame (~0.5 KB / frame, estimated);
  focus60 not excluding the Player tours (-skygrindShots / Strip / Tile / Hitch, DoorFree29Check, LevelSelect29Check); the Editor's
  never-locked runs; n60-v-lamp.py ignoring "cleared"; lamp60's per-frame line (OFF).
- **B7 DONE: rim56 ON** (ba80cfee, 06:13): the flow gate with rim56 ON: `gate flow --off60: EQUAL to flow-base`, all GREEN 06:12:42
  (Logs/n60/gates-rim56-flow.txt).
- **B18 / B9 DONE: focus60 critic PASS** (06:30, Research/2026-10-21-wave60/gate-b18.md): MAIN True on / False off in the same build;
  GUARDS: planted False, the smoke with two planted steals "228.7s DONE: all steps passed" (Logs/n56/smoke-steal-n60-b8.out; SmokeGuard55
  resolved the row's pause), verbs37 27 / 29 / 31 / 31 / 31 with the row on, 0 NREs; no trap (Pause() acts only while Playing; Esc, START,
  B and RESUME leave it). Its notes: only the first smoke steal took the foreground from the Player; rim56 ON and focus60 ON have not run
  together in one build (the n60-freeze build does). focus60 turned ON in both copies 06:30:54; the flow gate running.
- **B23 (S) the two reels** (06:28-06:30, Builds/n60-b8; Logs/feel/n60-b8/summary47.txt): a148 with -skygrindReelPad GREEN: "lines bad 0
  of 14", "crack@1080 ok: <sprite padglyphs xboxrt> through the crack. em 40.0 px": the lead looked at shot-N29-W48-g5-crack-1080.png (the
  RT key glyph, then the line, over THE WICK's yellow core; the weapon panel "KESTREL LT ZOOM"): T-98 closed. a349 on n52-iceline: RED, the
  combat frame 2 / 4 (saturated 10-30 no, hue bins 3-5 no): T-104's iceline still 2 / 4, measured (it was "not run"). 0 NREs both.
- **The builders' last items picked**: the wave-60 harvest draft (b348afee, builder L: ledger, THREADS, FEATURES; the lead finishes it in the
  close) and NIGHT60-PROPOSALS.md P2 / P3 (18d173b0, builder V).

## The close (NIGHT60.md section 8; started 07:06:00 = 8 h 00 m since t0)
| step | result | evidence |
|---|---|---|
| 1 the final check command | `gates: all GREEN 07:07:42` (config identical; golden 0 failing, 0 mismatches, 22 / 22 replays exact; fingerprints plain / --off57..--off60 EQUAL; Editor 0 errors) | Logs/n60/gates-final.txt |
| 1 n60-freeze | built 07:08 from ed5624dd; its config = the repo's (kill55, rim56, focus60 ON; yellow56, lamp60, span58 OFF) | Builds/n60-freeze |
| 1 verbs37 x5 | 27 / 29 / 31 / 31 / 31, RED rows none, 0 NREs | Logs/feel/n60-freeze/summary47.txt |
| 1 the smoke | `[smoke] 227.9s DONE: all steps passed`, two planted steals took the foreground | Logs/n56/smoke-steal-n60-freeze.out |
| 1 B11 the Game suite | A-L and N-Z: 1006 / 1006 passed, 0 failed (07:18-07:33); M by class: M9Fade25Tests 4 / 4, MapChoiceTests TIMEOUT after 606 s (the hanging M class, named for the first time: T-147), 9 M classes not run; the runner stopped by PID 07:47 (TaskStop left it alive: L-088), the Editor never killed | Logs/n41-suite-n60.txt, Logs/n41-suite-n60m.txt |
| 2 LESSONS | L-113 / L-114 / L-115 added; L-080 merged into L-104; six rows re-confirmed at w60; 119 lines | docs/memory/LESSONS.md |
| 3 NIGHT60-REPORT.md | written | NIGHT60-REPORT.md |
| 4 HANDOFF, harvest, L-107, tag | HANDOFF.md rewritten; the harvest: ledger/wave-60.md (60 lines), THREADS (60: T-91 / T-67 / T-98 / T-130 / T-138 closed, T-140..T-147 added), FEATURES, CORE (40) rebuilt; `git tag -l u3-n60` and each ON row's `enabled` printed after the tag (L-107) | docs/memory/ |

## Assumptions (each a DECIDE60 id)
- D-10: t0 is this run's first `date` (23:05:36), not the session's first `date`: GG set NIGHT60's /goal in the session that forged it
  (the session began 13:59 with NIGHT58), and NIGHT60.md section 0 step 4 defines t0 as the run's start.
- D-11: builder L takes B14 before B10 (B10 needs tonight's first sim row: a design.n60 block and its plumbing, a shared commit first);
  builder V takes B3's next steps (it knows the yellow56 code) while the lead verifies B2 in the Player.
- D-12: B9 re-sized S -> M (it needs a new Player automation probe: no path starts a run outside the smoke or a reel).

## Blocked
- B3 (yellow56 keeps the halo): tried sKeep 0.85, 0.75, 0.65 in the Player; at every value either the MAIN or the halo guard fails, because
  a spitter's parry tell and spire's world yellow share hue / saturation / value; best guess: only a mask on the tells separates them
  (NIGHT60-PROPOSALS.md P1).
- B8 (lamp60, the exit lamp): two attempts (the clear shot fixed; the frame moved out of the 4 m doorway walls); the lamp's own gain on the
  door is 1.02-1.14x and kb3c's door reads 0.85x of its room; best guess: the lamp is too small and dim to change a door 25 m away, and the
  absolute 1.5x MAIN is mostly the scene's own light (OFF already passes 2 / 5). Next: a brighter / larger lamp, or a relative MAIN (the
  lamp's own gain >= 1.5x) written before the numbers.
- B4 / B5 (level 4's GAP NEST): two contract items are GG's calls: a ride wall that does not cross the gap (or the item waived for this
  card), and a flyer room over a 10 m deck above the 16 m height band (or the drones' roof behaviour, or a lower far deck).

## Unverified
