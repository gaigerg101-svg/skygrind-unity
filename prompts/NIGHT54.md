# NIGHT54: THE LOOK PASS (GG 2026-10-20 ~14:05; go 14:09 MST; plan to 14:24, build to 16:39, hard cap 16:54: DECIDE54 D-1)

## GG's request (verbatim, the top of the night)
"We first need to improve the visuals across the entire game and make the animations, attacks, boss behaviors, movement effects, and
general feedback feel significantly smoother and more polished, because right now a lot of the decorative elements in several arenas are
poorly executed and lack strong color direction, and the lighting and flickering bugs that happen constantly break immersion and absolutely
need to be fixed; the UI looks good as it is, so don't touch that, and don't change the core mechanics either, but the movement needs to feel
cleaner, more fluid, and less choppy, with fewer bugs overall. The first boss fight in particular has far too many lighting issues and is
honestly much harder than it should be, so that entire encounter needs to be adjusted to make the fight relatively easy while keeping all the
attacks exactly the same except for the one where it covers half the arena before becoming vulnerable—remove that attack entirely and just
cycle through the others. Beyond that, each boss should have its own distinct color palette and lighting identity so the arena shifts in tone
and atmosphere during the fight, similar to how JoJo's Bizarre Adventure uses dramatic color changes to amplify tension and style, and the
guns and effects during boss fights should follow proper color theory and feel creative, intentional, and well-designed. We should continue
finding improvements and bug fixes, add new lighting passes, enhance weapon effects, and build a clear plan for the first fifteen minutes of
development that ramps into a focused 2.5-hour build window with a hard cap at 2.75 hours to ensure the work stays tight, efficient, and
productive."

## The first fifteen minutes (14:09-14:24): what was done
1. Grounding: Drive and Linear searched for SKYGRIND (nothing there); the repo's HANDOFF, CORE, FEATURES, THREADS read.
2. Three read-only scouts: boss 1 and its attacks; the flicker sources; the camera's choppiness (their findings in section 2).
3. The baseline measured before any change: THE WICK's fight with the human-model pilot (`n47human --boss wick --seeds 40`,
   Logs/n54/wick-base.txt).
4. The rows, owners and checks fixed (section 3); the calls written to DECIDE54.md.

## 1. Hard rules (CLAUDE.md; nothing new)
Never commit to master / push / merge; explicit paths + .meta; never GG's dirty files; the TMP fallback restored before a commit; movement
exact (golden 0 mismatches, fingerprints EQUAL plain / --off53 / --off54); gameplay numbers from config; no audio code; palette (cyan player,
red-orange danger, magenta RIVAL); UI text >= 16 px; **no UI file touched tonight (D-4)**; nothing heavy beside reels / builds (L-005).

## 2. The read (the scouts' findings; file:line in DECIDE54.md)
(filled at 14:24 from the scouts)

## 3. Rows (design.n54; `-skygrindOff54` / `--off54` turns every one off)
| id | row | kind | owner | the check (measured, RED once / GREEN once) |
|---|---|---|---|---|
| P0 | baseline + shared commit | - | lead | design.n54 rows exist and are off; N54SharedTests; gates green |
| W | wick54: FLARE removed (all legs, the cap flare too), the fight made easy | sim | builder W | n47human 40 seeds: 0 flares; median fight time and the deaths per fight below the baseline (targets in DECIDE54) |
| L | light54: the flicker fixes + the lighting pass | view | builder L | a still-camera flicker count in the Player (frames whose pixels jump) RED off / GREEN on, on the WICK's arena and two levels |
| G | grade54: each boss's palette + its phase shifts (the JoJo turn); the guns' boss-fight colours | view | builder G | a boss reel's frames: each boss's grade measured (hue / value) distinct, the shift at each phase; cyan's contrast against each palette |
| M | move54: the camera's smoothness (view only) | view | builder M | a camera-jitter number in the Player (frame-to-frame step variance at a steady run) RED off / GREEN on; golden + fingerprints EQUAL |
| Z | freeze + close | - | lead | verbs37 x5 nothing lost; the smoke; suite; tag u3-n54 |
