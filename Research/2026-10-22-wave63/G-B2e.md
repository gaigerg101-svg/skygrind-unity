# NIGHT63 builder G: B2e, the lead's three B2 questions on n63-b9 (branch n63-B2d, after B2d)

Builder G never ran the Editor, a build, a Player or a reel. Read: Logs/player/n47-reel-n63-b9-b2{bk,bp,k,p}.log, the b9 wyrm tell frames
(Logs/feel/n63-b9/b2k-on|b2k-off/boss63/shot-N30-B63-wyrm-tell.png and the on stagger frame), FeelReel.N63V.cs, FeelReel.N63G.cs,
FeelClock.cs, BossTell63.cs, WyrmRing43View.cs.

## 1. crownFire: the meter now prints the earliest danger-hue cue on its own (test-first)
- **Why the line never named the column:** the cue's hue is the strongest riser's. The clapper (Crown47WhiteHex x 2.4, hue 41, saturation 0.14)
  rises far more than the column (level 0.6). A test planting b9's case gives that result on today's meter: the clapper is the strongest
  riser, danger no, verdict off-hue.
- **The change:**
  - `Cue63.dangerCue*`: from the wind-up, the first frame with a danger-class riser, its start and lead, and that frame's strongest danger
    riser's hue and id.
  - The cue line appends ` | danger cue start <ms> lead <ms> hue <deg> obj <name>`, or ` | danger cue none`.
  - The verdict is still the strongest riser's (not loosened). Every earlier field and the fix-list tool's parse are unchanged.
- **Commits:** RED 2935740c (N63BossCueTests 13 / 14), GREEN cbffe7b8 (14 / 14; N63BossTellTests 9 / 9; `--selftest` PASS; gates GREEN 02:37:54).
- **With B2d (9d3e74f0)** the clapper itself is pulled to danger during crownFire, so the verdict can read ok. The new field shows the column's
  own lead either way.

## 2. The wyrm's tell pair, 8 px: the column is off screen at that frame (read from the frames)
- The held ON and OFF frames at shot 5 look the same: the camera faces a dark block at close range, with the KILN WYRM bar, the gun and the
  room to the right. Neither shows a red tube.
- The grainy red border is in both frames and in the stagger shot (no column there), so it is the game's hurt vignette, not the column.
- The column did draw: the log says "column drawn", and the spit's cue line names "KILN WYRM/tell63 cue" as its strongest riser. The meter
  scans renderers regardless of the camera.
- So the 8 px is the pose, not the OFF half still drawing the column. A tell frame that sees the column needs a camera aimed at it; the reel's
  camera aims at the boss's weak point.

## 3. wyrm aggro's 33 ms (13033 -> 13000; one frame at the reel's 30 fps). Not the row, not the cadence.
**Ruled out by the code:**
- **The row:** aggro is in `UnityTuning.BossTell63Skip` ("wyrm:aggro"). So in `BossTell63.Draw`, `show` is false: the column's renderer is
  disabled, and `ActiveFix` is cleared for that root. The late pull (BossTell63Late) only runs while `ActiveFix` is live. Nothing is recoloured
  during aggro.
- **The cadence:** the boss63 loop skips a held frame whole (`if (FeelClock.Holding) { yield return null; continue; }`, FeelReel.N63V.cs:127).
  Each sample's ms is `(FeelClock.Now - riseAt) * 1000` (:129). `FeelClock.Now` counts frames through `FrameCount`, which skips held frames
  (FeelClock.cs:64, :113). So the hold leaves the sample grid as it was. This matches the identical attack lines (windup 12875, hit 13408).

**What the riser is:**
- The aggro cue's object is "NIGHT43 THE RING/ring43 throat", one of the ring's furnace mouths on the shared lava material (WyrmRing43View.cs:307).
- Its colour only breathes (`t += Time.deltaTime`, 0.82-1.0 of its glow, :416-418). That cannot pass the meter's rise threshold of 1.3x its
  reference (arithmetic: 1.0 / 0.82 = 1.22).
- It can rise by becoming VISIBLE: `Boss63SnapG` counts a renderer drawn only when `r.isVisible` (FeelReel.N63G.cs). A throat entering the
  camera's view goes from not drawn to its full glow, a riser.
- So aggro's "start" is the frame the reel's camera, turned to the wyrm each frame, first sees that throat. b2bk and b2k are two separate
  processes (the before has no same-run option), and where the camera points can differ by a frame between runs (two-run drift, L-129). The
  logs do not print per-frame visibility, so this exact cause is **not confirmed**.

**To decide it (not done):**
- (a) Two before-runs with the same flags: if aggro's start also moves by a frame, it is drift.
- (b) Have the meter not count a renderer entering the view as a rise. That is a meter change: the lead's call.

Either way, aggro's attack timing and its verdict are the same in both runs (ok, lead 375 / 408 ms).
