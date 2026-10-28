# FORGE72 research: what the night stands on (2026-10-27)

Our data first (spike.md, gg-proxy.md, off-why.txt), then the four web scouts (each claim's URL is in its scout file), then the model's
own guidance.

## R1 Our own record: why GG has "not seen it"
- w61-w71 shipped rows OFF behind gates that rarely ran; 76 design rows are OFF (off-rows.txt). Of the 32 harvest candidates read, every
  `enabled_why` says "until" a gate (a critic, a 41,472 px SPOT, GG's keep), and none says the row failed (off-why.txt).
- NIGHT71 (ledger wave-71): builders' lists ran dry at 1:20 (D), about 2:20 (B, F) and 5:25 (R). The lead's Player queue was the limit
  (its red team's top finding). No n71 row went ON.
- The evening of 2026-10-26: eight changes ON in about 2.5 hours, one lead, no critic, each with a Player check and an off switch. GG's
  next words: "keep double jump and do the rest".
- So the lever is not more building but **shipping what is built**, with a cheaper proof: one same-build pair, one blind SPOT, the bots'
  guards (D-1), and the lead's own look.

## R2 Light and flicker (scout-lighting.md)
- Rank 1 in the scout (a custom shader off the Forward+ contract) is ruled out here: Stone.shader has the cluster loop, the screen UV and
  the fog (spike.md S2).
- Left to measure: SSAO without TAA (noise that crawls), animated film grain (0.035-0.045 on Medium / High), bloom on thin bright strips
  (High Quality Filtering; strips at least 3 px), the shadow atlas (point-light shadows six maps each), the CRT pass, and the red noise
  border on the wyrm's frames.
- A still-camera meter: 60 frames per pose, the share of pixels whose luma change flips sign repeatedly, p99.9 |dY|, a heatmap; planted
  case first (L-150). Thresholds are proposals, calibrated on the best current room.
- Readable dark rooms: ambient as a floor, shadows tinted cool and never black (Valve), a per-fight exposure for a boss rather than more
  lights, key and rim behind the boss; light the same kind of thing the same way.

## R3 Telling the player where to go (scout-guidance.md)
- Fixed corner text is ignored (eye tracking); a line at the thing it names, that changes, is seen.
- ULTRAKILL 0-1: 4-8 word imperatives, the verb in capitals, in a room that cannot be left without the verb.
- Hints on stuck or failure only: first at about 8-10 s without progress, stronger at about 20 s, retired after success; reset the timer only
  on progress, not on any input.
- One reserved colour for the route (Neon White, Ghostrunner): cyan here (player-usable), never red.
- Xbox XAG 101: body height at least 18 px on PC and 26 px on console at 1080p; CLAUDE.md's 16 px is below both (D-9: 22 px tonight;
  the floor is GG's call).

## R4 Bodies, motion and one colour language (scout-animation.md)
- Spring secondary motion (t3ssel8r's second-order system: f, z, r; f 1-3 Hz body, 4-8 Hz flinch, z 0.3-0.5 soft, 0.6-0.8 armour) fixes
  "rigid" more cheaply than anything else; squash keeps volume (1/sqrt(s) across).
- Three-beat attacks (pull back, hold, snap) with the tell visible at least 16 frames before damage.
- One hue per meaning, unchanged across phases; a flash kept rare so it keeps its value (ULTRAKILL); measure colour after bloom.
- Unity: per-instance materials over MaterialPropertyBlock with the SRP Batcher.

## R5 Movement and camera (scout-movement.md; Research/2026-10-26-ultrakill/movement.md)
- Already in the profile (ultra71): the jump (5.07 m in 0.702 s, no fall multiplier), walk 8.49, dash 25.46 for 0.2 s, slam 51.43, wall
  jump 12.34 + 12.34.
- Not yet: the ground's snap (ULTRAKILL closes 25 % of the gap per 0.008 s: half-life 19 ms; ours is Quake friction 6, about 115 ms) and
  air control (0.48 u/s per tick per axis, cap 16.5 u/s, over-speed kept, turning scaled 0.4 above the cap). Patch 17 / 17c (2026) made
  air strafing stronger, so the mod's numbers may be older than the live game.
- Camera (one source, the V1 mod's port): FOV 105; dash or slide forward x0.95, backward x1.1, back at 300 deg/s; strafe roll +-1 deg,
  x5 dashing or sliding; slide eye drop 0.32 m at our scale; no landing dip, no speed FOV.
- ULTRAKILL has no coyote time or jump buffer as far as any source says; ours stay (nothing lost).

## R6 The model and the harness (re-read 2026-10-27: platform.claude.com ... prompting-claude-opus-5-5)
- Effort is the main control; medium is the default and matches Opus 5 high on coding; reserve xhigh / max for measured gains; set it
  before the run (a change invalidates the cache).
- Unattended runs: keep the task list in a file the model updates; name the early stops not wanted (a summary that announces the next step,
  an offer to continue, decisions that block nothing, "a good place to report") and the stops wanted; a smaller model checking a stated
  completion condition (our /goal) stops after two or three automatic continuations on the same task.
- Time signals: the model paces to an elapsed / budget line (our clock tool and the guard's phases).
- Opus 5.5 reads screenshots more precisely than Opus 5; crops and higher resolution still help on dense images (our contact sheets for
  the overview, a full frame to zoom one defect).
