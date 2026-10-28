# NIGHT41-LOG: the finish night (lead + eight builders; NIGHT41.md)

t0 = 2026-10-14 09:06:54 (the night's first `date`; GG's go is the /goal message). Hard cap 15:06:54.

## G0 (0:00-0:30)
- 09:06:54 `date`. The Editor: ready, stopped, not compiling (Tools/unity-loop.sh status). Disk C: 103 GB free (GG freed space after
  DECIDE40 D-0). Memory: 7.9 GB free of 63.1 GB (three Unity processes since 10/10, 3.7-4.2 GB working set each). Branch port/stage3 at
  e0be68a8 (the forge commit). GG's four modified files left as they are (never staged).
- GG's words first: no message from GG since the forge (08:41-08:57). DECIDE41 D-1..D-7 and DECIDE40's open rows (D-3, D-4, D-13, D-14)
  have no answers: every card runs as written on its default (D-2 foundry41 builds the no-ride band and may turn ON; D-3 level 6 plan only;
  D-4 the light colours; D-5 two wind-ups; D-6 pad assist only; D-7 the previews). No card changed.
- The shared commit (Tools/night/n41-config-block.cjs, Tools/night/n41-shared-patch.cjs; new Core/N41Blocks.cs, Core/WeakPoint41.cs,
  Game/SurvivalSim.N41.cs, Game/N41View.cs, Tests N41SharedTests.cs):
  - N41Blocks: sweep41, tokens41, heavy41, aim41, foundry41, kiln41, spire41, finale41 (8 bits); tape format 24 (a VarI after NIGHT40's;
    formats 1-23 replay with every NIGHT41 block off); `--off41` / `-skygrindOff41` (PlayerChecks, N29Fixes.IsAutomationArg, SimHarness);
    FingerprintBaseline.N41OnPath false.
  - design.n41: 18 rows, every one off (8 sim, 10 view: juice41, juice41On, doors41On, pump41On, bars41, entrance41, select41, look41,
    brawlLook41, models41). design.maps.list: n41-foundry, n41-kiln, n41-spire, n41-finale registered (campaignOnly) with placeholder files
    (byte copies of n38-foundry / n40-kiln / n40-spire / n35-finale with only the name changed; both copies), so the list never points at a
    missing file.
  - Curriculum25.N41Levels (the level rows' swaps after N40Levels; finale41 puts level 6 back as the last level when on).
  - SurvivalSim.N41BaseId: an n41 level reads as its levelFrom to every older system that knows a level by its id: the unlock table and
    NIGHT40's start sets / awards (SurvivalSim.Weapons31), kiln40's boost chain (SurvivalSim.N40.Kiln), the walk pilots (Walk38Foundry /
    Kiln / Spire), the verbs reel's NIGHT40 start rules (FeelReel.N35.Verbs).
  - The pre-placed hooks (L-021 / L-050): Enemy.weakPoint41 (aim41 reads it; B8 / P3 fill it); Loadout.Heavy41 + hitstop41 at the
    gauntlet's knock (E6's Core/Heavy41.cs); juice38 / doors38 / pump40 on through juice41On / doors41On / pump41On (the n38 / n40 rows
    untouched); the HUD33 boss bar and the Boss37 stagger bar hide while N41View.BossBar41Live (J2's bar); Lesson25Tests and
    N37TestBaseline force NIGHT41 off; the format pins move to 24 (AsFormat23 joins the chain); harness partials N41Proof / Juice / Look /
    Bosses / Enemies / Pad / Levels / Finale.
- 09:32 gates GREEN (Tools/night/n41-gates.sh): config copies identical; golden 39 sets / 22 replays, 0 mismatches; fingerprint EQUAL
  plain and --off41; Editor compile 0 errors. Editor tests: N41SharedTests 3/3, N40Shared 3/3, N39Shared 2/2, N38Shared 3/3, N37Shared
  3/3, TapeTests 7/7, Lesson25Tests 10/10. **Shared commit 3fa2c92a (09:41).**
- **The suite's batches proven (A6):** run_tests takes one filter string and matches a test's full name by substring
  ("Skygrind.Game.Tests.N41Shared" ran 3; "A|B", "A;B" and "A,B" ran 0; filter_type is testName / assembly / category only). So a batch
  is a name prefix: Tools/night/n41-suite.sh runs each namespace by the first letter of its classes (Core 137 files, Game 143), each call
  under 600 s, a no-message failure re-run once. The maps.list tests after the shared commit: MapChoiceTests 58/58 (54 + the four n41
  rows), Level5Tests 3/3, PortExportTests 27/27 (replays 22/22).
- **Eight builders launched 09:43** (T2, J2, K2, B8, E6, P3, R5, L6), worktrees from 3fa2c92a; integrate.sh's map filled. NIGHT41-BRIEF.md
  written (untracked): tape 24, no merge / rebase (the lead cherry-picks; `git show port/stage3:<path>`), hand-offs through the lead.
- dn-check-wt proven with a plant (09:48): the shared tree + a new Game file with a type error -> "planted: 1 error(s)" (CS0029 at
  Plant41.cs(1,89)): the checker sees a new file's error.
- VERBS.md: the NIGHT41 rows marked before any row lands (9 / 11 foundry41; 13 the reel's press; 17 / 19 / 22 / 23 spire41; 40 under
  tokens41; the gauntlet's big-enemy hit heavy41; 46 with sweep41; weak-point aim aim41). The reel runner Tools/night/n41-reels.sh (a 10 GB
  disk floor; summary41.txt). The before for the verbs: NIGHT40's freeze verbs (n40-r5; NIGHT40-LOG round 4 / freeze).
- The before, verbs per level (NIGHT40's n40-r4 reels, the r5 config; Logs/feel/n40-r4/summary40.txt, n40-r5 f5v2): L1 n38-foundry 26/26,
  L2 n40-iceline 20/20, L3 n40-kiln 24/25 (row 13 RED: slid False), L4 n40-span 30/30, L5 n40-spire 26/30 (rows 17 / 19 / 22 / 23 RED: "no
  grapple node in clear sight of a floor spot on this level").

## Round 1 (0:30-1:30)
- 10:12 L6 386d6672 (level 6's beat sheet, THE MIRROR: nine rooms 6.1-6.9, every verb of levels 1-5 two or three at once, a climb route
  per room, 6.9 THE RIVAL's empty arena; docs only). Resumed for the greybox + RIVAL.md.
- 10:14 K2 f63e8ab9 (look41 round 1: LightPlan33.Green named every room light "warm" and LightKit29.Named / CampaignLighting24 drew it
  0xffe6c8 on every level; now a key + fill per theme behind look41 (OFF): iceline c8dcff / a4b4ec, kiln ffbe78 / 7060b0, span dcc4f4 /
  8a6cd8, spire fff0c0 / 8078d8, foundry unchanged (the control); a reserved-colour check 0 / 30 fail; the prediction for level 2's floor:
  (155, 128, 111) -> about (121, 123, 141), blue above red). Watch: the kiln key is 19 deg from red-orange. Resumed for level 2's frost /
  ice sheen.
- 10:14 picked (L6 -> 0c898a3f, K2 -> ffaac970); 10:21 gates GREEN (golden 0 / 22 exact, fingerprint EQUAL plain and --off41, compile 0
  errors); Look41Tests 6/6 (10:23).
- 10:33 B8 2fefa87a (sweep41 round 1, sim + harness, OFF): `n41loom root` (sweep off): THE LOOM's attacks are lash / spool / web, the
  pilot is on the ground 75 % of the fight and takes 0 / 45 lash and 0 / 60 spool hits: standing answers everything (10/10, median
  51.8 s). The sweep: a 1.2 s tell, a line wall to wall in 0.9 s over a 36 m band, 3 m over the ledges in leg 1, 11 m in legs 2-3;
  answers: a ride, a rope, feet above the line; else 25 hp and a lift. With it on: stay pilot 9/10 (median 49.1 s; 26 sweeps: 6 hits,
  rope 7, height 7, ride 0), casual 8/10 (56.2 s; 10 hits of 28): not yet 10/10. Weak points filled (THE LOOM's spinneret / lit knee,
  THE ICEBREAKER's stuck fist). Event `loom41Sweep` (tell, hit, ride, rope, high).
- 10:34 P3 0349599d / f3a32a85 (aim41, OFF): the pad's assist mode steers to Enemy.weakPoint41 (the pad only: a finite breakaway cap;
  the mouse unchanged, D-6); THE CROWN's ring centre while locked / in the window (N41CrownStep). `n41pad aim` (5 seeds x 20 s, planted
  weak points), hits per second off -> on: stuck fist 0.78 -> 3.91, strafing CYAN core 0.10 -> 0.83, the ring 4.00 -> 4.00 (already its
  centre), a striding knee 0.00 -> 0.00 (the pilot sits ~8 deg behind, outside the 6 deg cone: a pilot fault, round 2).
- 10:36 E6 ad632f77 (tokens41, OFF): HuskBrain.windupCap41 (0 = unchanged); a think does not take a token while the cap is full, a
  commit that fills it releases the others' `ai.attack`; set every campaign step (campaignOn is known after N24StartRun);
  design.n41.tokens41.cap = 2. `n41foes level` (walk bot, 10 seeds), most at once / time above 2, off -> on: n40-iceline 4 / 3.14 % ->
  2 / 0.00 % (rooms 9/9), n40-span 5 / 0.61 % -> 2 / 0 % (7/7), n40-spire 3 / 0.08 % -> 2 / 0 % (10/10), n40-kiln 3 / 0.02 % -> 2 / 0 %
  (0/7 rooms: the general walk bot clears no room on n40-kiln or n38-foundry with the cap on or off: their own pilots, round 2).
- 10:37 picked: B8 -> fdbaa208, P3 -> 7df3f887 / 2d56fe54, E6 -> 6229ce94. 10:43 gates GREEN (golden 0 / 22, fingerprint EQUAL plain
  and --off41, compile 0 errors).
- 10:43 K2 05f7febf (round 2: Ice41View, level 2's frost crust round every wall base 0.18-0.7 m, sheen streaks along the floors, a glaze
  on g4's canal; behind look41, iceline only, no collider, no level data; a count from the file: 85 walls, ~2120 frost pieces, 72 streaks,
  1 glaze). Waits for frames.
- 10:44 J2 8a4f7040 / e9849bb2 / 0207a662 (bars41: UiBossBar41, one bar for all five bosses, a tick per phase threshold (Boss37 legAt,
  THE ICEBREAKER's phase2At, a kit boss's mark), the stagger meter under it, BossBar41Live set while it draws; entrance41: one 1.5 s beat,
  name + shake + flicker at 0 s, the rule line at 0.45 s, fade 1.25-1.5 s; juice41 numbers override juice38's while on). Open: the rule
  lines config (THE WICK and THE STAG have no BossLine40), a one-frame double bar at a fight's start.
- 10:45 T2 3fad5f8d / bc2f16d8 / d738ea28, **round 1 SHORT** (late start): the telemetry (fired, charge, pump, spin, spinKind, device after
  `vel` in every frame) and `weapons41` (KESTREL taps x3 at 0.5 s, the charge, the THUNDERCLAP volley + pump, the slug; frames
  W41-*-000 / 150 / 300 / 450); doors41 / entrance41 / pad41, A9, A11 and the pump41On script moved to its round 2 (entrance41 + doors41
  first: they gate juice41On / doors41On).
- 10:46 picked: T2 -> 32be701a / 7b862d5f / 235d2aae, J2 -> f8c02d69 / 3d2987a3 / d173c019, K2 -> 0aaf9007. Gates + build n41-r1.
- 10:50 gates GREEN; **build n41-r1** (0aaf9007, 10:51-10:52, Succeeded, 0 errors; 1 / 7 builds; the drift: only GG's two
  ProjectSettings files). Rows on in the build copy only (n39-build-rows.cjs): n41.pump41On, tokens41, bars41, entrance41, juice41On.
- Reels on n41-r1 (7 / 10 of round 1's; Tools/night/n41-reels.sh; Logs/feel/n41-r1/summary41.txt), 0 NRE / 0 exceptions in every one:
  - w41 weapons41 n38-foundry: **GREEN** (KESTREL taps 3/3, charged 1/1 held 0.90 s, the THUNDERCLAP volley grounded, the slug 1/1 held
    1.20 s; frames 23/23; pump40 drawing True). The volley's telemetry: pump -1 at +0 / +0.15, **0.53 at +0.30**, -1 at +0.45.
  - v1 verbs37 n38-foundry 26/26 (tokens41 on: row 40 parry PASS: parry36 1, return burst 1, health 50 -> 66); v2 n40-iceline 20/20;
    v3 n40-kiln 24/25 (row 13 RED as before: T2's A11 fix pending); v4 n40-span 30/30; v5 n40-spire 26/30 (17 / 19 / 22 / 23 RED as
    before: spire41 not in this build). = the before, unchanged with tokens41 on.
  - b2 brawl40 n40-iceline: **GREEN** (THE ICEBREAKER killed in 54.8 s; tells 4; haymakers 9, slams 3, a block parried, windows 5).
- **Frames looked at** (contact sheets / crops in the lead's scratch):
  - weapons41: volley +0 muzzle flash and pellets on the target; +0.15 the gun back at rest; **+0.30 the pump mid-stroke (the gun dipped
    and turned)**; +0.45 at rest. The pump reads and is done inside 0.45 s (the old web timing ran to 0.75 s). KESTREL tap: the kick at +0,
    at rest by +0.15 (the crosshair's turn is too small to judge at frame scale: spin 0.20 / 0.21 / 0.23 at +0.15 in telemetry); the
    charged shot and the slug flood the screen cyan for one frame at release (a blue-grey tint left at slug +0.15).
  - brawl40 (bars41 / entrance41 / juice41On on): the entrance card "THE ICEBREAKER" in red-orange and the rule line "GET IN CLOSE: LEFT
    CLICK THE GAUNTLET HITS FULL" on the arena / line frames; the top-centre crop over six frames: **one bar** (no HUD33 double), the name
    over a red-orange bar with a white tick at 50 %, the fill past the tick in phase 2, an empty grey bar at the kill. **Missing: no meter
    under THE ICEBREAKER's bar** (it has no Boss37 stagger): J2's iteration = its stuck-fist window as the cyan countdown. Also seen: "2.2
    THE SHIELD OPEN" over the fight (A9, T2's fix pending); the body still reads wall-tan (C6, B8's brawlLook41).
- 11:02 R5 12e8d0f3 (spire41: n41-spire = n40-spire + six grapple nodes, three in v2 THE NAVE, three in v6 THE CHOIR; none in the noHook
  gate rooms v3-v5 / v7, v9-v10 (the jump dash), v11 (the CROWN's own); `n41lv nodes` GREEN 6 / 6 attach, feet 0.62-1.52 m from the node;
  `n41lv walk spire` 10/10 median 129.9 s = the n40-spire control, 0 node attaches by the walk bot; gates 4/4, 4/4 with --ride40;
  fingerprint EQUAL plain and --off41). Resumed for foundry41.
- 11:15 J2 round 2: 4d2dc4bb (no double bar on a fight's first frame), e38e97c7 (the five rule lines in design.n41.entrance41.lines; THE
  WICK "RIDE A WALL, HOLD {fire}: SHOOT THROUGH A CRACK", THE STAG "CATCH IT ON THE ICE: {punch} ITS LEGS": new, not checked against
  play), 652cd894 (UnityTuning.Doors41.cs: scales for the curtains, all 1 until frames), 75624db0 (select41: the boss name per slot in
  red-orange, slot 6 "6  REBUILDING"; **the drawn silhouette SHORT**: no art; the name stands in).
- 11:16 picked: J2 -> 74aebecd / 6e12d106 / c336aa38 / deba7b35, R5 -> ef50cdaf. Gates + build n41-r2.

## Round 2 (1:30-2:30)
- 11:21 gates GREEN; **build n41-r2** (11:22, Succeeded, 0 errors; 2 / 7). Rows on in its copy: pump41On, tokens41, bars41, entrance41,
  juice41On, look41, spire41.
- Reels on n41-r2 (4; round 2 total 4 / 8), 0 NRE each: v2 n40-iceline 20/20, v3 n40-kiln 24/25 (13 RED), v4 n40-span 30/30, **v5s
  n41-spire 30/30** (rows 17 / 19 / 22 / 23 RED -> PASS: 17 RopeAttach 1, the chest within 0.62 m of the node; 19 held 1.6 s within
  0.93 m; 22 bend34Start 1; 23 the swap in 2 steps). Frames (v5s f1380 / f1395 / f1405 / f1625): the cyan rope to the nave's ring, the
  cyan node closing in, then the camera at y 13.8 under the roof (the whole view dark); R5's iteration: the ring's clearance.
- 11:23-11:30 reports: K2 4a878d4b (the before numbers: mean floor RGB of a fixed 500 x 80 rect, frames f0003 / f0005 / f0007 of each
  verbs reel, the three within 0.1: L1 (142.7, 101.5, 86.0) B/R 0.602, L2 (150.1, 129.5, 119.3) 0.795, L3 (140.2, 98.0, 81.5) 0.581, L4
  (142.3, 102.0, 89.8) 0.631, L5 (137.3, 95.8, 79.8) 0.581); T2 717360fc (entrance41: DOORFREE with the banners cleared, the player put
  at the boss room's spawn after 3 s, frames E41-<boss>-walkin / 000 ... 1600 after bossRise, probes printed; doors41: no DOORFREE, the
  first door out of a wave room, frames D41-locked / open / boss far 12 m and near 3.5 m); J2 c757c203 (THE ICEBREAKER's meter WAS drawn
  but read as the bar's outline: now 10 px under the bar, the stuck fist counts down cyan, a 16 px label "STAGGER" / "OPEN" for all five);
  E6 fa5fdbed (heavy41: knock x2, at least 0.35 s, a 0.14 s hitstop on tank / WARDEN / CHARGER / the brawler; `n41foes heavy`: CHARGER
  hold 0.19 -> 0.34 s + 0.14 s hitstop, tank / WARDEN 0 -> 0.14 s hitstop, husk unchanged; event `heavy41`; the DRONE room for R5: v6
  THE CHOIR wave 2, two drones); P3 34df25cc / 85f90a35 / 280e7e85 (the knee 0.00 -> 1.94 hits/s with a leading pilot; `n41pad real`
  on B8's bodies: THE LOOM 0.621 -> 3.050, THE ICEBREAKER 0.428 -> 0.500; C1 `n41pad glyphs`: 9 tokens in every n40 / n41 line and level
  file, each with a button on pad layouts v1-v5: GREEN, no text change); B8 18499c45 / 3a52ec58 (the stay pilot 10/10 52.5 s with the
  sweep on (29 sweeps: hit 5, rope 2, height 16); the casual pilot 8/10 (its 1 s ledge wait runs into the tell); a tell refill of one
  grapple charge when the sweep covers the tower tops; sweep damage 25 -> 15; the decal view Loom37View.N41.cs); L6 2529c1c4 (the
  greybox: levelgen 0 errors, 6.9 kind boss "rival41" clears on entry; the walk 6.1 2/2, 6.2 2/2, **6.3 0/2** (the 3.6 m deck: no nav
  route), 6.4 0/2).
- 11:28 **pump41On ON** (e33aabbf; frames looked at on n41-r1 w41).
- 11:26-11:31 picked: T2 -> c0bb8799, J2 -> dd651d6f, E6 -> 3cccbfce, K2 -> 9718048f, P3 -> dbdc57ad / f22a4a63 / b40b2328, B8 ->
  dad60066 / 685c18a7, L6 -> 607fe2e5. Gates + build n41-r3.
- 11:35 gates GREEN; **build n41-r3** (11:36, Succeeded, 0 errors; 3 / 7). Rows on in its copy: tokens41, bars41, entrance41, juice41On,
  doors41On, look41, spire41, sweep41, heavy41 (+ pump41On, ON in the repo).
- Reels on n41-r3 (8; rounds 2-3: 12 / 16), 0 NRE / 0 exceptions in every one:
  - entrance41 x5 (one Player per boss): **GREEN** on THE WICK (n38-foundry a9), THE ICEBREAKER (n40-iceline x9), THE KILN WYRM (n40-kiln
    k5), THE LOOM (n40-span o3), THE CROWN (n40-spire v11): each begun by the room's own entry, bossRise x1, 8/8 frames. Probes: Age 0
    the card + shake 2.45 (decaying 2.3 / 2.0) + 2 flicker pulses + 1 entrance hitstop; the line at Age 0.45; the fade 1.25-1.5 (Age
    runs behind real time by the entrance freeze: Age 0.383 at real 0.611 s).
  - d41 doors41 n38-foundry: **RED** (the reel never entered the wave room a2: "room a1, husks 0"; T2's fix e92b63b3 for n41-r4). The
    locked and boss-door frames are still good evidence: a red-orange holographic sheet with scanlines in the doorway, readable at 12 m
    and filling the doorway at 3.5 m; the boss door red in a pale violet-white frame. The open (cyan) state not framed.
  - l41 loom37 n40-span (sweep41 on): GREEN, THE LOOM killed at 35.3 s, the sweep's tell 1.20 s; **but no red-orange decal reads** in
    the three sweep frames (a tan ledge with a faint grey ring; the body; a white flash): B8 to fix.
  - b3 brawl40 n40-iceline (J2's meter, heavy41 on): see below.
- **Frames looked at:** THE WICK's eight (the card over the CRT flicker at 000 / 150, the rule line "RIDE A WALL, HOLD LEFT CLICK: SHOOT
  THROUGH A CRACK" under the name from 900, still up at 1600 real); the four others at 900: **the rule line shows twice** on THE
  ICEBREAKER, THE LOOM and THE CROWN (under the card and the BossLine40 line under the reticle); THE KILN WYRM's card says "WYRM" while
  its bar says "KILN WYRM", and it has no rule line (Line ''). The bar (ticks, the "STAGGER" label + meter) reads on THE LOOM, THE CROWN
  and the WYRM. J2's iteration: one line during the beat, one name for the WYRM, its rule line.
- 11:42 K2 a58c9049 + 11:55 0a285730 (the n41-r2 after numbers refuted round 1: every floor within 0.2 / 255 of the before, L2 B/R 0.795
  -> 0.797: the key spots never reach the floor seen from the spawn; the directional dusk sun 0xffbd8a (1.00 / 0.74 / 0.54) lights it.
  Now look41 tints the sun by theme (iceline bcc4d8, kiln ffc070, span c8b4e6, spire d8c298, foundry unchanged), written in Update
  (Init destroyed the first tinted sun), boss rooms keep the config sun (tells stay red-orange over a warm floor), restored on unload).
- 11:55-12:00 picked: K2 -> cb703b34, T2 -> d411b71a (pad41) / db69caa7 (A11: row 13's press from the first grounded frame, held 2 steps,
  one retry) / e2a3d928 (A9: wick37 / loom37 / crown37 call DoorFree41) / e92b63b3 (doors41's room entry), E6 -> 28b85e3e (models41);
  the lead's brawl40 reel line DoorFree41 (A9's last one). Gates + build n41-r4.

## Round 3 (2:30-3:30)
- 12:03 gates GREEN; **build n41-r4** (12:04, Succeeded, 0 errors; 4 / 7). 12:07 **bars41 ON** (14589324: frames looked at on n41-r1 /
  n41-r3; iterated once by c757c203).
- Reels on n41-r4 (7; total 26 / 50), 0 NRE each:
  - d41 doors41: RED again but closer: the entry works ("room entered a2 (want a2)", husks 3 alive, d2 LOCKED); a2 has more waves, so
    d2 stayed locked for 25 s; D41-locked-far blocked by a frozen husk in front of the camera. T2's fix 933ca68c (kill every wave up to
    60 s; move enemies off the camera's line).
  - pad41 x3 (-skygrindReelPad): **GREEN** L2 n40-iceline (10 lines: every gauntlet / KESTREL / LB / LT / RS line as a pad glyph), L3
    n40-kiln (8: "BOOST 1/5 · PRESS A TO JUMP" ... "BOOST 2/5 · IN THE AIR, LOOK STRAIGHT DOWN"), L5 n40-spire (4: "THE TORCH: HOLD <RT>
    CLOSE", THE CROWN's "<LB> DASH + <A> JUMP"); device pad on every frame, 0 keyboard names, 0 unresolved tokens (C1, A14's device).
  - v2 verbs n40-iceline 20/20; v3 n40-kiln 24/25: **row 13 still RED with T2's A11 press fix** ("slid False ... 7.07 -> 10.70 m/s", the
    same numbers as NIGHT40: the slide never starts on n40-kiln; L2's row 13 PASS "slid True"); T2 is on it.
  - lu lineup40 n40-iceline (models41 on): GREEN, placed 7/7; at 12 m the husk's new arm cannot be judged (a faint capsule).
- K2 f5462205: look41's after on n41-r4 (the sun by theme): L2 floor (150.1, 129.5, 119.3) -> (117.0, 132.7, 162.6), B/R 0.795 ->
  **1.390**, luminance 133.1 -> 131.5; L3 (140.2, 98.0, 81.5) -> (140.3, 98.8, 65.6), B/R 0.581 -> 0.468 (hue 17 -> 27 deg, away from
  the danger red-orange at 12). The lead's own look (f0005 before / after): L2's room reads pale blue ice with a white frost crust at the
  wall bases (was tan); L3 reads warm amber (was pink-tan). **look41 ON** (1e900bc1, 12:34); L1 / L4 / L5 checked in round 4.
- Reports 12:05-12:30: J2 6df7f9a3 / ee305b1d / 8ba87a48 (the rule line once: the card's copy yields to a BossLine40 line; the card
  takes the bar's name, "KILN WYRM"; THE WICK's ticks at its HP floors 60 % / 30 %; THE KILN WYRM's line "SHOOT ITS HEAD. THE OPEN CORE
  HITS x3"; leg pips for THE LOOM and THE CROWN); E6 caf575c6 models41 (the husk's dragging red-orange-claw arm; every kind's parts break
  into blocks), 0fb32a20 (`n41foes walk`: n40-kiln 6/10 with the cap on AND off; n38-foundry a3-a6 3/10 on vs 4/10 off, a5 THE GANTRY
  80 vs 32 deaths: **SHORT, measured**; the all-off control below NIGHT40's 10/10 is being traced), 9af64517 kiln41 (kb2 3.2 POUR HALL's
  wave 3 = charger + 2 swarmers; `n41foes kiln` 6/10 = the control); P3 6c307f83 / c6c3a873 (a live weak point's own cone atan(1.5 m /
  d) and acquire x0.5; `n41pad real` hits per live second off -> on: THE ICEBREAKER 0.428 -> 1.891 (weak pilot) / 0.892 -> 2.605 (mid) /
  2.284 -> 2.426 (expert); THE LOOM 0.629 -> 3.168 / 1.517 -> 3.160 / 2.130 -> 2.484); L6 e8dc3f6c (the room ids m1-m7 / mr / mx:
  Walk29Pilot ran old n29 / n30 scripts keyed on f2-f9 ids; 6.3's step blocks for the route graph) + 31263a41 RIVAL.md + 7e64cdaa (6.7's
  blocks: the walk **10/10**); T2 933ca68c; B8 9ffadf14 (sweep fairness: a 1.5 s gap after another tell, a 1.5 s tail, the tell 1.2 ->
  2.0 s; casual 10/10 58.6 s, stay 10/10 51.6 s; 20 seeds casual 19/20, the miss with no sweep hit) + 2aff3eba (the frames' cause: leg 1's
  band held no ledge and the reel kept the pilot's look: a faint red-orange sheet over the band at its height + edge bars; the reel aims
  at the band) and the cause of THE ICEBREAKER's tan (Stone.shader reads vertex colours only; Brawl40View set _BaseColor: every part
  rendered white); R5 24faa9a0 foundry41 (W1's last 4 m a no-ride band standing 0.6 m out + no-ride pit liners; the probe without W2:
  RED 0/2 "does NOT cross"; shipped GREEN 2/2 with --ride40, b2-g1 "ride chain L" +32 % (was "climb L" +156 %); walk b1-b2 10/10) +
  3b06966b (spire41: the pull keeps 29-33 m/s vertical so every steep pull flew to the roof; rings at 4 m / 5.4 m, eye 4.85-9.91 m under
  the roof on 12/12 pulls; v6 wave 2 = heavy + spitter + 2 drones (12 points); walk 10/10 126.2 s; gates 4/4).
- 12:26 picked: J2 e5206277, E6 457e2809 / d8edcf45, P3 dd58a691 / 555b6540, L6 7302d6b4 / b45675ff, T2 2fef9607, B8 ee8fa2ec /
  31ab12c1, R5 8d2b4029 / f3552c86. **Disk:** C: fell 93 -> 30 GB by 12:24 (reel frames ~0.7 GB a reel); the lead's own f*.jpg past
  f0019 deleted in n41-r1..r4 (named shots, telemetry and the first 20 frames kept): 30 -> 45 GB.
- 12:31 gates GREEN; **build n41-r5** (12:32, Succeeded, 0 errors; 5 / 7). Rows on in its copy: tokens41, heavy41, entrance41,
  juice41On, doors41On, look41, spire41, foundry41, models41, sweep41, aim41 (+ pump41On / bars41 ON in the repo).
- Reels on n41-r5 (8; total 34 / 50), 0 NRE / 0 exceptions each:
  - d41 doors41: RED a third time: the room reached phase Cleared after 15 kill rounds (60.5 s) yet zone d2 stayed LOCKED ("open /
    locked / boss shown 0/9/1"); 3 bodies moved off the camera line for the locked frames. The open (cyan) curtain is on no doors41
    frame; the only cyan curtain framed is n40-iceline 2.1's far door in n41-r4 v2 f0005 (red in n41-r1's same frame): J2 asked to read
    the code path.
  - e41worm / e41crown entrance41 **GREEN**: the card reads "KILN WYRM" (= its bar) with "SHOOT ITS HEAD. THE OPEN CORE HITS X3" under
    it by 0.9 s; THE CROWN shows its rule once (Yielded 1, LineYielded True) and three leg pips at its bar's right end.
  - l41 loom37 n40-span (B8's fairness + view fix): GREEN, killed 35.9 s, but **no sweep fired** (tells spool / lash only; no L41
    frames): B8 asked for a reel-guaranteed sweep per leg.
  - v5s verbs37 n41-spire (R5's lowered rings + the DRONE room): **30/30** (17 within 1.35 m, 19 within 1.64 m); the pull's top at y 9.2
    with the room in view (f1425 / f1432; was y 13.8 under the roof). -> **spire41 ON** (86f96744).
  - v1f verbs37 n41-foundry (foundry41): **26/26** (9: a 1.81 s ride on a 24 m wall; 11: 16.52 m/s off the wall); the verbs reel runs
    in a2, so THE GANTRY's band (b2) is not on these frames: level36 on n41-foundry in round 4.
  - b5pad brawl40 -skygrindReelPad (aim41 on): GREEN 52.6 s; the stuck frame shows the fist and "ITS FIST IS STUCK: HIT IT NOW" but no
    visible lock on the fist (P3's weak-point brackets come in n41-r6).
  - fin level36 n41-finale (L6's greybox, finale41 off, booted by -skygrindMap): 9/9 rooms entered, 24/27 frames; RED on level36's
    dressing checks (a greybox: bubbles 6/7, own title 8/9); frames for L6.
- 12:44-12:58 reports: P3 58b8516f / 50ad8f59 (the brackets frame the live weak point; their lower corners snap clear of arc40's band;
  C1 read on the pad41 frames: L2 / L5 draw real glyph icons, L3's boost chain says "PRESS A TO JUMP" in words by design (its slot has
  no rich text), per device, no keyboard names); E6 f460644b (the kiln walk NIGHT40 used, `n40start kiln --start40` = `n41foes s1kiln`:
  **10/10** all off (234.9 s), 10/10 tokens41 on (243.6 s), 10/10 kiln41 + tokens41 on (226.5 s; kb2 deaths 8 -> 12); kiln41 gates 4/4 on
  the shared plan (the files differ only in kb2's waves): the round-3 6/10 was the wrong walk command); L6 7e64cdaa (walk 10/10).
- 12:56 picked: E6 cab6cc94, P3 a668bbe4 / 0b08b613.
- 12:58 **doors41On ON** (0ec992ce): J2 read the code: doors38 colours from the sim's zones every frame (a locked zone is always
  red-orange); the cyan far door on n41-r4 v2 f0005 was really open (the verbs reel runs DOORFREE), and n41-r1's red sheet on that same
  door was Arena36View's view-only slab (lit by arena36Lock, blind to the zones), which doors38 hides. The damage static ("like a TV"):
  0.5 + 0.3 x min(1, damage / 50), falling 1 per real second (a haymaker 0.68, gone in ~0.7 s); no reel caught a hit inside that window.
- 12:53-12:56 picked: B8 94396101 (the loom37 reel queues one sweep in legs 1 and 2 through Boss37Brain.Force, reel only; harness
  `n41loom sweep --force`: stay pilot 10/10, 47.8 s, 24 sweeps, 0 hits) + fc0e4144 (brawlLook41, OFF: vertex-coloured ice plates 5f95ff,
  a near-black core, red-orange fist shells through the tells, cyan while stuck, crouch poses, a shatter into crunch blocks); T2 f2d354eb
  (row 13's root cause: under DOORFREE the reel picked kb2's 26 m wall and its 45-deg slide line runs over THE EAST POUR's lava, so the
  player is airborne from the first step: "7.00 -> 10.66" reproduced by `n41proof slidewall --doorfree`; VerbsWall41.PickSlide keeps a
  start whose whole line has floor; `--doorfree --fixed` PASS 8.20 -> 11.56 on all six levels); K2 b7db0bac; L6 ebb394f7.

## Round 4 in the Player (4:20-5:00)
- 12:57 gates GREEN, but **build n41-r6 FAILED twice** ("script class layout is incompatible between the editor and the player":
  Brawl40View's new fields Look41On / Shell41Lit / Shatter41Blocks in the player only). Cause: B8's new file Brawl40View.N41.cs was
  cherry-picked at 12:56:24, four seconds after the gates' Editor compile wrote Skygrind.Game.dll (12:56:20); `recompile` then answered
  up_to_date and an AssetDatabase.Refresh did not recompile. Fixed by eval: ImportAsset(ForceUpdate) + RequestScriptCompilation
  (Skygrind.Game.dll 13:05:49; the field present). **Build n41-r6** 13:07, Succeeded, 0 errors (6 / 7 Players; two failed attempts made
  nothing). Lesson: never pick while the gates' compile runs; after a pick that adds a file, force its import before a build.
- Rows on in n41-r6's copy: tokens41, heavy41, entrance41, juice41On, foundry41, kiln41, models41, sweep41, aim41, brawlLook41, select41
  (+ pump41On, bars41, look41, spire41, doors41On ON in the repo).
- **Round-4 reels on n41-r6** (13; total 47 / 50), 0 NRE / 0 exceptions in every one:
  - **the verbs, the after:** L1 n41-foundry **25/26** (row 13 RED: T2's new slide pick moved it onto THE GANTRY b2's wall: "slid True,
    WallJump 0, ground Jump 1, 11.60 -> 9.43"); L2 n40-iceline 20/20; L3 n41-kiln **24/25** (row 13 RED the same way, 26 m wall); L4
    n40-span 30/30; L5 n41-spire 30/30. T2's d7ad30d2 keeps Pick's wall whenever its line has floor (no-ride faces skipped): `n41proof
    slidewall --doorfree --fixed` PASS 8.20 -> 11.56 on all six levels; retakes on n41-r7. R5 / T2 read the Mover: a no-ride face
    refuses only rides (entry :1476, attach :1856, a corner :1987); wall jumps and slide bounces never test it.
  - l41 loom37 n40-span: **GREEN** 38.0 s, sweep41: sweeps 2 (reel-queued 2), hit 0, frames legs [1, 2], line True. **Frames looked at:**
    the canyon floor under a dark red-orange sheet with bright orange edge bars (leg 1, the line), the view washed red-orange under the
    leg-2 sheet at 11 m while hooked.
  - b2c brawl40casual n40-iceline: **GREEN** 53.9 s (haymakers 9, hit 5; slams 3, hit 3; 13 gauntlet hits). hurt38Static at frames 244 /
    511 / 1715. **Frames looked at:** f0243 the haymaker's red-orange shell on the player, f0245 / f0251 the TV static (grain, an RGB
    split, the health text red), clean by f0262; brawlLook41: a slate-blue body (was wall-tan), red-orange shells over both fists on the
    slam, a cyan shell on the stuck fist, the kill's blocks flying.
  - b5pad brawl40 -skygrindReelPad (aim41 on): GREEN 52.6 s; the stuck frame shows no bracket box on the fist: no Player proof of a
    pad lock on a weak point (the reel's pilot does not drive the pad's assist). **aim41 stays OFF.**
  - lv1 level36 n41-foundry: **PASS** 11/11 rooms, 33/33 frames (b2-a / -b: THE GANTRY from the lip, "HOLD SPACE: CHAIN THE WALLS."; the
    band is not drawn differently from the wall). lv3 level36 n41-kiln: RED on the dressing checks (7/8 rooms entered, arena rooms 2/3
    locked; not compared with n40-kiln tonight); kb2-b frames the CHARGER in 3.2 POUR HALL.
  - wk wick37 n41-foundry: space GREEN, **fight RED** (the pilot never rode a panel: "panels ridden [], leg 1, alive"); THE WICK's fight
    has not been killed in a Player since NIGHT37 (HANDOFF NIGHT37 "Open"), so not a NIGHT41 change; noted.
  - cr crown37 n41-spire (spire41 on): **GREEN** (gems 0, finisher, kill frame; legs 2 / 3 framed; falls 3).
  - w41p weapons41 -skygrindReelPad: GREEN (taps 3/3, spin 0.20 / 0.21 at +0.15 s with the pad flag, as on the keyboard: A14).
- 13:23 picked T2 4c9fb5a3; 13:27 gates GREEN. **13:28 the freeze config** (df379618): juice41On, entrance41, sweep41, brawlLook41,
  foundry41, kiln41, tokens41, heavy41 ON with their evidence (13 / 18 rows ON). **Build n41-r7** 13:28 (Succeeded, 0 errors; 7 / 7):
  the freeze candidate. Its copy adds select41 for the LevelSelect29 check.
- 13:09-13:22 **the suite, Core namespace** in the live Editor (n41-suite.sh, a batch per class letter): **1095 / 1095 passed**, 0
  failed (13 minutes; Logs/n41-suite-freeze.txt).
- On **n41-r7** (the freeze build; 2 retakes, total 49 / 50; 0 NRE): v1 verbs37 n41-foundry **26/26** (row 13 PASS on Pick's wall, "the
  old start (its slide line floored, no noride face)", 8.20 -> 11.56); v3 verbs37 n41-kiln **24/25**: row 13 still RED ("VerbsWall41 room
  kb2 24 m wall, start 18 m along ... (Pick's: floored False": the only floored start under DOORFREE; the Player's press lands a ground
  jump at 11.60 -> 9.43 m/s while the harness passes there at 8.20 -> 11.56). **A11 SHORT**: a reel limit on n41-kiln (the slide bounce
  itself passes on L1 / L2 / L4 / L5 and in the harness on all six levels; the Mover has no no-ride test on wall jumps).
- **LevelSelect29** on n41-r7 (select41 on in its copy): every press PASS (slot 3 -> n41-kiln, 5 -> n41-spire, n41-foundry, n40-iceline,
  n40-span), 0 exceptions, 0 NRE; no frame of the preview was taken, so **select41 stays OFF** (built, unseen).
- **The smoke** on n41-r7: PASS through round 11 (the power switch, SHOCK PLATE, THE PROTOCOL in 31 s, the four relays, the HOLD ring
  3600 / 3600, the exfil radio called), 0 exceptions, 0 NRE.
- 13:43 the suite's Game namespace started (n41-suite.sh freeze Game).

## Freeze (5:00) and the test hour
- The freeze: df379618 (13 / 18 rows ON) + the close docs; Player build n41-r7. 13:47 gates on df379618: golden 0 mismatches (22 / 22
  exact), fingerprint EQUAL plain and --off41.
- The suite (A6): Core 1095 / 1095 (13:09-13:22); Game **SHORT**: A-L 527 / 528 passed (13:42-13:52; the 1 failure is NIGHT40's: Lesson25TextTests.EveryLevelPrompt_... '{grapple} HOOK THE KNEE. {grapple} PULL AWAY.' in n40-span.json is 7 words, the rule is 6; added by NIGHT40 B4 f7878a50, never caught because NIGHT40's suite never finished; an n40 file is not edited in place: the fix is an n41 text row or n41-span); M (MapChoice 58 + MapPipeline + 8 classes) and N (21 classes) each ran past run_tests' 600 s timeout twice (20 min each); P-Z: the lead's stop at 14:43 did not take (pkill matched no process), so the script kept calling run_tests until 15:03: P timed out (600 s, twice), R-Z each returned no JSON twice: no test past L produced a result, and those calls overlapped the bench runs. Next night: split M and N into smaller prefixes (Skygrind.Game.Tests.MapC, .Mo, .N3, .N4 ...).
- The bench (C8): **3 runs, indicative** (n41-r7, `-skygrindBench 60 -skygrindMap n40-iceline`, 24 husks, 1920x1080, RTX 5070 Laptop, Balanced plan on AC; CPU 10-16 % before: the Editor's leftover test run, so not alone): models41 off median 3.24 / 3.34 ms, p95 5.87 / 6.72, p99 11.10 / 10.67 ms, mean 278 / 269 fps; models41 on median 3.35 ms, p95 6.25, p99 10.71 ms, 269 fps. models41 moves nothing past the off runs' own spread; GPU median 1.18 ms (the CPU main thread is the tail). Logs/player/n41-bench-n41-r7-*.log.

## Close table
| card | result | measured reason |
|---|---|---|
| G0 premise | DONE | `date` 09:06:54; GG's words first (none since the forge: every DECIDE41 default stands); the shared commit 3fa2c92a at 0:35 (N41Blocks, design.n41 18 rows off, --off41, tape 24, the four n41 ids in design.maps.list with placeholder files so no reel booted SURVIVAL, N41BaseId, the weak point / heavy / view-row / bar hooks), gates GREEN; eight builders at 0:37; dn-check-wt proven with a plant (1 error); the suite's batch filter proven (one substring); VERBS marked |
| T2 proof tooling | DONE (A11 SHORT) | per-frame telemetry (fired, charge, pump, spin, device) in every reel; weapons41 / doors41 / entrance41 / pad41 used for every ON row's frames; A9 (banners) fixed in all five DOORFREE boss reels; pump41On ON; A11: the reel's slide ran over n40-kiln's lava (found), fixed on L1 (26/26), still RED on n41-kiln (the only floored start lands a ground jump in the Player; harness PASS) |
| J2 boss moment / doors / bars / select | DONE (select41 SHORT) | juice41On, doors41On, entrance41, bars41 ON after frames + one iteration each (the meter label, the line once, the WYRM's name and line, leg pips); select41 built (boss names, slot 6 REBUILDING; no silhouette art), LevelSelect29 PASS with it on, no frame looked at: OFF |
| K2 levels' look | DONE | look41 ON: round 1 refuted by its own numbers (floors moved < 0.2 / 255), the sun by theme: L2 B/R 0.795 -> 1.390, L3 0.581 -> 0.468, brightness within 1.2 %; level 2's frost / sheen; L4 / L5 after not measured |
| B8 bosses | DONE | sweep41 ON (stay 10/10 51.6 s, casual 10/10 58.6 s; the sheet framed in legs 1 / 2 after the r3 iteration); brawlLook41 ON (the tan cause: Stone.shader reads vertex colours; framed on r6); weak points filled |
| E6 enemies | DONE (models41 SHORT) | tokens41 ON (time above two wind-ups 3.14 % -> 0 %, walks 10/10), heavy41 ON (CHARGER hold 0.19 -> 0.34 s + 0.14 s hitstop), kiln41 ON (NIGHT40's kiln walk 10/10, gates 4/4); models41 built, OFF (the husk's arm unreadable at the lineup's 12 m; no blind re-test) |
| P3 pad | SHORT | aim41 OFF: harness gains (THE LOOM 0.63 -> 3.17, THE ICEBREAKER 0.43 -> 1.89 hits / s, weak pilot) but no Player frame of a pad lock on a weak point; C1 DONE (22 pad lines, 0 keyboard names; the glyph check GREEN); C10 built from the layout, unframed |
| R5 levels | DONE | spire41 ON (L5 26/30 -> 30/30, the rings lowered after the r2 frames, the DRONE room); foundry41 ON (b2-g1 +156 % -> +32 % "ride chain", walk 10/10, verbs 26/26) |
| L6 level 6 plan | DONE | the beat sheet (THE MIRROR), the greybox n41-finale (levelgen 0 errors, walk 10/10), RIVAL.md; finale41 OFF (D-3) |
| L0 lead | DONE (the suite SHORT) | every integration gated (golden 0 / 22, fingerprint EQUAL plain and --off41, compile 0 errors); 7 / 7 builds; 49 / 50 reels, 0 NRE; the smoke and LevelSelect29 PASS on the freeze build; the suite: Core 1095 / 1095, Game A-L 527 / 528 (NIGHT40's 7-word prompt), M / N past the 600 s timeout, P-Z not run; the bench 3 runs, indicative |

## INVENTORY fates (28)
A1 ON (pump41On) · A2 DONE (judged on frames, no change) · A3 ON (look41) · A4 ON (sweep41) · A5 ON (tokens41) · A6 SHORT (Core 1095 / 1095, Game A-L 527 / 528 with NIGHT40's one failure; M / N over the 600 s timeout, P-Z not run) ·
A7 SHORT (models41 OFF) · A8 SHORT (models41 OFF) · A9 DONE (DoorFree41 in the reels) · A10 ON (spire41: 30/30) · A11 SHORT (reel limit
on n41-kiln) · A12 ON (foundry41) · A13 DONE (plan; finale41 OFF by D-3) · A14 DONE (the pad spin) · B1 ON (juice41On) · B2 ON (doors41On) ·
B3 RETIRED · B4 RETIRED · C1 DONE · C2 SHORT (aim41 OFF) · C3 ON (bars41) · C4 ON (entrance41) · C5 ON (kiln41 + spire41) · C6 ON
(brawlLook41) · C7 ON (heavy41) · C8 DONE, indicative (p99 ~10.7-11.1 ms, models41 no measurable cost; 3 runs beside the Editor) · C9 SHORT (select41 OFF) · C10 SHORT (built behind aim41, unframed).
