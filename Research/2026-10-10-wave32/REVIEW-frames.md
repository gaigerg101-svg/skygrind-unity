# The lead's review of the NIGHT31 Player frames (2026-10-10, ~21:05)

Source: Player build n31-r5 (the freeze) reels `Logs/feel/n31-r5/bosses/{lantern31,stag31,wyrm31}` and n31-r3 `Logs/feel/n31-r3/span/{arenas31,grapple31}`, 1920x1080 frames, looked at by eye. The reels run with the player at 1,000,000 health, so the deaths and damage taken are not GG's.

## What the frames show

- **Boss 1 arena (THE LANTERN) is a polygon, not a circle.** The wall reads as about 14-16 flat panels with a visible crease at every joint, at eye height and from the middle (f0400, f1200). GG is right.
- **Bosses read as dark boxes.** THE STAG is a black block with two glowing white antler planes (f0800); the KILN WYRM fills the screen as a purple slab with no visible head (f0200, f1200); THE LOOM is two dark pillars with two cyan cubes (span arenas31 f0100). Nothing says "shoot here" except the Lantern's core cube.
- **The boss arenas are empty floors.** The Stag's rink is a flat blue grid to the horizon; the Wyrm's is pink boxes and a purple void; the Loom's is a square deck over lava with two posts. No cover, no height, no route choice. This is the "arenas are boring" GG names.
- **HUD errors during boss fights:** "ROUND 1 IN 83" countdown shows during every boss fight; the LT slot label is cut off ("CHARGIN"); the lesson "RIDE THE WALL. SHOOT THE CORE." (the Lantern's) is still on screen in the Stag and Wyrm reels (may be the reel's room jump; not checked in a normal campaign run); the style meter sits at "D DULL" through whole fights.
- **Level 4 (the span) reads as corridors.** "4.3 THE GAP" is a long narrow hall with rings in a line; "4.8 THE LOOM" is a square box.

## Numbers from the same reels (n31-r5 summary.txt)

| boss | Player pilot result | note |
|---|---|---|
| THE LANTERN | killed 25.1 s | 3 shield breaks; only charged shots from a wall panel hurt the core |
| THE STAG | killed 115.4 s | 7 back punches; damage only during the 12 wall-slam stuns |
| KILN WYRM | not killed in 240 s, 31 % left | 13 full hits vs 384 chip hits: guns barely hurt it |
| THE LOOM | not run as a kill | deck camper dies at 18.3 s |

## Reading

The bosses that GG calls "too much health" and "barely hit" are the Stag and the Wyrm: both gate damage behind a short window (the Stag's back after a wall slam; the Wyrm's head after a punch) and chip the rest at a fraction, so a player spends most of a fight unable to hurt them. The Lantern is short but the only damage path is a charged shot from a wall panel. The fix direction is the same for all four: keep the identity mechanic as a big bonus (a stagger, a crit window), not as the only way to deal damage.
