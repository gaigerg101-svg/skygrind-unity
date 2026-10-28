# NIGHT60 backlog (top to bottom; seeded 2026-10-21 ~22:35 from NIGHT59's close table, THREADS T-135 / T-139 and DECIDE59)

Sizes: S <= 30 min, M <= 90 min (one worker, verified), L = more: spec it in NIGHT60-PROPOSALS.md, do not build. Every MAIN and GUARD is
fixed here BEFORE any number is seen (L-100); a row ships ON only by NIGHT57.md section 0's rule. "Before" numbers cite their files.

## Seeded by the lead (NIGHT59 close)
- [x] **B1 (S) kb2c's spawn view (NIGHT59 L0's open caveat).** DONE 23:42 (NIGHT60-LOG): seen 0 / 5 from the spawn; hand count 1 / 4
  nodes vs the harness's 4 / 4 (it ignores the yaw). A reel frame from kb2c's spawn pose ((-2, 0, -168), yaw 45) on n57-kiln
  (a157 frames each room at its fixed entry pose, so no frame from the spawn exists: gate-l0.md). MAIN: the frame exists and the lead reads
  it; wave 1's nodes in its view counted by hand vs the harness's "w1 in view" (the critic doubted its 4 / 4). Files: FeelReel.N57.Level.cs
  or a new FeelReel.N60.Level.cs partial; no game change.
- [x] **B2 (M) kill55's marks near-black (design.n55.kill55, OFF).** DONE 01:18: kill55 ON (critic PASS; 0.42 % off -> 15.29 % on). Before: marks at the flash end 0.46 % on = off = the old draw; the marks
  darken the body by > 0.12 V on 5.89 % of it at median V 0.33 (Logs/n59/b2/g-probe-on-off.txt; L-111). Change: the Kill59Mark colour toward
  V < 0.12 (a Unity-only number in UnityTuning.N59G.cs). MAIN: marks at the flash end >= 5 % (a258 on vs -skygrindNoKill55) AND the a355
  ladder ok. GUARDS: no whole-frame change outside the marks (n56-colour.py cover lines); verbs37 x5 equal. One-row critic if both pass.
- [x] **B3 (M) yellow56 keeps the halo (design.n56.yellow56, OFF).** BLOCKED 01:39 after 3 attempts (sKeep 0.85 / 0.75 / 0.65: no value
  passes the MAIN and the halo guard together); the probe fix and the new meter landed; the way on is an L proposal (P1, a tells mask). Before: frames over 0.5 % non-parry 11 / 12 off -> 0 / 12 on; the halo kept
  90.4 % off -> 56.9 % on (Logs/n59/b2/halo-*.txt; guard 80). The MAIN meter counts the halo's less saturated rim as non-parry (gate-x.md).
  Change, in this order: (a) the meter folds the halo's own connected pixels into the parry tell, with planted cases (old and new) that
  still FAIL where they must (L-106: a fresh reviewer reads the meter diff); (b) only then a lower sKeep (n59-v-rows.cjs). MAIN: <= 1 / 12
  frames over 0.5 %; GUARD: the halo kept >= 80 % (n59-v-halo.py); the spire v5-mid orbs' share within 10 % RELATIVE of the off side
  (n58-v-yellow.py's "orbs" figure; NIGHT59 15.42 off -> 17.29 % on = +12 % relative would FAIL); verbs37 x5 equal.
- [x] **B4 (M) level 4 step 1: split THE GAP NEST's slot.** SHORT 00:33 (12/14; the two left are GG's calls: NIGHT60-REPORT "Your calls" 1). Before: p2 contract 11/14 (size 36 x 50 x 22 m vs 20-32 m long and 10-16 m high,
  density 0.72 vs >= 2.5, no ride wall; Logs/n59/L/contract/n58-span-c3/table.md; L-112). Change: p2 becomes a room <= 32 m long + an approach
  link (gen-n57-kiln.cjs's slot DE is the model), in gen-n58-span.cjs (p2 stays out of NEW until it passes). Measure in a builder's
  worktree on the level built with `node Tools/night/gen-n58-span.cjs --slots=p2`, levelgen, `--finish` (that rewrites n58-span.json: on
  port/stage3 it stays n52's until p2 passes; NIGHT58 EXT 56681aee); re-run levelgen after each cut so Reports/LEVEL-n58-span-validate.md
  matches the plan. MAIN: p2 contract 14/14 with `OUT=Logs/n60/L/contract/n58-span-p2 bash Tools/night/n59-l-contract.sh n58-span p2`
  (p2 is a link in span's plan, so name it; OUT keeps NIGHT59's Logs/n59/L evidence) and its planted run (`... n58-span planted` must
  FAIL), and walk 10/10. GUARDS: span stall <= 14.5 % +
  band; deaths in band; put at the next room <= 1 per set; p2 falls within the void rooms' range (seeds 1-10: p3 55, p5 42; 11-20: p3 78,
  p5 57). The card's spatial law (the pull is the only fast way across) stays: a ride wall needs GG (DECIDE59 D-10).
- [x] **B5 (M) level 4 step 2: a158 and the Player.** NOT STARTED: waits on B4's two calls (GG; DECIDE60 D-8). FeelReel.N58.Level.cs (the N58Level partial is pre-placed in FeelReel.N58.cs) from
  FeelReel.N57.Level.cs; a build with span58 ON in its copy only; a158 + verbs37 on n58-span; p2 into NEW; the one-row critic's verdict.
  STOP there: span58 stays OFF in the repo, because GG has not answered NIGHT59-REPORT's call 2 (split the room or keep the long gap;
  DECIDE60 D-8). Only after B4 passes.
- [x] **B6 (S) kb3c's stall (NIGHT59 critic caveat).** DONE 23:52 (NIGHT60-LOG): every kb3c death is in THE TAP RUN's lava; the bot is
  stranded in a closed 3.4 x 4 m pocket between THE TAP HOOD, THE RUN WALL and the lava (NO PATH): B14. Before: n57-kiln stall 18.9 / 20.5 / 18.8 -> 20.3 / 22.9 / 22.7 % with L0, mostly in
  kb3c (stall 35 %; 6 of 7 deaths on seeds 11-20; Logs/n59/l0-B.txt). Done when: a cause note in NIGHT60-LOG.md citing kb3c's per-seed
  rows (n46flow's per-seed print and its F1 trace: read Harness.N46.Flow.cs for the flags; a worktree-only one-line print is allowed, not
  committed, as NIGHT58's L did) with where the bot stands still. A data cut only as a new backlog item with its own MAIN.
- [x] **B7 (M) rim56's trims (design.n56.rim56, OFF).** DONE 06:13: rim56 ON by the rule (75.7 -> 90.1 %; gate-b7.md PASS; flow EQUAL). Before: bodies over the world's p90 78 / 99 off -> 75 / 98 on (Logs/n58/r1/colour-v1*.txt);
  the accent dim never reached the frame (V.md, Research/2026-10-21-wave58). Cause first: which draw path the trims take. MAIN: bodies
  over the world's p90 (n56-colour.py's V2 line on a357 frames, as Logs/n58/r1/colour-v1*.txt) >= 90 % overall across the five levels.
- [x] **B8 (M) the exit lamp (V3).** BLOCKED 05:49 after 2 attempts (lamp60 OFF; the row and its meter landed): MAIN 4 / 5 rooms ON vs
  2 / 5 OFF, the lamp's own gain on the door 1.02-1.14x (Logs/n60/b8a2-lamp.txt; NIGHT60-LOG Blocked). Its switch is a compile-time constant (UnityTuning.N58V.cs `Exit58On = false`; Exit58.N58V.cs reads
  Off58 / NoExit58), so n46-build-rows.cjs cannot turn it on and -skygrindOff60 cannot turn it off. Step 1: move it behind a `design.n60`
  row (OFF) and write its meter with a planted case. MAIN: the exit door's luma / the room's median at the clear frame >= 1.5x on n57-kiln's
  new rooms; GUARD: the room's p95 luma outside the door's mask within the planted case's tolerance of the off side. ON only by the rule.
- [x] **B9 DONE 06:02 (the probe and its RED / GREEN; it found B18) (M; re-sized from S at 00:43, DECIDE60 D-12: needs a new Player automation probe) T-138 the focus pause outside the smoke.** A Player run without -skygrindSmoke that loses focus while Playing must print
  Paused (n56-foreground.ps1 is the planted steal). MAIN: the line printed once RED (a planted no-pause) and once GREEN.
- [x] **B10 (M) tokens56's iceline idle (ON row).** NOT MET 04:56 (cause found; idle60 on branch n60-b10 only, DECIDE60 D-13). Before: 18.4 % / 20.2 % of iceline's enemies idle > 20 % of their time (n46flow's
  `n52-iceline IDLE` line, "over 20 %: 113 (18.4 %)", Logs/n58/flow-base.txt line 52, and flow-base-B.txt; by kind drone 32.6 %);
  cause noted (drones holding the ranged token hover in view: Research/2026-10-21-wave58/G.md). The change goes behind a NEW `design.n60`
  row (tokens56 is ON: a change inside it would ship with no off switch). MAIN: <= 10 % on both seed sets; GUARDS: deaths and clear in band.
- [x] **B11 (S, lead, in the close only) the Game suite** DONE 07:47: 1006 / 1006 without M; M: M9Fade25Tests 4 / 4, MapChoiceTests
  TIMEOUT 606 s, 9 M classes unrun (T-147)., after NIGHT60.md section 8 step 1 (the Editor no longer needed): A-L and N-Z,
  then M by class alone; a timed-out batch is logged TIMEOUT. It never counts toward "no S or M item left".

## Proposed by Claude (the audit pass appends here: id, size, one-line reason)
- [x] **B12 (S) n46-flow-geo.cjs's "w1 in view" ignores the spawn's yaw** (found by B1). DONE 01:00: kb2c 4 / 4 -> 1 / 4, every other
  room byte-equal on six levels; the flow baseline re-made. `bearing()` measures off -z, so a rotated spawn
  (kb2c, yaw 45) prints 4 / 4 where the real facing holds 1 / 4. Fix: the bearing off the spawn's facing. MAIN: kb2c prints 1 / 4 (the
  planted case); every yaw-0 room's geometry line byte-equal before / after on all five levels (the older cases). The --flow baseline is
  re-made in the same commit (the tool changed, not the game; say so).
- [x] **B13 (M) kb2c's respawn faces a wall** DONE 04:11 (critic PASS, gate-b13.md: seen 0 -> 5 of wave 1 from the spawn). (found by B1): after a death in THE CLINKER BRIDGES you face the west wall and the near
  bridge's face 2-4 m away, 0 of 5 wave-1 enemies seen (the yaw 45 was chosen so spawn31's 12 m rule passes: wave 1's nodes sit 10.8-12.4 m
  from the spawn). MAIN: from the spawn, >= 2 of wave 1 seen in the Player's spawn frame (a157 -skygrindSpawnShots60) with spawn31 still
  passing (e.g. wave 1's nodes moved >= 12 m out and the spawn facing the fight). GUARDS: L0's numbers on seeds 1-30 (put at the next room
  <= 5 per set, deaths pooled <= 9.9, stall <= 23.65 %), kb2c / kb3 contracts 14/14, verbs37 n57-kiln 31 / 31. kiln57 is ON: one-row critic.
- [x] **B14 (M) kb3c's tap-hood pocket** (found by B6): DONE 02:24 (critic PASS; deaths 8 -> 0, NO PATH -> 0, put-ats 8 -> 1). a closed 3.4 x 4 m corner (THE TAP HOOD x 2..7 floor to roof, THE RUN WALL x 10.4,
  the tap run's lava, the north wall) at the head of the room's hazard, reachable only out of the lava or off the ride. Fix in
  gen-n57-kiln.cjs (kb3c): open the pocket on foot (e.g. THE TAP HOOD 1-1.5 m shorter on its south or east side) keeping the hood's job
  (the foothold's cover) and the run wall's ride. MAIN: kb3c's NO PATH trace lines 1591 -> 0 and its put-ats 2 -> 0 on seeds 11-20
  (and seeds 1-10 / 21-30 no worse). GUARDS: kb3c deaths no worse (seeds 1-30), n57-kiln stall <= 23.65 %, kb3c contract 14/14
  (`n59-l-contract.sh n57-kiln kb3c`), n59-l-room table PASS, verbs37 n57-kiln 31 / 31, a157 in view kb3c >= 3 / 6. kiln57 is ON: one-row critic.
- [x] **B15 (S/M) kb3c's blow pipe launches the bot past d6b** DONE 05:05 (critic PASS, gate-b15.md: put-ats 0 / 1 / 1 -> 0 / 0 / 0;
  a design question for GG: the pad now needs two launches from the furnace top). (found by B14, builder L): on seeds 11-20 one put-at remains after B14 (seed 14;
  cut 1's seed 20 the same): THE BLOW PIPE pad at (-6.5, -251), 2.5 m north of d6b's mouth, throws the bot onto the fettlers' walk on its way
  out; it lands past its next waypoint and stands at d6b's west jamb until the 15 s rule. MAIN: kb3c put-ats 0 on seeds 1-30. A person: a
  pad 2.5 m in front of an exit door is L-087's "a pad on a walk line"; the fix moves or turns the pad, its job kept.
- [x] **B16 (S) n60-v-yellow.py's fold bound** DONE 01:57: planted 10 / 10 (cases 9 / 10 RED on the old rule, GREEN on the bound); the
  sKeep scan's verdicts unchanged (0 / 0 / 7 / 11 of 12). (the meter's fresh review, Research/2026-10-21-wave60/review-b3-meter.md: SOUND WITH A BOUND):
  fold a 4-connected yellow component only when >= 90 % of its pixels lie within TELL_PX of an enemy / boss rect, then only its part outside
  the rects; add the reviewer's two planted cases (a floor band and a tell-coloured plate behind an enemy, both must FAIL) and rewrite case 7.
- [x] **B17 (S) a giant enemy rect** DONE (cause only) 02:02: the honest projected AABB of a body beside the camera; no S fix
  (NIGHT60-LOG "B17 cause noted"). (the same review): on spire v6-mid one enemy rect is [1286.9, 906.8, 779.8, 1441.1] (5.3 % of the
  frame, the gun included) and two rects lie fully off-screen: FeelProbe.ScreenRect for a body behind or beside the camera; every meter that
  subtracts enemy rects loses that area. Cause first (FeelProbe.cs ScreenRect), a planted case, then clamp or drop such rects.

## The audit pass (05:00-05:20; builder L: Research/2026-10-21-wave60/audit-L.md, sections d / e / f; builder V: audit-V.md, h / i)
Tonight's Player logs (a) and the Editor's errors (b): 0 NREs / exceptions in 32 Player logs, 0 console errors (02:30, NIGHT60-LOG). The
Core suite (c) and the frame time (g) were not run: the Editor and the Player were busy with the backlog's Player checks all night.
- [x] **B18 (S, in flight as B9 attempt 2) the focus pause never fires in a Player** DONE 06:30: focus60 ON by the rule (gate-b18.md PASS). (found by B9's probe, 05:09): the n60-b7 Player lost
  the focus while Playing and did not pause ("cursor-lock drops 0, pauses 0", cursor Locked, focused False): SurvivalGame.PausesOnLockDrop
  reads Cursor.lockState, which a standalone Player keeps Locked when the window loses the focus. Fix behind a new OFF row design.n60.focus60.
- [x] **B19 (S) A-V3: can a pad player take the round-end card?** SETTLED from the code 05:44 (audit-V.md, 6b8b5834): yes: UiNav selects
  card 0 when the reveal opens (UiNav.cs:376-384) and the pad's A submits the card's Button.onClick (UiNav.cs:168-187; UiReveal20.cs:88-91).
  No run has ever taken a card on a pad: SmokeGuard55 resumes past a reveal (SmokeGuard55.cs:168) and takes a draft card by code (:167).
  The Player proof (a reel step: open a reveal, wait the 0.8 s turn-over, press the virtual pad's A, print the press and the pick) is M: not tonight.
- [x] **B20 (S) A-V5: the x-ray toggle's pad path** SETTLED 05:44: d-pad down in every layout (Gamepad.cs:261, :290) -> PadReader.cs:397 ->
  SurvivalGame.cs:236-241 toggles the setting; the keyboard read is a separate path (no double toggle).
- [x] **B21 (S) A-L1: T-91 re-measured with idle60 on** DONE 06:00 (not reproduced; L-b21.md, 10adcfc8) (n52-kiln's 240 s caps "with a drone high in the air" may be B10's climb freeze):
  n46flow `--rows chargerKiln48,idle60` on n60-b10's code vs without.
- [x] **B22 DONE in the close (ledger, THREADS, FEATURES, CORE) (S, the lead's harvest) A-L2 / A-L3 / A-L6 / A-L17 / A-L18: threads and verbs rows that tonight's numbers close or name**:
  T-67 (the gantry: n52-foundry GREEN 10/10 in flow-base), T-124 (spire re-flowed: stall 17.5 %), T-138 (after B18), VERBS rows 36 / 37
  name their checks (the smoke's hold-E buy; rea34's line).
- [x] **B23 (S, the lead, Player) A-L4 / A-L5: T-98's pad pass (a148) and T-104's iceline frame (a349)** DONE 06:30: a148 pad GREEN (RT
  glyph drawn: T-98 closed); a349 iceline combat 2 / 4 (T-104 measured, still open).
- [x] **B24 (S, GG) A-V1: the PULSE pips are pink** MEASURED 06:00 (V-b24.md: hue 332, 2 deg outside the RIVAL arc): GG's call. (#FF6FB0, h 333, 3 deg from the meters' magenta arc; rule 8 has no pink family):
  one frame's hue measure, then GG's call (keep, or cyan).
- [ ] **B25 (S, GG) A-V4: the 16 px rule** passes as font size (every text >= 20 px); if GG means glyph height, the 20-22 px texts
  need a measure.
- [ ] **B26 (M) A-V2: docs/VERBS.md's pad column vs the code's layout "v3"** (dash, grapple, weapon swap disagree; read v3, rewrite,
  list keyboard-only verbs).
- [ ] **B27 (M) A-V6: spire's accent (h 345) and foundry's (h 268) sit 15-17 deg from the danger / magenta arcs**: the share of world
  pixels inside the role arcs on vs -skygrindOff56.
- [ ] **B28 (M) A-L8 / A-L9 / A-L10 / A-L11 / A-L12: aged threads with a written next step** (T-75 the Game walk pilot's lava rule; T-82 a
  void fall as a kill for the pull; T-83 THE WICK's climb top third; T-100 IgnoreFocus in every pressing reel; T-52 / T-53 the bend's
  game-time length) and **A-L14 / A-L15 / A-L16** (idle60's second cut: the heavy holders; press57's meter on stick-at-rest steps; step55
  re-tried), **A-L19 / A-L20** (verbs37 rides a glide rail; the pad column re-performed). Not tonight (no new M after 05:35:36).
- [ ] **B29 (M) A-L7: T-102's missing ledgers** (w47 / w48 / w50 / w51 harvests). Not tonight.
- A-L13 (L): the stack-climbing and wall-riding pilots (T-59 / T-66): NIGHT60-PROPOSALS.md P2.
- GG's own calls (aged, no worker step): T-18, T-20, T-57, T-79, T-103, T-107, T-112, T-117, T-132, T-134, T-136.
