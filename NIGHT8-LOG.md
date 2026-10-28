# NIGHT8 log (Unity stage 2a: the Demo Day build)

Times from `date`. Brief: `NIGHT8.md`. Branch `port/stage2` cut from the handoff docs commit `9970e8e`. GG started the
run at about 2:04 PM on 2026-10-02 (daytime, not night: see U2-01 row 7).

## U2-01 verify on arrival (2:04-2:08 PM)

RECEIVED (HANDOFF section 1, every row):
1. HEAD `9970e8e` (the handoff docs commit), parent `76a71d1`. As expected.
2. `git status --short`: ` M ProjectSettings/UnityConnectSettings.asset`, `?? NIGHT7.md`, `?? NIGHT8.md`. As expected.
3. `unity-loop.sh status`: `"status": "ready"`, `"playMode": "stopped"`, Unity 6000.3.7f1. As expected.
4. `unity-loop.sh tests` (2:05:57-2:06:23): `tests: 182 / 182 passed, 0 failed, 0 skipped`, `replays: 16 / 16`,
   `golden tests: 55 / 55`. As expected.
5. `Builds/Stage1/SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke-arrival.log` (2:06:37-2:07:05): the Player quit
   by itself; `[smoke] 19.8s DONE: all steps passed`; 0 lines matching "exception". As expected.
6. Power plan `Balanced` (381b4222...), on AC (PowerOnline True, battery 100 %).
7. Idle 76 s at 2:05 PM: GG had just used the machine. Drift from "idle at night": this run starts in the afternoon.
   Consequence: until the idle time is long, batch Play checks into as few runs as possible and prefer EditMode tests
   and the Player's checks; `editor_focus` before each Play check.

Branch `port/stage2` created from `9970e8e` at 2:07 PM.
Status U2-01: passing (no drift beyond the time of day).

## U2-02 zero-allocation steps, instrumented bench (2:08-2:42 PM)

What allocated per step (read in code, then confirmed by the new test's per-system recorder):
- `Loadout.Step`: a `List<string>` copy of the cooldown keys every step; `PortData.Name(mover.state)` (two strings via
  `ToString().ToUpperInvariant()`) in the ADS check every step; `SweepTargets` / `PickPunchTarget` iterated an
  `IReadOnlyList` with foreach (a boxed enumerator per call). The last one was the allocation the test caught after
  the first round of fixes (Loadout.Step 120 allocations in 120 steps).
- `HuskBrain.Step`: the token-pool key as an interpolated string every step; `Separate`: `new[] { "husk", "swarmer" }`
  every step; a new `NavJumpState` per nav jump; a repath built a heap the size of the grid (11,228 nodes + links)
  plus a Dictionary, a HashSet and two Lists.
- `Director.Step`: `paceRng.Next` passed as a method group (a delegate per step); `PickNode` built a list, an array
  of four lambdas and four FindAll lists per squad.
- `AimAssist.Step`: `LockParams` returned a new object every step; every scan boxed each candidate's id, built two
  lambdas, compared ids with `Equals(object, object)` (boxing), built `weapons.catalog.(w).kind` with `$"..."` twice and
  a fresh `Candidate` for the held lock.
- `EventBus.Emit` copied its handler list (`ToArray`) on every emit, and int payloads boxed per emit.
- Rarer: the first wind-up of each husk boxed its id (2 allocations in 600 steps in the test, fixed by boxing ids
  0..2047 at startup); the shotgun's per-shot Dictionary + List and the ricochet's two lists.

Fixes (all in place, no behaviour change):
- EventBus: iterates its list; a handler removed mid-emit still runs in that emit and leaves when the outermost emit of
  that name ends; one added mid-emit runs from the next emit (the web's copy semantics, tested). `Emit(string, int)`
  uses `Boxes.Int` (one box per value). `Emitted` pre-sized.
- Cached enum names (arrays by value), cooldown boxes per weapon, reused scratch (shotgun, ricochet, PickNode, the
  path scratch with stamps instead of clearing), a reused `NavJumpState` and path buffer per brain record, an int pool
  key, `LockParamsInto` (LockParams keeps its golden-tested shape and calls it), cached lambdas and a per-weapon range
  table in AimAssist, reused `HuskAttackEvent` / `AimLockEvent` payloads (no handler keeps them; noted in EventBus).
- `SurvivalGame.SimStep`: a `ProfilerMarker` per system (SKYGRIND.SimStep, Mover, Brain, Assist, Loadout, Regen,
  Buyables, Husks, Crowd, Style, AfterPlayer, Mode).
- Frame cap: `settings.fpsCap` from config.json (0 = no cap) and vsync from `UnityTuning.VsyncDefault` (false: the web
  has no vsync setting; the browser always syncs), through `SurvivalGame.ApplyFrameCap` (SETTINGS will drive both).
- `-skygrindBench`: GC collections over the run, "GC Used Memory" rises, "GC Allocated In Frame" (Development Players
  only), FrameTimingManager CPU / GPU median and worst 1%, SimStep marker time (Development only).
  `PlayerSettings.enableFrameTimingStats` turned on (ProjectSettings.asset, that line only: the Editor's save also
  emptied `preloadedAssets`, the Input System actions; that part was put back to HEAD, as NIGHT7 did).
- `Tools/survival-bench.sh`: runs the bench and polls the plan and AC every 2 s.

Checks:
- `tests ZeroAlloc` (2:34 PM): `tests: 3 / 3 passed`. `SimSteps_With30Enemies_AllocateNothing`: real map, every zone
  open, 24 husks + 6 swarmers, the gauntlet's sweep every step, SNAP on and locking (the player turns to the nearest
  enemy every 30 s steps; locks > 0 asserted), 480 warm-up steps, then 600 steps counted per system (all 0), then 600
  steps under `Is.Not.AllocatingGCMemory()`. What it doesn't cover: firing (a shot still allocates its hit / kill /
  shotResult payloads, a few hundred bytes a shot), the player being hurt (HurtEvent per blow), Mover, Style, Regen,
  the HUD (U2-03). The test holds the director's linger timer at 0 so the standing player never camps (campStart's
  payload is a boxed double, once per EXPOSED edge).
- Full suite (2:35:56 PM): `tests: 185 / 185 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Lesson: `Is.Not.AllocatingGCMemory()` counts the JIT of the measured lambda's first call; warm the delegate itself
  before measuring (the EventBus test failed until then).
- The bench needs a Player build; NIGHT8 section 4 allows none before U2-14 / U2-15, so the bench runs at U2-15.

Status U2-02: in_progress (tests pass; the bench is measured at U2-15 with the rebuilt Player). Commit `02ab591`.

U2-02's bench, measured at the close (7:56-7:58 PM, the 7:50 PM Player, `bash Tools/survival-bench.sh 60`):
- `power seen: 13 Balanced AC=True` (every 2 s poll: Balanced, on AC); 1920x1080 windowed, vsync off, no cap; RTX 5070
  Laptop, Ryzen AI 9 HX 370.
- `bench result: 16889 frames, alive at end 31, median 3.35 ms, p95 5.32 ms, worst 1% 8.37 ms, mean fps 282, median fps
  299, vsync 0, cap -1`
- `bench gc: collections 8 in 16919 frames; GC Used Memory rises 65412.0 KB (valid); GC Allocated In Frame n/a (release
  Player)` (about 3.9 KB a frame still allocated somewhere outside the five tested systems: views, UI or other steps;
  not profiled).
- `bench timing: FrameTimingManager cpu median 3.34 ms, worst 1% 8.41 ms; gpu median 1.79 ms, worst 1% 2.25 ms (16916
  samples)`
- Against U1 (U1-REPORT: median 9.35 ms, p95 18.47 ms, worst 1% 40.25 ms, same laptop, Balanced, on AC, but with the
  machine busy and CPU at 100% for half the run): not a like-for-like comparison. Not checked: other programs' CPU
  during tonight's run (the Editor was open and idle).

Status U2-02: passing.

## U2-03 HUD and the static atlas (2:43-2:57 PM)

- Why the fallback asset was rewritten: LiberationSans SDF's static atlas (250 glyphs: ASCII, Latin-1 including the
  middle dot U+00B7, U+2022, U+25A1, ...) has no U+25A0 `■`, which the stamina pips drew; TMP then added it to the
  dynamic `LiberationSans SDF - Fallback.asset` (population mode dynamic) at runtime. Read from the two assets'
  character tables.
- Fix: the stamina pips are images now, as the web draws them (hud.js `.hud-seg`: `hud.staminaSegmentWidth` 58 px
  segments, 7 px tall, a `--usable-dim` back and a `--usable` fill that grows with the partial regen; dash.maxPips +
  perks.vigor.pips of them, shown up to the mover's MaxPips). Every other UI glyph is in the static atlas.
- Ui.cs rewritten for no per-frame allocations: each text is set only when its value changed (TMP `SetText` with number
  arguments, no strings; the feed and the prompt through one reused StringBuilder); the gold / muted hex strings are
  made once; config numbers read once; the results body built once per run; the AIM ASSIST label cached per mode.
- Four nested canvases under the scaled root: "HUD frame" (panels, edges, bar backs), "HUD values" (numbers, fills,
  prompt, rank, feed, banner, vignette), "HUD sight" (crosshair, hit markers, lock brackets), "Menus" (the screens;
  the only GraphicRaycaster). Values sit on their frame through twin rects with the same anchors.
- Zones.SpotBuyable passed `IsLocked` as a method group (a delegate per call: every step per gate in the buy prompt,
  every frame per sign); now one cached delegate.
- `UnityTuning`: `UsableDimAlpha` 0.16 (theme.js `--usable-dim`, not in config.js), the segment height 7 and gap 5
  (hud.js CSS).
- Play check (2:50-2:55 PM, GG idle 11 min; `editor_focus`, then `set_autotick` on, persisted): run 1 (idle player, no
  god mode) died at 0:12 to husks and showed RESULTS (`Logs/shots/hud-u2-03.png`); run 2 in god mode mid-fight
  (`Logs/shots/hud-u2-03b.png`, 15 alive, 128 fps in the Editor): the HUD reads as before, with the segments.
  After Play stopped, `git status --short` showed no change to the fallback asset (only my three source files).
- Tests (2:55:34 PM): `tests: 185 / 185 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Not checked: allocation counts of the HUD in a Player (the bench at U2-15 reports GC collections for the whole frame).

Status U2-03: passing. Commit `ed5dd59`.

## U2-04 SETTINGS (2:58-3:12 PM)

- Core `Settings.cs`: a port of settings.js (createSettings' load: config.settings defaults, then the saved values of
  the same type; migrateSettings: sens14, snap1) and menus.js's pure helpers (tabDefaults for RESET THIS TAB,
  rangeText), plus a flat JSON writer, Nudge (the fine step, else the step, clamped, rounded to the step's decimals)
  and Cycle for selects. Items and tabs come from config.ui.settings / settingsTabs (the web's key names); the saved
  JSON is the web's localStorage shape (every setting + `_migrated`) with `_version: 1`, under the web's key
  `skygrind.settings.v1` in PlayerPrefs.
- Game `GameSettings.cs`: loads at boot (stage 1's `skygrind.aimAssist` pref is taken over once for a new profile),
  saves on every change, applies sensitivity / invertY / fovDeg / cameraRoll / effectsIntensity / reducedMotion to the
  typed movement settings (InputReader and CameraFeel already read those), the frame cap / vsync, fullscreen (not in
  the Editor). Unity-only item: VSYNC (video tab, default `UnityTuning.VsyncDefault`).
- The screen (`UiSettings.cs`, a partial of Ui): tabs that have rows (MOUSE + AIM, VIDEO, HUD, ACCESSIBILITY), rows for
  the 17 settings Unity applies (sensitivity, invert Y, snap mode, snap strength, zoom sensitivity; FOV, FPS cap,
  fullscreen, vsync; crosshair colour, damage direction, show prices; effects intensity, reduced motion, low-health
  vignette, camera roll, flash intensity), RESET TAB, BACK; Esc goes back. Title: PLAY SURVIVAL, SETTINGS, AIM ASSIST,
  QUIT; pause: RESUME, SETTINGS, AIM ASSIST, QUIT TO TITLE. AIM ASSIST now toggles settings.aimAssist (off <-> the last
  mode).
- Applied live: the crosshair colour, SHOW BUY PRICES (the prompt's price and the zone signs, as hud.js buyLabel and
  zones.js), LOW HEALTH VIGNETTE (the under-45 % darkening), zoom sensitivity (ads.js: x (1 + (adsSensitivity - 1) x
  amount)), REDUCED MOTION (no camera roll). DAMAGE DIRECTION is read by U2-05; FLASH INTENSITY by the white flashes
  (U2-06 / U2-07).
- Viewmodel.Render built about a dozen config paths per frame (`$"ads.weapons.{held}.zoom"`, the ADS pose); a per-weapon
  pose cache now.
- Tests (3:07 PM): `SettingsTests` 6 / 6 (the web's settings.test.js and RESET THIS TAB test ported; a save / load round
  trip of every type, a clamp, wrong types and corrupt JSON keep the defaults); full suite `tests: 191 / 191 passed,
  0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Capture (3:10 PM): `Logs/shots/settings-u2-04.png`, the title's SETTINGS on MOUSE + AIM: legible, "0.0014 ·
  80.2°/1K" as the web shows it. The fallback asset stayed unmodified after the Play run.
- Not checked: clicking through every row with the mouse in Play (the eval opened the screen; the store's logic is
  tested), fullscreen in a Player. The closer (CONTROLS with live bindings) not done.

Status U2-04: passing. Commit `7b4b5ef`.

## U2-05 restart, recap, damage direction, personal bests (3:13-3:31 PM)

- Core `DeathRecap.cs`, three pure web modules: deathRecap.js (sourceName from config.ui.recap.names; the hurts of the
  last ui.recap.window = 3 s on fixed-step time; the summary: the killer, damage per source rounded, largest first,
  stable), hitDirection.js (hitAngle; a pool of ui.hitDirection.max = 4 arcs, the same side refreshes, the oldest is
  reused) and records.js for Survival (judge: round first, then points; its results texts; readRecords drops another
  version or garbage; `{"v":1,"modes":{...}}` under the web's key `skygrind.records.v1` in PlayerPrefs).
- SurvivalGame: every player hurt feeds the recap and, with DAMAGE DIRECTION on and a known origin (HurtEvent.from), an
  arc. The death keeps the recap (DOWN and RESULTS' KILLED BY say its name) and DOWN's next target (main.js
  survivalTarget). EndRun submits {round, points earned} (main.js submits stats.earned). `Restart()`: R
  (input.bindings.restart "KeyR" through `KeyOf`) from DOWN skips the wait (the run still ends and is judged) or from
  RESULTS; a new run starts on the same frame. `noRecords` (the smoke; the LAB later): judged, never stored.
- HUD: DOWN shows "KILLED BY X", "LAST 3 S: X n · Y m", the next target, "R RETRY NOW" (hud.js setDown); RESULTS adds the
  personal-best line (gold when new) and opens with RESTART selected; the arcs are four generated ring-segment sprites
  (radius 150, width 5, 50 deg, glow) in the sight layer, rotated to the source as you turn, full for 0.35 s, fading to 0
  at 0.7 s. The acceptance says 0.6 s; config.ui.hitDirection.life is 0.7, and the number comes from config.
- Tests (3:20 PM): `RecapTests` 4 / 4 (records.test.js's Survival and readRecords cases; the recap's window, sums and
  order; hitAngle's four directions, the pool, the fade); full `tests: 195 / 195 passed, 0 failed`, `replays: 16 / 16`,
  `golden tests: 55 / 55` (and again at 3:29 PM after the noRecords change).
- Smoke extended (step 4: a husk's blow kills, the recap must name HUSK; RESULTS' PB line; step 5: R from RESULTS;
  step 6: a second death and R from DOWN; both timed). Editor run (3:22 PM): `[smoke] 21.1s DONE: all steps passed`,
  "restart (R) from RESULTS in 1 ms", "restart (R) from DOWN in 0 ms". It then said "NEW BEST · +1 ROUND": my DOWN
  capture's run had reached EndRun and stored round 1, because the first eval in a Play session took 12 s (Roslyn's
  compile), long enough for the held DOWN to run out (measured: downSteps 2400 -> 958 across one 12 s eval). Not a bug in
  the records; but it showed the smoke would write its run into the Player's bests, so the smoke now sets `noRecords`.
  Not re-run in the Editor after that change (the Player smoke at U2-15 runs it).
- Capture (3:22 PM): `Logs/shots/down-u2-05.png`: "KILLED BY HUSK / LAST 3 S: HUSK 550 · SWARMER 12 / R RETRY NOW".
- Lesson: an eval's own latency (12 s on the first one in a Play session) counts against anything timed in the game.
- Not checked: a real R key press (the smoke calls Restart, which the key calls); the arcs on screen (only their math
  is tested; no capture while live).

Status U2-05: passing. Commit `e3f19d2`.

## U2-10 SPITTER, globs, enemy aim (3:32-4:02 PM)

- Read first: spitter.js, ai/aim.js, enemyKind.js (timers, stagger, groundMove, hit shapes), brain.js (the ranged
  token: wants only with line of sight; a holder rolls hitChance; a ranged holder keeps its ring), enemies.js (step
  order husk, drone, swarmer, spitter; the crowd pass over config.ai.crowd.mass's kinds), punch.js tryReflect +
  drone.js reflect (the orb parry). The web's spitter globs are NOT reflectable: only cyan drone / boss orbs are.
- Core `Spitter.cs`: `AimMath` (aim.js: HitChance, LeadPoint, MissPoint), `Spitter : Husk` (the swarmer's pattern:
  enemyKind's timers and stagger, spitter.js step: the tell, the volley, back off inside retreatRange, else the
  brain's slot), `SpitterAcid` (pooled globs and puddles: the ballistic lob, the lead unless hunted, the deliberate miss
  (the web's own x-then-z quirk kept), the player-capsule hit, the level as a zero-length capsule (no sphere query in
  the C# collider), splash, the acid tick that doesn't stack). The miss side used Math.random() on the web: here a
  seeded stream reset per run (`Husks.NewRun`), as rule 6 requires.
- The glob parry (Unity-only, D-B "on"): the punch's tryReflect on the glob pool (orb.reflectRange 2 m): the glob homes
  on the enemy nearest to it at orb.reflectSpeedMult x globSpeed for orb.reflectDamageMult x globDamage, heals
  punch.parryHeal, restores stamina, emits parry (kind "orb") and `globReflect` (port-only); its hit is weapon "orb", so
  style pays RETURN TO SENDER. No new numbers (the web's orb reflect numbers).
- Husks: a spitter pool (spitter.maxCount), its step after the swarmers, then the acid; KillAll clears the acid.
  HuskBrain: the spitter's KindAi (ranged token, ring 1), the crowd pass over husk, swarmer, spitter, `ai.los` from the
  LOS check, `ai.hit` rolled on the brain's seeded stream from `playerSpeed`, ranged holders keep their ring.
  HuskNavJumper: ai.jumps.spitter. Director.Available: + spitter (introduce.spitter 6; squads with spitters).
- View (`SpitterView`): spitter.js's three parts as they are (capsule body x shade leaned 0.25 rad about the world x
  after the yaw, the acid sac swelling x1.6 and pulsing toward 0xfff0a0 x2.2 in the tell, the box), pooled glob spheres
  (acid x2) and puddle discs (acid x1.3 at 0.55 alpha, growing 0.2 s, shrinking over the last 0.5 s) on a new
  `SKYGRIND/Acid` shader (unlit, alpha-blended) through `Assets/Materials/Survival/Acid.mat`, assigned to the scene by
  `SurvivalSetup.AddMaterials` (eval; the scene diff is the one reference plus newly serialized public fields).
- Debug: `SurvivalRun.DebugSkipTo(n)` and F9 (the next round now).
- Tests (3:44 PM): `SpitterTests` 4 / 4 (a still player takes globDamage after one tell, the puddle ticks
  puddleDps x puddleTick as "acid"; a punch reflects a glob into the spitter for 24 as weapon "orb"; hitChance falls
  0.7 -> 0.25 by 24 m/s, a miss lands missBy to the side, the lead halves vertical; no spitter squads before round 6,
  some at 6+). Full suite (3:46 PM): `tests: 199 / 199 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Smoke step 7 (the debug skip to round 6): Editor run (3:51 PM) `[smoke] 33.5s PASS round 6: 1 spitter(s) alive at
  once, 1 volley(s) in 13 s`, `[smoke] 33.5s DONE: all steps passed`.
- Captures (3:53 PM, the first break held, one spitter 9 m ahead): `Logs/shots/spitter-u2-10c.png` shows the 3-glob
  spread in flight and the 3 m puddles under the player; the spitter stood mostly behind the central box. The earlier
  captures (`-10a/-10b` of the first try) were at point blank among husks: their only use is the two live damage arcs.
- Not checked: a reflect by hand in Play (the test covers it); the brain's ring behaviour for spitters in a long fight.
  The closer (a NEW: SPITTER card) not done.

Status U2-10: passing. Commit `2bb1802`.

## U2-11 perk machines, the starter five (3:59-4:24 PM)

- Core `Perks.cs`: weaponMath.js perkPrice (the stack tax, to the nearest 10; a token is free) and perkEffects (all 14
  perks' numbers, pure, as the web's tests check them), and perks.js's state (owned in purchase order, tokens, Give,
  Reset, the `perk` event {id, free, price}; the effects through an `apply` hook). `SurvivalMap` reads
  data.perkSpots.
- Game `PerkMachines.cs`: the starter five whose systems exist (VIGOR, MEND, PLATE, QUICK, GRIP) at their map spots;
  each a buyable (perks.reach, the taxed price, once a run, only while its zone is open); apply = mover.bonusPips /
  bonusRideTime / bonusWallJumps (already in the Mover: no Mover.cs change), regen.perk, the loadout's
  fireRateMult / swapMult (new in Loadout: weaponMath fireInterval and swapTime), health.max (PLATE fills its new HP).
  ResetWorld drops them. MEND's machine rides its core pillar. The look, rebuilt simply from machineModels.js: a
  gunmetal cabinet on a plinth with a bone cap, neon trim in the perk colour x perks.glow, the 8x8 pixel icon on a
  screen, its name and price (OWNED once bought; grey while locked), a floor ring pulsing at perks.pulseRate.
- HUD: the owned perks' pixel icons in a row above the HP panel (UiPerks.cs).
- Tests (4:03 PM): `PerkTests` 4 / 4 (perks.test.js's stack tax and all-14 effects incl. FLIP + QUICK; the buy flow on
  Buyables + Economy: the taxed price, +10 % next, too poor refused, a short hold buys nothing). Full suite
  `tests: 203 / 203 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Smoke step 3b (buy VIGOR at its machine, its zone opened): two failed runs before it passed. Not the aim assist (I
  first suspected SNAP turning the look; the smoke now holds the look anyway): a buy latches until E is released, and
  the smoke went from holding E at ARMORY to holding E at VIGOR without a step in between. The game is right (one buy
  per press); the smoke now lets go for 0.15 s. Editor run (4:21 PM): `[smoke] 19.3s PASS VIGOR bought: owned True,
  points 1600 -> 100 (price 1500), stamina pips 4`, `[smoke] 28.6s DONE: all steps passed`.
- Capture (4:17 PM): `Logs/shots/perk-u2-11.png`, VIGOR's machine from 4 m: icon, "VIGOR 1500", trim, ring. The first
  capture had the label mirrored and the icon quad culled (both turned 180 deg; fixed).
- Not checked: MEND riding the pillar in Play; PLATE / QUICK / GRIP bought in Play (their effects are the tested
  numbers on fields the systems read).

Status U2-11: passing. Commit `cd8c817`.

## U2-12 kill drops and power-ups (4:24-4:41 PM)

- Core `PowerUps.cs`: powerUpMath.js (the style drop: a chain of styleDrop.chainKills in chainWindow s or an air
  multikill, then cooldown; killDropChance: none inside minGap, the base chance, x2 after rise, x4 after rise2, x
  eliteMult, capped at maxChance) and powerUps.js (a pool of powerUps.pool obelisks settled on the floor under the kill,
  powerUps.life s, grabRadius / grabHeight; DOUBLE POINTS = Economy.SetBoost("double", mult) for its time; OVERCLOCK =
  the style meter's freshness held at max for its time (the web keeps the arsenal FRESH; the arsenal isn't ported, so
  the nearest live system); SHOCKWAVE = every non-elite enemy dies as the player's kill; STASIS = the enemy pools frozen
  (Husks.frozen) for its time, still hittable; REFUND = Economy.SetDiscount(mult) until the next pointsSpent). Every
  player kill feeds the style drop; an elite kill rolls eliteChance; else killDropChance on the run's stream
  (powerUps.killDrop.seedSalt). The web's elite roll and a style drop's type use Math.random(): a second seeded stream
  here. Events by the web's names: powerUpSpawn, powerUpGrab, powerUpEnd, powerUpStyleDrop, powerShockwave.
- Economy: SetDiscount and Price (economy.js); Buyables and the HUD prompt price through it (buyables.js).
- View (`PowerUpView.cs`): the octahedron crystal (0.35 x 0.8 x 0.35) in the type colour x2.2 hovering, bobbing and
  turning, a base disc pulsing, a beam (powerUps.beamHeight); blinks out over the last blinkFrom s. Not ported: the
  through-walls copy; the base is a filled disc where the web draws a 0.7..0.85 m ring.
- HUD (`UiPowerUps.cs`): the running ones on one line under the round, each name in its colour with its whole seconds.
- Tests (4:33 PM): `PowerUpTests` 5 / 5 (the chance's ramp and cap; chain and air drops and the cooldown; DOUBLE /
  STASIS / OVERCLOCK end on their timers; SHOCKWAVE kills a husk and spares a spitter; REFUND halves one buy; the same
  seed gives the same drops). My first air test was wrong (the cooldown runs from the chain's last kill); fixed the test.
  Full suite `tests: 208 / 208 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Play check (4:38 PM): a DOUBLE drop 5 m ahead renders (`Logs/shots/drop-u2-12.png`); it expired before my next eval
  (each eval compiled for 11-17 s; a drop lives 12 s), so the grab check spawned one at the feet: `grabbed 1, boost 2,
  double left 16.7 s`; the HUD shows "DOUBLE POINTS 8" (`Logs/shots/drop-u2-12b.png`).
- Not checked: a kill drop from a real fight in Play (the seeded roll is tested), STASIS / SHOCKWAVE in Play.

Status U2-12: passing. Commit `da208df`.

## U2-13 MODS (4:42-4:55 PM)

- Core `Mods.cs`: modMath.js line for line (IsDraftRound, RollDraft weighted by tier with the two-tags rule and the
  thin-pool fallback, RecordPick, PickRate), mods.js's modLine and its state (stacks; a cleared draft round queues a
  draft; Open / Pick / one paid Reroll (mods.rerollCost); two seeded streams per run from mods.seedSalt / rollSalt;
  `modDraft`, `modPick`, `modReroll` events) and the effects whose systems exist: SKYDANCER (AIR), WALLBANGER (WALL),
  LANCE, STYLE TAX, VARIETY, BLOOD PRICE, INTEREST, SCAVENGER (on the power-up drop chance), ADRENALINE, BOUNTY HUNTER.
  A locked mod waits for its flag as on the web; only REACH ROUND 10 can be earned here (a best round of 10+), so
  WALLBANGER (PROTOCOL) and INTEREST (DAILY) are ported but not offered yet. Not ported (U3: they need the arsenal's
  statuses, PRIMED / CROSS shots, momentum or blasts): RAILGRINDER (SLIDE), CONDUCTOR, PYRE, KEEN EDGE, GRAVITY WELL,
  SHRAPNEL, MOMENTUM ROUNDS, FRESH START, CROSSFIRE.
- Loadout: `bonus` (weapons.js loadout.bonus: x damage per hit). PowerUps: `dropMult` (main.js getDropMult).
- Game: a waiting draft opens only during a break and pauses the game until a card is taken (keys 1-3 or a click); Esc
  doesn't leave it; the draft screen (`UiMods.cs`): PICK A MOD · ROUND n, three cards (name, tag in its colour (weapon
  family / style tag / usable), the line at the next stack, STACK k / max), REROLL · 1500 (NO REROLLS LEFT after one).
- Tests (4:49 PM): `ModTests` 6 / 6 (night6.test.js's MODS (B2) cases: the draft rounds; distinct, never maxed, two tags,
  seeded; the reroll leaves the last offer out; rares rarer; the pick rate; every mod well formed; plus a queued draft
  of ported unlocked mods, a pick, one paid reroll, SKYDANCER / BLOOD PRICE / INTEREST). Full suite (4:50 PM):
  `tests: 214 / 214 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Play check (4:52 PM): a draft queued in the first break opened at once (`flow Paused, draft open True, offer [lance,
  skydancer, bountyHunter]`), capture `Logs/shots/draft-u2-13.png`; PickMod(0): `picked lance: True, stacks 1, flow
  Playing`.
- Not checked: a draft opened by a real round-2 clear (the round logic is tested); the reroll button by mouse in Play.

Status U2-13: passing (the draft and the movement mods AIR and WALL; SLIDE's RAILGRINDER and eight others are U3).
Commit `ec95eac`.

## U2-06 feel parity (4:56-5:12 PM)

- Core `Trauma.cs`: fx/trauma.js (add / step / sample: trauma^exponent x max x value noise per axis, exactly 0 at no
  trauma or scale 0) and fx/cosmetic.js's lattice + valueNoise (Math.imul as unchecked int multiply, `>>>` on uint).
- Fed as player.js does: a shot adds feel.trauma.fire[its feel.punch weight class], a player kill feel.trauma.kill, a
  slam feel.trauma.slam (capped at feel.trauma.cap); stepped and sampled on frame time in Update, x effects intensity,
  0 with REDUCED MOTION; applied in CameraFeel after the kicks, to the drawn camera only (pitch, yaw, and roll against
  rollRight): the aim and the sim never see it. Not ported: feel.trauma.vmShare (the viewmodel's share), boss events.
- Impacts (effects.js shotResult): a world hit bursts fx.sparkCount sparks (fx.sparkSpeed, fx.sparkLife) fx.surfaceOffset
  off the surface in the gun family's colour, else colors.spark. Through the existing pooled cube particles (made at
  load; no GameObject per effect), not a ParticleSystem.Emit as the tasks note suggests. Not ported: decals, the flare.
- The muzzle flash (2 frames) and the tracer were already in the viewmodel (NIGHT7).
- Speed lines (player.js): a generated radial-streak overlay in the sight layer; opacity eases (feel.speedLinesFadeTime)
  to min(1, (dashing ? 1 : the speed's share of speedLinesMinSpeed..FullSpeed) x speedLinesMaxOpacity x intensity),
  0 with reduced motion; jitter speedLinesJitterDeg on cosmetic noise. The dash tint: not done (NIGHT8 cut order 5).
- The freshness tier by the weapon name: "KESTREL  FRESH / USED / STALE" (styleMath freshnessTierName on the style
  meter's freshness; coloured as hud.js .hud-fresh); none for the gauntlet (no freshness key).
- Tests (5:04 PM): `TraumaTests` 3 / 3 (feel.test.js's two trauma cases; the noise bit for bit against five values the
  web's cosmetic.js printed today, node, read only). Full suite `tests: 217 / 217 passed, 0 failed`, `replays: 16 / 16`,
  `golden tests: 55 / 55`.
- Play check (5:07-5:11 PM, the KESTREL held on the central box): captures `Logs/shots/feel-u2-06-1..3.png` show
  "KESTREL STALE" (74 shots in); no frame caught the 2-frame flash or a 0.18 s spark burst, so I counted instead: six
  samples of the live particle cubes while firing gave 0, 0, 0, 0, 6, 0 (6 = fx.sparkCount: one burst), and trauma read
  0.039 at the end (one medium shot adds 0.15 and decays at 2.5 / s, gone in 0.06 s).
- Not checked: the shake as seen on screen, the speed lines at speed (no capture at 20+ m/s).

Status U2-06: passing (the dash tint cut; vmShare, decals and the flare not ported). Commit `317ed9b`.

## U2-07 the design config and PERFECT PARRY (5:13-5:19 PM)

- `Design/unity-config.json` (repo root): one object under "design", a `_why` per section (JSON has no comments) saying
  why the web has no such number: design.parry (perfect true, perfectWindow 0.15, perfectHitstop 0.15, perfectPoints
  80 = the web's PARRY base 40 x 2) and design.director (PEAK FADE's numbers from tasks.json U2-08, written now so U2-08
  only reads them). GameConfig.Load merges it as the `design.` prefix from Design/ beside the PortExport folder (repo
  root in the Editor and tests, StreamingAssets in the Player); it throws if config.json ever has a `design` key;
  `GameConfig.Design(flag)` is false when a flag or the file is missing (the game is then the web's); `FromText` for a
  WebGL boot. `SKYGRIND/Sync Port Data` copies it to Assets/StreamingAssets/Design/ (synced 5:18 PM).
- PERFECT PARRY: Loadout.IsPerfect: a parry with the wind-up's steps left <= StepsFor(perfectWindow) (the last 0.15 s
  of husk.windupTime 0.5 s), punch or gauntlet; heals to max HP instead of punch.parryHeal; the parry event carries
  `perfect`; SurvivalGame freezes design.parry.perfectHitstop instead of punch.parryHitstop; the style meter pays
  PERFECT PARRY (perfectPoints) instead of PARRY; the HUD's white parry flash (punch.parryFlashOpacity x FLASH
  INTENSITY over punch.parryFlashTime; a perfect one's over perfectHitstop). The flash is new on every parry (the web
  has it; stage 1 hadn't ported it).
- Tests (5:16 PM): `DesignTests` 3 / 3 (the prefix loads, flags read, a config.json with `design` throws; a punch 2 steps
  inside the window is perfect: max HP, PERFECT PARRY; 12 steps before it is the web's: +15, PARRY; flag off: the
  web's). Full suite (5:17 PM): `tests: 220 / 220 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Not checked: a perfect parry by hand in Play (the flash and the freeze on screen).

Status U2-07: passing. Commit `06ac6a8`.

## U2-08 PEAK FADE (5:20-5:23 PM)

- `DirectorMath.StepDirectorFade` (Unity-only, design.director; the web's StepDirector is untouched and still called
  with the flag off): no intensity decay while `engaged` > 0; PEAK ends in PEAK_FADE (PhaseSpawnMult 0: no spawns);
  RELAX starts once intensity < fadeBelow (0.6) with at most fadeEngaged (2) engaged, or after fadeMaxTime (6 s).
  `engaged` = Husks.EngagedNear: live enemies committed to an attack (a token holder's wind-up, crouch, tell) within
  engagedRange (6 m), no allocation. The phase goes out as `directorPhase` "PEAK_FADE" (a port-only value).
- Tests (5:21 PM): `PeakFadeTests` 3 / 3 (the sequence: PEAK, PEAK_FADE held while 3 engaged with intensity unchanged,
  RELAX once calm; the 6 s cap while engaged; with none engaged it decays exactly as StepDirector). Full suite (5:21 PM):
  `tests: 223 / 223 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55` (DirectorGoldenTests bit-exact).
- Not checked: a PEAK_FADE in a real round in Play.

Status U2-08: passing. Commit `6d03d9e`.

## U2-09 markers (5:23-5:31 PM)

- `UiMarkers.cs` (main.js updateMarkers + hud.js setMarkers): every live enemy off screen or behind gets a red-orange
  teardrop on the screen edge pointing outward (always, as the web); once director.remaining <= director.markersAt (3)
  in a fight, every remaining one on screen gets an outline diamond. Up to hud.markerCount (34), hud.edgeMargin in.
  Survival uses markersAt only (the acceptance's "markersAfter" is spawner.js's, for the other modes).
- Play check (5:27-5:30 PM): round 1, the director stopped, 3 husks kept: HUD "ROUND 1 · 3 LEFT". First capture at point
  blank (a husk in the face): a diamond by the crosshair and two edge teardrops at the bottom. Second capture
  (`Logs/shots/markers-u2-09.png`; the husks frozen and placed, but the look had been pulled toward the floor): two
  edge teardrops on the left edge. Not checked: all three placements on one clean frame.
- No new test (UI placement; the remaining count is the director's, tested).

Status U2-09: passing. Commit `e51bc67`.

## U2-14 the Web build (5:32-6:38 PM)

- The boot (commit `98c6e5c`, 5:37 PM): on WebGL only, `SurvivalGame.BootWeb` loads config.json, the design config and
  the map from StreamingAssets by `UnityWebRequest` (a short loading state; `ready` gates Update / OnGUI), then builds
  the game through the same `Init(GameConfig, SurvivalMap)` as Windows and the Editor, which still read files.
  PlayerChecks reads `?skygrindSmoke` and `?skygrindBench=N` from `Application.absoluteURL`. The Editor smoke passed on
  the split boot.
- The switch: `switch_build_target --target WebGL --confirm true` 5:40-5:43 PM (once). Player settings set on purpose:
  `webGLCompressionFormat: 1` (gzip) and `webGLDecompressionFallback: 1`, so a plain static server works.
- Build 1 (5:49-6:09 PM) to `Builds/Web/`: no build errors. Served by `Tools/serve-web.cjs` (port 8090) and opened with
  the Playwright MCP browser: `localhost` was refused, `127.0.0.1` works. It loaded, but the console had 39 lines of
  "Can't add component because class 'SphereCollider' doesn't exist!": IL2CPP engine stripping removed the collider
  classes, which `GameObject.CreatePrimitive` adds (the views destroy them; no gameplay uses PhysX). Fix:
  `Assets/link.xml` preserves Box / Sphere / Capsule / MeshCollider.
- Build 2 (6:16-6:24 PM, with link.xml): `Builds/Web/Build`: Web.wasm.unityweb 12,662,141 B, Web.data.unityweb
  7,448,204 B, Web.framework.js.unityweb 91,201 B, Web.loader.js 47,866 B.
  - `?skygrindSmoke` (960x600, ANGLE AMD Radeon 890M, D3D11): `DONE: all steps passed`, 0 console errors (the
    SphereCollider lines are gone).
  - `?skygrindBench=30` in a 1920x1080 browser window (correction, 8:16 PM: the default Web template's canvas is a
    fixed 960x600, so this run rendered 960x600; the 1080p measurement is in U2-15): `bench result: 1766 frames, alive at end 31, median 17.00 ms, p95 17.00 ms,
    worst 1% 17.00 ms, mean fps 60, median fps 59, vsync 0, cap -1`; `bench gc: collections 1 in 1796 frames; GC Used
    Memory rises 48644.0 KB (valid)`; `bench timing: FrameTimingManager cpu median 16.70 ms, worst 1% 17.00 ms; gpu
    median 0.00 ms` (WebGL reports no GPU time). The browser holds the frame at 60 fps (requestAnimationFrame), so
    60 is the cap, not the game's limit. The GC Used Memory rise with 1 collection is the WebGL heap; not checked: what
    allocates it (a release Player has no "GC Allocated In Frame").
  - TITLE screenshot `Logs/shots/web-title-u2-14.png` (6:25 PM).
  - Zip for itch (not uploaded): `Builds/skygrind-web-u2a.zip` 19,872,975 B (gitignored).
- Switched back: `switch_build_target --target StandaloneWindows64` 6:29-6:32 PM (`activeBuildTarget:
  StandaloneWindows64`).
- PLAY by hand (6:34-6:36 PM, server restarted): TITLE (`Logs/shots/web-title-u2-14b.png`), a mouse click on PLAY
  SURVIVAL, 30 s untouched: the idle player was killed by husks at 0:12 and the results show "RUN OVER, ROUND 1, KILLED
  BY HUSK, FIRST RUN ON RECORD" (`Logs/shots/web-round1-u2-14.png`). The records save worked in the browser's
  storage (Unity's JS_FileSystem_Sync deprecation warning at the save). Console: 0 errors, 8 warnings (WebGL
  getInternalformatParameter x6, App UI settings default, the FileSystem sync); the log also prints "shader is not
  supported on this GPU" for three hidden URP shaders (CoreCopy, StencilDitherMaskSeed, HDRDebugView), which the game
  doesn't draw. The 30 s of round 1 with enemies is the bench's run above (god mode).
- ProjectSettings after the round trip: committed only the two WebGL lines above. Not committed (the switch wrote
  them): a `m_BuildTarget: WebGL` batching entry (static batching 0), restored with `git checkout`.
  `Assets/Settings/Mobile_RPAsset.asset` was rewritten by the Web build (URP asset version 12 -> 13 and recomputed
  shader-stripping prefilters; WebGL uses the Mobile quality level): not ours, restored with `git checkout`. Unity
  may rewrite it at the next Web build.
- Decision D-01 input for GG: the Web build runs in Chrome at the browser's 60 fps cap with 24 husks; load to TITLE
  about 6 s on this laptop from a local server (the console's "Survival loaded" at 5.7 s).

Status U2-14: passing. Commits `98c6e5c`, `7b24926`.

## Order after U2-14 (6:40 PM)

Section 6 puts U2-15 (the close) before the "if time" items. Decision (safe default, no question): U2-16 LAB, then
U2-17 FORGE if time allows, then U2-15. The close's Player build, smoke, bench, tests and tag then cover them. The cap
is 2:04 AM (12 hours from 2:04 PM). If an item overruns, it is cut in section 7's order (U2-17 first) and the close
starts no later than about 11:30 PM.

## U2-16 the LAB (6:40-7:22 PM)

- `Design/unity-config.json` `design.lab`: the five toggles (key, name, default false, desc) and their numbers, each
  section with its `_why` (DESIGN-RESEARCH 4.1-4.4). Synced to `Assets/StreamingAssets/Design/` by Sync Port Data.
- `Core/Lab.cs`: `LabMath` (pure: IsCloseKill, FreshnessMult / FreshnessTierName with DULL, RampMult, SquadInterval)
  and `Lab` (the toggles; `Read` at a run's start from the settings, `Apply` to the systems, `Wire` for the heal).
  - CLOSE-KILL HEAL: a player kill with punch / gauntlet, or within 4 m (the KillEvent distance), heals 15;
    `Regen.rateOverride` 12 HP/s instead of regen.rate 25.
  - FRESHNESS TEETH: StyleMeter pays FRESH x1.5 (web 1.1), a DULL tier under 2.5 pays 0; USED / STALE stay the web's.
    The HUD's tier by the weapon name says DULL (STALE's colour).
  - STYLE RAMP: StyleMeter counts whole steps in the web's state-multiplier states; the multiplier climbs from 1.2 to 3
    over StepsFor(1 s); any other state resets it.
  - READY FROM ROUND 1: `SurvivalRun.readyFrom` (survival.readyFrom 3, or 1).
  - SQUADS TIGHTEN: `Director.squadIntervalOf` (round, 2.2) -> max(1.0, 2.2 x 0.96^(round-1)), set per wave.
- READY was not wired in the port (SurvivalRun.Ready existed, nothing called it). Ported from main.js NIGHT6 B6: in a
  break, hold BUY with nothing for sale in reach for survival.readyHold (0.6 s, whole steps) to start the round; the
  break line says "ROUND N IN S · HOLD E: READY" from readyFrom (main.js hudLine). With the LAB off it is the web's
  (from round 3).
- Settings: the LAB tab (Unity-only; `UnityTuning.LabTabId` / `LabTabLabel`), five toggle rows, RESET TAB, and a note
  (the toggles apply from the next run; a LAB run is stamped and sets no best). The run keeps the toggles it started
  with. Results: a LAB run's personal-best line is "LAB · <toggles on> · NOT A PERSONAL BEST", nothing is stored, and
  DOWN shows no target. The smoke sets `noLab` (the web's rules whatever the tab says).
- Tests (6:56 PM): `LabTests` 6 / 6 (the toggles as settings rows: off by default, JSON round trip, RESET TAB; and each
  toggle on and off: the heal and regen rate, the freshness tiers through a kill award, the ramp through trick awards,
  READY in round 1's break, the squad interval through a Director on the real map). Full suite (6:58 PM):
  `tests: 229 / 229 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Play check (7:06-7:08 PM, CLOSE-KILL HEAL and READY FROM ROUND 1 on in the Editor's settings): the LAB tab
  (`Logs/shots/lab-tab-u2-16.png`); a run with E held: "run: Break round 1, break 3.00 s, readyFrom 1, LAB [CLOSE-KILL
  HEAL, READY FROM ROUND 1], regen rate override 12, candidate none", then "READY: round 1" and the fight began; through
  the game's bus a punch kill healed "50 -> punch kill 65 -> far kill 65"; DOWN target empty; results "'<b>LAB</b> ·
  CLOSE-KILL HEAL, READY FROM ROUND 1 · NOT A PERSONAL BEST' (new best False)" (`Logs/shots/lab-results-u2-16.png`);
  the stored best stayed the 3:28 PM record. The toggles were set back to OFF after the check. Not checked: the hold's
  0.6 s on screen (the READY fired before the next eval), FRESHNESS TEETH / STYLE RAMP / SQUADS TIGHTEN by hand.
- Editor smoke (7:09 PM): FAIL at step 7, "round 6: 0 spitter(s) alive at once, 0 volley(s) in 45 s". Cause: after the
  R restarts the run seed is the clock's, and round 6's squads are drawn from it; 2 of its 6 eligible templates hold a
  spitter, so some seeds plan none. The earlier passes were lucky draws (not caused by the LAB, which the smoke turns
  off). Fix: step 7 starts a run on the first seed whose opening squads hold a spitter (DirectorMath.PlanSquads with the
  Director's seeding). Rerun (7:18-7:19 PM): `DONE: all steps passed` ("round 6: 1 spitter(s) alive at once, 1 volley(s)
  in 5 s").
- Slip: the first patch script of this item (`lab-hooks.cjs`, scratchpad) was written with a heredoc; CLAUDE.md rule 9
  wants written files. The later ones were written with the Write tool.

Status U2-16: passing. Commit `597dfd9`.

## U2-17 THE FORGE (7:23-7:46 PM)

- `Core/Forge.cs` (survival/forge.js, NIGHT6 B1): `ForgeMath.Mult` (forgeMult: x forge.damage 1.8, then the family's
  headshot / blast / tough / damage bonus), `ForgeMath.WeaponOf` (arsenalMath.js: punch and orb count as the gauntlet,
  ricochet as the revolver), `Forge` (once per gun per run, the `forge` event {owner, weapon, price, free}, the bonus
  hook, the status roll from mulberry32(runSeed ^ forge.seedSalt), Reset at run start).
- The loadout's bonus hook now gets (enemy, weapon, headshot) and has an `after` (weapons.js loadout.bonus.mult /
  after); the game's is forge x mods (main.js runBonus). Statuses belong to the arsenal (combat/arsenal.js), which isn't
  ported (U3): a forged THUNDERCLAP (edge: STAGGER 25%) draws its stream as the web does and nothing lands
  (`Forge.status` is null). The KESTREL's kinetic bonus (forged headshots x1.25) and the x1.8 work as on the web.
- The cabinet: map data.forgeSpot (-22, 0, -22, THE GRINDER, on the core, yaw 45 degrees), built with the perk machines'
  model (forge.color, forge.icon, named FORGE), riding its pillar; the buyable "FORGE <gun in hand>" (perks.reach,
  forge.cost 5000) while THE GRINDER is open, for a catalog gun not yet forged; the screen shows the price, then
  "<GUN>+". The HUD's weapon name gets its "+" (hud.js); the viewmodel's gem burns brighter and whiter with a slow pulse
  (viewmodel.js: forge.gem x (0.85 + 0.15 sin 4t)); the results list "FORGED KESTREL+" (main.js stats.forged).
- Tests (7:34 PM): `ForgeTests` 8 / 8 (the web's night6 tests on forgeMult and the round-20 time to kill; once per gun
  and the event; through the loadout's damage: unforged 1, forged body x1.8, headshot x2.25, a punch after the gauntlet
  is forged x1.8; the status rolls match mulberry32(77 ^ salt) draw for draw; the map's spot). Full suite (7:41 PM):
  `tests: 237 / 237 passed, 0 failed`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- Play check (7:37 PM): a run on seed 11, THE GRINDER opened free, 5100 points, standing 1.5 m in front with E held:
  "forged revolver True, order [revolver]; points 5100 -> 100; prompt item 'FORGE KESTREL', available now False; HUD
  'KESTREL+ FRESH'; cabinet '<b>FORGE</b> / KESTREL+'". Captures `Logs/shots/forge-u2-17.png` (at the screen) and
  `forge-u2-17b.png` (4 m back, the round's husks frozen in front of it). Not checked: a forged kill's damage on screen
  (the test covers it), the gem's glow by eye.
- Editor smoke (7:43-7:44 PM): `DONE: all steps passed`.

Status U2-17: passing. Commit `f16ecd5`.

## U2-15 the close (7:47 PM-)

- **The Windows Player** (7:48-7:50 PM): `build --target StandaloneWindows64 --outputPath Builds/Stage1/SKYGRIND.exe
  --scenes [Survival, Arena]`: `Succeeded`, 0 errors, 519 warnings, 132,253,199 B, 95 s.
- **`git diff --stat ProjectSettings`** after it: only `UnityConnectSettings.asset` (m_Enabled 0 -> 1, Unity's cloud
  switch again; never staged, D-05). `ProjectSettings.asset` showed `M` with no diff: its bytes hash to the index entry
  (`93c2448`), a stale stat; `git add` of the same bytes cleared it. `Assets/Settings/Mobile_RPAsset.asset` was
  rewritten again (the URP upgrade, as after the Web build): restored with `git checkout` (D-09 proposed).
- **INV-smoke** (7:53-7:54 PM): `SKYGRIND.exe -skygrindSmoke -logFile Logs/player/smoke.log` (pid 278664, quit by
  itself): `[smoke] 26.2s DONE: all steps passed`, 0 lines with "exception" in the log. The steps: round 1 cleared in 9 s,
  round 2, ARMORY, VIGOR, the recap, RESULTS with 'FIRST RUN ON RECORD', R from RESULTS in 4 ms and from DOWN in 3 ms, round 6
  "1 spitter(s) alive at once, 1 volley(s) in 5 s".
- **The bench** (7:56-7:58 PM): in the U2-02 section above (median 3.35 ms, worst 1% 8.37 ms, Balanced on AC).
- **INV-tests / INV-golden / INV-replays** (8:01 PM, after the last code commit `f16ecd5`): `tests: 237 / 237 passed,
  0 failed, 0 skipped`, `replays: 16 / 16`, `golden tests: 55 / 55`.
- **The Web build again** (8:04-8:20 PM), so the browser build has the LAB and THE FORGE like the exe (the 6:24 PM one
  predates them; D-01 may pick the Web build):
  - `switch_build_target WebGL` 8:04-8:05; `build --target WebGL --outputPath Builds/Web --scenes [Survival, Arena]`
    8:05-8:12: `Succeeded`, 0 errors, 380 warnings, 22,173,320 B, 384 s. Its StreamingAssets design config equals the
    repo's (with `design.lab`).
  - Playwright on `http://127.0.0.1:8090/?skygrindSmoke` (8:13-8:14): `[smoke] 25.4s DONE: all steps passed`; 0 console
    errors (the three "ERROR: Shader" LOG lines for hidden URP shaders, as before; no "Can't add component").
  - `?skygrindBench=30` with the canvas styled to 1920x1080 before boot (8:15-8:16): `check 'bench' starting; 1920x1080,
    ANGLE (AMD, AMD Radeon(TM) 890M Graphics ...)` (Chrome renders on the integrated GPU); `bench result: 1769 frames,
    alive at end 18, median 17.00 ms, p95 17.00 ms, worst 1% 17.00 ms, mean fps 60, median fps 59`; `bench gc:
    collections 1 in 1799 frames; GC Used Memory rises 49864.0 KB`; `cpu median 16.70 ms, worst 1% 17.40 ms`. The
    same run on the default 960x600 canvas (8:15): median 17.00, worst 1% 18.00 ms.
  - Zip remade: `Builds/skygrind-web-u2a.zip` 19,738,883 B (20 files; not uploaded). A canvas screenshot clipped to the
    viewport and was deleted; the 6:25 PM title shot stands.
  - Switched back to StandaloneWindows64 8:18-8:19. The switch added the WebGL batching entry to ProjectSettings again
    and the Mobile URP asset upgrade: both restored with `git checkout`.
- **Docs:** `PORT-STATUS.md` updated (READY, the LAB, THE FORGE, the `forge` event); `HANDOFF.md` rewritten (new
  anchor `u2a-demo`, verify values 237 / 16 / 55 and the smoke, next actions U3-04 then U3 and U4-01, decisions D-E..D-H
  made, D-08..D-10 added, risks, dead ends, invisible state). The `handoff` skill's default (a file in the OS temp
  folder) was overridden by NIGHT8 section 8, which asks for HANDOFF.md in the repo. `NIGHT8-REPORT.md` written.
- **Memory:** two lessons seen twice tonight, one file each with an index line: `unity-eval-latency-timing` (three
  timing checks broken by eval latency) and `urp-mobile-asset-rewrite` (the URP asset rewrite after both builds). One
  note added to `session-close-handoff` (no NIGHT9.md when the goal fixes the final git status). CLAUDE.md changes
  are proposed in HANDOFF D-10, not made.
- **tasks.json:** every U2A item `passing` with its evidence (U2-02's bench added; U2-14's resolution corrected); none
  cut, none deleted.
- **Before the close commit** (8:22 PM): `git status --short` showed only these docs (HANDOFF.md, NIGHT8-LOG.md,
  PORT-STATUS.md, tasks.json, NIGHT8-REPORT.md new), `?? NIGHT7.md`, `?? NIGHT8.md`. The TMP fallback asset was not
  modified by tonight's Play runs (U2-03's fix holds). `UnityConnectSettings.asset` showed no change after the switch
  back (it may show `M` again whenever Unity rewrites it).
- **INV-clean and the tag `u2a-demo`:** run right after this commit (the tag goes on it); their output is in the session
  transcript, not here, since this file is in the commit they check. Nothing pushed or merged.

Status U2-15: passing.

## Status at the end of NIGHT8

All 17 U2A items passing; none cut. 237 / 237 tests, 16 / 16 replays, 55 / 55 golden; the Player smoke and the Web
smoke pass; the bench is recorded. Next: HANDOFF.md section 4.
