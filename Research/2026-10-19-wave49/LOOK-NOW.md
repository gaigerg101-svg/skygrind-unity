# The game as it looks now (the forge's screen pass, 2026-10-19 ~11:30, n48-r5 Player frames)

Sheet: `Research/2026-10-19-wave49/look-now.png` (one frame per level from the n48-r5 reels, 1920x1080 scaled). Looked at by the lead.

## What reads as "bland" (measured by eye, to be measured by number in the run)
- **Flat values.** Walls and floors are pale beige, tan and grey; ceilings are a flat navy. There is no strong key light and almost
  no dark-to-light contrast in the arena itself. Most of each frame sits in the middle of the value range.
- **Cyan everywhere.** Cyan edge lines run along nearly every wall, floor lip and ceiling beam in all five levels. CLAUDE.md rule 8 says
  cyan is player-usable; on frame it reads as "the default line colour", so it no longer tells the player what is usable.
- **No hue identity per level.** L1 is orange-beige, L2 grey-beige, L3 brown-navy, L4 navy-tan, L5 purple-grey: close to each other,
  all low saturation.
- **Enemies and bosses do not pop.** THE ICEBREAKER is black-and-white boxes on a beige wall; the WICK's orange sits on an orange-beige
  room. There is no rim or emissive separation between a foe and its background.
- **Effects in different families.** The CROWN's flame (white jagged ring, gold sparkles), the gauntlet (pink rings), the LOOM's
  outline (amber lines), the core (white-yellow), the grapple (cyan): each picks its own colour, shape and timing.
- **What already works:** the HUD's health bar and boss bar read; the fight lines read at both resolutions; the boss outlines (LOOM)
  and the lava's orange glow give the strongest frames.

## Rules that constrain the polish (CLAUDE.md rule 8, still in force)
- Cyan = player-usable; red-orange = enemies and danger only; magenta = RIVAL only; pale violet-white = the boundary; yellow = punch it
  / parryable (NIGHT47). UI text >= 16 px at 1080p. Fonts: the converted theme fonts.
- A new runtime shader goes in a Resources folder or Always Included, and its Player frames are checked for magenta (Column47's check;
  the G10 pink-door bug was a missing material).
