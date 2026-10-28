# NIGHT39 report for GG: the feel night

**Play:** `Builds/n39-r4/SKYGRIND.exe` (the freeze build; see the bottom for what it was built from). `-skygrindOff39` gives last night's
game for a side-by-side. Your asks from after NIGHT38, one row each (your words are in Research/2026-10-14-wave39/GG-REQUEST.md):

| you asked | tonight | how it was checked |
|---|---|---|
| the pistol: holding should charge, not fire the fast shot first | **ON.** A quick click fires the tap when you let go (within 0.15 s); a hold fires nothing and charges. The shot still goes where you aimed at the press. | the moves check on level 1 in a Player (26/26), a timing check (a 60 ms click fires 58 ms after the press) |
| the pistol heavier, slower, a cannon special shot, the crosshair spin | **ON with the row above** (fire interval x1.2, a bigger kick, a hit freeze, the cannon blast and ring, the crosshair turns through the charge) | **not looked at on screen tonight** |
| the ghost weapon after the shotgun on level 3 | **fixed.** Level 3's room gave a weapon by its display name ("thunderclap") instead of its id: a slot that never fired. | the code path; the harness could not drive that room's clear |
| wall rides gain speed too quickly; jump a bit higher; the three-wall room too fast | **ON.** The chain never really had a speed cap: it climbed 10 -> 15 -> 19 -> 21 -> 22 -> 24 m/s over six walls. Now 9 -> 13 -> 16 and holds at 16. The jump-off rises a little higher (+0.2-0.4 m). | the harness wall by wall; THE GANTRY's walk 10/10; the moves check (rides) passes on levels 1, 4, 5 |
| a wall texture that says "you cannot ride this" | **built, off.** Dark plates with vertical fins (not cyan, not red-orange). Level 1 has no such walls. | not seen on screen |
| riding + aiming should slow the world dramatically (it felt removed) | **ON.** It was never removed: it was only 25 % slower. Now the world runs at 30 %, easing in over 0.15 s; the screen goes blue and dark at the edges. | a Player check (both ways), a frame |
| grappling onto an enemy slows the same way | **ON** (in the same row) | the harness only |
| the parry emphasised and slowed | **ON** (a slow window after the 0.25 s freeze; a wider, brighter streak) | the harness only |
| level 3 overhauled, the worm's AI the same, a new arena | **ON.** A new level 3 built around the shotgun: every gate a 12 m climb only the boost reaches (three shots + the double jump: hard on purpose; tell me if too hard), a room where you boost through the pack, a new worm arena of the same size (the worm's AI unchanged). The ghost weapon is gone. | the gate checker 4/4 (incl. the tricks that beat last night's version), the bot walks it 10/10, the moves check 29/30 in a Player |
| the worm needs more health; every boss 1-2 min | **ON.** Every boss now takes the bot 37-45 s (about 1-2 min for a person): THE WICK 44.7 s, THE STAG 37.9, THE KILN WYRM 43.0, THE LOOM 45.4, THE CROWN 36.5, level 6's Stag 44.4. Mostly more stagger and longer legs, health where it decides the legs. | the harness, 10 seeds each; THE CROWN's fight in a Player |

## What else changed
- **Level 4 (THE SPAN) is back on.** Its flight rooms now set your other guns aside (as THE SPIRE's do) so only the grapple crosses.
- **The charge bar under the crosshair is bigger and on.** The doors' curtains stay off: a locked door was never seen on screen tonight.
- **The boss entrance shake / flicker and the damage static stay off:** the damage static now gives the full burst only for real hits (a lava
  floor gave a constant static before), but neither was looked at on screen after the fix.

## Decisions for you
1. **Level 2's skate gap** (DECIDE39): with today's ice, no gap on flat ice needs skating: a dash jump crosses whatever a skate does. Keep it
   a flow beat (no gate), or change the ice?
2. **Level 6's gates:** every move is owned on the last level. Take the boost and grapple away in its gate rooms (as levels 4 and 5 do now)?
3. **The boss lengths** follow your new words (1-2 min); in w31 / w32 you had called the bosses too tanky. Play one and say if 40 s of bot
   time feels right.
4. Your own SLOW-MO / REDUCED MOTION settings: the slow-motion needs SLOW-MO on.

## Not done
See NIGHT39-LOG.md's close table: the freeze test suite (stopped for low memory), ride38's old deaths in level 1's a5, the no-ride texture on screen, the doors and
the boss juice on screen, the pistol's look on screen, level 5's grapple moves (no grapple node on that level for the check).
