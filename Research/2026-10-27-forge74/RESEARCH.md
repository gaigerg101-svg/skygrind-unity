# FORGE74 research (three web-only scouts, 2026-10-27; every claim's URL is in the scout file)

## scout-rooms.md: small arenas that ask you to move (feeds a2, a5, b2, kb2c, v5, v2 and S0r)
- Size by speed, not metres: a fight space a dash crosses in under 2 s (Doom Eternal: "Too big, you actually feel slow. Too small, you're
  frustrated"; no published metres).
- At most three floor planes per area [published, Level Design Book]; upward needs ramps or pads, downward is free (one-way drops funnel).
- Cover classes: high >= 1.75 m, low 1.0-1.25 m, <= 0.5 m is not cover [published, Uncharted 4 via LDB].
- A pillar in a small room makes a circle-strafe loop; at least two loops per room [derived from CS's overlaid loops].
- Reveal on purpose (ambush unaware enemies visible from the entrance, or a threshold ambush); never spawn in the entry's rear arc (a
  180-degree stick turn is slow) [rule published, check derived]. "N LEFT with none in view" -> reveal74 (L2).
- Hakita: difficulty is count and composition; "as soon as you look at a room it's easy to understand it". Steve Lee: small and dense.
- Gaps: no published loop length, cover spacing or arena area for ULTRAKILL; thresholds marked [D] are starting values.

## scout-bosses.md: a weak point read at a glance, a kill that lands (feeds B1-B3)
- One state, one colour, held across the game (DOOM Eternal blue = stagger, orange = glory; ULTRAKILL yellow = parryable); a second channel
  (shape or motion) for colour-blind players; keep the colour channel quiet in combat (Horizon). Rule W (D-4) is this rule.
- A weak point is a distinct, non-spiky part of the silhouette with surface to hit (Horizon dropped thin-legged designs).
- Kill beat: hitstop scale from SF V 8-15 frames to ULTRAKILL's 0.25 s major hitstop and Smash's 30-frame cap -> 0.25-0.5 s for a boss
  kill; freeze then slow (Dead Cells); Smash's KO frame flips the background and zooms to the impact (reads in one still). kill74 keeps it
  view only (CLAUDE.md rule 4: the sim's hitstop is the web's).
- Gaps: no published weak-point size, pulse rate or contrast; white-flash frame counts are from blogs (unverified).

## scout-flicker.md: flicker with the camera moving (feeds L1, L4)
- No standard temporal-flicker metric or threshold exists (NVIDIA FLIP is still-image only).
- Recommended: motion-compensated flip-flop (warp t-1 and t+1 into t, flag opposite-sign residuals over tau, 2+ flips in 6 frames).
- Fallback, chosen for L1 (no optical flow needed): 8x8 block-mean luma, second difference large and first differences flipping sign; a
  slow fixed pan keeps blocks nearly aligned. Calibrate with a baseline-vs-baseline run (noise floor) and a planted +-3 % alternating control.
- Frostbite's stable SSAO (selective temporal filtering) is the reference fix for AO crawl.
