# NIGHT63 builder V: B1's rule-8 round (branch n63-B1c from port/stage3 at 91f998f8)

Kept apart from V.md so this pick and C2's notes (n63-C2, appending to V.md) never conflict.

## Why (the lead, n63-b4, D1's same-run toggle working)
THE WICK's stagger pair (my SPOT pose) changed 37,345 px, under rule 8's 41,472; the lead aims at 60,000 or more. b2's two-run 42,273 px
included drift. At boss63's 12.5 m the cage is about 200 x 300 px, and the slump moved too little in the screen's plane:
- the droop toward the player (22 deg) turns about the screen's horizontal axis, mostly in depth;
- the sink was 0.77 m (about 40 px), the side keel 14 deg (the top about 36 px), the squash 12 %.

## The change (numbers only, by Tools/night/n63-v-rows.cjs --replace; the layer's code unchanged)
| number | was | now | on screen |
|---|---|---|---|
| slump.sink | 0.45 | 0.60 | THE WICK sinks 1.02 m (about 53 px) |
| slump.sideDeg | 14 | 26 | the keel in the screen's plane (the top about 60 px) |
| slump.deg | 22 | 12 | the droop (in depth) |
| slump.squash | 0.12 | 0.22 | the height x 0.78 at the full slump |
| flinch.squash | 0.20 | 0.26 | the flinch's one part in the screen's plane |
| limit.squash | 0.28 | 0.32 | the guard's room for them |
| bosses.wick.limitShare | 0.5 | 0.6 | 1.02 m of its 1.7 m collider radius |
| bosses.wick.limitDeg | 26 | 30 | the keel plus the droop and sway |

Worked by hand at the reel's stagger frame (0.6 s in): keel about 22 deg plus the 12 deg droop, sink 1.00 m, height x 0.80, offset 0.98 m
inside the 1.02 m guard. The other bosses' own guards cap the bigger numbers (wyrm 0.375 m / 14 deg with its hitbox rule; crown 0.75 m /
18 deg; loom 0.8 m / 20 deg; brawl 0.6 m / 26 deg).

## Shas
`a861c7fa` (test first, RED: N63BossMotionTests.Rig_TheWicksSlumpReadsOnScreen, which pins a keel of 20 deg or more, a sink of 0.9 m or
more and a height of 0.85 or less at 0.6 s, the guard held), `b76a84f1` (the numbers), then this notes commit.
dn-check 0 errors; gates --no-editor all GREEN 00:19:31.

## Not checked
The pixels. 60,000 or more on b63f's same-run B63-wick-stagger pair is the aim, estimated from the on-screen displacement growing about
1.5-1.7x; not measured. The keel's side alternates stagger by stagger and the sway adds +-4 deg, so the frame 0.6 s in shows 22-29 deg.
