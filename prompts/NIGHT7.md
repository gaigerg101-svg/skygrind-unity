# NIGHT7.md: SKYGRIND in Unity, stage 1 (the vertical slice), no human in the loop, ~8 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Unity Editor is open on this project. Nobody answers questions and nobody playtests.
This file is the whole brief.

**GG's decision (2026-10-02):** SKYGRIND is built in Unity from now on. The web game (`C:\Users\gaige\dev\skygrind`, tag
`p17-night6`) is **frozen**. It is the reference you copy, and it gets nothing new. The port goes in playable stages:
1. the vertical slice (tonight)
2. the Survival loop
3. the arsenal and the enemy roster
4. the bosses and the NIGHT6 systems
5. menus, modes and feel

**Goal tonight:** in the morning GG launches `Builds/Stage1/SKYGRIND.exe` and plays a real SKYGRIND Survival run in Unity:

- Title, then PLAY. He spawns in LANDING with the KESTREL and the PULSE GAUNTLET and 500 points.
- Husks come in rounds, from out of view, and chase him across the map.
- He shoots, gets headshots, punches and parries, scores points, and buys gates to open zones.
- He gets hit, regenerates, dies, sees the DOWN screen, and restarts.
- The movement is exactly the web's.

**GG's standing order: build, don't test.** About 15% of the night goes to checks (section 4).

## 0. Hard rules (break none)

1. **Repo and branch:**
   - Work only in the Unity repo. Branch `port/stage1` from `spike/unity-feasibility` (b454e41). Never `master`, never
     push, never merge, never `gh auth switch`.
   - Commit with explicit paths and their `.meta` files (never `-A` or `.`).
   - Two files are modified at start: `ProjectSettings/ProjectSettings.asset` and `UnityConnectSettings.asset`. Never
     stage either one. Log any further change Unity makes to ProjectSettings.
   - Leave `NIGHT7.md` untracked. Log to `NIGHT7-LOG.md` with times from `date` only.
2. **The web game is read only:**
   - It stays on tag `p17-night6`, branch `night6`. No commits there and no branch switch.
   - Never edit its `port-export/` or its untracked files.
   - The one write allowed there is a new untracked folder `port-export-p17/` (S1).
3. **Movement stays exact:**
   - The only change to `Mover.cs` tonight is NIGHT5's ceiling corner correction (S2).
   - At every commit that touches `Skygrind.Core`, the golden vectors show 0 mismatches and all 16 trajectory replays are
     bit-exact on the C# web collider.
4. **The sim is the web's:**
   - One fixed 120 Hz loop runs in `Update` with an accumulator: ArenaBootstrap's loop, the web's `frame()`.
   - Timers are whole steps (`stepsFor`), with render interpolation and hitstop.
   - Systems run in the order of the web's `simulate()` (main.js).
   - No Rigidbody, no CharacterController, no PhysX for gameplay. The player, the enemies and every ray use the C# web
     collider (`TriangleWorld`, `CoreGrid` and box layers).
5. **Every gameplay number comes from `PortExport/config.json`,** through a typed config read with `JsJson`. Never type
   one by hand; PORT-INVENTORY's tables are from `a001407`, and NIGHT4-6 changed some values. Unity-only numbers
   (render, UI layout, camera stack) go in one `UnityTuning` file, each with a comment.
6. **Randomness:**
   - Gameplay randomness comes from the ported `mulberry32` (golden-checked), seeded per run.
   - Never `UnityEngine.Random` or `System.Random` in sim code.
   - Anything per run resets at run start and on respawn.
7. **What GG owns:**
   - Sound and music: no audio code, no AudioSource, no sound files.
   - Enemy visuals: the Husk is the web's existing blocky model rebuilt box for box in its colors, with nothing new.
   - Gameplay events keep the web's names (`src/core/events.js` payloads), so GG's AUDIO-HOOKS map carries over.
8. **Look:**
   - A dusk palette. Cyan is player-usable; red-orange is enemies and danger only; magenta is RIVAL only; pale
     violet-white is the boundary.
   - UI text is at least 16 px at 1080p.
   - Fonts: the web ships only woff2. Unity can't import woff2, and it can't be converted offline here (fontTools is
     installed, brotli is not). Use TextMeshPro's default font tonight and log it. The theme fonts come later.
9. **Driving the Editor:**
   - Drive the open Editor with `unity command <name> --caller plugin --skill unity-cli`.
   - Load the `unity:unity-cli` skill first, and `unity:ui` before any UI code.
   - Never hand-edit `.unity` or `.prefab` YAML while the Editor is live. Scenes are built by an editor script through
     `eval`, so they can be rebuilt.
10. **No questions, no waiting.** Pick the safe default, log why, and continue. Write a status line in the log at the end
    of every phase, because GG will ask "is it done?" and the log answers.
11. **Deleting:** delete nothing that isn't tonight's own temporary output (screenshots you moved, scratch files).

## 1. Where things stand (read these before building; all of them exist)

- **`SPIKE-RESULTS.md`:**
  - The movement port: `MovementMath`, `SwingMath` and `Mover`, with 134 EditMode tests.
  - The golden vectors: 379 calls, 0 mismatches.
  - 16 trajectory replays, bit-exact on the C# port of three's Octree (`TriangleWorld`).
  - The Arena scene: `ArenaBootstrap` (the 120 Hz loop), `InputReader`, `CameraFeel` (FOV, roll, dips, kicks),
    `CoreView` (the Grinder) and `ArenaBuilder` (map meshes).
  - An enemy crowd (`EnemyCrowd.cs`): husk-like chase, the aggregated capsule push-out, a BFS flow field.
  - Task 6 is the workflow report; its traps are listed in section 5.
- **The web's `PORT-INVENTORY.md`** is the system map: lines per system, the collision API, AI, content, rendering, risks.
- **`PortExport/`** holds the web's own outputs from commit `a001407`: config, 13 golden-vector files, 16 trajectories,
  and 3 maps.
  - `maps/survival.json` has the collision triangles, boxes, wedges, barriers, the zone parts (6 zones, start
    `landing`), 28 ground spawns, 8 pads, 17 grapple nodes, and the **whole nav grid**: 11,232 nodes and walk, jump, drop
    and gap links, with the core `flat` (Survival's core).
  - **It is 60 commits behind the frozen web game.** S1 refreshes it.
- **Missing from the Unity Mover:** NIGHT5 F6's ceiling corner correction (web `docs/MOVEMENT-CHANGES.md`). The 16
  trajectories never rise into a ceiling edge, so the replays can't see it.
- **The Editor exposes:**
  - `eval`, `eval_file`, `recompile`, `recompile_status`, `codereload_status` and `run_tests`
  - `editor_play`, `editor_stop`, `editor_status` and `editor_focus`
  - `console`, `capture_game_view` and `screenshot`
  - `simulate_key` and `simulate_pointer`
  - `get_performance_stats`, `build` and `build_status`
  - `create_scene`, `save_scene`, `add_scene_to_build` and `package_add`
  - UI Toolkit and uGUI 2.0, which includes TextMeshPro, are installed.

## 2. Definition of done (what GG plays in the morning)

1. Launching the built Player (or opening `Assets/Scenes/Survival.unity` and pressing Play) shows a title with PLAY and
   QUIT, the cursor free. PLAY starts a run: cursor locked, LANDING, the KESTREL in hand and the PULSE GAUNTLET on its
   slot, points from config (500), HP 100. Esc pauses (RESUME, QUIT TO TITLE).
2. **Survival rounds as the web runs them (husks only tonight):**
   - The round budget is `6 + 3N`, with at most 24 alive. Squads come on the director's cadence, spawned 15-40 m away,
     out of view, in open zones only.
   - Round clear pays `100 x N`. Then a 10 s break, then the next round.
   - Rounds that bring other kinds or bosses on the web spawn husks tonight (logged).
3. **The Husk:** chases over the nav grid's flow field, including drops, jumps and gaps. It melees with its tell, leaps
   at a player above it, and separates from the crowd. The watchdog relocates a stuck husk.
4. **The KESTREL:** hitscan on the collider's rays and the enemies' hit shapes, headshots, the fire rate, ADS zoom, the
   recoil springs and camera punch, a 2-frame muzzle flash, a hit marker. **The PULSE GAUNTLET** melee, and **punch /
   parry** on F. Kill hitstop.
5. **Points** for hits, kills and headshot kills (`pointsMath`).
6. **Zones:** every gate is solid until it's bought. Hold E near a gate's sign to buy it (the buy rule from config). It
   opens and the director spawns there too.
7. **Health:** husk hits hurt, regen follows its delay and rate. At 0 HP, the DOWN screen shows the round reached,
   points and kills, with RESTART and TITLE.
8. **HUD:** crosshair, hit marker, HP, points, round, enemies left, the break countdown, the buy prompt with its price.
9. **Movement:** the web's, exactly, plus the ceiling correction. The Grinder sits at `flat`. Pads work. The ARCLINE
   isn't owned at start, as on the web.
10. **Performance:** the built Player holds at least 60 fps at 1080p with 24 husks alive (one measurement, with the
    power plan recorded).

## 3. Architecture (decided: don't re-open it)

- **Assemblies:**
  - `Skygrind.Core` is pure C# with no UnityEngine. It holds the sim and every rule, tested in EditMode.
  - `Skygrind.Game` holds the MonoBehaviours: the loop driver, input, views, UI.
  - When a web module is pure, its port goes in Core.
- **New Core modules**, each after its web file:
  - `GameConfig` and `EventBus` (events.js)
  - `Health` and `Regen` (health.js, regen.js, regenMath.js)
  - `Economy` (economy.js, pointsMath.js)
  - `CombatMath`, `WeaponMath` and `GunMath`
  - `Weapons`: the Kestrel, the gauntlet, and the punch and parry (weapons.js, melee.js, punch.js)
  - `NavGrid` (loaded from the export), `FlowField`, `Crowd`, `Tokens` and `Watchdog`
  - `EnemyKind` and `Husk`
  - `DirectorMath` and `Director`
  - `Zones` (zoneMath.js, buyables.js)
  - `SurvivalRun` (modes/survival.js)
- **One driver:** `SimLoop` (Game) owns the step and calls the systems in `simulate()` order.
- **Data at runtime:**
  - The Player reads `Assets/StreamingAssets/PortExport/` (config.json, maps/survival.json).
  - A menu item, `SKYGRIND/Sync Port Data`, copies them there from `PortExport/`.
  - Tests keep reading `PortExport/`.
- **The map** is built at load from `survival.json`:
  - render meshes from the boxes, wedges and strips (ArenaBuilder's approach)
  - collision from the Octree triangles
  - the Grinder through `CoreGrid` at `flat`
  - the gates as a live box layer that blocks bodies and rays
  - the barriers as the boundary
  - the pads, and the grapple nodes drawn
- **One scene:** `Assets/Scenes/Survival.unity` holds a bootstrap and the UI states (title, playing, paused, down).
  The Arena scene stays as it is.
- **The viewmodel:**
  - An overlay camera in the URP camera stack, with the viewmodel FOV from config.
  - The blocky Kestrel and gauntlet, rebuilt from `src/assets/weaponModels.js` parts.
  - Recoil and punch use the closed-form springs (`src/fx/springs.js`); `KickStack.cs` already holds the kicks.
- **The look** (basic tonight): URP, a dusk gradient sky, linear fog from config, gradient ambient, a warm key light
  with shadows, a violet rim light, ArenaBuilder's grid stone. The web's world shader is stage 5.

## 4. Testing budget (build, don't test)

- **Per feature (about 3 min):**
  - One `run_tests` (the whole EditMode suite is about 21 s).
  - Golden vectors for every pure module that has a file in `PortExport/vectors/`: combatMath, gunMath, weaponMath,
    directorMath, spawnMath, styleCore / styleMath / styleSim, momentumMath, arsenalMath, assistMath.
  - At most one Play-mode check (an `eval`, or one screenshot if it's visual).
- **One tool only:** a slice smoke script (`Assets/Editor/SliceSmoke.cs`, run by `eval` in Play mode). It prints one
  line per step and stops itself after 10 minutes. Write it in S8; run it in Q.
- **Never** a Player build before S9. **Never** two Play-mode sessions or two builds at once.

## 5. Mistakes not to repeat (Unity spike, NIGHT5, NIGHT6)

1. **Recompiling during Play mode** reloaded the domain under the running scene: 27,000 errors in a minute. Always run
   `editor_status` first; stop Play mode before every `recompile`.
2. **Play mode with the Editor unfocused froze every command for about 40 min**, while `unity status` still said "ready".
   - At P0, check that Run In Background is on (`get_player_settings`).
   - If two commands in a row time out in Play mode, run `editor_stop` and then `editor_focus`.
3. **`recompile_status` reported "completed" before the new DLLs were written.** Check the timestamps of
   `Library/ScriptAssemblies/Skygrind.*.dll`, and that the tests changed.
4. **The first command after a domain reload often times out** (30-60 s reloads, and the Pipeline port moves between
   7800 and 7801). Retry once after 10 s before deciding anything is broken.
5. **`unity test` starts a second Editor** that can't open the project. Always use `unity command run_tests`.
6. **`capture_game_view --save_path` is relative to `Assets/`.** Move every capture out to `Logs/shots/` right away and
   delete its `.meta`. Never commit a screenshot under `Assets/`.
7. **A Player build rewrote four settings files,** and one of them turned on Unity cloud services
   (`UnityConnectSettings.asset`, `m_Enabled` 0 to 1). After every build, run `git diff --stat ProjectSettings` and
   commit only changes you understand (URP, build list). Never commit UnityConnectSettings.
8. **Git Bash rewrites `/switch` arguments as paths** (the spike's `powercfg /getactivescheme`). Use `-switch` forms or
   PowerShell.
9. **NIGHT6:**
   - Two docs went in through quoted heredocs. Write every doc, report and log only with the Write / Edit tools.
   - Patch scripts go in written `.cjs` / `.cs` files, never `node -e`.
   - A commit message claimed more than was shown. Write only what was measured or read; otherwise say "not checked".
10. **NIGHT5: stopping a task left its processes alive.** After any check, close every Player you launched
    (`SKYGRIND.exe`) by PID: `Get-CimInstance Win32_Process` with a CommandLine match. **Never kill the Editor**
    (`Unity.exe` with this project).
11. **NIGHT6 finished its 8 h brief in 1.5 h, with thin features.** Tonight, when a phase lands early, spend its leftover
    budget making it closer to the web's behavior (each phase lists what "closer" means) before moving on. If every phase
    is done, take the if-time list (section 6, after S9), then stage 2's first items: wall buys, perks, the style meter.
12. **NIGHT5: tools before building ate half a night.** The smoke script is the only new tool.

Keep doing what worked in the spike:
- Golden vectors and replays after every Core port: they caught every port bug.
- `eval` for one-off setup instead of menus and dialogs.
- Port line by line, from the web file open beside you, with the web's comments kept.
- The Mono vs V8 math helpers (`JsMath`, `JsJson`) wherever exactness matters.

## 6. Phases (in order; budgets include their own checks)

### P0 Start (15 min)

- Load the `unity:unity-cli` skill. Run `unity status` and `editor_status`. Not playing: good. Playing: it isn't yours,
  so log it, stop it, and continue.
- Check Run In Background, and record the power plan (`powercfg -getactivescheme`) and whether you're on AC.
- `git switch -c port/stage1 spike/unity-feasibility`.
- `run_tests`: record the count (134 expected) and the 16 replays.
- Log one line.

### S1 Refresh the data from the frozen web game (30 min)

- Copy `skygrind/port-export/scripts/` to `skygrind/port-export-p17/scripts/`. `lib.mjs` writes next to its own folder,
  so the original `port-export/` is never touched. Then run `node port-export-p17/scripts/all.mjs` and `verify.mjs` in
  the web repo. verify must show 1267 vectors, or whatever it reports, and 16 / 16.
- Copy the outputs into this repo's `PortExport/`. Update its README's snapshot line to `p17-night6`.
- `run_tests`:
  - Golden vectors 0 mismatches and 16 / 16 replays.
  - The `MovementConfig` vs `config.json` test will flag the new `collision` keys (`cornerCorrect`, `cornerSkin`): add them.
- Any other mismatch is a real difference between `a001407` and `p17-night6`. Find it in the web's git log, port it, and
  log it.
- Commit.

### S2 The ceiling corner correction (25 min)

- Port `Mover._resolveContact`'s NIGHT5 F6 branch (web `src/movement/mover.js`, `docs/MOVEMENT-CHANGES.md`) line for
  line.
- Port its `tests/movementSim.test.js` cases:
  - At 0.02, 0.05 and 0.08 m of overlap, the apex equals an open jump and the drift stays under overlap + 2 cm.
  - At 0.15, 0.3 and 0.6 m it still blocks.
  - Away from the ceiling, nothing changes.
- 16 / 16 replays still bit-exact. Commit.

### S3 The Survival map and the look (60 min)

- Build `Survival.unity` with an editor builder run through `eval`. At load, build the map from `survival.json` (section
  3): collision, render, gates, barriers, core, pads, nodes, kill plane, the spawn at `playerSpawn`.
- The player mover runs on the C# web collider, with the camera feel from Arena.
- The basic dusk look.
- Check: one screenshot from the spawn, and one walk from LANDING to a gate (it blocks).
- **Closer:** the zone signs as world text (name and price), and the barrier's force-field tint.
- Commit.

### S4 The loop, events, health and the run (45 min)

- `SimLoop` (the accumulator, the step order, hitstop that freezes and skips sim time), `EventBus`, `GameConfig`.
- `Health` and `Regen` (port their tests), and `SurvivalRun`'s states: title, playing, break, down.
- The run seed (mulberry32). Restart rebuilds the run state, not the scene.
- Check: `run_tests` and one Play-mode `eval` (damage, regen, death, restart).
- Commit.

### S5 The KESTREL, the PULSE GAUNTLET and punch / parry (60 min)

- Port `combatMath`, `weaponMath` and `gunMath` with their golden vectors.
- The Kestrel's hitscan through the collider ray, plus the enemy hit shapes (head and body, as husk.js defines them).
  The fire rate, headshot multiplier, range and ADS zoom (ads.js) all come from config.
- The gauntlet melee sweep and the punch / parry: parrying a husk swing in its window heals and pays, as on the web.
- The viewmodel (section 3): recoil and punch springs, a 2-frame muzzle flash, a hit marker, the kill confirm.
- Input bindings from `config.input.bindings`: fire, ADS, melee / slot, F punch, E buy, Esc pause, 1 / 2 weapon slots.
- Check: golden vectors, plus one eval that shoots a dummy capsule (body and head damage).
- **Closer:** the Kestrel's special and its momentum variant (web `weapons.specials`).
- Commit.

### S6 The Husk (80 min)

- `NavGrid`, read from `survival.json`'s navGrid (decode `tagAt` for the zone mask, so locked zones are masked as on the
  web). `FlowField` is a port of `src/ai/flowField.js`: two fields (all links, and walk + drop), recomputed when the
  player's node changes, with the seeds and the airborne-goal rule.
- `Husk` and `EnemyKind`, after `src/enemies/husk.js` and `enemyKind.js`:
  - the chase (straight inside 12 m, otherwise the flow-field carrot) and the aggregated capsule collision with the
    edge and support rules (`EnemyCrowd.cs` has a start)
  - jump and gap links through `navJump.js`'s arcs
  - the melee tell and hit, the leap at a player above
  - `Crowd` separation, `Tokens` (the melee pool), `Watchdog`
  - its HP from config, a hit flash and the web's death look
- The model: the web's blocky husk, box for box.
- Check: one eval with 6 husks chasing the player across a drop and a jump link; screenshot the fight.
- **Closer:** the brain's steering blend (Arrive / Separation / Wander gains from `brain.js`; Yuka isn't available, so
  port the few behaviors used), LOS on the 3-rays-per-step budget, the staggered 12 Hz think.
- Commit.

### S7 The director, points and gates (60 min)

- Port `directorMath` with its golden vectors, and `director.js`:
  - the budget, the alive cap and the squad cadence
  - spawns 15-40 m away and out of view (55 degrees plus LOS), at least one squad from behind, open zones only
  - intensity, PEAK and RELAX, and camping (EXPOSED halves points)
- `Economy` with `pointsMath`: hit, kill, headshot kill, punch kill, parry, round clear.
- `Zones` with `zoneMath` and `buyables`: hold E for the buy time within the buy range, the price from config. Opening a
  gate removes its box layer, unmasks the zone in the nav grid and adds its spawns.
- Check: one eval that buys ARMORY and sees a spawn in it.
- **Closer:** the round banner ("ROUND N"), and the break's full heal and stamina refill (as the web's break does).
- Commit.

### S8 The HUD, title, pause and DOWN, plus the smoke script (40 min)

- Load the `unity:ui` skill first; default to UI Toolkit. Build the HUD items, title, pause and DOWN screens from
  section 2, at least 16 px at 1080p, in the theme's colors.
- Write `SliceSmoke.cs` (section 4).
- Check: screenshots of the HUD in a fight and of the DOWN screen.
- Commit.

### S9 The Player build (15 min)

- Add `Survival.unity` to the build list first, `Arena.unity` second. Run Sync Port Data. Build a Windows x64 Player to
  `Builds/Stage1/` (`build`, then `build_status`). Keep `Builds/` out of git, as now.
- Launch it once: title, PLAY, 30 s of round 1. Close it by PID.
- Check the ProjectSettings diff (rule 7 of section 5). Commit the build settings.

**If time** (in this order, one commit each):
1. The style meter: `styleCore`, `styleMath` and `styleSim` are pure with golden vectors; add its HUD rank.
2. The THUNDERCLAP wall buy in ARMORY (shotgun cone from combatMath).
3. Swarmers from round 3, as the web introduces them.
4. Aim assist off/on as the web's default (assistMath has golden vectors).

### Q Close (45 min)

1. **Q1:** `run_tests`. Record the count, golden 0 mismatches, 16 / 16 replays.
2. **Q2:** the slice smoke once, in Play mode. It scripts a run where an aim bot clears round 1, waits out the break,
   sees round 2 start, buys a gate, dies, sees DOWN and restarts. Record every line. Fix what fails if it's tonight's;
   log what isn't.
3. **Q3:** one performance number in the built Player: 24 husks, 1080p, 60 s. Record median and p95 frame time, with
   the power plan and AC polled during the run (the plan silently flips on this laptop). Close the Player by PID.
4. **Q4:** write the docs with Write / Edit:
   - **`PORT-STATUS.md`:** every web system from PORT-INVENTORY section 1, marked ported, partial or not started, with
     its Unity files. Also every event the port emits, by its web name, for GG's audio map.
   - **`U1-REPORT.md`:**
     - what shipped, with commits
     - the numbers: tests, replays, fps
     - what was cut, and the differences from the web, each with its reason
     - the open risks
     - **how to play:** the exe path, the scene path, the keys
     - a 10-minute playtest checklist with one question per new feel point: the Kestrel's punch, the husk's chase, the
       gate buy, the HUD
   - The final lines of `NIGHT7-LOG.md`.
5. Tag `u1-slice` locally. `git status` shows only the two ProjectSettings files from the start and the untracked
   `NIGHT7.md`. Log anything else Unity wrote. Nothing pushed, nothing merged.

## 7. Cut order and done list

Cut from the bottom:
1. the if-time list
2. each phase's "closer"
3. gate buying (gates stay solid; F8 opens all, as a debug key)
4. the husk's leap and its jump and gap links (it uses the walk + drop field only)
5. punch / parry
6. the gauntlet

Never cut S1, S2, S3, S4, the Kestrel, the Husk's chase and attack, the rounds, the HUD, S9 or Q. A phase 25% over budget:
stop, ship what works behind a `UnityTuning` switch, log it, move on.

Done list:
- Branch `port/stage1`, one commit per phase, `run_tests` green, golden 0 mismatches, 16 / 16 replays bit-exact.
- The Player build at `Builds/Stage1/SKYGRIND.exe` plays section 2's run.
- The slice smoke run once, with its result written down.
- Tag `u1-slice`; NIGHT7-LOG.md, U1-REPORT.md and PORT-STATUS.md written.
- The web repo unchanged except the untracked `port-export-p17/`. Nothing pushed or merged. No audio, no new enemy
  design. Movement exact.
