# SKYGRIND's game principles (GG 2026-10-11: "these game principles ... should be a core embedded into this project")

SKYGRIND is ULTRAKILL's movement and style, CoD Zombies' rounds and wall buys, a boss rush's bosses, and Terraria's progression and
readable items, at dusk. Every night's plan, card and review is checked against these rules. They come from GG's own words
(docs/memory/OWNER.md) and from what the games he names actually do (Research/2026-10-11-wave34/research/rea-ultrakill.md,
Research/2026-10-11-wave35/research/teach.md).

## 1. Never lose what the player had
- A verb, weapon ability or mechanic the player had stays unless GG says to remove it. "Replace" means the old thing still works or
  GG said yes. The list of every verb and how to perform it is docs/VERBS.md; every night ends with the verbs check in a Player
  (each verb performed, RED if any is gone) before the close table.
- A new row that changes an old verb proves the old verb still works in its harness check and in the Player.

## 2. Teach every verb where it is first needed
- Each verb is taught once, in the level it first matters, by short on-screen text with the player's own bound button (pad glyph or
  key), shown when the player enters the space to try it (ULTRAKILL's HudMessage: a trigger, the bound key, about 5 s, once).
- Then the room makes the player use it, safely, before it is tested under pressure. A boss is the level's final exam of its verb.

## 3. Read at a glance
- A pickup shows what it is: a weapon pickup shows the weapon's own model, turning, with its name on approach; health looks like
  health; an unlock shows the silhouette before it is earned (CoD Zombies' chalk outline).
- One colour per meaning: cyan player-usable, red-orange enemies and danger, magenta RIVAL, pale violet-white boundary (CLAUDE.md
  rule 8). Every state the player must react to has a cue they can see in one frame.

## 4. One new idea per level, six levels
- Six campaign levels, each built around one new verb or weapon and ending in a boss that tests it. Deeper verbs (the jump dash,
  the grapple) come later: the grapple at level 4 or 5, never earlier.
- Rooms are small and action-packed; every room has a movement line (wall-ride walls, nodes, lanes) and a climb route for enemies.

## 5. Every action answers on screen
- Firing, hitting, killing, riding a wall, zooming, swapping and bending each change the screen in a way a still frame shows.
  Feel is done when the screen measurably changes, judged on Player frames (L-001), never by "implemented and tested".

## 6. Movement first, and exact
- Movement verbs must feel crisp: no limp ropes, no floaty pulls; a pull takes you where you fired. Long cooldowns on crowd-control
  verbs (GG w19). The web-ported movement stays bit-exact with every new row off (CLAUDE.md rule 3).

## 7. Simple style, strong look
- Low detail, strong look: the PS2-era treatment (low internal resolution, dither, colour steps, vertex snap) must be visible in
  every frame; lights hang from real fixtures and light the room; the UI moves a little like an old TV. No clutter for its own sake.

## 8. Build, then iterate on the Player
- Each night ships fewer, finished things: built, seen in the Player, iterated at least once on what the frames show.

## 9. The checkable rules
- docs/DESIGN-RULES.md (GG 2026-10-16, G12) holds the numbered rules for movement (M), combat feedback (C), enemies (E), encounters (R),
  arenas (A), tutorials (T), progression (P), UI (U), art direction (V) and bosses (B), and the new-room checklist. Every card lists the
  rule ids it touches; every review checks them. docs/ROOMS.md is the per-room table (R-6).
