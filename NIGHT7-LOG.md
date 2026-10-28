# NIGHT7 log (Unity stage 1: the vertical slice)

Times from `date`. Brief: `NIGHT7.md`. Branch `port/stage1` cut from `spike/unity-feasibility` (b454e41). GG's order when
starting it (2026-10-02 8:30 AM): build only on Unity, now and in the future. The web game is the frozen reference.

## P0
- 8:34 AM: the Editor was not running (the session "ready" earlier had closed; only the Hub's `unity.exe serve`). Opened
  the project with `unity open`; its Pipeline server was ready 5 s after the wait started (port 7800, playMode stopped).
- 8:37 AM: branch `port/stage1`. Power plan "Balanced", on AC. The two ProjectSettings files modified at start
  (ProjectSettings.asset: preloadedAssets emptied; UnityConnectSettings.asset) are left unstaged, as the brief says.
- 8:39 AM: `run_tests` EditMode **134 / 134** in 28 s (golden vectors, 16 trajectory replays, collision parity).
- Run In Background: `runInBackground: 1` in ProjectSettings.asset (working copy and HEAD).
- Workflow: a scratchpad helper (not in the repo) wraps the spike's traps. `compile` stops Play mode first, polls
  `recompile_status`, then checks the Skygrind DLL timestamps and prints console errors. `tests` runs `run_tests`
  in the live Editor and prints the summary.

## S1 data refresh (8:40-8:43 AM, c6bfbab)
- Copied the export scripts to `skygrind/port-export-p17/scripts/` and ran `all.mjs` there (12.6 s). verify: 1267 vector
  calls exact, 0 mismatched, 6 skipped (injected callbacks), and all 16 trajectories exact.
- Diff against Unity's `a001407` copy: every vector file and every trajectory is byte-identical. What changed:
  - `config.json` and `config.notes.md`
  - `maps/survival.json`: boxes (NIGHT5's LANDING covers), forgeSpot, holdSpots, relaySpots and secretRelay; the nav
    grid went from 11,232 to 11,228 nodes
  - `maps/training.json`: its sign labels
- `spire.json` is identical. run_tests 134 / 134.

## S2 ceiling corner correction (8:43-8:47 AM, a1bbfa1)
- `Mover.ResolveContact`: the NIGHT5 F6 branch, ported line for line; `JsMath.Hypot` matches the web's Math.hypot.
  Added `collision.cornerCorrect` and `cornerSkin` to MovementConfig; PortData checks both against config.json.
- Ported the three web tests (CeilingCornerTests, on TriangleWorld boxes instead of BoxGeometry + Octree; same 12
  triangles per box): 137 / 137, 16 / 16 replays bit-exact.

## S3 the Survival map (8:47-9:09 AM, 114fcee)
- Core:
  - `GameConfig`: the whole config.json by dotted path; the typed MovementConfig for the Mover.
  - `GameWorld`: the Octree, then the core, then live `BoxLayer`s in the web collider's order; `Geometry` (no layers)
    is the nav grid's view.
  - `ZoneMath` (ZoneIndexAt, GateQuads, SpotBuyable) and `Zones`: locks, the gate layer, spawn nodes sampled as the
    web does (rays + a husk capsule), sleeping pads, buy spots with config.zones prices.
  - `SurvivalMap` (the export), `EventBus` (the web's event names).
  - Added `CoreGrid.TopAt`.
- Game:
  - `SurvivalGame`: the bootstrap and the 120 Hz loop (ArenaBootstrap's accumulator, hitstop skips steps).
  - `SurvivalBuilder`: stone with colors.pillarTop on tops and pillarSide on sides; neon strips, rims, nodes and pad
    rings; the boundary; `CoreMesh`; `ZonesView`.
  - `Look`: the sky shader, linear fog, trilight ambient, key light with soft shadows, the rim as a shader global, URP
    bloom + ACES + vignette. Web light intensities are divided by pi (three's Lambert), in `UnityTuning`.
- Shaders: `SKYGRIND/Stone` (vertex color, world grid, main-light shadows, rim, fog), `SKYGRIND/Sky`, `SKYGRIND/Field`.
- `Editor/SurvivalSetup`: material assets (so the shaders ship), the scene, and the build list (Survival first, Arena
  second). `SKYGRIND/Sync Port Data` copies config.json and survival.json to StreamingAssets for the Player.
- Check: a Play-mode screenshot from the spawn (Logs/shots/s3-spawn3.png). The first pass drew the gate fields as a
  full striped sheet over the view; they are now thin hazard lines over a faint sheet.
- 4 new tests: the ARMORY gate stops the walk at x = 14 - r; after `Open` it passes x = 16; spawn nodes; sleeping pads;
  gate quads. 141 / 141.
- `capture_game_view` refuses `..` in save_path. Captures go to `Assets/NightShots/`, are moved to `Logs/shots/`, and
  the folder and its .meta are removed.
- Slip: one inline `node -e` edit with backticks (MeshKit winding). The result was checked and correct. From then on,
  Edit only.
- Cut to later (logged): the zone signs as world text. They need TMP's resources and the `unity:ui` skill, so they
  move to S8.

## S4 the loop, health, the run (9:10-9:27 AM, a16e980)
- Ports:
  - health.js (i-frames from `mover.invulnerable`, the timed shield)
  - regenMath.js + regen.js (delay, fast countdown, camping pause, the rank perk)
  - pointsMath.js + economy.js (hit 10, kill 60, headshot 100, melee 130, parry 50, round clear 100 x N, rank and camp
    multipliers)
  - modes/survival.js (first break 3 s, break 10 s, `READY`)
- `SurvivalGame` hitstop is main.js's: the longest request wins, the frozen time is skipped (`accumulator %= step`), not
  caught up.
- The flow: TITLE, PLAYING (Esc: PAUSED), DOWN (health.downTime), RESULTS, restart. Falling past the kill plane in a
  run is a full-HP blow, as on the web.
- Tests: the web's survivalS1 cases ported (regen x4, points) plus a fake-spawner run loop: 147 / 147.
- Play check through `eval`, all as expected:
  - a hit of 30 left 70 HP; regen filled it to 100 after the 4 s delay
  - death -> DOWN -> RESULTS (round 1, killed by "test")
  - restart: Playing, Break, 100 HP, 500 points
- Slip: one long check command's final `editor_stop` landed late, so my next samples read stale Edit-mode values and
  looked like a stall. The real cause: `eval` takes several seconds while the Editor is unfocused. From then on:
  `editor_focus` before Play checks.

## S5 KESTREL, GAUNTLET, punch / parry (9:28-9:56 AM, d6414e1)
- Core:
  - CombatMath: combatMath.js whole, gunMath sweepTargets with a stable sort like JS, the ads.js zoom helpers,
    springs.js.
  - Combat: an Enemy base and EnemyRegistry, and the Loadout with the swap timeline.
    - the Kestrel: hitscan vs walls and hit shapes, x2 heads, hip spread drawn from the run's mulberry32, ADS (zoom
      1.5, x1.2 heads, walk x0.85, the zoomed ricochet)
    - the gauntlet jab; punch / parry on F (stagger, heal 15, stamina refill); damage() with hit and kill events
  - Husk.cs: husk.js stepHusk, moveHusk, the leap, the tell and the hit shapes. Husks.cs is the pool.
- Mover: added `Impulse` and `RestoreStamina`, the web Mover API the spike left out (rule 3 read as "no behavior change
  against the web Mover"; the 16 replays never call them, and they stay bit-exact).
- Game:
  - Viewmodel: an overlay camera at viewmodel.fovDeg 54 and the kit Kestrel and Gauntlet rebuilt from weaponModels.js
    part lists (BlockyKit: box, tubeZ, profile by ear clipping; no bevels and no holes, so the frame window is solid)
  - feel: recoil springs per weight class, the camera punch pattern, the swap drop, the jab, a 2-frame muzzle flash,
    tracers
  - CombatReader for the inputs; HuskView (the web's look: capsule + hunched head in colors.enemy, unlit like
    MeshBasicMaterial, tell pulse, flash, stagger lean); Particles for hit and death bursts
- Tests:
  - combatMath golden vectors bit-exact (sampleCone 20, ray tests, leap, swap timeline, punch pick, ricochet with the
    recorded LOS rules) and sweepTargets 3 / 3
  - CombatTests: body 25, head 50, zoomed head 60, a wall blocks, gauntlet 32, a melee kill pays 130, parry heals and
    staggers
  - 158 / 158
- Screenshot `s5-combat.png`: the Kestrel in hand, three husks. The first pass drew the husks lit (muddy brown); they
  are now unlit, as on the web.

## S6 the Husk brain (9:57 AM-10:07 AM, f35d0fd)
- Exact ports:
  - NavField.cs: the exported nav grid (11,228 nodes, CSR links, reverse links built as navGrid.js does), flowField.js
    (resumable Dijkstra; float32 distances and costs with JS's compare-then-store), bestLink, gridPath
  - navField.js: route, walkableAt, lineWalkable, path, relocate
- HuskBrain.cs:
  - ported exactly: tokens (with the web's steal margin), watchdog.js, crowd.js, navJump.js, LOS on a 3-ray budget
  - simplified (logged): steering is Arrive blended at steer.gain (no Yuka); non-holders wait on ring 1 (10 m) at their
    own angle with the strafe; no slots, squads or flankers
- Tests:
  - a husk spawned at the north wall walks to LANDING with relocation forbidden
  - the melee token pool caps the simultaneous wind-ups
- Slip: I ran the HuskNavTests rerun with the compile output hidden (`>/dev/null`). `Assert.Warn` doesn't exist in
  Unity's NUnit, so the test assembly failed to compile and the "2 / 2" was the old build. f35d0fd contains that test
  file; S7 (81ef1f1) fixed it. Compile output is never hidden again.

## S7 the director, points, gates (10:07-10:28 AM, 81ef1f1)
- Director.cs:
  - directorMath.js whole and director.js: the seeded deal and pace streams, the opening squads, squadInterval,
    maxAlive, the spawn tiers (15-40 m, behind first, out of view with the LOS ray), intensity and the BUILD_UP / PEAK /
    RELAX phases
  - camping: EXPOSED halves combat points and pauses regen; at 100% a hunter squad comes (4 husks until swarmers and
    chargers exist)
  - spawner.js's telegraph (1 s beam, then the group spread 2.5 m)
  - Buyables: buyables.js, hold E 0.35 s, one buy per press, denied without the points
- **Two web bugs found and fixed in Unity (deviations, kept):**
  - a group offset inside the 2 m south wall passed the web's `navField.reachable` (it takes the nearest node within a
    cell), the 0.4 m capsule fits inside the wall, and the husk fell through it; offsets now need a node in their own
    column
  - nodes on a zone edge (LANDING's x = +-14) overlap the locked neighbour's gate field, so a squad there lost every
    member; spawn picks now skip nodes a husk can't fit at in the live world
- Tests:
  - directorMath golden vectors bit-exact, including all 16 recorded planSquads deals (recorded availability callbacks
    parsed)
  - the buy flow; director round 1 on the real map: 9 spawned plus the 4-husk hunter squad (the test's player camps),
    none fell
  - 165 / 165
- Play check, round 1:
  - two husks attack (the melee tokens) while the rest wait about 9.5 m out on ring 1
  - the pacing went to RELAX after the damage taken
  - player HP fell from the hits

## S8 the HUD, the screens, the slice smoke (10:28-10:44 AM, fcf3973)
- Ui.cs: uGUI + TextMeshPro built at load from config.ui.theme (Canvas Scale With Screen Size from 1920 x 1080, match
  the height, as the web scales its UI). Nothing under 16 px at 1080p. Cyan for player-usable, red-orange for danger.
  - HUD: crosshair + hit marker, HP bar with the low-HP blink, stamina pips, points, the weapon, ROUND N / the break
    countdown / enemies left, the buy prompt with its hold bar and NOT ENOUGH POINTS, the round and zone banners,
    EXPOSED, the hurt vignette
  - screens: TITLE, PAUSED, DOWN, RESULTS (round, points, kills, headshots, accuracy, time, zones opened, killed by)
  - ZoneSigns: the gates' price signs (TextMeshPro in the world)
- TMP Essentials imported with `TMP_PackageResourceImporter.ImportResources()` (non-interactive, no modal).
- SliceSmoke.cs: the brief's one new tool. An aim bot clears round 1, waits out the break, sees round 2 begin, buys
  ARMORY at its gate, dies, sees DOWN and RESULTS, restarts. All steps pass in 20 s.
- Fix: a new run resets max HP (the smoke's god mode carried into the restart).
- **Slip:** I put SliceSmoke in the Editor assembly first. Unity won't add an Editor-assembly MonoBehaviour to a
  scene object at runtime, so it moved to the Game assembly (`Skygrind.Game`). Caught by the first Play run, before
  the commit.
- A runtime write changed the TMP fallback font asset on disk. I restored it with `git checkout` before committing.

## S9 the Player build (10:44-10:54 AM, ea5ca4c)
- Build list: `Survival.unity` first, `Arena.unity` second. Windows x64 Player at `Builds/Stage1/SKYGRIND.exe`, 0 errors.
  `Builds/` stays out of git.
- The PortExport data ships in `StreamingAssets/PortExport` (GameConfig.PortExportDir picks the Editor's repo copy or
  the Player's StreamingAssets).
- The Player has no Pipeline runtime, so checks run from its command line: `-skygrindSmoke` runs the slice smoke
  inside the build (all steps pass); `-skygrindBench N` measures N s of frames with 24 husks (PlayerChecks.cs).
- Fix: Look wrote `urp.shadowDistance` at runtime, which dirtied `Assets/Settings/PC_RPAsset.asset`. The value (60 m)
  is now set on the asset once and nothing writes it at runtime.
- ProjectSettings: only the two files modified at start; neither staged.

## If-time 1: the style meter (10:54-11:09 AM, 726fb0e)
- Style.cs: styleMath.js whole (golden vectors bit-exact), styleCore.js and styleMeter.js:
  - named awards x the movement state x weapon freshness x per-source diminishing
  - hit awards once per shot under the income cap
  - tricks (wall / dash / boost / slide jumps, pads) with an enemy near
  - six ranks with drain and the rank-down delay
- The rank multiplies points, speeds regen and stamina; EXPOSED doubles the drain.
- HUD: rank letter, name, fill and the award feed. Results: style and best rank.
- config.style.ranks are objects (name / letter / colour): parsed as such.

## If-time 2: the THUNDERCLAP wall buy (11:09-11:15 AM, 2054d43)
- WallBuys.cs (wallBuys.js): on ARMORY's east wall, a plate, the turning cyan hologram of its kit model and the sign
  (price or OWNED, its family). Hold E: it takes the next slot and goes in hand. Only the THUNDERCLAP has a port, so
  the rest of the wall stays bare.
- Loadout.FireShotgun (weapons.js fireShotgun): 8 pellets in the cone on the run's mulberry32, one hit per enemy per
  shot, and the airborne shotgun jump.
- BlockyKit.Thunderclap: the kit viewmodel, with recoil and the heavy camera punch.
- 170 / 170.

## If-time 3: swarmers from round 3 (11:15-11:46 AM, 7354bf2)
- Swarmer.cs (swarmer.js on enemyKind.js): a Husk subclass with its own step:
  - idle, then a 0.3 s crouch tell, then the pounce (leapVelocity to 0.6 m short of you, apex 0.9)
  - one bite for 8 within 1.2 m, then recover 0.6 s; cooldown 1.6 s from the crouch
  - no head (body capsule only), never parryable; a stagger only cuts a committed attack (enemyKind.stagger)
- Husks.cs: a second pool (swarmer.maxCount 40), stepped after the husks as enemies.js does.
- HuskBrain.cs, now per kind (ai.kinds): the token pool (`swarm` for swarmers), speed, engage range, wait ring.
  The flank rule is ported as the web has it:
  - a swarmer pounces only outside your front cone (frontConeDeg 70), checked as it crouches (flankNow)
  - a token holder runs for a flank point beside or behind you (slots.js flankPoint)
  - simplified: a waiter's ring angle is pushed out of the cone (the web refuses it the slots there)
- The crowd pass follows enemies.js: husks then swarmers, standing walkers only (the port skipped the `grounded`
  check before), per-kind radius and mass (the lighter swarmer moves more).
- HuskNavJumper: per-kind tell and apex (ai.jumps.swarmer).
- Director.cs: availability = husk + swarmer. Round 3 deals swarmer squads, swarm rounds (8, 13, ...) deal only
  swarmers, and the hunter squad is the web's six swarmers (its charger isn't ported).
- **Two web parities the port had missed, fixed:**
  - the round HP multiplier is rounded and maxHp keeps the kind's (main.js onSpawn: `Math.round(hp * hpMult)`)
  - the brain gets +1 tokens while hunted (main.js getCamping: director.hunted); S7 passed `false`
- SwarmerView (EnemyViews.cs): the three boxes as swarmer.js draws them, the crouch squash and the tell pulse.
- Tests: SwarmerTests (pounce and one bite, cooldown, the front-cone gate, the token gate, no head, rounds 1-2 deal
  no swarmers, round 3 does, swarm rounds only swarmers). DirectorRoundTests now expects the six-swarmer hunter
  squad. 174 / 174.
- Play check: six swarmers spawned 5 m in front. They ran to your sides and back, bit from there and never
  pounced in the front cone. Smoke: all steps pass.
- **Slip:** one code patch script was written through a bash heredoc. Its quoting broke and nothing was written;
  the rest went through Write-tool .cjs files (the rule from my notes).

## If-time 4: aim assist at the web's default (11:46 AM-12:40 PM)
- AssistMath.cs: assistMath.js whole. Golden vectors bit-exact: every recorded call plus the 60-row springStep
  sequence (.NET's Math.Acos / Exp / Atan2 match V8 on all of them).
- AimAssist.cs: aimAssist.js for the player's shots.
  - the scan every 2 steps on a 3-ray budget, the acquire time, and the pre-lock (aimLock)
  - the critically damped spring and the follow
  - SNAP's stick under 1.5 deg
  - the fire gate (a press waits at most 40 ms while the spring closes, x2 boost meanwhile)
  - the breakaway flick (blocked 0.4 s), release on death / 0.25 s without sight / 1.6x the cone
  - the 30% / 120 ms hysteresis, ASSIST's friction (input sensitivity x LookScale), and the payout
- Not ported (no such things yet): the V key's manual lock, projectile lead and homing, the melee lunge, boss weak
  points, perks, the gamepad's own mode.
- Default: settings.aimAssist = SNAP at strength 60, on. AIM ASSIST: SNAP / OFF toggles on the title and pause
  screens (PlayerPrefs `skygrind.aimAssist`, UnityTuning.AimAssistPref).
- Assisted hits carry `assisted` and `assistPay` (lockPayout(60) = 0.79) into the points (economy.js:
  base x assist, then x fresh x boost) and style (styleCore: state x assist).
- HUD: the lock brackets (main.js updateLockBrackets + hud.js setLock: four white corners with a danger glow,
  tightening from 1.6x over 60 ms, SNAP's pulse); the crosshair's ticks close in 2 px and turn white while locked,
  and flash white on a new lock.
- Tests: AssistGoldenTests (4) and AimAssistTests: pre-lock and settle, OFF never moves the look, a flick breaks and
  blocks, the fire gate's hold and the assisted payout. 182 / 182.
- Play checks:
  - a lone husk 4 deg off at 12 m: locked on its head and settled at 0.00 deg error, brackets around it
  - assist OFF: the look never moved over 4.5 s; ON: the lock followed a walking husk
  - a husk that walks up to a camping player drags the look down with it, as the web's stick does
- From about 12:10 PM GG was using this machine (input seconds apart). Each Play check calls `editor_focus`. The
  Editor's main thread stalled whenever it lost focus (5 s eval timeouts), so the checks took minutes each.
- 8f82418.

## Q close (12:40 PM-1:06 PM)
- **Q1** (12:42-12:44 PM): `run_tests` EditMode in the live Editor:
  - **182 / 182**
  - golden: 55 / 55 tests, 0 mismatches
  - replays: **16 / 16** trajectories bit-exact on the C# web collider
  - the Unity-physics parity and replay reports still pass as reports (PhysX is not exact; the game doesn't use it)
- **Q2** (12:40 PM, Editor, aim assist on): the slice smoke, every line PASS:
  - a run starts: Playing, Break, round 1
  - round 1 cleared in 8 s: 9 kills, 9 headshots, points 1359 (1570 before aim assist: assisted hits pay 0.79)
  - round 2 begins after the break (10.2 s)
  - ARMORY bought: points 1359 -> 609
  - death: Down
  - results: round 2, kills 9, zones ARMORY
  - restart: Playing, round 1, hp 100, points 500, ARMORY locked again
  - the assist during it: 10 locks, 9 assisted shots, 5 held presses
- The Player rebuilt at 12:47 PM, 0 errors. `-skygrindSmoke` in the Player (12:51 PM): every step passes; it quit by itself.
- **Q3** (12:52-12:53 PM), `SKYGRIND.exe -skygrindBench 60`, 1920 x 1080 windowed, vsync off, PID 129180 (it quit by
  itself after 76 s):
  - **median 9.35 ms (107 fps), p95 18.47 ms**, worst 1% 40.25 ms, 5,681 frames
  - 24 husks placed, plus round 1's opening squads: 31 alive at the end
  - polled every 5 s: power plan **Balanced** the whole run (no flip), **on AC**
  - system CPU went from 20% to 100% for the second half; the machine was in use, so this is a loaded number
- 7ec106e: the crosshair in the web's colour (`ui.crosshairColors[settings.crosshairColor]`, cyan); S8 had drawn it in
  the theme's text colour. The title's controls line lists 1-3 and Q.
- Final Player build at 1:02 PM, 0 errors. `-skygrindSmoke` on it (1:04 PM): every step passes, 0 exceptions, quit by itself.
- Unity wrote `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` during Play
  (dynamic glyphs, a re-serialization) twice more. Restored with `git checkout` both times, never committed.
- **Q4:** PORT-STATUS.md (every PORT-INVENTORY system, ported / partial / not started, with its files; every emitted
  event by its web name, port-only ones marked) and U1-REPORT.md, both written with Write / Edit.
- Captures are in `Logs/shots` (if3-*, if4-*, q-*), and the Player logs are in `Logs/player`.

## Final lines
- Done list:
  - branch `port/stage1`, one commit per phase
  - `run_tests` green (182 / 182), golden 0 mismatches, 16 / 16 replays bit-exact
  - the Player plays the slice: the smoke passes in the build
  - the smoke's result is written above
  - the docs are written; tag `u1-slice` set locally
  - nothing pushed, nothing merged
  - no audio code, no new enemy design (the swarmer is the web's model as it is)
  - movement exact (the only Mover changes: the ceiling correction, `Impulse`, `RestoreStamina`)
- ProjectSettings:
  - `UnityConnectSettings.asset` is still modified from the start and unstaged
  - `ProjectSettings.asset` is back to HEAD's contents: a Player build re-wrote its preloadedAssets
  - no commit touched either file
- The web repo: on `night6` at `p17-night6` (1c73d68), no commits.
  - `port-export-p17/` is tonight's only write there
  - `vite.config.js` was already modified (on 2026-09-30, 14:32), before this night; `port-export/` is the spike's
    untracked folder
- Slips:
  - S6's hidden compile output (fixed in S7)
  - the Editor-assembly smoke (S8)
  - one heredoc-written code patch (if-time 3)
  - a placeholder commit hash in the first U1-REPORT draft, corrected before the docs commit
- Open for GG:
  - the steering simplification and its feel
  - the worst-1% frame spikes (profile on a quiet machine)
  - the TMP fallback asset rewrites
  - UnityConnectSettings.asset
  - SNAP's stick on a husk at your feet
  - the 10-minute playtest in U1-REPORT.md
