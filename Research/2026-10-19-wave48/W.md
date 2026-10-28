# NIGHT48 builder W: research and round 1 measurements (THE WICK: G1 the climb's top third, G5 the fight text, G9 the three R4 rows)

Each research line is **Verified** (I read the page) or **Reported** (read second hand). Every number below was measured with the harness
named next to it (headless, the real Mover in the real sim); anything else says "not checked". No Player was run by W (the lead's).

## Research
- **Verified** (github.com/NoelFB/Celeste, Source/Player/Player.cs, raw file read 2026-10-19): Celeste's climb ends at a wall's top with a
  CLIMB HOP: in ClimbUpdate, under `//No wall to hold`, `if (Speed.Y < 0)` ("Climbed over ledge?") calls `ClimbHop()`, with
  `ClimbHopY = -120f`, `ClimbHopX = 100f`, `ClimbHopForceTime = .2f`. A climber who runs out of wall while still going up is popped over
  the edge, not dropped. The lip is the same idea for a ride that runs out of panel: the end of a hold is a place to stand, then a short
  hop on.
- **Verified** (the same file): `JumpGraceTime = 0.1f` (the jump after leaving the ground). NIGHT47's climb47 coyote off a climb wall is
  0.15 s (design.n47.climb47.numbers.coyote).
- **Reported** (Research/2026-10-18-wave47/W.md, NIGHT47's lines on Celeste's WallJump assigning Speed.Y and the corner correction): read
  there, not re-checked tonight.

## G1: what the misses are (n48wick sloppy --steps, Tools/SimHarness/Harness.N48.Wick.cs)
- NIGHT47's diagnosis was "a 150 ms late release running past the panel's end". Traced step by step tonight: NIGHT47's sloppy model holds
  jump for 18 steps once the pilot lets go, but the pilot's own release is only 4 steps long (Wick37Pilot.Scripted holds again from sSince 4,
  for the attach). So a "late" rider **never lets go at all**: the ride runs on, ends on its own at the panel's end (or when the stick turns
  to the next panel), the late window (lateLeft 23 steps) runs out with jump still held, and the rider slides along the hot wall past the
  panel's end and falls to ledge 2. This is the "held past the end" hand: a real mistake (a player who keeps the button down), but not the
  "150 ms late" one.
- With the release let go 150 ms late (n48wick's default model; `--oldlate` keeps NIGHT47's): the top third is 38/40, 39/40, 32/32 (95.0 /
  97.5 / 100 %) **with or without the lips**; climb median 19.8 s; the cap 40/40.

## G1: the lips (level row climbTop48, n48-foundry; numbers design.n48.climbTop48.numbers)
- A lip after panels 4, 5, 6: top 0.8 m over the panel's bottom (under the rider's feet at the catch line, 1.0), 22 deg from the panel's end
  angle, 2.0 m deep from the wall; the next panel starts 14.9 deg (about 2 m of hot wall) past the lip, its bottom 2.0 m over the lip top
  (n48wick lips: tops 17.3 / 20.1 / 22.9 m).
- The step pilot (W47StepFrame, also n47human's P2 policy) mounts the next panel from a lip as it does from a ledge: run at the face 35 %
  along, jump, jump held. Without it Wick37Pilot's ground logic jumped inward off the lip (traced).
- Sweep, NIGHT47's model, 20 seeds (lipOver, lipArcDeg, lipStartDeg, lipDepth -> top-third first tries): 0.8 / 22 / 0 / 2.0 -> 95 / 90 / 94.7 %;
  1.0 / 28 / -2 / 2.0 -> 95 / 90 / 82.4 %; 1.0 / 34 / -2 / 2.0 -> 95 / 90 / 86.7 %; 0.8 / 34 / 0 / 2.0 -> 95 / 80 / 78.6 %. Before the lip
  mount: 0.8 / 22 / 0 / 2.0 -> 77.5 / 72.5 / 74.3 % (40 seeds), so the mount is half the fix.

| 40 seeds, n48wick sloppy | top third first tries (4->5, 5->6, 6->7) | bottom (steps) | the cap | climb median |
|---|---|---|---|---|
| NIGHT47's model, lips off (`--nolips --oldlate`) | 31/40, 30/40, 19/33 (77.5 / 75.0 / 57.6 %) | 187/192 = 97.4 % | 39/40 | 29.0 s |
| NIGHT47's model, lips on (`--oldlate`) | 36/40, 35/40, 33/35 (90.0 / 87.5 / 94.3 %) | 187/192 = 97.4 % | 39/40 | 20.7 s |
| let go 150 ms late, lips off (`--nolips`) | 38/40, 39/40, 33/33 (95.0 / 97.5 / 100 %) | 186/191 = 97.4 % | 40/40 | 19.7 s |
| let go 150 ms late, lips on | 38/40, 39/40, 32/32 (95.0 / 97.5 / 100 %) | 186/191 = 97.4 % | 40/40 | 19.8 s |

- **The one gate not met: the climb median 25-45 s.** Every forgiving change makes the climb faster (fewer falls to ledge 2): 20.7 s with the
  lips. Lengthening it means more panels (the cap, the glass and every frame spot move): not done in round 1; the lead's call.

## G1 round 2: one more top panel for the time (the lead's call: one attempt)
- design.n48.climbTop48.numbers.extraTopPanels 1 (n47's 8 panels + 1; the cap 26.3 -> 29.1 m), lipAfter [4, 5, 6, 7]. n48wick sloppy,
  40 seeds, NIGHT47's model, from the config: top third 36/40, 35/40, 34/38, 29/35 (90.0 / 87.5 / 89.5 / 82.9 %); bottom 187/191 = 97.9 %;
  the cap 39/40; **climb median 24.7 s: still under 25 s** (was 20.7). n47human --rows climbTop48: 40/40, 90.4 / 106.0 / 75.0 s; with the
  sloppy climb 40/40, 100.5 / 117.5 / 79.2 s: both IN.
- Measured as a data point only (not in config): extraTopPanels 2 (the cap 31.9 m), lipAfter 4..8: top third 90.0 / 87.5 / 89.5 / 84.6 /
  82.4 %, bottom 97.9 %, cap 39/40, **climb median 27.0 s (met)**; n47human 40/40, 90.6 / 106.2 / 76.1 s, sloppy climb 103.6 / 117.6 / 82.9 s:
  both IN. Switching to it is two numbers (extraTopPanels 2, lipAfter [4, 5, 6, 7, 8]); the a148 frames follow the stack.

- **In config from DECIDE48 D-23 (the lead's call): extraTopPanels 2, lipAfter [4, 5, 6, 7, 8].** Re-measured from the config (no
  W48LIP), 40 seeds: n48wick sloppy --oldlate G1 GREEN: top third 36/40, 35/40, 34/38, 33/39, 28/34 (90.0 / 87.5 / 89.5 / 84.6 / 82.4 %),
  bottom 187/191 = 97.9 %, the cap 39/40, climb median 27.0 s, 37 lip landings; n47human --rows climbTop48: 40/40, 90.6 / 106.2 / 76.1 s
  IN; W47SLOPPY=1: 40/40, 103.6 / 117.6 / 82.9 s IN.

## G5 (the reel; not measured headless)
- THE WICK's lines (design.n47.wick47.numbers.lines, read): "{punch} the yellow embers back." / "{fire} through the crack." / "It's climbing.
  Get up there." / "Hold {jump} to ride. Look up." / "Caught. Keep climbing." / "No way down. Finish it." and the intro card "The Wick. The
  furnace wakes.": each <= 6 words, sentence case. The core line is F's ("{punch} the core!").
- Size, computed from code (not measured): hud47's fight line is 40 px em (UnityTuning.F47.cs Hud47FightPx) on a screen-space overlay canvas
  scaled with the height from 1080 (Ui.cs: ScaleWithScreenSize, match 1), so 40 px at 1080p and 26.7 px at 720p. The a148 reel measures both
  (Ui.W48.cs reads the em and the drawn glyphs' height in screen px).

## G9
- The three rows are NIGHT47's (built, off): wickRideParry47, wickIntro47 (sim), wickFinale47 (view). Round 1 adds their a148 frames
  (W48-g9-intro, -rideparry, -finale, -finale-2).

## G9 round 3: the ride-parry frame's whole-screen rainbow (the lead's n48-r2 look)
- Read from the lead's frames (Logs/feel/n48-r2/w-on/a148, f0940 / f0947 / f0949 / f0951 and the shots): the split starts at f0949, the
  frame whose telemetry has the "hit" event (the forced ember hitting THE WICK while a plate was cracked, OPEN); the same split shows in the
  w-off run and, decaying, in W48-g9-finale-2. It is URP's chromatic aberration at full intensity, splitting the shaft's red-orange hot seams
  into red / green / blue rings; strongest at the edges (URP's is radial).
- The cause (read from code): GunFx.Hits43.cs OnBossHit kicks PostFx with UnityTuning.BossOpen43Chroma 0.6, but PostFx's pulse is in post.js
  units (post.chromaHurt 0.006, post.chromaRank 0.004), x ChromaScale 36: 21.6, clamped to 1.0, decaying at post.chromaDecay 0.03 a second:
  about 19 s at full on every OPEN hit of every boss (computed from the numbers, not timed in a Player).
- The fix (view only): the open hit's kick is post.chromaHurt x Open48ChromaOfHurt 1.0 (0.22 intensity, gone in 0.2 s; UnityTuning.W48.cs,
  one line in GunFx.Hits43.cs). The hit's own read is unchanged and local: the star, the flare, the two rings at the hit point, trauma,
  rumble. a148 measures the intensity at the hit (W48-g9-rideparry-hit) and 0.6 s on (wants <= 0.25). Not checked in a Player.
- Not fixed (not THE WICK's): UnityTuning.Fusion43Chroma 1.4 (the FUSION pop) has the same unit mix-up: 1.4 x 36, about 46 s at full.

## G4 held with the lips (n47human --boss wick --seeds 40 --rows climbTop48; the lead's pilot, run by W)
- Skilled climb: 40/40 killed, median 93.2 s, p90 104.1, min 71.0: IN (NIGHT47 round 4 without lips: 95.0 / 105.4 / 71.0).
- The sloppy climb in P2 (W47SLOPPY=1): 40/40, median 101.2 s, p90 116.5, min 78.1: IN (NIGHT47: 101.2 / 119.4 / 82.9).
