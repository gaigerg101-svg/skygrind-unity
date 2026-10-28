# NIGHT63 builder V: B1d, the fix round after B1's gate FAIL (branch n63-B1d from port/stage3 at ab8ade3e)

The fix answers Logs/n63/verdict-B1.md.

Shas:
- `7e6a0a3c`: test first (RED).
- `f3f09659`: GREEN.
- This notes commit.

Cherry-pick: `git cherry-pick 7e6a0a3c^..<notes sha>`. The config hunk touches only `design.n63.bossMotion63.numbers.bosses.wick`.

## 1. THE WICK's stagger slump: the cap and the collider
**The red:**
- b8's same-run pair at `shot-N30-B63-wick-stagger.png` (boss63, n52-foundry, 12.5 m) changed 40,503 px, under rule 8's 41,472.
- Every boss ran pinned at its guard through the stagger ("limit share max 1.00"), so the clamp set the slump's size, not the spring.
- B1c's raise went mostly into the clamp: b4 37,345 px, then b8 40,503 px, after the sink went up a third and the keel and the squash
  nearly doubled.
- b8's ON frame shows why the sink cannot grow: at 1.02 m the orb already sits through the floor.
- The changed mask is the orb's crescents (its interior overlaps, cyan in both frames), the cap and the petals.

**The change (view only):** THE WICK gets its own slump numbers. The other four bosses keep the shared ones.

| | the shared (B1c) | THE WICK (B1d) |
|---|---|---|
| sink | 0.6 x radius = 1.02 m | 0.5 = 0.85 m (the orb rests on the floor, not through it) |
| droop (deg) | 12 | 14 (keeps the top's lean toward the player under the deeper keel) |
| keel (sideDeg) | 26 | 40 |
| squash | 0.22 | 0.30 (height x 0.70, width x 1.20; the shared squash guard 0.32) |
| slide | none | 0.7 = 1.19 m sideways, the way the top keels, alternating stagger by stagger |
| guard: limitShare | 0.6 = 1.02 m | **0.9 = 1.53 m** |
| guard: limitDeg | 30 | **45** |

- **The cap and the collider:**
  - THE WICK's collider is the sim's hit sphere, radius `design.n37.wick37.boss.cageR` = **1.7 m** (Wick37: a ray against the cage sphere at
    its centre).
  - The guard caps the drawn centre's offset at **0.9 x 1.7 = 1.53 m**, so the drawn centre stays inside the collider.
  - The full slump's offset is sqrt(0.85^2 + 1.19^2) = 1.46 m, which is 0.86 of the radius. It sits under the cap, so the clamp no longer
    sets its size.
  - The cage's own extent around that centre reaches past the sphere on the slid side, as it already did at 0.6.
- **Code:**
  - `BossMotionParams63` reads a boss's own `slump.sink / deg / sideDeg / squash / slide` over the shared ones.
  - The layer adds the slide along the keel's side (`-axis x slide x size x slump x side`): AngleAxis(keel, toward) turns the top toward
    toward x up = -axis.
  - The slide is capped with the rest of the offset by the guard.
- **The prediction (a model, not a measurement):**
  - A crude 2D model of the cage: the orb, the cap and the petals, the floor at the petals' base, scaled at 57 px per m. It was calibrated
    on b8's pair as 1.00.
  - It puts B1d at about 1.5 x b8's change: **about 60,000 px**.
  - Model rows: b8's slump 1.00, + a 0.85 m slide 1.20, + a 36 deg keel and 0.72 squash 1.29, B1d's numbers 1.52.
  - Scripts: scratchpad `wicksim.py` and `orbsim.py`.
- **The rig at the reel's stagger frame (0.6 s in):** a Python port of the layer's slump path (Spring63, the compose, the guard) on these
  numbers gives:
  - slump 0.98, slide 1.17 m, sink 0.84 m;
  - offset 1.43-1.47 m, under the 1.53 m cap;
  - keel 36 deg (the first stagger) and 43 deg (the next, the other way), droop 14 deg;
  - the top's lean toward the player 0.17-0.19;
  - squash y 0.69-0.72.
- **THE LOOM: not done.** Its b8 stagger pair (40,733 px) changes thin lines (the lightning arcs and the hull's spikes) round a hull that
  is mostly hidden behind the big blue disc at the reel's stagger pose. A thin line moved further than its own width changes the same
  number of pixels, so a bigger LOOM slump is not predicted to clear the bar. A different pose would do more than more motion; that is
  the lead's call.

## 2. The crown's "drawn off" at 8983 ms: not a held frame
**What happened**, read in n47-reel-n63-b8-b1p.log lines 126-127:
- The two damage events at 8983 and 9000 ms are on **consecutive frames**.
- THE CROWN draws in LateUpdate, after the reel's coroutine, so the reel takes its draw of a frame on the next one (`drewAgo` 1).
- The old loop counted a new damage event before judging the waiting one. At 9000 ms the 8983 event was declared "off" before frame
  8983's draw, taken that same frame, was looked at.
- No shot or held frame was near: shot 3 was about 8200 ms, the stagger shot about 14,600 ms.

**The fix:** the reel's flinch counting moves into `FlinchBook63` (Game/BossFlinchBook63.N63V.cs, pure).
- Each fight frame the book first judges the waiting event on this frame's take, then lets a new one replace it.
- The rules are unchanged: drawn if a draw dated at or after it shows a flinch over 0.1, late after 2 frames, off after 6 frames, off
  when replaced with no draw taken, off when pending at the end.
- D1's held frames are still skipped by the reel before they reach the book (c8f9ec75).

**Tests:** the crown's case, with two events on consecutive frames and the draw a frame late, now reads both drawn +0. The Update-drawn,
never-drawn, replaced, late and end cases are pinned too.

## 3. THE KILN WYRM's small on-screen motion is its hitbox rule (by design)
- WyrmView: "a shot finds what you see". The wyrm's drawn boxes ARE its hitboxes, so moving the drawing moves what a shot can hit.
- Its guard is therefore the tightest, in `bosses.wyrm`:
  - gain 0.6;
  - follow 0 (no lag along its fast path);
  - limitShare 0.25 of its size, half its head's width (1.5 m): **0.375 m on the head**;
  - limitDeg 14;
  - each body piece's guard is 0.25 x its own half width.
- The 24 body pieces and the tail take the flinch as a ripple (0.025 s a piece, the tail keeping 0.35) and the sag and the breath as a
  wave. They never tilt.
- **On screen** at the reel's wyrm camera (the flinch frame at 29.4 m: about 24 px per m at fov 74, 1080p), the head's whole 0.375 m guard
  is about **9 px**.
- b8 measured it at that size:
  - rms best 0.37 m (the others 1.0-2.2 m);
  - same-run pairs: flinch 1,084 px, wind-up 1,097, idle 373, tell 65, stagger 16,942.
- This is the rule doing its job, not a missing motion. A bigger wyrm pose would draw the body where the shots do not land.

## The tests
- **N63B1dTests (new), 5 tests:**
  - THE WICK's own slump under the raised guard, the full slump under it, the other bosses unchanged;
  - the rig sliding the way it keels, inside the guard and alternating (Editor only);
  - the flinch book (two tests);
  - the reel counting through the book.
- **N63BossMotionTests: two assertions changed** (for the slate):
  - the row's guard ranges, limitShare (0.05, 0.6) to (0.05, 0.95) ("the drawn centre inside the collider") and limitDeg (5, 30) to (5, 45);
  - the WICK's on-screen slump: "a sink of 0.9 m or more" became a sink of 0.7 m or more plus a screen-plane move (sink and slide) of 1.2 m
    or more, because the slide took part of the sink.
- **One count fixed in my own new test** between RED and GREEN: 5 damage events, not 6.
- **RED, measured outside Unity** with the scratch runner on the dn-check build: 4 fail on the placeholders. The rig test needs the engine
  (the Quaternion ECall).
- **GREEN outside Unity:**
  - N63B1dTests 4 / 5 (the rig test needs the engine);
  - N63BossMotionTests' engine-free 7 (its row test with the new ranges among them);
  - N63BossClockTests 6, N63ReviewTests 3.
- The Editor-only rig assertions (`Rig_TheWicksSlumpReadsOnScreen`, `Rig_AStaggerSlumps_AndRisesAfter`, the new rig test) were checked
  against the Python port above. All hold, with the droop's lean toward the player at 0.17-0.19 against the 0.15 bar.
- dn-check: 0 errors. Gates: all GREEN 02:41:39.

## The Player steps
1. Build with bossMotion63 on: `node Tools/night/n46-build-rows.cjs <tag> on n63.bossMotion63`.
2. Run boss63 on n52-foundry (THE WICK) with `-skygrindSameRun63 n63.bossMotion63`. The SPOT is the `shot-N30-B63-wick-stagger.png` pair:
   want >= 50,000 px, predicted about 60,000 by the model.
3. The crown's boss63: the line `damage events N: drawn N ... not drawn 0`, and GREEN.
4. verbs37 x5 unchanged. It never fights a boss.

## Not checked
- Any Player or Editor frame of the new slump: its look, whether the slid cage clips a wall at THE STACK's shaft, and the bloom.
- The rig tests in the Editor (the Python port only).
- THE WICK's other states with the raised guard: its wind-up, flinch and breath stay well under 1.53 m and 45 deg by their own numbers.
