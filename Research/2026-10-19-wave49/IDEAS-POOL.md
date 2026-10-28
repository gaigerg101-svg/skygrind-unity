# Wave 49 idea pool (S2: unfiltered; scored in SHORTLIST.md)

Lenses: GG's fantasy (ULTRAKILL / DOOM), the genre's best, other genres, what a player shows a friend, tech just possible, a constraint card.

## The charge bar (G1)
1. A thin curved charge arc under the reticle (not a bar): fills left to right, colour by weapon, pulses when full, gone when no charge.
2. A short straight bar under the reticle, one segment per charge tier (KESTREL charge, THUNDERCLAP slug, PULSE blast, BEND), the label
   in the weapon panel only.
3. The reticle itself grows a ring that fills (DOOM Ballista-like): no extra element, the reticle is the meter.
4. A full-charge "ping": the bar flashes white for 2 frames and the reticle's centre box lights, so the player knows to release.

## Palette and colour (G4)
5. A palette-as-data file: one token per role (environment dark / mid / light, accent per level, danger, usable, pickup, UI) read by
   every view, so a level's look is one table.
6. A colour script: one dominant hue per level (L1 molten amber, L2 ice teal-blue against red-orange foes, L3 obsidian and magma, L4
   bronze and deep indigo, L5 violet storm and gold), each with a dark base value.
7. Cyan budget: cyan only on what the player can use (ride walls, grapple nodes, pads, pickups); every decorative edge line goes to the
   level's accent or a dim neutral.
8. Value budget: 60 % of each frame in dark values, 30 % mid, 10 % bright accents (the ULTRAKILL / DOOM read).
9. Enemies get a 1-2 px emissive rim / back-light in the danger family so they pop on any wall.

## Lighting (G2)
10. URP Volume per level: tonemapping, a colour-adjust contrast lift, shadows-midtones-highlights toward the level's hue, bloom
    threshold set so only emissive accents bloom.
11. Fog per level in the level's dark hue (depth and silhouettes), starting at the arena's mid distance.
12. Projectile point lights (each shot lights the room for a few frames, ULTRAKILL-style).
13. One warm key and one cool fill per arena (DOOM's framing), from config.

## Effects (G5, G6)
14. One impact kit: every hit made of the same three parts (a flash, a ring, sparks) with sizes and colours by tier (light / heavy /
    critical) and by owner (player cyan-white, enemy red-orange, parry yellow).
15. One timing curve set: every effect's rise, hold and fade from a shared table (e.g. 2 / 4 / 12 frames for light hits).
16. Hitstop tiers shared by every weapon and boss (the sim already has hitstop: the view reads one table).
17. Kill flare + gibs in one family: a short white flash, then the danger colour bursts.
18. A style word on kills (ULTRAKILL's style meter feel) in the HUD's font: a moonshot, behind a switch.

## Arenas and design (G3)
19. Each level's landmark lit as a light source (the WICK's furnace, the KILN's core, the LOOM's warp, the SPIRE's column).
20. Floor and wall material contrast: floors darker than walls, walls darker than the sky, the play space framed.
21. Decals and trims: dark trim lines and panel seams in the environment palette instead of cyan edges.
22. The skybox / ceiling as the level's colour statement (a dusk gradient per level).

## Constraint cards
23. "No new shader": everything through URP built-in Volume overrides and vertex colours (the G10 lesson).
24. "Four hours": the biggest visible gain first; each pass measured by a frame histogram and judged by a blind pair.
