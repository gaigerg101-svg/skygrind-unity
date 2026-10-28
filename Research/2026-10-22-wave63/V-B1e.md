# NIGHT63 builder V: B1e, does the slide break "a shot finds what you see"? (branch n63-B1e from port/stage3 at 393bdc8d)

This is a measurement only; it changes no behaviour. It answers DECIDE63 D-12: THE WICK runs at its full 1.53 m cap in s14 / s15, so during its
OPEN window the drawn orb can sit up to 1.53 m from the centre of its 1.7 m hit sphere.

Shas:
- `8c01bc1c`: test first (RED).
- `ae09208a`: GREEN.
- This notes commit.

Cherry-pick: `git cherry-pick 8c01bc1c^..<notes sha>`.

## The proof line (boss63, one per boss, and in its verdict line)
`[boss63] reach <boss> open frames <n> | drawn weak point to the sim's hit sphere centre max <m> m, p95 <m> m | outside its <r> m sphere <k> frames (<share> %)`

The verdict line also gains `| reach (open frames n) max m, p95 m, outside the sim's sphere k`. It is a measurement and never adds a RED.

- **The frames:** the OPEN window's frames that the layer drew: the stagger phase (THE WICK's cyan window, THE ICEBREAKER stuck or dazed,
  THE KILN WYRM staggered, THE LOOM's and THE CROWN's windows).
  - D1's held frames are skipped, as before.
  - With the row off the layer draws nothing, so the line reads "open frames 0 ... not measured". That is the OFF side.
- **The drawn weak point** is `BossMotion63Math.DrawnPoint`: the layer's body transform applied to BossAttackClock63's `weak`, as
  pivot + offset + tilt x (squash x (weak - pivot)). The layer records it every frame it draws (`BossMotion63Probe.Weak`).
  - **THE WICK:** exact. Its view sets exactly that transform on its cage, and its weak point is the cage's centre (the flame core), so the
    distance is the layer's offset.
  - **THE ICEBREAKER:** its view reports its own torso centre from the body as placed, because its knees take the slump's sink back.
  - **THE CROWN and THE LOOM:** the formula assumes the body's rest axes are the world's. Their views squash in the ring's or the hull's own
    axes, so the squash part is an approximation for them. The offset and tilt parts are as drawn.
  - **THE KILN WYRM:** its head.
- **The sphere:** the size the guard scales by. For THE WICK that is `design.n37.wick37.boss.cageR` = 1.7 m, the sim's ray-sphere.

## What it should read for THE WICK (a prediction, not a measurement)
- The guard caps the offset at 0.9 x 1.7 = **1.53 m**, so the drawn core's centre stays inside the 1.7 m sphere on every frame. Expected:
  **outside 0 frames**, max at the cap 1.53 m (the slump spring's overshoot pins it), p95 about 1.45-1.53 m.
- **The edge**, which is arithmetic and not part of the line:
  - the drawn core has radius `coreR` = 0.7 m, so its far edge reaches up to 1.53 + 0.7 = 2.23 m from the sim's centre;
  - the part of the drawn core beyond 1.7 m (up to 0.53 m of its 1.4 m width) lies outside the hit sphere, and a shot there misses;
  - a shot at the drawn core's centre hits.
  - Whether "a shot finds what you see" should cover the edge is the lead's call. A cap of 1.0 m (0.59 x 1.7) would keep the whole core
    inside, but at that cap the B1c-era numbers measured 40,503 px, under rule 8.

## Tests (N63ReachTests)
- The drawn point on planted motions: a weak point at the pivot moves by the offset; a point a metre up gets the offset plus the squashed
  metre; at rest it is the sim's point.
- The book on 20 planted open frames (0.1 ... 2.0 m, a 1.75 m sphere): max 2.00, p95 1.90 (nearest rank), outside 3 frames (15.0 %), the
  line's exact text, and the no-frame line.
- A real WICK rig through 90 stagger frames: max over 1.2 m, at most the guard, 0 frames outside its 1.7 m sphere. Editor only (the
  engine's Quaternion).
- Source scans: the layer records it, THE ICEBREAKER reports its own, the reel adds the open frames and prints the line.
- RED, measured outside Unity with the scratch runner: 3 fail on the placeholders; the rig test needs the engine.
- GREEN outside Unity: N63ReachTests 3 / 4 (the rig test needs the engine), N63B1dTests 4 / 5, N63ReviewTests 3 / 3.
- dn-check: 0 errors. Gates: all GREEN 03:25:51.

## The Player steps
1. boss63 x5 with bossMotion63 on: same-run or plain. The line comes from the ON side.
2. Grep `[boss63] reach`, or read the verdict lines in summary47.txt.

## Not checked
- Any Player run.
- THE CROWN's and THE LOOM's squash axes (approximate, as said above).
- The edge of the drawn weak point: only the centre is measured.
