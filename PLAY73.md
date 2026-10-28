# PLAY73.md: what changed in the game you play (NIGHT73, 2026-10-27 evening)

Build: Builds/n73-play (made at the close from the last build with the repo's config). Everything NIGHT72 turned ON (PLAY72.md) stays ON.
Each line: what it is, where to see it, how to turn it off for an A / B. Tick keep or drop.

| change | where to see it | off switch | keep | drop |
|---|---|---|---|---|
| **kiln73**: THE KILN WYRM's arena is no longer orange wall to wall. The wyrm's two colour grades were pushing every surface into the danger hue (83 % of the screen's pixels); now the room is cool plum and violet (8 %) and the wyrm, the lava and its tells are the only red-orange things | level 3 (the kiln), the wyrm's fight | `-skygrindRows72 kiln73:0` | [ ] | [ ] |
| **goal72**: once a room is cleared, one line at the exit says what to do, verb first, in your cyan with an arrow ("GO to the exit"; stuck 10 s: "GO AHEAD to the exit" / "LOOK UP: the exit is there"; stuck 20 s: how, e.g. "DOUBLE JUMP: press SPACE twice"). It retires the moment you make progress | any campaign room after it is cleared; stand still to see the hints | `-skygrindNoGoal72` | [ ] | [ ] |
| **cam72**: the ULTRAKILL camera. The slide view sits higher (eye 1.28 m instead of 0.90 m) with less roll; strafes tilt the view slightly; a forward dash narrows the FOV a little (x0.95) instead of the wide speed punch (x1.16) | slide and dash anywhere | `-skygrindRows72 cam72:0` | [ ] | [ ] |
| **calm72**: the red screen border is a thin rim (about 58 px at the sides, 32 px top and bottom) instead of a thick speckled haze, at any health. Every cue is kept: the hit flash still shows on every hit, the damage edge and the EXPOSED (standing-still) edge still draw, only thinner, and EXPOSED pulses at half strength. Not changed: the speckled "TV static" after a hit (your 2026-10-13 ask) still reaches about 200 px in; see DECIDE73 | take a hit anywhere; stand still in a fight | `-skygrindNoCalm72` | [ ] | [ ] |
| **core73**: when THE KILN WYRM is open (staggered), its head stays the cyan "shoot here" block with a glow on its face; the big see-through blue box that used to sit around it is gone | level 3, the wyrm's stagger | `-skygrindRows72 core73:0` | [ ] | [ ] |
| **fix72**: less flicker. The film grain no longer fights the PS2 colour steps, the CRT grain no longer swings hard in the shadows, bloom filters thin bright strips. Measured with the camera still: the share of flickering pixels down 42-87 % on every level | everywhere; most in dark rooms | `-skygrindNoFix72` | [ ] | [ ] |

Not changed (built, measured, stayed OFF): finish55 (the boss kill's punch-in: three tries, a viewer could not see it, even with the boss filling the
frame), spring72 (enemy squash on hits: the game's existing hit pop already does most of it).
