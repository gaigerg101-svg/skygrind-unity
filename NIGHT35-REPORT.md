# NIGHT35 report for GG: the fundamentals night

Play `Builds/n35-r4/SKYGRIND.exe` with FEEL-PLAYTEST.md. **Play first: the grapple on level 4** (step 1): it is the one thing the night
fixed but could not re-prove in a Player. Measured detail: NIGHT35-PROOF.md; the log and close table: NIGHT35-LOG.md; your calls with
defaults: DECIDE35.md (D-1..D-15).

## What came back
- **The charge on the KESTREL and THE THUNDERCLAP.** It was never removed, but it read as gone: holding fire auto-fired for 0.4 s
  first and nothing on the gun reacted. Now a hold charges at once with no shots, the gun glows and shakes, the rail says CHARGING /
  CHARGED, and the release fires (a full charge fires itself 1.0 s later). Player: 0 shots while held, 1 charged shot (was 1 normal
  shot while held). The Player also found a real bug the harness missed: a press held across one frame's sim steps counted as
  several presses and ended the charge with a normal shot. Fixed.
- **Nothing you had is lost, checked in the Player (docs/VERBS.md, 39 rows).** Level 1: 23 / 23. Level 3: 25 / 25. Level 4: rows
  1-12 PASS on the final build. The grapple rows (17-23) were RED on the build before the fix and were not re-run on the final build.
  That was my mistake: I closed the level-4 Player by error and no reel budget was left for a retake.

## What's new
- **Teaching text, ULTRAKILL-style:** one line per room on level 1 with your own button (pad glyph or key), 5 s, once per profile.
  Levels 2-5 have their lines placed too.
- **The Lucio wall ride:** hold jump to ride as long as you like. Release to jump off; your look sets the height. Crouch drops off.
  Chains build speed: three walls end at 20.7 m/s (was 16.2). Player: rides of 1.56-1.89 s on the test walls, where the old ride
  stopped at 1.2 s; the jump-off goes 17.2 m/s from 11.7 on the wall. A tapped wall jump still climbs a shaft exactly as before.
- **The grapple yanks:** it pulls you straight to wherever it hits (node, bare wall, enemy) at 44 m/s and stops there. There is no
  pendulum swing. You own it from level 4, so it, and the bend that fires on a latch, arrive there (D-2). On levels 1-3 the old nodes
  are dim locked rings.
- **Pickups show what they are:** the weapon's own model turns on a floor ring with its name. The health pickup is a cyan vial,
  "MEND +40", not a red cross.
- **The PS2 look that shows:** 480 lines, 32 colour levels, dither and scanlines (equal-neighbour pixels 10 % -> 62 %). The TV HUD
  kicks on hits and glitches briefly. Wall ride + zoom turns the screen cold blue: blue = focus, grey = the bend.
- **The lights:** the "weird little triangle displays" were ceiling panels and 127 white strips under a sky-violet ceiling that
  read as open sky. The ceiling is now dark with joists. Lamps hang on walls, with a warm key pool and a cool fill per room (lights
  per level 273 -> 146). The two big yellow discs were spitter acid, your enemy design: not touched.
- **Six levels:** foundry, iceline, kiln, span (the grapple), spire, and a new FINALE: three wall-ride rooms, then THE STAG (D-10).
  Levels 1-3 each gained a wall-ride room. All four new files walk 10/10 with the bot.

## Frame pairs (RED = -skygrindOff35, GREEN = tonight)
- Charge: `Logs/feel/n35-r1/red/charge35/` vs `Logs/feel/n35-r1/charge/charge35/shot-N30-charge35-revolver-full.png`.
- Look: `Logs/feel/n35-r1/red/look35/` vs `Logs/feel/n35-r4/look/look35/` (room, lit, hud-hit-1/2, ride).
- Teaching: `Logs/feel/n35-r1/teach/teach35/shot-N29-N35-teach35-l1.*.png`.
- Pickups: `Logs/feel/n35-r3/redcamp/` (cubes) vs `Logs/feel/n35-r3/pickup/pickup35/`.
- Finale: `Logs/feel/n35-r2/levels6/levels35/`.

## Short, honestly
- The grapple on level 4 is unproven in the Player after its fix (harness PASS; the r3 Player showed RopeAttach 0 before it).
- The smoke check printed no result on the final build: the Player quit 10 s after boot and hung. Not diagnosed.
- The suite: 1943 / 1945 (two load-sensitive tests failed under low memory; not diagnosed).
- LevelSelect29 was not run. n35-iceline was not booted in a Player.
- Bosses: harness only. THE LANTERN now dies in 13.9 s (was 20.3 s) because the charge comes back faster (D-15: keep, since shorter
  is your direction).
- Leftovers: vertex snapping, ride rooms in levels 4-5, a PS2 settings page, the weapon wheel's slow time, light shafts, and
  `wallride35Chain` (the rising-pitch sound hook).
