REVIEW: 13 findings (0 must-fix)

Fresh-context code review of NIGHT60's M-size C# commits (read-only: git show / grep / file reads; no Editor, build, Player or reel run).
Scope: B1 5e64a858; B2 3524e84e 284a4783; B3 076310a5 89803e51; B7 c658cdba 2771de00 e6e11996 5cddef27 4d6a6f62 4669d2be;
B8 563795e2 db599f4c 9fec7fb5 + attempt 2 7ce08c33 51e4b04f 338cfa30; B9 b43e1490 b016ca87 + attempt 2 ff57b3c5 c6a6e196 0d3589c8.
Not repeated: c6a6e196's NIGHT56 comment moved onto the focus line (the lead's find, fixed in 224d8338).
At review time the working tree turns rim56 ON (Design/unity-config.json, uncommitted: "ON at NIGHT60 by the rule: B7 critic PASS"),
so S1 and S2 apply to the row as it is about to ship.

## Should-fix (6)

### S1. B7 5cddef27 / 4669d2be: the lit gain went down, the shadow gain did not (the cel terminator nearly vanishes on near enemies)
- Where: Assets/Scripts/Game/Rim56.cs:121 (`lit = Rim60.LitGain(lit, Rim60.On(g.cfg))`), line 123 (the shadow side still `Rim56Shade* x sh`,
  sh = Rim57Gain 1.47); Assets/Shaders/Enemy.shader:158-159 (`shadeC = lerp(lit, _Skin52Shade.rgb, rampK)`; `c = base * lerp(shadeC, lit, litK)`).
- Defect: with rim56 on, the shadow side's multiplier is (0.70, 0.38, 0.30) x 1.47 = (1.029, 0.559, 0.441). NIGHT59's lit side was
  1.5 x 1.47 = 2.205, so shadow / lit was R 0.47, G 0.25, B 0.20. At Rim60LitGain 1.1 it is R 0.94, G 0.51, B 0.40. On a red-orange body
  (R the largest channel) the shadow side's value is about 94 % of the lit side's in linear terms, about 97 % in sRGB (computed from the
  numbers above, not measured on a frame). NIGHT56's own design note (Rim56.cs header) says the shadow side is "~0.75 of the lit side's value".
  The B7 Player checks measured the bodies over the world's p90, the world over the tells, greyscale and tint. None of them measures the
  terminator / form read that skin52 (NIGHT52, 3 rounds, the a252 shadow-share floor) was built for.
- Failure scenario: rim56 ships ON. A husk at 6-10 m (rampK near 1) shows almost no value step between its lit and shadow halves: the
  pill reads as a flat red-orange disc. Nothing in the gates or the critic's evidence would flag it.
- Smallest fix: before the ON commit, run skin52's terminator measure (the a252 shadow / body share) on n60-b7's frames, rim56 on vs off.
  If it fails, scale the shadow side by the same factor in the same hook:
  `sh *= Rim60.On(...) ? UnityTuning.Rim60LitGain / (UnityTuning.Rim56LitGain * UnityTuning.Rim57Gain) : 1f`. That restores NIGHT59's ratio,
  but the MAIN must then be measured again.

### S2. B7 5cddef27: the rim56 row's config is read twice a frame, with allocations
- Where: Assets/Scripts/Game/Rim56.cs:121 calls `Rim60.On(g.cfg)` every LateUpdate, and Assets/Scripts/Game/Rim60.N60V.cs:32 calls
  `Rim56.On(cfg)` again. LateUpdate already evaluated `On(g.cfg)` at the top of the same method.
- Defect: `Rim56.On` -> `NightRows.ViewOn` builds "n56.rim56.enabled" (an int ToString, a params array and the result string), then
  `GameConfig.Design` concatenates "design." + path twice and `Has` runs `Split('.')` (an array and 4 substrings): about 9 allocations per call.
  B7 doubles a cost that already ran once per frame. The row is not read once.
- Failure scenario: rim56 ON. That is roughly 0.5 KB of extra garbage a frame, about 60 KB/s at 120 fps (estimated from the code, not
  measured). This is incremental-GC pressure on the Steam Deck / Switch path, for a value that never changes during a run.
- Smallest fix (one line): `lit = Rim60.LitGain(lit, !Rim60.Off60() && !Rim58.Off58());`. `on` already implies Rim56.On. Optionally cache
  `Rim56.On` per config (`ReferenceEquals(cfg, last)`), which removes the older per-frame read too.

### S3. B9 attempt 2 c6a6e196: focus60 does not exclude the Player tours that start live runs
- Where: Assets/Scripts/Game/SurvivalGame.N60L.cs:37. The only exclusions are -skygrindOff60, -skygrindNoFocus60 and -skygrindBench (plus the
  attract and FeelReel.Running at the call, SurvivalGame.cs:297).
- Defect: these modes call StartRun and play a run in Flow.Playing in a Player: ShotTour (-skygrindShots, ShotTour.cs:85/127/203),
  FeelStrip (-skygrindStrip, FeelStrip.cs:441; FeelReel.Running is set only by FeelReel.Run), StyleTile (-skygrindTile), HitchTour24
  (-skygrindHitch), DoorFree29Check and LevelSelect29Check. Before c6a6e196 they could not be paused by a focus loss in a standalone Player,
  because the lock never drops there (B9's own finding). With focus60 ON they can.
- Failure scenario: focus60 ON, a capture tour's Player loses the foreground to a popup or a planted steal (L-099). Pause() runs, the
  following captures show the PAUSED screen, and hitch / door-free timings include a paused stretch. The row's enabled_why guards (the
  planted probe, the smoke, 0 NREs) would not catch it.
- Smallest fix: one static `PlayerChecks.AutomationRun` set in PlayerChecks.Attach for every mode except FocusProbe60, read in
  FocusPause60On in place of the single -skygrindBench test. Add one tour run with the row on to the row's guards.

### S4. B9 attempt 2 c6a6e196: a new pause path for unlocked Editor Play runs
- Where: Assets/Scripts/Game/SurvivalGame.N60L.cs:20-21 (`PausesOnFocusLoss`); SurvivalGame.cs:297.
- Defect: the lock rule needs a locked cursor (wasLocked). The focus rule needs only a focused-to-unfocused change. Eval-driven Editor Play
  checks (play-check-eval-recipe) often never lock the cursor. They could not be paused by focus before; with focus60 ON they can be,
  provided Application.isFocused changes in the Editor when the Game view or Editor loses focus (not checked here). For example: the CLI's
  editor_focus moment, then GG's window taking the foreground.
- Double pause (the coordinator's question): none. Pause() returns unless flow is Playing, and the focus line reads `flow` after the lock
  line's Pause() has already set it to Paused. That gives one pause per loss, so SmokeGuard55 sees one Playing -> Paused change.
- Failure scenario: focus60 ON, the lead runs a timed Editor Play check while GG uses the machine. The run pauses mid-window and the check
  measures a paused game. CLAUDE.md already warns about this setting.
- Smallest fix: also require the cursor locked this frame: `... && wasFocused && !focused && locked && ...` (pass `input.Locked`). In a Player
  the lock still reads Locked after the focus loss (B9's finding), so the Player case still pauses. An unlocked Editor run never pauses.
  A locked Editor run is already paused by the lock rule.

### S5. B8 attempt 2 7ce08c33: the sidecar's `cleared` is written but never read; the ON frames are not a real clear's lamp
- Where: Assets/Scripts/Game/FeelReel.N60.Lamp.cs:83 (writes `cleared`); Tools/night/n60-v-lamp.py:65-86 (`run()` scores every
  `*-clear.png` and prints `opened`, never `cleared`).
- Defect: under a157's DOORFREE boot, `opened` is always true (7ce08c33's own note). A room that hits Lamp60ClearWaitS (30 s) without reaching
  phase Cleared is still scored as a clear frame. The same note says Exit58's lamps all light at about frame 272 under DOORFREE. So the ON
  frames measure a lamp lit since boot, plus any lamps further down the level seen through open doors. They do not test "dark while locked,
  rising once on the clear".
- Failure scenario: lamp60's MAIN / GUARD is computed from an uncleared room's frame (enemies alive), or a later room's lamp lights the
  frame. The verdict then rests on frames the row's enabled_why does not describe.
- Smallest fix: in n60-v-lamp.py, skip and print "not a clear" when `side.get("cleared") is not True`. State in the B8 verdict that the
  rise-on-clear behaviour is not covered by this reel (the a358 Exit58.Line on a non-DOORFREE run covers it).

### S6. B8 db599f4c: NIGHT58's lamp code now reachable by a row allocates every frame
- Where: Assets/Scripts/Game/Exit58.N58V.cs:200 (`Line = $"exit lamps ON on {Level}: ..."` in LateUpdate); Show() re-sets the property
  block, the light's colour and its intensity every frame after the fade has finished.
- Defect: while UnityTuning.Exit58On was a compile-time false, this path was dead. lamp60 makes it live: one new string per frame for the
  whole of n57-kiln.
- Failure scenario: lamp60 ON, about 150-250 B of garbage a frame (estimated) for a debug line read only by a358.
- Smallest fix: rebuild Line only when `lit` or LastLitFrame changes. Skip Show() for a lamp whose k was already 1 last frame.

## Notes (7)

### N1. B2 284a4783: the marks now draw over transparent effects in front of them
- Where: Assets/Scripts/Game/Kill55Damage.cs:120 (queue 3006, ZWrite On, the 0.3 m pull).
- At 3006 the marks overwrite every ZWrite-Off transparent effect that stands nearer than their pulled depth: the hit's impact cube (the
  critic's play note), sparks, glob trails (EnemyViews.cs:816, queue 3000). At 2010 those effects blended over the marks. The reorder also
  applies with the x-ray setting off, when there is no x-ray pass to avoid.
- If it reads badly: keep 2010 and have the x-ray pass reject a stencil bit the marks write.

### N2. B7 4d6a6f62: a constant pin, and no test of the switches
- Where: Assets/Tests/EditMode/Game/N60RimTests.cs:40. `LitGain_KeepsTheTellsOverTheWorldsCap` asserts Rim60LitGain in [Skin52LitGain, 1.1],
  where 1.1 is the chosen value. It fails only if the constant is edited.
- No test covers `Rim60.On`'s switches (-skygrindOff60, -skygrindOff58, -skygrindNoRim56) or the two hook lines (Decor55.cs:131-132,
  Rim56.cs:121). The Player's input lines ("x0.45" / "gain 1.10" vs "no") were the proof, checked per file.
- Cheap add: assert `Rim60.On(cfg)` is false with NightRows.ForceOffFrom = 60 and = 58, as N60SharedTests does for ViewOn.

### N3. B3 89803e51: the enemy-rect set changed mid-night
- Where: EnemyViews.cs:718-724, 940-946.
- Every rect-based meter's denominator moved: B7's MAIN went from 99 to 140 bodies (the log says so), and a157's `drawn` now counts
  swarmers / spitters. Comparisons against NIGHT58 / NIGHT59 runs are not like for like.
- FeelProbe.Enemy -> Anchor() walks every enemy per report, which is O(n^2) per recorded frame (at most 32 x 32). Recording only, negligible.

### N4. B1 5e64a858: the spawn line is missing from a157's done line
- Where: Assets/Scripts/Game/FeelReel.N57.Level.cs:104. The spawn line is Debug.Log'd but not `lines.Add`ed, while B8's clear line is
  (FeelReel.N60.Lamp.cs:106). a157's done line omits the spawn frames.
- Fix: `lines.Add(lineS);`.

### N5. B8 338cfa30: a missing room B now moves the frame toward an arbitrary side
- Where: Assets/Scripts/Game/Exit58.N58V.cs:37-43, 134, together with Doors38View.cs:46. Ahead() returns a fixed (0,-1) / (1,0) when room B
  is missing, and FaceAnchor then moves the frame half a passage that way (before, it stayed centred).
- Latent: all nine n57-kiln doors (d1-d8, d6b) name two known rooms (checked), and Exit58 builds only on n57-kiln.
- If the lamp spreads to other levels: keep the centre when rb is null.

### N6. B9 attempt 2 c6a6e196: focusWas60 starts true
- Where: SurvivalGame.N60L.cs:24. The first ready frame of an unfocused Player counts as a focus loss.
- This matters only if a run is already Playing on that frame (a run resumed at boot).
- Fix: initialise focusWas60 from Application.isFocused on the first call.

### N7. B9 attempt 2 c6a6e196: DOWN is not paused (a design question for GG, not a code slip)
- Where: SurvivalGame.N60L.cs:20-21.
- The sim keeps running in Flow.Down (SurvivalGame.cs `simulating = Playing || Down`). An alt-tab during DOWN lets its clock run out while
  the player is away. The lock rule has the same shape.

## Checked and clean
- Player-only failures (L-035): no Unity API call was added in a field initializer or static constructor. The new statics are ints / consts.
  AutomationFlags references FocusProbe60's const strings. The only Shader.Find is NIGHT59's SKYGRIND/Kill59Mark, which lives in
  Assets/Shaders/Resources and so is in the build. No null path when a view is absent:
  - Exit58.Build guards map / campaign / doors.
  - L60ClearShot guards camp, zones and campaignLevel.
  - B1's `r.spawn` exists for every room (Campaign24.cs:320-322 reads it unconditionally).
  - MarkQueue returns -1 before the first mark.
- Off paths: these read the command line themselves, so they hold before PlayerChecks sets ForceOffFrom:
  - -skygrindOff60: Rim60.Off60, Exit58.Off and FocusPause60On.
  - -skygrindNoRim56: Rim56.On false, so neither the band dim nor the lit gain applies.
  - -skygrindNoExit58: the lamp alone.
  - -skygrindNoKill55: Kill55.On false, so no marks (NIGHT59's game for that row).
  - -skygrindOff60 leaves kill55 on. Rows switch off by the night in their name: systemic, not B2's.
- Reads once: lamp60 is read once per map (Build), focus60 once per SurvivalGame (cached), Decor55's band once per map. rim56 is the
  exception (S2).
- Per frame: B3 runs only while FeelProbe.Recording. B9 reads Application.isFocused once a frame (extern, no allocation). B2 builds its
  materials once (Ready()).
- Rules:
  - Every new Unity-only number sits in a UnityTuning partial with its comment: N60V (Kill60MarkQueueAfterXray, Rim60BandScale,
    Rim60LitGain), N60V8 (Lamp60ClearWaitS, Lamp60SettleS, Exit60FaceGap), N60L (five), FeelAB (ProbeMaxEnemies).
  - No gameplay number typed by hand: the reel's door-height default 4 mirrors Exit58's NIGHT58 default.
  - No Random, no audio.
  - Colours keep their roles: the lamp is cyan (usable, "go here"), the marks are near-black / ember on enemies, the band is the level's
    accent dimmed.
  - No Skygrind.Core file is touched by any reviewed commit. lamp60 and focus60 have no tape bit (N60SharedTests, N60FocusRowTests).
- Tests, each RED for the right reason on its test-first commit:
  - N60MarkTests: 2010 <= 3005.
  - N60ProbeTests: 8 < 24.
  - N60RimTests: band / 2.205 / 1.3. The last of these is a constant pin (N2).
  - N60SharedTests: no design.n60.
  - N60FocusProbeTests: 4 RED.
  - N60LampTests: the centre.
  - N60FocusPauseTests / N60FocusRowTests: the placeholder / no row.
- Leaks into a normal run: none.
  - SpawnShots60 and ClearShots60 are read only inside a157.
  - FocusProbe60 is added only with its flag (PlayerChecks.cs:75, after ForceOffFrom is set). Its god health and seed are written only in
    its own run.
  - n60-focus-probe.sh closes the Player by its Windows PID after checking the name (CLAUDE.md rule 10).
- B9 attempt 2, the coordinator's questions:
  - A pause the player cannot leave: none found. Pause() frees the cursor and the pause screen's RESUME / pad path calls Resume(), which
    re-locks it.
  - Draft and reveal: they pause first (SurvivalGame.cs:304-307, UiReveal20.cs:214), so flow is already Paused; a pending draft opens after
    Resume.
  - Title, Down and Results are not Playing, so there is no pause (N7). The attract is excluded, and the kiosk attract starts only from
    Title (SurvivalGame.Kiosk.cs:87), so it never ends a paused run.
  - Bench: excluded by its flag. Smoke: SmokeGuard55 resolves the pause. Golden, fingerprint, farm and replay are synchronous Core runs
    (PlayerChecks.cs:224-242) and unaffected. Tours: S3.
  - Editor: no double pause (S4).
- Config: both copies are byte-identical after 0d3589c8 (cmp). lamp60 and focus60 are enabled false.
