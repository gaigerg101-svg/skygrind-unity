# NIGHT63 builder V: exit63, the doorway itself is the way out (view row design.n63.exit63, OFF; branch n63-EXIT from port/stage3 at 5e467514)

GG 2026-10-13: doors "not visible now and you just run through it and it's like dark". Tonight's P1 (exitFrame61) was SHORT: the cyan frame's own
gain on the door was 1.07-1.56x and reached 1.5x in 1 of 5 rooms, never 4 of 5 (Logs/n63/p1-owngain-b3.txt), and a brighter frame lit the
player's gun and broke the GUARD. DECIDE63 D-6 asks for "a door that changes colour" rather than more glow.

## doneWhen (written BEFORE any Player run; the lead copies it to the slate)
1. **OWN GAIN (MAIN):** on n57-kiln's a157 clear shots (`-skygrindClearShots60`), the door's luma on / off (`n60-v-lamp.py`'s OWN GAIN
   line; exit63 on vs off, the same-run pair `-skygrindSameRun63 n63.exit63`) is **>= 1.5x in >= 4 of 5 rooms**.
2. **GUARD:** the room's p95 luma outside the door box is within 0.02 of off in **5 / 5 rooms** (the lamp meter's GUARD). exit63 uses no
   light: it is the doorway's own geometry, so nothing lights the room or the gun.
3. **ONE RISE:**
   - one Player log line `[exit63] door lit: room <id> (door <door>, frame <n>)` per exit a clear opens on the a157 route, and 0 with the row
     off;
   - the drawn level rises once over numbers.riseS and holds (monotone, never above 1, no flash, no blink: pinned by N63ExitTests).
4. **SPOT:** the same-run pair at the **a157 n57-kiln kb3 clear shot** (`shot-N29-L57-n57-kiln-kb3-clear.png`) changes >= 41,472 px, and the
   SPOT names the door or the way out.
5. **verbs37 x5 unchanged** (27 / 29 / 31 / 31 / 31).
6. **Off exact:** with the row off (its config, `-skygrindOff63` or older, `-skygrindNoExit63`) nothing is built or drawn.

## The design (before the code)
When a room's clear opens its exit, the doorway turns the player-usable cyan (CLAUDE.md rule 8) from inside. It is drawn by the door's
geometry, never a light, and lies wholly inside the door's passage, so it falls inside the meter's box and outside the GUARD's frame:
- **The veil:** an additive cyan sheet filling the opening just inside room A's face of the passage. The opening itself reads cyan.
- **The liners:** the passage's own surfaces for its first metres (both jambs and the lintel, the doorway's inner faces) lined in cyan.
- **The threshold and the arrow:** a lit strip across the threshold just inside the face, and dashes along the passage floor toward room B.
- **One rise:** smoothstep over numbers.riseS from the open (FeelClock.UnscaledDelta: 0 in a same-run held frame), then held. Dark while
  locked; the same lit-once rule as exitFrame61 (a door open since the map was read stays dark).
- **The row:** read every frame (NightRows.ViewOn: D1's override); off hides every part and keeps each door's state.
- **Every gain is 1 or under,** so no bloom halo spills past the door's box.
- **n57-kiln only,** as exitFrame61 and the meter: campaign doors carry no floor height (kiln's stand on y 0).

## Correction to doneWhen 4, written before any Player run (and a prediction for doneWhen 1)
**SPOT (replaces 4 above):** the same-run pair at the **a157 n57-kiln kb3d exit63 shot** changes >= 41,472 px, and the SPOT names the door
or the way out.
- The shot is `shot-N29-L57-n57-kiln-kb3d-exit63.png`, from a157 `-skygrindClearShots60 -skygrindExitShots63 kb3d`.
- The pose: the feet on d7's axis 10 m into kb3d from its face, at (0, -267.5), looking at the door (web yaw 0).
- Why the change: the kb3 clear shot cannot pass rule 8. Its door box is 130 x 134 = 17,420 px on b3 (p1k's sidecar json), under 41,472
  even if every pixel in it changed. exit63 draws only inside the passage, so only that box can change. All five clear boxes are
  15,004-37,810 px.
- At 10 m the 4 x 4.5 m opening covers about 92,000 px (camera.fovDeg 74 vertical, 1080p). That is arithmetic, not a measurement.
- The spot is clear of every box and ramp in the level file (read, not walked), and the door is not occluded from it.

**A prediction for doneWhen 1 (the MAIN), before any Player run: likely SHORT on the clear poses whatever the doorway draws.** Read from
b3's p1k-off clear frames (exitFrame61 off; Doors38's open curtain on) and n57-kiln's collision:
- **kb3, d6: mostly hidden.** A 4.5 m ramp (x -5..3, z -222.5..-219.5) stands 3-6 m in front of the face. In the frame, Doors38's posts and
  exitFrame61's frame are cut along its diagonal.
- **kb2c, d5: mostly hidden.** A 4.5 m ramp (x -7..1, z -184.5..-181.5) stands 9-12 m out. Only a sliver of the curtain shows at the right.
- **kb3c, d6b: hidden.** A 2.5 m ledge (x -5..1, z -243.5..-237.5) and the ramp at x -4..-1 stand between the entry spot and the door. The
  door's box shows a dark face with a cyan edge on top.
- **kb2 (d3) and kb3d (d7): visible, but already lit.** Doors38's open curtain is there; the off opening's luma is 0.496 and 0.573, the box
  mean 0.450 and 0.538.
  - 1.5x needs a box mean of 0.675 and 0.807.
  - A simulation painted additive cyan over the WHOLE opening of the off frame (sRGB-linear add, no tonemap). Its upper bound at gain 1.0
    reaches 1.49x and 1.31x: short even with a full-strength sheet.
- Upper bounds from that simulation at veilGain 0.4 (run at 0.4), if every opening were fully visible: kb2 1.26x, kb2c 1.76x, kb3 1.58x,
  kb3c 2.38x, kb3d 1.17x. kb2c, kb3 and kb3c are not fully visible, so their real values will be lower.
- So the expected result is 0-1 of 5, against >= 4 of 5. The cause is the poses (occlusion) and Doors38's existing cyan, not the doorway's
  drawing.
- Measuring the same meter on the exit63 shots (the door in view at 10 m) would test the drawing itself. That is the lead's call.
  n60-v-lamp.py globs `-clear.png`, so it would need its sidecars copied under that suffix. Not done.

## Shas
In order, all on n63-EXIT from port/stage3 at 5e467514:
- `038f833d`: the doneWhen and the design, before any code.
- `885a0ccc`: test first (RED). N63ExitTests, 7 tests. The placeholders were Exit63Math and UnityTuning.Exit63ShotM 0. RED was measured
  outside Unity with my scratch runner on the dn-check build: 7 / 7 fail.
- `15b0f333`: GREEN. The row, Exit63Math, the Exit63 view, the exit63 shot and the row script.
- This notes commit.

Cherry-pick: `git cherry-pick 038f833d^..<notes sha>`. The config hunk appends exit63 after text63. If port/stage3 has rows after text63 by
then, take HEAD's copies and re-run `node Tools/night/n63-v-exit-rows.cjs` (L-065).

## What was built (view only; no sim, ray, collider or light)
- **`Game/Exit63Math.N63V.cs`** (pure): the parts in web space (veil, liners, strip, dashes), the rise (smoothstep), the veil's step aside
  (smoothstep from veilNearM 1 m to veilFarM 3 m), the switch, the lit line, and the exit shot's spot, yaw and switch.
- **`Game/Exit63.N63V.cs`** (the view), modelled on exitFrame61's lamp with these changes:
  - additive cubes on FxDraw's neon copy, cyan x gain x rise, all gains <= 1, no Light, no collider;
  - `NightRows.ViewOn` read in LateUpdate every frame; off hides every part and keeps each door's lit / rise state;
  - a map built while the row was off is built when it turns on;
  - the veil's level is re-set only when it changes, and the other parts only during the rise.
- **The numbers** (design.n63.exit63.numbers; Tools/night/n63-v-exit-rows.cjs):

  | number | value |
  |---|---|
  | riseS | 0.6 s |
  | veilGain / linerGain / stripGain / dashGain | 0.4 / 0.5 / 0.7 / 0.6 |
  | inset | 0.03 m |
  | veilT / linerT | 0.02 / 0.04 m |
  | linerDepthM | 4 (the whole of kiln's 4 m passages) |
  | threshold | 0.3 m deep |
  | dashes | 4, 0.5 m long, 0.25 m wide, 0.35 m gaps (the last ends 3.73 m in) |
  | veilNearM / veilFarM | 1 / 3 m |

- **`Game/FeelReel.N63V.Exit.cs` plus one hook line in `FeelReel.N57.Level.cs`:** the exit63 shot after each clear shot.
  - It runs only with both `-skygrindClearShots60` and `-skygrindExitShots63 [room]`. Without the switch a157 is unchanged.
  - Its sidecar json has the clear shot's fields plus the spot.
  - kb2c's spot falls in its ramp and kb3's door is behind its ramp, so shoot only kb3d for the SPOT.
- **Tests** (N63ExitTests, 7):
  - the switch;
  - the row and gains <= 1;
  - every part inside d3's passage, clear of its walls, floor and ceiling and never in room A; the veil >= 85 % of the opening, just
    inside the face; the jambs, the lintel, the strip, the dashes;
  - the rise, monotone and never above 1;
  - the lit line, plus a source scan for ViewOn in LateUpdate, FeelClock.UnscaledDelta and no Light;
  - the veil's step aside;
  - the shot's spot and yaw on two axes, the switch parse, the shot distance and a157's hook.
- **Measured outside Unity** with the scratch runner on the dn-check build:
  - N63ExitTests 7 / 7, N61ExitFrameTests 8 / 8, N63SameRunGameTests 7 / 7, N63R1Tests 2 / 2.
  - Core's N63SameRunTests and N46SharedTests cannot run outside Unity (an engine call): not checked.
- dn-check: 0 errors on every commit. Gates: all GREEN 01:40:21.

## The Player steps
1. Build, then `node Tools/night/n46-build-rows.cjs <tag> on n63.exit63`. Keep exitFrame61 and lamp60 off, as on b3.
2. a157 on n57-kiln, same run:
   `-skygrindMap n57-kiln -skygrindReelOnly a157 -skygrindClearShots60 -skygrindExitShots63 kb3d -skygrindSameRun63 n63.exit63`.
   - The five clear shots and the kb3d exit63 shot get their row-off twins in `<side>-off/`.
   - D1's toggle line will call exit63 "unclaimed" (it is not in SameRun63Math.FrameRows). The view does read ViewOn every frame, so the
     toggle works. Adding it to FrameRows is D's file: not done.
3. The MAIN and the GUARD: `python -I Tools/night/n60-v-lamp.py <on a157 dir> <on a157 dir>-off`, the OWN GAIN line and the GUARD line.
4. The SPOT: the kb3d exit63 pair, measured with the toggle line's px or n62-gun-yellow.py's > 24 rule. Then the SPOT read.
5. The proof: grep the Player log for `[exit63] exit63 ON on n57-kiln: doors 8` and the `[exit63] door lit:` lines. On a157 (doorFree)
   all 8 should light at once, early (exitFrame61's lit at frame 272 on b3). A run with the row off prints 0.
6. verbs37 x5.

## Not checked
- Any Player or Editor frame. Everything above about the look is design and arithmetic.
- Whether the veil plus liners plus strip stays under the bloom threshold where they overlap Doors38's scanlines. Doors38's scanlines
  already go over it.
- The GUARD in a Player (no Light is used, so nothing outside the box is lit by exit63; bloom halos not measured).
- Whether kb3d's 10 m spot is walkable (the level file read only), and the camera's exact eye height.
- How exit63 reads next to Doors38's open curtain and its shimmer when you walk through.
