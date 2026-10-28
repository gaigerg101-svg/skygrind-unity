# U1 report: SKYGRIND in Unity, stage 1 (the vertical slice)

Branch `port/stage1`, tag `u1-slice` (local; nothing pushed or merged). Brief: `NIGHT7.md`. Log: `NIGHT7-LOG.md`.
What's ported, system by system, and the event list for the audio map: `PORT-STATUS.md`. The web game is frozen at
`p17-night6` and is the reference. From here on Unity is the only build.

## What shipped

A playable Survival run in Unity, in the Editor and as a Windows Player. Title, then round 1, then the break, then round
2 and on, with gate buys, a wall gun, swarmers from round 3, then death, DOWN and RESULTS, then restart. Movement is the
web's, bit for bit. Every gameplay number comes from the web's exported config.

| Commit | Phase | What |
|---|---|---|
| c6bfbab | S1 | PortExport refreshed from the frozen web game (config, survival map, nav grid) |
| a1bbfa1 | S2 | The ceiling corner correction (web NIGHT5 F6) in the Mover |
| 114fcee | S3 | The Survival map on the web collider: config by path, zones and gate layers, pads, the core |
| a16e980 | S4 | Health, regen, points and the run loop |
| d6414e1 | S5 | KESTREL (hitscan, headshots, ADS + ricochet), PULSE GAUNTLET, punch / parry, the HUSK |
| f35d0fd | S6 | The husk brain on the web's nav grid: flow fields, tokens, watchdog, crowd, nav jumps |
| 81ef1f1 | S7 | The director (squads, pacing, camping, hunters), spawn telegraphs, hold-E gate buys |
| fcf3973 | S8 | The HUD and the screens (uGUI + TextMeshPro), the slice smoke |
| ea5ca4c | S9 | The Windows x64 Player, its command-line smoke and bench |
| 726fb0e | if-time 1 | The style meter (ranks multiply points, regen and stamina) |
| 2054d43 | if-time 2 | THUNDERCLAP wall buy in ARMORY |
| 7354bf2 | if-time 3 | SWARMERS from round 3 (and the hunter squad), the brain per kind |
| 8f82418 | if-time 4 | Aim assist at the web's default (SNAP), with an on/off toggle |
| 7ec106e | Q | The crosshair in the web's colour (cyan); the controls line lists 1-3 and Q |

Nothing on the cut list was cut. All four if-time items shipped.

## The numbers

- **Tests:** `run_tests` EditMode in the live Editor, **182 / 182** (12:44 PM). It started the night at 134.
- **Golden vectors:** 55 / 55 golden tests, **0 mismatches**, compared bit for bit:
  - movementMath, swingMath, spawnMath mulberry32, core presets
  - combatMath, gunMath sweepTargets
  - directorMath (with all 16 recorded planSquads deals)
  - styleMath, assistMath (with the springStep sequence)
- **Replays:** **16 / 16** recorded web trajectories replay bit-exact on the C# web collider.
- **Slice smoke:** all steps pass, in the Editor (12:40 PM, aim assist on) and in the built Player (`-skygrindSmoke`,
  12:51 PM, and again on the final build at 1:04 PM, 0 exceptions). Each run goes through round 1 cleared in 8 s
  (9 kills), the break, round 2, ARMORY bought, death, DOWN, RESULTS and restart.
- **Performance:** the Player at 1920 x 1080, windowed, vsync off, 60 s
  - load: 24 husks chasing a god-mode player across the open map, plus round 1's opening squads (31 alive at the end)
  - frame time: **median 9.35 ms (107 fps)**, **p95 18.47 ms**, worst 1% 40.25 ms (5,681 frames)
  - machine: RTX 5070 Laptop and Ryzen AI 9 HX 370, on AC, power plan Balanced the whole run (polled every 5 s)
  - caveat: the machine was in use, and system CPU sat at 100% for the second half; treat this as a loaded number
  - measured on the 12:47 build; the final build changes only the crosshair colour

## Differences from the web, with reasons

1. **Two web spawn bugs, fixed in Unity** (S7):
   - The web lets a group member spawn at an offset inside a wall (its check takes the nearest node within a cell),
     and the husk falls through the map. Unity requires a node in the offset's own column.
   - Spawn nodes on a zone's edge overlap the locked neighbour's gate field, so the web loses whole squads there.
     Unity skips nodes a husk can't fit at in the live world.
2. **Husk and swarmer steering is simplified** (S6, if-time 3). The web uses Yuka: Arrive, Separation, OffsetPursuit
   and Wander. Unity has Arrive blended at `ai.steer.gain`, and non-holders wait on ring 1 at their own angle with the
   strafe. There are no slots, squads, formations or flankers yet; a swarmer's wait spot is pushed out of your front
   cone instead of the web's slot filter.
   - Why: the slice's time box. Tokens, the crowd pass, the watchdog and the flow field are exact, and they carry
     most of the fight's shape.
3. **Aim assist without** the V key's manual lock, projectile lead and homing, the melee lunge, boss weak points,
   perks and the gamepad's mode. None of those things exist in the port yet.
4. **One setting, AIM ASSIST** (SNAP / OFF on the title and pause screens, stored in PlayerPrefs). The web has a full
   settings menu.
5. **The look is URP's, matched by eye:** stone, sky and gate-field shaders, URP bloom and fog. three.js materials
   don't map one to one. Enemies are unlit, like the web's MeshBasicMaterial.
6. **Fonts:** TextMeshPro's default. The web's Chakra Petch and Barlow ship as woff2 only.
7. **ARMORY's wall** has only the THUNDERCLAP. The other wall guns aren't ported.
8. **Parity fixes the port had missed**, now as the web: the round HP multiplier is rounded with maxHp kept; the
   brain gets +1 tokens while hunted; the crowd pass skips airborne bodies.

## Open risks

1. **Steering feel.** Item 2 above is the largest behaviour difference from the web. Groups may clump or orbit
   differently, and only a playtest will show it.
2. **Frame spikes.** A worst 1% of 40 ms under load needs a profile in a quiet machine. Suspects: per-step
   allocations (Loadout.Step copies the cooldown keys every step; the director and the assist allocate lists and
   lambdas), and GC.
3. **The Editor stalls when unfocused.** With someone using the machine, Pipeline evals time out after 5 s and Play
   checks take minutes. Night runs are fine; day runs need the Editor left alone.
4. **TextMeshPro's dynamic fallback font asset** is rewritten by Play mode (glyphs added, re-serialized). I restored it
   twice before committing. It needs a static atlas or an ignore rule.
5. **`ProjectSettings/UnityConnectSettings.asset`** has been modified since before the night (cloud services
   m_Enabled 0 to 1) and is not staged. GG decides whether to keep it. `ProjectSettings.asset` (preloadedAssets
   emptied at the start) is back to HEAD's contents: a Player build re-wrote it. No commit touched either file.
6. **Event payloads** are C# classes with the web's field names where they exist. The audio hook-up needs the name
   list in PORT-STATUS.md, not the web's objects.
7. **SNAP's stick follows a husk that walks up to a camping player** and drags the camera down to its feet. The web's
   lock does the same, but it reads stronger when standing still. A feel call for GG.

## How to play

- **Player:** `C:\Users\gaige\dev\SKYGRIND-unity\Builds\Stage1\SKYGRIND.exe` (Survival loads first).
- **Editor:** open `Assets/Scenes/Survival.unity` and press Play. `Assets/Scenes/Arena.unity` is the movement test
  arena.
- **Keys:**
  - move: WASD, SPACE jump, SHIFT dash, C slide (slam in the air)
  - weapons: LMB fire, RMB zoom (KESTREL), F punch / parry, 1-3 weapons, Q last weapon
  - buy: E (hold) for gates and wall guns
  - ESC pause (AIM ASSIST toggles there)
  - debug: F3 overlay, F8 opens every gate
- **Player checks:** `SKYGRIND.exe -skygrindSmoke -logFile smoke.log` (the slice smoke, then quit) and
  `SKYGRIND.exe -skygrindBench 60 -logFile bench.log` (60 s of frames with 24 husks).

## 10-minute playtest checklist

Play one run from the title to round 3 or 4, then answer one question per feel point.

1. **KESTREL's punch.** Hip-fire a husk at about 10 m, then zoom (RMB) and take a headshot. Does a hit and a kill
   land as hard as on the web (recoil, camera punch, hit marker, the kill's hitstop)?
2. **The husk's chase.** Let round 1 come to you. Do husks come from different sides and hold back while two attack,
   or do they clump into one line?
3. **The husk's attack.** Can you see the wind-up early enough to parry it with F, and does a parry feel rewarding
   (stagger, heal, stamina back)?
4. **The gate buy.** With the points, hold E at LANDING's gate. Are the prompt, the hold bar and the price clear, and
   does the gate opening read as a moment?
5. **The HUD.** In a fight, can you read HP, points, round and enemies left, and the buy prompt at a glance?
6. **The style meter.** Does the rank letter climbing while you keep moving, and falling when you stop, change how
   you play?
7. **THUNDERCLAP.** Buy it on ARMORY's wall. Is the blast satisfying up close, and is the airborne shotgun jump
   useful?
8. **Swarmers (round 3).** Do they feel like they come at your flanks? Is the crouch tell readable before the bite?
9. **Aim assist.** Does SNAP help without taking the camera from you? Toggle it OFF on the pause screen and compare.
10. **Movement.** Do dash, slide, wall ride, slam and pads feel identical to the web build?
