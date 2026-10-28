# NIGHT42 report for GG: the feel night (your fifteen asks from n41-r7)

**Play build: `Builds/n42-r4/SKYGRIND.exe`** (frozen 23:53; smoke PASS). `-skygrindOff42` gives NIGHT41's game. The night lost an hour
to a dropped connection (19:15-20:23) and the Editor crashed on its own at 20:01 (a D3D12 device error); both are in NIGHT42-LOG.md.

## Your asks, one line each (ON = in the build; OFF = built, behind its row, with the reason)
| ask | what changed | how it was checked | state |
|---|---|---|---|
| G1 the wall-ride section on level 1 | THE GANTRY rebuilt in n42-foundry: the no-ride band gone, a 6 m lane, a third wall, the walls closer, a return stair at each lip | a person riding each wall crosses 40 % -> 100 % (harness); the gates still need the chain; walk 10/10; the after frame: the next walls in view | **ON** |
| G2 flung back near a wall | a wall jump only when you push into the wall (or ride it); 5 m/s back instead of 8; no wall jump just before a landing | a jump press beside a wall: 8.0 -> 0.0 m/s back (the frame: you stay at the wall); every level's gates GREEN; verbs 26/26 on level 1 | **ON** |
| G3 the gauntlet 0.7 s faster | the swing 2.5 -> 1.8 s, the damage kept (DECIDE42 D-2) | 300 -> 216 steps; THE ICEBREAKER still 46 s for the bot | **ON** |
| G4 the drone orb parry | 1.5x reach from the whole body, a 0.1 s mercy after the orb touches, a longer early press; the swing opens the buffer too | the swing's parries inside 2.5 m 11/16 -> 16/16 (harness) | OFF (never on a frame: the reel's orb never reached the reach; DECIDE42 D-10) |
| G5 shotgun boost | more lift, by pitch; a fall is cut before the lift | lift at 45 deg 7.1 -> 11 m/s; reaching 8 m 3.8 % -> 22 % of tries | OFF (level 3's first boost gate then passes by only 10 %: the kb1b roof; D-9) |
| G6 too much instruction | at most 2 lines in a level's first 60 s (teach42) | the reel's walk showed 2 lines in 60 s with or without it | OFF (no measured difference; D-11: tell us which lines) |
| G7 the meter | one language: the true charge (no eased under-read), FULL only when full, the gauntlet's swing cooldown shown | over 754 frames: wrong fill 44 -> 0 frames, false FULL 301 -> 0 | **ON** |
| G8 the reticle | the next shot waits for the cannon turn (0.46 s); the turns shortened (view row) | shots fired mid-turn 1 -> 0 (harness) | the wait **ON** (in charge42); the faster view OFF (not shown faster on the frame) |
| G9 hits and kills | heavier freezes per weapon, a bigger kill, the hit lean per weapon | THUNDERCLAP 58 -> 85 ms; frames looked at | **ON** |
| G10 hold fire after a charge | holding fire through the cooldown starts the next charge (no tap spam) | shots while held 7 -> 1; a second charged shot fires | **ON** |
| G11 a context skill | `night-context` (brief / close), installed in ~/.claude/skills | its close check found THE WICK dropped from HANDOFF and 15 missing items | DONE |
| G12 arena elements | THE GRINDER (enemies knocked in die: +80 GROUND), breakable cover (+60 SHATTER), aimed launch pads; placed in level 1's b1 | the harness and the reel GREEN | OFF (the grinder never seen on a frame; D-12) |
| G13 style meter + health bar | rank letters that punch in, shake, decay, a combo count, award lines; the health chunk that drains, a heal glow | 2 rank ups and the chunk / glow measured; frames looked at, one iteration | **ON** |
| G14 THE ICEBREAKER harder | two new attacks: SWEEP (0.9 s tell: jump it) and LEAP (1.1 s tell, a red landing disc: dash out); phase 2 double sweep; never more HP | bot 48.6 -> 55.9 s, the casual bot dies more (10 -> 14 per 10 fights) | **ON** |
| G15 the gauntlet hit | the strongest freeze (husk 143 ms, tank 183, boss 240), a cyan burst, a camera kick, THE ICEBREAKER chips and recoils | frames before / after; one iteration (the ring covered the screen) | **ON** |

## What to try first
Level 1's THE GANTRY; a jump press beside any wall; hold fire after a KESTREL charged shot; THE ICEBREAKER (level 2's boss) with the
gauntlet. Your calls are in DECIDE42.md (D-9..D-12 new tonight).
