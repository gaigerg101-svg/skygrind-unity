# NIGHT61 builder V: slate S1, the exit frame (design.n61.exitFrame61, OFF)

Branch `n61-S1`, from port/stage3 at fdf4d336. No Editor, no build, no Player and no reel were run in this worktree. Every number below
was read from a file, computed from the map's data, or printed by a command named here. Anything else is marked "not checked".

## Shas, in order

1. `6e67afd1`: the test first. `N61ExitFrameTests` (Game) and `N61ExitFrameRowTests` (Core), with pre-placed `ExitFrame61Math.N61V.cs`
   (types and signatures; every answer a placeholder) and `UnityTuning.N61V.cs` (the numbers the tests read). Expected RED: every test
   except the "row on, no switch" line.
2. `67f5b873`: `Tools/night/n61-v-rows.cjs` and the row in both config copies (byte-identical). Expected after it:
   `N61ExitFrameRowTests` GREEN, `N61ExitFrameTests` still RED.
3. `745da143`: the code. `ExitFrame61Math` filled in and `ExitFrame61.N61V.cs` (the component) added. Expected: both test classes GREEN.
4. This notes commit.

## The red (read only)

**From `Logs/n60/b8a2-lamp.txt`, as written** (lamp60 on vs `-skygrindNoExit58`, Builds/n60-b8, a157 `-skygrindClearShots60`):

| room (door) | ON door luma / room median | OFF ratio | the lamp's own gain (door luma on / off) | p95 outside on / off |
|---|---|---|---|---|
| kb2 (d3) | 0.516 / 0.190 = 2.72x | 2.40x | 1.14x | 0.469 / 0.469 |
| kb2c (d5) | 0.277 / 0.183 = 1.51x | 1.49x | 1.02x | 0.482 / 0.483 |
| kb3 (d6) | 0.368 / 0.229 = 1.61x | 1.42x | 1.14x | 0.480 / 0.478 |
| kb3c (d6b) | 0.197 / 0.232 = 0.85x | 0.80x | 1.06x | 0.501 / 0.502 |
| kb3d (d7) | 0.647 / 0.222 = 2.92x | 2.60x | 1.14x | 0.539 / 0.533 |

The file's summary line: "LAMP: MAIN (door / room median >= 1.5x) 4 / 5 rooms; GUARD (p95 outside within 0.02 of off) 5 / 5". The slate's
red is the own gain: 1.02-1.14x in 5 of 5 rooms. Every row's lamp field reads "exit lamps ON on n57-kiln: gated doors 8, lit 8; last lit
d7 at frame 272".

**The kb3 door, from `Logs/feel/n60-b8/b8on/a157/shot-N29-L57-n57-kiln-kb3-clear.png`**, opened and compared with
`b8off/a157`'s same shot. I measured nothing in these frames. The box is read from the sidecar `.json`: `[895.0, 451.4, 130.0, 133.6]` px
of 1920 x 1080. What I see:

- The door d6 sits at the frame's centre under the "25 m" waypoint marker. Its opening is a small rectangle, dark navy-violet inside, with
  thin cyan bars along its top and its left side.
- A dark diagonal surface crosses in front of the door's right half. The map data puts it there: kb3's gallery ramp, a wedge across
  z -222.5..-219.5, 3 m in front of the door and about 2.8 m high on the door's axis.
- The brightest cyan thing in the frame is not the door. It is a vertical cyan light beam with a ring on the floor, left of the door (around
  x 810-860 px), taller and brighter than the door.
- The OFF frame looks nearly the same to my eye: the same thin cyan edges, the same dark opening. In the ON frame the top bar reads a
  little thicker. Both frames show "DOOR OPEN" at the top.

## The cause (read from the code and data, not measured in a Player)

- **Doors38View already lights every open passage** (it is on through `design.n41.doors41On`). It draws a cyan veil at the passage's middle
  (alpha 0.16, up to 0.35 far away), thin cyan edges (0.09 m, at least 4 px on screen), a cyan point light at the passage's middle
  (intensity 3, range 7) and one 5 m beyond (2.2, range 14). The opening still reads dark in the frame above. Stone.shader lights the
  stone as albedo x (ambient + Lambert x the point lights), and the stone is dark. So a 3-intensity cyan light adds little luma.
- **lamp60's frame was thin.** Its bars are 0.22 m (about 6 px at 25 m), standing 0.15 m out from the opening. By geometry they cover
  roughly 14 % of the meter's box. That is consistent with a 1.02-1.14x gain, but it is an estimate, not a measurement.
- **The meter** (`Tools/night/n60-v-lamp.py`): MAIN is the mean luma inside the sidecar's box. The box is the opening grown by
  Exit58FrameOut + Exit58FrameW (0.37 m) on each side and on top. The GUARD is the p95 luma outside the box, within 0.02 of off.

## The redesign

All of this lives in `Game/ExitFrame61.N61V.cs` and `Game/ExitFrame61Math.N61V.cs`, and all of it is view only. `Exit58.N58V.cs` and its
numbers are untouched.

On n57-kiln, each gated campaign door that does not lead into the boss room gets the parts below (8 doors: d1, d2, d3, d4, d5, d6, d6b, d7).
They appear the moment the door opens after being seen locked, which is the room's clear.

- **The frame.** Two posts and a lintel of emissive cyan bars, `ExitFrame61BarW` 0.37 m wide from the opening's edge outward. That fills
  exactly the band the a157 box counts as the door; a test pins that the bars never get wider than the band. They stand 0.02 m in front of
  room A's face of the 4 m passage, at an HDR glow of 4 (cyan x 4).
- **The passage light.** A point light, intensity 12 and range 4.8 m, with no shadow. It hangs 0.35 m under, and 0.3 m on room A's side of,
  a lit gate. The gate is two posts and a lintel, 0.25 m wide, just inside the passage's far mouth: it is the light's fixture (PRINCIPLES 7),
  and from room A it frames the passage's end inside the opening. On d6 the light is 5.54 m from room A's floor at the box's edge, past its
  4.8 m range, so room A's floor outside the box takes none of its light. That is the GUARD's reason; a test pins it.
- **The floor line.** Dashes 0.2 m wide, 0.8 m long with 0.4 m gaps, lifted 0.03 m. They run along the door's axis from room A's floor,
  through the doorway, to the far mouth, at glow 2 x a ramp from 0.3 at the room end to 1 at the door. The line stops 0.75 m short of the
  first thing on its strip (a low solid, a ramp, a floor at another height, lava), taken from the level's own data. Runs on n57-kiln:

  | door | run (m) | what stops it |
  |---|---|---|
  | d1 | 6.25 | kb1's ledge at 7 m |
  | d2 | 5.75 | ledge at 6.5 m |
  | d3 | 4.25 | kb2's 0.9 m step at 5 m |
  | d4 | 10 | nothing (the cap) |
  | d5 | 8.25 | kb2c's far ramp at 9 m |
  | d6 | 2.25 | kb3's gallery ramp at 3 m |
  | d6b | 10 | nothing (the cap) |
  | d7 | 6.25 | the breach (its floor 0.8 m down) at 7 m |

- **Timing.** One rise over 0.35 s, in real time; never a flash. The parts hold until the door locks again, which Campaign24 does behind the
  player as the next room is entered. A door that was already open when the map was read stays dark (Exit58's rule).
- **Off.** The row off; `-skygrindOff61` (or older); `-skygrindNoExitFrame61` (this row alone); or `-skygrindNoExit58` (no exit lamp at
  all). The last one is the lead's call, accepted: it means the slate's "on vs `-skygrindNoExit58`" measures this row as written, in a build
  where lamp60 is off. With lamp60 off, `-skygrindNoExitFrame61` gives the same off frames. Turn this row on alone: with both rows on, both
  draw.
- **Scope.** Only n57-kiln, the one level the slate measures. The Level constant gates it; other levels are not checked.
- **Numbers.** All Unity-only, in `UnityTuning.N61V.cs`, one comment each. There is no gameplay number and no sim row, so the config row's
  `numbers` is empty.

## The proof lines (L-089 / L-014)

- When the row is on and the map builds the frames: `[exit61] exit frames ON on n57-kiln: doors 8, lit 0`.
- Each time a door lights: `[exit61] door lit: room <id> (door <door>, frame <n>)`.
- When the row is off, nothing is logged, so an off reel has 0 `[exit61]` lines. That is the planted RED beside the on reel's GREEN.
- In a157 (DOORFREE) all eight doors open at the start. Expect 8 "door lit" lines then (lamp60's line on n60-b8: "lit 8; last lit d7 at
  frame 272").
- The a157 clear-shot line's `lamp` field prints Exit58.Line (lamp60). With only this row on it will read "exit lamps OFF (design.n60.lamp60
  off)". That is lamp60's state, not this row's.

## Checks run here

- `dn-check-wt.sh .../scratchpad/chk-V`: 0 errors in Skygrind.Game, Skygrind.Game.Tests, Skygrind.Core.Tests and Assembly-CSharp-Editor,
  at the test-first state (12:14; the new classes were in the built DLLs) and the code state (12:20).
- `bash Tools/night/n61-gates.sh --no-editor`: "gates: all GREEN 12:25:19". Config copies identical; golden 39 vector sets, 22 replays
  exact, 0 mismatches; fingerprint plain and `--off57`..`--off61` EQUAL.
- A scratch copy of the two test classes' checks, compiled outside Unity the way `Tools/CoreHarness` is (C# 9, the real `ExitFrame61Math`
  and `UnityTuning` files): 168 / 168 passed on `745da143`'s code. It is deleted now. It is not the EditMode run.
- `node Tools/night/n61-v-rows.cjs`: the dry run, the write, and a refused second run.

## Not checked

- The EditMode tests in the Editor: RED on `6e67afd1`, GREEN on `745da143`. That run is the lead's.
- Anything in a Player: the frames, the meter, the GUARD, the SPOT, verbs37, NREs, and how the frame, gate, light and line actually look.
- Whether the doneWhen can be met. Arithmetic on the file's numbers (ON luma / own gain) puts the OFF door lumas near 0.45 (kb2), 0.27
  (kb2c), 0.32 (kb3), 0.19 (kb3c) and 0.57 (kb3d). A 1.5x gain then needs ON door lumas near 0.68, 0.41, 0.48, 0.28 and 0.85. I expect
  kb3d and kb2 to be the hardest, and kb3's ramp hides part of its door from the entry. Four of five may not be reached; not checked.
  If MAIN misses, the levers are in `UnityTuning.N61V.cs`: `ExitFrame61Glow`, `ExitFrame61LightIntensity`, and the gate's width. The
  frame cannot grow past the 0.37 m band without leaving the meter's box.
- Whether a157's DOORFREE lets the component see the doors locked before it opens them. If it did not, the frames would stay dark in a157.
  lamp60 uses the same rule and lit 8 at frame 272 on n60-b8, so this should hold.

## The lead's next steps (Player)

1. **Pick and run the tests.** Pick `6e67afd1` and run `N61ExitFrameTests` and `N61ExitFrameRowTests` (RED). Pick `67f5b873` (RowTests
   GREEN). Pick `745da143` (both GREEN).
   - If the config conflicts with another builder's n61 row, take the merged copies and re-run `node Tools/night/n61-v-rows.cjs`. It
     appends after whatever design.n61 holds and asserts nothing else changed.
   - Then run `bash Tools/night/n61-gates.sh`.
2. **Build:**
   ```
   bash Tools/night/n41-build.sh <tag>
   git diff --stat ProjectSettings
   node Tools/night/n46-build-rows.cjs <tag> on n61.exitFrame61
   ```
   lamp60 stays OFF.
3. **Reels:**
   ```
   bash Tools/night/n61-reels.sh <tag> s1on,n57-kiln,a157,-skygrindClearShots60 s1off,n57-kiln,a157,-skygrindClearShots60,-skygrindNoExit58
   ```
   Plus verbs37 x5, as the night runs it once per build.
4. **Meter:**
   ```
   python -I Tools/night/n60-v-lamp.py Logs/feel/<tag>/s1on/a157 Logs/feel/<tag>/s1off/a157
   ```
   Its planted cases: `python -I Tools/night/n60-v-lamp.py planted <a157 dir>`.
5. **Proof:**
   ```
   grep -c "\[exit61\] door lit: room" Logs/player/n47-reel-<tag>-s1on.log
   grep -c "\[exit61\]" Logs/player/n47-reel-<tag>-s1off.log
   ```
   Expect 8 for the on reel and 0 for the off reel. Also grep `"[exit61] exit frames ON on n57-kiln: doors 8"` in the on log.
6. **SPOT:** the kb3 clear frames, on and off, as A and B.
