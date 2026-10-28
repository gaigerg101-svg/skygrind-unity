# NIGHT63 builder D: eye63, THE EASED EYE (DECIDE63 D-10, M1's proposal 2)

Branch n63-EYE, from port/stage3 3df29537. These notes are in their own file so they never clash with D.md at a pick.

## Commits, in order
Cherry-pick `4422f7c8^..<this notes sha>`.

1. `4422f7c8` **The tests, committed RED.**
   - What it adds: N63EyeTests, with Eye63Math / Eye63Follow as placeholders (today's path, never on), and the numbers in
     UnityTuning.N63Eye.cs.
   - Result outside Unity: 5 of 7 RED. The 2 that pass pin what must not change: the end positions, and that the crouch is not slower.
2. `6dfe849e` **The row.**
   - `node Tools/night/n63-d-eye63-row.cjs` appends design.n63.eye63 as design.n63's last key, after text63.
   - It is a VIEW row, enabled false, numbers {}.
   - Both copies are byte-identical. The script asserts that nothing else changed and refuses a second run.
   - On a pick onto a tree whose design.n63 gained rows: take HEAD's copies and run the script again (L-065).
3. `46591d2e` **The eased eye (GREEN).**
   - Eye63Math / Eye63Follow, and the hook in CameraFeel.
   - Tests: 7 of 7 outside Unity; 39 of 39 with D1's, C1's, B3's and the switch tests.
   - Gates all GREEN 01:22:38: golden 0 mismatches, 22 replays exact, fingerprints EQUAL.
4. `e627a434` and `ff0439a3` **The proof tool:** `n63-m1-chop.cjs --pair`, in a two-tag form and a one-tag form.
5. This notes commit.

dn-check: 0 errors at commits 1 and 3.

## The read (measured offline, read only)
**Input:** Logs/n63/m1.md and NIGHT62's freeze telemetry (`Logs/feel/n62-freeze/v*/verbs37`, five levels, 1800 frames each at 60 fps).

**Today's slide eye:**
- MoveTowards at 7 m/s: 11.67 cm per frame, starting and stopping at full speed in one frame.
- Plus the slide dip: 0.15 m in 0.04 s.
- Plus the landing dip: a sine attack over 0.03 s.

**The measure is linear.** n63-m1-chop's residual (the camera against a centred 9-frame quadratic Savitzky-Golay fit) is a linear
operation. So the vertical residual is exactly the feet's residual plus the eye's (cam.height's).

Today, by event:

| Event | The eye's own residual | The feet's own residual |
|---|---|---|
| Slide entry (with its dip) | 9.04 cm | 0.04 cm |
| Slide exit | 4.55 cm | 0.03 cm |
| Landing dips | 3.1-4.7 cm | 4.1-17.6 cm |

**Said plainly:** a landing's total residual is mostly the feet's stop. No camera ease changes that. M1 leaves the slam's stop out for
the same reason. A landing's total can only drop by the eye's part.

## The design search (offline; scratch scripts, not in the repo)
1. **A plain critically damped spring on the step target, at "the same 0.10 s"** (ω 47-55, within 1 cm by 0.10-0.12 s) makes it
   **worse**:
   - entry 7.1-9.1 cm, exit 6.2-7.9 cm;
   - the start is not eased at 60 fps (ωh ≈ 0.8);
   - halving it needs ω ≤ 30, a 0.2 s crouch.
2. **The limit:** no shape over exactly 0.10 s halves the exit's 4.55 cm. Over T, the exit's residual is:

   | Shape | T 0.10 s | T 0.133 s | T 0.15 s |
   |---|---|---|---|
   | Linear (today) | 4.55 cm | — | — |
   | Smoothstep | 3.54 cm | 1.84 cm | 1.40 cm |
   | Cosine | 3.75 cm | 1.79 cm | 1.32 cm |

   Halving takes a little tail.
3. **Chosen:** today's composite (eye less dips), with the eye's own ease a little quicker, through a critically damped follow.
   - The grid: the inner ease time 0.07-0.10 s against ω 40-100. The pick is **0.08 s (x0.8) at ω 70**.
   - Slide entry: 9.04 → 3.20 cm (35 %).
   - Slide exit: 4.55 → 2.02 cm (44 %).
   - Landing dip: 39 % of today's at every fall from 3 to 48 m/s.
   - The per-frame speed jump at the start and stop: 37 % / 44 % of today's.
   - 90 % of the exit is reached in 6 frames, as today.
   - Half-way is one frame later than today (17 ms).
   - The drop 83 ms into a slide: 66.5 cm (today 69.3).
   - Within 1 cm of the rest at 0.15 s (today 0.10 s).
   - The landing dip's deepest point: 12.9 cm against 14.5 at the cap (89 %). The slide's deepest point: 77.5 cm against 79.6.

## What it does (view only)
- **Every frame**, CameraFeel.Apply steps an Eye63Follow, whether or not the row is on:
  - the eye's own ease: 0.70 m in camera.eyeEaseTime x Eye63EaseTimeMult (0.08 s);
  - less today's dips (the slide's, the landing's, as CameraFeel computes them);
  - followed by a critically damped spring, exact for a target held over the frame, at Eye63FollowRate (70 /s).
- **The camera:** with the row on (NightRows.Live, every frame) it shows the follow. With the row off it shows today's exact expression,
  `drawn.y + eye - dip`.
- **D1's contract:**
  - a held frame (dt 0) moves nothing;
  - the follow keeps running while the row is off, so the next frame on is the frame it would have drawn.
- **A respawn** (CameraFeel.Cut) cuts the follow to the standing eye, at rest.
- **The decision** is made once, at Wire. Off: the row off, `-skygrindOff63` (or older), `-skygrindNoEye63` (any case).
- **Not touched:** the sim's eye height (body.eyeHeight: Combat's ray, the bots), the crouch, the hitboxes and the fingerprints.
- **Proof lines (Player.log):**
  - `[eye63] eye63 ON: the eased eye (...)` or `[eye63] eye63 OFF (...)`, at Wire;
  - `[eye63] fired at frame N: the shown eye ... m, today's ... m (... cm apart): an ease in progress`, the first frame they are 5 mm or
    more apart (UnityTuning.Eye63FiredM).

## The proof tool: `node Tools/night/n63-m1-chop.cjs --pair`
**Forms:**
- Two tags: `--pair <onTag> <offTag>`.
- One tag: `--pair <tag>`, which pairs each side S with S-off.

**What it prints, per level:**
- **View only:** the largest feet / x / z difference between the runs. The telemetry rounds y to 1 mm and height to 0.1 mm.
- **Per event of the off run:** the peak of the eye's own residual, off → on, HALVED or not; the 3D total; the feet's own.
  - Left out: the reel placing the player (the tool's own rule: the feet step over 10 cm with no vertical speed; M1's f267 / f342).
  - Events under the 2 cm chop line on the off run are listed, not counted.
- **The whole path:** off → on.

**The last line** says whether D-10's measure is MET: the eye's own residual halved at every counted slide and landing event.

**Checked:**
- **The default mode is unchanged:** n62-freeze through the new code gives Logs/n63/m1-chop-n62-freeze.txt byte for byte, but for the
  header's tag.
- **Planted RED** (the same run on both sides): halved 0 of 64, NOT MET.
- **A replayed eased run.** This is the C#'s algorithm applied offline to n62-freeze's telemetry, with the crouch and the dips
  reconstructed (exact to 0.0 mm). **It is not a Player run.**
  - halved 64 of 64, worse 0 (and 0 of the 67 under the line);
  - the total halved at 17 (the slide entries and exits on flat feet);
  - the whole-path rms 1.93 → 1.53 cm (foundry), 2.61 → 2.27 (spire);
  - frames over 2 cm 134 → 100 (foundry);
  - feet within 0.50 mm;
  - MET.

## SPOT
A camera ease is not a still-frame change, so its proof is the telemetry, not a pair of frames.
- A D1 held pair mid-slide-entry (`-skygrindSameRun63 n63.eye63`, e.g. verbs37 vf f463 / f464) would show the whole view a few cm higher
  or lower: the follow's lag. Pixels change, but they say nothing about smoothness.
- The row waits OFF for GG's blind paired A / B (L-001).

## The exact Player steps (the lead's: I ran no Editor, build, Player or reel)
1. **Build:** `bash Tools/night/n41-build.sh <tag>`, then `node Tools/night/n46-build-rows.cjs <tag> on n63.eye63`.
2. **Reels, one set:**
   ```
   bash Tools/night/n63-reels.sh <tag> e3f,n52-foundry,verbs37 e3f-off,n52-foundry,verbs37,-skygrindNoEye63 \
     e3i,n52-iceline,verbs37 e3i-off,n52-iceline,verbs37,-skygrindNoEye63 e3k,n57-kiln,verbs37 e3k-off,n57-kiln,verbs37,-skygrindNoEye63 \
     e3p,n52-spire,verbs37 e3p-off,n52-spire,verbs37,-skygrindNoEye63 e3s,n52-span,verbs37 e3s-off,n52-span,verbs37,-skygrindNoEye63
   ```
3. **Measure:** `node Tools/night/n63-m1-chop.cjs --pair <tag>`.
   - Want the last line MET.
   - Want "view only: yes" (the feet, x and z within 1.1 mm).
4. **Proof lines:** `grep "\[eye63\]" Logs/player/n47-reel-<tag>-e3f.log` should show ON and one "fired" line. The -off side should
   show `[eye63] eye63 OFF (-skygrindOff63 or older / -skygrindNoEye63)` and no "fired" line.
5. **verbs37 x5:** the gates as on every build. The sim is untouched, so the scores should not move.

## Not checked
- **Any Player or Editor run:**
  - N63EyeTests in the Editor (the scratch runner copies CrunchMath.Tilt and CameraMove.LandDip line for line);
  - the reel pair.
- **Whether a verbs37 reel run is deterministic between two Players**, frame for frame. The pair needs the same frames on both runs. The
  tool reports the feet difference, so a drift shows there.
- **Real frame pacing:** the follow is exact per frame, but the reel's 60 fps fixed clock is all that was measured.
- **Whether GG feels the difference.** The blind A / B is his.
- **A reel placement that is not a Cut.** If the reel teleports the player without CameraFeel.Cut, the follow eases across the teleport
  for about 0.1 s. The tool cuts its segments there, so the measure does not see it.
