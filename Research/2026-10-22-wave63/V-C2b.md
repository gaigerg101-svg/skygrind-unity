# NIGHT63 builder V: C2b, the near fade as a deliberate see-through (branch n63-C2b from port/stage3 at 770278bb; row design.n63.nearFade63, OFF)

Shas, in order:
- `2fe8157d`: test first (RED). N63ScreenDoorTests, plus NearFade63 placeholders.
- `08a79c77`: the screen-door (GREEN). The shader, the hook, the numbers and their row script.
- This notes commit.

Cherry-pick: `git cherry-pick 2fe8157d^..<notes sha>`.

## The red (read, not measured by me)
Logs/n63/verdict-C2.md section 6, the critic on n63-b5's near63 frames:
- At 0.5 m and 0.3 m the husk "reads as noise ... an exploding cloud of sheared flakes".
- The cells are skewed quads of many sizes, and "a large share are BLACK".
- With kill55 on, a live enemy at the eye may read as a kill shattering.
- The 1.0 m pair moved 63,402 px at dissolve 0.99, so the fade already shows at the edge of the punch range.

## The causes (read in Assets/Shaders/Enemy.shader)
- **The flakes:** C2 drove the death's own `_Dissolve`. That dissolve cuts 3D noise cells of 2.5 cm, floored in world space, so on a curved
  capsule they project as sheared quads of every size.
- **The black:**
  - The ink51 outline is an inverted hull drawn with `Cull Front` in dark ink, so the faces it draws are the hull's far side.
  - Wherever C2 cut a hole in the body, the hull's far side behind it showed through, dark.
  - This is a reading of the shader, not a probe. The critic's "inner / back faces drawn dark" matches it.

## The change (view only; no sim, ray or hitbox change)
**Enemy.shader: a screen-door of its own.** `_NearFade` is a per-part property in the per-material buffer, default 1. At 1 the shader
returns before any cost, so with the row off or the part far away, nothing changes.
- **The pattern:**
  - `NearDither` is a 4 x 4 ordered (Bayer) dither in screen pixels, `_NearFade63.x` px a cell at 1080p, scaled to the screen height.
  - A pixel is kept when its threshold `(m + 0.5) / 16` is at or under the level.
  - Every 4 x 4 tile keeps exactly round(level x 16) cells, spread evenly: each 2 x 2 quarter is within one of a quarter of them.
  - So the near body reads as an even see-through screen, and no cell is bigger than another.
- **The rim kept:** a body pixel whose N.V is under `_NearFade63.y` is never cut. The silhouette edge stays solid as a thin rim, so you still
  read where the enemy is.
- **No black:**
  - The ink pass is cut by the same screen-door at the same pixel as the body. Where the body has a hole, the ink behind it has one too, so
    its dark far side never shows.
  - The outline also thins with the level (`InkNearK`) and is gone at `_NearFade63.z`. From inside the hull (the eye in the body) nothing
    dark draws.
- **The rest:**
  - The depth and depth-normals passes cut the same way, so SSAO matches what is drawn.
  - The x-ray pass (another enemy seen through this one) is untouched.
  - The death's `_Dissolve`, its edge band and its edge colour are left to the kill.
- **`Game/NearFade63.N63V.cs`:**
  - `Bayer` / `Kept` are the shader's formula in C#; the tests pin them.
  - `Globals` gives the shader's `_NearFade63` (cellPx, rimNdv, inkGone).
  - The level function (`Dissolve`) is unchanged: smoothstep from farM to the floor at nearM.
- **The hook (`EnemyViews.cs`):**
  - `EnemyLook.Tint` sets `_NearFade` on the part's shared block every time. It no longer sets `_Dissolve` or `_DissolveBand`.
  - `EnemyLook.Frame` sets the globals once a frame while the row is on.
  - D1's contract is as before: the row and the eye are read every frame, and there is no per-part allocation.
- **The numbers:** `Tools/night/n63-v-c2b-rows.cjs` rewrites only the row's `_why` and `numbers`.
  - `enabled false` and `enabled_why` are untouched.
  - Both copies are byte-identical, and nothing else changed (the script asserts it).

| number | C2 | C2b | why |
|---|---|---|---|
| farM | 1.0 | 0.9 | 1.0 m is exactly whole. Every cell is kept down to 0.83 m (level 31 / 32). The punch reaches 2.0 m. |
| nearM | 0.3 | 0.3 | |
| floor | 0.1 (of 3D cells) | 0.2 | 3 of 16 cells at the eye, plus the rim. |
| cellPx | | 2 | An 8 px tile at 1080p. |
| rimNdv | | 0.25 | About the outer 3 % of a sphere's radius. |
| inkGone | | 0.45 | The outline is gone at 0.52 m from the bounds. |
| bossHideM | 0.6 | 0.6 | THE ICEBREAKER: unchanged. |

The levels the screen-door keeps at the near63 placements, computed from the numbers (the reel's gap is to the capsule's bounds, so a
placement's level is lower than the table's figure for its nominal metre):

| from the bounds | level | cells kept |
|---|---|---|
| 0.3 m | 0.20 | 3 / 16 |
| 0.5 m | 0.41 | 7 / 16 |
| 0.7 m | 0.79 | 13 / 16 |
| 0.9 m and 1.0 m | 1 | all |

## The tests
**N63ScreenDoorTests (new), 4 tests:**
- The pattern:
  - each of the 16 thresholds appears once per tile, and the tile repeats;
  - for every level k / 16, a tile keeps exactly k cells, and each 2 x 2 quarter is within one of k / 4;
  - 31 / 32 and 1 keep every cell, and 0 keeps none.
- The repo's numbers:
  - 1.0 m from a husk's bounds is exactly 1;
  - the floor is in [0.15, 0.5], cellPx in [1, 4], rimNdv in (0, 0.5], inkGone in (floor, 1);
  - Globals carries them.
- The shader (a source scan):
  - `_NearFade` is a property and is in the per-material buffer;
  - NearClip uses NearDither and never the death's dissolve;
  - the body, depth and normals passes are cut with the rim kept, and the ink pass is cut without it and thinned;
  - the x-ray pass is not cut.
- The hook: Tint sets `_NearFade` (not `_Dissolve` or `_DissolveBand`), and Frame sets the globals.

**N63NearFadeTests: one assertion changed (for the slate, as D-6 was).** The hook test asserted
`block.SetFloat(Dissolve,`, which was C2's mechanism and the one the critic failed. It now asserts
`block.SetFloat(NearFade63.NearFadeId, near63)`. The header comment changed with it.

**RED:** red by construction on `2fe8157d`, which contains:
- the placeholders: Bayer 0, Kept false, Globals 0;
- no cellPx / rimNdv / inkGone in the config;
- no `_NearFade` in the shader;
- the old hook.

Each of the 5 tests asserts at least one of these. These tests cannot run outside Unity: NearFade63's `Shader.PropertyToID` fields need
the engine. The Editor run is the lead's.

**GREEN:**
- dn-check: 0 errors on both commits. gates: all GREEN 01:30:23.
- The new HLSL functions compile stand-alone with Unity's own `D3DCompiler_47.dll` for ps_5_0, vs_5_0 and ps_4_0. The functions were taken
  from the shader file by a script, with stub globals.
- A planted undeclared name in that check fails with X3004.
- The whole shader was not compiled outside Unity. Its call sites only use existing fields (`i.positionCS`, `i.positionWS`, `i.normalWS`,
  `ndv`).

## doneWhen for the re-measure (proposed; the lead's call)
1. **SPOT:** the near63 same-run pair at `shot-N30-N63-near-0.5.png` changes >= 41,472 px. The SPOT names a see-through or screen-door
   enemy, not "broken apart" or "a cloud of pieces".
2. **NO BLACK:** on the near-0.5 and near-0.3 ON frames, the frame's dark share (luma < 0.08, the critic's rule) is no higher than its OFF
   twin's plus 0.01. C2's black flakes would raise it.
3. **EVEN:** the critic reads the 0.5 m and 0.3 m ON frames as an even screen with the enemy's edge kept, not flakes.
4. **1.0 m WHOLE:** the near-1.0 pair changes no more than D1's noise band. b5's d1noop measured 0-8 px; C2 measured 63,402.
5. **Far controls:** near-1.5 and near-3.0 at 0 px.
6. **The verdict:** the `near63` verdict GREEN with the row on, and OFF on a run with `-skygrindNoNearFade63` (the lead said he will run
   it).
7. **Nothing lost:** verbs37 x5 unchanged (27 / 29 / 31 / 31 / 31). The Player log has 0 shader errors for SKYGRIND/Enemy, and no enemy is
   drawn magenta.

## The Player steps
1. Build, then `node Tools/night/n46-build-rows.cjs <tag> on n63.nearFade63`.
2. near63, same run: `-skygrindSameRun63 n63.nearFade63` on a near63 side (n52-foundry). Then a near63 run with `-skygrindNoNearFade63`
   for the OFF verdict.
3. `python -I Tools/night/n63-v-near.py --pair <on>/shot-N30-N63-near-<m>.png <off>/...` for m in 0.3, 0.5, 1.0, 1.5 and 3.0: the changed
   px and the enemy-hue share. Add the dark share for doneWhen 2.
4. A contact sheet for GG: `python -I Tools/night/n63-sheet.py --row nearFade63 --pair ...`.
5. verbs37 x5.

## Not checked
- Any Player or Editor frame of the screen-door, and how it reads under bloom, SSAO and the anti-aliasing.
- The whole Enemy.shader compiled by Unity.
- kill55's shatter next to a screen-doored live enemy.
- Every enemy kind one by one. They all tint through EnemyLook.Tint, as in C2.
- The critic's "alpha 0" field on the c2n toggle lines.
- THE ICEBREAKER's half: the lead re-runs it with `-skygrindNoBossMotion63`. C2b does not touch HideNear or bossHideM.
