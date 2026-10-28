# SKYGRIND Unity feasibility spike: results

Unity 6000.3.7f1 (URP), branch `spike/unity-feasibility`. Web reference: `skygrind` commit `a001407`, exported by its
own code into `PortExport/` (three 0.186.1, Node v24.19.0).

Status: Tasks 1–6 done.

## Task 1–2: the pure modules

`movementMath.js` and `swingMath.js` are ported as static C# classes in `Assets/Scripts/Core` (assembly
`Skygrind.Core`, no UnityEngine reference, doubles throughout).

| Check | Cases | Result |
|---|---|---|
| Ported JS unit tests (`movementMath`, `swingMath`, `movementSim`) | 45 | 45 pass |
| Golden vectors `movementMath.json` + `swingMath.json` (every call, bit for bit) | 379 (incl. 1,544 sequence rows) | 0 mismatches |
| Golden vectors `spawnMath.json` mulberry32 | 9 seeds × 16 draws | 0 mismatches |
| `presetHeights` vs `maps/{spire,survival}.json` core presets | 16 preset lists | 0 mismatches |
| `MovementConfig` (hand copy) vs `config.json` | every ported field | equal |
| Callback vectors (README: cannot be stored) | 6 | skipped (none in these modules) |

Hand-copied expected values replaced by golden vectors where one exists: the jump pair at (1.4, 0.3) and at the config
values (1.5, 0.36), `stepsFor(0.25)`, `padLaunchSpeed(8, G)`, the first slide-jump chain value.

The wind-vs-apex-hang gravity test was fixed (both multipliers are 0.5 in the shipped config, so a swapped priority
passed in the JS and in the port). Planted-bug check: with hang before wind, only that test fails; the 140 golden
`gravityScale` vectors and all 16 trajectories still pass with the bug.

### Exactness findings (the Unity runtime vs V8)

Measured on 20,000 inputs against Node 24:

| | Unity runtime (Mono) | Fix in `JsMath` / `JsJson` |
|---|---|---|
| `Math.Sin` / `Math.Cos` | differ from V8 in the last bit on 510 / 502 | fdlibm port (what V8 uses): 0 / 0 |
| `Math.Max(0, -0)` | −0 (JS: +0) | JS max/min semantics |
| `sqrt(x² + y²)` vs `Math.hypot` | 7,519 differ | V8's Kahan-summed hypot: 0 |
| `double.Parse` of shortest-round-trip numbers | misread 1 of 60,000 | exact parser (Clinger fast path + BigInteger) |
| three.js `divideScalar` / `normalize` | multiplies by 1/s (not a division) | `Vec3.DivideScalar` / `Normalize` |

Without the fdlibm port, `bunny_hop` (continuously turning yaw) diverges from the web run (2.3e-15 m/s); the other
runs use yaws where .NET and V8 happen to agree.

## Task 3: the mover and the arena

`mover.js` is ported line for line (`Mover.cs`), with every move the config defines: ground/air Quake movement, jump
(coyote, buffer, variable height, apex hang), double jump, dash and dash jump, slide, slide jump and slope boost, ARCLINE
swing (hook, pull-in, pump, reel, release rules, line-of-sight break), wall ride and wall jump, slam and boost jump,
jump pads (plain and aimed). Timers count whole 120 Hz steps.

### Trajectory replay (the acceptance test)

The 16 recorded web runs, fed step by step into the C# Mover at 120 Hz. Compared every step: position, velocity, state,
grounded, crouched, wall contact, pips, gravity multiplier, rope (phase, L, taut) and every event payload field.

**On the C# port of the web's collider** (three's Octree, rebuilt exactly: same triangles, same order, same dropped
faces, verified against the stored triangles of all three maps): **16/16 pass, 7,380 steps identical bit for bit.**

| Run | Steps | C# web collider | Unity PhysX, per-triangle MeshColliders | Unity PhysX, BoxColliders |
|---|---|---|---|---|
| walk | 420 | PASS | not bit exact from step 1; same behaviour; max 2.5e-7 m | same behaviour; max 1.3e-7 m |
| walk_diagonal | 300 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| run_jump_double | 480 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| short_hop | 300 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| bunny_hop | 600 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| slide | 480 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| slide_jump | 420 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| dash | 600 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| dash_jump | 360 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| slam_boost | 420 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| wall_ride | 480 | PASS | **behaviour differs at step 248**: `touchingWall` true (web) vs false on 12 steps; no event differs; max 4.1e-5 m | differs at step 250 (7 steps); max 8.7e-6 m |
| wall_jump | 480 | PASS | same behaviour; max 3.3e-5 m | same; 8.2e-7 m |
| swing | 960 | PASS | same behaviour; max 2.6e-7 m | same; 1.3e-7 m |
| swing_from_ground | 540 | PASS | same behaviour; max 2.5e-7 m | same; 1.3e-7 m |
| ramp_walk_up (Spire) | 300 | PASS | same behaviour; max 6.2e-6 m | same; 3.5e-7 m |
| ramp_slide_down (Spire) | 240 | PASS | same behaviour; max 6.0e-7 m | same; 4.7e-7 m |

"Same behaviour" = at every step the same state, grounded / crouched / wall contact, rope phase, and the same events with
the same types and reasons (only their numbers differ by rounding). Full per-run reports: `Logs/replay-*.md` (generated
by the tests).

### Where the Unity collision query can't reproduce mover.js (causes)

No port bugs remain: on the C# web collider every run is bit exact. Every difference on PhysX is a Unity limitation:

1. **PhysX queries are float32.** `Physics.ComputePenetration` returns a float direction and depth, so every floor
   push leaves the feet ~1e-7 m off (5.6e-8 to 2.6e-7 m at step 1). The web game (doubles) lands exactly on y = 0.
   Effect on feel: none measurable (≤ 4e-5 m over a run).
2. **Contacts below float32 resolution are invisible.** In `wall_ride`, after the ride ends, the forward input presses
   the body into the wall by ~1e-15 m per step; three.js counts that as a contact (`touchingWall` true), PhysX does not.
   No event changed: the ride lock blocks re-entry, and the wall-jump check falls back to a 0.12 m probe.
3. **No contact point.** `ComputePenetration` gives a normal and a depth but no point, and mover.js needs the point
   (feet vs wall, floor vs overhang). With per-triangle MeshColliders the point is placed the way three.js places a face
   contact (on the capsule axis where the pushed-out plane crosses it); `ClosestPoint` is not available on non-convex
   meshes. This reproduced the web's classification in every recorded run and every preset. Edge contacts (three.js
   puts the point on the triangle edge) are the case it can get wrong.
4. **Concave corners drift slightly.** In my L-piece scenario (pressing into a wall standing on a slab, 1.5 s), PhysX
   creeps 1.1 cm along the wall where the web collider creeps 0; pressing into the 12 m wall drifts 2.4 mm.
5. **Not a cause:** the fixed step. Both run whole 1/120 s steps.

The faces three's Octree drops on the outer max-x / max-z planes (training 14, spire 24, survival 28) are excluded in
the Unity collider too: it is built from the Octree's own triangles.

### The web's regression presets (scripts.js CHECKS) on the training map

All 17 web presets pass on both worlds (jump apex 1.5 / 1.584 m, dash 6 m, stamina, slide-jump chain to 24, coyote,
buffer, pillar-edge drift 0, no ride on a strafe into a wall, dash from every state, dash jump at 24, slam boost apex
4.86 m, 1.2 s wall ride, 3 wall jumps, pad apex 8 m, slam vs slide, the full chain), and so do the 5 spike scenarios
(tunnel crouch, ramp slide 18.0 m/s, the L piece, a swing from a node, the Grinder carry). `Logs/collision-parity.md`.

### Sim cost per step

Editor, Mono JIT, steady state: 2.2 µs per 120 Hz step on PhysX, 11 µs on the C# web collider (the player Mover).
Built-player numbers for the enemy update are in Task 4–5.

### Untested against the web game

Not in the exported runs, implemented from the source and flagged: jump pads (plain and aimed), the kill plane, the
Grinder's motion and carry (a small Grinder is added to the arena: P reshapes, O goes there), perks (VIGOR, GRIP,
SLIPSTREAM hooks; neutral by default).

### Not implemented

- Camera overlays: speed lines, the dash edge tint, the aim-lock FOV pinch, recoil (visual only; the FOV, roll, dip and
  kicks are ported).
- The ARCLINE's enemy pull and zip (no enemies in the arena), the LOW GRAVITY mutator, rocket-blast impulses.
- Weapons, audio, the viewmodel, the style meter.

## Task 4–5: enemy scale, measured in a built Player

Set up to match the web bench (`skygrind/port-export/bench/RESULTS.md`), so the two can be compared row by row.
Raw records: `Tools/EnemyBench/results/*.json`, one per Player run. Every table below is generated by
`Tools/EnemyBench/summarize.mjs` into `results/tables.md`, which also has the per-pass values. To reproduce:
`Tools/EnemyBench/run.sh <exe> pass1` (then pass2, pass3).

### Hardware and software

| | |
|---|---|
| Machine | ASUS ProArt P16 (H7606WP) |
| CPU | AMD Ryzen AI 9 HX 370 w/ Radeon 890M, 12 cores / 24 threads |
| GPU used | NVIDIA GeForce RTX 5070 Laptop GPU, 8 GB, driver 32.0.15.9200 (nvidia-smi 592.00). The Player log of every run shows `D3D12 Device Filter ... Device Name: NVIDIA GeForce RTX 5070 Laptop GPU, Device Type: Discrete`, and each record stores the device name. D3D12 picked the RTX on its own, with no `-force-device-index`. |
| Other GPU | AMD Radeon 890M (driver 32.0.22018.5001), which drives the 3840 x 2400 panel |
| RAM | 64 GB (Unity reports 64,633 MB) |
| OS | Windows 11 Pro 10.0.26200 |
| Power plan | **Silent** in every run kept (powercfg, polled every 2–4 s during each run and stored in its record). See below: the plan switched to Balanced three times during the session. |
| Unity | 6000.3.7f1, URP 17.3.0, Windows Player, **Mono** backend (the IL2CPP module is not installed), release build (not development), Direct3D 12, quality level "PC" |
| Display | 1920 x 1080 windowed, vsync 0, `Application.targetFrameRate = -1` (read back from the Player and stored per run) |

### What runs

- **Map:** Survival, from `PortExport/maps/survival.json`. Collision is the web's own Octree triangles (1,244, with three's 28 dropped faces left out) plus the core at its `flat` load preset, as `world/collider.js` builds it.
- **Player:** stands still at the map's playerSpawn (0, 0, 40), yaw 0, with the camera at eye height and the web's FOV (74°) and clip planes.
- **Spawns:** N enemies from the web bench's own spawn plan (`spawnPlan.mjs`, ported as `SpawnPlan` with tests): the 4 ground spawns 15–40 m from the player, squads of 5. Enemies never die: alive at the end is N in every run.
- **Loop:** the web game's. Frame delta clamped to 0.05 s, whole 1/120 s steps, then render.
- **Runs:** each (variant, N) in a fresh Player process; 10 s warm-up, then 30 s measured; 3 passes, back to back. Each table value is the median of the 3 passes (`sorted[floor(n/2)]`, as the web's summarizer computes it), with the min–max across passes in parentheses. p99 = `sorted[floor(0.99 n)]`; worst 1% = the mean of the slowest 1%, the web's definitions.
- **Sim cost** is a Stopwatch around each 120 Hz step's whole enemy update, inside the running Player: 3,600 steps per run.

Variants (all enemies are Husks: husk.js's chase, then moveHusk):

- **closer-web** (the main one): steering every step, line of sight every 0.25 s per enemy (staggered), a grid flow field when out of sight, separation, and collider.js's aggregated capsuleIntersect push-out. All world queries go through **the C# web collider** (three's Octree port, bit-exact in Task 3, plus the core layer).
- **closer-unity:** the same, through Unity physics (Physics.Raycast; ComputePenetration on the per-triangle MeshColliders).
- **plain:** chase, separation and gravity on the plane y = 0. No world queries.

The Jobs/Burst versions (`EnemyCrowdBurst`) were dropped from the comparison as you asked, because N = 200 holds far above 60 fps. The code stays in `Assets/Scripts/Bench`.

### Sim: ms per 120 Hz step, the whole enemy update

| variant | N | passes | avg | p99 | worst 1% (mean) | max | share of the 8.33 ms step | web Node avg / worst 1% | LOS rays/s |
|---|---|---|---|---|---|---|---|---|---|
| closer-web | 24 | 3 | 0.1539 (0.1497-0.1676) | 0.3176 (0.2978-0.3542) | 0.3575 (0.3334-0.4134) | 0.539 (0.503-0.615) | 1.8% | 0.0609 / 0.2757 | 96 |
| closer-web | 40 | 3 | 0.2433 (0.2325-0.2469) | 0.4016 (0.3748-0.5056) | 0.4591 (0.4573-0.5534) | 0.639 (0.555-0.679) | 2.9% | 0.0791 / 0.3396 | 160 |
| closer-web | 100 | 3 | 0.5852 (0.5504-0.7082) | 1.1832 (1.1368-1.7295) | 1.2483 (1.2333-2.7596) | 1.487 (1.467-6.121) | 7.0% | 0.2956 / 0.9506 | 400 |
| closer-web | 200 | 3 | 1.1469 (1.1143-1.1883) | 1.8488 (1.8346-1.8899) | 1.9644 (1.9189-1.9905) | 2.449 (2.198-3.467) | 13.8% | 0.6026 / 1.5794 | 800 |
| closer-unity | 24 | 3 | 0.1291 (0.1224-0.1849) | 0.2443 (0.2220-0.5104) | 0.2694 (0.2668-0.7152) | 0.541 (0.516-1.495) | 1.5% | 0.0609 / 0.2757 | 96 |
| closer-unity | 40 | 3 | 0.2018 (0.1857-0.2699) | 0.3941 (0.3860-0.7253) | 0.4800 (0.4078-1.4596) | 0.855 (0.509-4.568) | 2.4% | 0.0791 / 0.3396 | 156 |
| closer-unity | 100 | 3 | 0.4582 (0.4355-0.4898) | 0.9142 (0.8941-0.9398) | 0.9871 (0.9600-1.0573) | 1.293 (1.048-1.633) | 5.5% | 0.2956 / 0.9506 | 400 |
| closer-unity | 200 | 3 | 0.8999 (0.8740-0.9589) | 1.5313 (1.5302-1.5738) | 1.7568 (1.5819-1.8758) | 2.977 (1.723-5.449) | 10.8% | 0.6026 / 1.5794 | 800 |
| plain | 24 | 3 | 0.0287 (0.0266-0.0318) | 0.0539 (0.0517-0.0620) | 0.0771 (0.0624-0.0808) | 0.182 (0.077-0.505) | 0.3% | 0.0609 / 0.2757 | 0 |
| plain | 40 | 3 | 0.0594 (0.0571-0.0611) | 0.1072 (0.0941-0.1310) | 0.1479 (0.1280-0.1626) | 0.285 (0.154-0.323) | 0.7% | 0.0791 / 0.3396 | 0 |
| plain | 100 | 3 | 0.2238 (0.2087-0.2313) | 0.4173 (0.3451-0.5040) | 0.5209 (0.4549-0.5958) | 0.676 (0.614-0.976) | 2.7% | 0.2956 / 0.9506 | 0 |
| plain | 200 | 3 | 0.5760 (0.5707-0.5904) | 1.1481 (1.1356-1.1973) | 1.2668 (1.2530-1.2990) | 1.627 (1.560-2.393) | 6.9% | 0.6026 / 1.5794 | 0 |

**Against the web (Node, the same N, avg / worst 1%):**

| N | web | Unity closer-web (C# web collider) | ratio avg / worst 1% |
|---|---|---|---|
| 24 | 0.061 / 0.28 ms | 0.154 / 0.36 ms | 2.5x / 1.3x |
| 40 | 0.079 / 0.34 ms | 0.243 / 0.46 ms | 3.1x / 1.4x |
| 100 | 0.30 / 0.95 ms | 0.585 / 1.25 ms | 2.0x / 1.3x |
| 200 | 0.60 / 1.58 ms | 1.147 / 1.96 ms | 1.9x / 1.2x |

- At N = 200 the whole enemy update takes 1.15 ms on average (13.8% of the 8.33 ms step) and 1.96 ms in the worst 1% (23.6%). Your estimate of 200 x ~11 µs ≈ 2.2 ms was high: the push-out alone costs 3.4 µs per enemy per step at N = 200 (breakdown below).
- The spread across passes is small for the sim (≤ 7% at N = 200). The one outlier is pass 2 at N = 100, with a worst 1% of 2.76 ms against 1.23–1.25 ms in the other passes.
- Unity's update does less than the web's (Husks only, no brain) and still costs more, so per unit of work the C# side is slower than these ratios show.
- closer-unity (PhysX) is about 22% faster than closer-web at N = 200 (0.90 against 1.15 ms), but it is not bit exact (Task 3).

### Where the step goes

One extra run per row, with per-phase timers (`-benchProfile`). These runs are not in the tables above, and the timers add 2–3% (0.159 against 0.154 ms at N = 24).

| variant | N | step avg (timed) | grid + flow field | line of sight | steering + separation | move + push-out | separation pairs / step | power plan |
|---|---|---|---|---|---|---|---|---|
| closer-web | 24 | 0.1587 | 0.0056 (4%) | 0.0122 (8%) | 0.0183 (12%) | 0.1221 (77%) | 278 | Silent x15 |
| closer-web | 200 | 1.1648 | 0.0086 (1%) | 0.0589 (5%) | 0.4145 (36%) | 0.6824 (59%) | 10458 | Silent x18 |
| closer-unity | 24 | 0.1277 | 0.0064 (5%) | 0.0057 (4%) | 0.0194 (15%) | 0.0958 (75%) | 284 | Silent x17 |
| closer-unity | 200 | 0.9162 | 0.0081 (1%) | 0.0245 (3%) | 0.4164 (45%) | 0.4666 (51%) | 10433 | Silent x17 |
| plain | 24 | 0.0315 | 0.0051 (16%) | 0.0012 (4%) | 0.0232 (74%) | 0.0014 (4%) | 400 | Silent x17 |
| plain | 200 | 0.6003 | 0.0078 (1%) | 0.0075 (1%) | 0.5760 (96%) | 0.0085 (1%) | 15005 | Silent x17 |

- **Collision is the biggest cost, as on the web.** The C# push-out takes 77% of the step at N = 24 and 59% at N = 200 (0.68 ms). Line of sight, at 800 rays/s, is 5%.
  - The web profile puts push-out plus raycasts at about 0.26 ms at N = 200. The C# Octree port does the same queries about 3x slower.
  - It is a line-for-line port tuned for exactness, not speed: JS min/max semantics, double Vec3 structs, a HashSet dedupe per query, all under Mono's JIT.
  - It was not optimized beyond removing a per-ray allocation. IL2CPP was not measured.
- **Separation is the second cost at N = 200**, and it is a pile-up. All enemies push toward the same point 1 m from the player, so the 1.2 m grid holds about 50 neighbours per enemy: 10,458 pairs per step and 0.41 ms. Without walls (plain), every enemy ends within 3 m of the player: 15,005 pairs and 0.58 ms. The web's crowd pass (rings, slots, mass) costs 0.022 ms at N = 200.
- Flow-field lookups and the grid rebuild are ≤ 5%. The player never moves, so the flow field is built once.

### Frames: built Player, RTX 5070, 1920 x 1080 windowed

| variant | N | alive at end | fps | avg ms | p99 ms | worst 1% (mean) ms | max ms | steps/s |
|---|---|---|---|---|---|---|---|---|
| closer-web | 24 | 24/24/24 | 497.5 (301.4-506.3) | 2.01 (1.98-3.32) | 4.28 (4.23-11.32) | 7.73 (6.65-13.19) | 34.9 (25.5-44.4) | 120.0 (120.0-120.0) |
| closer-web | 40 | 40/40/40 | 500.1 (341.4-514.6) | 2.00 (1.94-2.93) | 4.11 (4.03-7.26) | 4.81 (4.81-8.19) | 10.6 (7.5-14.3) | 120.0 (120.0-120.0) |
| closer-web | 100 | 100/100/100 | 307.0 (300.6-449.4) | 3.26 (2.23-3.33) | 8.34 (4.33-9.01) | 9.40 (5.03-10.80) | 15.7 (12.9-19.8) | 120.0 (120.0-120.0) |
| closer-web | 200 | 200/200/200 | 411.2 (304.9-415.3) | 2.43 (2.41-3.28) | 4.77 (4.57-8.01) | 5.87 (5.14-8.88) | 13.3 (7.6-15.0) | 120.0 (120.0-120.0) |
| closer-unity | 24 | 24/24/24 | 385.7 (287.5-398.4) | 2.59 (2.51-3.48) | 7.42 (6.45-9.62) | 8.67 (8.10-11.40) | 16.7 (13.3-23.3) | 120.0 (120.0-120.0) |
| closer-unity | 40 | 40/40/40 | 362.5 (328.4-382.4) | 2.76 (2.62-3.04) | 7.98 (7.96-10.81) | 17.03 (12.66-27.22) | 848.2 (22.9-1720.9) | 116.8 (113.3-120.0) |
| closer-unity | 100 | 100/100/100 | 379.8 (362.6-422.9) | 2.63 (2.36-2.76) | 6.86 (6.51-8.00) | 8.13 (8.03-8.85) | 13.8 (11.8-18.9) | 120.0 (120.0-120.0) |
| closer-unity | 200 | 200/200/200 | 397.3 (387.0-476.6) | 2.52 (2.10-2.58) | 5.55 (3.95-6.01) | 6.87 (4.50-7.30) | 12.7 (9.9-15.9) | 120.0 (120.0-120.0) |
| plain | 24 | 24/24/24 | 489.5 (393.6-567.0) | 2.04 (1.76-2.54) | 4.81 (3.55-7.36) | 6.06 (4.03-8.69) | 14.5 (5.7-28.3) | 120.0 (120.0-120.0) |
| plain | 40 | 40/40/40 | 485.6 (442.8-539.4) | 2.06 (1.85-2.26) | 4.52 (3.91-6.03) | 5.65 (4.63-8.00) | 16.9 (9.4-48.9) | 120.0 (120.0-120.0) |
| plain | 100 | 100/100/100 | 467.7 (383.3-508.3) | 2.14 (1.97-2.61) | 4.76 (4.04-7.73) | 5.71 (4.67-11.84) | 18.8 (9.7-136.0) | 120.0 (119.7-120.0) |
| plain | 200 | 200/200/200 | 440.2 (437.4-451.6) | 2.27 (2.21-2.29) | 4.43 (4.38-4.69) | 5.07 (4.97-6.02) | 15.4 (14.2-17.4) | 120.0 (120.0-120.0) |

**Against the web (RTX 5070, High, median of 3):**

| N | web | Unity closer-web |
|---|---|---|
| 24 | 166 fps (6.0 ms avg, 10.5 ms worst 1%) | 497 fps (2.0 ms, 7.7 ms) |
| 200 | 138 fps (7.2 ms avg, 41.9 ms worst 1%) | 411 fps (2.4 ms, 5.9 ms) |

- **The frames are not like for like (see the list below).** Unity renders the map's boxes and capsule enemies, without the game's HUD, post, effects or enemy models. Read these as the frame cost of this scene, not of a full port.
- **The spread between passes is large**, as on the web: closer-web at N = 24 ran 301 / 497 / 506 fps. Most differences between cells are within that noise. By your rule, treat differences under about 30% as noise.
- **Two unexplained long frames:** 848 ms (pass 1, 24.7 s into the measurement) and 1,721 ms (pass 2, 10.6 s in), both in closer-unity at N = 40.
  - The sim was not the cause: the slowest step in that cell was 4.6 ms, and the Player logs show nothing.
  - They lowered that cell's steps per second (113–117) and are included in its worst 1%.
- Steps per second held 120 in every other cell. The sim kept real time.

### Success line

The line was 40 enemies at ≥ 120 fps, 200 at ≥ 60 fps, and all ported tests passing. On closer-web it is met by a wide margin:

- N = 40: 500 fps median (lowest pass 341).
- N = 200: 411 fps median (lowest pass 305); worst 1% 5.9 ms, about 170 fps.
- Tests: 134 / 134 EditMode tests pass, including the golden vectors and the 16 bit-exact trajectory replays.

**Suggested change:** these frame numbers do not include the game's rendering, so the line says little about a full port. A line that this spike can actually test is the sim budget. For example: the whole enemy update at N = 200 under 15% of the step on average and under 25% in the worst 1%. On the C# web collider it measures 13.8% and 23.6%, which passes, but closer to the edge than the frame rate suggests.

### What I could not match to the web setup

1. **Enemy logic.**
   - Here: Husks only (husk.js chase, separation, moveHusk).
   - Web: the 10-kind mix (7 kinds; drones fly, lancers perch) with the real AI brain (tokens, slots, squads, Yuka steering, the stuck watchdog, nav jumps), orbs and crowd.js. No attacks or leaps here.
2. **AI cadence.**
   - Here: 1 line-of-sight ray per enemy every 0.25 s, the agreed spike rule: 800 rays/s at N = 200. Steering runs every step.
   - Web: at most 3 rays per fixed step (`ai.losPerStep`, ≤ 360/s) with a 0.3 s max age (`ai.losMaxAge`), and the brain thinks at 12 Hz (`ai.thinkHz`).
   - So at high N this bench does more line-of-sight and steering work than the web.
3. **Navigation.**
   - Here: a spike flow field (BFS on a 1 m grid built from the map's boxes), built once because the player never moves.
   - Web: its nav grid plus flow fields (`navGrid` in the export, not ported).
4. **How the sim is timed.**
   - Web: Node, headless, steps back to back. Its average also includes the stationary player's Mover (~0.01 ms) and `orbs.step`.
   - Here: timed inside the rendered Player, with no player Mover step.
5. **Rendering.** The frames are not comparable as a port's frame time.
   - Web: the full game at High (245 draw calls, 68–143k triangles, HUD, post, particles, enemy models).
   - Here: the Survival boxes, ramps, pads and nodes with URP Lit at the "PC" quality level, and GPU-instanced capsules for enemies. No HUD, post or effects. Chrome's "High" and URP's "PC" are different presets.
6. **Presentation.**
   - Web: Chrome ran headless (offscreen).
   - Here: the Player is a real 1920 x 1080 window on a panel driven by the 890M, so each frame is copied across adapters (hybrid graphics).
7. **Build flavour.**
   - Web: a `?debug=1` build.
   - Here: a release Player on Mono. IL2CPP (usually faster for C#) could not be tested because the module is not installed.
8. **Power state.**
   - The plan switched from Silent to Balanced three times during the session (`results/power-plan.txt`): once before the passes, once mid-pass 3, once during the breakdown runs. Windows' power diagnostic log is off, so the cause was not found. Armoury Crate ProArt, which manages its own operating modes, is the likely source.
   - Every affected run was set aside (`results/not-silent/`, `results/profile/not-silent/`) and rerun on Silent. Every run kept was Silent for its whole duration.
   - powercfg cannot show the ASUS firmware mode (fans, power limits), so that part of the state is not verified.
   - The web bench read the plan once, before its runs, so whether it stayed on Silent during them is not known.
9. **Machine state.** The Unity Editor stayed open and idle (Edit mode) during the runs. Background apps were not controlled, the same as for the web runs.


## Task 6: workflow report

How the work was driven: the unity CLI (1.0.0-beta.11) against the open Editor through the project's Pipeline package:
`recompile` / `recompile_status`, `run_tests`, `eval` / `eval_file`, `editor_play` / `editor_stop` / `editor_status`,
`console` / `console_status`, `capture_game_view`, `package_add`, `build` / `build_status`. No AI Assistant, no
generators, no Asset Store packages.

### Counts

| | Task 2 | Task 3 | Task 4-5 | Total |
|---|---|---|---|---|
| Compile-fix loops (a recompile that failed and needed an edit) | 0 | 1 | 0 | 1 |
| Runtime bugs found after a clean compile | 0 | port bugs caught by the golden vectors / replays (below) | 1 (Burst dispose) | |
| Blocking Editor dialogs | 0 | 0 | 0 | 0 |
| Stalls | 0 | 1 (about 40 min) | 0 | 1 |

- The one compile error: `t.CompareOrdinal(...)` called as an instance method (JsJson).
- Port bugs the tests caught (each fixed in the port, none by tuning numbers): three's `divideScalar` is a multiply by
  1/s; `Vector3.length` is a plain sqrt, not hypot; mover distances use `distanceTo`; StepAir / StepWallRide used
  1/simHz instead of dt; the core tween in floats; a translate that mixed components.
- The Burst bug: a shared dummy NativeArray made Dispose throw midway, so the crowd kept a half-disposed state and
  every step threw. Found by a short Editor run of every variant, before any Player measurement.

### Domain reloads and recompiles

- A recompile plus domain reload took 30-60 s. The first command after a reload often timed out (the Pipeline server
  is down while the domain reloads); a retry worked. The Pipeline port moved between 7800 and 7801 across reloads; the
  CLI followed it on its own.
- `recompile_status` could report "completed" before the new assemblies were written, so every cycle was checked by
  the DLL timestamps in `Library/ScriptAssemblies` and by the test results changing.

### Where the tooling hindered

1. **The Task 3 stall (about 40 min).** The Editor was in Play mode (not entered by me) with Run In Background off
   and the window unfocused. Play mode froze, and every Pipeline command timed out, while `unity status` kept saying
   "ready". The CLI gives no sign that the Editor is in Play mode or stalled. I wrote the Task 3 code offline meanwhile
   (about 2,500 lines, which then compiled with 0 errors), and turned Run In Background on once it recovered.
2. **Recompiling during Play mode (Task 4).** `recompile` reloaded the domain under the running Arena; its runtime
   state went null and it threw every frame (27,000 console errors in about a minute). Nothing warned. My compile loop
   now checks `editor_status` and refuses to recompile while playing.
3. `unity test` starts a second, batch Editor, which cannot open a project the open Editor holds, so every test run
   went through the live Editor's `run_tests` instead.
4. `capture_game_view --save_path` is relative to `Assets/`, so a screenshot landed inside the project's assets and
   had to be moved out.
5. A connected Switch Pro controller made the Input System warn about its per-frame budget once.
6. The Player build rewrote four settings files. Three were ordinary URP and build settings (committed). The fourth,
   `UnityConnectSettings.asset`, switched `m_Enabled` from 0 to 1, turning on Unity's cloud services. I did not ask
   for that and left it uncommitted.
7. Not the CLI, but the same kind of friction: Git Bash rewrites `/switch` arguments as paths, so the first bench
   runner called `powercfg /getactivescheme` wrongly and recorded no plan. It now uses `-getactivescheme`.

### Where it helped

- `run_tests` in the live Editor: the whole suite in one call with JSON per test (134 tests in about 21 s), so the
  golden vectors and the 16 bit-exact replays ran after every change.
- `eval`: one-off setup without menus or dialogs (creating the scenes, the materials, the build list) and starting the
  bench inside Play mode.
- `build` / `build_status`: a Windows Player build in about a minute with the full BuildReport (0 errors; the 487
  warnings are all from the template's preinstalled `com.unity.ai.inference` shaders).
- `console` with stack traces and `editor_status`: they found the Burst bug and the Play-mode reload in minutes.
- `package_add` for Burst, Collections and Mathematics.

### Runtime findings that cost time (Mono vs V8)

Bit-exact parity with the web needed my own `JsMath` (fdlibm sin/cos, V8's hypot, JS max/min) and `JsJson` (an exact
number parser): Mono's `double.Parse`, `Math.Sin` / `Math.Cos` and `Math.Max(0, -0)` differ from V8 (Task 1-2 table).

### Privacy note

During the Task 3 stall, a full-desktop screenshot (taken to find a dialog) also captured your browser. I deleted it
at once and told you; after that I captured only Unity's window.
