# Veto reviewer brief (NIGHT21 onward; from NIGHT20's blind brief; fresh, read-only, blind)

> **NIGHT21's changes.** Budget: 25 minutes and about 80 tool calls. If time runs short, answer the rest from what you
> saw and mark it "(quick look)".
>
> Your job is to **find defects**. Weight the banned-pattern ticks over the A / B picks: the picks are logged, but
> nothing passes on them. Add these patterns to the numbered list below:
> - 18: a feature that only shows (a HUD element, an effect or a prompt) with no visible effect in the world
>   (display-only);
> - 19: a stand-in where the real thing belongs (a stub);
> - 20: the missing-material magenta (255, 0, 255) anywhere.
>
> The scenario questions come from each sNN.txt.

You are judging a game's screens and moves from frames only: SKYGRIND, a first-person movement shooter at dusk (CoD
Zombies meets ULTRAKILL; Steam, hopefully the Steam Deck and Switch). You won't see code, metrics or anyone's notes:
**do not read any file except the images and the sNN.txt files in the folder you are given.** For each scenario you get
flipbook sheets for A and B: the same moment rendered by two versions of the game, 8 key frames in order, near native
resolution (`sNN-A*.png`, `sNN-B*.png`; `sNN.txt` names the scenario, its question and what the tiles are). Open every
image with your image reader. Crop or zoom with PIL (python) if a detail matters. There is no sound in either build.

The colour roles (fixed): cyan is the player's (everything you act with, the speed lines, the shockwave's air);
red-orange is enemies and danger only; magenta is a rival only; gold is points only; pale violet-white is the boundary.

For each scenario, answer **A**, **B**, **Same** or **Unknown**, and one line naming the frames that decided it, to the
question in its sNN.txt:
- "Which shows speed better while keeping the middle readable?" (sprint, boost, boostside, slide)
- "Which weapon moment reads better?" (shockwave, zoom, kill12, swapall)
- "Which HUD looks more finished, less cheap?" (hud-fight, vitals, rankup)
- "Which menu looks more finished?" (ui-title, ui-weapons, ui-results)
- "Which enemy reads better: what it is, what it's about to do?" (climb, special-round)

For the shockwave scenario also answer: "Does either side read as a burst of air that moves the player?" (A yes/no, B
yes/no). Then answer "Is either one too much (noisy, hard to read, tiring)?" per scenario where it applies.

Then tick each banned pattern below you can see in A and in B, naming a frame for each tick:
1. Flat TMP text on a flat rectangle, with no edge, cut, texture or depth.
2. Default rounded Unity sprites and pill buttons.
3. Soft drop shadows and outer glows on everything; gradient fills as decoration.
4. One ease for every motion, or a rubbery overshoot on everything.
5. Elements that pop in at full size and alpha.
6. Confetti: many small sparks after the main hit.
7. A bloom or blur that hides the thing it celebrates.
8. Effects over text.
9. Persistent HUD in the middle of the screen.
10. Button names written as words where a drawn glyph belongs; keyboard letters on a pad.
11. Colours outside their roles.
12. Centred stacks of same-weight text with no hierarchy.
13. Untextured quads as particles.
14. Grey boxes where a gun or an enemy model belongs.
15. Speed shown as a frame or border around the screen; lines that don't move with the camera's velocity; the same lines
    at every speed.
16. Effects that ignore the world: lines drawn over an enemy's body or the crosshair; streaks with no parallax.
17. An enemy standing below a player it can't reach.

Then score B only on this rubric, 1-5 each, one line per row (5 is described):
1 Kill crunch (a kill reads in 1 frame); 2 Hit feedback; 3 Weapon feel (each gun reads differently; the LT moment);
4 Movement feel (speed is visible, the middle stays readable); 5 Score energy; 6 Style meter; 7 HUD clarity (200 ms
glance, nothing in the middle at rest); 8 Menu simplicity; 9 Identity (one frame says SKYGRIND); 10 Motion (every
entrance animated, nothing pops in).

Return one table (scenario, pick, deciding frames), the shockwave yes/no line, the too-much answers, the banned-pattern
ticks (pattern, side, frame), and the rubric for B: under 700 words, no preamble.
