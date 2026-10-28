# port-export: reference data for a Unity port of SKYGRIND

Everything here was produced by running the game's own code, unmodified, headless in Node. Nothing was typed in by
hand. A port can check itself against these files: same inputs in, same numbers out.

- Source snapshot: tag `p17-night6` (commit `1c73d68`, the frozen web game; refreshed on NIGHT7 from a copy of the
  scripts in `skygrind/port-export-p17/`). Before that: commit `a001407`. Every vector and trajectory file is
  byte-identical between the two; `config.json`, `config.notes.md`, `maps/survival.json` and `maps/training.json`
  changed. three `0.186.1` (the pinned version). Generated with Node `v24.19.0`.
- Nothing in `src/` was changed. The scripts import the real modules from `src/` and record what they return.
- The files are regenerated from scratch in about 5 s. Two runs produce byte-identical files (checked with sha1 over
  all 34 outputs).

## Regenerate

From the repo root:

```sh
node port-export/scripts/all.mjs          # everything below (A-D)
node port-export/scripts/verify.mjs       # replays the exported files through the real code (exit 1 on any mismatch)

node port-export/scripts/export-config.mjs   # A only
node port-export/scripts/vectors.mjs         # B only
node port-export/scripts/trajectories.mjs    # C only
node port-export/scripts/maps.mjs            # D only
```

There is no npm script for these. `package.json` is an existing file, and this export was not allowed to change it.
The scripts need only the repo's installed `node_modules` (three).

`verify.mjs` currently reports:

- 1,267 vector calls reproduced exactly from the JSON alone. 0 mismatched. 6 skipped because they take an injected
  callback, see below.
- All 16 trajectories replayed exactly (every step's position, velocity, state and events) from their recorded
  inputs alone.

## Layout

| Path | What | Size |
|---|---|---|
| `config.json` | A: the whole `src/config.js` object | 116 KB |
| `config.notes.md` | A: every source comment next to a config value (units live there), with its path | 178 KB |
| `vectors/<module>.json` | B: golden vectors, 13 files | ~1 MB |
| `trajectories/<name>.json` | C: 16 Mover runs at 120 Hz | 2.3 MB |
| `maps/<map>.json` | D: `spire`, `training`, `survival` | 5.1 MB |
| `scripts/` | the generators (`lib.mjs` shared helpers, `all.mjs`, `verify.mjs`) | |

## Conventions

- **Units:** meters, seconds, m/s, degrees, as `config.js`'s header says. Angles in function arguments are radians
  wherever the source does math (yaw, pitch, cone half-angles). `config.notes.md` gives the unit per key where the
  source states one.
- **Axes** (three.js): right-handed, y up. `yaw` 0 looks toward −z, and a positive yaw turns toward −x (forward =
  (−sin yaw, 0, −cos yaw); see `wishDirection`). Unity is left-handed (z forward), so a port converts by flipping z
  (and triangle winding).
- **Fixed step:** `dt = 1/120` s (`config.loop.simHz`). Every file that depends on it records `dt`.

### Value encoding (all JSON files)

JSON numbers are written by `JSON.stringify`. That is the shortest text that parses back to the identical IEEE-754
double, so every float here is at full precision. Values JSON cannot hold are wrapped (see `scripts/lib.mjs` `enc()`):

| Wrapper | Meaning |
|---|---|
| `{"$num": "-0"}` / `"NaN"` / `"Infinity"` / `"-Infinity"` | that number |
| `{"$undefined": true}` | `undefined` (for example an award's absent `quiet` flag) |
| `{"$config": "path.to.object"}` | this argument *is* that object of `config.json` (`""` is the whole config). Resolve it, don't copy it |
| `{"$fn": {"$rng": "mulberry32", "seed": S, "draws": N}}` | a fresh `mulberry32(S)` (the game's PRNG, `src/enemies/spawnMath.js`). `draws` is how many numbers the call consumed (0 in `args`, the count in `argsAfter`) |
| `{"$fn": {"$callback": "text"}}` | an injected callback (line of sight and the like). Only its behaviour is described. `verify.mjs` skips these 6 calls |
| `{"$map": [[k, v], ...]}`, `{"$set": [...]}` | Map / Set |

Typed arrays are written as plain arrays. Where a source value was a Float32 (geometry buffers, nav grid `y` /
`clear` / `linkCost`), the number is that float32 value written as a double.

## A. config.json and config.notes.md

- **`config.json`** is `JSON.stringify(config)` of the live export of `src/config.js` (pretty-printed). The config
  holds only plain data (no functions, NaN or undefined), and a JSON round trip is exact. Derived values are *not* in
  it, because the game computes them at use time. For example, jump v0 = `2·jump.height/jump.apexTime` and gravity =
  `2·jump.height/jump.apexTime²` (`movementMath.js` `jumpVelocity` / `gravityFor`).
- **`config.notes.md`** is a table of `path | value | line | comment`: 2,027 rows, and every path resolves in
  `config.json`.
  - Each row is a `//` comment copied verbatim from `src/config.js`, attached to the key(s) on that source line.
    Several keys on one line share its comment.
  - A comment on its own line is attached to the next key and marked `(above)`. That includes the second half of a
    comment that wraps onto the next line (for example `aim.lock.fire` at line 594 continues at 595).
  - A key the source doesn't comment has no row. `(S)` in a comment is the source's own marker for "our own starting
    guess".

## B. vectors/

One file per module:

| File | Covers |
|---|---|
| `movementMath` | every export |
| `swingMath` | ARCLINE |
| `styleMath` | style meter math |
| `styleCore` | a scripted scoring session through `createStyleCore` |
| `styleSim` | `paceReport` and `simulateStyle`, recorded whole |
| `directorMath` | Survival director |
| `spawnMath` | wave spawner and `mulberry32` |
| `combatMath`, `weaponMath`, `gunMath`, `arsenalMath`, `momentumMath`, `assistMath` | the weapon and combat math in `src/combat/` |

File shape: `{module, generatedBy, encoding, dt?, gravity?, ..., cases: [...]}`. `gravity` is
`gravityFor(config.jump.height, config.jump.apexTime)`, as the Mover uses it.

Case kinds:

- **Call:** `{fn, args, result, argsAfter?, note?}`. `args` are the arguments as they were *before* the call.
  `argsAfter` is present only when the function mutated an argument (for example `heatStep`, `diminish`,
  `stepShownRank`, `swingStep`, `bounceVelocity`, `challenge`). Functions with an optional `out` parameter were called
  without it, so `result` is the returned object.
- **Constant:** `{constant, value}`, for example `MoveState`, `FRESH_WEAPONS`, `PHASES`.
- **Sequence:** `{sequence, note, rows}`. One function called every step with the state carried over. `note` names
  the columns and the inputs. The sequences are:
  - `heldJumpVerticalArc`: the Mover's airborne vertical rule, `gravityScale` then `verticalStep`
  - `freePendulum` and `pumpAndReel`: `swingStep`
  - `stepDirector`: reaches PEAK, RELAX and BUILD_UP again
  - `stepCamp`: reaches EXPOSED and hunted, then a relocation
  - `stuckStep`, `heatStep`, `smoothVelocity`, `springStep`
- **`mulberry32`:** `{fn: "mulberry32", seed, first16}`, the first 16 draws for 9 seeds (including 0, 2^32−1 and −1).
  Port this PRNG first. The rng-driven vectors (`planSquads`, `waveComposition`, `groupSizes`, `shuffled`,
  `pickSpawnPoint`, `simulateStyle`) depend on it.
- **`styleCore` session:** `{session: [...]}`. `createStyleCore(config.style, hooks)`, then each entry `{op, args,
  result, events, after, repeat?}` in order. `events` are the `onAward(name, points, quiet)` / `onRank(rank, up)`
  calls during that op, and `after` is the core's `meter`, `rank`, `total`, `time`, `fill()` and `stats`. A `step`
  entry with `repeat: n` is `core.step(dt)` n times.

To replay a call: decode `args` (resolve `$config`, create fresh rngs), call the function, compare the result with
`result` exactly (and the arguments with `argsAfter`). `scripts/verify.mjs` is a working reference.

## C. trajectories/

The real `Mover` (`src/movement/mover.js`) over the real world collider (`src/world/collider.js` wrapping a three.js
Octree built the way `arena.js` builds it), stepped at 120 Hz.

**Header:**

- `dt`, `simHz`
- `scene`: either `{kind: "boxes", boxes}` (build a box collider from these) or
  `{kind: "map", map: "spire", wedge, ...}` (the real Spire collider, see `maps/spire.json`)
- `start`: `{position, yaw}`. Setup is `mover.reset(x, y, z)`: state AIR, zero velocity, full pips. Then one
  `Mover.step` per row. The first 0.25 s of every run is standing still.
- `ropeEnabled`, `anchor`, `statesSeen`, `eventsSeen`

**Row** (the state *after* that step):

- `step` (1-based), `t = step·dt`
- `input: {moveX, moveZ, yaw, pitch, held: [...], pressed: [...]}`
- `position` (feet), `velocity`
- `state`, `grounded`, `crouched`, `pips`, `touchingWall`, `gravityMultiplier`
- `events`: every `{type, ...payload}` the Mover emitted that step
- `rope: {phase, L, taut}` on runs with the rope enabled

**Rebuilding the InputFrame from `input`:**

- Buttons are `{held, pressed}`. `pressed` is true only on the step the button goes down, the latched edge the
  keyboard input produces.
- `grappleTarget` is the file's `anchor` on every step where `grapple` is held, else `null`.
- Buttons not listed are not held.
- Some runs were scripted reactively (for example "press jump on the step after the `slam` event"), but the recorded
  inputs are explicit per step, so replaying them needs no script.

| Run | Scene | What happens (from the recorded events) |
|---|---|---|
| `walk` | flat | ground accel to 8 m/s, then friction to a stop |
| `walk_diagonal` | flat | forward + right, normalized wish, yaw 0.3 |
| `run_jump_double` | flat | jump, double jump (apex 2.26 m above the floor), land. SKYGRIND has no sprint: 8 m/s is the ground cap, so this is the requested "sprint-jump" |
| `short_hop` | flat | one-step jump tap: the release cut (apex 0.43 m) |
| `bunny_hop` | flat | jump on every landing step with a strafe and turning yaw: speed climbs to 15.6 m/s |
| `slide` | flat | slide at the 11.6 m/s floor speed, then release |
| `slide_jump` | flat | jump out of a slide: 11.6 × 1.2 = 13.92 m/s |
| `dash` | flat | standing dash, running dash, repeated presses until `dashFail`, pip regen |
| `dash_jump` | flat | dash, jump in the dash (the lighter-gravity window) |
| `slam_boost` | flat | jump, slam, land (the slide key still held: a slide), boost jump at 15 m/s (apex 4.86 m) |
| `wall_ride` | wall | ride starts on contact at 8.26 m/s and ends on its 1.2 s timer (144 steps) |
| `wall_jump` | wall | ride, then a wall jump 0.3 s in |
| `swing` | pit | ARCLINE over a 40 m gap: hook, attach (L 22.74 m), swing through the bottom, let go rising at z −48.7, land on the far platform at z −68.3 |
| `swing_from_ground` | flat | anchor too low for the floor: you land and the rope pays out on the ground until let go |
| `ramp_walk_up` | Spire | walk up the 20° ramp at (−5, −26.5) to its 2.91 m top, off the back, land |
| `ramp_slide_down` | Spire | slide down the same ramp (slope boost to 17.35 m/s), across the flat and up the facing ramp |

Not simulated in these runs: jump pads (`arena.checkPads` runs in `main.js`, outside the Mover), the kill plane, the
core's moving pillars (the Spire's core stays at its `flat` load preset), style and perk modifiers (the Mover's
defaults: `regenMult` 1, `walkMult` 1, no perk bonuses).

## D. maps/

For `spire`, `training` and `survival`: `createArena(new Scene(), config).load(name)` (`src/world/arena.js`).

**`collision`**

- `boxes`: `[floor, ...map.boxes]`, the floor slab arena.js adds plus the map's solids, as
  `{min: [x, y, z], max: [x, y, z], kind}`.
- `barriers`: invisible walls and caps, same box shape.
- `wedges`: 20° ramps `{x, z, yaw, length, width, rise}`. A wedge rises along (−sin yaw, −cos yaw) from its low-end
  centre and has 6 triangles.
- `triangles`: `{count, octreeBounds, flat: [ax, ay, az, bx, by, bz, cx, cy, cz, ...]}`, every distinct triangle in
  the built static Octree.
- `droppedByOctree`: the box triangles three's Octree does **not** hold, measured, with `allOnMaxPlanes`. Those faces
  never collide in the game:
  - Spire 24, Training 14, Survival 28.
  - All are faces lying exactly on the Octree's max-x / max-z bounds: the outer end faces of the perimeter walls, and
    on Training the floor slab's +x and +z sides.
  - Cause: `three/addons/math/Octree.js` `calcBox()` pads only the min corner of its bounds, so `split()` can put
    such a triangle in no child box.
  - They face out of the map. A port that builds colliders from `boxes` gets those faces back, which should only
    matter outside the playable area.

**`core`** (Spire and Survival): the "Grinder".

- `spec`: `{x0, z0, cells, cellSize}`.
- `presets`: pillar heights from the real `presetHeights` for `flat`, `stairs`, `ring`, `towers`, `random`,
  `platforms`, `pen` and `pillars`. `random` uses `mulberry32(1)`.
- A pillar is the box `[x0 + k·cellSize, 0, z0 + r·cellSize]..[+cellSize, h, +cellSize]` (row-major, row 0 at the
  min-z edge). A pillar with h ≤ `config.core.minHeight` doesn't collide.
- The core is a separate live collider, not in `triangles`. It tweens on the fixed step and carries bodies.

**Other data**

- `spawns`: `playerSpawn`, `duelSpawns`, and the enemy spawn points `ground` and `high`.
- `pads`: with `target` and `apexAbove` for aimed pads.
- `grappleNodes`.
- `zoneParts` (Survival only): `resolveZones(map.zones, map.zoneHoles)`, 114 disjoint box parts for the 6 zones
  (`landing` is the start zone). In the game a locked zone's parts become solid boxes in the collider at run time
  (`survival/zones.js`, the `gates` layer). They are not in `triangles`.
- `data`: the whole map object, including armory, perk, trap and gadget spots.

**`navGrid`**: the Survival nav grid built exactly as `main.js` does:
`createNavGrid(config, {getCollider: () => arena.octree.geometry, tagAt}).build(map.floorSize, {sync: true})`, core at
`flat`.

- Nodes are parallel arrays `x, z, y, clear, tag, edge, col, blocked`.
- Links are CSR: node i's links are `linkStart[i]..linkStart[i+1]-1`, with `linkTo`, `linkKind`
  (`walk 0, jump 1, drop 2, gap 3`) and `linkCost`.
- Survival's tags come from `zoneIndexAt(parts, x, y + 1, z)`, which is exactly what `survival/zones.js` `tagAt()`
  does. That file can't load headless (canvas signs), so the same pure call is made directly. The other maps have tag
  −1 everywhere, as in the game.
- `blocked` is all 0 at build. The game masks locked zones by tag at run time.

**`duelNavGraph`** (Spire only): RIVAL's graph, built exactly as `src/nav/navWorker.js` does it (core locked to
`towers`, a nav grid on that, then `buildNavGraph`).

- `nodes` and `edges`. `edges[i]` holds node i's out-edges `{to, cost, kind, ...}`.
- `stats`: 38,554 simulated flights.

## Skipped, and why

- **Nothing was blocked from running headless.** The Mover, the arena, the Octree, the nav grid and the Duel nav graph
  all run in plain Node (three.js math only, no WebGL).
- **`survival/zones.js` was not loaded.** It draws price signs on a canvas (`document.createElement`). The two things
  it contributes to the data are made by calling its pure dependencies with the same inputs: `resolveZones` for the
  parts and `zoneIndexAt(parts, x, y + 1, z)` for the nav tags.
- **Not in the trajectories:** jump pads, the kill plane, moving core pillars, perks, mutators, style-rank stamina
  regen, and ADS walk speed. These all live outside `Mover.step` (in `main.js`, `arena.js` or `player.js`).
- **Pure modules not vectorized** (they were outside the requested list): `regenMath`, `pointsMath`, `bossMath`,
  `zoneMath`, `barrierMath`, `qualityMath`, `kicks`, `ai/tokens`, `ai/slots`, `ai/aim`, `ai/crowd`, `ai/flowField`,
  `ai/watchdog`, `nav/reach`, `nav/astar`. They can be added to `scripts/vectors.mjs` the same way.
- **6 vector calls use an injected callback.** These are line-of-sight stand-ins in `pickRicochetTarget`,
  `chainTargets`, `planSquads` and `pickBest`. The callback's rule is written in `$fn.$callback`, and a port must
  supply an equivalent function to replay them.
